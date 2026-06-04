using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models;

[Table("TenantDatabaseRegistry")]
public class TenantDatabaseRegistry
{
    [Key]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(128)]
    public string DatabaseName { get; set; } = "";

    [Required]
    [MaxLength(256)]
    public string ServerName { get; set; } = "logisticssoftware.vn";

    [Required]
    [MaxLength(128)]
    public string SqlUserId { get; set; } = "";

    [Required]
    [MaxLength(512)]
    public string SqlPassword { get; set; } = "";

    [MaxLength(256)]
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [MaxLength(128)]
    public string? CreatedByAppUser { get; set; }

    public bool IsActive { get; set; } = true;
}
