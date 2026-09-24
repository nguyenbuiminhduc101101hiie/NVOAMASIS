using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

/// <summary>Số tiền của hóa đơn mà một dòng tài khoản trong nghiệp vụ hạch toán sẽ lấy.</summary>
public enum AutoPostingSource
{
    /// <summary>Tổng tiền sau thuế.</summary>
    Total,
    /// <summary>Tiền hàng trước thuế.</summary>
    Net,
    /// <summary>Tiền thuế GTGT.</summary>
    Tax,
    /// <summary>Người dùng nhập tay cho từng hóa đơn khi hạch toán.</summary>
    Manual
}

public static class AutoPostingSources
{
    /// <summary>Giá trị lưu ở cột TransactionTypeMappings.AmountSource. Rỗng = để hệ thống tự đề xuất.</summary>
    public static string Code(AutoPostingSource source) => source switch
    {
        AutoPostingSource.Total => "TOTAL",
        AutoPostingSource.Net => "NET",
        AutoPostingSource.Tax => "TAX",
        _ => "MANUAL"
    };

    public static AutoPostingSource? Parse(string? code) => code?.Trim().ToUpperInvariant() switch
    {
        "TOTAL" => AutoPostingSource.Total,
        "NET" => AutoPostingSource.Net,
        "TAX" => AutoPostingSource.Tax,
        "MANUAL" => AutoPostingSource.Manual,
        _ => null
    };

    public static string Label(AutoPostingSource source) => source switch
    {
        AutoPostingSource.Total => "Tổng sau thuế",
        AutoPostingSource.Net => "Tiền trước thuế",
        AutoPostingSource.Tax => "Tiền thuế",
        _ => "Nhập tay"
    };
}

public sealed class AutoPostingAccount
{
    public Guid MappingId { get; init; }
    public Guid AccountId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsDebit { get; init; }
    public int SortOrder { get; init; }

    /// <summary>Nguồn số tiền đã cấu hình (null = chưa cấu hình, dùng gợi ý).</summary>
    public AutoPostingSource? ConfiguredSource { get; set; }

    /// <summary>Nguồn số tiền thực dùng khi hạch toán.</summary>
    public AutoPostingSource Source { get; set; }
}

public sealed class AutoPostingSetup
{
    public string? Error { get; init; }
    public List<AutoPostingAccount> Accounts { get; init; } = [];

    /// <summary>Tính lại nguồn số tiền mặc định cho các dòng chưa cấu hình.</summary>
    public void ResolveSources()
    {
        // TK Có mang tiền thuế (đã cấu hình hoặc 333x) thì TK Có còn lại mới lấy tiền trước thuế, nếu không lấy tổng sau thuế
        bool IsTaxCode(AutoPostingAccount a) => a.Code.StartsWith("333", StringComparison.Ordinal);
        var hasTaxCredit = Accounts.Any(a => !a.IsDebit && (a.ConfiguredSource == AutoPostingSource.Tax || (a.ConfiguredSource == null && IsTaxCode(a))));

        foreach (var a in Accounts)
        {
            if (a.ConfiguredSource.HasValue)
                a.Source = a.ConfiguredSource.Value;
            else if (a.IsDebit)
                a.Source = AutoPostingSource.Total;
            else if (IsTaxCode(a))
                a.Source = AutoPostingSource.Tax;
            else
                a.Source = hasTaxCredit ? AutoPostingSource.Net : AutoPostingSource.Total;
        }
    }
}

public sealed class AutoPostingLine
{
    public AutoPostingAccount Account { get; init; } = default!;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal DebitFC { get; init; }
    public decimal CreditFC { get; init; }
}

public sealed class AutoPostingInvoiceRow
{
    public BkavInvoiceGroup Group { get; init; } = default!;
    public string Currency { get; init; } = "VND";
    public decimal Rate { get; init; } = 1m;
    public decimal NetAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }

    /// <summary>Số tiền nhập tay (theo tiền tệ hóa đơn) cho các dòng có nguồn Manual, khóa = MappingId.</summary>
    public Dictionary<Guid, decimal> ManualAmounts { get; } = new();

    public List<AutoPostingLine> Lines { get; private set; } = [];
    public decimal DebitTotal => Lines.Sum(x => x.Debit);
    public decimal CreditTotal => Lines.Sum(x => x.Credit);
    public string? Error { get; private set; }
    public string? ExistingVoucherNo { get; set; }
    public bool CanPost => Error == null && ExistingVoucherNo == null && Lines.Count > 0;

    /// <summary>Tính lại các dòng bút toán theo nghiệp vụ đã chọn (không truy cập DB).</summary>
    public void Recalculate(AutoPostingSetup setup)
    {
        Lines = [];
        Error = null;

        if (setup.Error != null)
        {
            Error = setup.Error;
            return;
        }

        // quy đổi về VND; thuế = tổng - trước thuế để chắc chắn cân Nợ = Có khi làm tròn
        var totalBase = Math.Round(TotalAmount * Rate, 0, MidpointRounding.AwayFromZero);
        var netBase = Math.Round(NetAmount * Rate, 0, MidpointRounding.AwayFromZero);
        var taxBase = totalBase - netBase;

        if (totalBase == 0)
        {
            Error = "Tổng tiền hóa đơn bằng 0.";
            return;
        }

        foreach (var account in setup.Accounts.OrderBy(x => x.SortOrder))
        {
            decimal amountBase, amountFc;
            switch (account.Source)
            {
                case AutoPostingSource.Total:
                    (amountBase, amountFc) = (totalBase, TotalAmount);
                    break;
                case AutoPostingSource.Net:
                    (amountBase, amountFc) = (netBase, NetAmount);
                    break;
                case AutoPostingSource.Tax:
                    (amountBase, amountFc) = (taxBase, TaxAmount);
                    break;
                default:
                    amountFc = ManualAmounts.GetValueOrDefault(account.MappingId);
                    amountBase = Math.Round(amountFc * Rate, 0, MidpointRounding.AwayFromZero);
                    break;
            }

            if (amountBase == 0 && amountFc == 0)
                continue; // ví dụ hóa đơn không thuế: bỏ dòng thuế 0

            Lines.Add(new AutoPostingLine
            {
                Account = account,
                Debit = account.IsDebit ? amountBase : 0,
                Credit = account.IsDebit ? 0 : amountBase,
                DebitFC = account.IsDebit ? amountFc : 0,
                CreditFC = account.IsDebit ? 0 : amountFc
            });
        }

        if (Lines.Count == 0)
            Error = "Không có dòng bút toán nào có số tiền.";
        else if (DebitTotal != CreditTotal)
            Error = $"Bút toán không cân: Nợ {DebitTotal:N0} khác Có {CreditTotal:N0}.";
    }
}

public sealed class AutoPostingResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> VoucherNos { get; init; } = [];
}

/// <summary>
/// Tạo chứng từ kế toán (nháp) từ các hóa đơn bán ra theo nghiệp vụ hạch toán (TransactionTypes + TransactionTypeMappings).
/// Mỗi hóa đơn một chứng từ.
/// </summary>
public class AccountingAutoPostingService(
    IDbContextFactory<AppDbContext> dbFactory,
    SupportServices supsv,
    HistoryLogService historyLog)
{
    public const string SourceModuleName = "BILLING";

    public static string InvoiceKey(BkavInvoiceGroup group) => $"{group.InternalInvoiceNo.Trim()}|{group.CustomerId:N}";

    public async Task<List<M_TransactionTypes>> GetTransactionTypesAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.TransactionTypes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    /// <summary>Đọc các dòng tài khoản (đang hoạt động) của nghiệp vụ, kèm nguồn số tiền đã cấu hình.</summary>
    public async Task<AutoPostingSetup> LoadSetupAsync(Guid transactionTypeId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var mappings = await db.TransactionTypeMappings.AsNoTracking()
            .Where(x => x.TransactionID == transactionTypeId && (x.IsActive ?? false))
            .OrderBy(x => x.SortOrder)
            .ToListAsync();
        if (mappings.Count == 0)
            return new AutoPostingSetup { Error = "Nghiệp vụ này chưa cấu hình dòng tài khoản nào đang hoạt động." };

        // Lọc trong bộ nhớ: DB mức tương thích thấp không hỗ trợ Contains(list)
        var dms = (await db.DanhMucTaiKhoan.AsNoTracking().ToListAsync()).ToDictionary(x => x.Id);

        var accounts = new List<AutoPostingAccount>();
        foreach (var m in mappings)
        {
            if (m.DanhMucTaiKhoanID is null || !dms.TryGetValue(m.DanhMucTaiKhoanID.Value, out var dm))
                continue;

            accounts.Add(new AutoPostingAccount
            {
                MappingId = m.Id,
                AccountId = dm.Id,
                Code = (dm.Taikhoan ?? string.Empty).Trim(),
                Name = dm.Tentaikhoan ?? string.Empty,
                IsDebit = !string.Equals(m.LineType?.Trim(), "CREDIT", StringComparison.OrdinalIgnoreCase),
                SortOrder = m.SortOrder ?? 0,
                ConfiguredSource = AutoPostingSources.Parse(m.AmountSource)
            });
        }

        if (accounts.Count == 0)
            return new AutoPostingSetup { Error = "Các dòng của nghiệp vụ chưa gắn tài khoản hợp lệ." };

        var setup = new AutoPostingSetup { Accounts = accounts };
        setup.ResolveSources();
        return setup;
    }

    /// <summary>Lưu nguồn số tiền của một dòng vào cấu hình nghiệp vụ (null = quay về tự đề xuất).</summary>
    public async Task SaveAmountSourceAsync(Guid mappingId, AutoPostingSource? source)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var entity = await db.TransactionTypeMappings.FirstOrDefaultAsync(x => x.Id == mappingId);
        if (entity == null)
            return;

        entity.AmountSource = source.HasValue ? AutoPostingSources.Code(source.Value) : null;
        await db.SaveChangesAsync();
    }

    /// <summary>Dựng dòng xem trước của 1 hóa đơn (số tiền gốc + quy đổi), chưa tính bút toán — gọi Recalculate sau.</summary>
    public static AutoPostingInvoiceRow BuildRow(BkavInvoiceGroup group)
    {
        var currency = string.IsNullOrWhiteSpace(group.Currency) ? "VND" : group.Currency.Trim().ToUpperInvariant();
        var rate = 1m;
        if (currency != "VND")
        {
            var tigia = group.Lines.Select(x => x.tigia).FirstOrDefault(x => x.GetValueOrDefault() > 0);
            rate = tigia.HasValue ? (decimal)tigia.Value : 1m;
        }

        var total = (decimal)group.TotalAfterTax;
        var net = (decimal)group.TotalBeforeTax;
        return new AutoPostingInvoiceRow
        {
            Group = group,
            Currency = currency,
            Rate = rate,
            NetAmount = net,
            TaxAmount = total - net,
            TotalAmount = total
        };
    }

    /// <summary>Tìm chứng từ (chưa hủy) đã có dòng bút toán mang số hóa đơn + khách hàng này, để tránh hạch toán trùng.</summary>
    public async Task<Dictionary<string, string>> FindExistingVouchersAsync(IEnumerable<BkavInvoiceGroup> groups)
    {
        var result = new Dictionary<string, string>();
        await using var db = await dbFactory.CreateDbContextAsync();
        foreach (var inv in groups)
        {
            var no = inv.InternalInvoiceNo.Trim();
            if (no.Length == 0)
                continue;

            var voucherNo = await (
                from l in db.AccountingVoucherLines.AsNoTracking()
                join v in db.AccountingVouchers.AsNoTracking() on l.VoucherId equals v.Id
                where l.InvoiceNo == no && l.CustomerId == inv.CustomerId && v.Status != 3
                select v.VoucherNo).FirstOrDefaultAsync();
            if (voucherNo != null)
                result[InvoiceKey(inv)] = voucherNo;
        }

        return result;
    }

    /// <summary>Tạo chứng từ nháp (Status = 1, chưa ghi sổ), mỗi hóa đơn hợp lệ một chứng từ. Tất cả lưu trong một lần SaveChanges.</summary>
    public async Task<AutoPostingResult> CreateVouchersAsync(
        string transactionTypeCode,
        IReadOnlyList<AutoPostingInvoiceRow> rows,
        DateTime voucherDate,
        string usr)
    {
        var postable = rows.Where(x => x.CanPost).ToList();
        if (postable.Count == 0)
            return new AutoPostingResult { Success = false, Message = "Không có hóa đơn hợp lệ để hạch toán." };

        try
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var usedNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var created = new List<(M_AccountingVouchers Voucher, List<M_AccountingVoucherLines> Lines)>();

            foreach (var row in postable)
            {
                var invoiceNo = row.Group.InternalInvoiceNo.Trim();
                var voucher = new M_AccountingVouchers
                {
                    Id = Guid.NewGuid(),
                    CompanyId = row.Group.CustomerId,
                    VoucherNo = await NextVoucherNoAsync(db, voucherDate, usr, usedNos),
                    VoucherDate = voucherDate.Date,
                    PostingDate = voucherDate.Date,
                    FiscalYear = voucherDate.Year,
                    FiscalPeriod = voucherDate.Month,
                    TransactionTypeCode = transactionTypeCode,
                    Description = Truncate($"Hạch toán tự động HĐ {invoiceNo}", 500),
                    CurrencyCode = row.Currency,
                    ExchangeRate = row.Rate,
                    Status = 1,
                    ReferenceNo = Truncate(invoiceNo, 100),
                    ReferenceDate = row.Group.InvoiceDate,
                    SourceModule = SourceModuleName,
                    SourceId = InvoiceKey(row.Group),
                    CreatedBy = usr,
                    CreatedDate = DateTime.UtcNow,
                    Ghiso = false,
                    Approve = false
                };

                var lines = new List<M_AccountingVoucherLines>();
                var n = 1;
                foreach (var line in row.Lines)
                {
                    lines.Add(new M_AccountingVoucherLines
                    {
                        Id = Guid.NewGuid(),
                        VoucherId = voucher.Id,
                        LineNo_ = n.ToString(CultureInfo.InvariantCulture),
                        DanhMucTaiKhoanID = line.Account.AccountId,
                        AccountCode = line.Account.Code,
                        DebitAmount = line.Debit,
                        CreditAmount = line.Credit,
                        DebitAmountFC = line.DebitFC,
                        CreditAmountFC = line.CreditFC,
                        LineDescription = Truncate($"HĐ {invoiceNo} - {row.Group.CustomerName}", 500),
                        CustomerId = row.Group.CustomerId,
                        HBLNo = string.IsNullOrWhiteSpace(row.Group.HblNo) ? null : row.Group.HblNo,
                        InvoiceNo = invoiceNo,
                        InvoiceDate = row.Group.InvoiceDate,
                        IsTaxBook = false,
                        IsManagementBook = false,
                        thuho = row.Group.Thuho,
                        SortKey = n
                    });
                    n++;
                }

                db.AccountingVouchers.Add(voucher);
                db.AccountingVoucherLines.AddRange(lines);
                created.Add((voucher, lines));
            }

            await db.SaveChangesAsync();

            foreach (var (voucher, lines) in created)
            {
                try
                {
                    await historyLog.LogAsync(usr, "ADD", "PhieuKeToan", voucher.Id, voucher.VoucherNo, new { Voucher = voucher, Lines = lines });
                }
                catch
                {
                    // ghi lịch sử lỗi không được làm hỏng việc tạo chứng từ
                }
            }

            var nosCreated = created.Select(x => x.Voucher.VoucherNo).ToList();
            return new AutoPostingResult
            {
                Success = true,
                Message = $"Đã tạo {nosCreated.Count} chứng từ nháp: {string.Join(", ", nosCreated)}",
                VoucherNos = nosCreated
            };
        }
        catch (Exception ex)
        {
            return new AutoPostingResult { Success = false, Message = ex.InnerException?.Message ?? ex.Message };
        }
    }

    /// <summary>Số chứng từ theo cùng quy tắc form thêm chứng từ (Acc_yyyyMsố4chữ số) nhưng bảo đảm không trùng.</summary>
    private async Task<string> NextVoucherNoAsync(AppDbContext db, DateTime date, string usr, HashSet<string> usedNos)
    {
        var sequence = await supsv.GetRefNoByFunc("Account", date.Month, date.Year, usr) ?? 1;
        while (true)
        {
            var no = $"Acc_{date.Year}{date.Month}{sequence:D4}";
            if (!usedNos.Contains(no) && !await db.AccountingVouchers.AnyAsync(x => x.VoucherNo == no))
            {
                usedNos.Add(no);
                return no;
            }

            sequence++;
        }
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
