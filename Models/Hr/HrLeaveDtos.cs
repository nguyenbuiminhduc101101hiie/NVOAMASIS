namespace NVOAMASIS.Models.Hr
{
    /// <summary>Đơn nghỉ kèm thông tin người gửi / người duyệt.</summary>
    public sealed class HrLeaveRow
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public int LeaveType { get; set; }
        public int DurationType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal TotalDays { get; set; }
        public decimal? TotalHours { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int Status { get; set; }
        public Guid? AssignedApproverId { get; set; }
        public string? AssignedApproverName { get; set; }
        public Guid? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApproverComment { get; set; }
        public DateTime CreatedDate { get; set; }
        /// <summary>Phép năm còn lại của người gửi (chỉ điền ở tab duyệt).</summary>
        public decimal? RemainingAnnual { get; set; }
    }

    public sealed class HrLeaveFilter
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Status { get; set; }
        public int? LeaveType { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Search { get; set; }
    }

    /// <summary>Dữ liệu nhập đơn nghỉ (tạo/sửa).</summary>
    public sealed class HrLeaveInput
    {
        public Guid? Id { get; set; }
        public int LeaveType { get; set; } = HrLeaveType.Annual;
        public int DurationType { get; set; } = HrLeaveDuration.FullDay;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;
        /// <summary>Nửa ngày: true = buổi sáng, false = buổi chiều.</summary>
        public bool HalfMorning { get; set; } = true;
        public TimeSpan? StartTime { get; set; } = new(8, 0, 0);
        public TimeSpan? EndTime { get; set; } = new(10, 0, 0);
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>Kết quả tính số ngày/giờ của 1 đơn.</summary>
    public readonly record struct HrLeaveQuantity(decimal Days, decimal? Hours);

    public sealed class HrLeaveBalanceRow
    {
        public Guid? BalanceId { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? JoinDate { get; set; }
        public int Year { get; set; }
        public decimal Entitled { get; set; }
        public decimal SeniorityBonus { get; set; }
        public decimal CarriedOver { get; set; }
        public decimal Adjustment { get; set; }
        public decimal Used { get; set; }
        public decimal Pending { get; set; }
        public string? Note { get; set; }
        public decimal Total => Entitled + SeniorityBonus + CarriedOver + Adjustment;
        public decimal Remaining => Total - Used;
        /// <summary>Còn lại sau khi trừ cả đơn đang chờ duyệt.</summary>
        public decimal Available => Remaining - Pending;
        public bool HasRecord => BalanceId.HasValue;
    }

    // ───────────── Bảng công ─────────────

    public sealed class HrTimesheetFilter
    {
        public int Year { get; set; } = DateTime.Today.Year;
        public int Month { get; set; } = DateTime.Today.Month;
        /// <summary>Chỉ 1 người (UserList.UsrId).</summary>
        public Guid? UserId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Branch { get; set; }
        public string? Search { get; set; }
    }

    /// <summary>Loại ngày trong lịch.</summary>
    public enum HrDayKind { Work, HalfWork, Off, Holiday }

    public sealed class HrTimesheetDay
    {
        public DateTime Date { get; set; }
        public HrDayKind Kind { get; set; }
        public string? HolidayName { get; set; }
        /// <summary>
        /// Số công của ngày làm việc / ngày lễ (1; 0,5 cho T7 làm buổi sáng, hoặc 1 nếu bật "T7 sáng tính 1 công").
        /// Gán trong HrSettingsService.Classify.
        /// </summary>
        public decimal Weight { get; set; } = 1m;
        /// <summary>
        /// Công chuẩn của ngày. Ngày lễ nằm trong công chuẩn vì được tính vào công hưởng lương (row.Paid);
        /// nếu không, nhân viên vắng 1 ngày trong tháng có lễ vẫn được đủ lương.
        /// </summary>
        public decimal Standard => Kind == HrDayKind.Off ? 0m : Weight;
        /// <summary>Số công của ngày khi tính đơn nghỉ (ngày lễ / ngày nghỉ không trừ phép).</summary>
        public decimal LeaveDays => Kind is HrDayKind.Work or HrDayKind.HalfWork ? Weight : 0m;
    }

    public sealed class HrTimesheetCell
    {
        /// <summary>Ký hiệu hiển thị: X, R, P, Ô, KL, V, L, X/P ...</summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>Giải thích (tooltip).</summary>
        public string? Tip { get; set; }
        public bool Warn { get; set; }
        /// <summary>Ký hiệu buổi sáng / chiều (rỗng = chưa chấm / không làm).</summary>
        public string MorningCode { get; set; } = string.Empty;
        public string AfternoonCode { get; set; } = string.Empty;
    }

    public sealed class HrTimesheetRow
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? Branch { get; set; }
        public bool HasAccount { get; set; }
        /// <summary>UserList.UsrId của nhân viên (null = chưa có tài khoản).</summary>
        public Guid? UserId { get; set; }
        public HrTimesheetCell[] Cells { get; set; } = Array.Empty<HrTimesheetCell>();

        public decimal Standard { get; set; }
        public decimal Worked { get; set; }
        public decimal Remote { get; set; }
        public decimal Annual { get; set; }
        public decimal Sick { get; set; }
        public decimal Personal { get; set; }
        public decimal Unpaid { get; set; }
        public decimal Holiday { get; set; }
        public decimal Absent { get; set; }
        public int GoOutCount { get; set; }
        /// <summary>Công hưởng lương = làm việc + phép năm + việc riêng có lương + lễ.</summary>
        public decimal Paid => Worked + Annual + Personal + Holiday;
    }

    public sealed class HrTimesheet
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<HrTimesheetDay> Days { get; set; } = new();
        public List<HrTimesheetRow> Rows { get; set; } = new();
        public decimal StandardDays => Days.Sum(d => d.Standard);
    }
}
