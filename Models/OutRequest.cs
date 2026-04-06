using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class OutRequest
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
        // Pending, Approved, Rejected
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewerUserId { get; set; }
        [MaxLength(500)]
        public string? ReviewNote { get; set; }
    }
}
