using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteLine
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public int? ParentLineId { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Name { get; set; } = string.Empty;

    public B09ValueType ValueType { get; set; } = B09ValueType.Text;
    public B09SourceType SourceType { get; set; } = B09SourceType.Manual;
    public int DisplayOrder { get; set; }
    public bool IsRequiredNarrative { get; set; }
    public bool IsVisible { get; set; } = true;

    public FinancialStatementNoteSection Section { get; set; } = null!;
    public FinancialStatementNoteLine? ParentLine { get; set; }
    public ICollection<FinancialStatementNoteLine> ChildLines { get; set; } = new List<FinancialStatementNoteLine>();
    public ICollection<FinancialStatementNoteMapping> Mappings { get; set; } = new List<FinancialStatementNoteMapping>();
}
