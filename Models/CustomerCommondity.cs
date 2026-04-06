using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class CustomerCommondity
    {
        [Key]
        public Guid? CustomerCommondity_ID { get; set; }
        public Guid? Customer_ID { get; set; }
        public Guid? Commondity_ID { get; set; }
        public DateTime? From_date { get; set; }
        public DateTime? To_date { get; set; }
        public string? Season { get; set; }
        public double? Min_tueMonth { get; set; }
        public double? AVG_tueMonth { get; set; }
        public double? Max_tueMonth { get; set; }
        public string? Remarks { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? UserID { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? KhachHang { get; set; }
        public string? SoHD { get; set; }
        public string? NgayKy { get; set; }
        public string? NgayHetHan { get; set; }
        public bool? HD_NguyenTac { get; set; } = false;
        public bool? HD_DaiLyHaiQuan { get; set; } = false;
        public bool? HD_UyThacXuatKhau { get; set; } = false;
        public bool? Other { get; set; } = false;
        public bool? active { get; set; } = false;
        public string? hinhhopdong { get; set; }
    }
}
