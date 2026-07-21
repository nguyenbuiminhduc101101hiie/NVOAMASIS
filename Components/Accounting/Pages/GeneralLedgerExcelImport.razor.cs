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
using System.Threading;
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
    public partial class GeneralLedgerExcelImport
    {
        private const long MaxFileSize = 20L * 1024 * 1024;

        private string? _fileName;
        private long _fileSize;
        private string? _fileHash;
        private string? _sheetName;
        private int _headerRow;
        private bool _isReading;
        private bool _isChecking;
        private bool _isSaving;
        private string _searchText = string.Empty;

        private string _companyIdText = string.Empty;
        private CustomerLookupItem? _selectedCustomer;
        private string _currencyCode = "VND";
        private decimal _exchangeRate = 1M;
        private string _branchCode = string.Empty;
        private string _createdBy = "SYSTEM";
        private string _sourceModule = "EXCEL_DEBT_LEDGER";
        private bool _useVoucherDateAsPostingDate = true;
        private DateTime? _postingDate = DateTime.Today;
        private bool _isTaxBook = true;
        private bool _isManagementBook = true;

        private string _sourceAccountCode = "1311";
        private string? _defaultCustomerName;
        private readonly List<string> _messages = new();
        private readonly List<LedgerSourceRow> _sourceRows = new();
        private readonly List<LedgerVoucherPreview> _vouchers = new();

        private IEnumerable<LedgerVoucherPreview> FilteredVouchers
        {
            get
            {
                var query = _vouchers.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var search = _searchText.Trim();
                    query = query.Where(x => x.SearchText.Contains(search, StringComparison.OrdinalIgnoreCase));
                }
                return query;
            }
        }

        private IEnumerable<LedgerLinePreview> FilteredLines
        {
            get
            {
                var query = _vouchers
                    .Where(x => x.Selected)
                    .SelectMany(x => x.Lines)
                    .AsEnumerable();

                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var search = _searchText.Trim();
                    query = query.Where(x => x.SearchText.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                return query;
            }
        }

        private IEnumerable<LedgerVoucherPreview> ImportableVouchers =>
            _vouchers.Where(x => x.Selected && x.IsValid && !x.ExistsInDatabase);

        private int SelectedLineCount => ImportableVouchers.Sum(x => x.Lines.Count);
        private decimal SelectedDebit => ImportableVouchers.Sum(x => x.TotalDebit);
        private decimal SelectedCredit => ImportableVouchers.Sum(x => x.TotalCredit);
        private decimal SelectedDifference => SelectedDebit - SelectedCredit;
        private Color BalanceColor => Math.Abs(SelectedDifference) < 0.01M ? Color.Success : Color.Error;

        private bool CanImport =>
            ImportableVouchers.Any()
            && Guid.TryParse(_companyIdText, out _)
            && _selectedCustomer is not null
            && _exchangeRate > 0
            && Math.Abs(SelectedDifference) < 0.01M;

        private string SelectedCustomerHelperText => _selectedCustomer is null
            ? "Gõ mã hoặc tên khách hàng để tìm"
            : $"CustomerId: {_selectedCustomer.CustomerId}";

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
        }

        private void OnCustomerChanged(CustomerLookupItem? customer)
        {
            _selectedCustomer = customer;
        }

        // Tương thích cả MudBlazor bản dùng SearchFunc(string)
        // và bản dùng SearchFunc(string, CancellationToken).
        private Task<IEnumerable<CustomerLookupItem>> SearchCustomersAsync(string value)
            => SearchCustomersCoreAsync(value, CancellationToken.None);

        private Task<IEnumerable<CustomerLookupItem>> SearchCustomersAsync(
            string value,
            CancellationToken cancellationToken)
            => SearchCustomersCoreAsync(value, cancellationToken);

        private async Task<IEnumerable<CustomerLookupItem>> SearchCustomersCoreAsync(
            string value,
            CancellationToken cancellationToken)
        {
            using var scope = ScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var query = db.Customer
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(value))
            {
                var keyword = value.Trim();

                query = query.Where(x =>
                    x.Customer_Code.Contains(keyword)
                    || (x.MainCode != null && x.MainCode.Contains(keyword))
                    || (x.MaDT != null && x.MaDT.Contains(keyword))
                    || (x.COMPANY != null && x.COMPANY.Contains(keyword))
                    || (x.EnglishName != null && x.EnglishName.Contains(keyword))
                    || (x.TaxCode != null && x.TaxCode.Contains(keyword)));
            }

            return await query
                .OrderBy(x => x.Customer_Code)
                .Select(x => new CustomerLookupItem
                {
                    CustomerId = x.Customer_ID,
                    CustomerCode = x.Customer_Code,
                    MainCode = x.MainCode ?? string.Empty,
                    CompanyName = x.COMPANY ?? string.Empty,
                    EnglishName = x.EnglishName ?? string.Empty,
                    TaxCode = x.TaxCode ?? string.Empty
                })
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        private async Task OnFileChanged(InputFileChangeEventArgs e)
        {
            ResetFileData();
            var file = e.File;
            _fileName = file.Name;
            _fileSize = file.Size;

            if (!file.Name.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                Snackbar.Add("Chỉ hỗ trợ file .xlsx.", MudBlazor.Severity.Error);
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
                await using var source = file.OpenReadStream(MaxFileSize);
                using var memory = new MemoryStream();
                await source.CopyToAsync(memory);
                var bytes = memory.ToArray();
                _fileHash = Convert.ToHexString(SHA256.HashData(bytes));

                var worksheet = SimpleXlsxReader.ReadFirstWorksheet(bytes);
                _sheetName = worksheet.Name;
                ParseWorksheet(worksheet);
                BuildPreviews();

                if (Guid.TryParse(_companyIdText, out _))
                    await CheckExistingAsync();

                Snackbar.Add(
                    $"Đã nhận diện {_sourceRows.Count:N0} dòng Excel, {_vouchers.Count:N0} chứng từ và {_vouchers.Sum(x => x.Lines.Count):N0} dòng sổ cái.",
                    MudBlazor.Severity.Success);
            }
            catch (Exception ex)
            {
                _messages.Add(ex.Message);
                Snackbar.Add("Không đọc được file Excel. Kiểm tra lại đúng mẫu sổ chi tiết công nợ.", MudBlazor.Severity.Error);
            }
            finally
            {
                _isReading = false;
            }
        }

        private void ParseWorksheet(SimpleXlsxWorksheet worksheet)
        {
            _sourceAccountCode = ReadAccountCode(worksheet) ?? "1311";
            _defaultCustomerName = ReadDefaultCustomerName(worksheet);
            _headerRow = FindHeaderRow(worksheet);

            if (_headerRow == 0)
                throw new InvalidDataException("Không tìm thấy dòng tiêu đề 'Ngày tháng chứng từ / Sổ phát sinh'.");

            var firstDataRow = _headerRow + 2;
            var lastRow = worksheet.LastRowNumber;

            for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);
                var dateText = CellText(row.Cell(1));
                var description = CellText(row.Cell(6));

                if (IsIgnoredRow(dateText, description))
                    continue;

                var voucherDate = ParseDate(row.Cell(1));
                var debitVoucherNo = CleanVoucherNo(CellText(row.Cell(2)));
                var creditVoucherNo = CleanVoucherNo(CellText(row.Cell(3)));
                var hasDebitVoucher = IsRealVoucherNo(debitVoucherNo);
                var hasCreditVoucher = IsRealVoucherNo(creditVoucherNo);

                if (!voucherDate.HasValue)
                {
                    _messages.Add($"Dòng {rowNumber}: ngày chứng từ không hợp lệ.");
                    continue;
                }

                if (hasDebitVoucher == hasCreditVoucher)
                {
                    _messages.Add($"Dòng {rowNumber}: không xác định được số chứng từ nằm ở cột Nợ hay Có.");
                    continue;
                }

                var debitMovement = ParseDecimal(row.Cell(8));
                var creditMovement = ParseDecimal(row.Cell(9));
                var amount = ResolveMovementAmount(debitMovement, creditMovement, rowNumber);
                if (amount <= 0)
                    continue;

                var contraAccount = CellText(row.Cell(7));
                if (string.IsNullOrWhiteSpace(contraAccount) || IsPlaceholder(contraAccount))
                {
                    _messages.Add($"Dòng {rowNumber}: thiếu tài khoản đối ứng.");
                    continue;
                }

                var customerName = CellText(row.Cell(5));
                if (string.IsNullOrWhiteSpace(customerName))
                    customerName = _defaultCustomerName ?? string.Empty;

                _sourceRows.Add(new LedgerSourceRow
                {
                    ExcelRow = rowNumber,
                    VoucherDate = voucherDate.Value.Date,
                    VoucherNo = hasCreditVoucher ? creditVoucherNo : debitVoucherNo,
                    IsCreditVoucher = hasCreditVoucher,
                    InvoiceNo = CellText(row.Cell(4)),
                    CustomerName = customerName,
                    Description = description,
                    ContraAccountCode = contraAccount.Trim(),
                    Amount = amount,
                    ReferenceDn = CellText(row.Cell(12)),
                    ReferenceNo = CellText(row.Cell(13))
                });
            }

            if (_sourceRows.Count == 0)
                throw new InvalidDataException("Không tìm thấy dòng phát sinh hợp lệ trong file Excel.");
        }

        private decimal ResolveMovementAmount(decimal debitMovement, decimal creditMovement, int rowNumber)
        {
            var debit = Math.Abs(debitMovement);
            var credit = Math.Abs(creditMovement);

            if (debit > 0 && credit > 0 && Math.Abs(debit - credit) >= 0.01M)
            {
                _messages.Add($"Dòng {rowNumber}: phát sinh Nợ {debit:N2} khác phát sinh Có {credit:N2}; dòng bị bỏ qua.");
                return 0;
            }

            return debit > 0 ? debit : credit;
        }

        private void BuildPreviews()
        {
            _vouchers.Clear();

            var groups = _sourceRows
                .GroupBy(x => new
                {
                    x.VoucherDate,
                    VoucherNo = x.VoucherNo.ToUpperInvariant(),
                    x.IsCreditVoucher
                })
                .OrderBy(x => x.Key.VoucherDate)
                .ThenBy(x => x.Key.VoucherNo);

            foreach (var group in groups)
            {
                var voucher = new LedgerVoucherPreview
                {
                    VoucherDate = group.Key.VoucherDate,
                    VoucherNo = group.First().VoucherNo,
                    IsCreditVoucher = group.Key.IsCreditVoucher,
                    CustomerName = group.Select(x => x.CustomerName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                    Selected = true
                };

                var lineNo = 1;

                if (voucher.IsCreditVoucher)
                {
                    foreach (var contraGroup in group.GroupBy(x => x.ContraAccountCode, StringComparer.OrdinalIgnoreCase))
                    {
                        var amount = contraGroup.Sum(x => x.Amount);
                        voucher.Lines.Add(new LedgerLinePreview
                        {
                            VoucherDate = voucher.VoucherDate,
                            VoucherNo = voucher.VoucherNo,
                            LineNo = lineNo++,
                            AccountCode = contraGroup.Key,
                            Debit = amount,
                            Credit = 0,
                            CustomerName = voucher.CustomerName,
                            InvoiceNo = contraGroup.Select(x => x.InvoiceNo).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                            SoHD = contraGroup.Select(x => x.InvoiceNo).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                            Description = Truncate($"Đối ứng thu/giảm công nợ TK {_sourceAccountCode} - CT {voucher.VoucherNo}", 500),
                            SourceRow = 0,
                            SourceKey = $"CONTRA|{contraGroup.Key}"
                        });
                    }

                    foreach (var sourceRow in group.OrderBy(x => x.ExcelRow))
                    {
                        voucher.Lines.Add(CreateDetailLine(voucher, sourceRow, lineNo++, 0, sourceRow.Amount));
                    }
                }
                else
                {
                    foreach (var sourceRow in group.OrderBy(x => x.ExcelRow))
                    {
                        voucher.Lines.Add(CreateDetailLine(voucher, sourceRow, lineNo++, sourceRow.Amount, 0));
                    }

                    foreach (var contraGroup in group.GroupBy(x => x.ContraAccountCode, StringComparer.OrdinalIgnoreCase))
                    {
                        var amount = contraGroup.Sum(x => x.Amount);
                        voucher.Lines.Add(new LedgerLinePreview
                        {
                            VoucherDate = voucher.VoucherDate,
                            VoucherNo = voucher.VoucherNo,
                            LineNo = lineNo++,
                            AccountCode = contraGroup.Key,
                            Debit = 0,
                            Credit = amount,
                            CustomerName = voucher.CustomerName,
                            InvoiceNo = contraGroup.Select(x => x.InvoiceNo).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                            SoHD = contraGroup.Select(x => x.InvoiceNo).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty,
                            Description = Truncate($"Đối ứng ghi tăng công nợ TK {_sourceAccountCode} - CT {voucher.VoucherNo}", 500),
                            SourceRow = 0,
                            SourceKey = $"CONTRA|{contraGroup.Key}"
                        });
                    }
                }

                ValidateVoucher(voucher);
                _vouchers.Add(voucher);
            }
        }

        private LedgerLinePreview CreateDetailLine(
            LedgerVoucherPreview voucher,
            LedgerSourceRow sourceRow,
            int lineNo,
            decimal debit,
            decimal credit)
        {
            var description = string.IsNullOrWhiteSpace(sourceRow.InvoiceNo)
                ? sourceRow.Description
                : $"{sourceRow.Description} - HĐ {sourceRow.InvoiceNo}";

            return new LedgerLinePreview
            {
                VoucherDate = voucher.VoucherDate,
                VoucherNo = voucher.VoucherNo,
                LineNo = lineNo,
                AccountCode = _sourceAccountCode,
                Debit = debit,
                Credit = credit,
                CustomerName = sourceRow.CustomerName,
                InvoiceNo = sourceRow.InvoiceNo,
                SoHD = sourceRow.InvoiceNo,
                Description = Truncate(description, 500),
                SourceRow = sourceRow.ExcelRow,
                SourceKey = $"ROW|{sourceRow.ExcelRow}|{sourceRow.InvoiceNo}|{sourceRow.ReferenceNo}"
            };
        }

        private static void ValidateVoucher(LedgerVoucherPreview voucher)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(voucher.VoucherNo))
                errors.Add("Thiếu số chứng từ");
            if (voucher.Lines.Count < 2)
                errors.Add("Chứng từ phải có ít nhất 2 dòng");
            if (voucher.Lines.Any(x => string.IsNullOrWhiteSpace(x.AccountCode)))
                errors.Add("Có dòng thiếu AccountCode");
            if (voucher.Lines.Any(x => x.Debit < 0 || x.Credit < 0 || (x.Debit > 0 && x.Credit > 0) || (x.Debit == 0 && x.Credit == 0)))
                errors.Add("Mỗi dòng chỉ được có Nợ hoặc Có");
            if (Math.Abs(voucher.TotalDebit - voucher.TotalCredit) >= 0.01M)
                errors.Add($"Nợ/Có lệch {voucher.TotalDebit - voucher.TotalCredit:N2}");

            voucher.ValidationMessage = string.Join("; ", errors);
            voucher.IsValid = errors.Count == 0;
            if (!voucher.IsValid)
                voucher.Selected = false;
        }

        private async Task CheckExistingAsync()
        {
            if (_vouchers.Count == 0)
                return;

            if (!Guid.TryParse(_companyIdText, out var companyId))
            {
                Snackbar.Add("CompanyId không hợp lệ.", MudBlazor.Severity.Error);
                return;
            }

            _isChecking = true;
            try
            {
                using var scope = ScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var connection = db.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                var existing = await ReadExistingVoucherKeysAsync(connection, null, companyId, _sourceModule, _vouchers);
                foreach (var voucher in _vouchers)
                {
                    voucher.ExistsInDatabase = existing.Contains(BuildVoucherKey(voucher.VoucherDate.Year, voucher.VoucherNo));
                    if (voucher.ExistsInDatabase)
                        voucher.Selected = false;
                }

                Snackbar.Add($"Đã kiểm tra trùng. Có {_vouchers.Count(x => x.ExistsInDatabase):N0} chứng từ đã tồn tại.", MudBlazor.Severity.Info);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Không kiểm tra được dữ liệu trùng: {ex.Message}", MudBlazor.Severity.Error);
            }
            finally
            {
                _isChecking = false;
            }
        }

        private async Task ImportAsync()
        {
            if (!Guid.TryParse(_companyIdText, out var companyId))
            {
                Snackbar.Add("CompanyId không hợp lệ.", MudBlazor.Severity.Error);
                return;
            }

            if (_selectedCustomer is null)
            {
                Snackbar.Add("Vui lòng chọn khách hàng trước khi import.", MudBlazor.Severity.Warning);
                return;
            }

            var customerId = _selectedCustomer.CustomerId;

            if (_exchangeRate <= 0)
            {
                Snackbar.Add("ExchangeRate phải lớn hơn 0.", MudBlazor.Severity.Error);
                return;
            }

            var selected = ImportableVouchers.ToList();
            if (selected.Count == 0)
            {
                Snackbar.Add("Không có chứng từ hợp lệ để import.", MudBlazor.Severity.Warning);
                return;
            }

            if (selected.Any(x => Math.Abs(x.TotalDebit - x.TotalCredit) >= 0.01M))
            {
                Snackbar.Add("Có chứng từ chưa cân Nợ/Có.", MudBlazor.Severity.Error);
                return;
            }

            _isSaving = true;
            try
            {
                using var scope = ScopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var connection = db.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);

                var existing = await ReadExistingVoucherKeysAsync(connection, transaction, companyId, _sourceModule, selected);
                if (existing.Count > 0)
                {
                    await transaction.RollbackAsync();
                    foreach (var voucher in selected)
                    {
                        if (existing.Contains(BuildVoucherKey(voucher.VoucherDate.Year, voucher.VoucherNo)))
                        {
                            voucher.ExistsInDatabase = true;
                            voucher.Selected = false;
                        }
                    }
                    Snackbar.Add("Có chứng từ vừa được import trước đó. Hệ thống đã hủy toàn bộ lần import này.", MudBlazor.Severity.Warning);
                    return;
                }

                var insertedLines = 0;
                foreach (var voucher in selected)
                {
                    var voucherId = CreateDeterministicGuid($"{companyId:N}|{voucher.VoucherDate.Year}|{voucher.VoucherNo}|{_sourceModule}");
                    var postingDate = (_useVoucherDateAsPostingDate ? voucher.VoucherDate : (_postingDate ?? DateTime.Today)).Date;

                    foreach (var line in voucher.Lines)
                    {
                        await InsertLedgerLineAsync(
                            connection,
                            transaction,
                            voucherId,
                            companyId,
                            voucher,
                            line,
                            postingDate,
                            customerId);
                        insertedLines++;
                    }
                }

                await transaction.CommitAsync();

                foreach (var voucher in selected)
                {
                    voucher.ExistsInDatabase = true;
                    voucher.Selected = false;
                }

                Snackbar.Add($"Đã import {selected.Count:N0} chứng từ / {insertedLines:N0} dòng vào GeneralLedgerEntries.", MudBlazor.Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Import thất bại, dữ liệu đã rollback: {ex.Message}", MudBlazor.Severity.Error);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private async Task InsertLedgerLineAsync(
            DbConnection connection,
            DbTransaction transaction,
            Guid voucherId,
            Guid companyId,
            LedgerVoucherPreview voucher,
            LedgerLinePreview line,
            DateTime postingDate,
            Guid? customerId)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO dbo.GeneralLedgerEntries
(
    Id, VoucherId, VoucherLineId, CompanyId,
    FiscalYear, FiscalPeriod, PostingDate, VoucherDate, VoucherNo,
    AccountCode, Debit, Credit, CurrencyCode, ExchangeRate, DebitFC, CreditFC,
    CustomerId, SupplierId, EmployeeId, ShipmentId, ContractId,
    BranchCode, Description, SourceModule, SourceId, CreatedBy,
    IsTaxBook, IsManagementBook, LedgerType, SoHD
)
VALUES
(
    @Id, @VoucherId, NULL, @CompanyId,
    @FiscalYear, @FiscalPeriod, @PostingDate, @VoucherDate, @VoucherNo,
    @AccountCode, @Debit, @Credit, @CurrencyCode, @ExchangeRate, @DebitFC, @CreditFC,
    @CustomerId, NULL, NULL, NULL, NULL,
    @BranchCode, @Description, @SourceModule, @SourceId, @CreatedBy,
    @IsTaxBook, @IsManagementBook, @LedgerType, @SoHD
);";

            AddParameter(command, "@Id", Guid.NewGuid());
            AddParameter(command, "@VoucherId", voucherId);
            AddParameter(command, "@CompanyId", companyId);
            AddParameter(command, "@FiscalYear", voucher.VoucherDate.Year);
            AddParameter(command, "@FiscalPeriod", voucher.VoucherDate.Month);
            AddParameter(command, "@PostingDate", postingDate);
            AddParameter(command, "@VoucherDate", voucher.VoucherDate);
            AddParameter(command, "@VoucherNo", Truncate(voucher.VoucherNo, 50));
            AddParameter(command, "@AccountCode", Truncate(line.AccountCode, 20));
            AddParameter(command, "@Debit", line.Debit);
            AddParameter(command, "@Credit", line.Credit);
            AddParameter(command, "@CurrencyCode", Truncate(string.IsNullOrWhiteSpace(_currencyCode) ? "VND" : _currencyCode.Trim().ToUpperInvariant(), 10));
            AddParameter(command, "@ExchangeRate", _exchangeRate);
            AddParameter(command, "@DebitFC", Math.Round(line.Debit / _exchangeRate, 2, MidpointRounding.AwayFromZero));
            AddParameter(command, "@CreditFC", Math.Round(line.Credit / _exchangeRate, 2, MidpointRounding.AwayFromZero));
            AddParameter(command, "@CustomerId", customerId);
            AddParameter(command, "@BranchCode", DbNullIfEmpty(Truncate(_branchCode?.Trim(), 50)));
            AddParameter(command, "@Description", DbNullIfEmpty(Truncate(line.Description, 500)));
            AddParameter(command, "@SourceModule", Truncate(string.IsNullOrWhiteSpace(_sourceModule) ? "EXCEL_DEBT_LEDGER" : _sourceModule.Trim(), 100));
            AddParameter(command, "@SourceId", BuildSourceId(voucher, line));
            AddParameter(command, "@CreatedBy", DbNullIfEmpty(Truncate(_createdBy?.Trim(), 100)));
            AddParameter(command, "@IsTaxBook", _isTaxBook);
            AddParameter(command, "@IsManagementBook", _isManagementBook);
            AddParameter(command, "@LedgerType", CurrentLedgerType);
            AddParameter(command, "@SoHD", DbNullIfEmpty(Truncate(line.SoHD, 200)));

            await command.ExecuteNonQueryAsync();
        }

        private static async Task<HashSet<string>> ReadExistingVoucherKeysAsync(
            DbConnection connection,
            DbTransaction? transaction,
            Guid companyId,
            string sourceModule,
            IEnumerable<LedgerVoucherPreview> vouchers)
        {
            var uniqueVouchers = vouchers
                .GroupBy(x => BuildVoucherKey(x.VoucherDate.Year, x.VoucherNo), StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (uniqueVouchers.Count == 0)
                return result;

            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            AddParameter(command, "@CompanyId", companyId);
            AddParameter(command, "@SourceModule", Truncate(string.IsNullOrWhiteSpace(sourceModule) ? "EXCEL_DEBT_LEDGER" : sourceModule.Trim(), 100));

            var conditions = new List<string>();
            for (var index = 0; index < uniqueVouchers.Count; index++)
            {
                var yearParameter = $"@Year{index}";
                var voucherParameter = $"@Voucher{index}";
                conditions.Add($"(FiscalYear = {yearParameter} AND VoucherNo = {voucherParameter})");
                AddParameter(command, yearParameter, uniqueVouchers[index].VoucherDate.Year);
                AddParameter(command, voucherParameter, Truncate(uniqueVouchers[index].VoucherNo, 50));
            }

            command.CommandText = $@"
SELECT DISTINCT FiscalYear, VoucherNo
FROM dbo.GeneralLedgerEntries WITH (UPDLOCK, HOLDLOCK)
WHERE CompanyId = @CompanyId
  AND SourceModule = @SourceModule
  AND ({string.Join(" OR ", conditions)});";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(BuildVoucherKey(reader.GetInt32(0), reader.GetString(1)));
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
            _sourceAccountCode = "1311";
            _defaultCustomerName = null;
        }

        private string CurrentLedgerType => _isTaxBook && _isManagementBook
            ? "BOTH"
            : _isTaxBook ? "TAX" : _isManagementBook ? "MANAGEMENT" : "OTHER";

        private string BuildSourceId(LedgerVoucherPreview voucher, LedgerLinePreview line)
        {
            var raw = $"{_fileHash}|{voucher.VoucherDate:yyyyMMdd}|{voucher.VoucherNo}|{line.LineNo}|{line.SourceKey}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)))[..64];
        }

        private static Guid CreateDeterministicGuid(string value)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            var bytes = new byte[16];
            Array.Copy(hash, bytes, 16);
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
            return new Guid(bytes);
        }

        private static string? ReadAccountCode(SimpleXlsxWorksheet worksheet)
        {
            var maxRow = Math.Min(worksheet.LastRowNumber, 30);
            for (var row = 1; row <= maxRow; row++)
            {
                for (var column = 1; column <= Math.Min(worksheet.LastColumnNumber, 13); column++)
                {
                    var text = CellText(worksheet.Cell(row, column));
                    var match = Regex.Match(text, @"\bSỔ\s+([0-9A-Z.]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                        return match.Groups[1].Value.Trim();
                }
            }
            return null;
        }

        private static string? ReadDefaultCustomerName(SimpleXlsxWorksheet worksheet)
        {
            var maxRow = Math.Min(worksheet.LastRowNumber, 30);
            for (var row = 1; row <= maxRow; row++)
            {
                for (var column = 1; column <= Math.Min(worksheet.LastColumnNumber, 13); column++)
                {
                    var text = CellText(worksheet.Cell(row, column));
                    if (text.StartsWith("KHÁCH HÀNG:", StringComparison.OrdinalIgnoreCase))
                    {
                        var separator = text.IndexOf(':');
                        return separator >= 0 ? text[(separator + 1)..].Trim() : text.Trim();
                    }
                }
            }
            return null;
        }

        private static int FindHeaderRow(SimpleXlsxWorksheet worksheet)
        {
            var maxRow = Math.Min(worksheet.LastRowNumber, 50);
            for (var row = 1; row <= maxRow; row++)
            {
                var first = Normalize(CellText(worksheet.Cell(row, 1)));
                var eighth = Normalize(CellText(worksheet.Cell(row, 8)));
                if (first.Contains("ngay thang") && first.Contains("chung tu") && eighth.Contains("phat sinh"))
                    return row;
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

        private static bool IsRealVoucherNo(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || IsPlaceholder(value))
                return false;
            return value.Any(char.IsLetter) || value.Length >= 5;
        }

        private static bool IsPlaceholder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            var cleaned = value.Trim();
            return cleaned == "18" || cleaned == "0" || cleaned == "-";
        }

        private static string CleanVoucherNo(string? value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

        private static DateTime? ParseDate(SimpleXlsxCell cell)
        {
            if (cell.DateValue.HasValue)
                return cell.DateValue.Value.Date;

            var text = CellText(cell);
            var formats = new[]
            {
                "d/M/yyyy", "dd/MM/yyyy", "d/M/yy", "dd/MM/yy",
                "yyyy-MM-dd", "M/d/yyyy", "MM/dd/yyyy"
            };

            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces, out var exact))
                return exact.Date;

            if (DateTime.TryParse(text, CultureInfo.GetCultureInfo("vi-VN"),
                    DateTimeStyles.AllowWhiteSpaces, out var viDate))
                return viDate.Date;

            return null;
        }

        private static decimal ParseDecimal(SimpleXlsxCell cell)
        {
            if (cell.NumericValue.HasValue)
                return Convert.ToDecimal(cell.NumericValue.Value, CultureInfo.InvariantCulture);

            var text = CellText(cell);
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariant))
                return invariant;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("vi-VN"), out var vi))
                return vi;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out var en))
                return en;

            return 0;
        }

        private static string CellText(SimpleXlsxCell cell) => cell.Text.Trim();

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static void AddParameter(DbCommand command, string name, object? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        private static object DbNullIfEmpty(string? value)
            => string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;

        private static string BuildVoucherKey(int fiscalYear, string voucherNo)
            => $"{fiscalYear}|{voucherNo.Trim()}";

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static string? FindClaim(System.Security.Claims.ClaimsPrincipal user, params string[] names)
        {
            foreach (var name in names)
            {
                var value = user.Claims
                    .FirstOrDefault(x => string.Equals(x.Type, name, StringComparison.OrdinalIgnoreCase))
                    ?.Value;
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
            return null;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes:N0} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024D:N1} KB";
            return $"{bytes / 1024D / 1024D:N1} MB";
        }

        private sealed class CustomerLookupItem
        {
            public Guid CustomerId { get; set; }
            public string CustomerCode { get; set; } = string.Empty;
            public string MainCode { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string EnglishName { get; set; } = string.Empty;
            public string TaxCode { get; set; } = string.Empty;

            public string DisplayText
            {
                get
                {
                    var code = !string.IsNullOrWhiteSpace(CustomerCode)
                        ? CustomerCode
                        : MainCode;

                    var name = !string.IsNullOrWhiteSpace(CompanyName)
                        ? CompanyName
                        : EnglishName;

                    if (string.IsNullOrWhiteSpace(code))
                        return name;

                    if (string.IsNullOrWhiteSpace(name))
                        return code;

                    return $"{code} - {name}";
                }
            }
        }

        private sealed class LedgerSourceRow
        {
            public int ExcelRow { get; set; }
            public DateTime VoucherDate { get; set; }
            public string VoucherNo { get; set; } = string.Empty;
            public bool IsCreditVoucher { get; set; }
            public string InvoiceNo { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ContraAccountCode { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string ReferenceDn { get; set; } = string.Empty;
            public string ReferenceNo { get; set; } = string.Empty;
        }

        private sealed class LedgerVoucherPreview
        {
            public DateTime VoucherDate { get; set; }
            public string VoucherNo { get; set; } = string.Empty;
            public bool IsCreditVoucher { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public bool Selected { get; set; }
            public bool IsValid { get; set; }
            public bool ExistsInDatabase { get; set; }
            public string ValidationMessage { get; set; } = string.Empty;
            public List<LedgerLinePreview> Lines { get; } = new();
            public decimal TotalDebit => Lines.Sum(x => x.Debit);
            public decimal TotalCredit => Lines.Sum(x => x.Credit);
            public string SearchText => $"{VoucherNo} {CustomerName} {string.Join(" ", Lines.Select(x => x.SearchText))}";
        }

        private sealed class LedgerLinePreview
        {
            public DateTime VoucherDate { get; set; }
            public string VoucherNo { get; set; } = string.Empty;
            public int LineNo { get; set; }
            public string AccountCode { get; set; } = string.Empty;
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string InvoiceNo { get; set; } = string.Empty;
            public string SoHD { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int SourceRow { get; set; }
            public string SourceKey { get; set; } = string.Empty;
            public string SearchText => $"{VoucherNo} {AccountCode} {InvoiceNo} {SoHD} {Description} {CustomerName}";
        }
    }
}