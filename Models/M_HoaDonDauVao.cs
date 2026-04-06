using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_HoaDonDauVao
    {
        [Key]
        public Guid hoadondauvaoid { get; set; }
        public Guid mblid { get; set; }
        public Guid hblid { get; set; }
        public Guid customerid { get; set; }
        public double? dongia { get; set; }
        public double? soluong { get; set; }
        public double? thanhtien { get; set; }
        public string? tiente { get; set; }
        public double? tigia { get; set; }
        public double? thue { get; set; }
        public double? thanhtiensauthue { get; set; }
        public string? sohoadonNoibo { get; set; }
        public string? sohoadonDientu { get; set; }
        public DateTime? ngayphathanhhoadonDientu { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? approve { get; set; } = false;
        public bool? editable { get; set; } = true;
        public bool? continued { get; set; } = true;
    }
}
