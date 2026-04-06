using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    public class M_LenhDieuXe
    {
        [Key]
        public Guid Id { get; set; }
        public Guid?  YeuCauTruckingId { get; set; }
        public string? Nvdieuxe { get; set; }
        public string? Taixe { get; set; }
        public string? Phuxe { get; set; }
        public string? Nhaxe { get; set; }
        public string? Ghichu { get; set; }
        public string? Loaidhvc { get; set; }
        public string? Loaiphuongtien { get; set; }
        public string? Soxe { get; set; }
        public string? Somooc { get; set; }
        public double? GiaCost { get; set; }
        public double? Tongkm { get; set; }
        public double? Dinhmucdau { get; set; }
        public double? Tamung { get; set; }
        public string? Sophieu { get; set; }
        public DateTime? Ngaylap { get; set; }
        public DateTime? Ngaynhanlenh { get; set; }
        public DateTime? Ngaydi { get; set; }
        public DateTime? Ngayve { get; set; }
        public string? Lenhdieuxeno { get; set; }
        public string? TrangthaiCt { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Approve { get; set; } = false;
        public string? Pol { get; set; }
        public string? Pod { get; set; }
        //[JsonIgnore]
        //public M_HBL? HBL { get; set; }
    }
}
