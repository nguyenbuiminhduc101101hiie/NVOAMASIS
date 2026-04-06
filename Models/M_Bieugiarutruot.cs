using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Bieugiarutruot
    {
        [Key]
        public Guid RutRuotID { get; set; }
        public DateTime? NgayHieuLuc { get; set; }
        public string? CangICDDepot { get; set; }
        public string? DiaChi { get; set; }
        public double? RutRuotTaiBaiCY { get; set; }
        public double? RutRuoctTaiKhoCFS { get; set; }
        public double? RutRuocKiemHoa { get; set; }
        public double? PhiNangContainerTuDauKeoLenBaiRutHang { get; set; }
        public double? PhiNangRongLenXeTraContainerRong { get; set; }
        public double? PhiLuuContainer { get; set; }
        public double? PhiLuuBaiHang { get; set; }
        public double? PhiThueKhoCFSKiemHoa { get; set; }
        public double? PhiNhanCongKiemHoa { get; set; }
        public double? PhiNangContainerKiemHoa { get; set; }
        public double? PhiVeSinhContainer { get; set; }
        public double? PhiHaTangCangBien { get; set; }
        public string? Userupdate { get; set; }
        public string? Dateupdate { get; set; }
        public string? BieugiarutruotNo { get; set; }
        public string? TrangThai { get; set; }
        public string? TienTe { get; set; }
        public bool Approve { get; set; }
    }

}
