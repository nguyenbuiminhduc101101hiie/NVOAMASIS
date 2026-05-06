namespace NVOAMASIS.Models
{
    /// <summary>Payload cached theo token QR Arrival Notice (public link).</summary>
    public class ArrivalNoticeTokenPayload
    {
        public Guid HblId { get; set; }
        public string Type { get; set; } = "";
        public string BillType { get; set; } = "PASL";
        public string Branches { get; set; } = "";
    }
}
