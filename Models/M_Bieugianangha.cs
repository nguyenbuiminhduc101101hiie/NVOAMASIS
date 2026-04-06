using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Bieugianangha
    {
        [Key]
        public Guid BieuGiaNangHaID { get; set; }
        public string? CangICDDepot { get; set; }
        public string? DiaChi { get; set; }
        public string? LoaiContainer { get; set; }
        public string? LoaiHinhNangHa { get; set; }
        public DateTime? ThoiGianApDung { get; set; }
        public double? NangLenPhuongTien { get; set; }
        public double? HaXuongBai { get; set; }
        public double? NangHaNoiBo { get; set; }
        public double? NangHaRong { get; set; }
        public double? NangHaHang { get; set; }
        public double? NangHaKiemHoaSuaChuaVeSinh { get; set; }
        public double? NangHaKiemHoa { get; set; }
        public double? NangHaSieuTruongSieuTrong { get; set; }
        public double? NangHaNgoaiGioHanhChinh { get; set; }
        public double? NangHaKhuCachLy { get; set; }
        public double? PhuPhiHangNguyHiem { get; set; }
        public double? PhuPhiNgoaiGio { get; set; }
        public double? PhuPhiDungThietBiDacBiet { get; set; }
        public string? Loaitiente { get; set; }

        public string? BieugiananghaNo { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? TrangThai { get; set; }
        public bool Approve { get; set; }  
    }
}
