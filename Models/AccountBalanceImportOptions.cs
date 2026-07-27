namespace NVOAMASIS.Options;

public sealed class AccountBalanceImportOptions
{
    public const string SectionName = "AccountBalanceImport";

    public string TableName { get; set; } = "dbo.account_balance";

    public string IdColumn { get; set; } = "id";
    public string AccountCodeColumn { get; set; } = "account_code";
    public string PeriodYearColumn { get; set; } = "period_year";
    public string PeriodMonthColumn { get; set; } = "period_month";

    public string OpeningBalanceColumn { get; set; } = "opening_balance";
    public string DebitTotalColumn { get; set; } = "debit_total";
    public string CreditTotalColumn { get; set; } = "credit_total";
    public string ClosingBalanceColumn { get; set; } = "closing_balance";

    public string? AccountTypeColumn { get; set; } = "account_type";
    public string? CompanyIdColumn { get; set; } = "company_id";
    public string? CalculatedFromColumn { get; set; } = "calculated_from";
    public string? CalculatedToColumn { get; set; } = "calculated_to";
    public string? LastCalculatedAtColumn { get; set; } = "last_calculated_at";
    public string? BookCodeColumn { get; set; } = "book_code";

    public string CreatedAtColumn { get; set; } = "created_at";
    public string UpdatedAtColumn { get; set; } = "updated_at";

    // Hệ thống hiện có đồng thời hai nhóm cột số dư Nợ/Có.
    // Module sẽ ghi cả hai nhóm để giữ tương thích với các báo cáo cũ và mới.
    public string? OpeningDebitColumn { get; set; } = "opening_debit";
    public string? OpeningCreditColumn { get; set; } = "opening_credit";
    public string? ClosingDebitColumn { get; set; } = "closing_debit";
    public string? ClosingCreditColumn { get; set; } = "closing_credit";

    public string? LegacyOpeningDebitColumn { get; set; } = "openingdebit";
    public string? LegacyOpeningCreditColumn { get; set; } = "openingcredit";
    public string? LegacyClosingDebitColumn { get; set; } = "closingdebit";
    public string? LegacyClosingCreditColumn { get; set; } = "closingcredit";

    public int CommandTimeoutSeconds { get; set; } = 180;
    public int MaximumRows { get; set; } = 20_000;
    public long MaximumFileSizeBytes { get; set; } = 20 * 1024 * 1024;
}
