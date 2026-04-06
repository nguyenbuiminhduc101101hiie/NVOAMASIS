namespace NVOAMASIS.Models
{
    public class DebtReportRow
    {
        public int? No { get; set; }
        public string? Description { get; set; }
        public string? Hbl { get; set; }
        public double? Qty { get; set; }
        public string? Unit { get; set; }
        public double? Total_Debit { get; set; }
        public double? Total_Credit { get; set; }

        public double? Total_HoaDonDauRa { get; set; }

        public double? Total_HoaDonDauVao { get; set; }

        public string? InvoiceNo { get; set; }
    }
}
