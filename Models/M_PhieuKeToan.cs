using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_PhieuKeToan
    {
        [Key]
        public Guid PhieuketoanID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? Sophieuketoan { get; set; }
        public string? TKnophieuketoan { get; set; }
        public string? TKCophieuketoan { get; set; }
        public DateTime? Ngay { get; set; }
        public string? PTTT { get; set; }
        public string? Noidung { get; set; }
        public double? Sotien { get; set; }
        public double? SotienVnd { get; set; }
        public double? Tigia { get; set; }
        public string? Currency { get; set; }
        public string? Chungtu { get; set; }
        public string? Nguoinoptien { get; set; }
        public string? BillNo { get; set; }
        public string? Bank { get; set; }
        public string? Mblcarrier { get; set; }
        public string? SoHD { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public bool? Editable { get; set; }
        public string? Userid { get; set; }
        public string? Updatetime { get; set; }
        public int? STT { get; set; }
        public string? Thoihanhoantamung { get; set; }
        public string? Ngaydi { get; set; }
        public string? Ngayve { get; set; }
        public string? Mucdich { get; set; }
        public string? OLC { get; set; }
        public string? OFC { get; set; }
        public string? IFC { get; set; }
        public string? ILC { get; set; }
        public string? ICB { get; set; }
        public string? CLD { get; set; }
        public string? TK { get; set; }
        public string? NDC { get; set; }
        public string? YDC { get; set; }
        public string? NOMI { get; set; }
        public string? SALE { get; set; }
        public string? Ozb { get; set; }
        public string? Masterbill { get; set; }
        public string? Vesselvoy { get; set; }
        public string? Ref { get; set; }
        public string? Branch { get; set; }
        public string? Loaiphieu { get; set; }
        public string? Trangthai { get; set; }
        public string? Quyenso { get; set; }
        public string? Mbl { get; set; }

    }
}
