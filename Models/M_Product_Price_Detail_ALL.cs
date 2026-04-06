using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Product_Price_detail_ALL
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? RFQ_Id { get; set; }
        public Guid? Itemid { get; set; }
        public double? Dongia { get; set; }
        public double? Vat { get; set; }
        public double? Thanhtien { get; set; }
        public string? Dieukhoanthanhtoan { get; set; }
        public DateTime? Ngaybaogia { get; set; }
        public string? Trangthai { get; set; }

        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? Createat { get; set; }
        public string? Usercreate { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;
        public string? Cur {  get; set; }
        public string? Type { get; set; }
        public string? UserRespond {  get; set; }
        public string? loaicont { get; set; }
        public double? Tigia { get; set; }
    }
}
