using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models;

public class UserThemePreference
{
    [Key]
    public Guid PreferenceId { get; set; }

    [Required]
    public Guid UsrId { get; set; }

    [StringLength(50)]
    public string NavMenuBackgroundColor { get; set; } = ThemeDefaults.NavMenuBackgroundColor;

    [StringLength(50)]
    public string NavMenuTextColor { get; set; } = ThemeDefaults.NavMenuTextColor;

    [StringLength(50)]
    public string MainLayoutBackgroundColor { get; set; } = ThemeDefaults.MainLayoutBackgroundColor;

    [StringLength(20)]
    public string FontHeaderH6 { get; set; } = ThemeDefaults.FontHeaderH6;

    [StringLength(20)]
    public string FontTieuDe { get; set; } = ThemeDefaults.FontTieuDe;

    [StringLength(20)]
    public string FontNoiDung { get; set; } = ThemeDefaults.FontNoiDung;

    [StringLength(20)]
    public string FontNoiDungLuoi { get; set; } = ThemeDefaults.FontNoiDungLuoi;

    [StringLength(20)]
    public string FontNavMenu { get; set; } = ThemeDefaults.FontNavMenu;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("UsrId")]
    public virtual AuthUser? User { get; set; }
}
