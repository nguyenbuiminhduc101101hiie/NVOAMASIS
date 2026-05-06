namespace NVOAMASIS.Models
{
    /// <summary>
    /// Bảng lưu token QR Arrival Notice theo (HblId, Type, BillType, Branches).
    /// Mỗi cặp company + branch có QR riêng; xuất lại cùng lựa chọn thì tái dùng token còn hạn.
    /// </summary>
    public class ArrivalNoticeQrTokenRecord
    {
        public string Token { get; set; } = "";
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
        public string BillType { get; set; } = "PASL";
        public string Branches { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }

    public static class ArrivalNoticeQrCacheKeys
    {
        public const string Prefix = "AN_QR_";
    }
}

