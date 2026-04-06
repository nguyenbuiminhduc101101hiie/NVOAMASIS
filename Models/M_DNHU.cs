using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_DNHU
    {
        [Key]
        public Guid DeNghiHoanUngID { get; set; }
        public Guid? DeNghiTamUngID { get; set; }
        public string? So { get; set; }
        public DateTime? Ngay { get; set; }
        public string? Kinhgui { get; set; }
        public string? Tennguoidenghi { get; set; }
        public string? Bophan { get; set; }
        public string? Noidung { get; set; }
        public double? Sotien { get; set; }
        public string? Loaitiente { get; set; }
        public bool? Continued { get; set; } = true;
        public string? Chungtukemtheo { get; set; }
        public string? Bkno { get; set; }
        public string? Hbl { get; set; }
        public bool? Approve { get; set; }
        public string? DateApprove { get; set; }
        public string? UserUpdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? Files { get; set; }
        public string? Trangthai { get; set; }
        public double? tigia { get; set; }

    }
}
