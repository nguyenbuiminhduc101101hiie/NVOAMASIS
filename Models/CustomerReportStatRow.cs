namespace NVOAMASIS.Models
{
    public class CustomerReportStatRow
    {
        public Guid CusReport_ID { get; set; }
        public Guid? Customer_ID { get; set; }
        public string? Customer_Code { get; set; }
        public string? COMPANY { get; set; }
        public DateTime? Visitdate { get; set; }
        public bool? Daily { get; set; }
        public bool? hoanThanh { get; set; }
        public string? ValidUser { get; set; }
        public string? thoigian { get; set; }
        public string? KPI { get; set; }
        public string? Remarks { get; set; }
        public string? userid { get; set; }
        public DateTime? updatetime { get; set; }
    }
}
