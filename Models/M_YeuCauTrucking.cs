using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_YeuCauTrucking
    {
        [Key]
        public Guid YeuCauTruckingID { get; set; }
        public string? YeuCauTruckingNo { get; set; }
        public string? Nguoiyeucau { get; set; }
        public string? Nguoitraloi { get; set; }
        public string? TieuDeEmail { get; set; }
        public string? NoidungEmail { get; set; }
        public string? ContainerLocation_empty { get; set; }
        public string? DiaDiemNhanHang { get; set; }
        public string? DiaDiemTraHang { get; set; }
        public string? LoaiCont { get; set; }
        public DateTime? NgayDukienLayHang { get; set; }
        public DateTime? NgayDuKienTraHang { get; set; }
        public string? GhiChu { get; set; }
        public string? Routing { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? Trangthai { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public bool? Approve { get; set; }
        public string? Links { get; set; }
        public string? NoidungChitietEmail { get; set; }
        
    }
}
