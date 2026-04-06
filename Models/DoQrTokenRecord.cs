namespace NVOAMASIS.Models
{
    /// <summary>
    /// Bảng lưu token QR D/O theo (HblId, Type). Một HBL + loại DO chỉ có một token, tái sử dụng khi xuất lại.
    /// </summary>
    public class DoQrTokenRecord
    {
        public string Token { get; set; } = "";
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
