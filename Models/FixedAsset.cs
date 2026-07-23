using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models.Accounting;

public enum FixedAssetType : byte
{
    Tangible = 1,
    FinanceLease = 2,
    Intangible = 3
}

public enum FixedAssetStatus : byte
{
    Draft = 0,
    InUse = 1,
    Suspended = 2,
    Disposed = 3
}

public enum DepreciationMethod : byte
{
    StraightLine = 1,
    DecliningBalance = 2,
    UnitsOfProduction = 3,
    NotDepreciated = 9
}

public sealed class FixedAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }

    [Required, MaxLength(50)]
    public string AssetCode { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string AssetName { get; set; } = string.Empty;

    public FixedAssetType AssetType { get; set; } = FixedAssetType.Tangible;

    [MaxLength(50)]
    public string? AssetGroupCode { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? SerialNo { get; set; }

    [MaxLength(100)]
    public string? ModelNo { get; set; }

    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    [MaxLength(100)]
    public string? CountryOfOrigin { get; set; }

    [MaxLength(50)]
    public string? DepartmentCode { get; set; }

    [MaxLength(250)]
    public string? Location { get; set; }

    [MaxLength(200)]
    public string? Custodian { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime RecognitionDate { get; set; } = DateTime.Today;
    public DateTime DepreciationStartDate { get; set; } = DateTime.Today;

    public decimal PurchasePrice { get; set; }
    public decimal NonRefundableTax { get; set; }
    public decimal TransportCost { get; set; }
    public decimal InstallationCost { get; set; }
    public decimal OtherDirectCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal RecoverableVat { get; set; }

    public decimal OriginalCost { get; set; }
    public decimal ResidualValue { get; set; }
    public int UsefulLifeMonths { get; set; } = 36;
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal RemainingValue { get; set; }

    [Required, MaxLength(20)]
    public string AssetAccount { get; set; } = "211";

    [Required, MaxLength(20)]
    public string DepreciationAccount { get; set; } = "2141";

    [Required, MaxLength(20)]
    public string ExpenseAccount { get; set; } = "642";

    [MaxLength(20)]
    public string? SourceAccount { get; set; } = "331";

    [MaxLength(50)]
    public string? SupplierCode { get; set; }

    [MaxLength(50)]
    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    [MaxLength(50)]
    public string? VoucherNo { get; set; }

    public DateTime? VoucherDate { get; set; }

    public bool IsOpeningBalance { get; set; }
    public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Draft;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
