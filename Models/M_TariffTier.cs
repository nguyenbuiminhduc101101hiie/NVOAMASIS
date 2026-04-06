using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TariffTier
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TariffHeaderId { get; set; }
        public int FromDay { get; set; }
        public int? ToDay { get; set; }
        public decimal Rate { get; set; }
        public string RateBasis { get; set; }
        public string? Unit { get; set; }
        public decimal? MinCharge { get; set; }
        public int SequenceNo { get; set; }
        public string? Notes { get; set; }
    }
}
