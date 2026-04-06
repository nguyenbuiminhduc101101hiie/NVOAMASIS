using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Sale
    {
        [Key]
        public Guid Sale_ID { get; set; }
        public Guid? UsrID { get; set; }
        public string? SaleCode { get; set; }
        public string? Nhomsale { get; set; }
        public string? Usr { get; set; }
        public string? SaleName { get; set; }
        public string? Username { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? UserID { get; set; }
        public bool? Approve { get; set; }
        public DateTime? UpdateTime { get; set; }
        public double? Target { get; set; }
        public int? Tile { get; set; }
    }
}
