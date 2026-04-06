using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_RefNo
    {
        [Key]
        public Guid id { get; set; }
        public int? thang { get; set; }
        public int? nam { get; set; }
        public int? ref_number {get;set;}
        public bool? used_job { get; set; }
        public string? userused_job { get; set; }
        public bool? used_hbl { get; set; }
        public string? userused_hbl { get; set; }
        public string? userused_debit { get; set; }
        public bool? used_debit { get; set; }

        public string? userused_pricing { get; set; }
        public bool? used_pricing { get; set; }

        public string? userused_Product_Price { get; set; }
        public bool? used_Product_Price { get; set; }

        public string? userused_LDX { get; set; }
        public bool? used_LDX { get; set; }
        public string? userused_YCTrucking{ get; set; }
        public bool? used_YCTrucking { get; set; }
        public string? userused_TKHQ { get; set; }
        public bool? used_TKHQ{ get; set; }
        public string? userused_YeuCauTuVanHq { get; set; }
        public bool? used_YeuCauTuVanHQ { get; set; }

        public string? userused_BGNH { get; set; }
        public bool? used_BGNH { get; set; }
        public string? userused_BGRR { get; set; }
        public bool? used_BGRR { get; set; }

        public string? userused_Invoice { get; set; }
        public bool? used_Invoice { get; set; }

        public string? userused_KDTV { get; set; }
        public bool? used_KDTV { get; set; }

        public string? userused_KTCL { get; set; }
        public bool? used_KTCL { get; set; }

        public string? userused_BGLK { get; set; }
        public bool? used_BGLK { get; set; }
        public string? userused_DNTU { get; set; }
        public bool? used_DNTU { get; set; }

        public string? userused_DNHU { get; set; }
        public bool? used_DNHU { get; set; }

        public string? userused_ChitietLuuKho { get; set; }
        public bool? used_ChitietLuuKho { get; set; }

        public string? userused_PT { get; set; }
        public bool? used_PT { get; set; }
        public string? userused_PC { get; set; }
        public bool? used_PC { get; set; }

        public string? userused_PKT { get; set; }
        public bool? used_PkT { get; set; }

        public string? userused_RFQ { get; set; }
        public bool? used_RFQ { get; set; }

        public string? userused_RFQ_Log { get; set; }
        public bool? used_RFQ_Log { get; set; }

        public string? userused_QUO { get; set; }
        public bool? used_QUO { get; set; }

        public string? userused_DNTT { get; set; }
        public bool? used_DNTT { get; set; }

        public string? userused_Duyet_DNTT { get; set; }
        public bool? used_Duyet_DNTT { get; set; }

        public string? userused_pricing_import { get; set; }
        public bool? used_pricing_import { get; set; }
        public string? userused_pricing_truck { get; set; }
        public bool? used_pricing_truck { get; set; }

        public string? userused_pricing_KTCL { get; set; }
        public bool? used_pricing_KTCL { get; set; }

        public string? userused_pricing_Customs { get; set; }
        public bool? used_pricing_Customs { get; set; }

        public string? userused_IssueRpt { get; set; }
        public bool? used_IssueRpt { get; set; }

        public string? userused_CC { get; set; }
        public bool? used_CC { get; set; }

        public string? userused_Orderno { get; set; }
        public bool? used_Orderno { get; set; }

        public string? userused_CamKet { get; set; }
        public bool? used_CamKet { get; set; }

        public string? userused_ACC { get; set; }
        public bool? used_ACC { get; set; }

    } 
}
