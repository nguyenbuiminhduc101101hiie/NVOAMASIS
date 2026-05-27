using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using NVOAMASIS.Models;
using UglyToad.PdfPig;

namespace NVOAMASIS.Services
{
    public class InvoicePdfImportService(ChatGPTService chatGPTService, ILogger<InvoicePdfImportService> logger)
    {
        private static readonly RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        public async Task<InvoicePdfParsedData> ParseAsync(Stream pdfStream, CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            await using var memory = new MemoryStream();
            await pdfStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            var rawText = ExtractRawText(memory);
            var parsed = ParseWithRegex(rawText);
            parsed.RawText = rawText;

            if (parsed.ValidationErrors().Count > 0)
            {
                var aiParsed = await ParseWithAiFallbackAsync(rawText);
                if (aiParsed != null)
                {
                    MergeMissingFields(parsed, aiParsed);
                    parsed.UsedAiFallback = true;
                }
            }

            return parsed;
        }

        private static string ExtractRawText(Stream pdfStream)
        {
            pdfStream.Position = 0;
            using var document = PdfDocument.Open(pdfStream);

            var sb = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return sb.ToString();
        }

        private InvoicePdfParsedData ParseWithRegex(string rawText)
        {
            var parsed = new InvoicePdfParsedData();

            var buyerBlock = ExtractBuyerBlock(rawText);
            parsed.Serial = ParseSerial(rawText);

            parsed.BuyerName =
                MatchValue(
                    buyerBlock,
                    @"(?:Tên\s*(?:đơn\s*vị|công\s*ty)(?:\s*\(\s*Company(?:\s*name)?\s*\))?|Company(?:\s*name)?|\(\s*Company(?:\s*name)?\s*\))\s*:\s*(.+?)(?=\s*(?:Mã\s*số\s*thuế|MST|\(\s*Tax\s*code\s*\)|Tax\s*code|Taxcode|Địa\s*chỉ|\(\s*Address\s*\)|Hình\s*thức\s*thanh\s*toán|\(\s*Payment\s*method\s*\)|$))");

            parsed.BuyerAddress =
                MatchValue(
                    buyerBlock,
                    @"(?:Địa\s*chỉ|\(\s*Address\s*\))\s*:\s*(.+?)(?=\s*(?:Hình\s*thức\s*thanh\s*toán|\(\s*Payment\s*method\s*\)|$))");

            parsed.BuyerTaxCode = ParseBuyerTaxCode(rawText, buyerBlock);

            // Support both invoice templates:
            // - New: Sub total + Total amount
            // - Legacy: Total amount + Total payment
            var subTotal = ParseNumericLabel(rawText, "Sub total", "Cộng tiền hàng");
            var totalAmount = ParseNumericLabel(rawText, "Total amount", "Tổng cộng tiền thanh toán");
            var totalPayment = ParseNumericLabel(rawText, "Total payment");

            if (subTotal.HasValue)
            {
                parsed.TotalAmount = subTotal;
                parsed.TotalPayment = totalAmount ?? totalPayment;
            }
            else
            {
                parsed.TotalAmount = totalAmount;
                parsed.TotalPayment = totalPayment ?? totalAmount;
            }

            parsed.VatRate = ParseNumericLabel(rawText, "VAT rate", "Thuế suất GTGT");
            parsed.InvoiceIssueDate = ParseIssueDate(rawText);

            parsed.BuyerName = CleanupExtractedValue(parsed.BuyerName);
            parsed.BuyerAddress = CleanupExtractedValue(parsed.BuyerAddress);

            return parsed;
        }

        private static string? ParseSerial(string rawText)
        {
            var match = Regex.Match(
                rawText,
                @"\(\s*Serial\s*\)\s*:\s*([A-Za-z0-9\-\.\/]+?)(?=\s*(?:(?:S(?:ố|o)\s*\()|(?:\(\s*No\.?\s*\))|(?:No\.?)|$|\r|\n))",
                RegexFlags);

            if (!match.Success || match.Groups.Count < 2)
                return null;

            var value = CleanupExtractedValue(match.Groups[1].Value);
            value = Regex.Replace(value ?? string.Empty, @"[^A-Za-z0-9\-\.\/]", string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value;
        }

        private static string? CleanupExtractedValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        private static string ExtractBuyerBlock(string rawText)
        {
            var match = Regex.Match(
                rawText,
                @"Họ\s*tên\s*người\s*mua\s*hàng.*?(?:Hình\s*thức\s*thanh\s*toán|\(\s*Payment\s*method\s*\))",
                RegexFlags);

            return match.Success ? match.Value : rawText;
        }

        private static string? ParseBuyerTaxCode(string rawText, string buyerBlock)
        {
            var fromBuyerBlock =
                MatchValue(
                    buyerBlock,
                    @"\(\s*Tax\s*code\s*\)\s*:\s*([0-9A-Za-z\-\.\s]+?)(?=\s*(?:Phương\s*thức|Payment\s*method|STT|\r|\n|$))")
                ?? MatchValue(
                    buyerBlock,
                    @"Mã\s*số\s*thuế(?:\s*\(\s*Tax\s*code\s*\))?\s*:\s*([0-9A-Za-z\-\.\s]+?)(?=\s*(?:Phương\s*thức|Payment\s*method|STT|\r|\n|$))");

            if (!string.IsNullOrWhiteSpace(fromBuyerBlock))
                return NormalizeTaxCode(fromBuyerBlock);

            // Fallback: nếu không xác định được block buyer, ưu tiên lần xuất hiện thứ hai (thường là buyer tax code)
            var matches = Regex.Matches(
                rawText,
                @"\(\s*Tax\s*code\s*\)\s*:\s*([0-9A-Za-z\-\.\s]+?)(?=\s*(?:Phương\s*thức|Payment\s*method|STT|\r|\n|$))",
                RegexFlags);
            if (matches.Count > 1)
                return NormalizeTaxCode(matches[1].Groups[1].Value);
            if (matches.Count == 1)
                return NormalizeTaxCode(matches[0].Groups[1].Value);

            return null;
        }

        private static double? ParseNumericLabel(string text, params string[] labelCandidates)
        {
            if (string.IsNullOrWhiteSpace(text) || labelCandidates == null || labelCandidates.Length == 0)
                return null;

            foreach (var label in labelCandidates.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                var byEnglishInParentheses = $@"\(\s*{Regex.Escape(label)}\s*\)\s*:\s*([0-9\.,\s]+%?)";
                var value = MatchValue(text, byEnglishInParentheses);
                var parsed = ParseFlexibleNumber(value);
                if (parsed.HasValue)
                    return parsed;

                var byPlainLabel = $@"{Regex.Escape(label)}\s*:\s*([0-9\.,\s]+%?)";
                value = MatchValue(text, byPlainLabel);
                parsed = ParseFlexibleNumber(value);
                if (parsed.HasValue)
                    return parsed;
            }

            return null;
        }

        private static DateTime? ParseIssueDate(string rawText)
        {
            var viMatch = Regex.Match(
                rawText,
                @"Ng(?:à|a)y\s*\(?date\)?\s*(\d{1,2})\s*th(?:á|a)ng\s*\(?month\)?\s*(\d{1,2})\s*n(?:ă|a)m\s*\(?year\)?\s*(\d{4})",
                RegexFlags);
            if (viMatch.Success)
            {
                return BuildDate(viMatch.Groups[1].Value, viMatch.Groups[2].Value, viMatch.Groups[3].Value);
            }

            var enMatch = Regex.Match(
                rawText,
                @"\(\s*date\s*\)\s*(\d{1,2})\s*\(\s*month\s*\)\s*(\d{1,2})\s*\(\s*year\s*\)\s*(\d{4})",
                RegexFlags);
            if (enMatch.Success)
            {
                return BuildDate(enMatch.Groups[1].Value, enMatch.Groups[2].Value, enMatch.Groups[3].Value);
            }

            return null;
        }

        private static DateTime? BuildDate(string dayText, string monthText, string yearText)
        {
            if (!int.TryParse(dayText, out var day))
                return null;
            if (!int.TryParse(monthText, out var month))
                return null;
            if (!int.TryParse(yearText, out var year))
                return null;

            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        private async Task<InvoicePdfParsedData?> ParseWithAiFallbackAsync(string rawText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawText))
                    return null;

                var truncatedRawText = rawText.Length > 15000 ? rawText[..15000] : rawText;
                var prompt = """
                    Bạn là bộ trích xuất dữ liệu hóa đơn.
                    Chỉ trả về duy nhất một JSON object, không markdown, không giải thích.
                    Dùng schema:
                    {
                      "serial": "string|null",
                      "buyerTaxCode": "string|null",
                      "buyerName": "string|null",
                      "buyerAddress": "string|null",
                      "totalAmount": "number|null",
                      "vatRate": "number|null",
                      "totalPayment": "number|null",
                      "invoiceIssueDate": "yyyy-MM-dd|null"
                    }
                    Nếu không có trường thì để null.
                    """;

                var aiResponse = await chatGPTService.SendMessageAsync(
                    new List<ChatMessage>(),
                    $"{prompt}\n\n=== PDF TEXT START ===\n{truncatedRawText}\n=== PDF TEXT END ===");

                if (string.IsNullOrWhiteSpace(aiResponse))
                    return null;

                var json = ExtractFirstJsonObject(aiResponse);
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var parsed = new InvoicePdfParsedData
                {
                    Serial = GetString(root, "serial"),
                    BuyerTaxCode = NormalizeTaxCode(GetString(root, "buyerTaxCode")),
                    BuyerName = GetString(root, "buyerName"),
                    BuyerAddress = GetString(root, "buyerAddress"),
                    TotalAmount = GetDouble(root, "totalAmount"),
                    VatRate = GetDouble(root, "vatRate"),
                    TotalPayment = GetDouble(root, "totalPayment"),
                    InvoiceIssueDate = ParseDateFlexible(GetString(root, "invoiceIssueDate"))
                };

                return parsed;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "AI fallback parse invoice PDF failed.");
                return null;
            }
        }

        private static string? MatchValue(string text, string pattern)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var match = Regex.Match(text, pattern, RegexFlags);
            if (!match.Success || match.Groups.Count < 2)
                return null;

            return match.Groups[1].Value.Trim();
        }

        private static double? ParseFlexibleNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var cleaned = Regex.Replace(input, @"[^\d,\.\-]", string.Empty);
            if (string.IsNullOrWhiteSpace(cleaned))
                return null;

            var lastComma = cleaned.LastIndexOf(',');
            var lastDot = cleaned.LastIndexOf('.');

            if (lastComma >= 0 && lastDot >= 0)
            {
                if (lastComma > lastDot)
                {
                    // 1.234,56
                    cleaned = cleaned.Replace(".", string.Empty);
                    cleaned = cleaned.Replace(",", ".");
                }
                else
                {
                    // 1,234.56
                    cleaned = cleaned.Replace(",", string.Empty);
                }
            }
            else if (lastComma >= 0)
            {
                // hoặc là nghìn hoặc là decimal
                cleaned = IsLikelyDecimal(cleaned, ',')
                    ? cleaned.Replace(",", ".")
                    : cleaned.Replace(",", string.Empty);
            }
            else if (lastDot >= 0)
            {
                cleaned = IsLikelyDecimal(cleaned, '.')
                    ? cleaned
                    : cleaned.Replace(".", string.Empty);
            }

            return double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
        }

        private static bool IsLikelyDecimal(string value, char separator)
        {
            var idx = value.LastIndexOf(separator);
            if (idx < 0 || idx == value.Length - 1)
                return false;

            var right = value[(idx + 1)..];
            return right.Length <= 2;
        }

        private static string? NormalizeTaxCode(string? taxCode)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
                return null;

            var normalized = Regex.Replace(taxCode.Trim(), @"\s+", string.Empty);
            normalized = normalized.Replace(".", string.Empty);

            // Common VN tax-code shape: 10 digits or 10 digits + '-' + 3 digits.
            // Keep this strict extraction to avoid OCR bleed (e.g. trailing "PH" from "Phương thức...").
            var strict = Regex.Match(normalized, @"\d{10}(?:-\d{3})?");
            if (strict.Success)
                return strict.Value;

            var digitsAndDash = Regex.Replace(normalized, @"[^0-9\-]", string.Empty).Trim('-');
            return string.IsNullOrWhiteSpace(digitsAndDash) ? null : digitsAndDash;
        }

        private static void MergeMissingFields(InvoicePdfParsedData target, InvoicePdfParsedData source)
        {
            target.Serial ??= source.Serial;
            target.BuyerTaxCode ??= source.BuyerTaxCode;
            target.BuyerName ??= source.BuyerName;
            target.BuyerAddress ??= source.BuyerAddress;
            target.TotalAmount ??= source.TotalAmount;
            target.VatRate ??= source.VatRate;
            target.TotalPayment ??= source.TotalPayment;
            target.InvoiceIssueDate ??= source.InvoiceIssueDate;
        }

        private static string? ExtractFirstJsonObject(string text)
        {
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');
            if (start < 0 || end <= start)
                return null;

            return text[start..(end + 1)];
        }

        private static string? GetString(JsonElement root, string property)
        {
            if (!root.TryGetProperty(property, out var value))
                return null;
            if (value.ValueKind == JsonValueKind.Null)
                return null;
            if (value.ValueKind == JsonValueKind.String)
                return value.GetString()?.Trim();

            return value.ToString();
        }

        private static double? GetDouble(JsonElement root, string property)
        {
            if (!root.TryGetProperty(property, out var value))
                return null;
            if (value.ValueKind == JsonValueKind.Null)
                return null;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d))
                return d;

            return ParseFlexibleNumber(value.ToString());
        }

        private static DateTime? ParseDateFlexible(string? dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            var formats = new[]
            {
                "yyyy-MM-dd",
                "dd/MM/yyyy",
                "d/M/yyyy",
                "yyyy/M/d",
                "yyyy-MM-d"
            };

            if (DateTime.TryParseExact(dateText, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
                return exactDate;
            if (DateTime.TryParse(dateText, out var freeDate))
                return freeDate;

            return null;
        }
    }
}
