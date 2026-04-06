using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Debit
    {
        [Key]
        public Guid debitId { get; set; }
        public Guid quotationid { get; set; }
        public Guid mblid { get; set; }
        public Guid hblid { get; set; }
        public Guid itemid { get; set; }
        public Guid customerid { get; set; }
        public string? debitno { get; set; }
        public string? pol { get; set; }
        public string? pod { get; set; }
        public string? del { get; set; }
        public double? dongia { get; set; } = 0;
        public double? soluong { get; set; } = 0;
        public double? thanhtien { get; set; } = 0;
        public string? tiente { get; set; }
        public double? thue { get; set; } = 0;
        public double? thanhtiensauthue { get; set; } = 0;
        public bool? thuho { get; set; } = false;
        public double? tigiadebit { get; set; } = 0;
        public double? tigiahoandondaura { get; set; } = 0;
        public string? userupdate { get; set; }
        public string? dateupdate { get; set; }
        public bool? continued { get; set; } = true;
        public bool? editable { get; set; } = true;
        public bool? approve { get; set; } = false;
        public string? nhom { get; set; }
        public string? ghichu { get; set; }

        public string? type { get; set; }
        public bool? daIndebit { get; set; } = false;
        public bool? daXuatHoadon { get; set; } = false;
        public bool? khoa { get; set; } = false;
        public bool? InArrival { get; set; } = false;
        public string? sohoadondaura { get; set; }
        public bool? Copied { get; set; } = false;
        public string? ghichu_debit { get; set; }
    }
}
