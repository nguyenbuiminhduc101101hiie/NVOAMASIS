using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Phòng ban (bảng Department có sẵn) và chức vụ (HrPosition).
    /// Lưu ý: Department.Code đang được dùng làm UserList.Department và PermissionTemplate.Dept,
    /// nên không cho đổi mã / xóa khi đã có tài khoản dùng mã đó.
    /// </summary>
    public sealed class HrOrgService(IDbContextFactory<AppDbContext> dbFactory)
    {
        public sealed class DepartmentRow
        {
            public Guid DepartmentId { get; set; }
            public string? Code { get; set; }
            public string? Name { get; set; }
            public string? Remarks { get; set; }
            public int EmployeeCount { get; set; }
            public int UserCount { get; set; }
        }

        public sealed class PositionRow
        {
            public Guid Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public int SortOrder { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public int EmployeeCount { get; set; }
        }

        // ───────────── Phòng ban ─────────────

        public async Task<List<DepartmentRow>> GetDepartmentsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var depts = await db.Department.AsNoTracking().OrderBy(x => x.Code).ToListAsync();
            var empCounts = await db.HrEmployees.AsNoTracking()
                .Where(x => x.DepartmentId != null && x.Status != HrEmployeeStatus.Resigned)
                .GroupBy(x => x.DepartmentId!.Value)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);
            var userCounts = (await db.UserList.AsNoTracking()
                    .Where(x => x.Department != null)
                    .GroupBy(x => x.Department!)
                    .Select(g => new { g.Key, Count = g.Count() })
                    .ToListAsync())
                .GroupBy(x => x.Key.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Count), StringComparer.OrdinalIgnoreCase);

            return depts.Select(d => new DepartmentRow
            {
                DepartmentId = d.DepartmentId,
                Code = d.Code,
                Name = d.Name,
                Remarks = d.Remarks,
                EmployeeCount = empCounts.TryGetValue(d.DepartmentId, out var e) ? e : 0,
                UserCount = !string.IsNullOrWhiteSpace(d.Code) && userCounts.TryGetValue(d.Code.Trim(), out var u) ? u : 0
            }).ToList();
        }

        public async Task<HrResult> SaveDepartmentAsync(Guid id, string code, string name, string? remarks, string actor)
        {
            // Không đổi hoa/thường: mã được so khớp với UserList.Department.
            code = (code ?? "").Trim();
            name = (name ?? "").Trim();
            if (code.Length == 0) return HrResult.Fail("hr_err_code_required");
            if (name.Length == 0) return HrResult.Fail("hr_err_name_required");

            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (await db.Department.AnyAsync(x => x.Code == code && x.DepartmentId != id))
                    return HrResult.Fail("hr_err_code_exists");

                var stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    db.Department.Add(new Department
                    {
                        DepartmentId = id,
                        Code = code,
                        Name = name,
                        Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim(),
                        Editable = true,
                        UserId = actor,
                        UpdateTime = stamp
                    });
                }
                else
                {
                    var entity = await db.Department.FirstOrDefaultAsync(x => x.DepartmentId == id);
                    if (entity is null) return HrResult.Fail("hr_err_not_found");

                    var oldCode = (entity.Code ?? "").Trim();
                    if (!string.Equals(oldCode, code, StringComparison.OrdinalIgnoreCase) &&
                        await db.UserList.AnyAsync(x => x.Department == oldCode))
                        return HrResult.Fail("hr_err_dept_code_in_use");

                    entity.Code = code;
                    entity.Name = name;
                    entity.Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
                    entity.UserId = actor;
                    entity.UpdateTime = stamp;
                }

                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteDepartmentAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var entity = await db.Department.FirstOrDefaultAsync(x => x.DepartmentId == id);
                if (entity is null) return HrResult.Fail("hr_err_not_found");

                if (await db.HrEmployees.AnyAsync(x => x.DepartmentId == id))
                    return HrResult.Fail("hr_err_dept_has_employees");
                var code = (entity.Code ?? "").Trim();
                if (code.Length > 0 && await db.UserList.AnyAsync(x => x.Department == code))
                    return HrResult.Fail("hr_err_dept_code_in_use");

                db.Department.Remove(entity);
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        // ───────────── Chức vụ ─────────────

        public async Task<List<PositionRow>> GetPositionsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPositions.AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(p => new PositionRow
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    EmployeeCount = db.HrEmployees.Count(e => e.PositionId == p.Id && e.Status != HrEmployeeStatus.Resigned)
                })
                .ToListAsync();
        }

        public async Task<HrResult> SavePositionAsync(HrPosition model, string actor)
        {
            model.Code = (model.Code ?? "").Trim().ToUpperInvariant();
            model.Name = (model.Name ?? "").Trim();
            if (model.Code.Length == 0) return HrResult.Fail("hr_err_code_required");
            if (model.Name.Length == 0) return HrResult.Fail("hr_err_name_required");

            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (await db.HrPositions.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
                    return HrResult.Fail("hr_err_code_exists");

                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                    model.CreatedAt = DateTime.Now;
                    model.CreatedBy = actor;
                    db.HrPositions.Add(model);
                }
                else
                {
                    var entity = await db.HrPositions.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (entity is null) return HrResult.Fail("hr_err_not_found");
                    entity.Code = model.Code;
                    entity.Name = model.Name;
                    entity.SortOrder = model.SortOrder;
                    entity.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                    entity.IsActive = model.IsActive;
                }

                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved", model.Id);
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeletePositionAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var entity = await db.HrPositions.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return HrResult.Fail("hr_err_not_found");
                if (await db.HrEmployees.AnyAsync(x => x.PositionId == id))
                    return HrResult.Fail("hr_err_position_in_use");
                db.HrPositions.Remove(entity);
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
