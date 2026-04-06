using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ExportCostRequest
    {
        [Key]
        public Guid id { get; set; }
        public string? maRFQ { get; set; }
        public string? Tenhang { get; set; }
        public double? soluong { get; set; }
        public string? donvitinh { get; set; }
        public string? cangdi { get; set; }
        public string? cangden { get; set; }
        public string? incoterm { get; set; }
        public string? phuongthucVc { get; set; }
        public DateTime? ngayguiyeucau { get; set; }
        public string? trangthai { get; set; }
        public string? noidung { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? continued { get; set; }
        public bool? editable { get; set; }
        public bool? approve { get; set; }
        public string? Userhandle { get; set; }
        public string? CreateAt { get; set; }
        public string? Usercreate { get; set; }
        public string? loaihinh { get; set; }
    }
}
