using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("Booking_LenhCapRong_Attachment")]
    public class M_Booking_LenhCapRong_Attachment
    {
        [Key]
        public Guid AttachmentId { get; set; }

        public Guid BookingId { get; set; }

        [MaxLength(500)]
        public string FileName { get; set; } = "";

        [Column(TypeName = "varbinary(max)")]
        public byte[]? Content { get; set; }

        [MaxLength(255)]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        [MaxLength(100)]
        public string? UserUpload { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime UploadedUtc { get; set; }
    }

    /// <summary>Metadata list — không load Content.</summary>
    public sealed class M_Booking_LenhCapRong_AttachmentListItem
    {
        public Guid AttachmentId { get; set; }
        public Guid BookingId { get; set; }
        public string FileName { get; set; } = "";
        public string? ContentType { get; set; }
        public string? UserUpload { get; set; }
        public string? Note { get; set; }
        public DateTime UploadedUtc { get; set; }
        public long FileSize { get; set; }
    }
}
