using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_FinancialReportTypes
    {
        [Key]
        public Guid id { get; set; }
        public string? ReportCode { get; set; }
        public string? ReportName { get; set; }
        public string? FormNo { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public int? SortOrder { get; set; }

    }
}
