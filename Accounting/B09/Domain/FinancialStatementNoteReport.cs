using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CompanyId { get; set; }
    public int TemplateId { get; set; }
    public int FiscalYear { get; set; }
    public B09ReportStatus Status { get; set; } = B09ReportStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(200)] public string? CreatedBy { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    [MaxLength(200)] public string? LockedBy { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public FinancialStatementNoteTemplate Template { get; set; } = null!;
    public ICollection<FinancialStatementNoteValue> Values { get; set; } = new List<FinancialStatementNoteValue>();
}
