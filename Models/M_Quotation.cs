using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Quotation
    {
        [Key]
        public Guid quotationID { get; set; }
        public Guid? customer_id { get; set; }
        public string? quotationNo { get; set; }
        public string? BookingNo { get; set; }
        public string? Subject { get; set; }
        public string? o1 { get; set; }
        public string? o2 { get; set; }
        public string? o3 { get; set; }
        public string? o4 { get; set; }
        public string? terms { get; set; }
        public string? type { get; set; }
        public string? pol { get; set; }
        public string? pod { get; set; }
        public string? TransitTime { get; set; }
        public string? Transitport { get; set; }
        public string? shippingline { get; set; }
        public string? frequency { get; set; }
        public string? Routing { get; set; }
        public string? SaleName { get; set; }
        public string? remarks { get; set; }
        public DateTime? validDate { get; set; }
        public bool? Continued { get; set; }
        public bool? Editable { get; set; }
        public string? userupdate { get; set; }
        public bool? Approve { get; set; }
        public string? dateupdate { get; set; }
        public string? ViTri { get; set; }
        public string? Branch { get; set; }
        public string? MasterColoader { get; set; }
        public DateTime? ETA { get; set; }
        public DateTime? ETD { get; set; }
        public Guid? docid { get; set; }
        public DateTime? dated { get; set; }
        public string? description { get; set; }
        public string? status { get; set; }
        public string? RFQ_No { get; set; }
        public double? SL { get; set; }
        public string? loaivolume { get; set; }

    }
}
