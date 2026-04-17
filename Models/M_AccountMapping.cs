using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("account_mapping")]
    public class M_AccountMapping
    {
        [Key]
        [Column("account_code")]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [Column("account_name")]
        [MaxLength(200)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        [Column("statement_type")]
        [MaxLength(20)]
        public string StatementType { get; set; } = string.Empty;

        [Column("bs_section")]
        [MaxLength(50)]
        public string? BsSection { get; set; }

        [Column("bs_group")]
        [MaxLength(100)]
        public string? BsGroup { get; set; }

        [Column("bs_line_item")]
        [MaxLength(100)]
        public string? BsLineItem { get; set; }

        [Column("normal_balance")]
        [MaxLength(10)]
        public string? NormalBalance { get; set; }

        [Column("is_contra_account")]
        public bool IsContraAccount { get; set; } = false;

        [Column("display_order")]
        public int? DisplayOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
