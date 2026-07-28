namespace NVOAMASIS.Models.Accounting;

public sealed class FixedAssetDepreciationPostResult
{
    public Guid VoucherId { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public int PostedAssetCount { get; set; }
    public int LedgerLineCount { get; set; }
    public decimal TotalAmount { get; set; }
}
