using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("FinancialReportLines")]
    public class M_FinancialReportLine
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        [MaxLength(30)]
        public string ReportType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LineCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string LineName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Section { get; set; }

        [MaxLength(100)]
        public string? ReportGroup { get; set; }

        public Guid? ParentLineId { get; set; }

        [ForeignKey(nameof(ParentLineId))]
        public M_FinancialReportLine? ParentLine { get; set; }

        [InverseProperty(nameof(ParentLine))]
        public ICollection<M_FinancialReportLine> ChildLines { get; set; } = new List<M_FinancialReportLine>();

        [InverseProperty(nameof(M_FinancialReportAccountMapping.ReportLine))]
        public ICollection<M_FinancialReportAccountMapping> AccountMappings { get; set; } = new List<M_FinancialReportAccountMapping>();

        public int LineLevel { get; set; } = 1;

        public bool IsTotalLine { get; set; }

        public bool IsBold { get; set; }

        public int DisplayOrder { get; set; }

        public bool Active { get; set; } = true;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
