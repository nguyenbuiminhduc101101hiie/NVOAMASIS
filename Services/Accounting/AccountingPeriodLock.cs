using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services.Accounting
{
    /// <summary>Báo lỗi khi ghi chứng từ vào kỳ kế toán đã khóa sổ (10.8).</summary>
    public sealed class AccountingPeriodClosedException(string message) : InvalidOperationException(message);

    /// <summary>
    /// Khóa sổ kỳ kế toán (10.8 — accounting_period.is_closed = 1 hoặc status = 'CLOSED' / 'LOCKED').
    /// Kỳ đã khóa thì không thêm / sửa / xóa được:
    ///  - Phiếu kế toán (AccountingVouchers) + dòng (AccountingVoucherLines) — theo ngày hạch toán của phiếu,
    ///    kể cả ghi sổ / bỏ ghi sổ / hủy;
    ///  - Bút toán sổ cái (GeneralLedgerEntries) — theo ngày hạch toán;
    ///  - Phiếu thu / phiếu chi (10.1, 10.2): thêm, xóa, hoặc sửa các trường ảnh hưởng số liệu
    ///    (ngày, số tiền, tỷ giá, loại tiền, đối tượng, tài khoản). Vẫn cho đổi trạng thái thanh toán / duyệt
    ///    (webhook ngân hàng có thể đánh dấu "đã thanh toán" cho phiếu của tháng đã khóa).
    /// Kiểm tra tự động trong AppDbContext.SaveChanges; các chỗ ghi bằng SQL trực tiếp gọi EnsureOpen(Async).
    /// </summary>
    public static class AccountingPeriodLock
    {
        public readonly record struct ClosedPeriod(DateTime Start, DateTime End, int Year, int Month)
        {
            public string Label => Month is >= 1 and <= 12 ? $"{Month:D2}/{Year}" : $"{Start:dd/MM/yyyy}–{End:dd/MM/yyyy}";
            public bool Contains(DateTime d) => d.Date >= Start.Date && d.Date <= End.Date;
        }

        /// <summary>Trường của phiếu thu làm thay đổi số liệu kế toán.</summary>
        private static readonly string[] ReceiptFields =
        {
            nameof(M_PhieuThu.Ngay), nameof(M_PhieuThu.Ngayhachtoan), nameof(M_PhieuThu.Sotien), nameof(M_PhieuThu.Tigia),
            nameof(M_PhieuThu.Currency), nameof(M_PhieuThu.Customer_Id), nameof(M_PhieuThu.TKNo), nameof(M_PhieuThu.TKCo),
            nameof(M_PhieuThu.Taikhoan_Doiung), nameof(M_PhieuThu.Sodudauky), nameof(M_PhieuThu.Loaiphieu)
        };

        /// <summary>Trường của phiếu chi làm thay đổi số liệu kế toán.</summary>
        private static readonly string[] PaymentFields =
        {
            nameof(M_PhieuChi.Ngay), nameof(M_PhieuChi.Sotien), nameof(M_PhieuChi.Tigia), nameof(M_PhieuChi.Currency),
            nameof(M_PhieuChi.Customer_ID), nameof(M_PhieuChi.TKnophieuchi), nameof(M_PhieuChi.TKCophieuchi),
            nameof(M_PhieuChi.Loaiphieu)
        };

        // ───────────── Đọc kỳ đã khóa ─────────────

        public static async Task<List<ClosedPeriod>> LoadClosedAsync(AppDbContext db, CancellationToken ct = default)
        {
            try
            {
                return (await db.AccountingPeriods.AsNoTracking()
                        .Where(p => p.IsClosed || p.Status == "CLOSED" || p.Status == "LOCKED")
                        .Select(p => new { p.StartDate, p.EndDate, p.FiscalYear, p.PeriodMonth })
                        .ToListAsync(ct))
                    .Select(p => new ClosedPeriod(p.StartDate, p.EndDate, p.FiscalYear, p.PeriodMonth))
                    .ToList();
            }
            catch (Exception ex) when (IsMissingTable(ex))
            {
                return new List<ClosedPeriod>();
            }
        }

        public static List<ClosedPeriod> LoadClosed(AppDbContext db)
        {
            try
            {
                return db.AccountingPeriods.AsNoTracking()
                    .Where(p => p.IsClosed || p.Status == "CLOSED" || p.Status == "LOCKED")
                    .Select(p => new { p.StartDate, p.EndDate, p.FiscalYear, p.PeriodMonth })
                    .ToList()
                    .Select(p => new ClosedPeriod(p.StartDate, p.EndDate, p.FiscalYear, p.PeriodMonth))
                    .ToList();
            }
            catch (Exception ex) when (IsMissingTable(ex))
            {
                return new List<ClosedPeriod>();
            }
        }

        private static bool IsMissingTable(Exception ex) =>
            ex.GetBaseException().Message.Contains("accounting_period", StringComparison.OrdinalIgnoreCase)
            && ex.GetBaseException().Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase);

        public static ClosedPeriod? Find(IReadOnlyCollection<ClosedPeriod> closed, DateTime? date)
        {
            if (!date.HasValue || closed.Count == 0) return null;
            foreach (var p in closed)
                if (p.Contains(date.Value)) return p;
            return null;
        }

        public static string Message(ClosedPeriod p, DateTime date, string what) =>
            $"Kỳ kế toán {p.Label} đã khóa sổ (10.8) — không thể {System.Text.RegularExpressions.Regex.Replace(what.Trim(), @"\s+", " ")} ngày {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}. " +
            "Mở khóa kỳ ở 10.8 nếu cần điều chỉnh.";

        // ───────────── Dùng cho chỗ ghi bằng SQL trực tiếp ─────────────

        /// <summary>Báo lỗi nếu 1 trong các ngày thuộc kỳ đã khóa. <paramref name="what"/> vd "ghi sổ chứng từ".</summary>
        public static async Task EnsureOpenAsync(AppDbContext db, IEnumerable<DateTime?> dates, string what, CancellationToken ct = default)
        {
            var list = dates.Where(d => d.HasValue).Select(d => d!.Value.Date).Distinct().ToList();
            if (list.Count == 0) return;
            var closed = await LoadClosedAsync(db, ct);
            foreach (var d in list)
                if (Find(closed, d) is { } p) throw new AccountingPeriodClosedException(Message(p, d, what));
        }

        public static Task EnsureOpenAsync(AppDbContext db, DateTime? date, string what, CancellationToken ct = default) =>
            EnsureOpenAsync(db, new[] { date }, what, ct);

        /// <summary>Kiểm tra theo năm / tháng (vd số dư đầu kỳ nhập theo kỳ).</summary>
        public static async Task EnsureOpenAsync(AppDbContext db, int year, int month, string what, CancellationToken ct = default)
        {
            if (month is < 1 or > 12 || year < 1900) return;
            var first = new DateTime(year, month, 1);
            await EnsureOpenAsync(db, new DateTime?[] { first, first.AddMonths(1).AddDays(-1) }, what, ct);
        }

        // ───────────── Kiểm tra tự động khi SaveChanges ─────────────

        public static void CheckTrackedChanges(AppDbContext db) =>
            CheckTrackedChangesCoreAsync(db, sync: true, CancellationToken.None).GetAwaiter().GetResult();

        public static Task CheckTrackedChangesAsync(AppDbContext db, CancellationToken ct) =>
            CheckTrackedChangesCoreAsync(db, sync: false, ct);

        /// <remarks>Ở chế độ sync mọi truy vấn chạy đồng bộ (không await thật) → không bị deadlock.</remarks>
        private static async Task CheckTrackedChangesCoreAsync(AppDbContext db, bool sync, CancellationToken ct)
        {
            var entries = db.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                            && e.Entity is M_AccountingVouchers or M_AccountingVoucherLines or M_GeneralLedgerEntries
                                or M_PhieuThu or M_PhieuChi)
                .ToList();
            if (entries.Count == 0) return;

            var closed = sync ? LoadClosed(db) : await LoadClosedAsync(db, ct);
            if (closed.Count == 0) return;

            var checks = new List<(DateTime? Date, string What)>();

            // 1. Phiếu kế toán
            var voucherDbDates = await DbValuesAsync(db, sync, ct,
                entries.Where(e => e.Entity is M_AccountingVouchers && e.State != EntityState.Added)
                    .Select(e => ((M_AccountingVouchers)e.Entity).Id),
                ids => db.AccountingVouchers.AsNoTracking().Where(v => ids.Contains(v.Id))
                    .Select(v => new KeyValuePair<Guid, DateTime>(v.Id, v.PostingDate)));
            foreach (var e in entries.Where(e => e.Entity is M_AccountingVouchers))
            {
                var v = (M_AccountingVouchers)e.Entity;
                var what = e.State switch
                {
                    EntityState.Added => "thêm phiếu kế toán",
                    EntityState.Deleted => "xóa phiếu kế toán",
                    _ => "sửa / ghi sổ / bỏ ghi sổ phiếu kế toán"
                };
                if (e.State != EntityState.Deleted) checks.Add((v.PostingDate, $"{what} {v.VoucherNo}"));
                if (e.State != EntityState.Added && voucherDbDates.TryGetValue(v.Id, out var old))
                    checks.Add((old, $"{what} {v.VoucherNo}"));
            }

            // 2. Dòng phiếu kế toán → theo ngày hạch toán của phiếu
            var lineEntries = entries.Where(e => e.Entity is M_AccountingVoucherLines).ToList();
            if (lineEntries.Count > 0)
            {
                var lineDbVoucher = await DbValuesAsync(db, sync, ct,
                    lineEntries.Where(e => e.State != EntityState.Added).Select(e => ((M_AccountingVoucherLines)e.Entity).Id),
                    ids => db.AccountingVoucherLines.AsNoTracking().Where(l => ids.Contains(l.Id))
                        .Select(l => new KeyValuePair<Guid, Guid>(l.Id, l.VoucherId)));
                var voucherIds = new HashSet<Guid>();
                foreach (var e in lineEntries)
                {
                    var l = (M_AccountingVoucherLines)e.Entity;
                    voucherIds.Add(l.VoucherId);
                    if (lineDbVoucher.TryGetValue(l.Id, out var oldVoucher)) voucherIds.Add(oldVoucher);
                }
                // Ngày của phiếu: ưu tiên bản đang sửa trong context, cộng thêm ngày trong DB
                var tracked = db.ChangeTracker.Entries<M_AccountingVouchers>()
                    .GroupBy(x => x.Entity.Id).ToDictionary(g => g.Key, g => g.First().Entity.PostingDate);
                var dbDates = await DbValuesAsync(db, sync, ct, voucherIds,
                    ids => db.AccountingVouchers.AsNoTracking().Where(v => ids.Contains(v.Id))
                        .Select(v => new KeyValuePair<Guid, DateTime>(v.Id, v.PostingDate)));
                foreach (var e in lineEntries)
                {
                    var l = (M_AccountingVoucherLines)e.Entity;
                    var what = e.State switch
                    {
                        EntityState.Added => "thêm dòng phiếu kế toán",
                        EntityState.Deleted => "xóa dòng phiếu kế toán",
                        _ => "sửa dòng phiếu kế toán"
                    };
                    var ids = new List<Guid> { l.VoucherId };
                    if (lineDbVoucher.TryGetValue(l.Id, out var ov)) ids.Add(ov);
                    foreach (var vid in ids.Distinct())
                    {
                        if (tracked.TryGetValue(vid, out var td)) checks.Add((td, what));
                        if (dbDates.TryGetValue(vid, out var dd)) checks.Add((dd, what));
                    }
                }
            }

            // 3. Bút toán sổ cái
            var glEntries = entries.Where(e => e.Entity is M_GeneralLedgerEntries).ToList();
            if (glEntries.Count > 0)
            {
                var glDb = await DbValuesAsync(db, sync, ct,
                    glEntries.Where(e => e.State != EntityState.Added).Select(e => ((M_GeneralLedgerEntries)e.Entity).Id),
                    ids => db.GeneralLedgerEntries.AsNoTracking().Where(g => ids.Contains(g.Id))
                        .Select(g => new KeyValuePair<Guid, DateTime>(g.Id, g.PostingDate)));
                foreach (var e in glEntries)
                {
                    var g = (M_GeneralLedgerEntries)e.Entity;
                    var what = e.State switch
                    {
                        EntityState.Added => "ghi sổ cái",
                        EntityState.Deleted => "xóa bút toán sổ cái",
                        _ => "sửa bút toán sổ cái"
                    };
                    if (e.State != EntityState.Deleted) checks.Add((g.PostingDate, $"{what} {g.VoucherNo}"));
                    if (e.State != EntityState.Added && glDb.TryGetValue(g.Id, out var old)) checks.Add((old, $"{what} {g.VoucherNo}"));
                }
            }

            // 4. Phiếu thu / chi
            foreach (var e in entries.Where(e => e.Entity is M_PhieuThu or M_PhieuChi))
            {
                var receipt = e.Entity is M_PhieuThu;
                var label = receipt ? $"phiếu thu {((M_PhieuThu)e.Entity).SoPhieuthu}" : $"phiếu chi {((M_PhieuChi)e.Entity).Sophieuchi}";
                DateTime? Date(PropertyValues v) => receipt
                    ? v.GetValue<DateTime?>(nameof(M_PhieuThu.Ngayhachtoan)) ?? v.GetValue<DateTime?>(nameof(M_PhieuThu.Ngay))
                    : v.GetValue<DateTime?>(nameof(M_PhieuChi.Ngay));

                if (e.State == EntityState.Added)
                {
                    checks.Add((Date(e.CurrentValues), $"thêm {label}"));
                    continue;
                }

                var dbValues = sync ? e.GetDatabaseValues() : await e.GetDatabaseValuesAsync(ct);
                if (dbValues is null) continue; // đã bị xóa ở nơi khác
                if (e.State == EntityState.Deleted)
                {
                    checks.Add((Date(dbValues), $"xóa {label}"));
                    continue;
                }

                // Sửa: chỉ chặn khi đổi trường ảnh hưởng số liệu (so với giá trị trong DB)
                var fields = receipt ? ReceiptFields : PaymentFields;
                var changed = fields.Where(f => !Equals(e.CurrentValues[f], dbValues[f])).ToList();
                if (changed.Count == 0) continue;
                var what = $"sửa {label} ({string.Join(", ", changed)})";
                checks.Add((Date(dbValues), what));
                checks.Add((Date(e.CurrentValues), what));
            }

            foreach (var (date, what) in checks)
                if (Find(closed, date) is { } p)
                    throw new AccountingPeriodClosedException(Message(p, date!.Value, what));
        }

        /// <summary>Đọc giá trị trong DB theo lô (≤ 1000 id / truy vấn), sync hoặc async.</summary>
        private static async Task<Dictionary<Guid, T>> DbValuesAsync<T>(AppDbContext db, bool sync, CancellationToken ct,
            IEnumerable<Guid> ids, Func<Guid[], IQueryable<KeyValuePair<Guid, T>>> query)
        {
            var result = new Dictionary<Guid, T>();
            foreach (var chunk in ids.Distinct().Chunk(1000))
            {
                var rows = sync ? query(chunk).ToList() : await query(chunk).ToListAsync(ct);
                foreach (var kv in rows) result[kv.Key] = kv.Value;
            }
            return result;
        }
    }
}
