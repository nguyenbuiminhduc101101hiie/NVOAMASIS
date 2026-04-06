using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_DNTT_Logistics
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? Customerid { get; set; }
        public string? So { get; set; }
        public DateTime? NGAY { get; set; }
        public string? Kinhgui { get; set; }
        public string? Tennguoidenghi { get; set; }
        public string? Bophan { get; set; }
        public string? Noidung { get; set; }
        public string? PTTT { get; set; }
        public double? Sotientruocthue { get; set; }
        public double? Sotienthue { get; set; }
        public double? Thanhtien { get; set; }
        public string? Loaitiente { get; set; }
        public string? Sotk { get; set; }
        public string? Hbl { get; set; }
        public string? Chungtukemtheo { get; set; }
        public string? Bkno { get; set; }
        public string? Nguoilap { get; set; }
        public string? Ketoantruong { get; set; }
        public string? Giamdoc { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public string? Type { get; set; }
        public string? UserUpdate { get; set; }
        public string? Dateupdate { get; set; }
        public double? Tigia { get; set; }
        public int? SoLanGuiEmail { get; set; } = 0;

        public string? EmailUserApprove { get; set; }
        public string? Lidoduyet { get; set; }

        public string? timeDuyet { get; set; }
        public string? Trangthai { get; set; }
    }
}
