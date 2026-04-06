using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BieuGiaLuuKho
    {
        [Key]
        public Guid BieuGiaLuuKhoID { get; set; }
        public Guid? VenderID { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public string? GhiChu { get; set; }
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? Bieugialuukhono { get; set; }
        public string? Trangthai { get; set; }
    }
}
