namespace NVOAMASIS.Models
{
    /// <summary>Dữ liệu đọc từ XML hóa đơn điện tử chuẩn TCT (root HDon, PBan 2.x).</summary>
    public class InvoiceXmlParsedData
    {
        public bool IsInvoiceXml { get; set; }
        public string? XmlDocId { get; set; }
        public string? Version { get; set; }
        public string? InvoiceType { get; set; }
        public string? TemplateNo { get; set; }
        public string? Serial { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? Currency { get; set; }
        public double? ExchangeRate { get; set; }
        public string? PaymentMethod { get; set; }

        public string? SellerName { get; set; }
        public string? SellerTaxCode { get; set; }
        public string? SellerAddress { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerTaxCode { get; set; }
        public string? BuyerAddress { get; set; }

        public double? TotalBeforeTax { get; set; }
        public double? TotalTax { get; set; }
        public double? TotalDiscount { get; set; }
        public double? TotalPayment { get; set; }
        public string? TotalInWords { get; set; }
        public List<string> TaxRates { get; set; } = new();

        public string? Note { get; set; }
        public string? BillNo { get; set; }
        public string? VesselVoyage { get; set; }
        public string? Pol { get; set; }
        public string? Pod { get; set; }

        public List<InvoiceXmlLine> Lines { get; set; } = new();

        /// <summary>% VAT: 1 thuế suất → giá trị đó; nhiều thuế suất → tỉ lệ thuế / trước thuế.</summary>
        public double? VatRate
        {
            get
            {
                var rates = TaxRates.Select(InvoiceXmlLine.ParseTaxRate).Where(x => x.HasValue).Distinct().ToList();
                if (rates.Count == 1)
                    return rates[0];
                if (TotalBeforeTax.HasValue && TotalBeforeTax.Value != 0 && TotalTax.HasValue)
                    return Math.Round(TotalTax.Value / TotalBeforeTax.Value * 100, 2);
                return rates.Count == 0 && TotalTax == 0 ? 0 : null;
            }
        }

        public List<string> ValidationErrors()
        {
            var errors = new List<string>();

            if (!IsInvoiceXml)
            {
                errors.Add("The file is not an e-invoice XML (HDon).");
                return errors;
            }
            if (string.IsNullOrWhiteSpace(Serial))
                errors.Add("Cannot read invoice serial.");
            if (string.IsNullOrWhiteSpace(InvoiceNo))
                errors.Add("Cannot read invoice number.");
            if (!InvoiceDate.HasValue)
                errors.Add("Cannot read invoice issue date.");
            if (string.IsNullOrWhiteSpace(SellerTaxCode))
                errors.Add("Cannot read seller tax code.");
            if (string.IsNullOrWhiteSpace(BuyerTaxCode))
                errors.Add("Cannot read buyer tax code.");
            if (!TotalPayment.HasValue)
                errors.Add("Cannot read total payment.");

            return errors;
        }
    }

    public class InvoiceXmlLine
    {
        /// <summary>TChat: 1 hàng hóa/dịch vụ, 2 khuyến mại, 3 chiết khấu, 4 ghi chú/diễn giải.</summary>
        public int? Nature { get; set; }
        public int? No { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public double? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? DiscountRate { get; set; }
        public double? DiscountAmount { get; set; }
        public double? Amount { get; set; }
        public string? TaxRate { get; set; }

        public bool IsNote => Nature == 4;

        public static double? ParseTaxRate(string? taxRate)
        {
            if (string.IsNullOrWhiteSpace(taxRate))
                return null;
            var t = taxRate.Trim().ToUpperInvariant();
            if (t is "KCT" or "KKKNT")
                return 0;
            var m = System.Text.RegularExpressions.Regex.Match(t, @"\d+(?:\.\d+)?");
            return m.Success ? double.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture) : null;
        }
    }
}

namespace NVOAMASIS.Models
{
    /// <summary>Lô hàng (HBL hoặc MBL) tìm theo số vận đơn để gắn hóa đơn đầu vào.</summary>
    public record ShipmentBillRef(Guid HblId, Guid MblId, string Display);
}
