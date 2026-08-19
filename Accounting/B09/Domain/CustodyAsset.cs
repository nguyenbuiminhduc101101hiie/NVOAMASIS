using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

// Off-balance-sheet / custody goods for logistics and warehouse disclosures.
public class CustodyAsset
{
    public long Id { get; set; }
    public Guid CompanyId { get; set; }
    public int? ClientId { get; set; }
    public long? ShipmentId { get; set; }
    public long? ContainerId { get; set; }
    public int? WarehouseId { get; set; }

    [MaxLength(50)] public string CustodyType { get; set; } = "GoodsHeld";
    [MaxLength(300)] public string? CommodityGroup { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    [MaxLength(1000)] public string? Specification { get; set; }
    public decimal? Quantity { get; set; }
    [MaxLength(50)] public string? Unit { get; set; }
    [MaxLength(200)] public string? Condition { get; set; }
    public decimal? EstimatedValue { get; set; }
    [MaxLength(10)] public string? Currency { get; set; }

    public DateTime? ReceivedDate { get; set; }
    public DateTime? ReleasedDate { get; set; }

    public string? RightsAndObligations { get; set; }
    public string? StorageResponsibility { get; set; }
    public string? SignificantRisk { get; set; }
    public string? DisclosureNote { get; set; }
}
