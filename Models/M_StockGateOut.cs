using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_StockGateOut
    {
        [Key]
        public Guid StockGateOutID { get; set; }

        public string? Container { get; set; }
        public string? Booking { get; set; }
        public DateTime? DateOut { get; set; }
        public int? TotalDays { get; set; }

        public DateTime? DateImport { get; set; }
        public string? UserImport { get; set; }
    }
}