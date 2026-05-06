namespace NVOAMASIS.Models
{
    /// <summary>
    /// Bảng lưu token QR D/O theo (HblId, Type, BillType, Branches). Mỗi company/branch một QR còn hạn.
    /// </summary>
    public class DoQrTokenRecord
    {
        public string Token { get; set; } = "";
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
        public string BillType { get; set; } = "PASL";
        public string Branches { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
