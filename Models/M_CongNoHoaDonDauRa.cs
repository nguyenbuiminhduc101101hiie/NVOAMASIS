using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_CongNoHoaDonDauRa
    {
        [Key]
        public Guid congnoHoadonDauraid { get; set; }
        public Guid hoadondauraid { get; set; }
        public double? tongtien { get; set; }
        public DateTime? ngaytra { get; set; }
        public string? tiente { get; set; }
        public string? userupdate { get; set; }
        public DateTime? dateupdate { get; set; }
        public bool? continued { get; set; } = true;
        public bool? editable { get; set; } = true;
        public bool? approve { get; set; } = false;
        public double? tigia { get; set; }
    }
}
