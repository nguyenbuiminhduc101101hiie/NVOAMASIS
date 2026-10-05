using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services.Accounting
{
    /// <summary>Chứng từ kế toán (chưa hủy) đang gắn với một phiếu thu / phiếu chi.</summary>
    public sealed record PhieuVoucherInfo(Guid VoucherId, string VoucherNo, int Status, bool Ghiso)
    {
        public bool IsPosted => Ghiso || Status == 2;
    }

    /// <summary>Kết quả tạo / hủy chứng từ cho một phiếu.</summary>
    public sealed record PhieuVoucherResult(bool Success, bool Changed, string Message, string? VoucherNo = null)
    {
        public static PhieuVoucherResult Fail(string message) => new(false, false, message);
        public static PhieuVoucherResult NoChange(string message = "") => new(true, false, message);
    }

    /// <summary>
    /// Sinh / hủy chứng từ kế toán (10.4.1) cho phiếu thu (10.1) và phiếu chi (10.2).
    /// <para>
    /// Dùng lại đúng thủ tục mà nút "Tạo Voucher" trên 10.1 / 10.2 đang gọi
    /// (dbo.usp_CreateAccountingVoucher_FromPhieuThu / _FromPhieuChi) để chứng từ sinh tự động
    /// giống hệt chứng từ kế toán tạo bằng tay. Khi gọi tự động thì truyền @AutoPost = 0 (chứng từ nháp,
    /// kế toán kiểm tra rồi bấm Ghi Sổ ở 10.4.1).
    /// </para>
    /// <para>
    /// Liên kết phiếu ↔ chứng từ: AccountingVouchers.SourceId = Id của phiếu (sổ cái 10.6 cũng tra ngược phiếu thu theo SourceId).
    /// </para>
    /// </summary>
    public class PhieuThuChiVoucherService(AppDbContext db, HistoryLogService historyLog)
    {
        public const string LoaiThu = "Thu";
        public const string LoaiChi = "Chi";

        /// <summary>Trạng thái chứng từ đã hủy (AccountingVouchers.Status).</summary>
        private const int StatusCancelled = 3;

        private const int InChunkSize = 1000;

        private sealed record Source(Guid Id, string Kind, string So, bool Approved, string? TkNo, string? TkCo, double Amount, DateTime? PostingDate);

        // ───────────────────────── Tạo chứng từ ─────────────────────────

        /// <summary>
        /// Tạo chứng từ kế toán cho một phiếu đã duyệt. Bỏ qua (không lỗi) nếu phiếu đã có chứng từ chưa hủy.
        /// </summary>
        /// <param name="autoPost">false = chứng từ nháp; true = ghi sổ luôn (như nút "Tạo Voucher").</param>
        public async Task<PhieuVoucherResult> CreateVoucherAsync(string loai, Guid phieuId, string user, bool autoPost = false)
        {
            try
            {
                db.ChangeTracker.Clear();
                var src = await LoadSourceAsync(loai, phieuId);
                if (src == null)
                    return PhieuVoucherResult.Fail("Không tìm thấy phiếu.");

                var label = $"phiếu {src.Kind} {src.So}";
                if (!src.Approved)
                    return PhieuVoucherResult.Fail($"{Cap(label)} chưa được duyệt.");
                if (string.IsNullOrWhiteSpace(src.TkNo) || string.IsNullOrWhiteSpace(src.TkCo))
                    return PhieuVoucherResult.Fail($"{Cap(label)} thiếu TK Nợ / TK Có.");
                if (src.Amount == 0)
                    return PhieuVoucherResult.Fail($"{Cap(label)} có số tiền bằng 0.");

                var existing = await GetVoucherInfoAsync(new[] { phieuId });
                if (existing.TryGetValue(phieuId, out var info))
                    return PhieuVoucherResult.NoChange($"{Cap(label)} đã có chứng từ {info.VoucherNo}.");

                await AccountingPeriodLock.EnsureOpenAsync(db, src.PostingDate, $"tạo chứng từ kế toán cho {label}");

                object companyId = DBNull.Value;
                if (loai == LoaiThu)
                {
                    // Giống nút "Tạo Voucher" của 10.1: phiếu thu truyền CompanyId của công ty
                    var cid = await db.CompanyInfomation.AsNoTracking().Select(x => x.CompanyID).FirstOrDefaultAsync();
                    if (cid == Guid.Empty)
                        return PhieuVoucherResult.Fail("Chưa khai báo thông tin công ty (1.13), không tạo được chứng từ.");
                    companyId = cid;
                }

                var voucherId = await ExecCreateProcedureAsync(loai, phieuId, companyId, user, autoPost);

                var created = await FindVoucherAsync(voucherId, phieuId);
                var voucherNo = created?.VoucherNo;

                try
                {
                    await historyLog.LogAsync(user,
                        autoPost ? "Create Voucher From Phieu" : "Auto Create Draft Voucher From Phieu",
                        loai == LoaiThu ? "Phieuthu" : "Phieuchi", phieuId, src.So,
                        new { VoucherId = created?.VoucherId ?? voucherId, VoucherNo = voucherNo, AutoPost = autoPost });
                }
                catch
                {
                    // ghi lịch sử lỗi không được làm hỏng việc tạo chứng từ
                }

                if (created == null)
                    return new PhieuVoucherResult(true, true, $"Đã gọi tạo chứng từ cho {label} nhưng chưa đọc lại được số chứng từ.");

                var state = created.IsPosted ? "đã ghi sổ" : "nháp";
                return new PhieuVoucherResult(true, true, $"Đã tạo chứng từ kế toán {state} {created.VoucherNo} cho {label}.", created.VoucherNo);
            }
            catch (AccountingPeriodClosedException ex)
            {
                return PhieuVoucherResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                return PhieuVoucherResult.Fail(ex.GetBaseException().Message);
            }
        }

        /// <summary>Tạo chứng từ nháp cho các phiếu đã duyệt chưa có chứng từ. Trả về số đã tạo, số bỏ qua và danh sách lỗi.</summary>
        public async Task<(int Created, int Skipped, List<string> Errors)> CreateDraftsAsync(string loai, IReadOnlyCollection<Guid> phieuIds, string user)
        {
            var created = 0;
            var skipped = 0;
            var errors = new List<string>();
            foreach (var id in phieuIds.Distinct())
            {
                var rs = await CreateVoucherAsync(loai, id, user, autoPost: false);
                if (!rs.Success)
                    errors.Add(rs.Message);
                else if (rs.Changed)
                    created++;
                else
                    skipped++;
            }

            return (created, skipped, errors);
        }

        // ───────────────────────── Hủy chứng từ ─────────────────────────

        /// <summary>
        /// Hủy mọi chứng từ (chưa hủy) gắn với phiếu: gỡ bút toán sổ cái (nếu đã ghi sổ) và chuyển chứng từ sang trạng thái Hủy.
        /// Kỳ kế toán đã khóa (10.8) thì báo lỗi và không đổi gì.
        /// </summary>
        public async Task<PhieuVoucherResult> CancelVouchersAsync(string loai, Guid phieuId, string user)
        {
            try
            {
                db.ChangeTracker.Clear();
                var key = phieuId.ToString();
                var vouchers = await db.AccountingVouchers
                    .Where(v => v.SourceId != null && v.SourceId == key && v.Status != StatusCancelled)
                    .ToListAsync();
                if (vouchers.Count == 0)
                    return PhieuVoucherResult.NoChange();

                var voucherIds = vouchers.Select(v => v.Id).ToList();
                var entries = await db.GeneralLedgerEntries
                    .Where(g => voucherIds.Contains(g.VoucherId))
                    .ToListAsync();
                db.GeneralLedgerEntries.RemoveRange(entries);

                var now = DateTime.Now;
                foreach (var v in vouchers)
                {
                    v.Status = StatusCancelled;
                    v.Ghiso = false;
                    v.Approve = false;
                    v.CancelledBy = user;
                    v.CancelledDate = now;
                    v.ModifiedBy = user;
                    v.ModifiedDate = now;
                }

                // AppDbContext.SaveChanges tự chặn nếu ngày hạch toán thuộc kỳ đã khóa
                await db.SaveChangesAsync();

                var nos = string.Join(", ", vouchers.Select(v => v.VoucherNo));
                try
                {
                    await historyLog.LogAsync(user, "Cancel Voucher On Unapprove",
                        loai == LoaiThu ? "Phieuthu" : "Phieuchi", phieuId, nos,
                        new { Vouchers = vouchers.Select(v => new { v.Id, v.VoucherNo }), RemovedLedgerEntries = entries.Count });
                }
                catch
                {
                    // bỏ qua lỗi ghi lịch sử
                }

                var removed = entries.Count > 0 ? $", gỡ {entries.Count} bút toán khỏi sổ cái" : "";
                return new PhieuVoucherResult(true, true, $"Đã hủy chứng từ kế toán {nos}{removed}.", nos);
            }
            catch (AccountingPeriodClosedException ex)
            {
                db.ChangeTracker.Clear();
                return PhieuVoucherResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                db.ChangeTracker.Clear();
                return PhieuVoucherResult.Fail(ex.GetBaseException().Message);
            }
        }

        // ───────────────────────── Tra cứu ─────────────────────────

        /// <summary>Chứng từ (chưa hủy, mới nhất) của từng phiếu; phiếu chưa có chứng từ thì không có trong kết quả.</summary>
        public async Task<Dictionary<Guid, PhieuVoucherInfo>> GetVoucherInfoAsync(IEnumerable<Guid> phieuIds)
        {
            var result = new Dictionary<Guid, PhieuVoucherInfo>();
            var keys = phieuIds.Where(x => x != Guid.Empty).Distinct().Select(x => x.ToString()).ToList();
            if (keys.Count == 0)
                return result;

            var rows = new List<(string SourceId, Guid Id, string No, int Status, bool? Ghiso, DateTime Created)>();
            foreach (var chunk in keys.Chunk(InChunkSize))
            {
                var part = chunk.ToList();
                var found = await db.AccountingVouchers.AsNoTracking()
                    .Where(v => v.SourceId != null && part.Contains(v.SourceId) && v.Status != StatusCancelled)
                    .Select(v => new { v.SourceId, v.Id, v.VoucherNo, v.Status, v.Ghiso, v.CreatedDate })
                    .ToListAsync();
                rows.AddRange(found.Select(v => (v.SourceId!, v.Id, v.VoucherNo, v.Status, v.Ghiso, v.CreatedDate)));
            }

            foreach (var g in rows.GroupBy(r => r.SourceId.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                if (!Guid.TryParse(g.Key, out var phieuId))
                    continue;
                var v = g.OrderByDescending(r => r.Created).First();
                result[phieuId] = new PhieuVoucherInfo(v.Id, v.No, v.Status, v.Ghiso == true);
            }

            return result;
        }

        // ───────────────────────── Nội bộ ─────────────────────────

        private async Task<Source?> LoadSourceAsync(string loai, Guid id)
        {
            if (loai == LoaiThu)
            {
                var p = await db.Phieuthu.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuthuID == id);
                return p == null ? null : new Source(p.PhieuthuID, "thu", p.SoPhieuthu ?? string.Empty, p.Approve == true,
                    p.TKNo, p.TKCo, p.Sotien ?? 0, p.Ngayhachtoan ?? p.Ngay);
            }

            var c = await db.Phieuchi.AsNoTracking().FirstOrDefaultAsync(x => x.PhieuchiID == id);
            return c == null ? null : new Source(c.PhieuchiID, "chi", c.Sophieuchi ?? string.Empty, c.Approve == true,
                c.TKnophieuchi, c.TKCophieuchi, c.Sotien ?? 0, c.Ngay);
        }

        private async Task<Guid?> ExecCreateProcedureAsync(string loai, Guid phieuId, object companyId, string user, bool autoPost)
        {
            var conn = db.Database.GetDbConnection();
            var shouldClose = conn.State != ConnectionState.Open;
            if (shouldClose)
                await conn.OpenAsync();

            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 180;
                if (loai == LoaiThu)
                {
                    cmd.CommandText = "dbo.usp_CreateAccountingVoucher_FromPhieuThu";
                    AddParameter(cmd, "@PhieuthuID", phieuId);
                }
                else
                {
                    cmd.CommandText = "dbo.usp_CreateAccountingVoucher_FromPhieuChi";
                    AddParameter(cmd, "@PhieuchiID", phieuId);
                }

                AddParameter(cmd, "@CompanyId", companyId);
                AddParameter(cmd, "@CreatedBy", string.IsNullOrWhiteSpace(user) ? "system" : user);
                AddParameter(cmd, "@AutoPost", autoPost ? 1 : 0);
                AddParameter(cmd, "@ClearOld", 1);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        if (string.Equals(reader.GetName(i), "VoucherId", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            return reader.GetGuid(i);
                    }
                }

                return null;
            }
            finally
            {
                if (shouldClose)
                    await conn.CloseAsync();
            }
        }

        private async Task<PhieuVoucherInfo?> FindVoucherAsync(Guid? voucherId, Guid phieuId)
        {
            db.ChangeTracker.Clear();
            if (voucherId.HasValue && voucherId.Value != Guid.Empty)
            {
                var v = await db.AccountingVouchers.AsNoTracking()
                    .Where(x => x.Id == voucherId.Value)
                    .Select(x => new { x.Id, x.VoucherNo, x.Status, x.Ghiso })
                    .FirstOrDefaultAsync();
                if (v != null)
                    return new PhieuVoucherInfo(v.Id, v.VoucherNo, v.Status, v.Ghiso == true);
            }

            var byPhieu = await GetVoucherInfoAsync(new[] { phieuId });
            return byPhieu.TryGetValue(phieuId, out var info) ? info : null;
        }

        private static void AddParameter(DbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static string Cap(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
    }
}
