using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_CongNoHoaDonDauVao
    {
        [Key]
        public Guid congnoHoadonDauvaoid { get; set; }
        public Guid hoadondauvaoid { get; set; }
        public double? tongtien { get; set; }
        public double? tigia { get; set; }
        public DateTime? ngaytra { get; set; }
        public string? tiente { get; set; }
        public string? userupdate { get; set; }
        public DateTime? dateupdate { get; set; }
        public bool? continued { get; set; } = true;
        public bool? editable { get; set; } = true;
        public bool? approve { get; set; } = false;
    }
}
