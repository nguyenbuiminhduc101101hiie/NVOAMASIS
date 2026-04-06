using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
	public class In_bound
	{
		[Key]
		public Guid? Blib_id { get; set; }
		public string? soref {get; set; }
		public string? Gflc {get; set; }
		public string? Status {get; set; }
		public string? Eta {get; set; }
		public string? Vessel {get; set; }
		public string? Voyage {get; set; }
		public string? Consignee { get; set; }
		public string? Pol { get; set; }
		public string? Pod { get; set; }
		public string? Dest { get; set; }
		public string? MacangFcl { get; set; }
		public string? Kho { get; set; }
		public string? Order { get; set; }
		public string? Bl_type { get; set; }
		public string? DiadiemGiaoHang { get; set; }
		public string? CangGiaoHang { get; set; }
		public int? Stt { get; set; }
		public bool? Air { get; set; }
		public bool? Fcl { get; set; }
		public bool? Lcl { get; set; }
		public bool? Consol { get; set; }
		public string? Paidreceived { get; set; }
		public string? Lot { get; set; }
		public bool? InvoiceRequest { get; set; }
		public string? InvoiceRequestDate { get; set; }
		public bool? DebitIssued { get; set; }
		public bool? InvoiceIssued { get; set; }
		public bool? Paid { get; set; }
		public bool? Paiddebit { get; set; }
		public bool? PaidCredit { get; set; }
		public bool? NhanLenh { get; set; }
		public bool? CloseFile { get; set; }
		public string? MBL { get; set; }
		public string? HBL { get; set; }
		public string? BKNo { get; set; }
		public string? Shipper { get; set; }
		public string? Notify { get; set; }
		public string? NoDebit { get; set; }
		public string? NoCredit { get; set; }
		public string? DateReport { get; set; }
		public bool? Approve { get; set; }
		public bool? Editable { get; set; }
		public bool? Continued { get; set; }
		public string? UserUpdate { get; set; }
		public DateTime? DateUpdate { get; set; }
		public bool? Nvocc { get; set; }
		public double? Packages { get; set; }
		public double? Kgs { get; set; }
		public double? Cbm { get; set; }
		public int? Container20 { get; set; }
		public int? Container40 { get; set; }
	}
}
