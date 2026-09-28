using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>Hồ sơ nhân viên: CRUD, lịch sử thay đổi, giấy tờ, tổng quan, xuất Excel.</summary>
    public sealed class HrEmployeeService(IDbContextFactory<AppDbContext> dbFactory, HrFileStorage fileStorage)
    {
        public const string CodePrefix = "NV";

        // ───────────────────────────── Danh sách / chi tiết ─────────────────────────────

        public async Task<List<HrEmployeeRow>> GetListAsync(HrEmployeeFilter filter)
        {
            await using var db = await dbFactory.CreateDbContextAsync();

            var q = db.HrEmployees.AsNoTracking().AsQueryable();
            if (!filter.IncludeResigned && filter.Status != HrEmployeeStatus.Resigned)
                q = q.Where(x => x.Status != HrEmployeeStatus.Resigned);
            if (filter.Status.HasValue)
                q = q.Where(x => x.Status == filter.Status.Value);
            if (filter.DepartmentId.HasValue)
                q = q.Where(x => x.DepartmentId == filter.DepartmentId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Branch))
                q = q.Where(x => x.Branch == filter.Branch);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var t = filter.Search.Trim();
                q = q.Where(x => x.EmployeeCode.Contains(t) || x.FullName.Contains(t)
                                 || (x.Phone != null && x.Phone.Contains(t))
                                 || (x.WorkEmail != null && x.WorkEmail.Contains(t))
                                 || (x.IdCardNo != null && x.IdCardNo.Contains(t)));
            }

            var rows = await (
                from e in q
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                join p in db.HrPositions.AsNoTracking() on e.PositionId equals p.Id into pj
                from p in pj.DefaultIfEmpty()
                join m in db.HrEmployees.AsNoTracking() on e.ManagerEmployeeId equals m.Id into mj
                from m in mj.DefaultIfEmpty()
                join u in db.UserList.AsNoTracking() on e.UserId equals u.UsrId into uj
                from u in uj.DefaultIfEmpty()
                orderby e.EmployeeCode
                select new HrEmployeeRow
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                    Gender = e.Gender,
                    DateOfBirth = e.DateOfBirth,
                    Phone = e.Phone,
                    WorkEmail = e.WorkEmail,
                    IdCardNo = e.IdCardNo,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = d != null ? d.Name : null,
                    PositionId = e.PositionId,
                    PositionName = p != null ? p.Name : null,
                    Branch = e.Branch,
                    ManagerEmployeeId = e.ManagerEmployeeId,
                    ManagerName = m != null ? m.FullName : null,
                    UserId = e.UserId,
                    UserName = u != null ? u.Usr : null,
                    JoinDate = e.JoinDate,
                    ProbationEndDate = e.ProbationEndDate,
                    Status = e.Status,
                    ResignDate = e.ResignDate
                }).ToListAsync();

            return rows;
        }

        public async Task<HrEmployee?> GetAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrEmployees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>Mã NV kế tiếp dạng NV0001.</summary>
        public async Task<string> NextEmployeeCodeAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var codes = await db.HrEmployees.AsNoTracking()
                .Where(x => x.EmployeeCode.StartsWith(CodePrefix))
                .Select(x => x.EmployeeCode)
                .ToListAsync();
            return NextCode(codes);
        }

        private static string NextCode(IEnumerable<string> existing)
        {
            var max = 0;
            foreach (var c in existing)
            {
                if (c.Length > CodePrefix.Length && int.TryParse(c[CodePrefix.Length..], out var n) && n > max)
                    max = n;
            }
            return $"{CodePrefix}{max + 1:D4}";
        }

        // ───────────────────────────── Lưu / xóa ─────────────────────────────

        public async Task<HrResult> SaveAsync(HrEmployee model, string actor)
        {
            model.EmployeeCode = (model.EmployeeCode ?? "").Trim().ToUpperInvariant();
            model.FullName = (model.FullName ?? "").Trim();
            if (model.EmployeeCode.Length == 0) return HrResult.Fail("hr_err_code_required");
            if (model.FullName.Length == 0) return HrResult.Fail("hr_err_name_required");
            if (model.Status == HrEmployeeStatus.Resigned && model.ResignDate is null)
                return HrResult.Fail("hr_err_resign_date_required");
            if (model.Id != Guid.Empty && model.ManagerEmployeeId == model.Id)
                return HrResult.Fail("hr_err_manager_self");
            if (model.DependentCount < 0) model.DependentCount = 0;

            var wasNew = model.Id == Guid.Empty;
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();

                if (await db.HrEmployees.AnyAsync(x => x.EmployeeCode == model.EmployeeCode && x.Id != model.Id))
                    return HrResult.Fail("hr_err_code_exists");

                if (model.UserId.HasValue &&
                    await db.HrEmployees.AnyAsync(x => x.UserId == model.UserId && x.Id != model.Id))
                    return HrResult.Fail("hr_err_user_linked");

                if (model.ManagerEmployeeId.HasValue && model.Id != Guid.Empty &&
                    await CreatesManagerCycleAsync(db, model.Id, model.ManagerEmployeeId.Value))
                    return HrResult.Fail("hr_err_manager_cycle");

                var now = DateTime.Now;
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                    model.CreatedAt = now;
                    model.CreatedBy = actor;
                    db.HrEmployees.Add(model);
                    db.HrEmployeeHistories.Add(NewHistory(model.Id, HrChangeType.Created, null,
                        $"{model.EmployeeCode} - {model.FullName}", model.JoinDate ?? DateTime.Today, actor));
                }
                else
                {
                    var entity = await db.HrEmployees.FirstOrDefaultAsync(x => x.Id == model.Id);
                    if (entity is null) return HrResult.Fail("hr_err_not_found");

                    await AddChangeHistoryAsync(db, entity, model, actor);

                    var createdAt = entity.CreatedAt;
                    var createdBy = entity.CreatedBy;
                    db.Entry(entity).CurrentValues.SetValues(model);
                    entity.CreatedAt = createdAt;
                    entity.CreatedBy = createdBy;
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = actor;
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

        private static async Task<bool> CreatesManagerCycleAsync(AppDbContext db, Guid employeeId, Guid managerId)
        {
            // Đi ngược chuỗi quản lý từ managerId; nếu gặp employeeId → vòng lặp.
            var visited = new HashSet<Guid>();
            Guid? current = managerId;
            while (current.HasValue && visited.Add(current.Value))
            {
                if (current.Value == employeeId) return true;
                current = await db.HrEmployees.AsNoTracking()
                    .Where(x => x.Id == current.Value)
                    .Select(x => x.ManagerEmployeeId)
                    .FirstOrDefaultAsync();
            }
            return false;
        }

        private static async Task AddChangeHistoryAsync(AppDbContext db, HrEmployee old, HrEmployee now, string actor)
        {
            var effective = DateTime.Today;

            if (old.DepartmentId != now.DepartmentId)
            {
                var names = await db.Department.AsNoTracking()
                    .Where(x => x.DepartmentId == old.DepartmentId || x.DepartmentId == now.DepartmentId)
                    .Select(x => new { x.DepartmentId, x.Name })
                    .ToListAsync();
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Department,
                    names.FirstOrDefault(x => x.DepartmentId == old.DepartmentId)?.Name,
                    names.FirstOrDefault(x => x.DepartmentId == now.DepartmentId)?.Name, effective, actor));
            }

            if (old.PositionId != now.PositionId)
            {
                var names = await db.HrPositions.AsNoTracking()
                    .Where(x => x.Id == old.PositionId || x.Id == now.PositionId)
                    .Select(x => new { x.Id, x.Name })
                    .ToListAsync();
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Position,
                    names.FirstOrDefault(x => x.Id == old.PositionId)?.Name,
                    names.FirstOrDefault(x => x.Id == now.PositionId)?.Name, effective, actor));
            }

            if (!string.Equals(old.Branch, now.Branch, StringComparison.OrdinalIgnoreCase))
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Branch, old.Branch, now.Branch, effective, actor));

            if (old.ManagerEmployeeId != now.ManagerEmployeeId)
            {
                var names = await db.HrEmployees.AsNoTracking()
                    .Where(x => x.Id == old.ManagerEmployeeId || x.Id == now.ManagerEmployeeId)
                    .Select(x => new { x.Id, x.FullName })
                    .ToListAsync();
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Manager,
                    names.FirstOrDefault(x => x.Id == old.ManagerEmployeeId)?.FullName,
                    names.FirstOrDefault(x => x.Id == now.ManagerEmployeeId)?.FullName, effective, actor));
            }

            if (old.Status != now.Status)
            {
                var date = now.Status == HrEmployeeStatus.Resigned ? now.ResignDate ?? effective
                    : now.Status == HrEmployeeStatus.Active ? now.OfficialDate ?? effective
                    : effective;
                // Lưu khóa localization, trang lịch sử sẽ dịch.
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Status,
                    HrEmployeeStatus.Key(old.Status), HrEmployeeStatus.Key(now.Status), date, actor,
                    now.Status == HrEmployeeStatus.Resigned ? now.ResignReason : null));
            }

            if (old.UserId != now.UserId)
            {
                var names = await db.UserList.AsNoTracking()
                    .Where(x => x.UsrId == old.UserId || x.UsrId == now.UserId)
                    .Select(x => new { x.UsrId, x.Usr })
                    .ToListAsync();
                db.HrEmployeeHistories.Add(NewHistory(old.Id, HrChangeType.Account,
                    names.FirstOrDefault(x => x.UsrId == old.UserId)?.Usr,
                    names.FirstOrDefault(x => x.UsrId == now.UserId)?.Usr, effective, actor));
            }
        }

        internal static HrEmployeeHistory NewHistory(Guid employeeId, string type, string? oldValue, string? newValue,
            DateTime effective, string actor, string? note = null) => new()
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ChangeType = type,
            OldValue = Truncate(oldValue, 500),
            NewValue = Truncate(newValue, 500),
            EffectiveDate = effective.Date,
            Note = Truncate(note, 1000),
            CreatedAt = DateTime.Now,
            CreatedBy = actor
        };

        private static string? Truncate(string? s, int max) =>
            s is null ? null : s.Length <= max ? s : s[..max];

        public async Task<HrResult> DeleteAsync(Guid id, string tenantKey)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var entity = await db.HrEmployees.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return HrResult.Fail("hr_err_not_found");

                if (await db.HrContracts.AnyAsync(x => x.EmployeeId == id))
                    return HrResult.Fail("hr_err_has_contracts");
                if (await db.HrEmployees.AnyAsync(x => x.ManagerEmployeeId == id))
                    return HrResult.Fail("hr_err_has_subordinates");

                // HrDocument / HrEmployeeHistory: ON DELETE CASCADE ở DB.
                db.HrEmployees.Remove(entity);
                await db.SaveChangesAsync();
                fileStorage.TryDeleteEmployeeFolder(tenantKey, id);
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        // ───────────────────────────── Danh mục tra cứu ─────────────────────────────

        public async Task<List<HrLookupItem>> GetDepartmentLookupAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.Department.AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new HrLookupItem { Id = x.DepartmentId, Code = x.Code ?? "", Name = x.Name ?? "" })
                .ToListAsync();
        }

        public async Task<List<HrLookupItem>> GetPositionLookupAsync(bool activeOnly = true)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrPositions.AsNoTracking()
                .Where(x => !activeOnly || x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(x => new HrLookupItem { Id = x.Id, Code = x.Code, Name = x.Name })
                .ToListAsync();
        }

        /// <summary>Nhân viên chưa nghỉ việc (chọn quản lý / chọn NV cho hợp đồng).</summary>
        public async Task<List<HrLookupItem>> GetEmployeeLookupAsync(bool includeResigned = false)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrEmployees.AsNoTracking()
                .Where(x => includeResigned || x.Status != HrEmployeeStatus.Resigned)
                .OrderBy(x => x.EmployeeCode)
                .Select(x => new HrLookupItem { Id = x.Id, Code = x.EmployeeCode, Name = x.FullName })
                .ToListAsync();
        }

        /// <summary>Tài khoản chưa gắn hồ sơ (kèm tài khoản đang gắn với nhân viên đang sửa).</summary>
        public async Task<List<HrUserLookup>> GetUnlinkedUsersAsync(Guid? keepUserId = null)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var linked = db.HrEmployees.Where(x => x.UserId != null).Select(x => x.UserId!.Value);
            return await db.UserList.AsNoTracking()
                .Where(u => !linked.Contains(u.UsrId) || (keepUserId != null && u.UsrId == keepUserId))
                .OrderBy(u => u.Usr)
                .Select(u => new HrUserLookup
                {
                    UsrId = u.UsrId,
                    Usr = u.Usr,
                    Name = u.Name,
                    Email = u.Email,
                    Department = u.Department,
                    Branch = u.Branch
                })
                .ToListAsync();
        }

        /// <summary>Tạo nhanh hồ sơ từ tài khoản đăng nhập có sẵn.</summary>
        public async Task<HrResult> CreateFromUsersAsync(IReadOnlyCollection<Guid> userIds, string actor)
        {
            if (userIds.Count == 0) return HrResult.Fail("hr_import_none_selected");
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var linked = await db.HrEmployees.Where(x => x.UserId != null)
                    .Select(x => x.UserId!.Value).ToListAsync();
                var users = await db.UserList.AsNoTracking()
                    .Where(u => userIds.Contains(u.UsrId) && !linked.Contains(u.UsrId))
                    .OrderBy(u => u.Usr)
                    .ToListAsync();
                var depts = await db.Department.AsNoTracking()
                    .Select(x => new { x.DepartmentId, x.Code })
                    .ToListAsync();
                var codes = await db.HrEmployees.AsNoTracking()
                    .Where(x => x.EmployeeCode.StartsWith(CodePrefix))
                    .Select(x => x.EmployeeCode).ToListAsync();

                var now = DateTime.Now;
                foreach (var u in users)
                {
                    var code = NextCode(codes);
                    codes.Add(code);
                    var dept = depts.FirstOrDefault(d =>
                        !string.IsNullOrWhiteSpace(d.Code) &&
                        string.Equals(d.Code!.Trim(), u.Department?.Trim(), StringComparison.OrdinalIgnoreCase));

                    var e = new HrEmployee
                    {
                        Id = Guid.NewGuid(),
                        EmployeeCode = code,
                        FullName = string.IsNullOrWhiteSpace(u.Name) ? (u.Usr ?? code) : u.Name!.Trim(),
                        WorkEmail = u.Email,
                        UserId = u.UsrId,
                        DepartmentId = dept?.DepartmentId,
                        Branch = u.Branch,
                        CompanyCode = u.CompanyCode,
                        Status = HrEmployeeStatus.Active,
                        CreatedAt = now,
                        CreatedBy = actor
                    };
                    db.HrEmployees.Add(e);
                    db.HrEmployeeHistories.Add(NewHistory(e.Id, HrChangeType.Created, null,
                        $"{e.EmployeeCode} - {e.FullName} ({u.Usr})", DateTime.Today, actor));
                }

                await db.SaveChangesAsync();
                return new HrResult(true, "hr_import_done", null, users.Count.ToString());
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ───────────────────────────── Lịch sử ─────────────────────────────

        public async Task<List<HrEmployeeHistory>> GetHistoryAsync(Guid employeeId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrEmployeeHistories.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        // ───────────────────────────── Giấy tờ ─────────────────────────────

        public async Task<List<HrDocument>> GetDocumentsAsync(Guid employeeId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrDocuments.AsNoTracking()
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.UploadedAt)
                .ToListAsync();
        }

        public async Task<HrDocument?> FindDocumentAsync(Guid documentId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.HrDocuments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == documentId);
        }

        public async Task<HrResult> AddDocumentAsync(Guid employeeId, string docType, string? title,
            string fileName, string? contentType, Stream content, string tenantKey, string actor)
        {
            if (!HrFileStorage.IsAllowed(fileName))
                return HrResult.Fail("hr_doc_type_not_allowed");

            string? relativePath = null;
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (!await db.HrEmployees.AnyAsync(x => x.Id == employeeId))
                    return HrResult.Fail("hr_err_not_found");

                var id = Guid.NewGuid();
                relativePath = await fileStorage.SaveAsync(tenantKey, employeeId, id, fileName, content);
                long size;
                await using (var s = fileStorage.OpenRead(relativePath)) size = s.Length;

                db.HrDocuments.Add(new HrDocument
                {
                    Id = id,
                    EmployeeId = employeeId,
                    DocType = HrDocumentType.All.Contains(docType) ? docType : HrDocumentType.Other,
                    Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
                    FileName = Path.GetFileName(fileName),
                    FilePath = relativePath,
                    ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                    FileSize = size,
                    UploadedAt = DateTime.Now,
                    UploadedBy = actor
                });
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_doc_uploaded", id);
            }
            catch (InvalidOperationException ex) when (ex.Message == "hr_doc_too_large")
            {
                return HrResult.Fail("hr_doc_too_large");
            }
            catch (Exception ex)
            {
                if (relativePath != null) fileStorage.TryDelete(relativePath);
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteDocumentAsync(Guid documentId)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var doc = await db.HrDocuments.FirstOrDefaultAsync(x => x.Id == documentId);
                if (doc is null) return HrResult.Fail("hr_err_not_found");
                db.HrDocuments.Remove(doc);
                await db.SaveChangesAsync();
                fileStorage.TryDelete(doc.FilePath);
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        // ───────────────────────────── Tổng quan ─────────────────────────────

        public async Task<HrDashboardData> GetDashboardAsync(int alertDays = 30)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);
            var limit = today.AddDays(alertDays);

            var working = db.HrEmployees.AsNoTracking().Where(x => x.Status != HrEmployeeStatus.Resigned);

            var byStatus = await working.GroupBy(x => x.Status)
                .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

            var data = new HrDashboardData
            {
                Probation = byStatus.FirstOrDefault(x => x.Key == HrEmployeeStatus.Probation)?.Count ?? 0,
                Active = byStatus.FirstOrDefault(x => x.Key == HrEmployeeStatus.Active)?.Count ?? 0,
                Suspended = byStatus.FirstOrDefault(x => x.Key == HrEmployeeStatus.Suspended)?.Count ?? 0,
                JoinedThisMonth = await db.HrEmployees.CountAsync(x => x.JoinDate >= monthStart && x.JoinDate < nextMonth),
                ResignedThisMonth = await db.HrEmployees.CountAsync(x =>
                    x.Status == HrEmployeeStatus.Resigned && x.ResignDate >= monthStart && x.ResignDate < nextMonth),
                WithoutAccount = await working.CountAsync(x => x.UserId == null),
                WithoutContract = await working.CountAsync(x =>
                    !db.HrContracts.Any(c => c.EmployeeId == x.Id && c.Status == HrContractStatus.Active))
            };
            data.Total = data.Probation + data.Active + data.Suspended;

            var byDept = await (
                from e in working
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                group e by d != null ? d.Name : null into g
                select new { g.Key, Count = g.Count() }).ToListAsync();
            data.ByDepartment = byDept
                .Select(x => new HrCountItem { Label = x.Key ?? "", Count = x.Count })
                .OrderByDescending(x => x.Count).ToList();

            var byBranch = await working.GroupBy(x => x.Branch)
                .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();
            data.ByBranch = byBranch
                .Select(x => new HrCountItem { Label = x.Key ?? "", Count = x.Count })
                .OrderByDescending(x => x.Count).ToList();

            // HĐ hiệu lực sắp hết hạn (kèm cả HĐ đã quá hạn nhưng chưa xử lý)
            data.ContractsExpiring = await (
                from c in db.HrContracts.AsNoTracking()
                join e in working on c.EmployeeId equals e.Id
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                where c.Status == HrContractStatus.Active && c.EndDate != null && c.EndDate <= limit
                orderby c.EndDate
                select new HrAlertItem
                {
                    EmployeeId = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.FullName,
                    DepartmentName = d != null ? d.Name : null,
                    Date = c.EndDate!.Value,
                    Extra = c.ContractNo
                }).ToListAsync();

            data.ProbationEnding = await (
                from e in working
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                where e.Status == HrEmployeeStatus.Probation && e.ProbationEndDate != null && e.ProbationEndDate <= limit
                orderby e.ProbationEndDate
                select new HrAlertItem
                {
                    EmployeeId = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    EmployeeName = e.FullName,
                    DepartmentName = d != null ? d.Name : null,
                    Date = e.ProbationEndDate!.Value
                }).ToListAsync();

            var birthdays = await (
                from e in working
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                where e.DateOfBirth != null && e.DateOfBirth.Value.Month == today.Month
                select new { e.Id, e.EmployeeCode, e.FullName, DeptName = d != null ? d.Name : null, Dob = e.DateOfBirth!.Value })
                .ToListAsync();
            data.Birthdays = birthdays
                .Select(x => new HrAlertItem
                {
                    EmployeeId = x.Id,
                    EmployeeCode = x.EmployeeCode,
                    EmployeeName = x.FullName,
                    DepartmentName = x.DeptName,
                    Date = SafeDate(today.Year, x.Dob.Month, x.Dob.Day),
                    Extra = (today.Year - x.Dob.Year).ToString()
                })
                .OrderBy(x => x.Date).ToList();

            foreach (var a in data.ContractsExpiring.Concat(data.ProbationEnding).Concat(data.Birthdays))
                a.Days = (int)(a.Date.Date - today).TotalDays;

            return data;
        }

        private static DateTime SafeDate(int year, int month, int day) =>
            new(year, month, Math.Min(day, DateTime.DaysInMonth(year, month)));

        // ───────────────────────────── Xuất Excel ─────────────────────────────

        /// <param name="L">Hàm dịch khóa localization.</param>
        public static byte[] ExportExcel(IReadOnlyList<HrEmployeeRow> rows, Func<string, string> L)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Employees");
            string[] headers =
            {
                L("hr_employee_code"), L("hr_full_name"), L("hr_gender"), L("hr_dob"), L("hr_phone"),
                L("hr_work_email"), L("hr_id_card_no"), L("hr_department"), L("hr_position"), L("hr_branch"),
                L("hr_manager"), L("hr_account"), L("hr_join_date"), L("hr_probation_end"), L("hr_status"),
                L("hr_resign_date")
            };
            for (var i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            var r = 2;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.EmployeeCode;
                ws.Cell(r, 2).Value = x.FullName;
                ws.Cell(r, 3).Value = L(HrGender.Key(x.Gender));
                SetDate(ws.Cell(r, 4), x.DateOfBirth);
                ws.Cell(r, 5).Value = x.Phone ?? "";
                ws.Cell(r, 6).Value = x.WorkEmail ?? "";
                ws.Cell(r, 7).Value = x.IdCardNo ?? "";
                ws.Cell(r, 8).Value = x.DepartmentName ?? "";
                ws.Cell(r, 9).Value = x.PositionName ?? "";
                ws.Cell(r, 10).Value = x.Branch ?? "";
                ws.Cell(r, 11).Value = x.ManagerName ?? "";
                ws.Cell(r, 12).Value = x.UserName ?? "";
                SetDate(ws.Cell(r, 13), x.JoinDate);
                SetDate(ws.Cell(r, 14), x.ProbationEndDate);
                ws.Cell(r, 15).Value = L(HrEmployeeStatus.Key(x.Status));
                SetDate(ws.Cell(r, 16), x.ResignDate);
                r++;
            }

            var header = ws.Range(1, 1, 1, headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Column(5).Style.NumberFormat.Format = "@";
            ws.Column(7).Style.NumberFormat.Format = "@";
            ws.SheetView.FreezeRows(1);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        private static void SetDate(IXLCell cell, DateTime? value)
        {
            if (!value.HasValue) return;
            cell.Value = value.Value;
            cell.Style.DateFormat.Format = "dd/MM/yyyy";
        }
    }
}
