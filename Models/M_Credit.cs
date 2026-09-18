using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Credit
    {
        [Key]
        public Guid creditid { get; set; }
        public Guid mblid { get; set; }
        public Guid hblid { get; set; }
        public Guid? Quanly_tauid { get; set; }
        public Guid? Quanly_contid { get; set; }
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
        public bool? dathanhtoan { get; set; } = false;
        public DateTime? Date_Of_EST { get; set; }
        public string? Size { get; set; }
        public DateTime? Date_In_Yard { get; set; }
        public string? Manufacturing_Date { get; set; }
        public string? IT { get; set; }
        public string? Com_Code { get; set; }
        public string? COMPONENTS_DETAILS { get; set; }
        public string? LOC { get; set; }
        public string? DM_Code { get; set; }
        public string? RP_Code { get; set; }
        public double? LHT { get; set; }
        public double? WDT { get; set; }
        public double? R { get; set; }
        public double? Hours { get; set; }
        public double? Labor_Cost { get; set; }
        public double? Material_Cost { get; set; }
        public double? Labor_Rate { get; set; }
        public string? Billing { get; set; }
        public string? Owner { get; set; }
        public string? Location { get; set; }
    }
}
