using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ContainerMovement
    {
        [Key]
        public Guid MovementId { get; set; }

        public Guid? CTN_ID { get; set; }

        [StringLength(100)]
        public string? CurrentLocation { get; set; }

        [StringLength(100)]
        public string? LastLocation { get; set; }

        public DateTime? GateInDate { get; set; }

        public DateTime? GateOutDate { get; set; }

        [StringLength(30)]
        public string? MovementStatus { get; set; }

        [StringLength(30)]
        public string? MovementType { get; set; }

        public Guid? DepotId { get; set; }

        [StringLength(10)]
        public string? PortCode { get; set; }

        [StringLength(100)]
        public string? VesselName { get; set; }

        [StringLength(30)]
        public string? VoyageNo { get; set; }

        public DateTime? MovementDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? Remark { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }
    }
}
