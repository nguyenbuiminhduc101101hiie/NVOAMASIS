using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_TariffHeader
    {
        [Key]
        public Guid Id { get; set; }
        public string TariffCode { get; set; }
        public string TariffName { get; set; }
        public Guid ChargeTypeId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ShippingLineId { get; set; }
        public Guid DepotId { get; set; }
        public Guid PortId { get; set; }
        public string ContainerTypeId { get; set; }
        public string CargoTypeId { get; set; }
        public string? Direction { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
