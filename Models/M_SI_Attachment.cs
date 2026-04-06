using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    [Table("SI_Attachment")]
    public class M_SI_Attachment
    {
        [Key]
        public Guid AttachmentId { get; set; }

        public Guid SIID { get; set; }

        [MaxLength(500)]
        public string FileName { get; set; } = "";

        [Column(TypeName = "varbinary(max)")]
        public byte[] Content { get; set; } = Array.Empty<byte>();

        [MaxLength(255)]
        public string? ContentType { get; set; }

        public DateTime UploadedUtc { get; set; }
    }
}
