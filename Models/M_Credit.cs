using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Credit
    {
        [Key]
        public Guid creditid { get; set; }
        public Guid mblid { get; set; }
        public Guid hblid { get; set; }
        public Guid Quanly_tauid { get; set; }
        public Guid Quanly_contid { get; set; }
        public Guid quotationid { get; set; }
        public Guid customerid { get; set; }
        public Guid itemid { get; set; }
        public double? dongia { get; set; } = 0;
        public double? soluong { get; set; } = 0;
        public double? thanhtien { get; set; }
        public string? tiente { get; set; }
        public double? thue { get; set; } = 0;
        public double? thanhtiensauthue { get; set; }
        public bool? chiho { get; set; } = false;
        public double? tigiacredit { get; set; }
        public double? tigiahoadondauvao { get; set; }
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? continued { get; set; } = true;
        public bool? editable { get; set; } = true;
        public bool? approve { get; set; } = false;
        public string? type { get; set; }
        public bool? DNTT { get; set; } = false;
        public bool? DNTU { get; set; } = false;
        public bool? Copied { get; set; } = false;
        public string? sodntt { get; set; }
        public string? ghichu_credit { get; set; }
        public string? ContainerNo { get; set; }
    }
}
