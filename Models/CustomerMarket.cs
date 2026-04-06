using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class CustomerMarket
    {
        [Key]
        public Guid CustomerMarket_Id { get; set; }
        public Guid Customer_ID { get; set; }
        public Guid Market_ID { get; set; }
        public bool? Editable { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? exim { get; set; }
        public string? country_market { get; set; }
        public string? port { get; set; }
    }
}
