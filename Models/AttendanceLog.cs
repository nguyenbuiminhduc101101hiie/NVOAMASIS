using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models
{
    public class AttendanceLog
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.Now;
        // Store normalized client public IP detected
        [MaxLength(64)]
        public string IPAddress { get; set; } = string.Empty;
        // True = onsite (whitelisted), false = remote
        public bool IsOnsite { get; set; }
        // Optional: e.g. WEB, API, AUTO
        [MaxLength(20)]
        public string SourceType { get; set; } = "WEB";
        // Convenience partition (local date based on office timezone if needed later)
        public DateTime LocalDate { get; set; } = DateTime.Today;
        public string Session { get; set; }
    }
}
