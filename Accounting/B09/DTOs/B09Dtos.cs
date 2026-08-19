using NVOAMASIS.Accounting.B09.Domain;

namespace NVOAMASIS.Accounting.B09.DTOs;

public sealed class B09LineDto
{
    public int NoteLineId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public B09ValueType ValueType { get; set; }
    public B09SourceType SourceType { get; set; }
    public bool IsRequiredNarrative { get; set; }
    public decimal? CurrentSystemValue { get; set; }
    public decimal CurrentAdjustment { get; set; }
    public decimal? PreviousSystemValue { get; set; }
    public decimal PreviousAdjustment { get; set; }
    public decimal? CurrentReportedValue { get; set; }
    public decimal? PreviousReportedValue { get; set; }
    public string? Narrative { get; set; }
    public string? AdjustmentReason { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class B09ReportDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public int FiscalYear { get; set; }
    public string TemplateCode { get; set; } = "B09-DN";
    public string TemplateVersion { get; set; } = string.Empty;
    public B09ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockedBy { get; set; }
    public IReadOnlyList<B09LineDto> Lines { get; set; } = Array.Empty<B09LineDto>();
}

public sealed class B09ReportListItemDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public int FiscalYear { get; set; }
    public B09ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? LockedBy { get; set; }
}

public sealed class B09ValidationItemDto
{
    public B09ValidationSeverity Severity { get; set; }
    public string? LineCode { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class B09ValidationResultDto
{
    public bool IsValid => Items.All(x => x.Severity != B09ValidationSeverity.Error);
    public List<B09ValidationItemDto> Items { get; set; } = new();
}

public sealed class B09MappingDto
{
    public int MappingId { get; set; }
    public int NoteLineId { get; set; }
    public string LineCode { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public B09MappingMode Mode { get; set; }
    public string? AccountPrefixesCsv { get; set; }
    public decimal SignMultiplier { get; set; } = 1m;
    public decimal? DetailThresholdPercent { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool HasCustomSql { get; set; }
}

public sealed class B09ConcentrationDto
{
    public string Kind { get; set; } = "AR";
    public Guid? PartyId { get; set; }
    public string PartyLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PercentOfTotal { get; set; }
    public bool MeetsThreshold { get; set; }
}

public sealed class B09FixedAssetDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public decimal OriginalCost { get; set; }
    public decimal OpeningAccumulatedDepreciation { get; set; }
    public decimal RemainingValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RecognitionDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
}
