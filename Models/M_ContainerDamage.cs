using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ContainerDamage
    {
        [Key]
        public Guid DamageId { get; set; }

        public Guid CTN_ID { get; set; }

        [StringLength(30)]
        public string? DamageType { get; set; }

        [StringLength(50)]
        public string? DamageArea { get; set; }

        [StringLength(20)]
        public string? Severity { get; set; }

        [StringLength(500)]
        public string? DamageDescription { get; set; }

        [StringLength(30)]
        public string? DetectedAt { get; set; }

        public DateTime? DetectedDate { get; set; }

        [StringLength(30)]
        public string? ResponsibleParty { get; set; }

        [StringLength(30)]
        public string? ClaimStatus { get; set; }

        public decimal? RepairCost { get; set; }

        [StringLength(5)]
        public string? Currency { get; set; }

        [StringLength(255)]
        public string? PhotoUrl { get; set; }

        [StringLength(255)]
        public string? Remark { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        [StringLength(50)]
        public string? LastUpdatedBy { get; set; }
    }
}
