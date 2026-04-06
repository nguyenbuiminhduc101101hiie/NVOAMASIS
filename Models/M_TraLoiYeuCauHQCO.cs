using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TraLoiYeuCauHQCO
    {
        [Key]
        public Guid TraLoiyeuCauHQCOID { get; set; }
        public Guid? YctvtthqcoID { get; set; }
        public string? NguoiTraLoi { get; set; }
        public DateTime? ThoiGian { get; set; }
        public string? NoiDung { get; set; }
        public string? UserUpdate { get; set; }
        public string? Dateupdate { get; set; }
        public bool? Approve { get; set; }



    }
}
