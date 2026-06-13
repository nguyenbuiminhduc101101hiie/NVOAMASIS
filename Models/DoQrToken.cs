namespace NVOAMASIS.Models
{
    public class DoTokenPayload
    {
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
    }

    public static class DoQrCacheKeys
    {
        public const string Prefix = "DO_QR_";
        public static TimeSpan TokenExpiry => TimeSpan.FromHours(24);
    }
}
