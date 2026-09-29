using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Tính 1 phiếu lương (hàm thuần, không truy cập DB — kiểm thử được).
    /// Quy trình:
    ///  1. Lương theo công = Lương cơ bản × Công hưởng lương / Công chuẩn (tối đa 1); phụ cấp tính tương tự.
    ///  2. Tổng thu nhập = lương theo công + phụ cấp + làm thêm + thưởng + thu nhập khác + thu nhập không chịu thuế.
    ///  3. Bảo hiểm (chỉ HĐLĐ xác định / không xác định thời hạn, và số ngày không hưởng lương &lt; 14):
    ///     BHXH, BHYT tính trên min(lương đóng BH, trần × mức tham chiếu); BHTN trên min(lương đóng BH, trần × lương tối thiểu vùng).
    ///  4. Thuế TNCN: lũy tiến từng phần trên (thu nhập chịu thuế − BH NLĐ − giảm trừ gia cảnh);
    ///     hoặc khấu trừ 10% trên thu nhập chịu thuế nếu từ ngưỡng 2 triệu (thử việc / HĐ dịch vụ / không HĐLĐ).
    ///  5. Thực lĩnh = tổng thu nhập − BH NLĐ − thuế − tạm ứng − khấu trừ khác.
    /// Mọi số tiền làm tròn đến đồng.
    /// </summary>
    public static class HrPayrollCalculator
    {
        public const string WarnNoContract = "hr_pay_warn_no_contract";
        public const string WarnNoTimesheet = "hr_pay_warn_no_timesheet";
        public const string WarnBelowMinWage = "hr_pay_warn_below_min_wage";
        public const string WarnNoInsuranceDays = "hr_pay_warn_no_insurance_days";
        public const string WarnNegativeNet = "hr_pay_warn_negative_net";
        public const string WarnMonthNotFinished = "hr_pay_warn_month_not_finished";

        /// <summary>Cách tính thuế mặc định theo loại hợp đồng.</summary>
        public static int DefaultTaxMode(int? contractType) => contractType switch
        {
            HrContractType.FixedTerm or HrContractType.Indefinite => HrTaxMode.Progressive,
            _ => HrTaxMode.Flat
        };

        /// <summary>
        /// Quy đổi công từ bảng công sang (công chuẩn, công hưởng lương) dùng cho phiếu lương.
        /// - fixedStandard = 0: công chuẩn = lịch thực tế của tháng, công hưởng lương = bảng công.
        /// - fixedStandard &gt; 0 (vd 24):
        ///   + Làm trọn tháng: công hưởng lương = chuẩn cố định − số công không hưởng lương
        ///     → đi làm đủ thì đủ lương dù tháng có 22 hay 27 ngày làm việc; mỗi ngày thiếu trừ Lương / chuẩn cố định.
        ///   + Vào làm / nghỉ việc giữa tháng: công hưởng lương = công thực tế (tối đa bằng chuẩn cố định).
        /// </summary>
        /// <param name="calendarStandard">Công chuẩn cả tháng theo lịch.</param>
        /// <param name="employeeStandard">Công chuẩn theo lịch trong thời gian người đó làm việc (row.Standard).</param>
        /// <param name="paid">Công hưởng lương theo bảng công.</param>
        public static (decimal Standard, decimal Paid) ResolveDays(decimal fixedStandard, decimal calendarStandard,
            decimal employeeStandard, decimal paid)
        {
            if (fixedStandard <= 0) return (calendarStandard, paid);
            var fullMonth = employeeStandard >= calendarStandard;
            var result = fullMonth
                ? fixedStandard - Math.Max(0, calendarStandard - paid)
                : paid;
            return (fixedStandard, Math.Clamp(result, 0, fixedStandard));
        }

        /// <summary>Có tham gia BH bắt buộc theo loại hợp đồng hay không.</summary>
        public static bool InsuredContract(int? contractType) =>
            contractType is HrContractType.FixedTerm or HrContractType.Indefinite;

        public static void Compute(HrPayslip p, HrPayrollParam prm, HrPayrollConfig cfg, ICollection<string>? extraWarnings = null)
        {
            var warnings = new List<string>();
            if (extraWarnings != null) warnings.AddRange(extraWarnings);
            if (p.ContractId is null) warnings.Add(WarnNoContract);

            // 1. Ngày công
            p.PaidDays = Math.Max(0, p.PaidDaysOverride ?? p.TimesheetPaidDays);
            var ratio = p.StandardDays > 0 ? Math.Min(1m, p.PaidDays / p.StandardDays) : 0m;
            p.SalaryByDays = R(p.BaseSalary * ratio);
            p.AllowanceAmount = R(p.Allowance * ratio);

            // 2. Thu nhập
            p.GrossIncome = p.SalaryByDays + p.AllowanceAmount + p.Overtime + p.Bonus + p.OtherIncome + p.NonTaxableIncome;

            // 3. Bảo hiểm
            p.EmpSocial = p.EmpHealth = p.EmpUnemployment = 0;
            p.CoSocial = p.CoHealth = p.CoUnemployment = p.CoUnionFee = 0;
            p.InsuranceBase = 0;
            if (InsuredContract(p.ContractType))
            {
                var baseSalary = p.InsuranceSalary is > 0 ? p.InsuranceSalary.Value : p.BaseSalary;
                var minWage = prm.MinWage(cfg.Region);
                if (baseSalary > 0 && minWage > 0 && baseSalary < minWage) warnings.Add(WarnBelowMinWage);

                if (p.NonPaidDays >= cfg.NoInsuranceUnpaidDays)
                {
                    warnings.Add(WarnNoInsuranceDays);
                }
                else if (baseSalary > 0)
                {
                    var socialBase = Cap(baseSalary, prm.CapMultiplier * prm.ReferenceWage);
                    var unempBase = Cap(baseSalary, prm.CapMultiplier * minWage);
                    p.InsuranceBase = socialBase;
                    p.EmpSocial = R(socialBase * prm.EmpSocialRate / 100m);
                    p.EmpHealth = R(socialBase * prm.EmpHealthRate / 100m);
                    p.EmpUnemployment = R(unempBase * prm.EmpUnemploymentRate / 100m);
                    p.CoSocial = R(socialBase * prm.CoSocialRate / 100m);
                    p.CoHealth = R(socialBase * prm.CoHealthRate / 100m);
                    p.CoUnemployment = R(unempBase * prm.CoUnemploymentRate / 100m);
                    if (cfg.UnionFeeEnabled) p.CoUnionFee = R(socialBase * prm.CoUnionFeeRate / 100m);
                }
            }

            // 4. Thuế TNCN
            p.TaxableIncome = Math.Max(0, p.GrossIncome - p.NonTaxableIncome);
            p.FamilyDeduction = 0;
            p.AssessableIncome = 0;
            p.PersonalIncomeTax = 0;
            switch (p.TaxMode)
            {
                case HrTaxMode.Progressive:
                    p.FamilyDeduction = prm.SelfDeduction + Math.Max(0, p.Dependents) * prm.DependentDeduction;
                    p.AssessableIncome = Math.Max(0, p.TaxableIncome - p.EmpInsurance - p.FamilyDeduction);
                    p.PersonalIncomeTax = R(ProgressiveTax(p.AssessableIncome, prm.ParseBrackets()));
                    break;
                case HrTaxMode.Flat:
                    p.AssessableIncome = p.TaxableIncome;
                    if (p.TaxableIncome >= prm.FlatTaxThreshold)
                        p.PersonalIncomeTax = R(p.TaxableIncome * prm.FlatTaxRate / 100m);
                    break;
            }

            // 5. Thực lĩnh
            p.NetPay = p.GrossIncome - p.EmpInsurance - p.PersonalIncomeTax - p.Advance - p.OtherDeduction;
            if (p.NetPay < 0) warnings.Add(WarnNegativeNet);

            p.Warnings = warnings.Count == 0 ? null : string.Join(";", warnings.Distinct());
        }

        /// <summary>Thuế lũy tiến từng phần.</summary>
        public static decimal ProgressiveTax(decimal assessable, IReadOnlyList<(decimal? Upper, decimal Rate)> brackets)
        {
            if (assessable <= 0 || brackets.Count == 0) return 0;
            decimal tax = 0, lower = 0;
            foreach (var (upper, rate) in brackets)
            {
                var top = upper ?? decimal.MaxValue;
                if (assessable <= lower) break;
                var portion = Math.Min(assessable, top) - lower;
                if (portion > 0) tax += portion * rate / 100m;
                if (upper is null) break;
                lower = top;
            }
            return tax;
        }

        private static decimal Cap(decimal value, decimal cap) => cap > 0 ? Math.Min(value, cap) : value;

        private static decimal R(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);
    }
}
