using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Tính 1 phiếu lương (hàm thuần, không truy cập DB — kiểm thử được).
    /// Phiếu có thể gồm 2 phần khi trong tháng đổi hợp đồng (vd thử việc đến 14/11, chính thức từ 15/11):
    ///   phần HĐ trước (Pro*) và phần HĐ chính (hợp đồng mới nhất). Cùng 1 công chuẩn cho cả 2 phần.
    /// Quy trình:
    ///  1. Lương theo công mỗi phần = Lương cơ bản của HĐ đó × công hưởng lương trong thời gian HĐ đó / công chuẩn; phụ cấp tương tự.
    ///  2. Bảo hiểm: theo HĐ chính (chỉ HĐLĐ xác định / không xác định thời hạn), đóng cả tháng trên lương đóng BH của HĐ chính;
    ///     không đóng nếu (ngày không hưởng lương + ngày còn ở HĐ trước không đóng BH) ≥ ngưỡng (mặc định 14).
    ///  3. Thuế TNCN tách theo cách tính của từng phần:
    ///     - Lũy tiến: (thu nhập các phần lũy tiến − BH NLĐ − giảm trừ gia cảnh đủ tháng) theo biểu thuế;
    ///     - Khấu trừ 10%: trên thu nhập các phần thử việc / HĐ dịch vụ nếu từ ngưỡng 2 triệu.
    ///     Người dùng chọn tay cách tính thuế trên phiếu → áp dụng cho cả tháng (vd thử việc nằm trong HĐLĐ từ 3 tháng).
    ///  4. Lương NET: số trên HĐ (và các khoản thưởng, làm thêm, thu nhập khác khi HĐ chính là NET) là số thực nhận;
    ///     phần mềm tìm số "gross-up" (công ty chịu thay BH NLĐ + thuế) sao cho sau khi trừ BH và thuế, NLĐ nhận đúng số đó.
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
        public const string WarnNoInsuranceProbation = "hr_pay_warn_no_insurance_probation";
        public const string WarnManyContracts = "hr_pay_warn_many_contracts";
        public const string WarnNetNoInsuranceSalary = "hr_pay_warn_net_no_insurance_salary";

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

        /// <summary>
        /// Chia công hưởng lương của cả tháng cho 2 phần: HĐ trước lấy đúng số công thực tế trong thời gian của nó
        /// (tối đa bằng tổng), HĐ chính lấy phần còn lại — tổng 2 phần luôn bằng công hưởng lương cả tháng.
        /// </summary>
        public static (decimal Pro, decimal Main) SplitPaidDays(decimal totalPaid, decimal proPaidByCalendar) =>
            (Math.Clamp(proPaidByCalendar, 0, Math.Max(0, totalPaid)), Math.Max(0, totalPaid - Math.Clamp(proPaidByCalendar, 0, Math.Max(0, totalPaid))));

        /// <summary>Có tham gia BH bắt buộc theo loại hợp đồng hay không.</summary>
        public static bool InsuredContract(int? contractType) =>
            contractType is HrContractType.FixedTerm or HrContractType.Indefinite;

        public static void Compute(HrPayslip p, HrPayrollParam prm, HrPayrollConfig cfg, ICollection<string>? extraWarnings = null)
        {
            var warnings = new List<string>();
            if (extraWarnings != null) warnings.AddRange(extraWarnings);
            if (p.ContractId is null) warnings.Add(WarnNoContract);

            // 1. Ngày công & lương theo công từng phần
            p.PaidDays = Math.Max(0, p.PaidDaysOverride ?? p.TimesheetPaidDays);
            p.ProPaidDays = p.HasPro ? Math.Max(0, p.ProPaidDaysOverride ?? p.ProTimesheetPaidDays) : 0;
            var totalPaid = p.PaidDays + p.ProPaidDays;
            decimal Ratio(decimal days) => p.StandardDays > 0 ? Math.Min(1m, days / p.StandardDays) : 0m;
            // Tổng 2 phần không vượt quá 1 tháng lương
            var scale = p.StandardDays > 0 && totalPaid > p.StandardDays ? p.StandardDays / totalPaid : 1m;
            p.SalaryByDays = R(p.BaseSalary * Ratio(p.PaidDays * scale));
            p.AllowanceAmount = R(p.Allowance * Ratio(p.PaidDays * scale));
            if (p.HasPro)
            {
                p.ProSalaryByDays = R(p.ProBaseSalary * Ratio(p.ProPaidDays * scale));
                p.ProAllowanceAmount = R(p.ProAllowance * Ratio(p.ProPaidDays * scale));
            }
            else
            {
                p.ProSalaryByDays = p.ProAllowanceAmount = 0;
                p.ProCalendarDays = 0;
            }

            // 2. Bảo hiểm (theo HĐ chính)
            p.EmpSocial = p.EmpHealth = p.EmpUnemployment = 0;
            p.CoSocial = p.CoHealth = p.CoUnemployment = p.CoUnionFee = 0;
            p.InsuranceBase = 0;
            if (InsuredContract(p.ContractType))
            {
                var baseSalary = p.InsuranceSalary is > 0 ? p.InsuranceSalary.Value : p.BaseSalary;
                if (p.IsNet && p.InsuranceSalary is not > 0) warnings.Add(WarnNetNoInsuranceSalary);
                var minWage = prm.MinWage(cfg.Region);
                if (baseSalary > 0 && minWage > 0 && baseSalary < minWage) warnings.Add(WarnBelowMinWage);

                // Ngày còn ở HĐ trước không thuộc diện đóng BH (thử việc / dịch vụ) tính như ngày không tham gia BH.
                var notInsuredDays = p.HasPro && !InsuredContract(p.ProContractType) ? p.ProCalendarDays : 0;
                if (p.NonPaidDays >= cfg.NoInsuranceUnpaidDays)
                {
                    warnings.Add(WarnNoInsuranceDays);
                }
                else if (p.NonPaidDays + notInsuredDays >= cfg.NoInsuranceUnpaidDays)
                {
                    warnings.Add(WarnNoInsuranceProbation);
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

            // 3. Thuế TNCN theo nhóm cách tính + quy đổi NET → GROSS
            var mainMode = p.TaxMode;
            var proMode = p.HasPro ? (p.TaxModeManual ? p.TaxMode : DefaultTaxMode(p.ProContractType)) : mainMode;
            p.ProTaxMode = p.HasPro ? proMode : 0;
            var mainIncome = p.SalaryByDays + p.AllowanceAmount + p.Overtime + p.Bonus + p.OtherIncome; // chịu thuế
            var proIncome = p.ProSalaryByDays + p.ProAllowanceAmount;
            var bh = p.EmpInsurance;
            var family = prm.SelfDeduction + Math.Max(0, p.Dependents) * prm.DependentDeduction;
            var brackets = prm.ParseBrackets();

            // Thuế của 1 nhóm (cùng cách tính), có / không trừ BH của HĐ chính.
            decimal GroupTax(int mode, decimal income, bool withBh) => mode switch
            {
                HrTaxMode.Progressive => R(ProgressiveTax(Math.Max(0, income - (withBh ? bh : 0) - family), brackets)),
                HrTaxMode.Flat => income >= prm.FlatTaxThreshold ? R(income * prm.FlatTaxRate / 100m) : 0,
                _ => 0
            };

            // Nhóm theo cách tính thuế: HĐ chính (kèm BH) và HĐ trước; cùng cách tính thì gộp 1 nhóm.
            var groups = new List<(int Mode, bool HasMain, bool HasPro)>();
            if (!p.HasPro || proMode == mainMode) groups.Add((mainMode, true, p.HasPro));
            else { groups.Add((mainMode, true, false)); groups.Add((proMode, false, true)); }

            p.GrossUp = p.ProGrossUp = 0;
            decimal totalTax = 0, proTax = 0;
            p.FamilyDeduction = 0;
            decimal progAssess = 0, flatBase = 0;
            var hasProgressive = false;
            foreach (var g in groups)
            {
                var mainPart = g.HasMain ? mainIncome : 0;
                var proPart = g.HasPro ? proIncome : 0;
                var income = mainPart + proPart;
                var gBh = g.HasMain ? bh : 0;
                var mainNet = g.HasMain && p.IsNet;
                var proNet = g.HasPro && p.ProIsNet;

                decimal up = 0;
                if (mainNet || proNet)
                {
                    // Số NLĐ phải nhận: phần NET nhận đủ; phần GROSS (nếu có trong cùng nhóm) vẫn tự chịu BH.
                    var target = income - (mainNet ? 0 : gBh);
                    decimal Take(decimal u) => income + u - gBh - GroupTax(g.Mode, income + u, g.HasMain);
                    if (Take(0) < target)
                    {
                        decimal lo = 0, hi = Math.Max(1m, (income + gBh) * 2 + 1_000_000m);
                        while (Take(hi) < target) hi *= 2;
                        while (hi - lo > 1)
                        {
                            var mid = Math.Floor((lo + hi) / 2);
                            if (Take(mid) >= target) hi = mid; else lo = mid;
                        }
                        up = hi;
                    }
                }

                var tax = GroupTax(g.Mode, income + up, g.HasMain);
                totalTax += tax;
                if (g.Mode == HrTaxMode.Progressive)
                {
                    hasProgressive = true;
                    p.FamilyDeduction = family;
                    progAssess += Math.Max(0, income + up - gBh - family);
                }
                else if (g.Mode == HrTaxMode.Flat)
                {
                    flatBase += income + up;
                }

                // Chia gross-up và thuế giữa 2 phần
                var proShare = income > 0 ? proPart / income : 0;
                if (mainNet && proNet)
                {
                    var upPro = R(up * proShare);
                    p.ProGrossUp += upPro;
                    p.GrossUp += up - upPro;
                }
                else if (mainNet) p.GrossUp += up;
                else if (proNet) p.ProGrossUp += up;
                proTax += g.HasMain ? R(tax * proShare) : tax;
            }
            // Thu nhập tính thuế hiển thị: phần lũy tiến nếu có, không thì phần khấu trừ 10%.
            p.AssessableIncome = hasProgressive ? progAssess : flatBase;
            p.ProTax = p.HasPro ? proTax : 0;
            p.PersonalIncomeTax = totalTax;

            // 4. Thu nhập
            p.GrossIncome = (p.HasPro ? proIncome : 0) + mainIncome + p.NonTaxableIncome + p.GrossUp + p.ProGrossUp;
            p.TaxableIncome = Math.Max(0, p.GrossIncome - p.NonTaxableIncome);

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
