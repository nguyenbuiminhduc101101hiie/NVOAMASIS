using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class MARKET
    {
        [Key]
        public Guid Market_ID { get; set; }
        public string? MarketCode { get; set; }
        public string? Market { get; set; }
        public string? RemarksBooking { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
