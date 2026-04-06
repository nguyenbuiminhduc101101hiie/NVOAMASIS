namespace NVOAMASIS.Models
{
    /// <summary>
    /// Bảng lưu token QR Arrival Notice theo (HblId, Type). Một HBL + loại (Sea/Air) chỉ có một token.
    /// </summary>
    public class ArrivalNoticeQrTokenRecord
    {
        public string Token { get; set; } = "";
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }

    public static class ArrivalNoticeQrCacheKeys
    {
        public const string Prefix = "AN_QR_";
    }
}

