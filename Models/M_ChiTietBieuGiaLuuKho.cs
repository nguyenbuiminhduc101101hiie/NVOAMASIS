using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ChiTietBieuGiaLuuKho
    {
        [Key]
        public Guid ChiTietBieuGiaLuuKhoID { get; set; }
        public Guid? BieuGiaLuuKhoID { get; set; }
        public string? DienGiai { get; set; }
        public string? LoaiContainerHangHoa { get; set; }
        public string? DonViTinh { get; set; }
        public double? GiaTien { get; set; }
        public string? TienTe { get; set; }
        public string? GhiChuHangHoa { get; set; }
        public string? UserUpdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? So { get; set; }

    }
}
