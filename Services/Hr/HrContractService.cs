using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>Hợp đồng lao động. Lương chỉ trả về / cho sửa khi có quyền HR_Salary.</summary>
    public sealed class HrContractService(IDbContextFactory<AppDbContext> dbFactory)
    {
        public sealed class SaveOptions
        {
            /// <summary>Chấm dứt các HĐ đang hiệu lực khác của nhân viên (ngày chấm dứt = ngày bắt đầu HĐ mới - 1).</summary>
            public bool TerminatePreviousActive { get; set; }
            /// <summary>NV đang thử việc + HĐ không phải thử việc → chuyển trạng thái Chính thức.</summary>
            public bool PromoteToOfficial { get; set; }
            public bool CanEditSalary { get; set; }
        }

        public async Task<List<HrContractRow>> GetListAsync(HrContractFilter filter, bool includeSalary)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var today = DateTime.Today;

            var q = db.HrContracts.AsNoTracking().AsQueryable();
            if (filter.EmployeeId.HasValue) q = q.Where(x => x.EmployeeId == filter.EmployeeId.Value);
            if (filter.ContractType.HasValue) q = q.Where(x => x.ContractType == filter.ContractType.Value);
            switch (filter.StatusView)
            {
                case 1:
                    q = q.Where(x => x.Status == HrContractStatus.Active && (x.EndDate == null || x.EndDate >= today));
                    break;
                case 2:
                    q = q.Where(x => x.Status == HrContractStatus.Terminated);
                    break;
                case 3:
                    q = q.Where(x => x.Status == HrContractStatus.Active && x.EndDate != null && x.EndDate < today);
                    break;
            }
            if (filter.ExpiringWithinDays.HasValue)
            {
                var limit = today.AddDays(filter.ExpiringWithinDays.Value);
                q = q.Where(x => x.Status == HrContractStatus.Active && x.EndDate != null && x.EndDate <= limit);
            }

            var query =
                from c in q
                join e in db.HrEmployees.AsNoTracking() on c.EmployeeId equals e.Id
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                select new { c, e.EmployeeCode, e.FullName, DeptName = d != null ? d.Name : null };

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var t = filter.Search.Trim();
                query = query.Where(x => x.c.ContractNo.Contains(t) || x.EmployeeCode.Contains(t) || x.FullName.Contains(t));
            }

            var rows = await query
                .OrderByDescending(x => x.c.StartDate).ThenBy(x => x.EmployeeCode)
                .Select(x => new HrContractRow
                {
                    Id = x.c.Id,
                    EmployeeId = x.c.EmployeeId,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeName = x.FullName,
                    DepartmentName = x.DeptName,
                    ContractNo = x.c.ContractNo,
                    ContractType = x.c.ContractType,
                    SignDate = x.c.SignDate,
                    StartDate = x.c.StartDate,
                    EndDate = x.c.EndDate,
                    BaseSalary = includeSalary ? x.c.BaseSalary : null,
                    InsuranceSalary = includeSalary ? x.c.InsuranceSalary : null,
                    Allowance = includeSalary ? x.c.Allowance : null,
                    Status = x.c.Status,
                    TerminatedDate = x.c.TerminatedDate,
                    Note = x.c.Note
                })
                .ToListAsync();

            return rows;
        }

        public async Task<HrContract?> GetAsync(Guid id, bool includeSalary)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var c = await db.HrContracts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (c != null && !includeSalary)
            {
                c.BaseSalary = null;
                c.InsuranceSalary = null;
                c.Allowance = null;
            }
            return c;
        }

        /// <summary>Số HĐ kế tiếp dạng HD2026-0001.</summary>
        public async Task<string> NextContractNoAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var prefix = $"HD{DateTime.Today:yyyy}-";
            var nos = await db.HrContracts.AsNoTracking()
                .Where(x => x.ContractNo.StartsWith(prefix))
                .Select(x => x.ContractNo)
                .ToListAsync();
            var max = 0;
            foreach (var n in nos)
                if (int.TryParse(n[prefix.Length..], out var v) && v > max) max = v;
            return $"{prefix}{max + 1:D4}";
        }

        /// <summary>Thông tin NV cần cho dialog hợp đồng.</summary>
        public async Task<int?> GetEmployeeStatusAsync(Guid employeeId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrEmployees.AsNoTracking()
                .Where(x => x.Id == employeeId)
                .Select(x => (int?)x.Status)
                .FirstOrDefaultAsync();
        }

        public async Task<HrResult> SaveAsync(HrContract model, SaveOptions options, string actor)
        {
            model.ContractNo = (model.ContractNo ?? "").Trim().ToUpperInvariant();
            if (model.EmployeeId == Guid.Empty) return HrResult.Fail("hr_err_employee_required");
            if (model.ContractNo.Length == 0) return HrResult.Fail("hr_err_contract_no_required");
            if (!HrContractType.All.Contains(model.ContractType)) return HrResult.Fail("hr_err_contract_type");

            model.StartDate = model.StartDate.Date;
            if (model.ContractType == HrContractType.Indefinite)
                model.EndDate = null;
            else if (model.EndDate is null)
                return HrResult.Fail("hr_err_end_date_required");
            if (model.EndDate.HasValue && model.EndDate.Value.Date < model.StartDate)
                return HrResult.Fail("hr_err_end_before_start");
            if (model.Status == HrContractStatus.Terminated && model.TerminatedDate is null)
                return HrResult.Fail("hr_err_terminated_date_required");
            if (model.Status == HrContractStatus.Active)
                model.TerminatedDate = null;

            var wasNew = model.Id == Guid.Empty;
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();

                if (await db.HrContracts.AnyAsync(x => x.ContractNo == model.ContractNo && x.Id != model.Id))
                    return HrResult.Fail("hr_err_contract_no_exists");

                var employee = await db.HrEmployees.FirstOrDefaultAsync(x => x.Id == model.EmployeeId);
                if (employee is null) return HrResult.Fail("hr_err_not_found");

                var now = DateTime.Now;
                var isNew = model.Id == Guid.Empty;

                if (isNew)
                {
                    model.Id = Guid.NewGuid();
                    model.CreatedAt = now;
                    model.CreatedBy = actor;
                    if (!options.CanEditSalary)
                    {
                        model.BaseSalary = null;
                        model.InsuranceSalary = null;
                        model.Allowance = null;
                    }
                    db.HrContracts.Add(model);
                }
                else
                {
                    var entity = await db.HrContracts.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (entity is null) return HrResult.Fail("hr_err_not_found");

                    entity.EmployeeId = model.EmployeeId;
                    entity.ContractNo = model.ContractNo;
                    entity.ContractType = model.ContractType;
                    entity.SignDate = model.SignDate;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    entity.Status = model.Status;
                    entity.TerminatedDate = model.TerminatedDate;
                    entity.Note = model.Note;
                    if (options.CanEditSalary)
                    {
                        entity.BaseSalary = model.BaseSalary;
                        entity.InsuranceSalary = model.InsuranceSalary;
                        entity.Allowance = model.Allowance;
                    }
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = actor;
                }

                if (isNew && model.Status == HrContractStatus.Active)
                {
                    if (options.TerminatePreviousActive)
                    {
                        var olds = await db.HrContracts
                            .Where(x => x.EmployeeId == model.EmployeeId && x.Id != model.Id
                                        && x.Status == HrContractStatus.Active)
                            .ToListAsync();
                        var endDate = model.StartDate.AddDays(-1);
                        foreach (var o in olds)
                        {
                            o.Status = HrContractStatus.Terminated;
                            o.TerminatedDate = o.EndDate.HasValue && o.EndDate.Value < endDate ? o.EndDate : endDate;
                            o.UpdatedAt = now;
                            o.UpdatedBy = actor;
                        }
                    }

                    db.HrEmployeeHistories.Add(HrEmployeeService.NewHistory(employee.Id, HrChangeType.Contract,
                        null, model.ContractNo, model.StartDate, actor, HrContractType.Key(model.ContractType)));

                    if (employee.Status == HrEmployeeStatus.Probation)
                    {
                        if (model.ContractType == HrContractType.Probation)
                        {
                            employee.ProbationEndDate = model.EndDate;
                            employee.JoinDate ??= model.StartDate;
                        }
                        else if (options.PromoteToOfficial)
                        {
                            db.HrEmployeeHistories.Add(HrEmployeeService.NewHistory(employee.Id, HrChangeType.Status,
                                HrEmployeeStatus.Key(employee.Status), HrEmployeeStatus.Key(HrEmployeeStatus.Active),
                                model.StartDate, actor));
                            employee.Status = HrEmployeeStatus.Active;
                            employee.OfficialDate = model.StartDate;
                        }
                        employee.UpdatedAt = now;
                        employee.UpdatedBy = actor;
                    }
                    else if (employee.JoinDate is null)
                    {
                        employee.JoinDate = model.StartDate;
                    }
                }

                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", model.Id);
            }
            catch (Exception ex)
            {
                if (wasNew) model.Id = Guid.Empty;
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var entity = await db.HrContracts.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return HrResult.Fail("hr_err_not_found");
                db.HrContracts.Remove(entity);
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }
    }
}
