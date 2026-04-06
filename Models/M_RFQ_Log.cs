using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_RFQ_Log
    {
        [Key]
        public Guid RFQ_logID { get; set; }
        public Guid RFQID { get; set; }
        public string? StatusBefore { get; set; }
        public string? StatusAfter { get; set; }
        public string? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? Notes { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
    }

}

