using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteSection
{
    public int Id { get; set; }
    public int TemplateId { get; set; }

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public FinancialStatementNoteTemplate Template { get; set; } = null!;
    public ICollection<FinancialStatementNoteLine> Lines { get; set; } = new List<FinancialStatementNoteLine>();
}
