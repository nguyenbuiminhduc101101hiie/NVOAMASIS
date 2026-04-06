using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_CustomCostRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string? MaRFQ { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;
        public DateTime? NgayYeuCau { get; set; }
        public string? UserCreate { get; set; }
        public string? CreateAt { get; set; }
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public string? Loaihang { get; set; }
        public string? Loaihinh { get; set; }
        public string? Noidung { get; set; }
        public string? Userhandle {  get; set; }
        public string? Trangthai { get; set; }
    }
}
