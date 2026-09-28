using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models.Hr
{
    /// <summary>
    /// Hồ sơ nhân viên — script Scripts/CreateHrTables.sql (bảng HrEmployee).
    /// Liên kết tùy chọn 1-1 với tài khoản đăng nhập (UserList.UsrId).
    /// Phòng ban dùng lại bảng Department có sẵn.
    /// </summary>
    public class HrEmployee
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(30)]
        public string EmployeeCode { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>0=Chưa rõ, 1=Nam, 2=Nữ, 3=Khác</summary>
        public int Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
        [MaxLength(200)] public string? PlaceOfBirth { get; set; }

        [MaxLength(20)] public string? IdCardNo { get; set; }
        public DateTime? IdCardIssueDate { get; set; }
        [MaxLength(200)] public string? IdCardIssuePlace { get; set; }

        [MaxLength(30)] public string? Phone { get; set; }
        [MaxLength(200)] public string? PersonalEmail { get; set; }
        [MaxLength(200)] public string? WorkEmail { get; set; }
        [MaxLength(500)] public string? PermanentAddress { get; set; }
        [MaxLength(500)] public string? CurrentAddress { get; set; }

        /// <summary>Tài khoản đăng nhập (UserList.UsrId) — có thể trống.</summary>
        public Guid? UserId { get; set; }

        /// <summary>Department.DepartmentId</summary>
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }

        [MaxLength(10)] public string? Branch { get; set; }
        [MaxLength(50)] public string? CompanyCode { get; set; }

        /// <summary>Quản lý trực tiếp (HrEmployee.Id)</summary>
        public Guid? ManagerEmployeeId { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? OfficialDate { get; set; }

        /// <summary>Xem <see cref="HrEmployeeStatus"/></summary>
        public int Status { get; set; } = HrEmployeeStatus.Probation;

        public DateTime? ResignDate { get; set; }
        [MaxLength(500)] public string? ResignReason { get; set; }

        [MaxLength(20)] public string? PersonalTaxCode { get; set; }
        [MaxLength(20)] public string? SocialInsuranceNo { get; set; }
        [MaxLength(30)] public string? BankAccountNo { get; set; }
        [MaxLength(200)] public string? BankName { get; set; }
        [MaxLength(200)] public string? BankBranch { get; set; }
        public int DependentCount { get; set; }

        [MaxLength(200)] public string? EmergencyContactName { get; set; }
        [MaxLength(30)] public string? EmergencyContactPhone { get; set; }

        [MaxLength(2000)] public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    /// <summary>Danh mục chức vụ (bảng HrPosition).</summary>
    public class HrPosition
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(30)] public string Code { get; set; } = string.Empty;
        [MaxLength(200)] public string Name { get; set; } = string.Empty;
        /// <summary>Cấp bậc (số nhỏ = cấp cao), dùng để sắp xếp.</summary>
        public int SortOrder { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
    }

    /// <summary>Hợp đồng lao động (bảng HrContract).</summary>
    public class HrContract
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }

        [MaxLength(50)] public string ContractNo { get; set; } = string.Empty;

        /// <summary>Xem <see cref="HrContractType"/></summary>
        public int ContractType { get; set; } = HrContractType.Probation;

        public DateTime? SignDate { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        /// <summary>Null = không xác định thời hạn.</summary>
        public DateTime? EndDate { get; set; }

        public decimal? BaseSalary { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public decimal? Allowance { get; set; }

        /// <summary>1=Hiệu lực, 2=Đã chấm dứt. "Hết hạn" được tính theo EndDate.</summary>
        public int Status { get; set; } = HrContractStatus.Active;
        public DateTime? TerminatedDate { get; set; }

        [MaxLength(2000)] public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    /// <summary>Lịch sử thay đổi hồ sơ (bảng HrEmployeeHistory) — ghi tự động khi lưu.</summary>
    public class HrEmployeeHistory
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        [MaxLength(50)] public string ChangeType { get; set; } = string.Empty;
        [MaxLength(500)] public string? OldValue { get; set; }
        [MaxLength(500)] public string? NewValue { get; set; }
        public DateTime EffectiveDate { get; set; } = DateTime.Today;
        [MaxLength(1000)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
    }

    /// <summary>Giấy tờ đính kèm hồ sơ (bảng HrDocument). File lưu ngoài wwwroot.</summary>
    public class HrDocument
    {
        [Key]
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        [MaxLength(50)] public string DocType { get; set; } = HrDocumentType.Other;
        [MaxLength(200)] public string? Title { get; set; }
        [MaxLength(260)] public string FileName { get; set; } = string.Empty;
        [MaxLength(500)] public string FilePath { get; set; } = string.Empty;
        [MaxLength(150)] public string ContentType { get; set; } = "application/octet-stream";
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? UploadedBy { get; set; }
    }

    public static class HrEmployeeStatus
    {
        public const int Probation = 1;
        public const int Active = 2;
        public const int Suspended = 3;
        public const int Resigned = 4;

        public static readonly int[] All = { Probation, Active, Suspended, Resigned };

        /// <summary>Khóa localization tương ứng.</summary>
        public static string Key(int status) => status switch
        {
            Probation => "hr_status_probation",
            Active => "hr_status_active",
            Suspended => "hr_status_suspended",
            Resigned => "hr_status_resigned",
            _ => "hr_unknown"
        };
    }

    public static class HrContractType
    {
        public const int Probation = 1;
        public const int FixedTerm = 2;
        public const int Indefinite = 3;
        public const int Service = 4;

        public static readonly int[] All = { Probation, FixedTerm, Indefinite, Service };

        public static string Key(int type) => type switch
        {
            Probation => "hr_contract_type_probation",
            FixedTerm => "hr_contract_type_fixed",
            Indefinite => "hr_contract_type_indefinite",
            Service => "hr_contract_type_service",
            _ => "hr_unknown"
        };
    }

    public static class HrContractStatus
    {
        public const int Active = 1;
        public const int Terminated = 2;
    }

    public static class HrGender
    {
        public static readonly int[] All = { 0, 1, 2, 3 };

        public static string Key(int gender) => gender switch
        {
            1 => "hr_gender_male",
            2 => "hr_gender_female",
            3 => "hr_gender_other",
            _ => "hr_unknown"
        };
    }

    public static class HrDocumentType
    {
        public const string Cv = "CV";
        public const string IdCard = "IDCARD";
        public const string Degree = "DEGREE";
        public const string Contract = "CONTRACT";
        public const string Health = "HEALTH";
        public const string Other = "OTHER";

        public static readonly string[] All = { Cv, IdCard, Degree, Contract, Health, Other };

        public static string Key(string? type) => "hr_doctype_" + (type ?? Other).ToLowerInvariant();
    }

    /// <summary>Mã quyền (MenuNames.MenuName) — script Scripts/Add_HR_Menu_Permissions.sql.</summary>
    public static class HrMenus
    {
        public const string Dashboard = "HR_Dashboard";
        public const string Employee = "HR_Employee";
        public const string Contract = "HR_Contract";
        public const string Org = "HR_Org";
        /// <summary>See = xem lương trong hợp đồng; Edit = nhập/sửa lương.</summary>
        public const string Salary = "HR_Salary";
        // Giai đoạn 2
        /// <summary>See = xem quỹ phép mọi NV; Edit = tạo/tính lại/sửa quỹ phép.</summary>
        public const string LeaveBalance = "HR_LeaveBalance";
        /// <summary>See = xem/xuất bảng công tháng.</summary>
        public const string Timesheet = "HR_Timesheet";
        /// <summary>See = xem cài đặt; Edit = sửa lịch làm việc, ngày lễ, quy tắc phép.</summary>
        public const string Settings = "HR_Settings";
        // Giai đoạn 3
        /// <summary>See = xem bảng lương mọi NV; Add = tạo kỳ; Edit = tính lại, sửa phiếu; Delete = xóa kỳ nháp;
        /// Approve = chốt / mở chốt / hạch toán / hủy hạch toán, sửa tham số lương.</summary>
        public const string Payroll = "HR_Payroll";

        public static readonly string[] All = { Dashboard, Employee, Contract, Org, Salary, LeaveBalance, Timesheet, Settings, Payroll };
    }

    /// <summary>Loại thay đổi ghi vào HrEmployeeHistory.</summary>
    public static class HrChangeType
    {
        public const string Created = "CREATED";
        public const string Department = "DEPARTMENT";
        public const string Position = "POSITION";
        public const string Branch = "BRANCH";
        public const string Manager = "MANAGER";
        public const string Status = "STATUS";
        public const string Account = "ACCOUNT";
        public const string Contract = "CONTRACT";

        public static string Key(string? type) => "hr_change_" + (type ?? "").ToLowerInvariant();
    }
}
