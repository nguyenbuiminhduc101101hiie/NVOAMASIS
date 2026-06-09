namespace NVOAMASIS.Models
{
    public class InvoicePdfParsedData
    {
        public string? Serial { get; set; }
        public string? BuyerTaxCode { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerAddress { get; set; }
        public double? TotalAmount { get; set; }
        public double? VatRate { get; set; }
        public double? TotalPayment { get; set; }
        public DateTime? InvoiceIssueDate { get; set; }
        public string RawText { get; set; } = string.Empty;
        public bool UsedAiFallback { get; set; }

        public List<string> ValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Serial))
                errors.Add("Cannot read invoice serial.");
            if (string.IsNullOrWhiteSpace(BuyerTaxCode))
                errors.Add("Cannot read buyer tax code.");
            if (!TotalAmount.HasValue)
                errors.Add("Cannot read total amount.");
            if (!VatRate.HasValue)
                errors.Add("Cannot read VAT rate.");
            if (!TotalPayment.HasValue)
                errors.Add("Cannot read total payment.");
            if (!InvoiceIssueDate.HasValue)
                errors.Add("Cannot read invoice issue date.");

            return errors;
        }
    }
}
