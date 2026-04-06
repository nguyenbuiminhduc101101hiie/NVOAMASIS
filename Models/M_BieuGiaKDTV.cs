using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BieuGiaKDTV
    {
        [Key]
        public Guid KDTVID { get; set; }
        public string? BieuGiaKDTVNo { get; set; }
        public DateTime? NgayApDung { get; set; }
        public string? ChiCucKiemDich { get; set; }
        public string? DiaChiChiCuc { get; set; }
        public string? DienThoai { get; set; }
        public string? LoaiHinhKiemDich { get; set; }
        public double? CONT20_1 { get; set; }
        public double? CONT20_2 { get; set; }
        public double? CONT20_3 { get; set; }
        public double? CONT20_4 { get; set; }
        public double? CONT20_5 { get; set; }
        public double? CONT40_1 { get; set; }
        public double? CONT40_2 { get; set; }
        public double? CONT40_3 { get; set; }
        public double? CONT40_4 { get; set; }
        public double? CONT40_5 { get; set; }
        public string? TrangThai { get; set; }
        public string? Userupdate { get; set; }
        public string? DateUpdate { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }

    }
}
