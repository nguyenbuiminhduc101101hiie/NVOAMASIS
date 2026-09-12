namespace NVOAMASIS.Models
{
    public class BkavInvoiceSettings
    {
        public string ServiceUrl { get; set; } = "https://ws.ehoadon.vn/WSPublicEHoaDon.asmx";
        public string PartnerGuid { get; set; } = string.Empty;
        public string PartnerToken { get; set; } = string.Empty;
        public long Mode { get; set; } = 6;
        public int DefaultCreateCommandType { get; set; } = 101;
        public string OutputFolder { get; set; } = "exports/bkav";
    }
}
