using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services.Accounting;

/// <summary>
/// 10.4.8 Hạch toán phiếu thu / chi đã duyệt:
/// tick = tạo chứng từ kế toán NHÁP (Status 1) theo nghiệp vụ đã chọn (hoặc theo TK Nợ/Có trên phiếu);
/// bỏ tick = xoá chứng từ nháp đó; chứng từ đã ghi sổ thì không cho xoá.
/// Chứng từ gắn với phiếu qua AccountingVouchers.SourceId = Id phiếu (giống chứng từ do nút "Tạo Voucher" 10.1 / 10.2 tạo).
/// Không cần đăng ký DI: trang tạo bằng new với các service sẵn có.
/// </summary>
public sealed class PhieuHachToanService
{
    public const string LoaiThu = "Thu";
    public const string LoaiChi = "Chi";
    /// <summary>Giá trị "nghiệp vụ" đặc biệt: hạch toán theo TK Nợ / TK Có ghi trên phiếu.</summary>
    public const string TheoTkPhieu = "__PHIEU__";
    const int StatusCancelled = 3;

    readonly IDbContextFactory<AppDbContext> _dbFactory;
    readonly SupportServices _supsv;
    readonly HistoryLogService _history;

    public PhieuHachToanService(IDbContextFactory<AppDbContext> dbFactory, SupportServices supsv, HistoryLogService history)
    {
        _dbFactory = dbFactory;
        _supsv = supsv;
        _history = history;
    }

    public sealed class PhieuRow
    {
        public string Loai { get; init; } = LoaiThu;
        public Guid Id { get; init; }
        public string So { get; init; } = string.Empty;
        public DateTime? Ngay { get; init; }
        public DateTime PostingDate { get; init; }
        public Guid? CustomerId { get; init; }
        public string? NoiDung { get; init; }
        public string Currency { get; init; } = "VND";
        public decimal Amount { get; init; }
        public decimal Rate { get; init; } = 1m;
        public string? TkNo { get; init; }
        public string? TkCo { get; init; }
        public string? Bill { get; init; }
        public string? ApproveBy { get; init; }
        public decimal AmountVnd => Math.Round(Amount * Rate, 0, MidpointRounding.AwayFromZero);
    }

    public sealed record VoucherState(Guid Id, string VoucherNo, int Status, bool IsPosted, string? TransactionTypeCode);

    public sealed record Result(bool Success, string Message, string? VoucherNo = null);

    public static bool IsPosted(M_AccountingVouchers v) =>
        v.Status == 2 || v.Ghiso == true || !string.IsNullOrWhiteSpace(v.PostedBy);

    // ───────────── Đọc dữ liệu ─────────────

    /// <summary>Phiếu thu / chi đã duyệt có ngày trong khoảng [from, to].</summary>
    public async Task<List<PhieuRow>> LoadApprovedAsync(DateTime from, DateTime to, string? loai)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var rows = new List<PhieuRow>();

        if (loai != LoaiChi)
        {
            var thu = await db.Phieuthu.AsNoTracking()
                .Where(x => x.Approve == true && x.Ngay >= start && x.Ngay < end)
                .ToListAsync();
            rows.AddRange(thu.Select(x => new PhieuRow
            {
                Loai = LoaiThu, Id = x.PhieuthuID, So = x.SoPhieuthu ?? string.Empty, Ngay = x.Ngay,
                PostingDate = (x.Ngayhachtoan ?? x.Ngay ?? DateTime.Today).Date,
                CustomerId = x.Customer_Id, NoiDung = x.Noidung,
                Currency = string.IsNullOrWhiteSpace(x.Currency) ? "VND" : x.Currency.Trim().ToUpperInvariant(),
                Amount = (decimal)(x.Sotien ?? 0), Rate = RateOf(x.Currency, x.Tigia),
                TkNo = x.TKNo, TkCo = x.TKCo, Bill = x.BillNo, ApproveBy = x.ApproveBy
            }));
        }
        if (loai != LoaiThu)
        {
            var chi = await db.Phieuchi.AsNoTracking()
                .Where(x => x.Approve == true && x.Ngay >= start && x.Ngay < end)
                .ToListAsync();
            rows.AddRange(chi.Select(x => new PhieuRow
            {
                Loai = LoaiChi, Id = x.PhieuchiID, So = x.Sophieuchi ?? string.Empty, Ngay = x.Ngay,
                PostingDate = (x.Ngay ?? DateTime.Today).Date,
                CustomerId = x.Customer_ID, NoiDung = x.Noidung,
                Currency = string.IsNullOrWhiteSpace(x.Currency) ? "VND" : x.Currency.Trim().ToUpperInvariant(),
                Amount = (decimal)(x.Sotien ?? 0), Rate = RateOf(x.Currency, x.Tigia),
                TkNo = x.TKnophieuchi, TkCo = x.TKCophieuchi, Bill = x.Hbl, ApproveBy = x.ApproveBy
            }));
        }
        return rows.OrderBy(r => r.Ngay).ThenBy(r => r.Loai).ThenBy(r => r.So).ToList();
    }

    static decimal RateOf(string? currency, double? rate)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Equals("VND", StringComparison.OrdinalIgnoreCase))
            return 1m;
        return rate is > 0 ? (decimal)rate.Value : 1m;
    }

    /// <summary>Chứng từ (chưa huỷ) mới nhất của từng phiếu.</summary>
    public async Task<Dictionary<Guid, VoucherState>> GetVouchersAsync(IEnumerable<Guid> phieuIds)
    {
        var result = new Dictionary<Guid, VoucherState>();
        var keys = phieuIds.Distinct().Select(x => x.ToString()).ToList();
        if (keys.Count == 0) return result;
        await using var db = await _dbFactory.CreateDbContextAsync();
        foreach (var chunk in keys.Chunk(1000))
        {
            var ids = chunk.ToList();
            var list = await db.AccountingVouchers.AsNoTracking()
                .Where(v => v.SourceId != null && ids.Contains(v.SourceId) && v.Status != StatusCancelled)
                .ToListAsync();
            foreach (var g in list.GroupBy(v => v.SourceId!))
            {
                if (!Guid.TryParse(g.Key, out var pid)) continue;
                // Có chứng từ đã ghi sổ thì ưu tiên hiện chứng từ đó
                var v = g.OrderByDescending(IsPosted).ThenByDescending(x => x.CreatedDate).First();
                result[pid] = new VoucherState(v.Id, v.VoucherNo, v.Status, g.Any(IsPosted), v.TransactionTypeCode);
            }
        }
        return result;
    }

    public async Task<List<M_TransactionTypes>> GetTransactionTypesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.TransactionTypes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code).ToListAsync();
    }

    // ───────────── Tạo chứng từ nháp ─────────────

    sealed record LineSpec(Guid AccountId, string AccountCode, bool IsDebit);

    public async Task<Result> CreateDraftAsync(PhieuRow row, string transactionType, string user)
    {
        if (row.Amount == 0)
            return new Result(false, $"{row.So}: số tiền bằng 0.");

        await using var db = await _dbFactory.CreateDbContextAsync();
        var key = row.Id.ToString();
        var existing = await db.AccountingVouchers.AsNoTracking()
            .Where(v => v.SourceId == key && v.Status != StatusCancelled)
            .Select(v => v.VoucherNo).FirstOrDefaultAsync();
        if (existing != null)
            return new Result(false, $"{row.So}: đã có chứng từ {existing}.", existing);

        // Dòng hạch toán
        List<LineSpec> specs;
        string txCode;
        if (transactionType == TheoTkPhieu)
        {
            if (string.IsNullOrWhiteSpace(row.TkNo) || string.IsNullOrWhiteSpace(row.TkCo))
                return new Result(false, $"{row.So}: phiếu chưa có TK Nợ / TK Có.");
            var codes = new[] { row.TkNo.Trim(), row.TkCo.Trim() };
            var accounts = await db.DanhMucTaiKhoan.AsNoTracking()
                .Where(a => a.Taikhoan != null && codes.Contains(a.Taikhoan))
                .ToListAsync();
            var no = accounts.FirstOrDefault(a => a.Taikhoan!.Trim() == codes[0]);
            var co = accounts.FirstOrDefault(a => a.Taikhoan!.Trim() == codes[1]);
            if (no == null || co == null)
                return new Result(false, $"{row.So}: không tìm thấy tài khoản {(no == null ? codes[0] : codes[1])} trong danh mục tài khoản.");
            specs = new() { new(no.Id, codes[0], true), new(co.Id, codes[1], false) };
            txCode = row.Loai == LoaiThu ? "PHIEUTHU" : "PHIEUCHI";
        }
        else
        {
            var tx = await db.TransactionTypes.AsNoTracking().FirstOrDefaultAsync(t => t.Code == transactionType);
            if (tx == null)
                return new Result(false, $"Không tìm thấy nghiệp vụ {transactionType}.");
            var maps = await db.TransactionTypeMappings.AsNoTracking()
                .Where(m => m.TransactionID == tx.Id && m.IsActive != false && m.DanhMucTaiKhoanID != null)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
            // Phiếu chỉ có 1 số tiền: bỏ dòng thuế / nhập tay
            maps = maps.Where(m => !string.Equals(m.AmountSource?.Trim(), "TAX", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(m.AmountSource?.Trim(), "MANUAL", StringComparison.OrdinalIgnoreCase)).ToList();
            var accIds = maps.Select(m => m.DanhMucTaiKhoanID!.Value).Distinct().ToList();
            var accs = await db.DanhMucTaiKhoan.AsNoTracking().Where(a => accIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);
            specs = maps.Where(m => accs.ContainsKey(m.DanhMucTaiKhoanID!.Value))
                .Select(m => new LineSpec(m.DanhMucTaiKhoanID!.Value, accs[m.DanhMucTaiKhoanID!.Value].Taikhoan?.Trim() ?? string.Empty,
                    !string.Equals(m.LineType?.Trim(), "CREDIT", StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (specs.Count(s => s.IsDebit) != 1 || specs.Count(s => !s.IsDebit) != 1)
                return new Result(false, $"Nghiệp vụ {tx.Code} cần đúng 1 dòng Nợ và 1 dòng Có (bỏ qua dòng thuế) để hạch toán phiếu một số tiền; hiện có {specs.Count(s => s.IsDebit)} Nợ / {specs.Count(s => !s.IsDebit)} Có.");
            txCode = tx.Code;
        }

        var companyId = await db.CompanyInfomation.AsNoTracking().Select(c => c.CompanyID).FirstOrDefaultAsync();
        var date = row.PostingDate;
        var voucher = new M_AccountingVouchers
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            VoucherNo = await NextVoucherNoAsync(db, date, user),
            VoucherDate = date,
            PostingDate = date,
            FiscalYear = date.Year,
            FiscalPeriod = date.Month,
            TransactionTypeCode = txCode,
            Description = Truncate($"Hạch toán phiếu {row.Loai.ToLowerInvariant()} {row.So}" + (string.IsNullOrWhiteSpace(row.NoiDung) ? "" : $" - {row.NoiDung}"), 500),
            CurrencyCode = row.Currency,
            ExchangeRate = row.Rate,
            Status = 1,
            ReferenceNo = row.So,
            ReferenceDate = row.Ngay,
            SourceModule = row.Loai == LoaiThu ? "PHIEUTHU" : "PHIEUCHI",
            SourceId = key,
            CreatedBy = user,
            CreatedDate = DateTime.UtcNow,
            Ghiso = false,
            Approve = false
        };
        var vnd = Math.Abs(row.AmountVnd);
        var fc = Math.Abs(row.Amount);
        var lines = specs.Select((s, i) => new M_AccountingVoucherLines
        {
            Id = Guid.NewGuid(),
            VoucherId = voucher.Id,
            LineNo_ = (i + 1).ToString(),
            DanhMucTaiKhoanID = s.AccountId,
            AccountCode = s.AccountCode,
            DebitAmount = s.IsDebit ? vnd : 0,
            CreditAmount = s.IsDebit ? 0 : vnd,
            DebitAmountFC = s.IsDebit ? fc : 0,
            CreditAmountFC = s.IsDebit ? 0 : fc,
            LineDescription = Truncate($"{row.So} {row.NoiDung}".Trim(), 500),
            CustomerId = row.CustomerId,
            HBLNo = row.Bill,
            IsTaxBook = false,
            IsManagementBook = false,
            SortKey = i + 1
        }).ToList();

        db.AccountingVouchers.Add(voucher);
        db.AccountingVoucherLines.AddRange(lines);
        await db.SaveChangesAsync(); // kỳ đã khóa sổ → AccountingPeriodClosedException
        await _history.LogAsync(user, "ADD", "PhieuKeToan", voucher.Id, voucher.VoucherNo,
            new { Source = $"{row.Loai} {row.So}", TransactionType = txCode, Voucher = voucher, Lines = lines });
        return new Result(true, $"{row.So}: đã tạo chứng từ nháp {voucher.VoucherNo}.", voucher.VoucherNo);
    }

    // ───────────── Bỏ hạch toán ─────────────

    /// <summary>Xoá chứng từ nháp của phiếu. Có chứng từ đã ghi sổ thì từ chối (phải bỏ ghi sổ ở 10.4.1 trước).</summary>
    public async Task<Result> DeleteDraftAsync(Guid phieuId, string so, string user)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var key = phieuId.ToString();
        var vouchers = await db.AccountingVouchers
            .Where(v => v.SourceId == key && v.Status != StatusCancelled)
            .ToListAsync();
        if (vouchers.Count == 0)
            return new Result(true, $"{so}: chưa có chứng từ.");
        var posted = vouchers.FirstOrDefault(IsPosted);
        if (posted != null)
            return new Result(false, $"{so}: chứng từ {posted.VoucherNo} đã ghi sổ, không xoá được. Bỏ ghi sổ ở 10.4.1 trước.", posted.VoucherNo);

        var ids = vouchers.Select(v => v.Id).ToList();
        var lines = await db.AccountingVoucherLines.Where(l => ids.Contains(l.VoucherId)).ToListAsync();
        db.AccountingVoucherLines.RemoveRange(lines);
        db.AccountingVouchers.RemoveRange(vouchers);
        await db.SaveChangesAsync();
        foreach (var v in vouchers)
            await _history.LogAsync(user, "Delete", "PhieuKeToan", v.Id, v.VoucherNo, new { Source = so, OldData = v });
        return new Result(true, $"{so}: đã xoá chứng từ nháp {string.Join(", ", vouchers.Select(v => v.VoucherNo))}.");
    }

    // ───────────── tiện ích ─────────────

    async Task<string> NextVoucherNoAsync(AppDbContext db, DateTime date, string usr)
    {
        var sequence = await _supsv.GetRefNoByFunc("Account", date.Month, date.Year, usr) ?? 1;
        while (true)
        {
            var no = $"Acc_{date.Year}{date.Month}{sequence:D4}";
            if (!await db.AccountingVouchers.AnyAsync(x => x.VoucherNo == no))
                return no;
            sequence++;
        }
    }

    static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
