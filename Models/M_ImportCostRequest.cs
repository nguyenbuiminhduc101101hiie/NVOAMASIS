using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ImportCostRequest
    {
        [Key]
        public Guid Id { get; set; }
        public string? MaRFQ { get; set; }
        public string? Tenhang { get; set; }
        public double? Soluong { get; set; }
        public string? Donvitinh { get; set; }
        public string? Quocgiaxuatxu { get; set; }
        public string? Cangxuat { get; set; }
        public string? Cangnhap { get; set; }
        public string? Incoterm { get; set; }
        public DateTime? Ngayguiyeucau { get; set; }
        public string? Trangthai { get; set; }
        public string? Loaihinh { get; set; }
        public string? Noidung { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Continued { get; set; } = true;
        public bool? Editable { get; set; } = true;
        public string? Userhandle { get; set; }
        public string? CreateAt { get; set; }
        public string? Usercreate { get; set; }
        /// <summary>Id các file đính kèm (bảng ProductPriceAttachments), ngăn cách bằng dấu ;</summary>
        public string? AttachmentIds { get; set; }
    }
}
