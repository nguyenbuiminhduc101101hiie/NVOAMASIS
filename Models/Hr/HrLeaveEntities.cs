using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models.Hr
{
    /// <summary>
    /// Quỹ phép năm của 1 nhân viên (bảng HrLeaveBalance) — script Scripts/CreateHrLeaveTimesheetTables.sql.
    /// Số ngày ĐÃ DÙNG không lưu ở đây mà tính trực tiếp từ LeaveRequests (loại 1, đã duyệt).
    /// Còn lại = Entitled + SeniorityBonus + CarriedOver + Adjustment - Đã dùng.
    /// </summary>
    public class HrLeaveBalance
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public int Year { get; set; }
        /// <summary>Phép tiêu chuẩn (đã tính tỷ lệ theo tháng nếu vào làm trong năm).</summary>
        public decimal Entitled { get; set; }
        /// <summary>Phép thâm niên (+1 ngày mỗi N năm).</summary>
        public decimal SeniorityBonus { get; set; }
        /// <summary>Phép tồn chuyển từ năm trước.</summary>
        public decimal CarriedOver { get; set; }
        /// <summary>Điều chỉnh tay (+/-), không bị ghi đè khi tính lại.</summary>
        public decimal Adjustment { get; set; }
        [MaxLength(1000)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }

        public decimal Total => Entitled + SeniorityBonus + CarriedOver + Adjustment;
    }

    /// <summary>Ngày nghỉ lễ / nghỉ bù (bảng HrHoliday) — không tính công, không trừ phép.</summary>
    public class HrHoliday
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        [MaxLength(200)] public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
    }

    /// <summary>Cấu hình nhân sự dạng key/value (bảng HrSetting), riêng cho từng tenant.</summary>
    public class HrSetting
    {
        [Key]
        [MaxLength(100)] public string Key { get; set; } = string.Empty;
        [MaxLength(500)] public string? Value { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    /// <summary>Loại nghỉ — trùng mã với LeaveRequests.LeaveType đang dùng.</summary>
    public static class HrLeaveType
    {
        public const int Annual = 1;
        public const int Sick = 2;
        public const int Personal = 3;
        public const int Unpaid = 4;
        public const int GoOut = 5;

        public static readonly int[] All = { Annual, Sick, Personal, Unpaid, GoOut };

        public static string Key(int t) => t switch
        {
            Annual => "hr_leave_type_annual",
            Sick => "hr_leave_type_sick",
            Personal => "hr_leave_type_personal",
            Unpaid => "hr_leave_type_unpaid",
            GoOut => "hr_leave_type_goout",
            _ => "hr_unknown"
        };

        /// <summary>Ký hiệu trên bảng công.</summary>
        public static string Code(int t) => t switch
        {
            Annual => "P",
            Sick => "Ô",
            Personal => "R",
            Unpaid => "KL",
            _ => "?"
        };

        public static string Vi(int t) => t switch
        {
            Annual => "Nghỉ phép năm",
            Sick => "Nghỉ ốm",
            Personal => "Nghỉ việc riêng",
            Unpaid => "Nghỉ không lương",
            GoOut => "Xin ra ngoài",
            _ => "Khác"
        };
    }

    /// <summary>1=Cả ngày, 2=Nửa ngày, 3=Theo giờ (trùng LeaveRequests.DurationType).</summary>
    public static class HrLeaveDuration
    {
        public const int FullDay = 1;
        public const int HalfDay = 2;
        public const int Hours = 3;

        public static string Key(int d) => d switch
        {
            FullDay => "hr_duration_full",
            HalfDay => "hr_duration_half",
            Hours => "hr_duration_hours",
            _ => "hr_unknown"
        };
    }

    /// <summary>1=Chờ duyệt, 2=Đã duyệt, 3=Từ chối, 4=Đã hủy (HR hủy đơn đã duyệt).</summary>
    public static class HrLeaveStatus
    {
        public const int Pending = 1;
        public const int Approved = 2;
        public const int Rejected = 3;
        public const int Cancelled = 4;

        public static readonly int[] All = { Pending, Approved, Rejected, Cancelled };

        public static string Key(int s) => s switch
        {
            Pending => "hr_leave_status_pending",
            Approved => "hr_leave_status_approved",
            Rejected => "hr_leave_status_rejected",
            Cancelled => "hr_leave_status_cancelled",
            _ => "hr_unknown"
        };
    }

    /// <summary>Khóa trong bảng HrSetting.</summary>
    public static class HrSettingKeys
    {
        public const string WorkDays = "WorkDays";
        public const string SaturdayHalfDay = "SaturdayHalfDay";
        public const string SaturdayHalfDayFullCredit = "SaturdayHalfDayFullCredit";
        public const string FixedStandardDays = "FixedStandardDays";
        // Chấm công hằng ngày
        public const string AttendanceMode = "AttendanceMode";
        public const string WorkStart = "WorkStart";
        public const string LunchStart = "LunchStart";
        public const string LunchEnd = "LunchEnd";
        public const string WorkEnd = "WorkEnd";
        public const string SaturdayEnd = "SaturdayEnd";
        public const string LateGraceMinutes = "LateGraceMinutes";
        public const string OfficeLatitude = "OfficeLatitude";
        public const string OfficeLongitude = "OfficeLongitude";
        public const string OfficeRadiusM = "OfficeRadiusM";
        public const string MobileRequireOnsite = "MobileRequireOnsite";
        public const string HoursPerDay = "HoursPerDay";
        public const string AnnualLeaveBaseDays = "AnnualLeaveBaseDays";
        public const string SeniorityStepYears = "SeniorityStepYears";
        public const string MaxCarryOverDays = "MaxCarryOverDays";
        public const string AllowNegativeAnnualLeave = "AllowNegativeAnnualLeave";
        public const string AttendanceIpSource = "AttendanceIpSource";
    }

    /// <summary>Cấu hình đã parse, có giá trị mặc định.</summary>
    public sealed class HrWorkSettings
    {
        /// <summary>DayOfWeek làm việc (0=CN ... 6=T7). Mặc định T2–T6.</summary>
        public HashSet<DayOfWeek> WorkDays { get; set; } = new()
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
        };
        /// <summary>Thứ 7 chỉ làm buổi sáng (tính 0,5 công) — chỉ có tác dụng khi T7 nằm trong WorkDays.</summary>
        public bool SaturdayHalfDay { get; set; }
        /// <summary>
        /// Khi SaturdayHalfDay bật: buổi sáng T7 được tính 1 công (thay vì 0,5).
        /// Dùng cho công ty làm T2 – sáng T7 và tính 26 công/tháng.
        /// </summary>
        public bool SaturdayHalfDayFullCredit { get; set; }
        /// <summary>Số công của 1 ngày T7 làm buổi sáng.</summary>
        public decimal SaturdayHalfDayWeight => SaturdayHalfDayFullCredit ? 1m : 0.5m;
        /// <summary>
        /// Công chuẩn cố định để tính lương (vd 24 hoặc 26). 0 = theo lịch thực tế của từng tháng.
        /// Chỉ ảnh hưởng bảng lương; bảng công vẫn hiển thị theo lịch.
        /// </summary>
        public decimal FixedStandardDays { get; set; }
        public decimal HoursPerDay { get; set; } = 8;
        public decimal AnnualLeaveBaseDays { get; set; } = 12;
        public int SeniorityStepYears { get; set; } = 5;
        public decimal MaxCarryOverDays { get; set; }
        public bool AllowNegativeAnnualLeave { get; set; }
        /// <summary>"client" (mặc định, như cũ) hoặc "server".</summary>
        public string AttendanceIpSource { get; set; } = "client";

        // ───── Chấm công hằng ngày ─────
        /// <summary>
        /// "session" (mặc định, như cũ): mỗi buổi bấm 1 lần trong khung [giờ bắt đầu buổi, +1 giờ].
        /// "inout": chấm giờ vào / giờ ra bất kỳ lúc nào trong ngày, tính đi muộn / về sớm.
        /// </summary>
        public string AttendanceMode { get; set; } = HrAttendanceModes.Session;
        public TimeSpan WorkStart { get; set; } = new(8, 0, 0);
        public TimeSpan LunchStart { get; set; } = new(12, 0, 0);
        public TimeSpan LunchEnd { get; set; } = new(13, 0, 0);
        public TimeSpan WorkEnd { get; set; } = new(17, 0, 0);
        /// <summary>Giờ kết thúc T7 khi chỉ làm buổi sáng.</summary>
        public TimeSpan SaturdayEnd { get; set; } = new(12, 0, 0);
        /// <summary>Số phút cho phép đến muộn / về sớm mà không tính.</summary>
        public int LateGraceMinutes { get; set; } = 5;
        /// <summary>Tọa độ văn phòng để chấm công GPS trên điện thoại (null = không dùng GPS).</summary>
        public decimal? OfficeLatitude { get; set; }
        public decimal? OfficeLongitude { get; set; }
        public int OfficeRadiusM { get; set; } = 200;
        /// <summary>true = điện thoại chỉ chấm được khi ở văn phòng (WiFi văn phòng hoặc trong bán kính GPS).</summary>
        public bool MobileRequireOnsite { get; set; }

        public bool HasOfficeLocation => OfficeLatitude.HasValue && OfficeLongitude.HasValue;
    }

    public static class HrAttendanceModes
    {
        public const string Session = "session";
        public const string InOut = "inout";
    }

    public static class HrPunchTypes
    {
        public const string In = "in";
        public const string Out = "out";
    }

    public static class HrAttendanceSources
    {
        public const string Web = "WEB";
        public const string Mobile = "MOBILE";
        public const string Manual = "MANUAL";
    }
}
