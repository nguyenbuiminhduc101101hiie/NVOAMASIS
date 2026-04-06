using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Duan
    {
        [Key]
        public Guid Id { get; set; }
        public string? TenDuan { get; set; }
        public string? MaDuan { get; set; }
        public string? NguoiPhuTrach { get; set; }
        public string? NoiDung { get; set; }
        public string? CongNghe { get; set; }
        public string? NhanVienThamGia { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public string? EmailNguoiPhuTrach { get; set; }
        public string? UserUPdate { get; set; }
        public string? DateUpdate { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? Links { get; set; }
        public string? loai { get; set; }
        public string? Pricingno { get; set; }
        public string? POL { get; set; }
        public string? POD { get; set; }

        public string? LoaiHangHoa { get; set; }
        public double? TrongLuong { get; set; }
        public string? KichThuoc { get; set; }
        public string? ThoiGianGiaoHang { get; set; }
        public string? PhuongThucVanChuyen { get; set; }
        public string? DichVuBoSung { get; set; }
        public string? Size { get; set; }

    }
}
