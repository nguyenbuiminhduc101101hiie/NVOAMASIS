using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BieuGiaKTCL
    {
        [Key]
        public Guid BieuGiaKTCLID { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public string? DichVuKiemTra { get; set; }
        public string? CoQuanThucHien { get; set; }
        public string? LoaiHoaDon { get; set; }
        public string? NoiDungDichVu { get; set; }
        public string? LoaiMucPhi { get; set; }
        public double? SoLuong { get; set; }
        public double? DonGia { get; set; }
        public string? DonViTinh { get; set; }
        public double? ThanhTien { get; set; }
        public string? TienTe { get; set; }
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? BieuGiaKTCLNo { get; set; }
        public string? TrangThai { get; set; }
    }
}
