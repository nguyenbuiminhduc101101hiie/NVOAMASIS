using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Currency
    {
        [Key]
        public Guid CurrencyID { get; set; }
        public string? Currency { get; set; }
        public string? Details { get; set; }
        public double? Exchange { get; set; }
        public string? Remarks { get; set; }
        public string? ngay { get; set; }
        public string? code { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }

    }
}
