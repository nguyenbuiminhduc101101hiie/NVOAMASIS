using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models.Hr
{
    /// <summary>Kỳ lương theo tháng (bảng HrPayrollPeriod) — script Scripts/CreateHrPayrollTables.sql.</summary>
    public class HrPayrollPeriod
    {
        [Key]
        public Guid Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        [MaxLength(200)] public string Name { get; set; } = string.Empty;
        /// <summary>Xem <see cref="HrPayrollStatus"/></summary>
        public int Status { get; set; } = HrPayrollStatus.Draft;
        public decimal StandardDays { get; set; }
        public Guid? ParamId { get; set; }
        /// <summary>Chứng từ kế toán nháp tạo khi hạch toán (AccountingVouchers.Id).</summary>
        public Guid? VoucherId { get; set; }
        [MaxLength(50)] public string? VoucherNo { get; set; }
        [MaxLength(1000)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? CalculatedAt { get; set; }
        [MaxLength(100)] public string? CalculatedBy { get; set; }
        public DateTime? LockedAt { get; set; }
        [MaxLength(100)] public string? LockedBy { get; set; }
        public DateTime? PostedAt { get; set; }
        [MaxLength(100)] public string? PostedBy { get; set; }
    }

    /// <summary>Phiếu lương của 1 nhân viên trong 1 kỳ (bảng HrPayslip). Lưu ảnh chụp dữ liệu tại thời điểm tính.</summary>
    public class HrPayslip
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PeriodId { get; set; }
        public Guid EmployeeId { get; set; }
        [MaxLength(30)] public string EmployeeCode { get; set; } = string.Empty;
        [MaxLength(200)] public string FullName { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        [MaxLength(200)] public string? DepartmentName { get; set; }
        [MaxLength(10)] public string? Branch { get; set; }
        public Guid? UserId { get; set; }

        // Hợp đồng
        public Guid? ContractId { get; set; }
        [MaxLength(50)] public string? ContractNo { get; set; }
        public int? ContractType { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal? InsuranceSalary { get; set; }
        public decimal Allowance { get; set; }

        // Ngày công
        public decimal StandardDays { get; set; }
        /// <summary>Công hưởng lương lấy từ bảng công.</summary>
        public decimal TimesheetPaidDays { get; set; }
        /// <summary>Nhập tay (null = dùng bảng công).</summary>
        public decimal? PaidDaysOverride { get; set; }
        public decimal PaidDays { get; set; }
        /// <summary>Ngày không hưởng lương (không lương + vắng + ốm) — dùng cho quy tắc 14 ngày không đóng BHXH.</summary>
        public decimal NonPaidDays { get; set; }

        // Thu nhập
        public decimal SalaryByDays { get; set; }
        public decimal AllowanceAmount { get; set; }
        public decimal Overtime { get; set; }
        public decimal Bonus { get; set; }
        public decimal OtherIncome { get; set; }
        /// <summary>Thu nhập không chịu thuế (vd phần phụ cấp ăn ca được miễn).</summary>
        public decimal NonTaxableIncome { get; set; }
        public decimal GrossIncome { get; set; }

        // Bảo hiểm
        public decimal InsuranceBase { get; set; }
        public decimal EmpSocial { get; set; }
        public decimal EmpHealth { get; set; }
        public decimal EmpUnemployment { get; set; }
        public decimal CoSocial { get; set; }
        public decimal CoHealth { get; set; }
        public decimal CoUnemployment { get; set; }
        public decimal CoUnionFee { get; set; }

        // Thuế TNCN
        public int Dependents { get; set; }
        public decimal FamilyDeduction { get; set; }
        /// <summary>Xem <see cref="HrTaxMode"/></summary>
        public int TaxMode { get; set; } = HrTaxMode.Progressive;
        /// <summary>true = cách tính thuế do người dùng chọn, không tự đổi theo loại HĐ khi tính lại.</summary>
        public bool TaxModeManual { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal AssessableIncome { get; set; }
        public decimal PersonalIncomeTax { get; set; }

        // Khấu trừ & thực lĩnh
        public decimal Advance { get; set; }
        public decimal OtherDeduction { get; set; }
        public decimal NetPay { get; set; }

        [MaxLength(30)] public string? BankAccountNo { get; set; }
        [MaxLength(200)] public string? BankName { get; set; }
        /// <summary>Cảnh báo khi tính (khóa localization, phân cách bằng ;).</summary>
        [MaxLength(1000)] public string? Warnings { get; set; }
        [MaxLength(1000)] public string? Note { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }

        public decimal EmpInsurance => EmpSocial + EmpHealth + EmpUnemployment;
        public decimal CoInsurance => CoSocial + CoHealth + CoUnemployment;
        public decimal TotalDeduction => EmpInsurance + PersonalIncomeTax + Advance + OtherDeduction;
        /// <summary>Tổng chi phí DN = thu nhập + BH/KPCĐ phần DN.</summary>
        public decimal EmployerCost => GrossIncome + CoInsurance + CoUnionFee;
    }

    /// <summary>
    /// Tham số pháp lý theo thời điểm hiệu lực (bảng HrPayrollParam). Kỳ lương dùng dòng có
    /// EffectiveFrom lớn nhất nhưng không sau ngày 1 của tháng lương.
    /// </summary>
    public class HrPayrollParam
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime EffectiveFrom { get; set; }
        /// <summary>Mức tham chiếu (= lương cơ sở khi chưa bãi bỏ) — trần BHXH/BHYT = CapMultiplier × mức này.</summary>
        public decimal ReferenceWage { get; set; }
        public decimal MinWageRegion1 { get; set; }
        public decimal MinWageRegion2 { get; set; }
        public decimal MinWageRegion3 { get; set; }
        public decimal MinWageRegion4 { get; set; }
        public decimal CapMultiplier { get; set; } = 20;

        // Tỷ lệ % người lao động
        public decimal EmpSocialRate { get; set; } = 8;
        public decimal EmpHealthRate { get; set; } = 1.5m;
        public decimal EmpUnemploymentRate { get; set; } = 1;
        // Tỷ lệ % doanh nghiệp
        public decimal CoSocialRate { get; set; } = 17.5m;
        public decimal CoHealthRate { get; set; } = 3;
        public decimal CoUnemploymentRate { get; set; } = 1;
        public decimal CoUnionFeeRate { get; set; } = 2;

        // Thuế TNCN
        public decimal SelfDeduction { get; set; }
        public decimal DependentDeduction { get; set; }
        /// <summary>"ngưỡng:thuế suất;..." theo tháng, ngưỡng 0 = không giới hạn. Vd "10000000:5;30000000:10;...;0:35".</summary>
        [MaxLength(500)] public string TaxBrackets { get; set; } = string.Empty;
        public decimal FlatTaxRate { get; set; } = 10;
        /// <summary>Khấu trừ 10% khi tổng thu nhập mỗi lần chi trả từ mức này trở lên.</summary>
        public decimal FlatTaxThreshold { get; set; } = 2_000_000;

        [MaxLength(1000)] public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }

        public decimal MinWage(int region) => region switch
        {
            2 => MinWageRegion2,
            3 => MinWageRegion3,
            4 => MinWageRegion4,
            _ => MinWageRegion1
        };

        /// <summary>Parse biểu thuế: danh sách (ngưỡng trên, thuế suất %), ngưỡng null = vô hạn.</summary>
        public List<(decimal? Upper, decimal Rate)> ParseBrackets()
        {
            var list = new List<(decimal? Upper, decimal Rate)>();
            foreach (var part in (TaxBrackets ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var kv = part.Split(':');
                if (kv.Length != 2) continue;
                if (!decimal.TryParse(kv[0], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var upper)) continue;
                if (!decimal.TryParse(kv[1], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var rate)) continue;
                list.Add((upper <= 0 ? null : upper, rate));
            }
            return list.OrderBy(x => x.Upper ?? decimal.MaxValue).ToList();
        }
    }

    /// <summary>1=Nháp (tính lại được), 2=Đã chốt, 3=Đã hạch toán.</summary>
    public static class HrPayrollStatus
    {
        public const int Draft = 1;
        public const int Locked = 2;
        public const int Posted = 3;

        public static string Key(int s) => s switch
        {
            Draft => "hr_pay_status_draft",
            Locked => "hr_pay_status_locked",
            Posted => "hr_pay_status_posted",
            _ => "hr_unknown"
        };
    }

    /// <summary>1=Lũy tiến (HĐLĐ từ 3 tháng), 2=Khấu trừ 10% (thử việc/dịch vụ/không HĐLĐ), 3=Không khấu trừ.</summary>
    public static class HrTaxMode
    {
        public const int Progressive = 1;
        public const int Flat = 2;
        public const int None = 3;

        public static readonly int[] All = { Progressive, Flat, None };

        public static string Key(int m) => m switch
        {
            Progressive => "hr_tax_progressive",
            Flat => "hr_tax_flat",
            None => "hr_tax_none",
            _ => "hr_unknown"
        };
    }

    /// <summary>Cấu hình tính lương / hạch toán (lưu trong HrSetting).</summary>
    public sealed class HrPayrollConfig
    {
        /// <summary>Vùng lương tối thiểu nơi công ty hoạt động (1–4).</summary>
        public int Region { get; set; } = 1;
        public bool UnionFeeEnabled { get; set; } = true;
        /// <summary>Không đóng BHXH khi số ngày không hưởng lương trong tháng ≥ N (mặc định 14).</summary>
        public decimal NoInsuranceUnpaidDays { get; set; } = 14;

        public string TransactionTypeCode { get; set; } = string.Empty;
        public string ExpenseAccount { get; set; } = "642";
        public string PayableAccount { get; set; } = "334";
        public string SocialAccount { get; set; } = "3383";
        public string HealthAccount { get; set; } = "3384";
        public string UnemploymentAccount { get; set; } = "3386";
        public string UnionFeeAccount { get; set; } = "3382";
        public string PitAccount { get; set; } = "3335";
        public string AdvanceAccount { get; set; } = "141";
        public string OtherDeductionAccount { get; set; } = "1388";
    }

    public static class HrPayrollSettingKeys
    {
        public const string Region = "PayrollRegion";
        public const string UnionFeeEnabled = "PayrollUnionFeeEnabled";
        public const string NoInsuranceUnpaidDays = "PayrollNoInsuranceUnpaidDays";
        public const string TransactionTypeCode = "PayrollTransactionTypeCode";
        public const string ExpenseAccount = "PayrollAccExpense";
        public const string PayableAccount = "PayrollAccPayable";
        public const string SocialAccount = "PayrollAccSocial";
        public const string HealthAccount = "PayrollAccHealth";
        public const string UnemploymentAccount = "PayrollAccUnemployment";
        public const string UnionFeeAccount = "PayrollAccUnionFee";
        public const string PitAccount = "PayrollAccPit";
        public const string AdvanceAccount = "PayrollAccAdvance";
        public const string OtherDeductionAccount = "PayrollAccOtherDeduction";
    }

    /// <summary>Dữ liệu nhập tay trên phiếu lương.</summary>
    public sealed class HrPayslipInput
    {
        public Guid Id { get; set; }
        public decimal? PaidDaysOverride { get; set; }
        public decimal Overtime { get; set; }
        public decimal Bonus { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal NonTaxableIncome { get; set; }
        public decimal Advance { get; set; }
        public decimal OtherDeduction { get; set; }
        public int TaxMode { get; set; } = HrTaxMode.Progressive;
        public string? Note { get; set; }
    }

    /// <summary>1 dòng bút toán xem trước khi hạch toán.</summary>
    public sealed class HrPostingLine
    {
        public string AccountCode { get; set; } = string.Empty;
        public Guid? AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
    }
}
