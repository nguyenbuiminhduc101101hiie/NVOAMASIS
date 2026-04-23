using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_ShipmentChargeDateMapping
{
    [Key]
    public Guid Id { get; set; }
    public string TargetField { get; set; } = string.Empty;
    public string SourceModel { get; set; } = string.Empty;
    public string SourceProperty { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
