using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_YeuCauTuVanTTHQ
    {
        [Key]
        public Guid YctvtthqcoID { get; set; }
        public string? Congty { get; set; }
        public string? masothue { get; set; }
        public string? diachi { get; set; }
        public string? SoYeuCau { get; set; }
        public string? UserYeuCau { get; set; }
        public string? UserTraLoi { get; set; }
        public string? TieuDeYeuCau { get; set; }
        public string? NoiDung { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string? YKien { get; set; }
        public string? UserUPdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Editable { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public string? Links { get; set; }
    }
}
