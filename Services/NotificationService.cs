namespace NVOAMASIS.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using NVOAMASIS.Data;
    using NVOAMASIS.Hubs;
    using NVOAMASIS.Models;
    using NVOAMASIS.Response;

    /// <summary>
    /// Không inject MudBlazor ISnackbar — service còn được gọi từ API/JWT
    /// (NavigationManager chưa init → crash RemoteNavigationManager).
    /// </summary>
    public class NotificationService(
        IHubContext<NotificationHub> _hubContext,
        AppDbContext _context,
        AccountService asv,
        GlobalServices gsv,
        IHttpContextAccessor httpContextAccessor,
        ILogger<NotificationService> logger)
    {
        public async Task SendNotificationAsync(string[] recipients, string noidung)
        {
            try
            {
                await SendNotificationCoreAsync(recipients, noidung);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SendNotificationAsync failed");
            }
        }

        /// <summary>Gửi notification batch (duyệt phiếu / API).</summary>
        public async Task SendNotificationQuietAsync(string[] recipients, string noidung)
        {
            await SendNotificationCoreAsync(recipients, noidung);
        }

        private async Task SendNotificationCoreAsync(string[] recipients, string noidung)
        {
            Guid? userid_login = await ResolveCurrentUserIdAsync();

            foreach (var user_receiverid in recipients)
            {
                if (!Guid.TryParse(user_receiverid, out var receiverGuid))
                    continue;

                var notify = new Notification
                {
                    SenderUserId = userid_login,
                    ReceiverUserId = receiverGuid,
                    Message = noidung,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notify);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.User(user_receiverid.ToString())
                    .SendAsync("ReceiveNotification", noidung);

                int updatedCount = await _context.Notifications
                   .Where(n => n.ReceiverUserId == receiverGuid && !n.IsRead)
                   .CountAsync();
                await _hubContext.Clients.User(user_receiverid.ToString()).SendAsync("UpdateUnreadEmailCount", updatedCount);
            }
        }

        private async Task<Guid?> ResolveCurrentUserIdAsync()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal != null)
            {
                var jwtId = MobileJwtTokenService.GetUserId(principal);
                if (jwtId != null && jwtId != Guid.Empty)
                    return jwtId;
            }

            var name = principal?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                try { name = asv.GetAuth().Result.User.Identity?.Name; }
                catch { /* API / non-Blazor */ }
            }

            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await gsv.GetIdfromUser(name);
        }

        public async Task<int> GetUnreadEmailCountAsync(Guid? userId)
        {
            return await _context.Notifications
                .Where(e => e.ReceiverUserId == userId && !e.IsRead)
                .CountAsync();
        }

        AuthUser user = new AuthUser();
        public async Task<List<Notification>> GetList_noti()
        {
            try
            {
                _context.ChangeTracker.Clear();
                user = asv.GetUserDetail();
                var rs = _context.Notifications.Where(x => x.ReceiverUserId == user.UsrId && x.IsRead == false)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetList_notisent_all()
        {
            try
            {
                _context.ChangeTracker.Clear();
                user = asv.GetUserDetail();
                var rs = _context.Notifications.Where(x => x.SenderUserId == user.UsrId).OrderByDescending(x => x.CreatedAt).Take(50).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Notification>();
            }
        }

        public async Task<List<Notification>> GetList_noti_nhan_all()
        {
            try
            {
                _context.ChangeTracker.Clear();
                user = asv.GetUserDetail();
                var rs = _context.Notifications.Where(x => x.ReceiverUserId == user.UsrId).OrderByDescending(x => x.CreatedAt).Take(50).ToList();

                return rs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Notification>();
            }
        }

        public async Task<BoolandMessReponse> Deletenoti(Notification c)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (c?.Id == null || c?.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Nothing to Delete");

                _context?.Notifications.Remove(c!);
                await _context?.SaveChangesAsync()!;
                return new BoolandMessReponse(true, "Delete successful");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Cannot Delete Message with error code: " + ex.Message);
            }
        }

        public async Task<List<string>> UpdateSentNoti(Notification IV)
        {
            try
            {
                _context.ChangeTracker.Clear();

                _context.Update(IV);
                await _context.SaveChangesAsync();

                user = asv.GetUserDetail();

                int updatedCount = await _context.Notifications
                       .Where(n => n.ReceiverUserId == user.UsrId && !n.IsRead)
                       .CountAsync();
                await _hubContext.Clients.User(user.UsrId.ToString()).SendAsync("UpdateUnreadEmailCount", updatedCount);

                return ["Seen", "1"];
            }
            catch
            {
                return ["Fail", "0"];
            }
        }

        public async Task<BoolandMessReponse> SendOutRequestAsync(DateTime fromTime, DateTime toTime, string reason)
        {
            try
            {
                var auth = asv.GetAuth().Result;
                var userName = auth.User.Identity!.Name!;
                var userId = await gsv.GetIdfromUser(userName);
                if (userId == null) return new BoolandMessReponse(false, "User not found");

                if (toTime <= fromTime)
                    return new BoolandMessReponse(false, "'To' time must be greater than 'From' time");

                var req = new OutRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = userId.Value,
                    FromTime = fromTime,
                    ToTime = toTime,
                    Reason = reason,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                _context.OutRequests.Add(req);
                await _context.SaveChangesAsync();

                var adminUsers = _context.UserList
                    .Where(u => EF.Property<string>(u, "Department") == "ADMIN" || EF.Property<string>(u, "Department") == "admin")
                    .Select(u => u.UsrId)
                    .ToList();

                string msg = $"Yêu cầu ra ngoài: {reason} ({fromTime:HH:mm} - {toTime:HH:mm})";
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
                    await _hubContext.Clients.User(adminId.ToString()).SendAsync("ReceiveNotification", msg);

                    int updatedCount = await _context.Notifications
                        .Where(n => n.ReceiverUserId == adminId && !n.IsRead)
                        .CountAsync();
                    await _hubContext.Clients.User(adminId.ToString()).SendAsync("UpdateUnreadEmailCount", updatedCount);
                }

                return new BoolandMessReponse(true, "Sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SendOutRequestAsync failed");
                return new BoolandMessReponse(false, ex.Message);
            }
        }

        public async Task<BoolandMessReponse> ReviewOutRequestAsync(Guid requestId, bool approve, string? note)
        {
            try
            {
                var auth = asv.GetAuth().Result;
                var reviewerName = auth.User.Identity!.Name!;
                var reviewerId = await gsv.GetIdfromUser(reviewerName);
                if (reviewerId == null) return new BoolandMessReponse(false, "Reviewer not found");

                var req = await _context.OutRequests.FirstOrDefaultAsync(x => x.Id == requestId);
                if (req == null) return new BoolandMessReponse(false, "Request not found");
                if (req.Status != "Pending") return new BoolandMessReponse(false, "Already reviewed");

                req.Status = approve ? "Approved" : "Rejected";
                req.ReviewedAt = DateTime.UtcNow;
                req.ReviewerUserId = reviewerId;
                req.ReviewNote = note;
                _context.Update(req);
                await _context.SaveChangesAsync();

                string msg = approve ? $"Yêu cầu ra ngoài đã được DUYỆT ({req.FromTime:HH:mm}-{req.ToTime:HH:mm})" : $"Yêu cầu ra ngoài bị TỪ CHỐI ({req.FromTime:HH:mm}-{req.ToTime:HH:mm})";
                if (!string.IsNullOrWhiteSpace(note)) msg += $". Ghi chú: {note}";

                var noti = new Notification
                {
                    SenderUserId = reviewerId,
                    ReceiverUserId = req.UserId,
                    Message = msg,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(noti);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.User(req.UserId.ToString()).SendAsync("ReceiveNotification", msg);
                int updatedCount = await _context.Notifications
                    .Where(n => n.ReceiverUserId == req.UserId && !n.IsRead)
                    .CountAsync();
                await _hubContext.Clients.User(req.UserId.ToString()).SendAsync("UpdateUnreadEmailCount", updatedCount);

                return new BoolandMessReponse(true, "Reviewed");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, ex.Message);
            }
        }
    }
}
