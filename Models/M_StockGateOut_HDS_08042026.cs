using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_StockGateOut_HDS_08042026 : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public string? ChuHang { get; set; }
    public string? TenTau { get; set; }
    public DateTime? NgayCapBen { get; set; }
    public string? ChuyenNhap { get; set; }
    public string? ChuyenXuat { get; set; }
    public string? SoCont { get; set; }
    public string? KichCo { get; set; }
    public string? KichCoISO { get; set; }
    public string? SoNiemChi { get; set; }
    public string? SoNiemChi01 { get; set; }
    public string? SoNiemChi02 { get; set; }
    public string? HangKhaiThac { get; set; }
    public string? HKTTau { get; set; }
    public string? FE { get; set; }
    public decimal? TrongLuong { get; set; }
    public decimal? VGM { get; set; }
    public DateTime? ContVaoBai { get; set; }
    public DateTime? ContRaBai { get; set; }
    public DateTime? XeVaoCong { get; set; }
    public DateTime? XeRaCong { get; set; }
    public DateTime? NgayHoanTat { get; set; }
    public int? SoNgayLuuBai { get; set; }
    public string? SoXe { get; set; }
    public string? SoRomooc { get; set; }
    public string? PhuongAn { get; set; }
    public string? PhuongThucGiaoNhan { get; set; }
    public string? SoBooking { get; set; }
    public string? SoBL { get; set; }
    public string? GhiChu { get; set; }
    public string? GhiChuCont { get; set; }
    public string? GhiChuTaiCong { get; set; }
    public string? GhiChuTaiCauTau { get; set; }
    public string? MaDTTT { get; set; }
    public string? DoiTuongThanhToan { get; set; }
    public string? ChuyenCang { get; set; }
    public string? CangGiaoNhan { get; set; }
    public string? TTHaiQuan { get; set; }
    public string? LoaiHang { get; set; }
    public string? HangNgoaiNoi { get; set; }
    public string? ContainerStatus { get; set; }
    public string? TinhTrangVo { get; set; }
    public string? ContQuaCan { get; set; }
    public string? SoLenh { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
