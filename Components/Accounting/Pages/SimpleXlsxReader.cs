using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NVOAMASIS.Components.Accounting.Pages
{
    /// <summary>
    /// Minimal .xlsx reader used by the voucher import screen.
    /// It reads worksheet values directly from the Open XML package and requires no Excel NuGet package.
    /// </summary>
    internal static class SimpleXlsxReader
    {
        private static readonly XNamespace SpreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace OfficeRelationshipNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelationshipNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static SimpleXlsxWorksheet ReadFirstWorksheet(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new InvalidDataException("File Excel rỗng.");

            using var stream = new MemoryStream(bytes, writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

            var workbookDocument = LoadXml(archive, "xl/workbook.xml");
            var relationshipDocument = LoadXml(archive, "xl/_rels/workbook.xml.rels");

            var firstSheet = workbookDocument
                .Descendants(SpreadsheetNs + "sheet")
                .FirstOrDefault()
                ?? throw new InvalidDataException("File Excel không có worksheet.");

            var sheetName = (string?)firstSheet.Attribute("name") ?? "Sheet1";
            var relationshipId = (string?)firstSheet.Attribute(OfficeRelationshipNs + "id")
                                 ?? throw new InvalidDataException("Không đọc được quan hệ worksheet trong file Excel.");

            var relationship = relationshipDocument
                .Descendants(PackageRelationshipNs + "Relationship")
                .FirstOrDefault(x => string.Equals((string?)x.Attribute("Id"), relationshipId, StringComparison.Ordinal));

            var target = (string?)relationship?.Attribute("Target")
                         ?? throw new InvalidDataException("Không tìm thấy worksheet được khai báo trong workbook.");

            var worksheetPath = NormalizeZipPath("xl", target);
            var sharedStrings = ReadSharedStrings(archive);
            var dateStyleIndexes = ReadDateStyleIndexes(archive);
            var uses1904DateSystem = ReadUses1904DateSystem(workbookDocument);

            var worksheetDocument = LoadXml(archive, worksheetPath);
            return ParseWorksheet(sheetName, worksheetDocument, sharedStrings, dateStyleIndexes, uses1904DateSystem);
        }

        private static SimpleXlsxWorksheet ParseWorksheet(
            string name,
            XDocument document,
            IReadOnlyList<string> sharedStrings,
            ISet<int> dateStyleIndexes,
            bool uses1904DateSystem)
        {
            var cells = new Dictionary<(int Row, int Column), SimpleXlsxCell>();
            var lastRow = 0;
            var lastColumn = 0;

            foreach (var cellElement in document.Descendants(SpreadsheetNs + "c"))
            {
                var reference = (string?)cellElement.Attribute("r");
                if (!TryParseCellReference(reference, out var row, out var column))
                    continue;

                var type = (string?)cellElement.Attribute("t") ?? string.Empty;
                var styleIndex = ParseInt((string?)cellElement.Attribute("s"));
                var rawValue = (string?)cellElement.Element(SpreadsheetNs + "v") ?? string.Empty;

                string text;
                double? numericValue = null;
                DateTime? dateValue = null;

                switch (type)
                {
                    case "s":
                        var sharedIndex = ParseInt(rawValue);
                        text = sharedIndex >= 0 && sharedIndex < sharedStrings.Count
                            ? sharedStrings[sharedIndex]
                            : string.Empty;
                        break;

                    case "inlineStr":
                        text = string.Concat(cellElement
                            .Descendants(SpreadsheetNs + "t")
                            .Select(x => x.Value));
                        break;

                    case "str":
                    case "d":
                        text = rawValue;
                        if (type == "d" && DateTime.TryParse(rawValue, CultureInfo.InvariantCulture,
                                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var isoDate))
                        {
                            dateValue = isoDate;
                        }
                        break;

                    case "b":
                        text = rawValue == "1" ? "TRUE" : "FALSE";
                        break;

                    default:
                        text = rawValue;
                        if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                        {
                            numericValue = number;
                            if (dateStyleIndexes.Contains(styleIndex))
                            {
                                try
                                {
                                    dateValue = DateTime.FromOADate(number + (uses1904DateSystem ? 1462d : 0d));
                                    text = dateValue.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                }
                                catch
                                {
                                    // Keep the raw numeric value when it is outside the valid OLE Automation date range.
                                }
                            }
                        }
                        break;
                }

                cells[(row, column)] = new SimpleXlsxCell(text ?? string.Empty, numericValue, dateValue);
                if (row > lastRow) lastRow = row;
                if (column > lastColumn) lastColumn = column;
            }

            return new SimpleXlsxWorksheet(name, cells, lastRow, lastColumn);
        }

        private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
        {
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null)
                return Array.Empty<string>();

            using var stream = entry.Open();
            var document = XDocument.Load(stream, LoadOptions.PreserveWhitespace);
            return document
                .Descendants(SpreadsheetNs + "si")
                .Select(item => string.Concat(item.Descendants(SpreadsheetNs + "t").Select(t => t.Value)))
                .ToList();
        }

        private static HashSet<int> ReadDateStyleIndexes(ZipArchive archive)
        {
            var result = new HashSet<int>();
            var entry = archive.GetEntry("xl/styles.xml");
            if (entry == null)
                return result;

            using var stream = entry.Open();
            var document = XDocument.Load(stream);

            var customFormats = document
                .Descendants(SpreadsheetNs + "numFmt")
                .Select(x => new
                {
                    Id = ParseInt((string?)x.Attribute("numFmtId")),
                    Code = (string?)x.Attribute("formatCode") ?? string.Empty
                })
                .Where(x => x.Id >= 0)
                .ToDictionary(x => x.Id, x => x.Code);

            var cellXfs = document.Descendants(SpreadsheetNs + "cellXfs").FirstOrDefault();
            if (cellXfs == null)
                return result;

            var styleIndex = 0;
            foreach (var xf in cellXfs.Elements(SpreadsheetNs + "xf"))
            {
                var numFmtId = ParseInt((string?)xf.Attribute("numFmtId"));
                if (IsDateNumberFormat(numFmtId, customFormats))
                    result.Add(styleIndex);
                styleIndex++;
            }

            return result;
        }

        private static bool IsDateNumberFormat(int numFmtId, IReadOnlyDictionary<int, string> customFormats)
        {
            // Built-in Excel date/time formats.
            if ((numFmtId >= 14 && numFmtId <= 22)
                || (numFmtId >= 27 && numFmtId <= 36)
                || (numFmtId >= 45 && numFmtId <= 47)
                || (numFmtId >= 50 && numFmtId <= 58))
            {
                return true;
            }

            if (!customFormats.TryGetValue(numFmtId, out var formatCode))
                return false;

            var cleaned = Regex.Replace(formatCode, @"\[[^\]]*\]|""[^""]*""|\\.", string.Empty)
                               .ToLowerInvariant();
            return cleaned.Contains('y')
                   || cleaned.Contains('d')
                   || (cleaned.Contains('m') && (cleaned.Contains('h') || cleaned.Contains('s')));
        }

        private static bool ReadUses1904DateSystem(XDocument workbookDocument)
        {
            var workbookProperties = workbookDocument.Descendants(SpreadsheetNs + "workbookPr").FirstOrDefault();
            var value = (string?)workbookProperties?.Attribute("date1904");
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static XDocument LoadXml(ZipArchive archive, string path)
        {
            var entry = archive.GetEntry(path)
                        ?? throw new InvalidDataException($"File Excel thiếu thành phần bắt buộc: {path}");
            using var stream = entry.Open();
            return XDocument.Load(stream, LoadOptions.PreserveWhitespace);
        }

        private static string NormalizeZipPath(string baseFolder, string target)
        {
            var raw = target.Replace('\\', '/');
            if (raw.StartsWith("/", StringComparison.Ordinal))
                raw = raw.TrimStart('/');
            else
                raw = baseFolder.TrimEnd('/') + "/" + raw;

            var parts = new List<string>();
            foreach (var part in raw.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (part == ".")
                    continue;
                if (part == "..")
                {
                    if (parts.Count > 0)
                        parts.RemoveAt(parts.Count - 1);
                    continue;
                }
                parts.Add(part);
            }
            return string.Join("/", parts);
        }

        private static bool TryParseCellReference(string? reference, out int row, out int column)
        {
            row = 0;
            column = 0;
            if (string.IsNullOrWhiteSpace(reference))
                return false;

            var index = 0;
            while (index < reference.Length && char.IsLetter(reference[index]))
            {
                column = column * 26 + (char.ToUpperInvariant(reference[index]) - 'A' + 1);
                index++;
            }

            if (column <= 0 || index >= reference.Length)
                return false;

            return int.TryParse(reference[index..], NumberStyles.None, CultureInfo.InvariantCulture, out row)
                   && row > 0;
        }

        private static int ParseInt(string? value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : -1;
    }

    internal sealed class SimpleXlsxWorksheet
    {
        private readonly IReadOnlyDictionary<(int Row, int Column), SimpleXlsxCell> _cells;

        public SimpleXlsxWorksheet(
            string name,
            IReadOnlyDictionary<(int Row, int Column), SimpleXlsxCell> cells,
            int lastRowNumber,
            int lastColumnNumber)
        {
            Name = name;
            _cells = cells;
            LastRowNumber = lastRowNumber;
            LastColumnNumber = lastColumnNumber;
        }

        public string Name { get; }
        public int LastRowNumber { get; }
        public int LastColumnNumber { get; }

        public SimpleXlsxCell Cell(int row, int column)
            => _cells.TryGetValue((row, column), out var value) ? value : SimpleXlsxCell.Empty;

        public SimpleXlsxRow Row(int row) => new(this, row);
    }

    internal readonly struct SimpleXlsxRow
    {
        private readonly SimpleXlsxWorksheet _worksheet;
        private readonly int _row;

        public SimpleXlsxRow(SimpleXlsxWorksheet worksheet, int row)
        {
            _worksheet = worksheet;
            _row = row;
        }

        public SimpleXlsxCell Cell(int column) => _worksheet.Cell(_row, column);
    }

    internal sealed class SimpleXlsxCell
    {
        public static readonly SimpleXlsxCell Empty = new(string.Empty, null, null);

        public SimpleXlsxCell(string text, double? numericValue, DateTime? dateValue)
        {
            Text = text;
            NumericValue = numericValue;
            DateValue = dateValue;
        }

        public string Text { get; }
        public double? NumericValue { get; }
        public DateTime? DateValue { get; }
    }
}
