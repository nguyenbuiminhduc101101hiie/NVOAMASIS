using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_DebitCreditCustomer
    {
        [Key]
        public Guid debitcreditcustomerid { get; set; }
        public Guid? customerid { get; set; }
        public string? kyhieuhoadon { get; set; }
        public string? sohoadon { get; set; }
        public string? ngayphathanhhoadon { get; set; }
        public string? mathang { get; set; }
        public string? debitcredit { get; set; }
        public double? giatruocthue { get; set; }
        public double? thuesuat { get; set; }
        public double? thuegtgt { get; set; }
        public string? bank { get; set; }
        public string? department { get; set; }
        public string? currency { get; set; }
        public double? price { get; set; }
        public double? tigia { get; set; }
        public string? ngay { get; set; }
        public string? pttt { get; set; }
        public string? tennganhang { get; set; }
        public string? mota { get; set; }
        public bool? approve { get; set; }
        public bool? continued { get; set; }
        public bool? editable { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? phanbo { get; set; }
    }
}
