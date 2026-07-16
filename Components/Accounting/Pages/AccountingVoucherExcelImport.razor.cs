using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NVOAMASIS.Data;

namespace NVOAMASIS.Components.Accounting.Pages
{
public partial class AccountingVoucherExcelImport
{

    private const long MaxFileSize = 20L * 1024 * 1024;

    private string? _fileName;
    private long _fileSize;
    private string? _fileHash;
    private string? _sheetName;
    private int _headerRow;
    private bool _isReading;
    private bool _isSaving;
    private bool _isChecking;
    private string _searchText = string.Empty;
    private string _companyIdText = string.Empty;
    private string _customerIdText = string.Empty;
    private string _transactionTypeCode = "UNT";
    private string _currencyCode = "VND";
    private decimal _exchangeRate = 1M;
    private string _sourceAccountCode = "1311";
    private string _createdBy = "SYSTEM";
    private bool _isTaxBook = true;
    private bool _isManagementBook = true;

    private readonly List<string> _messages = new();
    private readonly List<string> _transactionTypes = new();
    private readonly List<SourceExcelRow> _sourceRows = new();
    private readonly List<VoucherPreview> _vouchers = new();
    private ImportMeta _meta = new();

    private IEnumerable<VoucherPreview> FilteredVouchers => string.IsNullOrWhiteSpace(_searchText)
        ? _vouchers
        : _vouchers.Where(v => v.SearchText.Contains(_searchText.Trim(), StringComparison.OrdinalIgnoreCase));

    private IEnumerable<GeneratedLine> FilteredGeneratedLines
    {
        get
        {
            var query = _vouchers.SelectMany(v => v.Lines.Select(l => new GeneratedLine
            {
                VoucherNo = v.VoucherNo,
                LineNo = l.LineNo,
                AccountCode = l.AccountCode,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount,
                Description = l.Description,
                InvoiceNo = l.InvoiceNo,
                SourceRow = l.SourceRow,
                SortKey = l.SortKey
            }));
            if (!string.IsNullOrWhiteSpace(_searchText))
                query = query.Where(l => l.SearchText.Contains(_searchText.Trim(), StringComparison.OrdinalIgnoreCase));
            return query;
        }
    }

    private int SelectedVoucherCount => _vouchers.Count(v => v.Selected && v.IsValid && !v.ExistsInDatabase);
    private decimal SelectedDebit => _vouchers.Where(v => v.Selected && v.IsValid && !v.ExistsInDatabase).Sum(v => v.TotalDebit);
    private decimal SelectedCredit => _vouchers.Where(v => v.Selected && v.IsValid && !v.ExistsInDatabase).Sum(v => v.TotalCredit);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = state.User;
            _createdBy = user.Identity?.Name
                         ?? user.FindFirst("email")?.Value
                         ?? user.FindFirst("preferred_username")?.Value
                         ?? "SYSTEM";

            _companyIdText = FindClaim(user, "CompanyId", "companyId", "company_id") ?? string.Empty;
        }
        catch
        {
            _createdBy = "SYSTEM";
        }

        await LoadTransactionTypesAsync();
    }

    private async Task LoadTransactionTypesAsync()
    {
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Code FROM dbo.TransactionTypes ORDER BY Code";
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var code = reader.GetString(0).Trim();
                if (!string.IsNullOrWhiteSpace(code))
                    _transactionTypes.Add(code);
            }

            if (_transactionTypes.Count > 0 && !_transactionTypes.Contains(_transactionTypeCode, StringComparer.OrdinalIgnoreCase))
                _transactionTypeCode = _transactionTypes[0];
        }
        catch (Exception ex)
        {
            _messages.Add($"Không đọc được TransactionTypes: {ex.Message}");
        }
    }

    private async Task OnFileChanged(InputFileChangeEventArgs e)
    {
        ResetFileData();
        var file = e.File;
        _fileName = file.Name;
        _fileSize = file.Size;

        if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add("Chỉ hỗ trợ file Excel .xlsx.", MudBlazor.Severity.Error);
            return;
        }

        if (file.Size > MaxFileSize)
        {
            Snackbar.Add("File vượt quá 20 MB.", MudBlazor.Severity.Error);
            return;
        }

        _isReading = true;
        try
        {
            await using var input = file.OpenReadStream(MaxFileSize);
            using var memory = new MemoryStream();
            await input.CopyToAsync(memory);
            var bytes = memory.ToArray();
            _fileHash = Convert.ToHexString(SHA256.HashData(bytes));

            var worksheet = SimpleXlsxReader.ReadFirstWorksheet(bytes);
            _sheetName = worksheet.Name;
            ParseWorksheet(worksheet);
            BuildVoucherPreview();

            var detectedType = DetectTransactionType(_vouchers.FirstOrDefault()?.VoucherNo);
            if (!string.IsNullOrWhiteSpace(detectedType)
                && (_transactionTypes.Count == 0 || _transactionTypes.Contains(detectedType, StringComparer.OrdinalIgnoreCase)))
            {
                _transactionTypeCode = detectedType;
            }

            if (Guid.TryParse(_companyIdText, out _))
                await CheckExistingAsync();

            Snackbar.Add($"Đã nhận diện {_sourceRows.Count:N0} dòng Excel và {_vouchers.Count:N0} voucher.", MudBlazor.Severity.Success);
        }
        catch (Exception ex)
        {
            _messages.Add(ex.Message);
            Snackbar.Add("Không đọc được file. Kiểm tra lại đúng mẫu sổ chi tiết công nợ.", MudBlazor.Severity.Error);
        }
        finally
        {
            _isReading = false;
        }
    }

    private void ParseWorksheet(SimpleXlsxWorksheet ws)
    {
        _meta = ReadMeta(ws);
        _sourceAccountCode = string.IsNullOrWhiteSpace(_meta.AccountCode) ? "1311" : _meta.AccountCode!;
        _headerRow = FindHeaderRow(ws);

        if (_headerRow == 0)
            throw new InvalidDataException("Không tìm thấy dòng tiêu đề 'Ngày tháng chứng từ'.");

        var lastRow = ws.LastRowNumber > 0 ? ws.LastRowNumber : _headerRow;
        var firstDataRow = _headerRow + 2;

        for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
        {
            var row = ws.Row(rowNumber);
            var dateText = CellText(row.Cell(1));
            var description = CellText(row.Cell(6));

            // Dòng “Tổng cộng TK” đánh dấu kết thúc vùng dữ liệu; bỏ toàn bộ phần chữ ký/ghi chú phía sau.
            if (Normalize(description).Contains("tong cong tk"))
                break;

            if (IsIgnoredRow(dateText, description))
                continue;

            var hasContent = Enumerable.Range(1, 13).Any(c => !string.IsNullOrWhiteSpace(CellText(row.Cell(c))));
            if (!hasContent)
                continue;

            var source = new SourceExcelRow
            {
                SourceRow = rowNumber,
                VoucherDate = ParseDate(row.Cell(1)),
                DebitVoucherNo = CellText(row.Cell(2)),
                CreditVoucherNo = CellText(row.Cell(3)),
                InvoiceNo = CellText(row.Cell(4)),
                CustomerName = NullIfEmpty(CellText(row.Cell(5))) ?? _meta.DefaultCustomerName ?? string.Empty,
                Description = description,
                ContraAccount = CellText(row.Cell(7)),
                DebitMovement = ParseDecimal(row.Cell(8)),
                CreditMovement = ParseDecimal(row.Cell(9)),
                RefDn = CellText(row.Cell(12)),
                RefNo = CellText(row.Cell(13))
            };

            ValidateSourceRow(source);
            _sourceRows.Add(source);
        }

        if (_sourceRows.Count == 0)
            throw new InvalidDataException("Không tìm thấy dòng phát sinh hợp lệ trong file.");
    }

    private void BuildVoucherPreview()
    {
        _vouchers.Clear();

        var grouped = _sourceRows.GroupBy(r => new
        {
            VoucherNo = r.VoucherNo,
            Date = r.VoucherDate?.Date,
            r.IsCreditVoucher
        });

        foreach (var group in grouped.OrderBy(g => g.Key.Date).ThenBy(g => g.Key.VoucherNo))
        {
            var voucher = new VoucherPreview
            {
                VoucherNo = group.Key.VoucherNo,
                VoucherDate = group.Key.Date ?? DateTime.MinValue,
                IsCreditVoucher = group.Key.IsCreditVoucher,
                CustomerName = group.Select(x => x.CustomerName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                RefNo = group.Select(x => x.RefNo).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            };
            voucher.SourceRows.AddRange(group);

            foreach (var error in group.SelectMany(x => x.Errors).Distinct())
                voucher.Errors.Add(error);

            BuildGeneratedLines(voucher);
            ValidateVoucher(voucher);
            voucher.Selected = voucher.IsValid;
            _vouchers.Add(voucher);
        }

        var duplicateNumbers = _vouchers.GroupBy(v => v.VoucherNo, StringComparer.OrdinalIgnoreCase)
                                        .Where(g => g.Select(x => x.VoucherDate.Date).Distinct().Count() > 1)
                                        .Select(g => g.Key)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var voucher in _vouchers.Where(v => duplicateNumbers.Contains(v.VoucherNo)))
        {
            voucher.Errors.Add("Cùng số voucher xuất hiện ở nhiều ngày khác nhau");
            voucher.Selected = false;
        }
    }

    private void BuildGeneratedLines(VoucherPreview voucher)
    {
        voucher.Lines.Clear();
        var lineNo = 1;

        foreach (var contraGroup in voucher.SourceRows.GroupBy(r => r.ContraAccount, StringComparer.OrdinalIgnoreCase))
        {
            var amount = contraGroup.Sum(r => r.Amount);
            voucher.Lines.Add(new GeneratedLine
            {
                LineNo = (lineNo++).ToString(CultureInfo.InvariantCulture),
                AccountCode = contraGroup.Key,
                DebitAmount = voucher.IsCreditVoucher ? amount : 0,
                CreditAmount = voucher.IsCreditVoucher ? 0 : amount,
                Description = BuildContraDescription(voucher, contraGroup.Key),
                SourceRow = 0,
                SortKey = lineNo - 1
            });
        }

        foreach (var row in voucher.SourceRows.OrderBy(r => r.SourceRow))
        {
            voucher.Lines.Add(new GeneratedLine
            {
                LineNo = (lineNo++).ToString(CultureInfo.InvariantCulture),
                AccountCode = _sourceAccountCode,
                DebitAmount = voucher.IsCreditVoucher ? 0 : row.Amount,
                CreditAmount = voucher.IsCreditVoucher ? row.Amount : 0,
                Description = row.Description,
                InvoiceNo = NullIfEmpty(row.InvoiceNo),
                SourceRow = row.SourceRow,
                SortKey = lineNo - 1
            });
        }
    }

    private static void ValidateSourceRow(SourceExcelRow row)
    {
        if (row.VoucherDate is null)
            row.Errors.Add($"Dòng {row.SourceRow}: ngày chứng từ không hợp lệ");

        if (string.IsNullOrWhiteSpace(row.DebitVoucherNo) && string.IsNullOrWhiteSpace(row.CreditVoucherNo))
            row.Errors.Add($"Dòng {row.SourceRow}: thiếu số chứng từ Nợ/Có");

        if (!string.IsNullOrWhiteSpace(row.DebitVoucherNo) && !string.IsNullOrWhiteSpace(row.CreditVoucherNo))
            row.Errors.Add($"Dòng {row.SourceRow}: đồng thời có cả số chứng từ Nợ và Có");

        if (string.IsNullOrWhiteSpace(row.ContraAccount))
            row.Errors.Add($"Dòng {row.SourceRow}: thiếu tài khoản đối ứng");
        else if (row.ContraAccount.Length > 20)
            row.Errors.Add($"Dòng {row.SourceRow}: tài khoản đối ứng vượt quá 20 ký tự");

        if (row.Amount <= 0)
            row.Errors.Add($"Dòng {row.SourceRow}: số tiền phải lớn hơn 0");

        if (row.DebitMovement > 0 && row.CreditMovement > 0
            && Math.Abs(row.DebitMovement - row.CreditMovement) > 0.01M)
        {
            row.Errors.Add($"Dòng {row.SourceRow}: phát sinh Nợ và Có không bằng nhau, không xác định được số tiền");
        }
    }

    private void ValidateVoucher(VoucherPreview voucher)
    {
        if (string.IsNullOrWhiteSpace(voucher.VoucherNo))
            voucher.Errors.Add("Thiếu số voucher");
        else if (voucher.VoucherNo.Length > 50)
            voucher.Errors.Add("Số voucher vượt quá 50 ký tự");
        if (voucher.VoucherDate == DateTime.MinValue)
            voucher.Errors.Add("Thiếu ngày voucher");
        if (voucher.Lines.Count < 2)
            voucher.Errors.Add("Voucher phải có ít nhất 2 dòng");
        if (voucher.Lines.Any(l => string.Equals(l.AccountCode, _sourceAccountCode, StringComparison.OrdinalIgnoreCase)
                                   && voucher.SourceRows.Any(r => string.Equals(r.ContraAccount, _sourceAccountCode, StringComparison.OrdinalIgnoreCase))))
            voucher.Errors.Add("Tài khoản công nợ trùng tài khoản đối ứng");
        if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) > 0.01M)
            voucher.Errors.Add($"Voucher không cân: Nợ {voucher.TotalDebit:N2}, Có {voucher.TotalCredit:N2}");
    }

    private async Task CheckExistingAsync()
    {
        if (_vouchers.Count == 0)
            return;

        if (!Guid.TryParse(_companyIdText, out var companyId))
        {
            Snackbar.Add("CompanyId không đúng định dạng GUID.", MudBlazor.Severity.Warning);
            return;
        }

        _isChecking = true;
        try
        {
            using var scope = ScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var connection = db.Database.GetDbConnection();
            await connection.OpenAsync();

            var voucherKeys = _vouchers
                .GroupBy(BuildVoucherKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
            if (voucherKeys.Count == 0)
                return;

            await using var command = connection.CreateCommand();
            var conditions = new List<string>();
            AddParameter(command, "@CompanyId", companyId);
            for (var i = 0; i < voucherKeys.Count; i++)
            {
                var noName = $"@No{i}";
                var yearName = $"@Year{i}";
                conditions.Add($"(VoucherNo = {noName} AND FiscalYear = {yearName})");
                AddParameter(command, noName, voucherKeys[i].VoucherNo);
                AddParameter(command, yearName, voucherKeys[i].VoucherDate.Year);
            }

            command.CommandText = $@"
SELECT FiscalYear, VoucherNo
FROM dbo.AccountingVouchers
WHERE CompanyId = @CompanyId
  AND ({string.Join(" OR ", conditions)});";

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                existing.Add(BuildVoucherKey(reader.GetInt32(0), reader.GetString(1)));

            foreach (var voucher in _vouchers)
            {
                voucher.ExistsInDatabase = existing.Contains(BuildVoucherKey(voucher));
                if (voucher.ExistsInDatabase)
                    voucher.Selected = false;
            }

            Snackbar.Add(existing.Count == 0
                ? "Không phát hiện voucher trùng trong database."
                : $"Có {existing.Count:N0} voucher đã tồn tại và đã được bỏ chọn.",
                existing.Count == 0 ? MudBlazor.Severity.Success : MudBlazor.Severity.Warning);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Không kiểm tra được voucher trùng: {ex.Message}", MudBlazor.Severity.Error);
        }
        finally
        {
            _isChecking = false;
        }
    }

    private async Task SaveAsync()
    {
        if (!Guid.TryParse(_companyIdText, out var companyId))
        {
            Snackbar.Add("CompanyId không đúng định dạng GUID.", MudBlazor.Severity.Error);
            return;
        }

        Guid? customerId = null;
        if (!string.IsNullOrWhiteSpace(_customerIdText))
        {
            if (!Guid.TryParse(_customerIdText, out var parsedCustomerId))
            {
                Snackbar.Add("CustomerId không đúng định dạng GUID.", MudBlazor.Severity.Error);
                return;
            }
            customerId = parsedCustomerId;
        }

        if (string.IsNullOrWhiteSpace(_transactionTypeCode))
        {
            Snackbar.Add("Chưa chọn loại chứng từ.", MudBlazor.Severity.Error);
            return;
        }

        if (_transactionTypeCode.Trim().Length > 50)
        {
            Snackbar.Add("Loại chứng từ vượt quá 50 ký tự.", MudBlazor.Severity.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(_currencyCode) || _currencyCode.Trim().Length > 10)
        {
            Snackbar.Add("Mã tiền tệ phải có từ 1 đến 10 ký tự.", MudBlazor.Severity.Error);
            return;
        }

        if (_exchangeRate <= 0)
        {
            Snackbar.Add("Tỷ giá phải lớn hơn 0.", MudBlazor.Severity.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(_sourceAccountCode) || _sourceAccountCode.Trim().Length > 20)
        {
            Snackbar.Add("Tài khoản công nợ phải có từ 1 đến 20 ký tự.", MudBlazor.Severity.Error);
            return;
        }

        if (!_isTaxBook && !_isManagementBook)
        {
            Snackbar.Add("Phải chọn ít nhất Sổ thuế hoặc Sổ quản trị.", MudBlazor.Severity.Error);
            return;
        }

        var selected = _vouchers.Where(v => v.Selected && v.IsValid && !v.ExistsInDatabase).ToList();
        if (selected.Count == 0)
            return;

        _isSaving = true;
        using var scope = ScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            if (!await TransactionTypeExistsAsync(connection, transaction, _transactionTypeCode))
                throw new InvalidOperationException($"TransactionTypeCode '{_transactionTypeCode}' không tồn tại trong dbo.TransactionTypes.");

            var existing = await ReadExistingVoucherNosAsync(connection, transaction, companyId, selected);
            if (existing.Count > 0)
                throw new InvalidOperationException($"Voucher đã tồn tại: {string.Join(", ", existing.OrderBy(x => x))}");

            var accountCodes = selected.SelectMany(v => v.Lines).Select(l => l.AccountCode)
                                       .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var accountIds = await ResolveAccountIdsAsync(connection, transaction, accountCodes);

            foreach (var voucher in selected)
            {
                var voucherId = Guid.NewGuid();
                await InsertVoucherAsync(connection, transaction, voucherId, companyId, voucher);

                foreach (var line in voucher.Lines)
                {
                    accountIds.TryGetValue(line.AccountCode, out var accountId);
                    await InsertVoucherLineAsync(connection, transaction, voucherId, line, customerId, accountId);
                }
            }

            await transaction.CommitAsync();

            foreach (var voucher in selected)
            {
                voucher.ExistsInDatabase = true;
                voucher.Selected = false;
            }

            Snackbar.Add($"Đã tạo thành công {selected.Count:N0} voucher nháp, gồm {selected.Sum(v => v.Lines.Count):N0} dòng hạch toán.", MudBlazor.Severity.Success);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Snackbar.Add($"Import thất bại, toàn bộ giao dịch đã rollback: {ex.Message}", MudBlazor.Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task InsertVoucherAsync(DbConnection connection, DbTransaction transaction, Guid voucherId,
                                          Guid companyId, VoucherPreview voucher)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO dbo.AccountingVouchers
(
    Id, CompanyId, VoucherNo, VoucherDate, PostingDate, FiscalYear, FiscalPeriod,
    TransactionTypeCode, Description, CurrencyCode, ExchangeRate, Status,
    ReferenceNo, ReferenceDate, BookScope, SourceModule, SourceId,
    CreatedBy, CreatedDate, Ghiso, Approve, Both
)
VALUES
(
    @Id, @CompanyId, @VoucherNo, @VoucherDate, @PostingDate, @FiscalYear, @FiscalPeriod,
    @TransactionTypeCode, @Description, @CurrencyCode, @ExchangeRate, 0,
    @ReferenceNo, @ReferenceDate, @BookScope, N'DEBT_EXCEL_IMPORT', @SourceId,
    @CreatedBy, SYSUTCDATETIME(), 0, 0, @Both
);";

        AddParameter(command, "@Id", voucherId);
        AddParameter(command, "@CompanyId", companyId);
        AddParameter(command, "@VoucherNo", voucher.VoucherNo);
        AddParameter(command, "@VoucherDate", voucher.VoucherDate.Date);
        AddParameter(command, "@PostingDate", voucher.VoucherDate.Date);
        AddParameter(command, "@FiscalYear", voucher.VoucherDate.Year);
        AddParameter(command, "@FiscalPeriod", voucher.VoucherDate.Month);
        AddParameter(command, "@TransactionTypeCode", _transactionTypeCode.Trim());
        AddParameter(command, "@Description", Truncate(BuildVoucherDescription(voucher), 500));
        AddParameter(command, "@CurrencyCode", _currencyCode.Trim().ToUpperInvariant());
        AddParameter(command, "@ExchangeRate", _exchangeRate);
        AddParameter(command, "@ReferenceNo", Truncate(NullIfEmpty(voucher.RefNo) ?? voucher.VoucherNo, 100));
        AddParameter(command, "@ReferenceDate", voucher.VoucherDate.Date);
        AddParameter(command, "@BookScope", CurrentBookScope);
        AddParameter(command, "@SourceId", BuildSourceId(voucher.VoucherNo));
        AddParameter(command, "@CreatedBy", Truncate(string.IsNullOrWhiteSpace(_createdBy) ? "SYSTEM" : _createdBy.Trim(), 100));
        AddParameter(command, "@Both", _isTaxBook && _isManagementBook);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertVoucherLineAsync(DbConnection connection, DbTransaction transaction, Guid voucherId,
                                              GeneratedLine line, Guid? customerId, Guid? accountId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
INSERT INTO dbo.AccountingVoucherLines
(
    Id, VoucherId, LineNo_, AccountCode,
    DebitAmount, CreditAmount, DebitAmountFC, CreditAmountFC,
    LineDescription, CustomerId, IsTaxBook, IsManagementBook,
    LedgerType, InvoiceNo, SortKey, DanhMucTaiKhoanID
)
VALUES
(
    NEWID(), @VoucherId, @LineNo, @AccountCode,
    @DebitAmount, @CreditAmount, @DebitAmountFC, @CreditAmountFC,
    @LineDescription, @CustomerId, @IsTaxBook, @IsManagementBook,
    @LedgerType, @InvoiceNo, @SortKey, @DanhMucTaiKhoanID
);";

        AddParameter(command, "@VoucherId", voucherId);
        AddParameter(command, "@LineNo", line.LineNo);
        AddParameter(command, "@AccountCode", line.AccountCode);
        AddParameter(command, "@DebitAmount", line.DebitAmount);
        AddParameter(command, "@CreditAmount", line.CreditAmount);
        var isLocalCurrency = string.Equals(_currencyCode.Trim(), "VND", StringComparison.OrdinalIgnoreCase);
        AddParameter(command, "@DebitAmountFC", isLocalCurrency ? 0 : Math.Round(line.DebitAmount / _exchangeRate, 2));
        AddParameter(command, "@CreditAmountFC", isLocalCurrency ? 0 : Math.Round(line.CreditAmount / _exchangeRate, 2));
        AddParameter(command, "@LineDescription", NullIfEmpty(Truncate(line.Description, 500)));
        AddParameter(command, "@CustomerId", customerId);
        AddParameter(command, "@IsTaxBook", _isTaxBook);
        AddParameter(command, "@IsManagementBook", _isManagementBook);
        AddParameter(command, "@LedgerType", CurrentBookScope);
        AddParameter(command, "@InvoiceNo", NullIfEmpty(Truncate(line.InvoiceNo, 100)));
        AddParameter(command, "@SortKey", line.SortKey);
        AddParameter(command, "@DanhMucTaiKhoanID", accountId);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> TransactionTypeExistsAsync(DbConnection connection, DbTransaction transaction, string code)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT COUNT(1) FROM dbo.TransactionTypes WHERE Code = @Code";
        AddParameter(command, "@Code", code.Trim());
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture) > 0;
    }

    private static async Task<HashSet<string>> ReadExistingVoucherNosAsync(DbConnection connection, DbTransaction transaction,
                                                                           Guid companyId, IEnumerable<VoucherPreview> vouchers)
    {
        var values = vouchers.GroupBy(BuildVoucherKey, StringComparer.OrdinalIgnoreCase)
                             .Select(g => g.First())
                             .ToList();
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (values.Count == 0)
            return result;

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        AddParameter(command, "@CompanyId", companyId);
        var conditions = new List<string>();
        for (var i = 0; i < values.Count; i++)
        {
            var noName = $"@PNo{i}";
            var yearName = $"@PYear{i}";
            conditions.Add($"(VoucherNo = {noName} AND FiscalYear = {yearName})");
            AddParameter(command, noName, values[i].VoucherNo);
            AddParameter(command, yearName, values[i].VoucherDate.Year);
        }

        command.CommandText = $@"
SELECT FiscalYear, VoucherNo
FROM dbo.AccountingVouchers WITH (UPDLOCK, HOLDLOCK)
WHERE CompanyId = @CompanyId
  AND ({string.Join(" OR ", conditions)});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(BuildVoucherKey(reader.GetInt32(0), reader.GetString(1)));
        return result;
    }

    private static async Task<Dictionary<string, Guid>> ResolveAccountIdsAsync(DbConnection connection, DbTransaction transaction,
                                                                                IReadOnlyCollection<string> accountCodes)
    {
        var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        if (accountCodes.Count == 0)
            return result;

        string? codeColumn = null;
        await using (var columnCommand = connection.CreateCommand())
        {
            columnCommand.Transaction = transaction;
            columnCommand.CommandText = @"
SELECT TOP (1) c.name
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = N'dbo'
  AND t.name = N'DanhMucTaiKhoan'
  AND c.name IN (N'AccountCode', N'MaTaiKhoan', N'MaTK', N'Code', N'SoHieuTK', N'account_code')
ORDER BY CASE c.name
    WHEN N'AccountCode' THEN 1
    WHEN N'MaTaiKhoan' THEN 2
    WHEN N'MaTK' THEN 3
    WHEN N'Code' THEN 4
    WHEN N'SoHieuTK' THEN 5
    ELSE 6 END;";
            codeColumn = Convert.ToString(await columnCommand.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
        }

        var allowedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "AccountCode", "MaTaiKhoan", "MaTK", "Code", "SoHieuTK", "account_code"
        };
        if (string.IsNullOrWhiteSpace(codeColumn) || !allowedColumns.Contains(codeColumn))
            return result;

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var names = new List<string>();
        var codes = accountCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        for (var i = 0; i < codes.Count; i++)
        {
            var name = $"@A{i}";
            names.Add(name);
            AddParameter(command, name, codes[i]);
        }

        command.CommandText = $@"
SELECT id, CAST([{codeColumn}] AS nvarchar(50))
FROM dbo.DanhMucTaiKhoan
WHERE CAST([{codeColumn}] AS nvarchar(50)) IN ({string.Join(",", names)});";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                result[reader.GetString(1).Trim()] = reader.GetGuid(0);
        }
        return result;
    }

    private void SelectAllValid()
    {
        foreach (var voucher in _vouchers)
            voucher.Selected = voucher.IsValid && !voucher.ExistsInDatabase;
    }

    private void ClearSelection()
    {
        foreach (var voucher in _vouchers)
            voucher.Selected = false;
    }

    private void Reset()
    {
        ResetFileData();
        _fileName = null;
        _fileSize = 0;
    }

    private void ResetFileData()
    {
        _fileHash = null;
        _sheetName = null;
        _headerRow = 0;
        _searchText = string.Empty;
        _sourceRows.Clear();
        _vouchers.Clear();
        _messages.Clear();
        _meta = new ImportMeta();
    }

    private string CurrentBookScope => _isTaxBook && _isManagementBook
        ? "BOTH"
        : _isTaxBook ? "TAX" : "MANAGEMENT";

    private string BuildSourceId(string voucherNo)
    {
        var raw = $"{_fileHash}|{voucherNo}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)))[..40];
    }

    private string BuildVoucherDescription(VoucherPreview voucher)
    {
        var action = voucher.IsCreditVoucher ? "Thu/giảm công nợ" : "Ghi tăng công nợ";
        return $"{action} {voucher.CustomerName} theo chứng từ {voucher.VoucherNo}";
    }

    private static string BuildContraDescription(VoucherPreview voucher, string contraAccount)
    {
        var action = voucher.IsCreditVoucher ? "Đối ứng thu/giảm công nợ" : "Đối ứng ghi tăng công nợ";
        return $"{action} {voucher.CustomerName} - TK {contraAccount} - CT {voucher.VoucherNo}";
    }

    private static ImportMeta ReadMeta(SimpleXlsxWorksheet ws)
    {
        var meta = new ImportMeta();
        var maxRow = Math.Min(ws.LastRowNumber > 0 ? ws.LastRowNumber : 30, 30);
        var all = new List<string>();

        for (var r = 1; r <= maxRow; r++)
        {
            for (var c = 1; c <= Math.Min(ws.LastColumnNumber > 0 ? ws.LastColumnNumber : 13, 13); c++)
            {
                var text = CellText(ws.Cell(r, c));
                if (!string.IsNullOrWhiteSpace(text))
                    all.Add(text);
            }
        }

        var title = all.FirstOrDefault(x => x.StartsWith("SỔ ", StringComparison.OrdinalIgnoreCase));
        if (title is not null)
        {
            var match = Regex.Match(title, @"SỔ\s+([0-9A-Z.]+)", RegexOptions.IgnoreCase);
            if (match.Success)
                meta.AccountCode = match.Groups[1].Value.Trim();
        }

        var period = all.FirstOrDefault(x => x.Contains("Từ ngày", StringComparison.OrdinalIgnoreCase)
                                          && x.Contains("đến ngày", StringComparison.OrdinalIgnoreCase));
        if (period is not null)
        {
            var dates = Regex.Matches(period, @"\d{1,2}/\d{1,2}/\d{4}")
                             .Cast<Match>()
                             .Select(x => ParseDateText(x.Value))
                             .Where(x => x.HasValue)
                             .Select(x => x!.Value)
                             .ToList();
            if (dates.Count > 0) meta.FromDate = dates[0];
            if (dates.Count > 1) meta.ToDate = dates[1];
        }

        var customer = all.FirstOrDefault(x => x.StartsWith("KHÁCH HÀNG:", StringComparison.OrdinalIgnoreCase));
        if (customer is not null)
            meta.DefaultCustomerName = customer[(customer.IndexOf(':') + 1)..].Trim();

        return meta;
    }

    private static int FindHeaderRow(SimpleXlsxWorksheet ws)
    {
        var max = Math.Min(ws.LastRowNumber > 0 ? ws.LastRowNumber : 50, 50);
        for (var r = 1; r <= max; r++)
        {
            var a = Normalize(CellText(ws.Cell(r, 1)));
            var h = Normalize(CellText(ws.Cell(r, 8)));
            if (a.Contains("ngay thang") && a.Contains("chung tu") && h.Contains("phat sinh"))
                return r;
        }
        return 0;
    }

    private static bool IsIgnoredRow(string dateText, string description)
    {
        var normalized = Normalize(description);
        if (string.IsNullOrWhiteSpace(dateText) && string.IsNullOrWhiteSpace(description))
            return true;
        return normalized.Contains("so dau ky")
               || normalized.Contains("tong cong tk")
               || normalized.Contains("nguoi lap bieu")
               || normalized.Contains("ke toan truong")
               || normalized.Contains("giam doc");
    }

    private static string? DetectTransactionType(string? voucherNo)
    {
        if (string.IsNullOrWhiteSpace(voucherNo))
            return null;
        var match = Regex.Match(voucherNo.ToUpperInvariant(), @"(?<=\d)([A-Z]{2,10})(?=\d)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string? FindClaim(System.Security.Claims.ClaimsPrincipal user, params string[] names)
    {
        foreach (var name in names)
        {
            var value = user.Claims.FirstOrDefault(c => string.Equals(c.Type, name, StringComparison.OrdinalIgnoreCase))?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    private static string BuildVoucherKey(VoucherPreview voucher)
        => BuildVoucherKey(voucher.VoucherDate.Year, voucher.VoucherNo);

    private static string BuildVoucherKey(int fiscalYear, string voucherNo)
        => $"{fiscalYear}|{voucherNo.Trim()}";

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value ?? string.Empty;
        return value[..maxLength];
    }

    private static string CellText(SimpleXlsxCell cell) => cell.Text.Trim();

    private static DateTime? ParseDate(SimpleXlsxCell cell)
    {
        if (cell.DateValue.HasValue)
            return cell.DateValue.Value.Date;
        if (cell.NumericValue.HasValue)
        {
            try
            {
                return DateTime.FromOADate(cell.NumericValue.Value).Date;
            }
            catch
            {
                // Fall through to text parsing.
            }
        }
        return ParseDateText(CellText(cell));
    }

    private static DateTime? ParseDateText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd" };
        return DateTime.TryParseExact(text.Trim(), formats, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var value) ? value.Date : null;
    }

    private static decimal ParseDecimal(SimpleXlsxCell cell)
    {
        var text = CellText(cell);
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // Numeric cells are stored as invariant text in the .xlsx package. Parse decimal first
        // to avoid the rounding noise that can occur when converting through double.
        if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant))
            return invariant;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out var vi))
            return vi;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var en))
            return en;
        return cell.NumericValue.HasValue
            ? Convert.ToDecimal(cell.NumericValue.Value, CultureInfo.InvariantCulture)
            : 0;
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC).ToLowerInvariant().Replace('\n', ' ').Replace('\r', ' ');
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatFileSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024d:N1} KB",
        _ => $"{bytes / 1024d / 1024d:N1} MB"
    };

    private sealed class ImportMeta
    {
        public string? AccountCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DefaultCustomerName { get; set; }
    }

    private sealed class SourceExcelRow
    {
        public int SourceRow { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string DebitVoucherNo { get; set; } = string.Empty;
        public string CreditVoucherNo { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ContraAccount { get; set; } = string.Empty;
        public decimal DebitMovement { get; set; }
        public decimal CreditMovement { get; set; }
        public string RefDn { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public List<string> Errors { get; } = new();
        public bool IsCreditVoucher => string.IsNullOrWhiteSpace(DebitVoucherNo) && !string.IsNullOrWhiteSpace(CreditVoucherNo);
        public string VoucherNo => !string.IsNullOrWhiteSpace(DebitVoucherNo) ? DebitVoucherNo.Trim() : CreditVoucherNo.Trim();
        public decimal Amount => IsCreditVoucher
            ? (CreditMovement > 0 ? CreditMovement : DebitMovement)
            : (DebitMovement > 0 ? DebitMovement : CreditMovement);
    }

    private sealed class VoucherPreview
    {
        public string VoucherNo { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public bool IsCreditVoucher { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public bool Selected { get; set; }
        public bool ExistsInDatabase { get; set; }
        public List<string> Errors { get; } = new();
        public List<SourceExcelRow> SourceRows { get; } = new();
        public List<GeneratedLine> Lines { get; } = new();
        public bool IsValid => Errors.Count == 0;
        public decimal TotalDebit => Lines.Sum(l => l.DebitAmount);
        public decimal TotalCredit => Lines.Sum(l => l.CreditAmount);
        public string SearchText => string.Join(" ",
            new[] { VoucherNo, CustomerName, RefNo }
                .Concat(SourceRows.SelectMany(r => new[] { r.InvoiceNo, r.Description, r.ContraAccount })));
    }

    private sealed class GeneratedLine
    {
        public string VoucherNo { get; set; } = string.Empty;
        public string LineNo { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? InvoiceNo { get; set; }
        public int SourceRow { get; set; }
        public int SortKey { get; set; }
        public string SearchText => string.Join(" ", new[] { VoucherNo, AccountCode, InvoiceNo ?? string.Empty, Description });
    }
}
}
