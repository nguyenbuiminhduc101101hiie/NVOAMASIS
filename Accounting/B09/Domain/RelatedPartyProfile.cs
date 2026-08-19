using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

// Kept separate from Client so the existing Client entity does not need to be changed immediately.
public class RelatedPartyProfile
{
    public int Id { get; set; }
    public Guid CompanyId { get; set; }
    public int? ClientId { get; set; }

    [MaxLength(100)] public string RelatedPartyType { get; set; } = string.Empty;
    [MaxLength(1000)] public string RelationshipDescription { get; set; } = string.Empty;

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
