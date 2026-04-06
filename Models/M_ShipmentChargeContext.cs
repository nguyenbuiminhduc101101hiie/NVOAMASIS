using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ShipmentChargeContext
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }
        public Guid ContainerId { get; set; }
        public Guid ChargeTypeId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShippingLineId { get; set; }
        public string? DepotId { get; set; }
        public string? PortId { get; set; }
        public string? ContainerTypeId { get; set; }
        public string? Direction { get; set; }
        public DateTime? EmptyPickupDate { get; set; }
        public DateTime? FullDischargeDate { get; set; }
        public DateTime? FullDeliveryDate { get; set; }
        public DateTime? EmptyReturnDate { get; set; }
        public DateTime? StorageInDate { get; set; }
        public DateTime? StorageOutDate { get; set; }
        public int FreeDays { get; set; }
        public int? BillableDays { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
