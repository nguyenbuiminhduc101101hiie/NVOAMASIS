using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace NVOAMASIS.Models
{
    public class HistoryLog
    {
        [Key]
        public Guid Id { get; set; }
        public string? UserName { get; set; } = string.Empty;
        public string? Action { get; set; } = string.Empty; // Create, Update, Delete
        public string? EntityName { get; set; } = string.Empty; // Tên bảng/đối tượng
        public Guid? EntityId { get; set; } 
        public string? No { get; set; }
        public string? Changes { get; set; } // JSON của các thay đổi
        public DateTime? Timestamp { get; set; } = DateTime.Now;

 
    }
}
