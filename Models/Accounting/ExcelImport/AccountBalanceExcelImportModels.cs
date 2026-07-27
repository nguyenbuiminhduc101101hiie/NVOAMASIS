namespace NVOAMASIS.Models.Accounting.ExcelImport;

public enum ExcelAccountingImportTarget : byte
{
    AccountBalance = 1,
    GeneralLedger = 2
}

public enum ExistingBalanceImportMode : byte
{
    RejectExisting = 0,
    UpdateExisting = 1,
    ReplaceCurrentAccountPeriod = 2
}

public enum ExcelImportMessageLevel : byte
{
    Information = 0,
    Warning = 1,
    Error = 2
}

public sealed class ExcelImportMessage
{
    public ExcelImportMessageLevel Level { get; init; }
    public int? ExcelRowNo { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class AccountBalanceExcelLine
{
    public int ExcelRowNo { get; init; }
    public int? SequenceNo { get; init; }

    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public decimal OpeningForeignAmount { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }

    public decimal PeriodForeignAmount { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }

    public decimal ClosingForeignAmount { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }

    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];

    public bool HasError => Errors.Count > 0;
    public bool HasWarning => Warnings.Count > 0;
    public string ErrorText => string.Join("; ", Errors);
    public string WarningText => string.Join("; ", Warnings);
}

public sealed class AccountBalanceImportTotals
{
    public decimal OpeningForeignAmount { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }

    public decimal PeriodForeignAmount { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }

    public decimal ClosingForeignAmount { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public sealed class AccountBalanceExcelPreview
{
    public string FileName { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public ExcelAccountingImportTarget Target { get; init; }

    public string CompanyNameInFile { get; set; } = string.Empty;
    public string TaxCodeInFile { get; set; } = string.Empty;
    public string ReportTitle { get; set; } = string.Empty;
    public string UnitText { get; set; } = string.Empty;

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int FiscalYear { get; set; }
    public int PeriodNo { get; set; }

    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "VND";

    public int HeaderRowNo { get; set; }
    public int? TotalRowNo { get; set; }

    public AccountBalanceImportTotals CalculatedTotals { get; set; } = new();
    public AccountBalanceImportTotals? WorkbookTotals { get; set; }

    public List<AccountBalanceExcelLine> Lines { get; } = [];
    public List<ExcelImportMessage> Messages { get; } = [];

    public int ErrorCount =>
        Messages.Count(x => x.Level == ExcelImportMessageLevel.Error) +
        Lines.Sum(x => x.Errors.Count);

    public int WarningCount =>
        Messages.Count(x => x.Level == ExcelImportMessageLevel.Warning) +
        Lines.Sum(x => x.Warnings.Count);

    public bool CanImport =>
        Target == ExcelAccountingImportTarget.AccountBalance &&
        Lines.Count > 0 &&
        ErrorCount == 0 &&
        !string.IsNullOrWhiteSpace(AccountCode) &&
        FiscalYear > 0;
}

public sealed class CommitAccountBalanceImportRequest
{
    public Guid CompanyId { get; init; }
    public string UserName { get; init; } = "system";
    public string CurrencyCode { get; init; } = "VND";

    // Để trống nếu hệ thống không phân sổ. Giá trị rỗng sẽ được lưu NULL.
    public string? BookCode { get; init; }

    // Để trống để ghi NULL; hoặc dùng AUTO, DEBIT, CREDIT theo quy ước dự án.
    public string? AccountType { get; init; }

    public ExistingBalanceImportMode ExistingMode { get; init; }
    public AccountBalanceExcelPreview Preview { get; init; } = new();
}

public sealed class AccountBalanceImportResult
{
    public Guid BatchId { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int DeletedCount { get; init; }
    public int ImportedRowCount { get; init; }
}
