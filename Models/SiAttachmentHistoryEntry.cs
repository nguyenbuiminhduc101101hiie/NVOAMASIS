using System.Text.Json.Serialization;

namespace NVOAMASIS.Models
{
    /// <summary>Một dòng lịch sử đính kèm/ghỡ file trên SI (lưu trong AttachmentHistoryJson).</summary>
    public sealed class SiAttachmentHistoryEntry
    {
        /// <summary>ISO 8601 UTC, ví dụ 2026-03-30T10:15:00.0000000Z</summary>
        public string At { get; set; } = "";

        public string? User { get; set; }

        /// <summary>attach | remove_one | remove_all</summary>
        public string? Action { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? FileNames { get; set; }
    }
}
