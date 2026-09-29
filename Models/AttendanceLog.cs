using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models
{
    /// <summary>
    /// 1 lần chấm công (1 lần bấm). Bảng công (HrTimesheetService) đọc theo LocalDate + Session.
    /// Cột bổ sung (PunchType ... CreatedBy) — script Scripts/AlterAttendanceLogsDaily.sql.
    /// </summary>
    public class AttendanceLog
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CheckInTime { get; set; } = DateTime.Now;
        // Store normalized client public IP detected
        [MaxLength(64)]
        public string IPAddress { get; set; } = string.Empty;
        // True = onsite (whitelisted IP hoặc trong bán kính GPS văn phòng), false = remote
        public bool IsOnsite { get; set; }
        /// <summary>WEB, MOBILE, MANUAL (HR chấm bù).</summary>
        [MaxLength(20)]
        public string SourceType { get; set; } = "WEB";
        // Convenience partition (local date based on office timezone if needed later)
        public DateTime LocalDate { get; set; } = DateTime.Today;
        /// <summary>"morning" / "afternoon" — buổi mà lần chấm này xác nhận có mặt.</summary>
        public string Session { get; set; } = string.Empty;

        /// <summary>"in" / "out"; null = chấm theo buổi (kiểu cũ).</summary>
        [MaxLength(10)]
        public string? PunchType { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Latitude { get; set; }
        [Column(TypeName = "decimal(9,6)")]
        public decimal? Longitude { get; set; }
        /// <summary>Khoảng cách tới văn phòng (m) khi chấm bằng GPS.</summary>
        public int? DistanceM { get; set; }
        /// <summary>true = HR chấm bù / sửa công.</summary>
        public bool IsManual { get; set; }
        [MaxLength(500)]
        public string? Note { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
    }
}
