using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Models.Kho;
using NVOAMASIS.Services.Accounting;

namespace NVOAMASIS.Services.Kho
{
    /// <summary>
    /// 10.18 Kho vật tư, hàng hóa (TK 151–156): danh mục, phiếu kho, tính giá xuất bình quân tức thời, ghi sổ cái, báo cáo.
    ///
    /// Quy trình phiếu: Lưu nháp (chưa ảnh hưởng tồn kho) → Ghi sổ (tính giá xuất + sinh chứng từ kế toán) → Bỏ ghi sổ để sửa.
    /// Ghi sổ / bỏ ghi sổ 1 phiếu có thể làm thay đổi giá xuất của các phiếu xuất SAU nó (bình quân tức thời):
    /// phần mềm tự tính lại và cập nhật luôn bút toán sổ cái của những phiếu đó.
    ///
    /// Bút toán khi ghi sổ (1 chứng từ AccountingVouchers, SourceModule = INVENTORY, số chứng từ = số phiếu kho):
    ///   Phiếu nhập:  Nợ TK kho (151–156) / Có TK đối ứng (331, 111, 112, 154...)   theo thành tiền
    ///                Nợ TK thuế (1331)   / Có TK đối ứng                            theo tiền thuế (nếu có)
    ///   Phiếu xuất:  Nợ TK đối ứng (632, 621, 627, 641, 642, 242...) / Có TK kho   theo giá xuất tính được
    ///   Tồn đầu kỳ, chuyển kho: không sinh bút toán (số dư đầu kỳ TK nhập ở 10.9; chuyển kho cùng TK).
    /// </summary>
    public sealed class KhoService(IDbContextFactory<AppDbContext> dbFactory)
    {
        public const string SourceModule = "INVENTORY";
        private static readonly SemaphoreSlim Gate = new(1, 1);

        // ═════════════════════════ Danh mục ═════════════════════════

        public async Task<List<KhoVatTu>> GetItemsAsync(bool includeInactive = true)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var q = db.KhoVatTus.AsNoTracking();
            if (!includeInactive) q = q.Where(x => x.IsActive);
            return (await q.ToListAsync()).OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public async Task<List<KhoHang>> GetWarehousesAsync(bool includeInactive = true)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var q = db.KhoHangs.AsNoTracking();
            if (!includeInactive) q = q.Where(x => x.IsActive);
            return (await q.ToListAsync()).OrderBy(x => x.Code, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>TK trong danh mục tài khoản (mã → tên).</summary>
        public async Task<Dictionary<string, string>> GetAccountsAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await LoadAccountsAsync(db);
        }

        private static async Task<Dictionary<string, string>> LoadAccountsAsync(AppDbContext db)
        {
            var rows = await db.DanhMucTaiKhoan.AsNoTracking()
                .Where(x => x.Taikhoan != null && x.Taikhoan != "")
                .Select(x => new { x.Taikhoan, x.Tentaikhoan })
                .ToListAsync();
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var r in rows)
            {
                var code = r.Taikhoan!.Trim();
                if (code.Length > 0 && !map.ContainsKey(code)) map[code] = r.Tentaikhoan?.Trim() ?? "";
            }
            return map;
        }

        public async Task SaveItemAsync(KhoVatTu input, string user)
        {
            var code = (input.Code ?? "").Trim();
            var name = (input.Name ?? "").Trim();
            var account = (input.InventoryAccount ?? "").Trim();
            if (code.Length == 0) throw new InvalidOperationException("Chưa nhập mã vật tư.");
            if (name.Length == 0) throw new InvalidOperationException("Chưa nhập tên vật tư.");
            if (!KhoAccounts.IsInventory(account))
                throw new InvalidOperationException($"TK kho '{account}' không hợp lệ — phải là 151, 152, 153, 154, 155, 156 hoặc TK con.");
            if (input.MinQty < 0) throw new InvalidOperationException("Tồn tối thiểu không được âm.");

            await using var db = await dbFactory.CreateDbContextAsync();
            await EnsureAccountExistsAsync(db, account, "TK kho");
            if (await db.KhoVatTus.AnyAsync(x => x.Code == code && x.Id != input.Id))
                throw new InvalidOperationException($"Mã vật tư '{code}' đã tồn tại.");

            var entity = await db.KhoVatTus.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity is null)
            {
                entity = new KhoVatTu { Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id, CreatedAt = DateTime.Now, CreatedBy = user };
                db.KhoVatTus.Add(entity);
            }
            else
            {
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = user;
            }
            entity.Code = code;
            entity.Name = name;
            entity.Unit = (input.Unit ?? "").Trim();
            entity.InventoryAccount = account;
            entity.Category = Clean(input.Category);
            entity.MinQty = input.MinQty;
            entity.Note = Clean(input.Note);
            entity.IsActive = input.IsActive;
            await db.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.KhoVatTus.FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new InvalidOperationException("Không tìm thấy vật tư.");
            var used = await db.PhieuKhoChiTiets.CountAsync(x => x.VatTuId == id);
            if (used > 0)
                throw new InvalidOperationException($"Vật tư {entity.Code} đã có trên {used} dòng phiếu kho — không xóa được, hãy bỏ chọn 'Đang dùng'.");
            db.KhoVatTus.Remove(entity);
            await db.SaveChangesAsync();
        }

        /// <summary>Nhập danh mục từ Excel: trùng mã thì cập nhật, mới thì thêm. Trả về (thêm, cập nhật).</summary>
        public async Task<(int Added, int Updated)> ImportItemsAsync(IReadOnlyList<KhoVatTu> rows, string user)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var accounts = await LoadAccountsAsync(db);
            var errors = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var line = $"Dòng {i + 2}";
                if (string.IsNullOrWhiteSpace(r.Code)) { errors.Add($"{line}: thiếu mã."); continue; }
                if (!seen.Add(r.Code.Trim())) errors.Add($"{line}: mã {r.Code} bị trùng trong file.");
                if (string.IsNullOrWhiteSpace(r.Name)) errors.Add($"{line}: thiếu tên.");
                var acc = (r.InventoryAccount ?? "").Trim();
                if (!KhoAccounts.IsInventory(acc)) errors.Add($"{line}: TK kho '{acc}' phải là 151–156.");
                else if (accounts.Count > 0 && !accounts.ContainsKey(acc)) errors.Add($"{line}: TK {acc} chưa có trong danh mục tài khoản.");
            }
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors.Take(15)) + (errors.Count > 15 ? $"\n... và {errors.Count - 15} lỗi khác." : ""));

            var existing = (await db.KhoVatTus.ToListAsync()).ToDictionary(x => x.Code.Trim(), StringComparer.OrdinalIgnoreCase);
            int added = 0, updated = 0;
            foreach (var r in rows)
            {
                if (!existing.TryGetValue(r.Code.Trim(), out var e))
                {
                    e = new KhoVatTu { Id = Guid.NewGuid(), Code = r.Code.Trim(), CreatedAt = DateTime.Now, CreatedBy = user, IsActive = true };
                    db.KhoVatTus.Add(e);
                    existing[e.Code] = e;
                    added++;
                }
                else
                {
                    e.UpdatedAt = DateTime.Now;
                    e.UpdatedBy = user;
                    updated++;
                }
                e.Name = r.Name.Trim();
                e.Unit = (r.Unit ?? "").Trim();
                e.InventoryAccount = r.InventoryAccount.Trim();
                e.Category = Clean(r.Category);
                if (r.MinQty.HasValue) e.MinQty = r.MinQty;
                if (!string.IsNullOrWhiteSpace(r.Note)) e.Note = r.Note.Trim();
            }
            await db.SaveChangesAsync();
            return (added, updated);
        }

        public async Task SaveWarehouseAsync(KhoHang input, string user)
        {
            var code = (input.Code ?? "").Trim();
            var name = (input.Name ?? "").Trim();
            if (code.Length == 0) throw new InvalidOperationException("Chưa nhập mã kho.");
            if (name.Length == 0) throw new InvalidOperationException("Chưa nhập tên kho.");

            await using var db = await dbFactory.CreateDbContextAsync();
            if (await db.KhoHangs.AnyAsync(x => x.Code == code && x.Id != input.Id))
                throw new InvalidOperationException($"Mã kho '{code}' đã tồn tại.");
            var entity = await db.KhoHangs.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (entity is null)
            {
                entity = new KhoHang { Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id, CreatedAt = DateTime.Now, CreatedBy = user };
                db.KhoHangs.Add(entity);
            }
            else
            {
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = user;
            }
            entity.Code = code;
            entity.Name = name;
            entity.Address = Clean(input.Address);
            entity.Keeper = Clean(input.Keeper);
            entity.IsActive = input.IsActive;
            await db.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var entity = await db.KhoHangs.FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new InvalidOperationException("Không tìm thấy kho.");
            var used = await db.PhieuKhos.CountAsync(x => x.WarehouseId == id || x.ToWarehouseId == id);
            if (used > 0)
                throw new InvalidOperationException($"Kho {entity.Code} đã có {used} phiếu — không xóa được, hãy bỏ chọn 'Đang dùng'.");
            db.KhoHangs.Remove(entity);
            await db.SaveChangesAsync();
        }

        // ═════════════════════════ Phiếu kho ═════════════════════════

        public async Task<List<PhieuKho>> GetDocsAsync(DateTime from, DateTime to, string? docType, Guid? warehouseId, int? status)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var toEx = to.Date.AddDays(1);
            var q = db.PhieuKhos.AsNoTracking().Where(x => x.DocDate >= from.Date && x.DocDate < toEx);
            if (!string.IsNullOrWhiteSpace(docType)) q = q.Where(x => x.DocType == docType);
            if (warehouseId.HasValue) q = q.Where(x => x.WarehouseId == warehouseId || x.ToWarehouseId == warehouseId);
            if (status.HasValue) q = q.Where(x => x.Status == status);
            return await q.OrderByDescending(x => x.DocDate).ThenByDescending(x => x.CreatedAt).ToListAsync();
        }

        /// <summary>Tổng tiền của các phiếu (để hiện trên danh sách).</summary>
        public async Task<Dictionary<Guid, (decimal Amount, decimal Vat, int Lines)>> GetDocTotalsAsync(IReadOnlyCollection<Guid> docIds)
        {
            var result = new Dictionary<Guid, (decimal, decimal, int)>();
            if (docIds.Count == 0) return result;
            await using var db = await dbFactory.CreateDbContextAsync();
            foreach (var chunk in docIds.Chunk(1000))
            {
                var part = await db.PhieuKhoChiTiets.AsNoTracking()
                    .Where(x => chunk.Contains(x.PhieuKhoId))
                    .GroupBy(x => x.PhieuKhoId)
                    .Select(g => new { g.Key, Amount = g.Sum(x => x.Amount), Vat = g.Sum(x => x.VatAmount), Lines = g.Count() })
                    .ToListAsync();
                foreach (var p in part) result[p.Key] = (p.Amount, p.Vat, p.Lines);
            }
            return result;
        }

        public async Task<(PhieuKho Doc, List<PhieuKhoChiTiet> Lines)?> GetDocAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var doc = await db.PhieuKhos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (doc is null) return null;
            var lines = await db.PhieuKhoChiTiets.AsNoTracking().Where(x => x.PhieuKhoId == id).OrderBy(x => x.LineNo).ToListAsync();
            return (doc, lines);
        }

        public async Task<string?> GetVoucherNoAsync(Guid? voucherId)
        {
            if (!voucherId.HasValue) return null;
            await using var db = await dbFactory.CreateDbContextAsync();
            return await db.AccountingVouchers.AsNoTracking().Where(x => x.Id == voucherId).Select(x => x.VoucherNo).FirstOrDefaultAsync();
        }

        /// <summary>Số phiếu tiếp theo: NK/XK/CK/DK + yyMM + "-" + 4 số.</summary>
        public async Task<string> NextDocNoAsync(string docType, DateTime date)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await NextDocNoAsync(db, docType, date);
        }

        private static async Task<string> NextDocNoAsync(AppDbContext db, string docType, DateTime date)
        {
            var prefix = $"{KhoDocTypes.Prefix(docType)}{date:yyMM}-";
            var existing = await db.PhieuKhos.AsNoTracking()
                .Where(x => x.DocNo.StartsWith(prefix))
                .Select(x => x.DocNo)
                .ToListAsync();
            var max = existing
                .Select(x => int.TryParse(x.AsSpan(prefix.Length), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();
            return $"{prefix}{max + 1:D4}";
        }

        /// <summary>Lưu phiếu nháp (thêm mới hoặc sửa). Phiếu đã ghi sổ phải bỏ ghi sổ trước khi sửa.</summary>
        public async Task<Guid> SaveDraftAsync(PhieuKho input, IReadOnlyList<PhieuKhoChiTiet> inputLines, string user)
        {
            await Gate.WaitAsync();
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var doc = await db.PhieuKhos.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (doc is { Status: KhoStatus.Posted })
                    throw new InvalidOperationException($"Phiếu {doc.DocNo} đã ghi sổ — bỏ ghi sổ trước khi sửa.");
                if (doc is not null && doc.DocDate.Date != input.DocDate.Date)
                    await AccountingPeriodLock.EnsureOpenAsync(db, doc.DocDate, "sửa phiếu kho");

                var lines = await NormalizeAndValidateAsync(db, input, inputLines);

                var isNew = doc is null;
                if (isNew)
                {
                    doc = new PhieuKho
                    {
                        Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id,
                        CreatedAt = DateTime.Now,
                        CreatedBy = user,
                        Status = KhoStatus.Draft
                    };
                    db.PhieuKhos.Add(doc);
                }
                else
                {
                    doc!.UpdatedAt = DateTime.Now;
                    doc.UpdatedBy = user;
                }

                var docNo = (input.DocNo ?? "").Trim();
                if (docNo.Length == 0 || (!isNew && doc.DocType != input.DocType && docNo == doc.DocNo))
                    docNo = await NextDocNoAsync(db, input.DocType, input.DocDate);
                if (await db.PhieuKhos.AnyAsync(x => x.DocNo == docNo && x.Id != doc.Id))
                    throw new InvalidOperationException($"Số phiếu {docNo} đã tồn tại.");

                doc.DocType = input.DocType;
                doc.DocNo = docNo;
                doc.DocDate = input.DocDate.Date;
                doc.Reason = Clean(input.Reason);
                doc.WarehouseId = input.WarehouseId;
                doc.ToWarehouseId = input.DocType == KhoDocTypes.Transfer ? input.ToWarehouseId : null;
                doc.PartnerId = input.PartnerId;
                doc.PartnerName = Clean(input.PartnerName);
                doc.ContactName = Clean(input.ContactName);
                doc.ContraAccount = Clean(input.ContraAccount);
                doc.VatAccount = input.DocType == KhoDocTypes.In ? Clean(input.VatAccount) : null;
                doc.InvoiceNo = Clean(input.InvoiceNo);
                doc.InvoiceDate = input.InvoiceDate?.Date;
                doc.Description = Clean(input.Description);

                if (!isNew)
                {
                    var old = await db.PhieuKhoChiTiets.Where(x => x.PhieuKhoId == doc.Id).ToListAsync();
                    db.PhieuKhoChiTiets.RemoveRange(old);
                }
                var no = 1;
                foreach (var l in lines)
                {
                    db.PhieuKhoChiTiets.Add(new PhieuKhoChiTiet
                    {
                        Id = Guid.NewGuid(),
                        PhieuKhoId = doc.Id,
                        LineNo = no++,
                        VatTuId = l.VatTuId,
                        InventoryAccount = l.InventoryAccount,
                        ContraAccount = l.ContraAccount,
                        Quantity = l.Quantity,
                        UnitCost = l.UnitCost,
                        Amount = l.Amount,
                        VatRate = l.VatRate,
                        VatAmount = l.VatAmount,
                        Note = l.Note
                    });
                }
                await db.SaveChangesAsync();
                return doc.Id;
            }
            finally
            {
                Gate.Release();
            }
        }

        public async Task DeleteDraftAsync(Guid id)
        {
            await Gate.WaitAsync();
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var doc = await db.PhieuKhos.FirstOrDefaultAsync(x => x.Id == id)
                          ?? throw new InvalidOperationException("Không tìm thấy phiếu.");
                if (doc.Status == KhoStatus.Posted)
                    throw new InvalidOperationException($"Phiếu {doc.DocNo} đã ghi sổ — bỏ ghi sổ trước khi xóa.");
                await AccountingPeriodLock.EnsureOpenAsync(db, doc.DocDate, "xóa phiếu kho");
                db.PhieuKhoChiTiets.RemoveRange(await db.PhieuKhoChiTiets.Where(x => x.PhieuKhoId == id).ToListAsync());
                db.PhieuKhos.Remove(doc);
                await db.SaveChangesAsync();
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>Ghi sổ phiếu: đưa vào tồn kho, tính giá xuất (bình quân tức thời), sinh bút toán sổ cái.</summary>
        public async Task<KhoPostResult> PostAsync(Guid id, string user)
        {
            await Gate.WaitAsync();
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                await using var tx = await db.Database.BeginTransactionAsync();

                var doc = await db.PhieuKhos.FirstOrDefaultAsync(x => x.Id == id)
                          ?? throw new InvalidOperationException("Không tìm thấy phiếu.");
                if (doc.Status == KhoStatus.Posted)
                    throw new InvalidOperationException($"Phiếu {doc.DocNo} đã ghi sổ.");
                await AccountingPeriodLock.EnsureOpenAsync(db, doc.DocDate, $"ghi sổ phiếu kho {doc.DocNo}");

                var lines = await db.PhieuKhoChiTiets.Where(x => x.PhieuKhoId == id).OrderBy(x => x.LineNo).ToListAsync();
                await NormalizeAndValidateAsync(db, doc, lines);

                doc.Status = KhoStatus.Posted;
                doc.PostedAt = DateTime.Now;
                doc.PostedBy = user;

                var result = new KhoPostResult { DocNo = doc.DocNo };
                var itemIds = lines.Select(l => l.VatTuId).Distinct().ToList();
                var changed = await RecostAsync(db, itemIds, forceDocs: new HashSet<Guid> { doc.Id }, user, result);
                await db.SaveChangesAsync();
                await tx.CommitAsync();

                result.VoucherNo = await db.AccountingVouchers.AsNoTracking()
                    .Where(x => x.Id == doc.VoucherId).Select(x => x.VoucherNo).FirstOrDefaultAsync();
                _ = changed;
                return result;
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>Bỏ ghi sổ: xóa bút toán, phiếu về nháp, tính lại giá các phiếu xuất sau nó.</summary>
        public async Task<KhoPostResult> UnpostAsync(Guid id, string user)
        {
            await Gate.WaitAsync();
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                await using var tx = await db.Database.BeginTransactionAsync();

                var doc = await db.PhieuKhos.FirstOrDefaultAsync(x => x.Id == id)
                          ?? throw new InvalidOperationException("Không tìm thấy phiếu.");
                if (doc.Status != KhoStatus.Posted)
                    throw new InvalidOperationException($"Phiếu {doc.DocNo} chưa ghi sổ.");
                await AccountingPeriodLock.EnsureOpenAsync(db, doc.DocDate, $"bỏ ghi sổ phiếu kho {doc.DocNo}");

                await RemoveVoucherAsync(db, doc);
                doc.Status = KhoStatus.Draft;
                doc.PostedAt = null;
                doc.PostedBy = null;
                doc.UpdatedAt = DateTime.Now;
                doc.UpdatedBy = user;

                var itemIds = await db.PhieuKhoChiTiets.Where(x => x.PhieuKhoId == id).Select(x => x.VatTuId).Distinct().ToListAsync();
                var result = new KhoPostResult { DocNo = doc.DocNo };
                await RecostAsync(db, itemIds, forceDocs: new HashSet<Guid>(), user, result);
                await db.SaveChangesAsync();
                await tx.CommitAsync();
                return result;
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>Tính lại giá xuất toàn bộ (vd sau khi sửa dữ liệu bằng tay). Chỉ cập nhật phiếu có giá thay đổi.</summary>
        public async Task<KhoPostResult> RecostAllAsync(string user)
        {
            await Gate.WaitAsync();
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                await using var tx = await db.Database.BeginTransactionAsync();
                var itemIds = await (from l in db.PhieuKhoChiTiets
                                     join d in db.PhieuKhos on l.PhieuKhoId equals d.Id
                                     where d.Status == KhoStatus.Posted
                                     select l.VatTuId).Distinct().ToListAsync();
                var result = new KhoPostResult { DocNo = "" };
                await RecostAsync(db, itemIds, forceDocs: new HashSet<Guid>(), user, result);
                await db.SaveChangesAsync();
                await tx.CommitAsync();
                return result;
            }
            finally
            {
                Gate.Release();
            }
        }

        /// <summary>Tồn (SL, giá trị) của các vật tư tại 1 kho tính đến hết ngày — theo phiếu đã ghi sổ.</summary>
        public async Task<Dictionary<Guid, KhoStockHint>> StockAtAsync(DateTime date, Guid warehouseId, IReadOnlyCollection<Guid> itemIds, Guid? excludeDocId = null)
        {
            var result = new Dictionary<Guid, KhoStockHint>();
            if (itemIds.Count == 0 || warehouseId == Guid.Empty) return result;
            await using var db = await dbFactory.CreateDbContextAsync();
            var moves = await LoadMovesAsync(db, itemIds.ToList(), until: date.Date.AddDays(1));
            if (excludeDocId.HasValue) moves = moves.Where(m => m.DocId != excludeDocId.Value).ToList();
            var run = KhoCostingEngine.Run(moves);
            foreach (var item in itemIds)
                result[item] = run.Closing.TryGetValue((item, warehouseId), out var s) ? new KhoStockHint(s.Qty, s.Value) : new KhoStockHint(0, 0);
            return result;
        }

        // ─────────── Tính lại giá + đồng bộ sổ cái ───────────

        /// <summary>
        /// Chạy lại tính giá cho các vật tư, cập nhật đơn giá / thành tiền dòng xuất, chuyển kho đã ghi sổ bị thay đổi,
        /// sinh lại bút toán cho các phiếu đó và cho <paramref name="forceDocs"/>. Thiếu hàng → báo lỗi (không lưu gì).
        /// </summary>
        private static async Task<HashSet<Guid>> RecostAsync(AppDbContext db, List<Guid> itemIds, HashSet<Guid> forceDocs, string user, KhoPostResult result)
        {
            var changedDocs = new HashSet<Guid>(forceDocs);
            if (itemIds.Count == 0) return changedDocs;

            // Dòng của mọi phiếu đã ghi sổ (kể cả phiếu đang được ghi sổ — trạng thái đã đổi trong context).
            await db.SaveChangesAsync(); // đẩy trạng thái phiếu hiện tại để truy vấn thấy (vẫn trong transaction)
            var moves = await LoadMovesAsync(db, itemIds, until: null);
            var run = KhoCostingEngine.Run(moves);

            if (run.Shortages.Count > 0)
            {
                var items = (await db.KhoVatTus.AsNoTracking().Where(x => itemIds.Contains(x.Id)).ToListAsync()).ToDictionary(x => x.Id);
                var whs = (await db.KhoHangs.AsNoTracking().ToListAsync()).ToDictionary(x => x.Id);
                var msgs = run.Shortages.Take(8).Select(s =>
                    $"• {s.DocNo} ngày {s.DocDate:dd/MM/yyyy}: {(items.TryGetValue(s.ItemId, out var it) ? it.Code : s.ItemId.ToString())} " +
                    $"tại kho {(whs.TryGetValue(s.WarehouseId, out var w) ? w.Code : "?")} xuất {s.Requested:#,##0.####} nhưng chỉ còn {s.Available:#,##0.####}");
                throw new InvalidOperationException(
                    "Không đủ hàng trong kho (tồn kho sẽ bị âm):\n" + string.Join("\n", msgs) +
                    (run.Shortages.Count > 8 ? $"\n... và {run.Shortages.Count - 8} trường hợp khác." : "") +
                    "\nKiểm tra lại ngày chứng từ, số lượng, hoặc ghi sổ phiếu nhập trước.");
            }

            var lineIds = run.Costs.Keys.ToList();
            var tracked = new List<PhieuKhoChiTiet>();
            foreach (var chunk in lineIds.Chunk(1000))
                tracked.AddRange(await db.PhieuKhoChiTiets.Where(x => chunk.Contains(x.Id)).ToListAsync());
            foreach (var line in tracked)
            {
                var c = run.Costs[line.Id];
                if (line.Amount != c.Amount || line.UnitCost != c.UnitCost)
                {
                    line.Amount = c.Amount;
                    line.UnitCost = c.UnitCost;
                    changedDocs.Add(line.PhieuKhoId);
                }
            }

            var docs = new List<PhieuKho>();
            foreach (var chunk in changedDocs.Chunk(1000))
                docs.AddRange(await db.PhieuKhos.Where(x => chunk.Contains(x.Id)).ToListAsync());
            var others = docs.Where(d => !forceDocs.Contains(d.Id)).ToList();
            if (others.Count > 0)
            {
                await AccountingPeriodLock.EnsureOpenAsync(db, others.Select(d => (DateTime?)d.DocDate),
                    "cập nhật lại giá xuất (bình quân tức thời) cho phiếu " + string.Join(", ", others.Select(d => d.DocNo).Take(5)) + " —");
                result.RecostedDocs = others.Count;
                result.RecostedDocNos = others.OrderBy(d => d.DocDate).Select(d => d.DocNo).ToList();
            }

            var accounts = await LoadAccountIdsAsync(db);
            foreach (var d in docs.Where(d => d.Status == KhoStatus.Posted))
                result.GlLines += await RebuildVoucherAsync(db, d, accounts, user);
            return changedDocs;
        }

        private static async Task<List<KhoMove>> LoadMovesAsync(AppDbContext db, List<Guid> itemIds, DateTime? until)
        {
            var list = new List<KhoMove>();
            foreach (var chunk in itemIds.Chunk(500))
            {
                var q = from l in db.PhieuKhoChiTiets.AsNoTracking()
                        join d in db.PhieuKhos.AsNoTracking() on l.PhieuKhoId equals d.Id
                        where d.Status == KhoStatus.Posted && chunk.Contains(l.VatTuId)
                        select new { d, l };
                if (until.HasValue) q = q.Where(x => x.d.DocDate < until.Value);
                var rows = await q.ToListAsync();
                list.AddRange(rows.Select(x => new KhoMove
                {
                    DocId = x.d.Id,
                    LineId = x.l.Id,
                    DocType = x.d.DocType,
                    DocNo = x.d.DocNo,
                    DocDate = x.d.DocDate,
                    CreatedAt = x.d.CreatedAt,
                    LineNo = x.l.LineNo,
                    ItemId = x.l.VatTuId,
                    WarehouseId = x.d.WarehouseId,
                    ToWarehouseId = x.d.ToWarehouseId,
                    Quantity = x.l.Quantity,
                    Amount = x.l.Amount
                }));
            }
            return list;
        }

        private static async Task<Dictionary<string, Guid>> LoadAccountIdsAsync(AppDbContext db)
        {
            var rows = await db.DanhMucTaiKhoan.AsNoTracking()
                .Where(x => x.Taikhoan != null && x.Taikhoan != "")
                .Select(x => new { x.Id, x.Taikhoan })
                .ToListAsync();
            var map = new Dictionary<string, Guid>(StringComparer.Ordinal);
            foreach (var r in rows.OrderBy(r => r.Id))
            {
                var code = r.Taikhoan!.Trim();
                if (!map.ContainsKey(code)) map[code] = r.Id;
            }
            return map;
        }

        private static async Task RemoveVoucherAsync(AppDbContext db, PhieuKho doc)
        {
            var sourceId = doc.Id.ToString();
            var vouchers = await db.AccountingVouchers
                .Where(v => (doc.VoucherId.HasValue && v.Id == doc.VoucherId) || (v.SourceModule == SourceModule && v.SourceId == sourceId))
                .ToListAsync();
            foreach (var v in vouchers)
            {
                db.GeneralLedgerEntries.RemoveRange(await db.GeneralLedgerEntries.Where(g => g.VoucherId == v.Id).ToListAsync());
                db.AccountingVoucherLines.RemoveRange(await db.AccountingVoucherLines.Where(l => l.VoucherId == v.Id).ToListAsync());
                db.AccountingVouchers.Remove(v);
            }
            doc.VoucherId = null;
        }

        /// <summary>Xóa chứng từ cũ của phiếu và sinh lại theo số liệu hiện tại. Trả về số dòng sổ cái.</summary>
        private static async Task<int> RebuildVoucherAsync(AppDbContext db, PhieuKho doc, Dictionary<string, Guid> accountIds, string user)
        {
            await RemoveVoucherAsync(db, doc);
            if (doc.DocType is not (KhoDocTypes.In or KhoDocTypes.Out)) return 0;

            // Truy vấn có theo dõi: dòng nào đã nạp (và vừa được tính lại giá) thì EF trả đúng đối tượng đang sửa.
            var lines = (await db.PhieuKhoChiTiets.Where(x => x.PhieuKhoId == doc.Id).ToListAsync())
                .OrderBy(x => x.LineNo).ToList();
            var items = (await db.KhoVatTus.AsNoTracking().Where(x => lines.Select(l => l.VatTuId).Contains(x.Id)).ToListAsync())
                .ToDictionary(x => x.Id);

            var entries = KhoPosting.BuildEntries(doc, lines, items);
            if (entries.Count == 0) return 0;

            var voucherId = Guid.NewGuid();
            var now = DateTime.Now;
            var desc = Clean(doc.Description) ?? $"{KhoDocTypes.Label(doc.DocType)} {doc.DocNo}";
            db.AccountingVouchers.Add(new M_AccountingVouchers
            {
                Id = voucherId,
                CompanyId = Guid.Empty,
                VoucherNo = doc.DocNo,
                VoucherDate = doc.DocDate,
                PostingDate = doc.DocDate,
                FiscalYear = doc.DocDate.Year,
                FiscalPeriod = doc.DocDate.Month,
                TransactionTypeCode = "GENERAL",
                Description = desc,
                CurrencyCode = "VND",
                ExchangeRate = 1,
                Status = 2,
                ReferenceNo = doc.InvoiceNo,
                ReferenceDate = doc.InvoiceDate,
                BookScope = "BOTH",
                SourceModule = SourceModule,
                SourceId = doc.Id.ToString(),
                CreatedBy = user,
                CreatedDate = now,
                PostedBy = user,
                PostedDate = now,
                Ghiso = true,
                Approve = true,
                Both = true
            });

            var no = 1;
            foreach (var e in entries)
            {
                if (!accountIds.TryGetValue(e.Account, out var accId))
                    throw new InvalidOperationException($"Phiếu {doc.DocNo}: TK {e.Account} chưa có trong danh mục tài khoản — thêm TK trước khi ghi sổ.");
                var lineId = Guid.NewGuid();
                db.AccountingVoucherLines.Add(new M_AccountingVoucherLines
                {
                    Id = lineId,
                    VoucherId = voucherId,
                    LineNo_ = no.ToString(),
                    SortKey = no,
                    DanhMucTaiKhoanID = accId,
                    AccountCode = e.Account,
                    DebitAmount = e.Debit,
                    CreditAmount = e.Credit,
                    DebitAmountFC = e.Debit,
                    CreditAmountFC = e.Credit,
                    LineDescription = e.Description,
                    CustomerId = e.PartnerId,
                    IsTaxBook = true,
                    IsManagementBook = true,
                    LedgerType = "BOTH",
                    InvoiceNo = doc.InvoiceNo,
                    InvoiceDate = doc.InvoiceDate
                });
                db.GeneralLedgerEntries.Add(new M_GeneralLedgerEntries
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    VoucherLineId = lineId,
                    CompanyId = Guid.Empty,
                    FiscalYear = doc.DocDate.Year,
                    FiscalPeriod = doc.DocDate.Month,
                    PostingDate = doc.DocDate,
                    VoucherDate = doc.DocDate,
                    VoucherNo = doc.DocNo,
                    AccountCode = e.Account,
                    Debit = e.Debit,
                    Credit = e.Credit,
                    CurrencyCode = "VND",
                    ExchangeRate = 1,
                    DebitFC = e.Debit,
                    CreditFC = e.Credit,
                    CustomerId = e.PartnerId,
                    Description = e.Description,
                    SourceModule = "ACCOUNTING_VOUCHER",
                    SourceId = voucherId.ToString(),
                    CreatedAt = now,
                    CreatedBy = user,
                    IsTaxBook = true,
                    IsManagementBook = true,
                    LedgerType = "BOTH",
                    SoHD = doc.InvoiceNo
                });
                no++;
            }
            doc.VoucherId = voucherId;
            return entries.Count;
        }

        // ─────────── Kiểm tra dữ liệu phiếu ───────────

        private static async Task<List<PhieuKhoChiTiet>> NormalizeAndValidateAsync(AppDbContext db, PhieuKho doc, IReadOnlyList<PhieuKhoChiTiet> input)
        {
            var errors = new List<string>();
            if (!KhoDocTypes.All.Contains(doc.DocType)) throw new InvalidOperationException("Loại phiếu không hợp lệ.");
            if (doc.DocDate == default) throw new InvalidOperationException("Chưa nhập ngày chứng từ.");
            await AccountingPeriodLock.EnsureOpenAsync(db, doc.DocDate, "lập / sửa phiếu kho");

            var whs = (await db.KhoHangs.AsNoTracking().ToListAsync()).ToDictionary(x => x.Id);
            if (!whs.ContainsKey(doc.WarehouseId)) errors.Add("Chưa chọn kho.");
            if (doc.DocType == KhoDocTypes.Transfer)
            {
                if (!doc.ToWarehouseId.HasValue || !whs.ContainsKey(doc.ToWarehouseId.Value)) errors.Add("Chưa chọn kho nhận.");
                else if (doc.ToWarehouseId == doc.WarehouseId) errors.Add("Kho nhận phải khác kho xuất.");
            }

            var accounts = await LoadAccountsAsync(db);
            bool AccountOk(string acc) => accounts.Count == 0 || accounts.ContainsKey(acc);

            var needContra = doc.DocType is KhoDocTypes.In or KhoDocTypes.Out;
            var docContra = Clean(doc.ContraAccount);
            if (docContra is not null && !AccountOk(docContra)) errors.Add($"TK đối ứng {docContra} chưa có trong danh mục tài khoản.");
            if (doc.DocType == KhoDocTypes.In)
            {
                var vat = Clean(doc.VatAccount);
                if (vat is not null && !AccountOk(vat)) errors.Add($"TK thuế {vat} chưa có trong danh mục tài khoản.");
            }

            var itemIds = input.Select(l => l.VatTuId).Distinct().ToList();
            var items = (await db.KhoVatTus.AsNoTracking().Where(x => itemIds.Contains(x.Id)).ToListAsync()).ToDictionary(x => x.Id);

            var result = new List<PhieuKhoChiTiet>();
            var userPriced = KhoDocTypes.UserPriced(doc.DocType);
            for (var i = 0; i < input.Count; i++)
            {
                var l = input[i];
                var n = $"Dòng {i + 1}";
                if (l.VatTuId == Guid.Empty && l.Quantity == 0 && l.Amount == 0) continue; // dòng trống
                if (!items.TryGetValue(l.VatTuId, out var item)) { errors.Add($"{n}: chưa chọn vật tư."); continue; }
                n = $"Dòng {i + 1} ({item.Code})";
                var invAcc = Clean(l.InventoryAccount) ?? item.InventoryAccount.Trim();
                if (!KhoAccounts.IsInventory(invAcc)) errors.Add($"{n}: TK kho {invAcc} phải là 151–156.");
                else if (!AccountOk(invAcc)) errors.Add($"{n}: TK kho {invAcc} chưa có trong danh mục tài khoản.");
                var contra = Clean(l.ContraAccount);
                if (contra is not null && !AccountOk(contra)) errors.Add($"{n}: TK đối ứng {contra} chưa có trong danh mục tài khoản.");
                if (needContra && contra is null && docContra is null) errors.Add($"{n}: chưa có TK đối ứng (nhập ở phiếu hoặc ở dòng).");
                if (l.Quantity < 0) errors.Add($"{n}: số lượng không được âm.");

                var qty = decimal.Round(l.Quantity, 4, MidpointRounding.AwayFromZero);
                decimal unitCost = l.UnitCost, amount = l.Amount, vatRate = l.VatRate, vatAmount = l.VatAmount;
                if (userPriced)
                {
                    if (amount == 0 && unitCost != 0) amount = qty * unitCost;
                    amount = decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
                    if (qty == 0 && (doc.DocType == KhoDocTypes.Opening || amount == 0))
                        errors.Add($"{n}: số lượng phải lớn hơn 0.");
                    if (amount < 0) errors.Add($"{n}: thành tiền không được âm.");
                    unitCost = qty == 0 ? 0 : decimal.Round(amount / qty, 4, MidpointRounding.AwayFromZero);
                    if (doc.DocType == KhoDocTypes.In)
                    {
                        if (vatRate < 0 || vatRate > 100) errors.Add($"{n}: thuế suất không hợp lệ.");
                        if (vatAmount == 0 && vatRate > 0) vatAmount = amount * vatRate / 100m;
                        vatAmount = decimal.Round(vatAmount, 0, MidpointRounding.AwayFromZero);
                    }
                    else { vatRate = 0; vatAmount = 0; }
                }
                else
                {
                    if (qty <= 0) errors.Add($"{n}: số lượng xuất phải lớn hơn 0.");
                    vatRate = 0; vatAmount = 0; // giá xuất do phần mềm tính khi ghi sổ
                }

                result.Add(new PhieuKhoChiTiet
                {
                    Id = l.Id == Guid.Empty ? Guid.NewGuid() : l.Id,
                    PhieuKhoId = doc.Id,
                    LineNo = l.LineNo,
                    VatTuId = l.VatTuId,
                    InventoryAccount = invAcc,
                    ContraAccount = contra,
                    Quantity = qty,
                    UnitCost = unitCost,
                    Amount = amount,
                    VatRate = vatRate,
                    VatAmount = vatAmount,
                    Note = Clean(l.Note)
                });
            }
            if (result.Count == 0) errors.Add("Phiếu chưa có dòng hàng nào.");
            if (errors.Count > 0)
                throw new InvalidOperationException(string.Join("\n", errors.Distinct().Take(12)));

            // Khi ghi sổ: cập nhật lại số đã chuẩn hóa vào các dòng đang theo dõi
            foreach (var l in input)
            {
                var n = result.FirstOrDefault(r => r.Id == l.Id);
                if (n is null || db.Entry(l).State == EntityState.Detached) continue;
                l.InventoryAccount = n.InventoryAccount;
                l.ContraAccount = n.ContraAccount;
                l.Quantity = n.Quantity;
                l.UnitCost = n.UnitCost;
                l.Amount = n.Amount;
                l.VatRate = n.VatRate;
                l.VatAmount = n.VatAmount;
            }
            return result;
        }

        private static async Task EnsureAccountExistsAsync(AppDbContext db, string account, string label)
        {
            var accounts = await LoadAccountsAsync(db);
            if (accounts.Count > 0 && !accounts.ContainsKey(account))
                throw new InvalidOperationException($"{label} {account} chưa có trong danh mục tài khoản — thêm TK trước.");
        }

        // ═════════════════════════ Báo cáo ═════════════════════════

        private sealed record ReportMove(Guid DocId, string DocType, string DocNo, DateTime DocDate, DateTime CreatedAt, int LineNo,
            Guid WarehouseId, Guid? ToWarehouseId, string? Reason, string? Description, string? PartnerName, string? DocContra,
            Guid ItemId, string Account, string? LineContra, decimal Quantity, decimal UnitCost, decimal Amount);

        private static async Task<List<ReportMove>> LoadReportMovesAsync(AppDbContext db, DateTime toExclusive, Guid? itemId)
        {
            var q = from l in db.PhieuKhoChiTiets.AsNoTracking()
                    join d in db.PhieuKhos.AsNoTracking() on l.PhieuKhoId equals d.Id
                    where d.Status == KhoStatus.Posted && d.DocDate < toExclusive
                    select new { d, l };
            if (itemId.HasValue) q = q.Where(x => x.l.VatTuId == itemId);
            var rows = await q.ToListAsync();
            return rows.Select(x => new ReportMove(x.d.Id, x.d.DocType, x.d.DocNo, x.d.DocDate, x.d.CreatedAt, x.l.LineNo,
                    x.d.WarehouseId, x.d.ToWarehouseId, x.d.Reason, x.d.Description, x.d.PartnerName, x.d.ContraAccount,
                    x.l.VatTuId, x.l.InventoryAccount, x.l.ContraAccount, x.l.Quantity, x.l.UnitCost, x.l.Amount))
                .ToList();
        }

        /// <summary>
        /// Chiều của 1 dòng đối với phạm vi kho đang xem: +1 nhập, −1 xuất, 0 không tính.
        /// Xem tất cả kho: chuyển kho nội bộ không tính (vừa nhập vừa xuất bằng nhau).
        /// </summary>
        private static IEnumerable<int> Directions(ReportMove m, Guid? warehouseId)
        {
            switch (m.DocType)
            {
                case KhoDocTypes.Opening:
                case KhoDocTypes.In:
                    if (!warehouseId.HasValue || m.WarehouseId == warehouseId) yield return 1;
                    break;
                case KhoDocTypes.Out:
                    if (!warehouseId.HasValue || m.WarehouseId == warehouseId) yield return -1;
                    break;
                case KhoDocTypes.Transfer:
                    if (!warehouseId.HasValue) yield break;
                    if (m.WarehouseId == warehouseId) yield return -1;
                    if (m.ToWarehouseId == warehouseId) yield return 1;
                    break;
            }
        }

        /// <summary>Tổng hợp nhập xuất tồn (S11-DN). Phiếu tồn đầu kỳ nằm trong kỳ được cộng vào cột tồn đầu.</summary>
        public async Task<List<KhoNxtRow>> SummaryAsync(DateTime from, DateTime to, Guid? warehouseId, string? accountRoot)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var toEx = to.Date.AddDays(1);
            var moves = await LoadReportMovesAsync(db, toEx, null);
            var items = (await db.KhoVatTus.AsNoTracking().ToListAsync()).ToDictionary(x => x.Id);

            var rows = new Dictionary<(Guid, string), KhoNxtRow>();
            KhoNxtRow Row(Guid itemId, string account)
            {
                var key = (itemId, account);
                if (rows.TryGetValue(key, out var r)) return r;
                items.TryGetValue(itemId, out var it);
                r = new KhoNxtRow
                {
                    ItemId = itemId,
                    Code = it?.Code ?? "?",
                    Name = it?.Name ?? "(vật tư đã xóa)",
                    Unit = it?.Unit ?? "",
                    Category = it?.Category ?? "",
                    MinQty = it?.MinQty,
                    Account = account
                };
                rows[key] = r;
                return r;
            }

            foreach (var m in moves)
            {
                var acc = (m.Account ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(accountRoot) && !acc.StartsWith(accountRoot.Trim(), StringComparison.Ordinal)) continue;
                foreach (var dir in Directions(m, warehouseId))
                {
                    var r = Row(m.ItemId, acc);
                    var opening = m.DocDate < from.Date || m.DocType == KhoDocTypes.Opening;
                    if (opening)
                    {
                        r.OpenQty += dir * m.Quantity;
                        r.OpenValue += dir * m.Amount;
                    }
                    else if (dir > 0)
                    {
                        r.InQty += m.Quantity;
                        r.InValue += m.Amount;
                    }
                    else
                    {
                        r.OutQty += m.Quantity;
                        r.OutValue += m.Amount;
                    }
                }
            }
            return rows.Values
                .OrderBy(r => r.Account, StringComparer.Ordinal)
                .ThenBy(r => r.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Sổ chi tiết vật tư / thẻ kho (S10-DN) của 1 vật tư.</summary>
        public async Task<KhoCard> StockCardAsync(Guid itemId, Guid? warehouseId, DateTime from, DateTime to)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var item = await db.KhoVatTus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemId)
                       ?? throw new InvalidOperationException("Không tìm thấy vật tư.");
            var whs = (await db.KhoHangs.AsNoTracking().ToListAsync()).ToDictionary(x => x.Id);
            var moves = await LoadReportMovesAsync(db, to.Date.AddDays(1), itemId);

            var card = new KhoCard { ItemId = item.Id, Code = item.Code, Name = item.Name, Unit = item.Unit, Account = item.InventoryAccount };
            var ordered = moves
                .OrderBy(m => m.DocDate.Date).ThenBy(m => KhoDocTypes.Rank(m.DocType)).ThenBy(m => m.CreatedAt)
                .ThenBy(m => m.DocNo, StringComparer.Ordinal).ThenBy(m => m.LineNo);
            foreach (var m in ordered)
            {
                foreach (var dir in Directions(m, warehouseId))
                {
                    if (m.DocDate < from.Date || m.DocType == KhoDocTypes.Opening)
                    {
                        card.OpenQty += dir * m.Quantity;
                        card.OpenValue += dir * m.Amount;
                        continue;
                    }
                    var wh = whs.TryGetValue(m.WarehouseId, out var w) ? w.Code : "";
                    if (m.DocType == KhoDocTypes.Transfer)
                        wh = $"{wh} → {(m.ToWarehouseId.HasValue && whs.TryGetValue(m.ToWarehouseId.Value, out var t) ? t.Code : "")}";
                    var reason = KhoReasons.Find(m.Reason)?.Label;
                    card.Lines.Add(new KhoCardLine
                    {
                        DocId = m.DocId,
                        Date = m.DocDate,
                        DocNo = m.DocNo,
                        DocType = m.DocType,
                        Description = FirstNonEmpty(m.Description, reason, KhoDocTypes.Label(m.DocType))
                                      + (string.IsNullOrWhiteSpace(m.PartnerName) ? "" : $" - {m.PartnerName}"),
                        Warehouse = wh,
                        Contra = m.DocType == KhoDocTypes.Transfer ? m.Account : (Clean(m.LineContra) ?? Clean(m.DocContra) ?? ""),
                        UnitCost = m.UnitCost,
                        InQty = dir > 0 ? m.Quantity : 0,
                        InValue = dir > 0 ? m.Amount : 0,
                        OutQty = dir < 0 ? m.Quantity : 0,
                        OutValue = dir < 0 ? m.Amount : 0
                    });
                }
            }
            decimal q = card.OpenQty, v = card.OpenValue;
            foreach (var l in card.Lines)
            {
                q += l.InQty - l.OutQty;
                v += l.InValue - l.OutValue;
                l.BalQty = q;
                l.BalValue = v;
            }
            return card;
        }

        /// <summary>Đối chiếu tồn kho với sổ cái theo từng TK kho, đến hết ngày <paramref name="to"/>.</summary>
        public async Task<List<KhoGlCheck>> ReconcileAsync(DateTime to)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var toEx = to.Date.AddDays(1);
            var moves = await LoadReportMovesAsync(db, toEx, null);
            var names = await LoadAccountsAsync(db);

            var checks = new Dictionary<string, KhoGlCheck>(StringComparer.Ordinal);
            KhoGlCheck Get(string acc)
            {
                if (!checks.TryGetValue(acc, out var c))
                    checks[acc] = c = new KhoGlCheck
                    {
                        Account = acc,
                        AccountName = names.TryGetValue(acc, out var n) && n.Length > 0 ? n : KhoAccounts.RootName(KhoAccounts.Root(acc))
                    };
                return c;
            }
            foreach (var m in moves)
            {
                var acc = (m.Account ?? "").Trim();
                foreach (var dir in Directions(m, null))
                {
                    Get(acc).StockValue += dir * m.Amount;
                    if (m.DocType == KhoDocTypes.Opening) Get(acc).OpeningDocs += m.Amount;
                }
            }
            if (checks.Count == 0) return new List<KhoGlCheck>();

            var accs = checks.Keys.ToList();
            var gl = await db.GeneralLedgerEntries.AsNoTracking()
                .Where(g => g.PostingDate < toEx && g.AccountCode != null
                            && (g.AccountCode.StartsWith("151") || g.AccountCode.StartsWith("152") || g.AccountCode.StartsWith("153")
                                || g.AccountCode.StartsWith("154") || g.AccountCode.StartsWith("155") || g.AccountCode.StartsWith("156")))
                .Select(g => new { g.AccountCode, g.Debit, g.Credit, g.VoucherId })
                .ToListAsync();
            var stockVoucherIds = (await db.AccountingVouchers.AsNoTracking()
                    .Where(v => v.SourceModule == SourceModule)
                    .Select(v => v.Id).ToListAsync())
                .ToHashSet();
            foreach (var g in gl)
            {
                var code = g.AccountCode.Trim();
                // Gán vào TK kho khớp dài nhất (vd 1561 → 1561; 156 → 156)
                var target = accs.Where(a => code == a || code.StartsWith(a, StringComparison.Ordinal))
                    .OrderByDescending(a => a.Length).FirstOrDefault();
                if (target is null) continue;
                Get(target).GlBalance += g.Debit - g.Credit;
                if (stockVoucherIds.Contains(g.VoucherId)) Get(target).GlFromStock += g.Debit - g.Credit;
            }
            return checks.Values.OrderBy(c => c.Account, StringComparer.Ordinal).ToList();
        }

        // ─────────── Tiện ích ───────────

        private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string FirstNonEmpty(params string?[] values) =>
            values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? "";
    }

    /// <summary>Lập bút toán cho phiếu nhập / xuất (tách riêng để kiểm thử).</summary>
    public static class KhoPosting
    {
        public sealed record Entry(string Account, decimal Debit, decimal Credit, string Description, Guid? PartnerId);

        public static List<Entry> BuildEntries(PhieuKho doc, IEnumerable<PhieuKhoChiTiet> lines, IReadOnlyDictionary<Guid, KhoVatTu> items)
        {
            var list = new List<Entry>();
            var docContra = string.IsNullOrWhiteSpace(doc.ContraAccount) ? null : doc.ContraAccount.Trim();
            string Name(PhieuKhoChiTiet l) => items.TryGetValue(l.VatTuId, out var it) ? $"{it.Code} - {it.Name}" : l.VatTuId.ToString();
            string Qty(PhieuKhoChiTiet l) => items.TryGetValue(l.VatTuId, out var it) && !string.IsNullOrWhiteSpace(it.Unit)
                ? $"{l.Quantity:#,##0.####} {it.Unit}" : $"{l.Quantity:#,##0.####}";

            if (doc.DocType == KhoDocTypes.In)
            {
                var vatByContra = new Dictionary<string, decimal>(StringComparer.Ordinal);
                foreach (var l in lines)
                {
                    var contra = string.IsNullOrWhiteSpace(l.ContraAccount) ? docContra : l.ContraAccount.Trim();
                    if (contra is null) throw new InvalidOperationException($"Phiếu {doc.DocNo} dòng {l.LineNo}: chưa có TK đối ứng.");
                    var d = $"Nhập kho {Name(l)} x {Qty(l)}";
                    if (l.Amount != 0)
                    {
                        list.Add(new Entry(l.InventoryAccount.Trim(), l.Amount, 0, d, null));
                        list.Add(new Entry(contra, 0, l.Amount, d, doc.PartnerId));
                    }
                    if (l.VatAmount != 0) vatByContra[contra] = vatByContra.GetValueOrDefault(contra) + l.VatAmount;
                }
                var vatAcc = string.IsNullOrWhiteSpace(doc.VatAccount) ? "1331" : doc.VatAccount.Trim();
                foreach (var (contra, vat) in vatByContra)
                {
                    var d = $"Thuế GTGT đầu vào - {doc.DocNo}" + (string.IsNullOrWhiteSpace(doc.InvoiceNo) ? "" : $" HĐ {doc.InvoiceNo}");
                    list.Add(new Entry(vatAcc, vat, 0, d, null));
                    list.Add(new Entry(contra, 0, vat, d, doc.PartnerId));
                }
            }
            else if (doc.DocType == KhoDocTypes.Out)
            {
                foreach (var l in lines)
                {
                    if (l.Amount == 0) continue;
                    var contra = string.IsNullOrWhiteSpace(l.ContraAccount) ? docContra : l.ContraAccount.Trim();
                    if (contra is null) throw new InvalidOperationException($"Phiếu {doc.DocNo} dòng {l.LineNo}: chưa có TK đối ứng.");
                    var d = $"Xuất kho {Name(l)} x {Qty(l)}";
                    list.Add(new Entry(contra, l.Amount, 0, d, doc.PartnerId));
                    list.Add(new Entry(l.InventoryAccount.Trim(), 0, l.Amount, d, null));
                }
            }
            return list;
        }
    }
}
