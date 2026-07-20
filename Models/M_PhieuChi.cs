using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_PhieuChi
    {
        [Key]
        public Guid PhieuchiID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? Sophieuchi { get; set; }
        public string? TKnophieuchi { get; set; }
        public string? TKCophieuchi { get; set; }
        public DateTime? Ngay { get; set; }
        public string? PTTT { get; set; }
        public string? Noidung { get; set; }
        public double? Sotien { get; set; }
        public double? Tigia { get; set; }
        public string? Currency { get; set; }
        public string? Chungtu { get; set; }
        public string? Nguoinoptien { get; set; }
        public string? Hbl { get; set; }
        public string? Bank { get; set; }
        public string? SoHoaDon { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public bool? Editable { get; set; }
        public string? Useupdate { get; set; }
        public string? DateUpdate { get; set; }
        public int? STT { get; set; }
        public string? Vesselvoy { get; set; }
        public string? Ref { get; set; }
        public string? Branch { get; set; }
        public string? Loaiphieu { get; set; }
        public string? Trangthai { get; set; }
        public string? Quyenso { get; set; }
        public string? Mbl { get; set; }
        public string? Remarks { get; set; }

        /// <summary>Token dùng chung cho link duyệt trên email (1 phiếu = 1 token).</summary>
        public string? ApproveToken { get; set; }
        public string? ApproveBy { get; set; }
        public Guid? ApproveByUserId { get; set; }
        public DateTime? ApproveDate { get; set; }
    }
}
