using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    /// <summary>Đọc XML hóa đơn điện tử chuẩn TCT (HDon/DLHDon/TTChung/NDHDon/TToan).</summary>
    public class InvoiceXmlImportService
    {
        private const RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        public async Task<InvoiceXmlParsedData> ParseAsync(Stream xmlStream, CancellationToken cancellationToken = default)
        {
            if (xmlStream == null)
                throw new ArgumentNullException(nameof(xmlStream));

            using var reader = new StreamReader(xmlStream);
            var content = await reader.ReadToEndAsync(cancellationToken);
            return Parse(content);
        }

        public static InvoiceXmlParsedData Parse(string xmlContent)
        {
            var parsed = new InvoiceXmlParsedData();
            var root = LoadInvoiceRoot(xmlContent);
            if (root == null)
                return parsed;

            parsed.IsInvoiceXml = true;

            var dlhDon = Child(root, "DLHDon");
            parsed.XmlDocId = dlhDon?.Attribute("Id")?.Value?.Trim();

            var ttChung = Child(dlhDon, "TTChung");
            parsed.Version = Text(ttChung, "PBan");
            parsed.InvoiceType = Text(ttChung, "THDon");
            parsed.TemplateNo = Text(ttChung, "KHMSHDon");
            parsed.Serial = Text(ttChung, "KHHDon");
            parsed.InvoiceNo = Text(ttChung, "SHDon");
            parsed.InvoiceDate = Date(Text(ttChung, "NLap"));
            parsed.Currency = Text(ttChung, "DVTTe") ?? "VND";
            parsed.ExchangeRate = Number(Text(ttChung, "TGia")) ?? 1;
            parsed.PaymentMethod = Text(ttChung, "HTTToan");

            // NBan/NMua cũng xuất hiện trong DSCKS (chữ ký) nên phải lấy đúng trong NDHDon
            var ndhDon = Child(dlhDon, "NDHDon");
            var nBan = Child(ndhDon, "NBan");
            parsed.SellerName = Text(nBan, "Ten");
            parsed.SellerTaxCode = Text(nBan, "MST");
            parsed.SellerAddress = Text(nBan, "DChi");

            var nMua = Child(ndhDon, "NMua");
            parsed.BuyerName = Text(nMua, "Ten") ?? Text(nMua, "HVTNMHang");
            parsed.BuyerTaxCode = Text(nMua, "MST");
            parsed.BuyerAddress = Text(nMua, "DChi");

            parsed.Lines = ParseLines(ndhDon);

            var tToan = Child(ndhDon, "TToan");
            parsed.TotalBeforeTax = Number(Text(tToan, "TgTCThue"));
            parsed.TotalTax = Number(Text(tToan, "TgTThue"));
            parsed.TotalDiscount = Number(Text(tToan, "TTCKTMai"));
            parsed.TotalPayment = Number(Text(tToan, "TgTTTBSo"));
            parsed.TotalInWords = Text(tToan, "TgTTTBChu");
            parsed.TaxRates = Children(Child(tToan, "THTTLTSuat"), "LTSuat")
                .Select(x => Text(x, "TSuat"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
            if (parsed.TaxRates.Count == 0)
                parsed.TaxRates = parsed.Lines.Where(x => !x.IsNote && !string.IsNullOrWhiteSpace(x.TaxRate)).Select(x => x.TaxRate!).Distinct().ToList();

            var notes = parsed.Lines.Where(x => x.IsNote && !string.IsNullOrWhiteSpace(x.Name)).Select(x => x.Name!.Trim()).ToList();
            if (notes.Count > 0)
            {
                parsed.Note = string.Join(Environment.NewLine, notes);
                parsed.VesselVoyage = LabelValue(parsed.Note, "VESSEL/VOYAGE");
                parsed.Pol = LabelValue(parsed.Note, "POL");
                parsed.Pod = LabelValue(parsed.Note, "POD");
                parsed.BillNo = LabelValue(parsed.Note, "BILL");
            }

            return parsed;
        }

        /// <summary>Đọc lại chi tiết dòng hàng từ XML gốc đã lưu (cột xmlcontent).</summary>
        public static List<InvoiceXmlLine> ParseLines(string? xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
                return new List<InvoiceXmlLine>();

            var root = LoadInvoiceRoot(xmlContent);
            return ParseLines(Child(Child(root, "DLHDon"), "NDHDon"));
        }

        /// <summary>Chuẩn hóa MST để so sánh: bỏ khoảng trắng, dấu chấm; giữ dấu '-' của MST chi nhánh.</summary>
        public static string NormalizeTaxCode(string? taxCode)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
                return string.Empty;
            return Regex.Replace(taxCode, @"[\s\.]+", string.Empty).Trim().ToUpperInvariant();
        }

        private static List<InvoiceXmlLine> ParseLines(XElement? ndhDon)
        {
            return Children(Child(ndhDon, "DSHHDVu"), "HHDVu")
                .Select(x => new InvoiceXmlLine
                {
                    Nature = (int?)Number(Text(x, "TChat")),
                    No = (int?)Number(Text(x, "STT")),
                    Name = Text(x, "THHDVu"),
                    Unit = Text(x, "DVTinh"),
                    Quantity = Number(Text(x, "SLuong")),
                    UnitPrice = Number(Text(x, "DGia")),
                    DiscountRate = Number(Text(x, "TLCKhau")),
                    DiscountAmount = Number(Text(x, "STCKhau")),
                    Amount = Number(Text(x, "ThTien")),
                    TaxRate = Text(x, "TSuat")
                })
                .ToList();
        }

        private static XElement? LoadInvoiceRoot(string xmlContent)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent.TrimStart('﻿'), LoadOptions.None);
                // Có thể là HDon trực tiếp hoặc được bọc trong thông điệp (TDiep/DLieu)
                return doc.Root == null
                    ? null
                    : doc.Root.DescendantsAndSelf().FirstOrDefault(x => x.Name.LocalName == "HDon");
            }
            catch (XmlException)
            {
                return null;
            }
        }

        private static string? LabelValue(string text, string label)
        {
            // "... (LABEL): value - NEXT LABEL (XXX): ..." → value
            var m = Regex.Match(
                text,
                @"\(\s*" + Regex.Escape(label) + @"\s*\)\s*:\s*(.+?)(?=\s+-\s+[^:\r\n]*\([^()]+\)\s*:|[\r\n]|$)",
                RegexFlags);
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static XElement? Child(XElement? parent, string localName) =>
            parent?.Elements().FirstOrDefault(x => x.Name.LocalName == localName);

        private static IEnumerable<XElement> Children(XElement? parent, string localName) =>
            parent?.Elements().Where(x => x.Name.LocalName == localName) ?? Enumerable.Empty<XElement>();

        private static string? Text(XElement? parent, string localName)
        {
            var value = Child(parent, localName)?.Value?.Trim();
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private static double? Number(string? value) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

        private static DateTime? Date(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            string[] formats = { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "dd/MM/yyyy" };
            return DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d
                : DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : null;
        }
    }
}
