using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Bảng lương tháng: tạo kỳ, tính từ hợp đồng + bảng công + tham số pháp lý, nhập các khoản tay,
    /// chốt, hạch toán sang kế toán (tạo chứng từ NHÁP để kế toán kiểm tra và ghi sổ).
    /// </summary>
    public sealed class HrPayrollService(IDbContextFactory<AppDbContext> dbFactory, HrTimesheetService timesheet)
    {
        public const string SourceModule = "HR_PAYROLL";

        // ═════════════════════════ Tham số pháp lý ═════════════════════════

        public async Task<List<HrPayrollParam>> GetParamsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPayrollParams.AsNoTracking().OrderByDescending(x => x.EffectiveFrom).ToListAsync();
        }

        private static async Task<HrPayrollParam?> ParamForAsync(AppDbContext db, DateTime date) =>
            await db.HrPayrollParams.AsNoTracking()
                .Where(x => x.EffectiveFrom <= date)
                .OrderByDescending(x => x.EffectiveFrom)
                .FirstOrDefaultAsync();

        public async Task<HrResult> SaveParamAsync(HrPayrollParam m, string actor)
        {
            if (m.ReferenceWage <= 0 || m.MinWageRegion1 <= 0) return HrResult.Fail("hr_pay_err_param_wage");
            if (m.ParseBrackets().Count == 0) return HrResult.Fail("hr_pay_err_param_brackets");
            m.EffectiveFrom = m.EffectiveFrom.Date;
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (await db.HrPayrollParams.AnyAsync(x => x.EffectiveFrom == m.EffectiveFrom && x.Id != m.Id))
                    return HrResult.Fail("hr_pay_err_param_exists");
                if (m.Id == Guid.Empty)
                {
                    m.Id = Guid.NewGuid();
                    m.CreatedAt = DateTime.Now;
                    m.CreatedBy = actor;
                    db.HrPayrollParams.Add(m);
                }
                else
                {
                    var e = await db.HrPayrollParams.FirstOrDefaultAsync(x => x.Id == m.Id);
                    if (e is null) return HrResult.Fail("hr_err_not_found");
                    var created = (e.CreatedAt, e.CreatedBy);
                    db.Entry(e).CurrentValues.SetValues(m);
                    (e.CreatedAt, e.CreatedBy) = created;
                }
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", m.Id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteParamAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (await db.HrPayrollPeriods.AnyAsync(x => x.ParamId == id))
                    return HrResult.Fail("hr_pay_err_param_in_use");
                var e = await db.HrPayrollParams.FirstOrDefaultAsync(x => x.Id == id);
                if (e is null) return HrResult.Fail("hr_err_not_found");
                db.HrPayrollParams.Remove(e);
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        // ═════════════════════════ Cấu hình (HrSetting) ═════════════════════════

        public async Task<HrPayrollConfig> GetConfigAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await GetConfigAsync(db);
        }

        private static async Task<HrPayrollConfig> GetConfigAsync(AppDbContext db)
        {
            var c = new HrPayrollConfig();
            Dictionary<string, string?> m;
            try { m = await db.HrSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value); }
            catch { return c; }

            string S(string k, string def) => m.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : def;
            if (int.TryParse(S(HrPayrollSettingKeys.Region, "1"), out var r) && r is >= 1 and <= 4) c.Region = r;
            if (bool.TryParse(S(HrPayrollSettingKeys.UnionFeeEnabled, "true"), out var u)) c.UnionFeeEnabled = u;
            if (decimal.TryParse(S(HrPayrollSettingKeys.NoInsuranceUnpaidDays, "14"), NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) c.NoInsuranceUnpaidDays = d;
            c.TransactionTypeCode = S(HrPayrollSettingKeys.TransactionTypeCode, "");
            c.ExpenseAccount = S(HrPayrollSettingKeys.ExpenseAccount, c.ExpenseAccount);
            c.PayableAccount = S(HrPayrollSettingKeys.PayableAccount, c.PayableAccount);
            c.SocialAccount = S(HrPayrollSettingKeys.SocialAccount, c.SocialAccount);
            c.HealthAccount = S(HrPayrollSettingKeys.HealthAccount, c.HealthAccount);
            c.UnemploymentAccount = S(HrPayrollSettingKeys.UnemploymentAccount, c.UnemploymentAccount);
            c.UnionFeeAccount = S(HrPayrollSettingKeys.UnionFeeAccount, c.UnionFeeAccount);
            c.PitAccount = S(HrPayrollSettingKeys.PitAccount, c.PitAccount);
            c.AdvanceAccount = S(HrPayrollSettingKeys.AdvanceAccount, c.AdvanceAccount);
            c.OtherDeductionAccount = S(HrPayrollSettingKeys.OtherDeductionAccount, c.OtherDeductionAccount);
            return c;
        }

        public async Task<HrResult> SaveConfigAsync(HrPayrollConfig c, string actor)
        {
            if (c.Region is < 1 or > 4) return HrResult.Fail("hr_pay_err_region");
            var values = new Dictionary<string, string>
            {
                [HrPayrollSettingKeys.Region] = c.Region.ToString(CultureInfo.InvariantCulture),
                [HrPayrollSettingKeys.UnionFeeEnabled] = c.UnionFeeEnabled.ToString().ToLowerInvariant(),
                [HrPayrollSettingKeys.NoInsuranceUnpaidDays] = c.NoInsuranceUnpaidDays.ToString(CultureInfo.InvariantCulture),
                [HrPayrollSettingKeys.TransactionTypeCode] = c.TransactionTypeCode?.Trim() ?? "",
                [HrPayrollSettingKeys.ExpenseAccount] = c.ExpenseAccount.Trim(),
                [HrPayrollSettingKeys.PayableAccount] = c.PayableAccount.Trim(),
                [HrPayrollSettingKeys.SocialAccount] = c.SocialAccount.Trim(),
                [HrPayrollSettingKeys.HealthAccount] = c.HealthAccount.Trim(),
                [HrPayrollSettingKeys.UnemploymentAccount] = c.UnemploymentAccount.Trim(),
                [HrPayrollSettingKeys.UnionFeeAccount] = c.UnionFeeAccount.Trim(),
                [HrPayrollSettingKeys.PitAccount] = c.PitAccount.Trim(),
                [HrPayrollSettingKeys.AdvanceAccount] = c.AdvanceAccount.Trim(),
                [HrPayrollSettingKeys.OtherDeductionAccount] = c.OtherDeductionAccount.Trim()
            };
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var existing = await db.HrSettings.Where(x => values.Keys.Contains(x.Key)).ToDictionaryAsync(x => x.Key);
                var now = DateTime.Now;
                foreach (var (k, v) in values)
                {
                    if (existing.TryGetValue(k, out var row)) { row.Value = v; row.UpdatedAt = now; row.UpdatedBy = actor; }
                    else db.HrSettings.Add(new HrSetting { Key = k, Value = v, UpdatedAt = now, UpdatedBy = actor });
                }
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<List<M_TransactionTypes>> GetTransactionTypesAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.TransactionTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<List<HrLookupItem>> GetCompaniesAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Customer.AsNoTracking()
                .Where(x => x.Customer_ID != Guid.Empty)
                .OrderBy(x => x.COMPANY)
                .Select(x => new HrLookupItem { Id = x.Customer_ID, Code = "", Name = x.COMPANY ?? x.BIZName ?? "" })
                .ToListAsync();
        }

        // ═════════════════════════ Kỳ lương ═════════════════════════

        public async Task<List<HrPayrollPeriod>> GetPeriodsAsync(int? year)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var q = db.HrPayrollPeriods.AsNoTracking().AsQueryable();
            if (year.HasValue) q = q.Where(x => x.Year == year.Value);
            return await q.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync();
        }

        public async Task<HrPayrollPeriod?> GetPeriodAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<HrResult> CreatePeriodAsync(int year, int month, string actor)
        {
            if (month is < 1 or > 12) return HrResult.Fail("hr_err_form_invalid");
            try
            {
                Guid id;
                await using (var db = await dbFactory.CreateDbContextAsync())
                {
                    if (await db.HrPayrollPeriods.AnyAsync(x => x.Year == year && x.Month == month))
                        return HrResult.Fail("hr_pay_err_period_exists");
                    if (await ParamForAsync(db, new DateTime(year, month, 1)) is null)
                        return HrResult.Fail("hr_pay_err_no_param");
                    var p = new HrPayrollPeriod
                    {
                        Id = Guid.NewGuid(), Year = year, Month = month,
                        Name = $"Lương tháng {month:D2}/{year}",
                        Status = HrPayrollStatus.Draft, CreatedAt = DateTime.Now, CreatedBy = actor
                    };
                    db.HrPayrollPeriods.Add(p);
                    await db.SaveChangesAsync();
                    id = p.Id;
                }
                var calc = await CalculateAsync(id, actor);
                return calc.Success ? HrResult.Ok("hr_pay_period_created", id) : calc;
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Tính (lại) toàn bộ kỳ: lấy hợp đồng, bảng công, tham số mới nhất; GIỮ các khoản nhập tay
        /// (làm thêm, thưởng, thu nhập khác, không chịu thuế, tạm ứng, khấu trừ khác, công nhập tay, ghi chú, cách tính thuế đã chọn).
        /// Thêm phiếu cho nhân viên mới; phiếu của người không còn trong kỳ bị xóa.
        /// </summary>
        public async Task<HrResult> CalculateAsync(Guid periodId, string actor)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var period = await db.HrPayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
                if (period is null) return HrResult.Fail("hr_err_not_found");
                if (period.Status != HrPayrollStatus.Draft) return HrResult.Fail("hr_pay_err_not_draft");

                var monthStart = new DateTime(period.Year, period.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var prm = await ParamForAsync(db, monthStart);
                if (prm is null) return HrResult.Fail("hr_pay_err_no_param");
                var cfg = await GetConfigAsync(db);
                var work = await HrSettingsService.GetAsync(db);
                var standardDays = work.FixedStandardDays > 0 ? work.FixedStandardDays : 0m;

                var sheet = await timesheet.BuildAsync(new HrTimesheetFilter { Year = period.Year, Month = period.Month });
                var rows = sheet.Rows.ToDictionary(r => r.EmployeeId);
                var empIds = rows.Keys.ToList();

                var emps = await db.HrEmployees.AsNoTracking()
                    .Where(e => empIds.Contains(e.Id))
                    .ToDictionaryAsync(e => e.Id);
                var contracts = (await db.HrContracts.AsNoTracking()
                        .Where(c => empIds.Contains(c.EmployeeId)
                                    && c.StartDate <= monthEnd
                                    && (c.EndDate == null || c.EndDate >= monthStart)
                                    && (c.Status == HrContractStatus.Active
                                        || (c.TerminatedDate != null && c.TerminatedDate >= monthStart)))
                        .ToListAsync())
                    .GroupBy(c => c.EmployeeId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.StartDate).ThenByDescending(c => c.CreatedAt).ToList());
                // Bảng công theo từng khoảng ngày của HĐ trước (thử việc) — tính 1 lần cho mỗi khoảng.
                var rangeSheets = new Dictionary<(DateTime, DateTime), Dictionary<Guid, HrTimesheetRow>>();
                async Task<HrTimesheetRow?> RangeRowAsync(Guid employeeId, DateTime from, DateTime to)
                {
                    if (!rangeSheets.TryGetValue((from, to), out var map))
                    {
                        var part = await timesheet.BuildRangeAsync(from, to, new HrTimesheetFilter { Year = period.Year, Month = period.Month });
                        rangeSheets[(from, to)] = map = part.Rows.ToDictionary(r => r.EmployeeId);
                    }
                    return map.TryGetValue(employeeId, out var r) ? r : null;
                }

                var existing = await db.HrPayslips.Where(x => x.PeriodId == periodId).ToListAsync();
                var byEmp = existing.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => g.First());
                var monthNotFinished = DateTime.Today < monthEnd;
                var now = DateTime.Now;

                foreach (var (empId, row) in rows)
                {
                    if (!emps.TryGetValue(empId, out var e)) continue;
                    contracts.TryGetValue(empId, out var list);
                    var c = list?.FirstOrDefault();
                    // HĐ trước trong cùng tháng: HĐ chính bắt đầu sau ngày 1 và còn 1 HĐ khác có hiệu lực trước đó trong tháng.
                    var prev = c is not null && c.StartDate.Date > monthStart
                        ? list!.Skip(1).FirstOrDefault(x => x.StartDate.Date < c.StartDate.Date)
                        : null;

                    if (!byEmp.TryGetValue(empId, out var p))
                    {
                        p = new HrPayslip { Id = Guid.NewGuid(), PeriodId = periodId, EmployeeId = empId };
                        db.HrPayslips.Add(p);
                    }

                    p.EmployeeCode = e.EmployeeCode;
                    p.FullName = e.FullName;
                    p.DepartmentId = e.DepartmentId;
                    p.DepartmentName = row.DepartmentName;
                    p.Branch = e.Branch;
                    p.UserId = e.UserId;
                    p.BankAccountNo = e.BankAccountNo;
                    p.BankName = e.BankName;
                    p.Dependents = e.DependentCount;

                    p.ContractId = c?.Id;
                    p.ContractNo = c?.ContractNo;
                    p.ContractType = c?.ContractType;
                    p.BaseSalary = c?.BaseSalary ?? 0;
                    p.InsuranceSalary = c?.InsuranceSalary;
                    p.Allowance = c?.Allowance ?? 0;
                    p.IsNet = c?.IsNetSalary ?? false;
                    if (!p.TaxModeManual) p.TaxMode = HrPayrollCalculator.DefaultTaxMode(c?.ContractType);

                    var extra = new List<string>();
                    if (prev is not null && list!.Count(x => x.StartDate.Date < c!.StartDate.Date) > 1)
                        extra.Add(HrPayrollCalculator.WarnManyContracts);
                    // Không có dữ liệu chấm công → mặc định đủ công chuẩn của người đó trong tháng.
                    if (!row.HasAccount) extra.Add(HrPayrollCalculator.WarnNoTimesheet);
                    var (std, paid) = HrPayrollCalculator.ResolveDays(standardDays, sheet.StandardDays, row.Standard,
                        row.HasAccount ? row.Paid : row.Standard);
                    p.StandardDays = std;
                    p.TimesheetPaidDays = paid;
                    p.NonPaidDays = row.Unpaid + row.Absent + row.Sick;

                    // Tách phần HĐ trước (thử việc): công trong khoảng [đầu tháng hoặc ngày bắt đầu HĐ trước, ngày trước HĐ chính]
                    DateTime proFrom = default, proTo = default;
                    HrTimesheetRow? proRow = null;
                    if (prev is not null)
                    {
                        proFrom = prev.StartDate.Date > monthStart ? prev.StartDate.Date : monthStart;
                        proTo = c!.StartDate.Date.AddDays(-1);
                        var prevEnd = prev.TerminatedDate ?? prev.EndDate;
                        if (prevEnd.HasValue && prevEnd.Value.Date < proTo) proTo = prevEnd.Value.Date;
                        proRow = proTo >= proFrom ? await RangeRowAsync(empId, proFrom, proTo) : null;
                        // HĐ trước chỉ còn ngày nghỉ trong tháng (vd CN 01/11) → không cần tách.
                        if (proRow is null || (proRow.Standard == 0 && proRow.Paid == 0)) prev = null;
                    }
                    if (prev is not null && proRow is not null)
                    {
                        var proPaidCal = row.HasAccount ? proRow.Paid : proRow.Standard;
                        var (proPaid, mainPaid) = HrPayrollCalculator.SplitPaidDays(paid, proPaidCal);
                        p.ProContractId = prev.Id;
                        p.ProContractNo = prev.ContractNo;
                        p.ProContractType = prev.ContractType;
                        p.ProIsNet = prev.IsNetSalary;
                        p.ProBaseSalary = prev.BaseSalary ?? 0;
                        p.ProAllowance = prev.Allowance ?? 0;
                        p.ProFrom = proFrom;
                        p.ProTo = proTo;
                        p.ProCalendarDays = proRow.Standard;
                        p.ProTimesheetPaidDays = proPaid;
                        p.TimesheetPaidDays = mainPaid;
                    }
                    else
                    {
                        p.ProContractId = null;
                        p.ProContractNo = null;
                        p.ProContractType = null;
                        p.ProIsNet = false;
                        p.ProBaseSalary = p.ProAllowance = 0;
                        p.ProFrom = p.ProTo = null;
                        p.ProCalendarDays = p.ProTimesheetPaidDays = 0;
                        p.ProPaidDaysOverride = null;
                    }
                    if (monthNotFinished) extra.Add(HrPayrollCalculator.WarnMonthNotFinished);

                    HrPayrollCalculator.Compute(p, prm, cfg, extra);
                    p.UpdatedAt = now;
                    p.UpdatedBy = actor;
                }

                // Phiếu của nhân viên không còn thuộc kỳ
                foreach (var old in existing.Where(x => !rows.ContainsKey(x.EmployeeId)))
                    db.HrPayslips.Remove(old);

                period.StandardDays = standardDays > 0 ? standardDays : sheet.StandardDays;
                period.ParamId = prm.Id;
                period.CalculatedAt = now;
                period.CalculatedBy = actor;
                await db.SaveChangesAsync();
                return new HrResult(true, "hr_pay_calculated", periodId, rows.Count.ToString());
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<List<HrPayslip>> GetPayslipsAsync(Guid periodId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPayslips.AsNoTracking()
                .Where(x => x.PeriodId == periodId)
                .OrderBy(x => x.DepartmentName).ThenBy(x => x.EmployeeCode)
                .ToListAsync();
        }

        public async Task<HrPayslip?> GetPayslipAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPayslips.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>Sửa các khoản nhập tay của 1 phiếu (chỉ khi kỳ còn nháp) rồi tính lại phiếu đó.</summary>
        public async Task<HrResult> UpdatePayslipAsync(HrPayslipInput input, string actor)
        {
            if (input.ProPaidDaysOverride < 0) return HrResult.Fail("hr_err_negative_value");
            if (input.Overtime < 0 || input.Bonus < 0 || input.OtherIncome < 0 || input.NonTaxableIncome < 0
                || input.Advance < 0 || input.OtherDeduction < 0 || input.PaidDaysOverride < 0)
                return HrResult.Fail("hr_err_negative_value");
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var p = await db.HrPayslips.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (p is null) return HrResult.Fail("hr_err_not_found");
                var period = await db.HrPayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.PeriodId);
                if (period is null) return HrResult.Fail("hr_err_not_found");
                if (period.Status != HrPayrollStatus.Draft) return HrResult.Fail("hr_pay_err_not_draft");
                var prm = period.ParamId.HasValue
                    ? await db.HrPayrollParams.AsNoTracking().FirstOrDefaultAsync(x => x.Id == period.ParamId)
                    : await ParamForAsync(db, new DateTime(period.Year, period.Month, 1));
                if (prm is null) return HrResult.Fail("hr_pay_err_no_param");
                var cfg = await GetConfigAsync(db);

                p.PaidDaysOverride = input.PaidDaysOverride;
                p.ProPaidDaysOverride = p.HasPro ? input.ProPaidDaysOverride : null;
                p.Overtime = input.Overtime;
                p.Bonus = input.Bonus;
                p.OtherIncome = input.OtherIncome;
                p.NonTaxableIncome = input.NonTaxableIncome;
                p.Advance = input.Advance;
                p.OtherDeduction = input.OtherDeduction;
                p.Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();
                var defaultMode = HrPayrollCalculator.DefaultTaxMode(p.ContractType);
                p.TaxModeManual = input.TaxMode != defaultMode;
                p.TaxMode = input.TaxMode;

                // Giữ các cảnh báo không phụ thuộc vào số liệu nhập tay.
                var keep = (p.Warnings ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w is HrPayrollCalculator.WarnNoTimesheet or HrPayrollCalculator.WarnMonthNotFinished
                        or HrPayrollCalculator.WarnManyContracts)
                    .ToList();
                HrPayrollCalculator.Compute(p, prm, cfg, keep);
                p.UpdatedAt = DateTime.Now;
                p.UpdatedBy = actor;
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", p.Id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeletePeriodAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var p = await db.HrPayrollPeriods.FirstOrDefaultAsync(x => x.Id == id);
                if (p is null) return HrResult.Fail("hr_err_not_found");
                if (p.Status != HrPayrollStatus.Draft) return HrResult.Fail("hr_pay_err_not_draft");
                db.HrPayrollPeriods.Remove(p); // HrPayslip: ON DELETE CASCADE
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> SetLockedAsync(Guid id, bool locked, string actor)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var p = await db.HrPayrollPeriods.FirstOrDefaultAsync(x => x.Id == id);
                if (p is null) return HrResult.Fail("hr_err_not_found");
                if (locked)
                {
                    if (p.Status != HrPayrollStatus.Draft) return HrResult.Fail("hr_pay_err_not_draft");
                    if (!await db.HrPayslips.AnyAsync(x => x.PeriodId == id)) return HrResult.Fail("hr_pay_err_empty");
                    p.Status = HrPayrollStatus.Locked;
                    p.LockedAt = DateTime.Now;
                    p.LockedBy = actor;
                }
                else
                {
                    if (p.Status != HrPayrollStatus.Locked) return HrResult.Fail("hr_pay_err_not_locked");
                    p.Status = HrPayrollStatus.Draft;
                    p.LockedAt = null;
                    p.LockedBy = null;
                }
                await db.SaveChangesAsync();
                return HrResult.Ok(locked ? "hr_pay_locked" : "hr_pay_unlocked");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ═════════════════════════ Hạch toán ═════════════════════════

        /// <summary>Bút toán tổng hợp của kỳ (chi phí tách theo phòng ban). Errors = mã TK chưa có trong danh mục.</summary>
        public async Task<(List<HrPostingLine> Lines, List<string> MissingAccounts)> BuildPostingAsync(Guid periodId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var cfg = await GetConfigAsync(db);
            var period = await db.HrPayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == periodId);
            var slips = await db.HrPayslips.AsNoTracking().Where(x => x.PeriodId == periodId).ToListAsync();
            var lines = BuildPostingLines(slips, cfg, period is null ? "" : $"{period.Month:D2}/{period.Year}");

            var codes = lines.Select(l => l.AccountCode).Distinct().ToList();
            var accounts = await db.DanhMucTaiKhoan.AsNoTracking()
                .Where(a => a.Taikhoan != null && codes.Contains(a.Taikhoan))
                .Select(a => new { a.Id, a.Taikhoan })
                .ToListAsync();
            var map = accounts.GroupBy(a => a.Taikhoan!.Trim()).ToDictionary(g => g.Key, g => g.First().Id);
            foreach (var l in lines)
                l.AccountId = map.TryGetValue(l.AccountCode, out var id) ? id : null;
            var missing = lines.Where(l => l.AccountId is null).Select(l => l.AccountCode).Distinct().ToList();
            return (lines, missing);
        }

        /// <summary>Hàm thuần: tạo bút toán từ danh sách phiếu lương.</summary>
        public static List<HrPostingLine> BuildPostingLines(IReadOnlyCollection<HrPayslip> slips, HrPayrollConfig cfg, string label)
        {
            var lines = new List<HrPostingLine>();
            void Add(string acc, decimal debit, decimal credit, string desc, Guid? dept = null)
            {
                if (debit == 0 && credit == 0) return;
                lines.Add(new HrPostingLine { AccountCode = acc, Debit = debit, Credit = credit, Description = desc, DepartmentId = dept });
            }

            // Chi phí lương + BH/KPCĐ phần DN, theo phòng ban
            foreach (var g in slips.GroupBy(s => new { s.DepartmentId, s.DepartmentName }))
            {
                var cost = g.Sum(s => s.GrossIncome + s.CoSocial + s.CoHealth + s.CoUnemployment + s.CoUnionFee);
                Add(cfg.ExpenseAccount, cost, 0, $"Chi phí lương {label} - {g.Key.DepartmentName ?? "Chưa phân phòng ban"}", g.Key.DepartmentId);
            }

            Add(cfg.PayableAccount, 0, slips.Sum(s => s.GrossIncome), $"Phải trả lương {label}");
            Add(cfg.SocialAccount, 0, slips.Sum(s => s.CoSocial + s.EmpSocial), $"BHXH {label}");
            Add(cfg.HealthAccount, 0, slips.Sum(s => s.CoHealth + s.EmpHealth), $"BHYT {label}");
            Add(cfg.UnemploymentAccount, 0, slips.Sum(s => s.CoUnemployment + s.EmpUnemployment), $"BHTN {label}");
            Add(cfg.UnionFeeAccount, 0, slips.Sum(s => s.CoUnionFee), $"KPCĐ {label}");

            // Khấu trừ vào lương
            var deductions = slips.Sum(s => s.EmpSocial + s.EmpHealth + s.EmpUnemployment + s.PersonalIncomeTax + s.Advance + s.OtherDeduction);
            Add(cfg.PayableAccount, deductions, 0, $"Khấu trừ lương {label}");
            Add(cfg.PitAccount, 0, slips.Sum(s => s.PersonalIncomeTax), $"Thuế TNCN {label}");
            Add(cfg.AdvanceAccount, 0, slips.Sum(s => s.Advance), $"Trừ tạm ứng {label}");
            Add(cfg.OtherDeductionAccount, 0, slips.Sum(s => s.OtherDeduction), $"Khấu trừ khác {label}");
            return lines;
        }

        public async Task<HrResult> PostAsync(Guid periodId, DateTime voucherDate, Guid companyId,
            string transactionTypeCode, string actor)
        {
            if (companyId == Guid.Empty) return HrResult.Fail("hr_pay_err_company_required");
            if (string.IsNullOrWhiteSpace(transactionTypeCode)) return HrResult.Fail("hr_pay_err_transaction_type");
            try
            {
                var (lines, missing) = await BuildPostingAsync(periodId);
                if (missing.Count > 0) return HrResult.Fail("hr_pay_err_missing_accounts", string.Join(", ", missing));
                if (lines.Count == 0) return HrResult.Fail("hr_pay_err_empty");
                if (lines.Sum(l => l.Debit) != lines.Sum(l => l.Credit)) return HrResult.Fail("hr_pay_err_unbalanced");

                await using var db = await dbFactory.CreateDbContextAsync();
                var period = await db.HrPayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
                if (period is null) return HrResult.Fail("hr_err_not_found");
                if (period.Status != HrPayrollStatus.Locked) return HrResult.Fail("hr_pay_err_not_locked");

                var baseNo = $"LUONG_{period.Year}{period.Month:D2}";
                var no = baseNo;
                for (var i = 2; await db.AccountingVouchers.AnyAsync(x => x.VoucherNo == no); i++)
                    no = $"{baseNo}_{i}";

                var label = $"{period.Month:D2}/{period.Year}";
                var voucher = new M_AccountingVouchers
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    VoucherNo = no,
                    VoucherDate = voucherDate.Date,
                    PostingDate = voucherDate.Date,
                    FiscalYear = voucherDate.Year,
                    FiscalPeriod = voucherDate.Month,
                    TransactionTypeCode = transactionTypeCode.Trim(),
                    Description = $"Hạch toán lương tháng {label}",
                    CurrencyCode = "VND",
                    ExchangeRate = 1m,
                    Status = 1, // Nháp — kế toán kiểm tra rồi ghi sổ tại màn hình chứng từ
                    ReferenceNo = period.Name,
                    ReferenceDate = new DateTime(period.Year, period.Month, 1).AddMonths(1).AddDays(-1),
                    SourceModule = SourceModule,
                    SourceId = period.Id.ToString(),
                    CreatedBy = actor,
                    CreatedDate = DateTime.UtcNow,
                    Ghiso = false,
                    Approve = false
                };
                db.AccountingVouchers.Add(voucher);
                var n = 1;
                foreach (var l in lines)
                {
                    db.AccountingVoucherLines.Add(new M_AccountingVoucherLines
                    {
                        Id = Guid.NewGuid(),
                        VoucherId = voucher.Id,
                        LineNo_ = n.ToString(CultureInfo.InvariantCulture),
                        DanhMucTaiKhoanID = l.AccountId!.Value,
                        AccountCode = l.AccountCode,
                        DebitAmount = l.Debit,
                        CreditAmount = l.Credit,
                        DebitAmountFC = l.Debit,
                        CreditAmountFC = l.Credit,
                        LineDescription = l.Description.Length > 500 ? l.Description[..500] : l.Description,
                        DepartmentId = l.DepartmentId,
                        IsTaxBook = false,
                        IsManagementBook = false,
                        SortKey = n
                    });
                    n++;
                }

                period.Status = HrPayrollStatus.Posted;
                period.VoucherId = voucher.Id;
                period.VoucherNo = no;
                period.PostedAt = DateTime.Now;
                period.PostedBy = actor;
                await db.SaveChangesAsync();
                return new HrResult(true, "hr_pay_posted", voucher.Id, no);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        /// <summary>Hủy hạch toán: xóa chứng từ NHÁP đã tạo, đưa kỳ về trạng thái đã chốt.</summary>
        public async Task<HrResult> UnpostAsync(Guid periodId)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var period = await db.HrPayrollPeriods.FirstOrDefaultAsync(x => x.Id == periodId);
                if (period is null) return HrResult.Fail("hr_err_not_found");
                if (period.Status != HrPayrollStatus.Posted) return HrResult.Fail("hr_pay_err_not_posted");

                if (period.VoucherId.HasValue)
                {
                    var v = await db.AccountingVouchers.FirstOrDefaultAsync(x => x.Id == period.VoucherId.Value);
                    if (v != null)
                    {
                        if (v.Status != 1 || v.Ghiso == true)
                            return HrResult.Fail("hr_pay_err_voucher_posted", v.VoucherNo);
                        var vl = await db.AccountingVoucherLines.Where(x => x.VoucherId == v.Id).ToListAsync();
                        db.AccountingVoucherLines.RemoveRange(vl);
                        db.AccountingVouchers.Remove(v);
                    }
                }
                period.Status = HrPayrollStatus.Locked;
                period.VoucherId = null;
                period.VoucherNo = null;
                period.PostedAt = null;
                period.PostedBy = null;
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_pay_unposted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ═════════════════════════ Phiếu lương của tôi ═════════════════════════

        /// <summary>Phiếu lương của chính user ở các kỳ đã chốt / đã hạch toán.</summary>
        public async Task<List<(HrPayrollPeriod Period, HrPayslip Slip)>> GetMyPayslipsAsync(Guid userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var rows = await (
                from s in db.HrPayslips.AsNoTracking()
                join p in db.HrPayrollPeriods.AsNoTracking() on s.PeriodId equals p.Id
                where s.UserId == userId && p.Status != HrPayrollStatus.Draft
                orderby p.Year descending, p.Month descending
                select new { p, s }).Take(36).ToListAsync();
            return rows.Select(x => (x.p, x.s)).ToList();
        }

        // ═════════════════════════ Xuất Excel ═════════════════════════

        public static byte[] ExportExcel(HrPayrollPeriod period, IReadOnlyList<HrPayslip> slips, Func<string, string> L)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"Luong_{period.Month:D2}_{period.Year}");
            ws.Cell(1, 1).Value = period.Name;
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"{L("hr_ts_standard_days")}: {period.StandardDays:0.##} · {L(HrPayrollStatus.Key(period.Status))}";

            var cols = new (string Key, Func<HrPayslip, object?> Get, bool Money)[]
            {
                ("hr_employee_code", s => s.EmployeeCode, false),
                ("hr_full_name", s => s.FullName, false),
                ("hr_department", s => s.DepartmentName, false),
                ("hr_contract_no", s => s.ContractNo, false),
                ("hr_base_salary", s => s.BaseSalary, true),
                ("hr_insurance_salary", s => s.InsuranceSalary, true),
                ("hr_allowance", s => s.Allowance, true),
                ("hr_pay_salary_kind", s => s.IsNet ? "NET" : "GROSS", false),
                ("hr_ts_standard", s => s.StandardDays, false),
                ("hr_pay_pro_contract", s => s.HasPro ? s.ProContractNo : null, false),
                ("hr_pay_pro_base_salary", s => s.HasPro ? s.ProBaseSalary : null, true),
                ("hr_pay_pro_paid_days", s => s.HasPro ? s.ProPaidDays : null, false),
                ("hr_pay_pro_salary_by_days", s => s.HasPro ? s.ProSalaryByDays + s.ProAllowanceAmount : null, true),
                ("hr_pay_pro_tax", s => s.HasPro ? s.ProTax : null, true),
                ("hr_pay_paid_days", s => s.PaidDays, false),
                ("hr_pay_salary_by_days", s => s.SalaryByDays, true),
                ("hr_pay_allowance_amount", s => s.AllowanceAmount, true),
                ("hr_pay_overtime", s => s.Overtime, true),
                ("hr_pay_bonus", s => s.Bonus, true),
                ("hr_pay_other_income", s => s.OtherIncome, true),
                ("hr_pay_non_taxable", s => s.NonTaxableIncome, true),
                ("hr_pay_gross_up", s => s.GrossUp + s.ProGrossUp, true),
                ("hr_pay_gross", s => s.GrossIncome, true),
                ("hr_pay_insurance_base", s => s.InsuranceBase, true),
                ("hr_pay_emp_social", s => s.EmpSocial, true),
                ("hr_pay_emp_health", s => s.EmpHealth, true),
                ("hr_pay_emp_unemployment", s => s.EmpUnemployment, true),
                ("hr_dependent_count", s => s.Dependents, false),
                ("hr_pay_family_deduction", s => s.FamilyDeduction, true),
                ("hr_pay_assessable", s => s.AssessableIncome, true),
                ("hr_pay_pit", s => s.PersonalIncomeTax, true),
                ("hr_pay_advance", s => s.Advance, true),
                ("hr_pay_other_deduction", s => s.OtherDeduction, true),
                ("hr_pay_net", s => s.NetPay, true),
                ("hr_pay_co_social", s => s.CoSocial, true),
                ("hr_pay_co_health", s => s.CoHealth, true),
                ("hr_pay_co_unemployment", s => s.CoUnemployment, true),
                ("hr_pay_co_union", s => s.CoUnionFee, true),
                ("hr_pay_employer_cost", s => s.EmployerCost, true),
                ("hr_bank_account", s => s.BankAccountNo, false),
                ("hr_bank_name", s => s.BankName, false),
                ("hr_note", s => s.Note, false)
            };

            const int hr = 4;
            for (var i = 0; i < cols.Length; i++) ws.Cell(hr, i + 1).Value = L(cols[i].Key);
            var r = hr + 1;
            foreach (var s in slips)
            {
                for (var i = 0; i < cols.Length; i++)
                {
                    var v = cols[i].Get(s);
                    var cell = ws.Cell(r, i + 1);
                    switch (v)
                    {
                        case null: break;
                        case decimal d: cell.Value = d; cell.Style.NumberFormat.Format = cols[i].Money ? "#,##0" : "0.##"; break;
                        case int n: cell.Value = n; break;
                        default: cell.Value = v.ToString(); break;
                    }
                }
                r++;
            }
            // Dòng tổng
            ws.Cell(r, 1).Value = L("hr_pay_total");
            ws.Cell(r, 1).Style.Font.Bold = true;
            for (var i = 0; i < cols.Length; i++)
            {
                if (!cols[i].Money || slips.Count == 0) continue;
                var col = ws.Column(i + 1).ColumnLetter();
                ws.Cell(r, i + 1).FormulaA1 = $"SUM({col}{hr + 1}:{col}{r - 1})";
                ws.Cell(r, i + 1).Style.NumberFormat.Format = "#,##0";
                ws.Cell(r, i + 1).Style.Font.Bold = true;
            }

            var head = ws.Range(hr, 1, hr, cols.Length);
            head.Style.Font.Bold = true;
            head.Style.Fill.BackgroundColor = XLColor.LightGray;
            head.Style.Alignment.WrapText = true;
            head.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            var table = ws.Range(hr, 1, r, cols.Length);
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.SheetView.FreezeRows(hr);
            ws.SheetView.FreezeColumns(2);
            ws.Columns().AdjustToContents(hr, r);
            foreach (var c in ws.Columns(1, cols.Length)) if (c.Width > 30) c.Width = 30;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
