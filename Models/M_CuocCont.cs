using System.ComponentModel.DataAnnotations;
namespace NVOAMASIS.Models
{
    public class M_CuocCont
    {
        [Key]
        public Guid id { get; set; }
        public Guid hblid{get;set;}
        public Guid itemid{get;set;}
        public Guid customerid{get;set;}
        public string? No{get;set;}
        public double? unitprice{get;set;}
        public double? quantity{get;set;}
        public double? total{get;set;}
        public string? cur{get;set;}
        public double? exc{get;set;}
        public string? userupdate{get;set;}
        public string? dateupdate{get;set;}
        public bool? continued { get; set; } = true;
        public bool? approve { get; set; } = false;
        public bool? editable { get; set; } = true;
        public double? Vat{get;set;}
        public double? thanhtiensauthue{get;set;}
    }
}
