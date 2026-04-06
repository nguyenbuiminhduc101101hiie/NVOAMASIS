using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class Quotation_Tico
    {
        [Key]
        public Guid? QuotationTicoID { get; set; }
        public string? No { get; set; }
        public string? den { get; set; }
        public string? ngay { get; set; }
        public string? diachi { get; set; }
        public string? mbl { get; set; }
        public string? hbl { get; set; }
        public string? vessel { get; set; }
        public string? volume { get; set; }
        public string? por { get; set; }
        public string? pol { get; set; }
        public string? podel { get; set; }
        public string? term { get; set; }
        public string? Description { get; set; }
        public string? shipper { get; set; }
        public string? consignee { get; set; }
        public string? containerType { get; set; }
        public string? polpod { get; set; }
        public string? etd { get; set; }
        public string? validity { get; set; }
        public string? remarks { get; set; }
        public bool? Editable { get; set; }
        public bool? Continued { get; set; }
        public bool? Approve { get; set; }
        public string? UserUpdate { get; set; }
        public string? DateUpdate { get; set; }
        public string? text1 { get; set; }
        public string? text2 { get; set; }
        public string? text3 { get; set; }
        public string? sale { get; set; }
        public string? sea_air { get; set; }
        public Guid? cusid { get; set; }
        public bool? closed { get; set; }
        public string? text4 { get; set; }
        public string? text5 { get; set; }
        public string? text6 { get; set; }
        public string? lichtau { get; set; }
        public string? placeofdelivery { get; set; }
        public string? placeofpickup { get; set; }
    }
}
