using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class M_BillSeaLayoutForm
    {
        [Key]
        public Guid BillSeaLayoutFormId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FormName { get; set; } = string.Empty;

        public byte[] MrtContent { get; set; } = Array.Empty<byte>();

        public byte[]? AttachMrtContent { get; set; }

        public byte[]? Logo { get; set; }

        public byte[]? FormBillAir { get; set; }

        [MaxLength(50)]
        public string FormKind { get; set; } = nameof(BillLayoutFormKind.Sea);

        [MaxLength(260)]
        public string SourceTemplate { get; set; } = "BillSea_NVOCC.mrt";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
