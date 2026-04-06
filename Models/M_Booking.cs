using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_Booking
    {
        [Key]
        public Guid? Booking_ID { get; set; }
        public Guid? Customer_ID { get; set; } 
        public Guid? POD_ID { get; set; } 
        public Guid? POL_ID { get; set; }
        public Guid? Agency_ID { get; set; } 
        public double? SL20GPOwner { get; set; }
        public double? SL40GPOwner { get; set; }
        public double? SL40HCOwner { get; set; }
        public double? SL45HCOwner { get; set; }
        public double? SL20RFOwner { get; set; }
        public double? SL40RFOwner { get; set; }
        public double? SL40RHOwner { get; set; }
        public double? SL20OTOwner { get; set; }
        public double? SL40OTOwner { get; set; }
        public double? Sl20FROwner { get; set; }
        public double? SL40FROwner { get; set; }
        public double? SLCBMOwner { get; set; }
        public double? SL20GPSale { get; set; }
        public double? SL40GPSale { get; set; }
        public double? SL40HCSale { get; set; }
        public double? SL45HCSale { get; set; }
        public double? SL20RFSale { get; set; }
        public double? SL40RFSale { get; set; }
        public double? SL40RHSale { get; set; }
        public double? SL20OTSale { get; set; }
        public double? SL40OTSale { get; set; }
        public double? SL20FRSale { get; set; }
        public double? SL40FRSale { get; set; }
        public double? SLCBMSale { get; set; }
        public DateTime? Validate { get; set; }
        public string? Currency { get; set; }
        public double? Commission { get; set; }
        public string? SCNO { get; set; }
        public string? Remarks { get; set; }
        public bool? Status { get; set; }
        public bool? editable { get; set; }
        public bool? continued { get; set; }
        public bool? approve { get; set; }
        public string? userid { get; set; }
        public DateTime? updatetime { get; set; }
        public string? buy { get; set; }
        public string? sell { get; set; }

    }
}
