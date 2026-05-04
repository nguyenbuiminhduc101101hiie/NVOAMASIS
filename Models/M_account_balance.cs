using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Models
{
    [Table("account_balance")]
    [Index(nameof(AccountCode), nameof(PeriodYear), nameof(PeriodMonth), IsUnique = true, Name = "UQ_account_balance_account_period")]
    public class M_account_balance
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("account_code", TypeName = "varchar(10)")]
        [MaxLength(10)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        [Column("period_year")]
        public int PeriodYear { get; set; }

        [Required]
        [Column("period_month")]
        public int PeriodMonth { get; set; }

        [Required]
        [Column("opening_balance", TypeName = "decimal(18,2)")]
        public decimal OpeningBalance { get; set; } = 0m;

        [Required]
        [Column("debit_total", TypeName = "decimal(18,2)")]
        public decimal DebitTotal { get; set; } = 0m;

        [Required]
        [Column("credit_total", TypeName = "decimal(18,2)")]
        public decimal CreditTotal { get; set; } = 0m;

        [Required]
        [Column("closing_balance", TypeName = "decimal(18,2)")]
        public decimal ClosingBalance { get; set; } = 0m;

        [Column("account_type", TypeName = "varchar(20)")]
        [MaxLength(20)]
        public string? AccountType { get; set; }

        [Column("company_id")]
        public Guid? CompanyId { get; set; }

        [Column("calculated_from")]
        public DateTime? CalculatedFrom { get; set; }

        [Column("calculated_to")]
        public DateTime? CalculatedTo { get; set; }

        [Column("last_calculated_at")]
        public DateTime? LastCalculatedAt { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [InverseProperty(nameof(M_account_balance_gl_line.AccountBalance))]
        public ICollection<M_account_balance_gl_line> GlSnapshotLines { get; set; } = new List<M_account_balance_gl_line>();
    }
}

