using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    /// <summary>File đính kèm (lưu dạng byte trong DB) của các bản ghi báo giá 3.2 - 3.6. Bản ghi chủ lưu danh sách AttachmentId ở cột AttachmentIds.</summary>
    [Table("ProductPriceAttachments")]
    public class M_ProductPriceAttachment
    {
        [Key]
        public Guid AttachmentId { get; set; }

        /// <summary>Import / Export / Truck / KTCL / Custom.</summary>
        [MaxLength(20)]
        public string OwnerType { get; set; } = "";

        [MaxLength(500)]
        public string FileName { get; set; } = "";

        [Column(TypeName = "varbinary(max)")]
        public byte[] Content { get; set; } = [];

        [MaxLength(255)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string? UserUpload { get; set; }

        public DateTime UploadedUtc { get; set; }
    }

    /// <summary>Metadata — không load Content.</summary>
    public sealed class M_ProductPriceAttachmentItem
    {
        public Guid AttachmentId { get; set; }
        public string FileName { get; set; } = "";
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
    }
}
