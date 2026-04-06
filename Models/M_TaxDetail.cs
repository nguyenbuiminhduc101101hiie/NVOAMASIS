using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TaxDetail
    {
        [Key]
        public Guid taxDetailID { get; set; }
        public Guid? taxInvoiceID { get; set; }
        public int? STT { get; set; }
        public string? Items { get; set; }
        public Guid? itemid { get; set; }
        public string? Clucidat { get; set; }
        public double? soLuong { get; set; }
        public double? dongiatruocthueVND { get; set; }
        public double? thanhtientruocthueVND { get; set; }
        public string? Container_Type { get; set; }
        public string? Currency { get; set; }
        public string? Note { get; set; }
        public string? ExchangeRate { get; set; }
        public bool? Editable { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? UserID { get; set; }
    }
}
