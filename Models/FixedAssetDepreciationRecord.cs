using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models.Accounting;

public enum FixedAssetDepreciationStatus : byte
{
    Draft = 0,
    Posted = 1
}

public sealed class FixedAssetDepreciationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public Guid FixedAssetId { get; set; }

    public int FiscalYear { get; set; }
    public int FiscalPeriod { get; set; }
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }

    public DepreciationMethod DepreciationMethod { get; set; }
    public bool ProrateByDay { get; set; } = true;
    public int DaysInPeriod { get; set; }
    public int DepreciableDays { get; set; }

    public decimal OriginalCost { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal MonthlyDepreciation { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingRemainingValue { get; set; }

    [Required, MaxLength(20)]
    public string ExpenseAccount { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string DepreciationAccount { get; set; } = string.Empty;

    public FixedAssetDepreciationStatus Status { get; set; }
        = FixedAssetDepreciationStatus.Draft;

    public Guid? VoucherId { get; set; }

    [MaxLength(50)]
    public string? VoucherNo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    [MaxLength(100)]
    public string? PostedBy { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class FixedAssetDepreciationRow
{
    public Guid? DepreciationId { get; set; }
    public Guid FixedAssetId { get; set; }

    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }

    public DateTime DepreciationStartDate { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; }
    public int UsefulLifeMonths { get; set; }

    public decimal OriginalCost { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal MonthlyDepreciation { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal DepreciationAmount { get; set; }
    public decimal ClosingAccumulatedDepreciation { get; set; }
    public decimal ClosingRemainingValue { get; set; }

    public int DaysInPeriod { get; set; }
    public int DepreciableDays { get; set; }

    public string ExpenseAccount { get; set; } = string.Empty;
    public string DepreciationAccount { get; set; } = string.Empty;

    public FixedAssetDepreciationStatus? SavedStatus { get; set; }
    public bool IsSavedDraft { get; set; }
    public bool IsPosted { get; set; }
    public bool CanSave { get; set; }
    public bool Selected { get; set; }
    public string? Message { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
