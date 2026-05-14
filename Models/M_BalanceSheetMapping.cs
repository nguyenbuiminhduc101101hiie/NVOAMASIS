using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    /// <summary>Ánh xạ chỉ tiêu bảng cân đối kế toán ↔ mã tài khoản (bảng BalanceSheetMapping).</summary>
    public class M_BalanceSheetMapping
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Section { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        public int Sign { get; set; } = 1;

        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
    }
}
