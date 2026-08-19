using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteTemplate
{
    public int Id { get; set; }

    [MaxLength(30)]
    public string Code { get; set; } = "B09-DN";

    [MaxLength(100)]
    public string Name { get; set; } = "Bản thuyết minh Báo cáo tài chính";

    [MaxLength(30)]
    public string AccountingRegime { get; set; } = "TT99/2025";

    [MaxLength(20)]
    public string Version { get; set; } = "2025.1";

    public DateTime EffectiveFrom { get; set; } = new(2026, 1, 1);
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FinancialStatementNoteSection> Sections { get; set; } = new List<FinancialStatementNoteSection>();
}
