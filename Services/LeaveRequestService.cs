using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using NVOAMASIS.Hubs;

namespace NVOAMASIS.Services
{
    public class LeaveRequestService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly AccountService _accountService;
        private readonly GlobalServices _globalServices;

        public LeaveRequestService(
            AppDbContext context,
            IHubContext<NotificationHub> hubContext,
            AccountService accountService,
            GlobalServices globalServices)
        {
            _context = context;
            _hubContext = hubContext;
            _accountService = accountService;
            _globalServices = globalServices;
        }

        // Submit leave request
        public async Task<BoolandMessReponse> SubmitLeaveRequestAsync(LeaveRequest request)
        {
            try
            {
                var auth = _accountService.GetAuth().Result;
                var userName = auth.User.Identity!.Name!;
                var userId = await _globalServices.GetIdfromUser(userName);
                if (userId == null) return new BoolandMessReponse(false, "User not found");

                request.Id = Guid.NewGuid();
                request.EmployeeId = userId.Value;
                request.Status = 1; // Pending
                request.CreatedDate = DateTime.Now;

                // Calculate total days/hours
                if (request.DurationType == 1) // Full day
                {
                    request.TotalDays = (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;
                    request.TotalHours = null;
                }
                else if (request.DurationType == 2) // Half day
                {
                    request.TotalDays = 0.5m;
                    request.TotalHours = null;
                }
                else if (request.DurationType == 3 && request.StartTime.HasValue && request.EndTime.HasValue) // Hours
                {
                    var duration = request.EndTime.Value - request.StartTime.Value;
                    request.TotalHours = (decimal)duration.TotalHours;
                    request.TotalDays = (decimal)duration.TotalDays;
                }

                _context.LeaveRequests.Add(request);
                await _context.SaveChangesAsync();

                // Notify all admins
                var adminUsers = await _context.UserList
                    .Where(u => u.Department == "ADMIN")
                    .Select(u => u.UsrId)
                    .ToListAsync();

                string leaveTypeStr = GetLeaveTypeName(request.LeaveType);
                string msg = $"Yêu cầu nghỉ phép ({leaveTypeStr}): {request.Reason} - {request.StartDate:dd/MM/yyyy}";
                if (request.EndDate.Date != request.StartDate.Date)
                    msg += $" đến {request.EndDate:dd/MM/yyyy}";

                foreach (var adminId in adminUsers)
                {
                    var noti = new Notification
                    {
                        SenderUserId = userId,
                        ReceiverUserId = adminId,
                        Message = msg,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(noti);
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.User(adminId.ToString())
                        .SendAsync("ReceiveNotification", msg);

                    int count = await _context.Notifications
                        .Where(n => n.ReceiverUserId == adminId && !n.IsRead)
                        .CountAsync();
                    await _hubContext.Clients.User(adminId.ToString())
                        .SendAsync("UpdateUnreadEmailCount", count);
                }

                return new BoolandMessReponse(true, "Gửi yêu cầu nghỉ phép thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        // Get pending requests (for admin)
        public async Task<List<LeaveRequest>> GetPendingRequestsAsync()
        {
            return await _context.LeaveRequests
                .Where(x => x.Status == 1)
                .OrderByDescending(x => x.CreatedDate)
                .Take(100)
                .ToListAsync();
        }

        // Get all requests (for admin)
        public async Task<List<LeaveRequest>> GetAllRequestsAsync()
        {
            return await _context.LeaveRequests
                .OrderByDescending(x => x.CreatedDate)
                .Take(100)
                .ToListAsync();
        }

        // Get user's own requests
        public async Task<List<LeaveRequest>> GetMyRequestsAsync(Guid userId)
        {
            return await _context.LeaveRequests
                .Where(x => x.EmployeeId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        // Approve/Reject request
        public async Task<BoolandMessReponse> ReviewLeaveRequestAsync(Guid requestId, bool approve, string? comment)
        {
            try
            {
                var auth = _accountService.GetAuth().Result;
                var reviewerName = auth.User.Identity!.Name!;
                var reviewerId = await _globalServices.GetIdfromUser(reviewerName);
                if (reviewerId == null) return new BoolandMessReponse(false, "Reviewer not found");

                var request = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == requestId);
                if (request == null) return new BoolandMessReponse(false, "Request not found");
                if (request.Status != 1) return new BoolandMessReponse(false, "Already reviewed");

                request.Status = approve ? 2 : 3; // 2=Approved, 3=Rejected
                request.ApproverId = reviewerId;
                request.ApprovedDate = DateTime.Now;
                request.ApproverComment = comment;
                request.UpdatedDate = DateTime.Now;

                _context.Update(request);
                await _context.SaveChangesAsync();

                // Notify employee
                string leaveTypeStr = GetLeaveTypeName(request.LeaveType);
                string statusStr = approve ? "ĐÃ ĐƯỢC DUYỆT" : "BỊ TỪ CHỐI";
                string msg = $"Yêu cầu nghỉ phép ({leaveTypeStr}) {statusStr} - {request.StartDate:dd/MM/yyyy}";
                if (!string.IsNullOrWhiteSpace(comment))
                    msg += $". Ghi chú: {comment}";

                var noti = new Notification
                {
                    SenderUserId = reviewerId,
                    ReceiverUserId = request.EmployeeId,
                    Message = msg,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(noti);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(request.EmployeeId.ToString())
                    .SendAsync("ReceiveNotification", msg);

                int count = await _context.Notifications
                    .Where(n => n.ReceiverUserId == request.EmployeeId && !n.IsRead)
                    .CountAsync();
                await _hubContext.Clients.User(request.EmployeeId.ToString())
                    .SendAsync("UpdateUnreadEmailCount", count);

                return new BoolandMessReponse(true, approve ? "Đã duyệt" : "Đã từ chối");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        // Get employee name by ID
        public async Task<string> GetEmployeeNameAsync(Guid employeeId)
        {
            var user = await _context.UserList.FirstOrDefaultAsync(u => u.UsrId == employeeId);
            return user?.Name ?? user?.Usr ?? user?.NickName ?? employeeId.ToString();
        }

        // Update leave request (only if status = 1 Pending)
        public async Task<BoolandMessReponse> UpdateLeaveRequestAsync(Guid requestId, LeaveRequest updatedData)
        {
            try
            {
                var auth = _accountService.GetAuth().Result;
                var userName = auth.User.Identity!.Name!;
                var userId = await _globalServices.GetIdfromUser(userName);
                if (userId == null) return new BoolandMessReponse(false, "User not found");

                var request = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == requestId);
                if (request == null) return new BoolandMessReponse(false, "Request not found");
                
                // Only allow update if pending and user is owner
                if (request.Status != 1) return new BoolandMessReponse(false, "Không thể sửa đơn đã được duyệt/từ chối");
                if (request.EmployeeId != userId) return new BoolandMessReponse(false, "Bạn không có quyền sửa đơn này");

                // Update fields
                request.LeaveType = updatedData.LeaveType;
                request.DurationType = updatedData.DurationType;
                request.StartDate = updatedData.StartDate;
                request.EndDate = updatedData.EndDate;
                request.StartTime = updatedData.StartTime;
                request.EndTime = updatedData.EndTime;
                request.Reason = updatedData.Reason;
                request.UpdatedDate = DateTime.Now;

                // Recalculate total days/hours
                if (request.DurationType == 1) // Full day
                {
                    request.TotalDays = (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;
                    request.TotalHours = null;
                }
                else if (request.DurationType == 2) // Half day
                {
                    request.TotalDays = 0.5m;
                    request.TotalHours = null;
                }
                else if (request.DurationType == 3 && request.StartTime.HasValue && request.EndTime.HasValue) // Hours
                {
                    var duration = request.EndTime.Value - request.StartTime.Value;
                    request.TotalHours = (decimal)duration.TotalHours;
                    request.TotalDays = (decimal)duration.TotalDays;
                }

                _context.Update(request);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Cập nhật yêu cầu thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        // Delete leave request (only if status = 1 Pending)
        public async Task<BoolandMessReponse> DeleteLeaveRequestAsync(Guid requestId)
        {
            try
            {
                var auth = _accountService.GetAuth().Result;
                var userName = auth.User.Identity!.Name!;
                var userId = await _globalServices.GetIdfromUser(userName);
                if (userId == null) return new BoolandMessReponse(false, "User not found");

                var request = await _context.LeaveRequests.FirstOrDefaultAsync(x => x.Id == requestId);
                if (request == null) return new BoolandMessReponse(false, "Request not found");
                
                // Only allow delete if pending and user is owner
                if (request.Status != 1) return new BoolandMessReponse(false, "Không thể xóa đơn đã được duyệt/từ chối");
                if (request.EmployeeId != userId) return new BoolandMessReponse(false, "Bạn không có quyền xóa đơn này");

                _context.LeaveRequests.Remove(request);
                await _context.SaveChangesAsync();

                return new BoolandMessReponse(true, "Đã xóa yêu cầu");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Lỗi: " + ex.Message);
            }
        }

        private string GetLeaveTypeName(int leaveType)
        {
            return leaveType switch
            {
                1 => "Nghỉ phép năm",
                2 => "Nghỉ ốm",
                3 => "Nghỉ việc riêng",
                4 => "Nghỉ không lương",
                5 => "Xin ra ngoài",
                _ => "Khác"
            };
        }

        public string GetDurationTypeName(int durationType)
        {
            return durationType switch
            {
                1 => "Cả ngày",
                2 => "Nửa ngày",
                3 => "Theo giờ",
                _ => ""
            };
        }

        public string GetStatusName(int status)
        {
            return status switch
            {
                1 => "Chờ duyệt",
                2 => "Đã duyệt",
                3 => "Từ chối",
                _ => "Không xác định"
            };
        }
    }
}
