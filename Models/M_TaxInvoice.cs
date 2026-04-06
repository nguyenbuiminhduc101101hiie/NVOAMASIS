using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TaxInvoice
    {
        [Key]
        public Guid TaxInvoiceID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? BillNumber { get; set; }
        public string? SerialNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? soPhieuThu { get; set; }
        public DateTime? DateInvoice { get; set; }
        public string? MethodPayment { get; set; }
        public string? VAT { get; set; }
        public string? VATShow { get; set; }
        public string? Chungtu { get; set; }
        public double? Exchange { get; set; }
        public bool? huy { get; set; } = false;
        public string? lyDoHuy { get; set; }
        public string? REF { get; set; }
        public string? mbl { get; set; }
        public string? hbl { get; set; }
        public bool? Approve { get; set; } = false;
        public bool? Editable { get; set; } = true;
        public bool? Continued { get; set; } = true;
        public string? UserID { get; set; }
        public DateTime? Updatetime { get; set; }
        public string? VesselVoy { get; set; }
        public string? branch { get; set; }
        public bool? daXuatHDDT { get; set; } = false;
        public string? mauSo { get; set; }
        public string? InvoiceGUID { get; set; }
        public bool? daKy { get; set; } = false;
        public double? tongTruocThue { get; set; }
        public double? tongThue { get; set; }
        public double? tongSauThue { get; set; }
        public string? ghichuNoibo { get; set; }
    }
}
