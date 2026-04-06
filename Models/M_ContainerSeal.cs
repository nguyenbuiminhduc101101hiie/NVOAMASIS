using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_ContainerSeal
    {
        [Key]
        public Guid SealId { get; set; }

        public Guid CTN_ID { get; set; }

        [StringLength(30)]
        public string SealNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string? SealType { get; set; }

        [StringLength(20)]
        public string? SealMaterial { get; set; }

        [StringLength(30)]
        public string? AppliedAt { get; set; }

        public DateTime? AppliedDate { get; set; }

        public DateTime? RemovedDate { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? Remark { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }
    }
}
