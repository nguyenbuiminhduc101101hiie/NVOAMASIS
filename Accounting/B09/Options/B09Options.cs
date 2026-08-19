namespace NVOAMASIS.Accounting.B09.Options;

public class B09Options
{
    public const string SectionName = "B09";

    public string GeneralLedgerTable { get; set; } = "GeneralLedgerEntries";
    public string CompanyIdColumn { get; set; } = "CompanyId";
    public string AccountCodeColumn { get; set; } = "AccountCode";
    public string PostingDateColumn { get; set; } = "PostingDate";
    public string DebitColumn { get; set; } = "Debit";
    public string CreditColumn { get; set; } = "Credit";

    // Optional schema, usually dbo.
    public string Schema { get; set; } = "dbo";
}
