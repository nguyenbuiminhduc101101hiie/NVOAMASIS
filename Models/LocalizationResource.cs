using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models;

public class LocalizationResource
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string ResourceKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Culture { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Value { get; set; } = string.Empty;
}
