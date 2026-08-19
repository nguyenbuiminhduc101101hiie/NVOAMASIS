using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteValue
{
    public long Id { get; set; }
    public Guid ReportId { get; set; }
    public int NoteLineId { get; set; }

    public decimal? CurrentSystemValue { get; set; }
    public decimal CurrentAdjustment { get; set; }
    public decimal? PreviousSystemValue { get; set; }
    public decimal PreviousAdjustment { get; set; }

    public string? Narrative { get; set; }
    public string? AdjustmentReason { get; set; }

    [MaxLength(200)] public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    [NotMapped]
    public decimal? CurrentReportedValue => CurrentSystemValue.HasValue
        ? CurrentSystemValue.Value + CurrentAdjustment
        : (CurrentAdjustment == 0m ? null : CurrentAdjustment);

    [NotMapped]
    public decimal? PreviousReportedValue => PreviousSystemValue.HasValue
        ? PreviousSystemValue.Value + PreviousAdjustment
        : (PreviousAdjustment == 0m ? null : PreviousAdjustment);

    public FinancialStatementNoteReport Report { get; set; } = null!;
    public FinancialStatementNoteLine NoteLine { get; set; } = null!;
}
