using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("FinancialReportAccountMappings")]
    public class M_FinancialReportAccountMapping
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountCode { get; set; } = string.Empty;

        [Required]
        public Guid ReportLineId { get; set; }

        [ForeignKey(nameof(ReportLineId))]
        public M_FinancialReportLine? ReportLine { get; set; }

        [MaxLength(20)]
        public string? NormalBalance { get; set; }

        public int Sign { get; set; } = 1;

        public bool ContraAccount { get; set; }

        public int DisplayOrder { get; set; }

        public bool Active { get; set; } = true;

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
