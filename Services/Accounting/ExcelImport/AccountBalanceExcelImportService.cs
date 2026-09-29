using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Accounting.ExcelImport;
using NVOAMASIS.Options;

namespace NVOAMASIS.Services.Accounting.ExcelImport;

public sealed class AccountBalanceExcelImportService : IAccountBalanceExcelImportService
{
    private const decimal VndTolerance = 1m;
    private const decimal ForeignTolerance = 0.01m;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly AccountBalanceImportOptions _options;

    public AccountBalanceExcelImportService(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IOptions<AccountBalanceImportOptions> options)
    {
        _dbContextFactory = dbContextFactory;
        _options = options.Value;
    }

    public async Task<AccountBalanceExcelPreview> ReadAndValidateAsync(
        Stream excelStream,
        string fileName,
        ExcelAccountingImportTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelStream);

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("Tên file Excel không hợp lệ.");

        var extension = Path.GetExtension(fileName);
        if (!extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Chỉ hỗ trợ file Excel định dạng .xlsx hoặc .xlsm.");
        }

        await using var buffer = new MemoryStream();
        await excelStream.CopyToAsync(buffer, cancellationToken);

        if (buffer.Length <= 0)
            throw new InvalidOperationException("File Excel không có dữ liệu.");

        if (buffer.Length > _options.MaximumFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File vượt quá giới hạn {_options.MaximumFileSizeBytes / 1024 / 1024:N0} MB.");
        }

        var fileBytes = buffer.ToArray();
        var preview = new AccountBalanceExcelPreview
        {
            FileName = Path.GetFileName(fileName),
            FileHash = Convert.ToHexString(SHA256.HashData(fileBytes)),
            Target = target
        };

        if (target == ExcelAccountingImportTarget.GeneralLedger)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message =
                    "File đang tải là sổ công nợ tổng hợp theo khách hàng, " +
                    "không có ngày chứng từ, số chứng từ, tài khoản Nợ/Có và số tiền từng nghiệp vụ. " +
                    "Vì vậy không được nhập trực tiếp vào GeneralLedgerEntries. " +
                    "Hãy chọn đích nhập là Account Balance."
            });
        }

        try
        {
            using var workbookStream = new MemoryStream(fileBytes, writable: false);
            using var workbook = new XLWorkbook(workbookStream);

            if (workbook.Worksheets.Count == 0)
                throw new InvalidOperationException("Workbook không có worksheet.");

            var worksheet = workbook.Worksheet(1);
            ParseWorksheet(worksheet, preview, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Không thể đọc file Excel. Hãy kiểm tra file có bị khóa, lỗi hoặc sai định dạng không.",
                ex);
        }

        return preview;
    }

    public async Task<AccountBalanceImportResult> CommitAsync(
        CommitAccountBalanceImportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Preview);

        if (request.CompanyId == Guid.Empty)
            throw new InvalidOperationException("Không xác định được công ty cần nhập dữ liệu.");

        if (!request.Preview.CanImport)
            throw new InvalidOperationException("Dữ liệu còn lỗi nên chưa thể import.");

        if (request.Preview.Lines.Count > _options.MaximumRows)
        {
            throw new InvalidOperationException(
                $"Số dòng vượt giới hạn {_options.MaximumRows:N0} dòng cho một lần import.");
        }

        if (request.Preview.FromDate is null || request.Preview.ToDate is null)
            throw new InvalidOperationException("Không xác định được khoảng ngày của báo cáo.");

        if (request.Preview.FiscalYear is < 2000 or > 2200 ||
            request.Preview.PeriodNo is < 1 or > 12)
        {
            throw new InvalidOperationException("Năm hoặc tháng nhập số dư không hợp lệ.");
        }

        var userName = string.IsNullOrWhiteSpace(request.UserName)
            ? "system"
            : request.UserName.Trim();

        var currencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode)
            ? "VND"
            : request.CurrencyCode.Trim().ToUpperInvariant();

        var bookCode = NormalizeOptional(request.BookCode);

        // Không tin số tổng có sẵn trong object từ giao diện.
        // Luôn tính lại từ toàn bộ dòng chi tiết đã kiểm tra.
        var totals = CalculateTotals(request.Preview.Lines);
        request.Preview.CalculatedTotals = totals;

        ValidateAggregateTotals(totals);
        var accountType = ResolveAccountType(request.AccountType, totals);

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Khóa sổ kỳ kế toán (10.8): không nhập số dư vào kỳ đã khóa.
        await NVOAMASIS.Services.Accounting.AccountingPeriodLock.EnsureOpenAsync(db,
            request.Preview.FiscalYear, request.Preview.PeriodNo,
            $"nhập số dư tài khoản kỳ {request.Preview.PeriodNo:D2}/{request.Preview.FiscalYear}", cancellationToken);

        var connection = (SqlConnection)db.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await EnsureTargetSchemaAsync(connection, cancellationToken);
        await EnsureAuditSchemaAsync(connection, cancellationToken);
        await EnsureFileNotImportedAsync(
            connection,
            request.CompanyId,
            request.Preview,
            bookCode,
            cancellationToken);

        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        var batchId = Guid.NewGuid();

        try
        {
            await InsertAuditBatchAsync(
                connection,
                transaction,
                batchId,
                request,
                userName,
                currencyCode,
                bookCode,
                accountType,
                totals,
                cancellationToken);

            await BulkCopyAuditLinesAsync(
                connection,
                transaction,
                batchId,
                request.Preview.Lines,
                cancellationToken);

            var actionResult = await UpsertAccountSummaryAsync(
                connection,
                transaction,
                request,
                bookCode,
                accountType,
                totals,
                cancellationToken);

            await CompleteAuditBatchAsync(
                connection,
                transaction,
                batchId,
                actionResult,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new AccountBalanceImportResult
            {
                BatchId = batchId,
                InsertedCount = actionResult.Inserted,
                UpdatedCount = actionResult.Updated,
                DeletedCount = actionResult.Deleted,
                ImportedRowCount = request.Preview.Lines.Count
            };
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private void ParseWorksheet(
        IXLWorksheet worksheet,
        AccountBalanceExcelPreview preview,
        CancellationToken cancellationToken)
    {
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        if (lastRow == 0 || lastColumn == 0)
            throw new InvalidOperationException("Worksheet đầu tiên không có dữ liệu.");

        preview.CompanyNameInFile = ReadText(worksheet.Cell(1, 1));
        preview.ReportTitle = ReadText(worksheet.Cell(4, 1));
        preview.UnitText = ReadText(worksheet.Cell(6, 1));

        var companyDetail = ReadText(worksheet.Cell(2, 1));
        var taxCodeMatch = Regex.Match(
            companyDetail,
            @"(?:MST|Mã\s*số\s*thuế)\s*:\s*(?<TaxCode>[0-9\-]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (taxCodeMatch.Success)
            preview.TaxCodeInFile = taxCodeMatch.Groups["TaxCode"].Value.Trim();

        var periodAndAccountText = string.Join(
            Environment.NewLine,
            Enumerable.Range(1, Math.Min(lastRow, 10))
                .Select(row => ReadText(worksheet.Cell(row, 1)))
                .Where(text => !string.IsNullOrWhiteSpace(text)));

        ParseDatesAndAccount(periodAndAccountText, preview);

        preview.HeaderRowNo = FindHeaderRow(worksheet, lastRow);
        if (preview.HeaderRowNo <= 0)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message =
                    "Không tìm thấy tiêu đề cột 'Mã KH' và 'Tên KH'. " +
                    "File không đúng mẫu sổ công nợ tổng hợp."
            });
            return;
        }

        var firstDataRow = preview.HeaderRowNo + 2;
        var totalRow = FindTotalRow(worksheet, firstDataRow, lastRow);
        preview.TotalRowNo = totalRow > 0 ? totalRow : null;

        var finalDataRow = totalRow > 0 ? totalRow - 1 : lastRow;

        for (var row = firstDataRow; row <= finalDataRow; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var customerCode = ReadText(worksheet.Cell(row, 2));
            var customerName = ReadText(worksheet.Cell(row, 3));

            if (string.IsNullOrWhiteSpace(customerCode) &&
                string.IsNullOrWhiteSpace(customerName))
            {
                continue;
            }

            var normalizedCustomerName = NormalizeForSearch(customerName);
            if (normalizedCustomerName.Contains("tong cong", StringComparison.Ordinal))
                continue;

            var line = new AccountBalanceExcelLine
            {
                ExcelRowNo = row,
                SequenceNo = ReadNullableInt(worksheet.Cell(row, 1)),
                CustomerCode = customerCode.Trim(),
                CustomerName = customerName.Trim(),

                OpeningForeignAmount = ReadDecimal(worksheet.Cell(row, 4)),
                OpeningDebit = ReadDecimal(worksheet.Cell(row, 5)),
                OpeningCredit = ReadDecimal(worksheet.Cell(row, 6)),

                PeriodForeignAmount = ReadDecimal(worksheet.Cell(row, 7)),
                PeriodDebit = ReadDecimal(worksheet.Cell(row, 8)),
                PeriodCredit = ReadDecimal(worksheet.Cell(row, 9)),

                ClosingForeignAmount = ReadDecimal(worksheet.Cell(row, 10)),
                ClosingDebit = ReadDecimal(worksheet.Cell(row, 11)),
                ClosingCredit = ReadDecimal(worksheet.Cell(row, 12))
            };

            ValidateLine(line);
            preview.Lines.Add(line);
        }

        if (preview.Lines.Count == 0)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message = "Không tìm thấy dòng công nợ khách hàng để import."
            });
            return;
        }

        if (preview.Lines.Count > _options.MaximumRows)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message =
                    $"File có {preview.Lines.Count:N0} dòng, vượt giới hạn " +
                    $"{_options.MaximumRows:N0} dòng."
            });
        }

        MarkDuplicateCustomerCodes(preview.Lines);
        preview.CalculatedTotals = CalculateTotals(preview.Lines);

        if (totalRow > 0)
        {
            preview.WorkbookTotals = ReadTotals(worksheet, totalRow);
            CompareWorkbookTotals(preview);
        }
        else
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Warning,
                Message =
                    "Không tìm thấy dòng Tổng cộng. Hệ thống vẫn tính tổng từ các dòng chi tiết."
            });
        }

        if (string.IsNullOrWhiteSpace(preview.AccountCode))
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message = "Không đọc được mã tài khoản từ phần tiêu đề báo cáo."
            });
        }

        if (preview.FromDate is null || preview.ToDate is null)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message = "Không đọc được khoảng thời gian báo cáo."
            });
        }
        else if (preview.FromDate > preview.ToDate)
        {
            preview.Messages.Add(new ExcelImportMessage
            {
                Level = ExcelImportMessageLevel.Error,
                Message = "Từ ngày không được lớn hơn đến ngày."
            });
        }
    }

    private static void ParseDatesAndAccount(
        string sourceText,
        AccountBalanceExcelPreview preview)
    {
        var dateMatch = Regex.Match(
            sourceText,
            @"Từ\s*ngày\s*:\s*(?<From>\d{1,2}/\d{1,2}/\d{4}).*?" +
            @"đến\s*ngày\s*:\s*(?<To>\d{1,2}/\d{1,2}/\d{4})",
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline |
            RegexOptions.CultureInvariant);

        if (dateMatch.Success)
        {
            if (DateTime.TryParseExact(
                    dateMatch.Groups["From"].Value,
                    "d/M/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var fromDate))
            {
                preview.FromDate = fromDate.Date;
            }

            if (DateTime.TryParseExact(
                    dateMatch.Groups["To"].Value,
                    "d/M/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var toDate))
            {
                preview.ToDate = toDate.Date;
                preview.FiscalYear = toDate.Year;
                preview.PeriodNo = toDate.Month;
            }
        }

        var accountMatch = Regex.Match(
            sourceText,
            @"(?:^|[\r\n])\s*(?<Code>\d{3,})\s*-\s*(?<Name>[^\r\n]+)",
            RegexOptions.IgnoreCase |
            RegexOptions.Multiline |
            RegexOptions.CultureInvariant);

        if (accountMatch.Success)
        {
            preview.AccountCode = accountMatch.Groups["Code"].Value.Trim();
            preview.AccountName = accountMatch.Groups["Name"].Value.Trim();
        }
    }

    private static int FindHeaderRow(IXLWorksheet worksheet, int lastRow)
    {
        for (var row = 1; row <= lastRow; row++)
        {
            var columnB = NormalizeForSearch(ReadText(worksheet.Cell(row, 2)));
            var columnC = NormalizeForSearch(ReadText(worksheet.Cell(row, 3)));

            if (columnB.Contains("ma kh", StringComparison.Ordinal) &&
                columnC.Contains("ten kh", StringComparison.Ordinal))
            {
                return row;
            }
        }

        return 0;
    }

    private static int FindTotalRow(
        IXLWorksheet worksheet,
        int firstDataRow,
        int lastRow)
    {
        for (var row = firstDataRow; row <= lastRow; row++)
        {
            var customerName = NormalizeForSearch(ReadText(worksheet.Cell(row, 3)));
            if (customerName.Contains("tong cong", StringComparison.Ordinal))
                return row;
        }

        return 0;
    }

    private static void ValidateLine(AccountBalanceExcelLine line)
    {
        if (string.IsNullOrWhiteSpace(line.CustomerCode))
            line.Errors.Add("Thiếu mã khách hàng.");

        if (string.IsNullOrWhiteSpace(line.CustomerName))
            line.Errors.Add("Thiếu tên khách hàng.");

        var vndValues = new[]
        {
            line.OpeningDebit,
            line.OpeningCredit,
            line.PeriodDebit,
            line.PeriodCredit,
            line.ClosingDebit,
            line.ClosingCredit
        };

        if (vndValues.Any(value => value < 0))
            line.Errors.Add("Số tiền Nợ/Có không được âm.");

        if (line.OpeningDebit > 0 && line.OpeningCredit > 0)
            line.Errors.Add("Số dư đầu kỳ đồng thời có cả Nợ và Có.");

        if (line.ClosingDebit > 0 && line.ClosingCredit > 0)
            line.Errors.Add("Số dư cuối kỳ đồng thời có cả Nợ và Có.");

        var expectedClosingNet =
            line.OpeningDebit -
            line.OpeningCredit +
            line.PeriodDebit -
            line.PeriodCredit;

        var actualClosingNet = line.ClosingDebit - line.ClosingCredit;
        if (Math.Abs(expectedClosingNet - actualClosingNet) > VndTolerance)
        {
            line.Errors.Add(
                "Không cân: Dư đầu kỳ + Phát sinh Nợ - Phát sinh Có khác dư cuối kỳ.");
        }

        var expectedForeignClosing =
            line.OpeningForeignAmount + line.PeriodForeignAmount;

        if (Math.Abs(expectedForeignClosing - line.ClosingForeignAmount) > ForeignTolerance)
        {
            line.Warnings.Add(
                "Số ngoại tệ đầu kỳ + phát sinh khác số ngoại tệ cuối kỳ.");
        }
    }

    private static void MarkDuplicateCustomerCodes(
        IReadOnlyCollection<AccountBalanceExcelLine> lines)
    {
        var duplicateGroups = lines
            .Where(line => !string.IsNullOrWhiteSpace(line.CustomerCode))
            .GroupBy(
                line => line.CustomerCode.Trim(),
                StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            foreach (var line in group)
            {
                line.Errors.Add(
                    $"Mã khách hàng '{group.Key}' bị trùng trong file.");
            }
        }
    }

    private static AccountBalanceImportTotals CalculateTotals(
        IEnumerable<AccountBalanceExcelLine> lines)
    {
        var result = new AccountBalanceImportTotals();

        foreach (var line in lines)
        {
            result.OpeningForeignAmount += line.OpeningForeignAmount;
            result.OpeningDebit += line.OpeningDebit;
            result.OpeningCredit += line.OpeningCredit;

            result.PeriodForeignAmount += line.PeriodForeignAmount;
            result.PeriodDebit += line.PeriodDebit;
            result.PeriodCredit += line.PeriodCredit;

            result.ClosingForeignAmount += line.ClosingForeignAmount;
            result.ClosingDebit += line.ClosingDebit;
            result.ClosingCredit += line.ClosingCredit;
        }

        return result;
    }

    private static AccountBalanceImportTotals ReadTotals(
        IXLWorksheet worksheet,
        int row)
    {
        return new AccountBalanceImportTotals
        {
            OpeningForeignAmount = ReadDecimal(worksheet.Cell(row, 4)),
            OpeningDebit = ReadDecimal(worksheet.Cell(row, 5)),
            OpeningCredit = ReadDecimal(worksheet.Cell(row, 6)),

            PeriodForeignAmount = ReadDecimal(worksheet.Cell(row, 7)),
            PeriodDebit = ReadDecimal(worksheet.Cell(row, 8)),
            PeriodCredit = ReadDecimal(worksheet.Cell(row, 9)),

            ClosingForeignAmount = ReadDecimal(worksheet.Cell(row, 10)),
            ClosingDebit = ReadDecimal(worksheet.Cell(row, 11)),
            ClosingCredit = ReadDecimal(worksheet.Cell(row, 12))
        };
    }

    private static void CompareWorkbookTotals(AccountBalanceExcelPreview preview)
    {
        if (preview.WorkbookTotals is null)
            return;

        CompareTotal(
            preview,
            "Ngoại tệ đầu kỳ",
            preview.CalculatedTotals.OpeningForeignAmount,
            preview.WorkbookTotals.OpeningForeignAmount,
            ForeignTolerance);

        CompareTotal(
            preview,
            "Dư đầu kỳ Nợ",
            preview.CalculatedTotals.OpeningDebit,
            preview.WorkbookTotals.OpeningDebit,
            VndTolerance);

        CompareTotal(
            preview,
            "Dư đầu kỳ Có",
            preview.CalculatedTotals.OpeningCredit,
            preview.WorkbookTotals.OpeningCredit,
            VndTolerance);

        CompareTotal(
            preview,
            "Ngoại tệ phát sinh",
            preview.CalculatedTotals.PeriodForeignAmount,
            preview.WorkbookTotals.PeriodForeignAmount,
            ForeignTolerance);

        CompareTotal(
            preview,
            "Phát sinh Nợ",
            preview.CalculatedTotals.PeriodDebit,
            preview.WorkbookTotals.PeriodDebit,
            VndTolerance);

        CompareTotal(
            preview,
            "Phát sinh Có",
            preview.CalculatedTotals.PeriodCredit,
            preview.WorkbookTotals.PeriodCredit,
            VndTolerance);

        CompareTotal(
            preview,
            "Ngoại tệ cuối kỳ",
            preview.CalculatedTotals.ClosingForeignAmount,
            preview.WorkbookTotals.ClosingForeignAmount,
            ForeignTolerance);

        CompareTotal(
            preview,
            "Dư cuối kỳ Nợ",
            preview.CalculatedTotals.ClosingDebit,
            preview.WorkbookTotals.ClosingDebit,
            VndTolerance);

        CompareTotal(
            preview,
            "Dư cuối kỳ Có",
            preview.CalculatedTotals.ClosingCredit,
            preview.WorkbookTotals.ClosingCredit,
            VndTolerance);
    }

    private static void CompareTotal(
        AccountBalanceExcelPreview preview,
        string name,
        decimal calculated,
        decimal workbook,
        decimal tolerance)
    {
        if (Math.Abs(calculated - workbook) <= tolerance)
            return;

        preview.Messages.Add(new ExcelImportMessage
        {
            Level = ExcelImportMessageLevel.Error,
            Message =
                $"Tổng '{name}' từ dòng chi tiết ({calculated:N2}) " +
                $"không khớp dòng Tổng cộng ({workbook:N2})."
        });
    }

    private async Task EnsureTargetSchemaAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        var tableName = ValidateAndQuoteTableName(_options.TableName);
        var configuredColumns = GetConfiguredColumns();

        await using var command = connection.CreateCommand();
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        command.CommandText =
            "SELECT c.name " +
            "FROM sys.columns c " +
            "WHERE c.object_id = OBJECT_ID(@TableName);";
        command.Parameters.AddWithValue("@TableName", _options.TableName);

        var actualColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            actualColumns.Add(reader.GetString(0));

        if (actualColumns.Count == 0)
        {
            throw new InvalidOperationException(
                $"Không tìm thấy bảng đích {_options.TableName}. " +
                "Hãy kiểm tra AccountBalanceImport:TableName trong appsettings.json.");
        }

        var missingColumns = configuredColumns
            .Where(item => !actualColumns.Contains(item.Name))
            .Select(item => item.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingColumns.Count > 0)
        {
            throw new InvalidOperationException(
                $"Bảng {tableName} thiếu các cột đã cấu hình: " +
                string.Join(", ", missingColumns) + ".");
        }
    }

    private static async Task EnsureAuditSchemaAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                CASE
                    WHEN OBJECT_ID(N'dbo.AccountBalanceImportBatches', N'U') IS NOT NULL
                     AND OBJECT_ID(N'dbo.AccountBalanceImportLines', N'U') IS NOT NULL
                     AND COL_LENGTH(N'dbo.AccountBalanceImportBatches', N'BookCode') IS NOT NULL
                     AND COL_LENGTH(N'dbo.AccountBalanceImportBatches', N'AccountType') IS NOT NULL
                    THEN 1
                    ELSE 0
                END;
            """;

        var exists = Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken),
            CultureInfo.InvariantCulture);

        if (exists != 1)
        {
            throw new InvalidOperationException(
                "Bảng audit chưa có hoặc chưa được nâng cấp. Hãy chạy script " +
                "Database/003_Create_AccountBalanceImportAudit.sql.");
        }
    }

    private static async Task EnsureFileNotImportedAsync(
        SqlConnection connection,
        Guid companyId,
        AccountBalanceExcelPreview preview,
        string? bookCode,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) BatchId
            FROM dbo.AccountBalanceImportBatches
            WHERE CompanyId = @CompanyId
              AND FileHash = @FileHash
              AND AccountCode = @AccountCode
              AND FiscalYear = @FiscalYear
              AND PeriodNo = @PeriodNo
              AND
              (
                    BookCode = @BookCode
                 OR (BookCode IS NULL AND @BookCode IS NULL)
              )
              AND Status = 1;
            """;

        command.Parameters.AddWithValue("@CompanyId", companyId);
        command.Parameters.AddWithValue("@FileHash", preview.FileHash);
        command.Parameters.AddWithValue("@AccountCode", preview.AccountCode);
        command.Parameters.AddWithValue("@FiscalYear", preview.FiscalYear);
        command.Parameters.AddWithValue("@PeriodNo", preview.PeriodNo);

        var bookParameter = command.Parameters.Add("@BookCode", SqlDbType.NVarChar, 50);
        bookParameter.Value = (object?)bookCode ?? DBNull.Value;

        var existing = await command.ExecuteScalarAsync(cancellationToken);
        if (existing is not null && existing != DBNull.Value)
        {
            throw new InvalidOperationException(
                "File này đã được import thành công trước đó cho cùng tài khoản, kỳ và sổ. " +
                "Hệ thống chặn import lại để tránh ghi đè ngoài ý muốn.");
        }
    }

    private async Task InsertAuditBatchAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid batchId,
        CommitAccountBalanceImportRequest request,
        string userName,
        string currencyCode,
        string? bookCode,
        string? accountType,
        AccountBalanceImportTotals totals,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        command.CommandText = """
            INSERT INTO dbo.AccountBalanceImportBatches
            (
                BatchId, CompanyId, FileName, FileHash, TargetTable,
                AccountCode, AccountName, FromDate, ToDate,
                FiscalYear, PeriodNo, CurrencyCode, BookCode, AccountType,
                ImportMode, RowCount,
                OpeningDebit, OpeningCredit,
                PeriodDebit, PeriodCredit,
                ClosingDebit, ClosingCredit,
                Status, ImportedAt, ImportedBy
            )
            VALUES
            (
                @BatchId, @CompanyId, @FileName, @FileHash, @TargetTable,
                @AccountCode, @AccountName, @FromDate, @ToDate,
                @FiscalYear, @PeriodNo, @CurrencyCode, @BookCode, @AccountType,
                @ImportMode, @RowCount,
                @OpeningDebit, @OpeningCredit,
                @PeriodDebit, @PeriodCredit,
                @ClosingDebit, @ClosingCredit,
                0, SYSUTCDATETIME(), @ImportedBy
            );
            """;

        command.Parameters.AddWithValue("@BatchId", batchId);
        command.Parameters.AddWithValue("@CompanyId", request.CompanyId);
        command.Parameters.AddWithValue("@FileName", request.Preview.FileName);
        command.Parameters.AddWithValue("@FileHash", request.Preview.FileHash);
        command.Parameters.AddWithValue("@TargetTable", _options.TableName);
        command.Parameters.AddWithValue("@AccountCode", request.Preview.AccountCode);
        command.Parameters.AddWithValue("@AccountName", request.Preview.AccountName);
        command.Parameters.AddWithValue(
            "@FromDate",
            (object?)request.Preview.FromDate ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "@ToDate",
            (object?)request.Preview.ToDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@FiscalYear", request.Preview.FiscalYear);
        command.Parameters.AddWithValue("@PeriodNo", request.Preview.PeriodNo);
        command.Parameters.AddWithValue("@CurrencyCode", currencyCode);

        var bookParameter = command.Parameters.Add("@BookCode", SqlDbType.NVarChar, 50);
        bookParameter.Value = (object?)bookCode ?? DBNull.Value;

        var auditAccountTypeParameter = command.Parameters.Add("@AccountType", SqlDbType.VarChar, 20);
        auditAccountTypeParameter.Value = (object?)accountType ?? DBNull.Value;
        command.Parameters.AddWithValue("@ImportMode", (byte)request.ExistingMode);
        command.Parameters.AddWithValue("@RowCount", request.Preview.Lines.Count);
        AddMoneyParameter(command, "@OpeningDebit", totals.OpeningDebit);
        AddMoneyParameter(command, "@OpeningCredit", totals.OpeningCredit);
        AddMoneyParameter(command, "@PeriodDebit", totals.PeriodDebit);
        AddMoneyParameter(command, "@PeriodCredit", totals.PeriodCredit);
        AddMoneyParameter(command, "@ClosingDebit", totals.ClosingDebit);
        AddMoneyParameter(command, "@ClosingCredit", totals.ClosingCredit);
        command.Parameters.AddWithValue("@ImportedBy", userName);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task BulkCopyAuditLinesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid batchId,
        IReadOnlyCollection<AccountBalanceExcelLine> lines,
        CancellationToken cancellationToken)
    {
        using var table = BuildAuditDataTable(batchId, lines);
        using var bulkCopy = new SqlBulkCopy(
            connection,
            SqlBulkCopyOptions.CheckConstraints,
            transaction)
        {
            DestinationTableName = "dbo.AccountBalanceImportLines",
            BatchSize = 1_000,
            BulkCopyTimeout = _options.CommandTimeoutSeconds
        };

        foreach (DataColumn column in table.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await bulkCopy.WriteToServerAsync(table, cancellationToken);
    }

    private async Task<(int Inserted, int Updated, int Deleted)> UpsertAccountSummaryAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        CommitAccountBalanceImportRequest request,
        string? bookCode,
        string? accountType,
        AccountBalanceImportTotals totals,
        CancellationToken cancellationToken)
    {
        var sql = BuildTargetImportSql(request.ExistingMode);

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandTimeout = _options.CommandTimeoutSeconds;
        command.CommandText = sql;

        command.Parameters.AddWithValue("@CompanyId", request.CompanyId);

        var accountCodeParameter = command.Parameters.Add("@AccountCode", SqlDbType.VarChar, 20);
        accountCodeParameter.Value = request.Preview.AccountCode.Trim();

        command.Parameters.AddWithValue("@PeriodYear", request.Preview.FiscalYear);
        command.Parameters.AddWithValue("@PeriodMonth", request.Preview.PeriodNo);

        var bookParameter = command.Parameters.Add("@BookCode", SqlDbType.NVarChar, 50);
        bookParameter.Value = (object?)bookCode ?? DBNull.Value;

        var accountTypeParameter = command.Parameters.Add("@AccountType", SqlDbType.VarChar, 20);
        accountTypeParameter.Value = (object?)accountType ?? DBNull.Value;

        var fromDateParameter = command.Parameters.Add("@CalculatedFrom", SqlDbType.DateTime2);
        fromDateParameter.Scale = 0;
        fromDateParameter.Value = request.Preview.FromDate!.Value.Date;

        var toDateParameter = command.Parameters.Add("@CalculatedTo", SqlDbType.DateTime2);
        toDateParameter.Scale = 0;
        toDateParameter.Value = request.Preview.ToDate!.Value.Date;

        AddMoneyParameter(
            command,
            "@OpeningBalance",
            totals.OpeningDebit - totals.OpeningCredit);
        AddMoneyParameter(command, "@DebitTotal", totals.PeriodDebit);
        AddMoneyParameter(command, "@CreditTotal", totals.PeriodCredit);
        AddMoneyParameter(
            command,
            "@ClosingBalance",
            totals.ClosingDebit - totals.ClosingCredit);

        AddMoneyParameter(command, "@OpeningDebit", totals.OpeningDebit);
        AddMoneyParameter(command, "@OpeningCredit", totals.OpeningCredit);
        AddMoneyParameter(command, "@ClosingDebit", totals.ClosingDebit);
        AddMoneyParameter(command, "@ClosingCredit", totals.ClosingCredit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException("Không nhận được kết quả từ lệnh import.");

        return (
            reader.GetInt32(reader.GetOrdinal("InsertedCount")),
            reader.GetInt32(reader.GetOrdinal("UpdatedCount")),
            reader.GetInt32(reader.GetOrdinal("DeletedCount")));
    }

    private static async Task CompleteAuditBatchAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        Guid batchId,
        (int Inserted, int Updated, int Deleted) result,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            UPDATE dbo.AccountBalanceImportBatches
            SET Status = 1,
                InsertedCount = @InsertedCount,
                UpdatedCount = @UpdatedCount,
                DeletedCount = @DeletedCount,
                CompletedAt = SYSUTCDATETIME()
            WHERE BatchId = @BatchId;
            """;

        command.Parameters.AddWithValue("@BatchId", batchId);
        command.Parameters.AddWithValue("@InsertedCount", result.Inserted);
        command.Parameters.AddWithValue("@UpdatedCount", result.Updated);
        command.Parameters.AddWithValue("@DeletedCount", result.Deleted);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private string BuildTargetImportSql(ExistingBalanceImportMode importMode)
    {
        var targetTable = ValidateAndQuoteTableName(_options.TableName);
        var keyCondition = BuildAccountPeriodKeyCondition("T");

        var insertColumns = new List<string>();
        var insertValues = new List<string>();
        var updateAssignments = new List<string>();

        AddInsertConstant(insertColumns, insertValues, _options.IdColumn, "NEWID()");
        AddInsertConstant(insertColumns, insertValues, _options.AccountCodeColumn, "@AccountCode");
        AddInsertConstant(insertColumns, insertValues, _options.PeriodYearColumn, "@PeriodYear");
        AddInsertConstant(insertColumns, insertValues, _options.PeriodMonthColumn, "@PeriodMonth");

        AddMappedColumn(
            insertColumns,
            insertValues,
            updateAssignments,
            _options.OpeningBalanceColumn,
            "@OpeningBalance");
        AddMappedColumn(
            insertColumns,
            insertValues,
            updateAssignments,
            _options.DebitTotalColumn,
            "@DebitTotal");
        AddMappedColumn(
            insertColumns,
            insertValues,
            updateAssignments,
            _options.CreditTotalColumn,
            "@CreditTotal");
        AddMappedColumn(
            insertColumns,
            insertValues,
            updateAssignments,
            _options.ClosingBalanceColumn,
            "@ClosingBalance");

        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.AccountTypeColumn, "@AccountType");
        AddInsertConstant(insertColumns, insertValues, _options.CompanyIdColumn, "@CompanyId");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.CalculatedFromColumn, "@CalculatedFrom");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.CalculatedToColumn, "@CalculatedTo");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.LastCalculatedAtColumn, "SYSUTCDATETIME()");
        AddInsertConstant(insertColumns, insertValues, _options.BookCodeColumn, "@BookCode");

        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.OpeningDebitColumn, "@OpeningDebit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.OpeningCreditColumn, "@OpeningCredit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.ClosingDebitColumn, "@ClosingDebit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.ClosingCreditColumn, "@ClosingCredit");

        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.LegacyOpeningDebitColumn, "@OpeningDebit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.LegacyOpeningCreditColumn, "@OpeningCredit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.LegacyClosingDebitColumn, "@ClosingDebit");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.LegacyClosingCreditColumn, "@ClosingCredit");

        AddInsertConstant(insertColumns, insertValues, _options.CreatedAtColumn, "SYSUTCDATETIME()");
        AddMappedColumn(insertColumns, insertValues, updateAssignments, _options.UpdatedAtColumn, "SYSUTCDATETIME()");

        var insertSql = $"""
            INSERT INTO {targetTable}
            (
                {string.Join(",\n                ", insertColumns)}
            )
            VALUES
            (
                {string.Join(",\n                ", insertValues)}
            );
            """;

        if (importMode == ExistingBalanceImportMode.ReplaceCurrentAccountPeriod)
        {
            return $"""
                DECLARE @DeletedCount INT = 0;

                DELETE T
                FROM {targetTable} AS T
                WHERE {keyCondition};

                SET @DeletedCount = @@ROWCOUNT;

                {insertSql}

                SELECT
                    CAST(1 AS INT) AS InsertedCount,
                    CAST(0 AS INT) AS UpdatedCount,
                    @DeletedCount AS DeletedCount;
                """;
        }

        if (importMode == ExistingBalanceImportMode.RejectExisting)
        {
            return $"""
                IF EXISTS
                (
                    SELECT 1
                    FROM {targetTable} AS T WITH (UPDLOCK, HOLDLOCK)
                    WHERE {keyCondition}
                )
                BEGIN
                    THROW 51001,
                        N'Đã tồn tại số dư cùng công ty, tài khoản, năm, tháng và sổ. Hãy chọn Cập nhật hoặc Xóa và thay thế.',
                        1;
                END;

                {insertSql}

                SELECT
                    CAST(1 AS INT) AS InsertedCount,
                    CAST(0 AS INT) AS UpdatedCount,
                    CAST(0 AS INT) AS DeletedCount;
                """;
        }

        if (updateAssignments.Count == 0)
            throw new InvalidOperationException("Không có cột nào được cấu hình để cập nhật.");

        return $"""
            DECLARE @UpdatedCount INT = 0;
            DECLARE @InsertedCount INT = 0;

            UPDATE T WITH (UPDLOCK, SERIALIZABLE)
            SET
                {string.Join(",\n                ", updateAssignments)}
            FROM {targetTable} AS T
            WHERE {keyCondition};

            SET @UpdatedCount = @@ROWCOUNT;

            IF @UpdatedCount = 0
            BEGIN
                {insertSql}
                SET @InsertedCount = 1;
            END;

            SELECT
                @InsertedCount AS InsertedCount,
                @UpdatedCount AS UpdatedCount,
                CAST(0 AS INT) AS DeletedCount;
            """;
    }

    private string BuildAccountPeriodKeyCondition(string targetAlias)
    {
        var conditions = new List<string>
        {
            $"{targetAlias}.{QuoteColumn(_options.AccountCodeColumn)} = @AccountCode",
            $"{targetAlias}.{QuoteColumn(_options.PeriodYearColumn)} = @PeriodYear",
            $"{targetAlias}.{QuoteColumn(_options.PeriodMonthColumn)} = @PeriodMonth"
        };

        if (!string.IsNullOrWhiteSpace(_options.CompanyIdColumn))
        {
            conditions.Insert(
                0,
                $"{targetAlias}.{QuoteColumn(_options.CompanyIdColumn)} = @CompanyId");
        }

        if (!string.IsNullOrWhiteSpace(_options.BookCodeColumn))
        {
            var column = $"{targetAlias}.{QuoteColumn(_options.BookCodeColumn)}";
            conditions.Add(
                $"({column} = @BookCode OR ({column} IS NULL AND @BookCode IS NULL))");
        }

        return string.Join("\n                  AND ", conditions);
    }

    private static void AddMappedColumn(
        ICollection<string> insertColumns,
        ICollection<string> insertValues,
        ICollection<string> updateAssignments,
        string? columnName,
        string sourceExpression)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return;

        var quotedColumn = QuoteColumn(columnName);

        // Tránh ghi một cột hai lần nếu cấu hình legacy trỏ cùng tên cột mới.
        if (!insertColumns.Contains(quotedColumn, StringComparer.OrdinalIgnoreCase))
        {
            insertColumns.Add(quotedColumn);
            insertValues.Add(sourceExpression);
        }

        var assignmentPrefix = $"T.{quotedColumn} =";
        if (!updateAssignments.Any(x =>
                x.StartsWith(assignmentPrefix, StringComparison.OrdinalIgnoreCase)))
        {
            updateAssignments.Add($"T.{quotedColumn} = {sourceExpression}");
        }
    }

    private static void AddInsertConstant(
        ICollection<string> insertColumns,
        ICollection<string> insertValues,
        string? columnName,
        string sourceExpression)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return;

        var quotedColumn = QuoteColumn(columnName);
        if (insertColumns.Contains(quotedColumn, StringComparer.OrdinalIgnoreCase))
            return;

        insertColumns.Add(quotedColumn);
        insertValues.Add(sourceExpression);
    }

    private IReadOnlyList<(string Name, bool Required)> GetConfiguredColumns()
    {
        var columns = new List<(string Name, bool Required)>
        {
            (_options.IdColumn, true),
            (_options.AccountCodeColumn, true),
            (_options.PeriodYearColumn, true),
            (_options.PeriodMonthColumn, true),
            (_options.OpeningBalanceColumn, true),
            (_options.DebitTotalColumn, true),
            (_options.CreditTotalColumn, true),
            (_options.ClosingBalanceColumn, true),
            (_options.CreatedAtColumn, true),
            (_options.UpdatedAtColumn, true)
        };

        AddOptional(columns, _options.AccountTypeColumn);
        AddOptional(columns, _options.CompanyIdColumn);
        AddOptional(columns, _options.CalculatedFromColumn);
        AddOptional(columns, _options.CalculatedToColumn);
        AddOptional(columns, _options.LastCalculatedAtColumn);
        AddOptional(columns, _options.BookCodeColumn);

        AddOptional(columns, _options.OpeningDebitColumn);
        AddOptional(columns, _options.OpeningCreditColumn);
        AddOptional(columns, _options.ClosingDebitColumn);
        AddOptional(columns, _options.ClosingCreditColumn);
        AddOptional(columns, _options.LegacyOpeningDebitColumn);
        AddOptional(columns, _options.LegacyOpeningCreditColumn);
        AddOptional(columns, _options.LegacyClosingDebitColumn);
        AddOptional(columns, _options.LegacyClosingCreditColumn);

        foreach (var column in columns)
            ValidateIdentifier(column.Name, "column");

        return columns
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private static void AddOptional(
        ICollection<(string Name, bool Required)> collection,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            collection.Add((value, false));
    }

    private static void ValidateAggregateTotals(AccountBalanceImportTotals totals)
    {
        var values = new[]
        {
            totals.OpeningDebit,
            totals.OpeningCredit,
            totals.PeriodDebit,
            totals.PeriodCredit,
            totals.ClosingDebit,
            totals.ClosingCredit
        };

        if (values.Any(value => value < 0))
            throw new InvalidOperationException("Tổng số tiền Nợ/Có không được âm.");

        var expectedClosing =
            totals.OpeningDebit - totals.OpeningCredit +
            totals.PeriodDebit - totals.PeriodCredit;

        var actualClosing = totals.ClosingDebit - totals.ClosingCredit;
        if (Math.Abs(expectedClosing - actualClosing) > VndTolerance)
        {
            throw new InvalidOperationException(
                "Tổng báo cáo không cân: dư đầu kỳ + phát sinh Nợ - phát sinh Có " +
                "khác dư cuối kỳ.");
        }
    }

    private static string? ResolveAccountType(
        string? requestedAccountType,
        AccountBalanceImportTotals totals)
    {
        var normalized = requestedAccountType?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        if (normalized is "DEBIT" or "CREDIT")
            return normalized;

        if (normalized != "AUTO")
        {
            throw new InvalidOperationException(
                "AccountType chỉ nhận rỗng, AUTO, DEBIT hoặc CREDIT.");
        }

        var closingNet = totals.ClosingDebit - totals.ClosingCredit;
        if (closingNet > 0)
            return "DEBIT";

        if (closingNet < 0)
            return "CREDIT";

        var openingNet = totals.OpeningDebit - totals.OpeningCredit;
        return openingNet < 0 ? "CREDIT" : "DEBIT";
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void AddMoneyParameter(
        SqlCommand command,
        string parameterName,
        decimal value)
    {
        var parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
        parameter.Precision = 18;
        parameter.Scale = 2;
        parameter.Value = decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static DataTable BuildAuditDataTable(
        Guid batchId,
        IEnumerable<AccountBalanceExcelLine> lines)
    {
        var table = new DataTable();
        table.Columns.Add("BatchId", typeof(Guid));
        table.Columns.Add("ExcelRowNo", typeof(int));
        table.Columns.Add("CustomerCode", typeof(string));
        table.Columns.Add("CustomerName", typeof(string));
        table.Columns.Add("OpeningForeignAmount", typeof(decimal));
        table.Columns.Add("OpeningDebit", typeof(decimal));
        table.Columns.Add("OpeningCredit", typeof(decimal));
        table.Columns.Add("PeriodForeignAmount", typeof(decimal));
        table.Columns.Add("PeriodDebit", typeof(decimal));
        table.Columns.Add("PeriodCredit", typeof(decimal));
        table.Columns.Add("ClosingForeignAmount", typeof(decimal));
        table.Columns.Add("ClosingDebit", typeof(decimal));
        table.Columns.Add("ClosingCredit", typeof(decimal));

        foreach (var line in lines)
        {
            table.Rows.Add(
                batchId,
                line.ExcelRowNo,
                line.CustomerCode,
                line.CustomerName,
                line.OpeningForeignAmount,
                line.OpeningDebit,
                line.OpeningCredit,
                line.PeriodForeignAmount,
                line.PeriodDebit,
                line.PeriodCredit,
                line.ClosingForeignAmount,
                line.ClosingDebit,
                line.ClosingCredit);
        }

        return table;
    }

    private static string ValidateAndQuoteTableName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Chưa cấu hình tên bảng account balance.");

        var parts = value.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 2)
            throw new InvalidOperationException("Tên bảng chỉ được có dạng Table hoặc Schema.Table.");

        foreach (var part in parts)
            ValidateIdentifier(part, "table");

        return string.Join('.', parts.Select(QuoteColumn));
    }

    private static string QuoteColumn(string value)
    {
        ValidateIdentifier(value, "column");
        return $"[{value}]";
    }

    private static void ValidateIdentifier(string value, string type)
    {
        if (!Regex.IsMatch(value, @"^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            throw new InvalidOperationException(
                $"Tên {type} '{value}' không hợp lệ trong cấu hình import.");
        }
    }

    private static string ReadText(IXLCell cell)
    {
        if (cell.IsEmpty())
            return string.Empty;

        return cell.GetFormattedString().Trim();
    }

    private static decimal ReadDecimal(IXLCell cell)
    {
        if (cell.IsEmpty())
            return 0m;

        if (cell.TryGetValue<decimal>(out var decimalValue))
            return decimalValue;

        var text = cell.GetFormattedString().Trim();
        if (string.IsNullOrWhiteSpace(text))
            return 0m;

        var cultures = new[]
        {
            CultureInfo.InvariantCulture,
            CultureInfo.GetCultureInfo("vi-VN"),
            CultureInfo.GetCultureInfo("en-US")
        };

        foreach (var culture in cultures)
        {
            if (decimal.TryParse(
                    text,
                    NumberStyles.Number |
                    NumberStyles.AllowCurrencySymbol |
                    NumberStyles.AllowLeadingSign,
                    culture,
                    out decimalValue))
            {
                return decimalValue;
            }
        }

        throw new InvalidOperationException(
            $"Ô {cell.Address} có giá trị số không hợp lệ: '{text}'.");
    }

    private static int? ReadNullableInt(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        if (cell.TryGetValue<int>(out var value))
            return value;

        var text = cell.GetFormattedString().Trim();
        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
            ? value
            : null;
    }

    private static string NormalizeForSearch(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .Normalize(NormalizationForm.FormD)
            .Where(character =>
                CharUnicodeInfo.GetUnicodeCategory(character) !=
                UnicodeCategory.NonSpacingMark)
            .ToArray();

        return Regex.Replace(
                new string(normalized).Normalize(NormalizationForm.FormC),
                @"\s+",
                " ")
            .Trim()
            .ToLowerInvariant()
            .Replace('đ', 'd');
    }
}
