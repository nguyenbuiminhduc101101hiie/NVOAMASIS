using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Hubs;
using NVOAMASIS.Models;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Nghỉ phép giai đoạn 2: tính ngày công theo lịch làm việc + ngày lễ, kiểm tra quỹ phép năm,
    /// duyệt theo quản lý trực tiếp (HrEmployee.ManagerEmployeeId), quỹ phép năm (HrLeaveBalance).
    /// Dùng chung bảng LeaveRequests với module cũ (EmployeeId = UserList.UsrId).
    /// </summary>
    public sealed class HrLeaveService(
        IDbContextFactory<AppDbContext> dbFactory,
        IHubContext<NotificationHub> hub,
        ILogger<HrLeaveService> logger)
    {
        /// <summary>Mã quyền cũ của module nghỉ phép: See = xem tất cả đơn, Approve = duyệt/hủy mọi đơn.</summary>
        public const string LeaveMenu = "LeaveRequests";

        private static readonly TimeSpan MorningStart = new(8, 0, 0);
        private static readonly TimeSpan MorningEnd = new(12, 0, 0);
        private static readonly TimeSpan AfternoonStart = new(13, 0, 0);
        private static readonly TimeSpan AfternoonEnd = new(17, 0, 0);
        private const int MaxRangeDays = 90;

        // ═════════════════════════ Tính số ngày nghỉ ═════════════════════════

        public async Task<(HrLeaveQuantity? Qty, string? ErrorKey)> CalculateAsync(HrLeaveInput input)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            return await CalculateAsync(db, s, input);
        }

        private static async Task<(HrLeaveQuantity? Qty, string? ErrorKey)> CalculateAsync(
            AppDbContext db, HrWorkSettings s, HrLeaveInput input)
        {
            var start = input.StartDate.Date;
            switch (input.DurationType)
            {
                case HrLeaveDuration.FullDay:
                {
                    var end = input.EndDate.Date;
                    if (end < start) return (null, "hr_err_end_before_start");
                    if ((end - start).TotalDays > MaxRangeDays) return (null, "hr_err_leave_too_long");
                    var cal = await HrSettingsService.BuildCalendarAsync(db, s, start, end);
                    var days = cal.Sum(d => d.Standard);
                    if (days <= 0) return (null, "hr_err_leave_no_workday");
                    return (new HrLeaveQuantity(days, null), null);
                }
                case HrLeaveDuration.HalfDay:
                {
                    var day = (await HrSettingsService.BuildCalendarAsync(db, s, start, start))[0];
                    if (day.Kind is HrDayKind.Off or HrDayKind.Holiday) return (null, "hr_err_leave_no_workday");
                    if (day.Kind == HrDayKind.HalfWork && !input.HalfMorning) return (null, "hr_err_leave_saturday_afternoon");
                    return (new HrLeaveQuantity(0.5m, null), null);
                }
                case HrLeaveDuration.Hours:
                {
                    if (input.StartTime is null || input.EndTime is null || input.EndTime <= input.StartTime)
                        return (null, "hr_err_leave_time_range");
                    var day = (await HrSettingsService.BuildCalendarAsync(db, s, start, start))[0];
                    if (day.Kind is HrDayKind.Off or HrDayKind.Holiday) return (null, "hr_err_leave_no_workday");
                    var span = input.EndTime.Value - input.StartTime.Value;
                    // Trừ giờ nghỉ trưa 12:00–13:00 nếu khoảng xin nghỉ đi qua.
                    var lunchStart = input.StartTime.Value > MorningEnd ? input.StartTime.Value : MorningEnd;
                    var lunchEnd = input.EndTime.Value < AfternoonStart ? input.EndTime.Value : AfternoonStart;
                    if (lunchEnd > lunchStart) span -= lunchEnd - lunchStart;
                    var hours = Math.Round((decimal)span.TotalHours, 2);
                    if (hours <= 0) return (null, "hr_err_leave_time_range");
                    var days = Math.Round(hours / s.HoursPerDay, 2);
                    return (new HrLeaveQuantity(days, hours), null);
                }
                default:
                    return (null, "hr_err_leave_duration");
            }
        }

        // ═════════════════════════ Gửi / sửa / xóa đơn ═════════════════════════

        public async Task<HrResult> SubmitAsync(HrLeaveInput input, Guid userId, string actor)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var s = await HrSettingsService.GetAsync(db);

                var check = await ValidateAsync(db, s, input, userId, null);
                if (check.Error != null) return HrResult.Fail(check.Error, check.Detail);
                var qty = check.Qty!.Value;

                var approver = await ResolveApproverAsync(db, userId);
                var req = new LeaveRequest
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = userId,
                    Status = HrLeaveStatus.Pending,
                    CreatedDate = DateTime.Now,
                    AssignedApproverId = approver
                };
                Apply(req, input, qty);
                db.LeaveRequests.Add(req);
                await db.SaveChangesAsync();

                var name = await UserDisplayAsync(db, userId);
                var msg = $"{name} gửi đơn {HrLeaveType.Vi(req.LeaveType).ToLowerInvariant()} {Describe(req)}: {req.Reason}";
                var receivers = approver.HasValue
                    ? new List<Guid> { approver.Value }
                    : await FallbackApproversAsync(db);
                receivers.Remove(userId);
                await NotifyAsync(db, userId, receivers, msg);

                return HrResult.Ok(approver.HasValue ? "hr_leave_submitted_manager" : "hr_leave_submitted", req.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Submit leave failed");
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> UpdateAsync(HrLeaveInput input, Guid userId)
        {
            if (input.Id is null) return HrResult.Fail("hr_err_not_found");
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var req = await db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == input.Id.Value);
                if (req is null) return HrResult.Fail("hr_err_not_found");
                if (req.EmployeeId != userId) return HrResult.Fail("hr_err_leave_not_owner");
                if (req.Status != HrLeaveStatus.Pending) return HrResult.Fail("hr_err_leave_not_pending");

                var s = await HrSettingsService.GetAsync(db);
                var check = await ValidateAsync(db, s, input, userId, req.Id);
                if (check.Error != null) return HrResult.Fail(check.Error, check.Detail);

                Apply(req, input, check.Qty!.Value);
                req.AssignedApproverId = await ResolveApproverAsync(db, userId);
                req.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", req.Id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteAsync(Guid id, Guid userId)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var req = await db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id);
                if (req is null) return HrResult.Fail("hr_err_not_found");
                if (req.EmployeeId != userId) return HrResult.Fail("hr_err_leave_not_owner");
                if (req.Status != HrLeaveStatus.Pending) return HrResult.Fail("hr_err_leave_not_pending");
                db.LeaveRequests.Remove(req);
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        private static void Apply(LeaveRequest req, HrLeaveInput input, HrLeaveQuantity qty)
        {
            req.LeaveType = input.LeaveType;
            req.DurationType = input.DurationType;
            req.StartDate = input.StartDate.Date;
            req.Reason = (input.Reason ?? "").Trim();
            req.TotalDays = qty.Days;
            req.TotalHours = qty.Hours;
            switch (input.DurationType)
            {
                case HrLeaveDuration.FullDay:
                    req.EndDate = input.EndDate.Date;
                    req.StartTime = null;
                    req.EndTime = null;
                    break;
                case HrLeaveDuration.HalfDay:
                    // Lưu buổi nghỉ vào StartTime/EndTime để bảng công biết sáng hay chiều.
                    req.EndDate = input.StartDate.Date;
                    req.StartTime = input.HalfMorning ? MorningStart : AfternoonStart;
                    req.EndTime = input.HalfMorning ? MorningEnd : AfternoonEnd;
                    break;
                default:
                    req.EndDate = input.StartDate.Date;
                    req.StartTime = input.StartTime;
                    req.EndTime = input.EndTime;
                    break;
            }
        }

        private async Task<(HrLeaveQuantity? Qty, string? Error, string? Detail)> ValidateAsync(
            AppDbContext db, HrWorkSettings s, HrLeaveInput input, Guid userId, Guid? excludeId)
        {
            if (!HrLeaveType.All.Contains(input.LeaveType)) return (null, "hr_err_leave_type", null);
            if (string.IsNullOrWhiteSpace(input.Reason)) return (null, "hr_err_leave_reason", null);
            if (input.LeaveType == HrLeaveType.GoOut && input.DurationType == HrLeaveDuration.FullDay)
                return (null, "hr_err_leave_goout_fullday", null);

            var (qty, err) = await CalculateAsync(db, s, input);
            if (err != null) return (null, err, null);

            // Trùng với đơn khác (chờ duyệt / đã duyệt), bỏ qua "xin ra ngoài".
            if (input.LeaveType != HrLeaveType.GoOut)
            {
                var start = input.StartDate.Date;
                var end = input.DurationType == HrLeaveDuration.FullDay ? input.EndDate.Date : start;
                var others = await db.LeaveRequests.AsNoTracking()
                    .Where(x => x.EmployeeId == userId && x.Id != excludeId
                                && (x.Status == HrLeaveStatus.Pending || x.Status == HrLeaveStatus.Approved)
                                && x.LeaveType != HrLeaveType.GoOut
                                && x.StartDate <= end && x.EndDate >= start)
                    .Select(x => new { x.DurationType, x.StartTime, x.StartDate })
                    .ToListAsync();
                foreach (var o in others)
                {
                    var conflict = input.DurationType == HrLeaveDuration.FullDay || o.DurationType == HrLeaveDuration.FullDay;
                    if (!conflict && input.DurationType == HrLeaveDuration.HalfDay && o.DurationType == HrLeaveDuration.HalfDay)
                    {
                        var otherMorning = o.StartTime is null || o.StartTime.Value < MorningEnd;
                        conflict = otherMorning == input.HalfMorning || o.StartTime is null;
                    }
                    if (conflict) return (null, "hr_err_leave_overlap", o.StartDate.ToString("dd/MM/yyyy"));
                }
            }

            // Kiểm tra quỹ phép năm
            if (input.LeaveType == HrLeaveType.Annual && !s.AllowNegativeAnnualLeave)
            {
                var emp = await db.HrEmployees.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
                if (emp != null)
                {
                    var year = input.StartDate.Year;
                    var bal = await GetOrBuildBalanceAsync(db, s, emp, year, persist: true, actor: "system");
                    var usage = await UsageAsync(db, new[] { userId }, year, excludeId);
                    usage.TryGetValue(userId, out var u);
                    var available = bal.Total - u.Used - u.Pending;
                    if (qty!.Value.Days > available)
                        return (null, "hr_err_leave_insufficient", available.ToString("0.##"));
                }
            }

            return (qty, null, null);
        }

        /// <summary>Quản lý trực tiếp (còn làm việc, có tài khoản) của user; null nếu không có.</summary>
        private static async Task<Guid?> ResolveApproverAsync(AppDbContext db, Guid userId)
        {
            var managerId = await db.HrEmployees.AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.ManagerEmployeeId)
                .FirstOrDefaultAsync();
            if (managerId is null) return null;
            var approver = await db.HrEmployees.AsNoTracking()
                .Where(x => x.Id == managerId && x.Status != HrEmployeeStatus.Resigned && x.UserId != null)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync();
            return approver == userId ? null : approver;
        }

        /// <summary>Người có quyền Approve trên LeaveRequests; không có ai → phòng ADMIN (như module cũ).</summary>
        private static async Task<List<Guid>> FallbackApproversAsync(AppDbContext db)
        {
            var names = await db.Permissions.AsNoTracking()
                .Where(p => p.MenuName == LeaveMenu && p.Approve == true && p.UserName != null)
                .Select(p => p.UserName!)
                .ToListAsync();
            var ids = await db.UserList.AsNoTracking()
                .Where(u => u.Usr != null && names.Contains(u.Usr))
                .Select(u => u.UsrId)
                .ToListAsync();
            if (ids.Count == 0)
                ids = await db.UserList.AsNoTracking().Where(u => u.Department == "ADMIN").Select(u => u.UsrId).ToListAsync();
            return ids.Distinct().ToList();
        }

        // ═════════════════════════ Duyệt / hủy ═════════════════════════

        /// <param name="isHrApprover">User có quyền Approve trên LeaveRequests (duyệt được mọi đơn).</param>
        public async Task<HrResult> ReviewAsync(Guid id, bool approve, string? comment,
            Guid reviewerId, bool isHrApprover)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var req = await db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id);
                if (req is null) return HrResult.Fail("hr_err_not_found");
                if (req.Status != HrLeaveStatus.Pending) return HrResult.Fail("hr_err_leave_already_reviewed");
                if (req.EmployeeId == reviewerId) return HrResult.Fail("hr_err_leave_self_review");
                if (!isHrApprover && req.AssignedApproverId != reviewerId) return HrResult.Fail("hr_err_leave_not_approver");

                req.Status = approve ? HrLeaveStatus.Approved : HrLeaveStatus.Rejected;
                req.ApproverId = reviewerId;
                req.ApprovedDate = DateTime.Now;
                req.ApproverComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
                req.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync();

                var msg = $"Đơn {HrLeaveType.Vi(req.LeaveType).ToLowerInvariant()} {Describe(req)} " +
                          (approve ? "ĐÃ ĐƯỢC DUYỆT" : "BỊ TỪ CHỐI") +
                          (req.ApproverComment is null ? "" : $". Ghi chú: {req.ApproverComment}");
                await NotifyAsync(db, reviewerId, new List<Guid> { req.EmployeeId }, msg);
                return HrResult.Ok(approve ? "hr_leave_approved" : "hr_leave_rejected");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        /// <summary>HR hủy đơn đã duyệt (hoàn lại ngày phép).</summary>
        public async Task<HrResult> CancelApprovedAsync(Guid id, string? comment, Guid reviewerId, bool isHrApprover)
        {
            if (!isHrApprover) return HrResult.Fail("hr_err_leave_not_approver");
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var req = await db.LeaveRequests.FirstOrDefaultAsync(x => x.Id == id);
                if (req is null) return HrResult.Fail("hr_err_not_found");
                if (req.Status != HrLeaveStatus.Approved) return HrResult.Fail("hr_err_leave_not_approved");

                req.Status = HrLeaveStatus.Cancelled;
                req.ApproverId = reviewerId;
                req.ApprovedDate = DateTime.Now;
                req.ApproverComment = string.IsNullOrWhiteSpace(comment) ? req.ApproverComment : comment.Trim();
                req.UpdatedDate = DateTime.Now;
                await db.SaveChangesAsync();

                var msg = $"Đơn {HrLeaveType.Vi(req.LeaveType).ToLowerInvariant()} {Describe(req)} ĐÃ BỊ HỦY" +
                          (string.IsNullOrWhiteSpace(comment) ? "" : $". Ghi chú: {comment.Trim()}");
                await NotifyAsync(db, reviewerId, new List<Guid> { req.EmployeeId }, msg);
                return HrResult.Ok("hr_leave_cancelled");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        /// <summary>User là quản lý trực tiếp của ít nhất 1 người, hoặc đang được giao duyệt đơn.</summary>
        public async Task<bool> IsManagerApproverAsync(Guid userId)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var myEmpId = await db.HrEmployees.AsNoTracking()
                    .Where(x => x.UserId == userId).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
                if (myEmpId.HasValue &&
                    await db.HrEmployees.AnyAsync(x => x.ManagerEmployeeId == myEmpId && x.Status != HrEmployeeStatus.Resigned))
                    return true;
                return await db.LeaveRequests.AnyAsync(x => x.AssignedApproverId == userId && x.Status == HrLeaveStatus.Pending);
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> CountPendingForAsync(Guid userId, bool isHrApprover)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.LeaveRequests.CountAsync(x => x.Status == HrLeaveStatus.Pending && x.EmployeeId != userId &&
                                                          (x.AssignedApproverId == userId || isHrApprover));
        }

        // ═════════════════════════ Danh sách ═════════════════════════

        private IQueryable<HrLeaveRow> RowsQuery(AppDbContext db, IQueryable<LeaveRequest> q) =>
            from r in q
            join u in db.UserList.AsNoTracking() on r.EmployeeId equals u.UsrId into uj
            from u in uj.DefaultIfEmpty()
            join e in db.HrEmployees.AsNoTracking() on (Guid?)r.EmployeeId equals e.UserId into ej
            from e in ej.DefaultIfEmpty()
            join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
            from d in dj.DefaultIfEmpty()
            join a in db.UserList.AsNoTracking() on r.AssignedApproverId equals a.UsrId into aj
            from a in aj.DefaultIfEmpty()
            join p in db.UserList.AsNoTracking() on r.ApproverId equals p.UsrId into pj
            from p in pj.DefaultIfEmpty()
            select new HrLeaveRow
            {
                Id = r.Id,
                UserId = r.EmployeeId,
                UserName = e != null ? e.FullName : (u != null ? (u.Name ?? u.Usr ?? "") : ""),
                EmployeeId = e != null ? e.Id : null,
                EmployeeCode = e != null ? e.EmployeeCode : (u != null ? u.Usr : null),
                DepartmentName = d != null ? d.Name : (u != null ? u.Department : null),
                LeaveType = r.LeaveType,
                DurationType = r.DurationType,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                TotalDays = r.TotalDays,
                TotalHours = r.TotalHours,
                Reason = r.Reason,
                Status = r.Status,
                AssignedApproverId = r.AssignedApproverId,
                AssignedApproverName = a != null ? (a.Name ?? a.Usr) : null,
                ApproverId = r.ApproverId,
                ApproverName = p != null ? (p.Name ?? p.Usr) : null,
                ApprovedDate = r.ApprovedDate,
                ApproverComment = r.ApproverComment,
                CreatedDate = r.CreatedDate
            };

        public async Task<List<HrLeaveRow>> GetMyAsync(Guid userId, int year)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var from = new DateTime(year, 1, 1);
            var to = from.AddYears(1);
            var q = db.LeaveRequests.AsNoTracking()
                .Where(x => x.EmployeeId == userId && x.StartDate >= from && x.StartDate < to);
            return await RowsQuery(db, q).OrderByDescending(x => x.StartDate).ToListAsync();
        }

        public async Task<List<HrLeaveRow>> GetPendingForAsync(Guid userId, bool isHrApprover)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var q = db.LeaveRequests.AsNoTracking()
                .Where(x => x.Status == HrLeaveStatus.Pending && x.EmployeeId != userId &&
                            (x.AssignedApproverId == userId || isHrApprover));
            var rows = await RowsQuery(db, q).OrderBy(x => x.StartDate).ToListAsync();

            // Phép năm còn lại của người gửi (năm của ngày nghỉ)
            if (rows.Count > 0)
            {
                var s = await HrSettingsService.GetAsync(db);
                foreach (var g in rows.Where(r => r.EmployeeId.HasValue).GroupBy(r => r.StartDate.Year))
                {
                    var userIds = g.Select(r => r.UserId).Distinct().ToList();
                    var usage = await UsageAsync(db, userIds, g.Key, null);
                    var emps = await db.HrEmployees.AsNoTracking().Where(e => e.UserId != null && userIds.Contains(e.UserId.Value)).ToListAsync();
                    foreach (var emp in emps)
                    {
                        var bal = await GetOrBuildBalanceAsync(db, s, emp, g.Key, persist: false, actor: "system");
                        usage.TryGetValue(emp.UserId!.Value, out var u);
                        foreach (var r in g.Where(r => r.UserId == emp.UserId))
                            r.RemainingAnnual = bal.Total - u.Used;
                    }
                }
            }
            return rows;
        }

        public async Task<List<HrLeaveRow>> GetAllAsync(HrLeaveFilter f)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var q = db.LeaveRequests.AsNoTracking().AsQueryable();
            if (f.Year.HasValue)
            {
                var from = f.Month.HasValue ? new DateTime(f.Year.Value, f.Month.Value, 1) : new DateTime(f.Year.Value, 1, 1);
                var to = f.Month.HasValue ? from.AddMonths(1) : from.AddYears(1);
                q = q.Where(x => x.StartDate < to && x.EndDate >= from);
            }
            if (f.Status.HasValue) q = q.Where(x => x.Status == f.Status.Value);
            if (f.LeaveType.HasValue) q = q.Where(x => x.LeaveType == f.LeaveType.Value);

            var rows = RowsQuery(db, q);
            if (f.DepartmentId.HasValue)
            {
                var deptUsers = db.HrEmployees.Where(e => e.DepartmentId == f.DepartmentId && e.UserId != null)
                    .Select(e => e.UserId!.Value);
                rows = rows.Where(r => deptUsers.Contains(r.UserId));
            }
            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var t = f.Search.Trim();
                rows = rows.Where(r => r.UserName.Contains(t) || (r.EmployeeCode != null && r.EmployeeCode.Contains(t))
                                       || r.Reason.Contains(t));
            }
            return await rows.OrderByDescending(x => x.StartDate).Take(2000).ToListAsync();
        }

        // ═════════════════════════ Quỹ phép năm ═════════════════════════

        private readonly record struct Usage(decimal Used, decimal Pending);

        /// <summary>Phép năm đã dùng (đã duyệt) và đang chờ duyệt, theo UsrId, trong năm (theo StartDate).</summary>
        private static async Task<Dictionary<Guid, Usage>> UsageAsync(AppDbContext db, IReadOnlyCollection<Guid> userIds,
            int year, Guid? excludeId)
        {
            var from = new DateTime(year, 1, 1);
            var to = from.AddYears(1);
            var q = db.LeaveRequests.AsNoTracking()
                .Where(x => x.LeaveType == HrLeaveType.Annual && x.StartDate >= from && x.StartDate < to
                            && (x.Status == HrLeaveStatus.Approved || x.Status == HrLeaveStatus.Pending)
                            && x.Id != excludeId);
            if (userIds.Count <= 500)
                q = q.Where(x => userIds.Contains(x.EmployeeId));
            var rows = await q.GroupBy(x => new { x.EmployeeId, x.Status })
                .Select(g => new { g.Key.EmployeeId, g.Key.Status, Days = g.Sum(x => x.TotalDays) })
                .ToListAsync();
            return rows.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => new Usage(
                g.Where(x => x.Status == HrLeaveStatus.Approved).Sum(x => x.Days),
                g.Where(x => x.Status == HrLeaveStatus.Pending).Sum(x => x.Days)));
        }

        /// <summary>Tính quỹ phép mặc định theo cấu hình (chưa gồm Adjustment).</summary>
        private static async Task<HrLeaveBalance> ComputeDefaultAsync(AppDbContext db, HrWorkSettings s, HrEmployee emp, int year)
        {
            var b = new HrLeaveBalance { EmployeeId = emp.Id, Year = year };
            var jan1 = new DateTime(year, 1, 1);
            var join = emp.JoinDate?.Date;

            if (join.HasValue && join.Value.Year > year)
            {
                b.Entitled = 0;
            }
            else if (join.HasValue && join.Value.Year == year)
            {
                // Vào làm trong năm: tính theo tỷ lệ số tháng làm việc (tháng vào làm tính nếu vào trước ngày 16).
                var months = 12 - join.Value.Month + (join.Value.Day <= 15 ? 1 : 0);
                b.Entitled = RoundHalf(s.AnnualLeaveBaseDays * months / 12m);
            }
            else
            {
                b.Entitled = s.AnnualLeaveBaseDays;
            }

            if (join.HasValue && s.SeniorityStepYears > 0 && join.Value < jan1)
            {
                var years = jan1.Year - join.Value.Year;
                if (join.Value.AddYears(years) > jan1) years--;
                b.SeniorityBonus = years / s.SeniorityStepYears;
            }

            // Phép tồn năm trước (nếu có bản ghi năm trước)
            if (s.MaxCarryOverDays > 0 && emp.UserId.HasValue)
            {
                var prev = await db.HrLeaveBalances.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.EmployeeId == emp.Id && x.Year == year - 1);
                if (prev != null)
                {
                    var used = await UsageAsync(db, new[] { emp.UserId.Value }, year - 1, null);
                    used.TryGetValue(emp.UserId.Value, out var u);
                    var remain = prev.Entitled + prev.SeniorityBonus + prev.CarriedOver + prev.Adjustment - u.Used;
                    b.CarriedOver = Math.Max(0, Math.Min(remain, s.MaxCarryOverDays));
                }
            }
            return b;
        }

        private static decimal RoundHalf(decimal v) => Math.Round(v * 2, MidpointRounding.AwayFromZero) / 2;

        private static async Task<HrLeaveBalance> GetOrBuildBalanceAsync(AppDbContext db, HrWorkSettings s, HrEmployee emp,
            int year, bool persist, string actor)
        {
            var existing = await db.HrLeaveBalances.AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmployeeId == emp.Id && x.Year == year);
            if (existing != null) return existing;

            var b = await ComputeDefaultAsync(db, s, emp, year);
            if (persist)
            {
                b.Id = Guid.NewGuid();
                b.CreatedAt = DateTime.Now;
                b.CreatedBy = actor;
                db.HrLeaveBalances.Add(b);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // Có thể đã được tạo đồng thời bởi request khác — bỏ qua.
                    db.ChangeTracker.Clear();
                }
            }
            return b;
        }

        public async Task<List<HrLeaveBalanceRow>> GetBalancesAsync(int year, Guid? departmentId, string? search)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var yearEnd = new DateTime(year, 12, 31);
            var yearStart = new DateTime(year, 1, 1);

            var empQ = db.HrEmployees.AsNoTracking()
                .Where(e => (e.JoinDate == null || e.JoinDate <= yearEnd)
                            && (e.Status != HrEmployeeStatus.Resigned || e.ResignDate == null || e.ResignDate >= yearStart));
            if (departmentId.HasValue) empQ = empQ.Where(e => e.DepartmentId == departmentId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var t = search.Trim();
                empQ = empQ.Where(e => e.EmployeeCode.Contains(t) || e.FullName.Contains(t));
            }

            var emps = await (
                from e in empQ
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                orderby e.EmployeeCode
                select new { E = e, DeptName = d != null ? d.Name : null }).ToListAsync();

            var empIds = emps.Select(x => x.E.Id).ToList();
            var balances = await db.HrLeaveBalances.AsNoTracking()
                .Where(b => b.Year == year)
                .ToListAsync();
            var balMap = balances.Where(b => empIds.Contains(b.EmployeeId)).ToDictionary(b => b.EmployeeId);
            var userIds = emps.Where(x => x.E.UserId.HasValue).Select(x => x.E.UserId!.Value).ToList();
            var usage = await UsageAsync(db, userIds, year, null);

            var list = new List<HrLeaveBalanceRow>();
            foreach (var x in emps)
            {
                var b = balMap.TryGetValue(x.E.Id, out var found) ? found : await ComputeDefaultAsync(db, s, x.E, year);
                var u = x.E.UserId.HasValue && usage.TryGetValue(x.E.UserId.Value, out var uu) ? uu : default;
                list.Add(new HrLeaveBalanceRow
                {
                    BalanceId = found?.Id,
                    EmployeeId = x.E.Id,
                    EmployeeCode = x.E.EmployeeCode,
                    FullName = x.E.FullName,
                    DepartmentName = x.DeptName,
                    UserId = x.E.UserId,
                    JoinDate = x.E.JoinDate,
                    Year = year,
                    Entitled = b.Entitled,
                    SeniorityBonus = b.SeniorityBonus,
                    CarriedOver = b.CarriedOver,
                    Adjustment = b.Adjustment,
                    Note = b.Note,
                    Used = u.Used,
                    Pending = u.Pending
                });
            }
            return list;
        }

        /// <summary>Quỹ phép của chính user (null nếu chưa có hồ sơ nhân viên).</summary>
        public async Task<HrLeaveBalanceRow?> GetMyBalanceAsync(Guid userId, int year)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var emp = await db.HrEmployees.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            if (emp is null) return null;
            var s = await HrSettingsService.GetAsync(db);
            var b = await GetOrBuildBalanceAsync(db, s, emp, year, persist: false, actor: "system");
            var usage = await UsageAsync(db, new[] { userId }, year, null);
            usage.TryGetValue(userId, out var u);
            return new HrLeaveBalanceRow
            {
                BalanceId = b.Id == Guid.Empty ? null : b.Id,
                EmployeeId = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                FullName = emp.FullName,
                UserId = userId,
                JoinDate = emp.JoinDate,
                Year = year,
                Entitled = b.Entitled,
                SeniorityBonus = b.SeniorityBonus,
                CarriedOver = b.CarriedOver,
                Adjustment = b.Adjustment,
                Note = b.Note,
                Used = u.Used,
                Pending = u.Pending
            };
        }

        /// <summary>
        /// Tạo quỹ phép năm cho NV chưa có; recalcExisting = true thì tính lại Entitled/Thâm niên/Tồn
        /// cho cả bản ghi đã có (giữ nguyên Adjustment và ghi chú).
        /// </summary>
        public async Task<HrResult> GenerateAsync(int year, bool recalcExisting, string actor)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var s = await HrSettingsService.GetAsync(db);
                var yearEnd = new DateTime(year, 12, 31);
                var yearStart = new DateTime(year, 1, 1);
                var emps = await db.HrEmployees.AsNoTracking()
                    .Where(e => (e.JoinDate == null || e.JoinDate <= yearEnd)
                                && (e.Status != HrEmployeeStatus.Resigned || e.ResignDate == null || e.ResignDate >= yearStart))
                    .ToListAsync();
                var existing = await db.HrLeaveBalances.Where(b => b.Year == year).ToDictionaryAsync(b => b.EmployeeId);

                int created = 0, updated = 0;
                var now = DateTime.Now;
                foreach (var e in emps)
                {
                    var def = await ComputeDefaultAsync(db, s, e, year);
                    if (existing.TryGetValue(e.Id, out var b))
                    {
                        if (!recalcExisting) continue;
                        b.Entitled = def.Entitled;
                        b.SeniorityBonus = def.SeniorityBonus;
                        b.CarriedOver = def.CarriedOver;
                        b.UpdatedAt = now;
                        b.UpdatedBy = actor;
                        updated++;
                    }
                    else
                    {
                        def.Id = Guid.NewGuid();
                        def.CreatedAt = now;
                        def.CreatedBy = actor;
                        db.HrLeaveBalances.Add(def);
                        created++;
                    }
                }
                await db.SaveChangesAsync();
                return new HrResult(true, "hr_balance_generated", null, $"{created}|{updated}");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        /// <summary>Sửa tay 1 dòng quỹ phép (tạo mới nếu chưa có).</summary>
        public async Task<HrResult> SaveBalanceAsync(Guid employeeId, int year, decimal entitled, decimal seniority,
            decimal carried, decimal adjustment, string? note, string actor)
        {
            if (entitled < 0 || seniority < 0 || carried < 0) return HrResult.Fail("hr_err_negative_value");
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var b = await db.HrLeaveBalances.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year);
                var now = DateTime.Now;
                if (b is null)
                {
                    if (!await db.HrEmployees.AnyAsync(x => x.Id == employeeId)) return HrResult.Fail("hr_err_not_found");
                    b = new HrLeaveBalance { Id = Guid.NewGuid(), EmployeeId = employeeId, Year = year, CreatedAt = now, CreatedBy = actor };
                    db.HrLeaveBalances.Add(b);
                }
                b.Entitled = entitled;
                b.SeniorityBonus = seniority;
                b.CarriedOver = carried;
                b.Adjustment = adjustment;
                b.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
                b.UpdatedAt = now;
                b.UpdatedBy = actor;
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", b.Id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ═════════════════════════ Tiện ích ═════════════════════════

        public static string Describe(LeaveRequest r) => r.DurationType switch
        {
            HrLeaveDuration.FullDay when r.EndDate.Date != r.StartDate.Date =>
                $"{r.StartDate:dd/MM/yyyy} - {r.EndDate:dd/MM/yyyy} ({r.TotalDays:0.##} ngày)",
            HrLeaveDuration.FullDay => $"{r.StartDate:dd/MM/yyyy} ({r.TotalDays:0.##} ngày)",
            HrLeaveDuration.HalfDay =>
                $"{r.StartDate:dd/MM/yyyy} ({(r.StartTime is null || r.StartTime < MorningEnd ? "buổi sáng" : "buổi chiều")})",
            _ => $"{r.StartDate:dd/MM/yyyy} {r.StartTime:hh\\:mm}-{r.EndTime:hh\\:mm} ({r.TotalHours:0.##} giờ)"
        };

        private static async Task<string> UserDisplayAsync(AppDbContext db, Guid userId)
        {
            var emp = await db.HrEmployees.AsNoTracking().Where(x => x.UserId == userId)
                .Select(x => x.FullName).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(emp)) return emp;
            var u = await db.UserList.AsNoTracking().Where(x => x.UsrId == userId)
                .Select(x => new { x.Name, x.Usr }).FirstOrDefaultAsync();
            return u?.Name ?? u?.Usr ?? userId.ToString();
        }

        private async Task NotifyAsync(AppDbContext db, Guid sender, List<Guid> receivers, string message)
        {
            if (receivers.Count == 0) return;
            try
            {
                foreach (var r in receivers.Distinct())
                {
                    db.Notifications.Add(new Notification
                    {
                        SenderUserId = sender,
                        ReceiverUserId = r,
                        Message = message,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await db.SaveChangesAsync();

                foreach (var r in receivers.Distinct())
                {
                    await hub.Clients.User(r.ToString()).SendAsync("ReceiveNotification", message);
                    var count = await db.Notifications.CountAsync(n => n.ReceiverUserId == r && !n.IsRead);
                    await hub.Clients.User(r.ToString()).SendAsync("UpdateUnreadEmailCount", count);
                }
            }
            catch (Exception ex)
            {
                // Không để lỗi thông báo làm hỏng việc lưu đơn.
                logger.LogWarning(ex, "Leave notification failed");
            }
        }
    }
}
