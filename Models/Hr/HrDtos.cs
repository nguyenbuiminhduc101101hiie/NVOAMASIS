namespace NVOAMASIS.Models.Hr
{
    /// <summary>Dòng danh sách nhân viên (đã join tên phòng ban/chức vụ/quản lý/tài khoản).</summary>
    public sealed class HrEmployeeRow
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Phone { get; set; }
        public string? WorkEmail { get; set; }
        public string? IdCardNo { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public string? Branch { get; set; }
        public Guid? ManagerEmployeeId { get; set; }
        public string? ManagerName { get; set; }
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public int Status { get; set; }
        public DateTime? ResignDate { get; set; }
    }

    public sealed class HrEmployeeFilter
    {
        public string? Search { get; set; }
        public int? Status { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? Branch { get; set; }
        /// <summary>Mặc định ẩn nhân viên đã nghỉ việc.</summary>
        public bool IncludeResigned { get; set; }
    }

    public sealed class HrContractRow
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string ContractNo { get; set; } = string.Empty;
        public int ContractType { get; set; }
        public DateTime? SignDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        /// <summary>Null khi user không có quyền HR_Salary.</summary>
        public decimal? BaseSalary { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public decimal? Allowance { get; set; }
        public bool IsNetSalary { get; set; }
        public int Status { get; set; }
        public DateTime? TerminatedDate { get; set; }
        public string? Note { get; set; }

        /// <summary>Hiệu lực nhưng đã qua EndDate.</summary>
        public bool IsExpired => Status == HrContractStatus.Active && EndDate.HasValue && EndDate.Value.Date < DateTime.Today;

        public int? DaysToEnd => EndDate.HasValue ? (int)(EndDate.Value.Date - DateTime.Today).TotalDays : null;
    }

    public sealed class HrContractFilter
    {
        public string? Search { get; set; }
        public Guid? EmployeeId { get; set; }
        public int? ContractType { get; set; }
        /// <summary>null=tất cả, 1=hiệu lực, 2=đã chấm dứt, 3=đã hết hạn (hiệu lực nhưng quá EndDate)</summary>
        public int? StatusView { get; set; }
        /// <summary>Chỉ lấy HĐ hiệu lực hết hạn trong N ngày tới.</summary>
        public int? ExpiringWithinDays { get; set; }
    }

    public sealed class HrLookupItem
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Display => string.IsNullOrWhiteSpace(Code) ? Name : $"{Code} - {Name}";
        public override string ToString() => Display;
    }

    /// <summary>Tài khoản UserList chưa gắn với hồ sơ nhân viên.</summary>
    public sealed class HrUserLookup
    {
        public Guid UsrId { get; set; }
        public string? Usr { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Branch { get; set; }
        public string Display => string.IsNullOrWhiteSpace(Name) ? (Usr ?? "") : $"{Usr} - {Name}";
        public override string ToString() => Display;
    }

    public sealed class HrCountItem
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class HrAlertItem
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime Date { get; set; }
        public int Days { get; set; }
        public string? Extra { get; set; }
    }

    public sealed class HrDashboardData
    {
        public int Total { get; set; }
        public int Probation { get; set; }
        public int Active { get; set; }
        public int Suspended { get; set; }
        public int JoinedThisMonth { get; set; }
        public int ResignedThisMonth { get; set; }
        public int WithoutAccount { get; set; }
        public int WithoutContract { get; set; }
        public List<HrCountItem> ByDepartment { get; set; } = new();
        public List<HrCountItem> ByBranch { get; set; } = new();
        public List<HrAlertItem> ContractsExpiring { get; set; } = new();
        public List<HrAlertItem> ProbationEnding { get; set; } = new();
        public List<HrAlertItem> Birthdays { get; set; } = new();
    }

    /// <summary>Quyền của user trên 1 menu HR.</summary>
    public readonly record struct HrPerm(bool View, bool Add, bool Edit, bool Delete, bool Approve)
    {
        public static readonly HrPerm None = new(false, false, false, false, false);
    }

    public readonly record struct HrResult(bool Success, string MessageKey, Guid? Id = null, string? Detail = null)
    {
        public static HrResult Ok(string key, Guid? id = null) => new(true, key, id);
        public static HrResult Fail(string key, string? detail = null) => new(false, key, null, detail);
    }
}
