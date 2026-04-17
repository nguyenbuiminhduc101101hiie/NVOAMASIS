using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("accounting_period")]
    public class M_AccountingPeriod
    {
        [Key]
        [Column("period_id")]
        public Guid PeriodId { get; set; }

        [Column("fiscal_year")]
        public int FiscalYear { get; set; }

        [Column("period_month")]
        public int PeriodMonth { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("period_code")]
        public int PeriodCode { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "OPEN";

        [Column("is_closed")]
        public bool IsClosed { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
