using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_RFQ
    {
        [Key]
        public Guid RFQ_ID { get; set; }
        public string? RFQNo { get; set; }
        public Guid? CustomerID { get; set; }
        public string? ServiceType { get; set; }
        public string? POL { get; set; }
        public string? POD { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Incoterm { get; set; }
        public string? CargoDescription { get; set; }
        public string? HSCode { get; set; }
        public bool? IsDangerous { get; set; } = false;
        public bool? IsFragile { get; set; } = false;
        public bool? IsTemperatureControlled { get; set; } = false;
        public bool? IsHighValue { get; set; } = false;
        public int? PackageQuantity { get; set; }
        public decimal? TotalWeightKg { get; set; }
        public decimal? TotalVolumeCBM { get; set; }
        public string? PackageDimensions { get; set; }
        public string? ContainerType { get; set; }
        public DateTime? ReadyDate { get; set; }
        public DateTime? RequestedShipDate { get; set; }
        public bool? NeedInlandTransport { get; set; } = false;
        public bool? NeedCustomsClearance { get; set; } = false;
        public bool? NeedPacking { get; set; } = false;
        public bool? NeedWarehousing { get; set; } = false;
        public bool? NeedLabeling { get; set; } = false;
        public bool? NeedAMSOrISF { get; set; } = false;
        public string? SpecialNote { get; set; }
        public string? AttachmentLink { get; set; }
        public string? RFQStatus { get; set; }
        public string? SalesPerson { get; set; }
        public DateTime? QuoteDeadline { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string? filesPath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? Approve { get; set; }
        public bool? Continued { get; set; }
        public string? UserUpdate { get; set; }
        public string? dateupdate { get; set; }
    }

}
