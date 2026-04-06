using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Product_Price
    {
        [Key]
        public Guid id { get; set; }
        public Guid? pricingid { get; set; }
        public string? MaRFQ { get; set; }
        public string? agent { get; set; }
        public string? line { get; set; }
        public string? note { get; set; }
        public string? Productpriceno { get; set; }
        public string? UserUPdate { get; set; }
        public string? DateUpdate { get; set; }
        
        public string? Trangthai { get;set; }
        public string? noidung { get; set; }
        public string? noidungcongviec { get; set; }
        public string? tieude { get; set; }
        public string? userguiemail { get; set; }
        public string? loai { get; set; }

        public bool? Approve { get; set; } = false;
    }
}
