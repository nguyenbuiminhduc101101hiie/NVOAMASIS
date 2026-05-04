using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("account_balance_gl_line")]
    public class M_account_balance_gl_line
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("account_balance_id")]
        public Guid AccountBalanceId { get; set; }

        [ForeignKey(nameof(AccountBalanceId))]
        public M_account_balance? AccountBalance { get; set; }

        /// <summary>Id bút toán GL nguồn (snapshot; không bắt buộc FK để tránh phụ thuộc xóa GL).</summary>
        [Required]
        [Column("general_ledger_entry_id")]
        public Guid GeneralLedgerEntryId { get; set; }

        [Required]
        [Column("voucher_no", TypeName = "nvarchar(100)")]
        [MaxLength(100)]
        public string VoucherNo { get; set; } = string.Empty;

        [Required]
        [Column("posting_date")]
        public DateTime PostingDate { get; set; }

        [Required]
        [Column("debit", TypeName = "decimal(18,2)")]
        public decimal Debit { get; set; }

        [Required]
        [Column("credit", TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [Column("description", TypeName = "nvarchar(500)")]
        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
