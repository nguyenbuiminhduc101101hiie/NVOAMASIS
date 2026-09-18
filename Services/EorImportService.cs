using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using ExcelDataReader;
using OfficeOpenXml;

namespace NVOAMASIS.Services
{
    /// <summary>Một dòng chi phí sửa chữa container đã đọc được từ file EOR/HDS excel (chưa tách Labor/Material).</summary>
    public sealed class EorImportRow
    {
        public string? ContainerNo { get; set; }
        public string? Size { get; set; }
        public DateTime? DateOfEst { get; set; }
        public DateTime? DateInYard { get; set; }
        public string? ManufacturingDate { get; set; }
        public string? IT { get; set; }
        public string? ComCode { get; set; }
        public string? ComponentDetails { get; set; }
        public string? Loc { get; set; }
        public string? DamCode { get; set; }
        public string? RepCode { get; set; }
        public double? Lht { get; set; }
        public double? Wdt { get; set; }
        public double? R { get; set; }
        public double? Hours { get; set; }
        public double? LaborCost { get; set; }
        public double? MaterialCost { get; set; }
        public double? LaborRate { get; set; }
        public string? Billing { get; set; }
        public string? Owner { get; set; }
        public string? Location { get; set; }
        public string? EstNo { get; set; }
        public string? Grade { get; set; }
        public string? DamageDetail { get; set; }
        public string? Remark { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public sealed class EorParseResult
    {
        public List<EorImportRow> Rows { get; set; } = new();
        public int SkippedRowCount { get; set; }
    }

    /// <summary>Khai báo vị trí cột (1-based, kiểu Excel: A=1,B=2,...) cho từng loại file EOR/HDS.</summary>
    public sealed class EorColumnMap
    {
        public int DataStartRow { get; init; }
        public int? ContainerCol { get; init; }
        public int? SizeCol { get; init; }
        public int? ItCol { get; init; }
        public int? ComCodeCol { get; init; }
        public int? ComponentDetailsCol { get; init; }
        public int? DamageDetailCol { get; init; }
        public int? LocCol { get; init; }
        public int? DamCodeCol { get; init; }
        public int? RepCodeCol { get; init; }
        public int? LhtCol { get; init; }
        public int? WdtCol { get; init; }
        public int? DimensionCol { get; init; }
        public int? RCol { get; init; }
        public int? HoursCol { get; init; }
        public int? LaborCostCol { get; init; }
        public int? MaterialCostCol { get; init; }
        public int? EstNoCol { get; init; }
        public int? GradeCol { get; init; }
        public int? RemarkCol { get; init; }
        public int? DateOfEstCol { get; init; }
        public string? DateOfEstFormat { get; init; }
        public int? DateInYardCol { get; init; }
        public int? ManufacturingDateCol { get; init; }
        public int? BillingCol { get; init; }
        public int? OwnerCol { get; init; }
        public int? LocationCol { get; init; }
        public (int Row, int Col)? GlobalDateCell { get; init; }
        public (int Row, int Col)? CurrencyCell { get; init; }
        public (int Row, int Col)? LaborRateCell { get; init; }
        public string? FixedCurrency { get; init; }
        public bool RequiresXls { get; init; }
        /// <summary>Cột dùng để nhận biết dòng "total"/dòng trống (rỗng hết thì bỏ qua). Để trống sẽ mặc định dùng ComCode/Loc/DamCode.</summary>
        public int[]? SkipCheckCols { get; init; }
    }

    /// <summary>Danh sách loại file EOR/HDS được hỗ trợ import — dùng chung cho mọi màn hình có tính năng import.</summary>
    public static class EorImportDefinitions
    {
        public static readonly (string Key, string Label)[] FileTypeOptions =
        {
            ("EOR_ASR", "EOR-ASR"),
            ("EOR_NAM_DINH_VU", "EOR NAM ĐÌNH VŨ"),
            ("EOR_SAO_A", "EOR SAO Á"),
            ("EOR_SAO_A_VSSL", "EOR SAO Á_VSSL"),
            ("HDS_NAM_DINH_VU_VSSL", "HDS NAM ĐÌNH VŨ_VSSL"),
            ("EST_HDS_TAN_VU_VSSL", "EST HDS TÂN VŨ_VSSL"),
            ("HDS_GFT_VSSL", "HDS GFT_VSSL"),
        };

        public static readonly Dictionary<string, EorColumnMap> Maps = new()
        {
            // Format EOR-ASR gốc — header kết thúc row 7, data từ row 8. Currency tại I6, Labor Rate tại T6.
            ["EOR_ASR"] = new EorColumnMap
            {
                DataStartRow = 8,
                DateOfEstCol = 2,
                ContainerCol = 3,
                SizeCol = 4,
                DateInYardCol = 5,
                ManufacturingDateCol = 6,
                ItCol = 7,
                ComCodeCol = 8,
                ComponentDetailsCol = 9,
                LocCol = 10,
                DamCodeCol = 11,
                RepCodeCol = 12,
                LhtCol = 13,
                WdtCol = 14,
                RCol = 16,
                HoursCol = 17,
                LaborCostCol = 18,
                MaterialCostCol = 19,
                BillingCol = 21,
                OwnerCol = 22,
                LocationCol = 23,
                CurrencyCell = (6, 9),
                LaborRateCell = (6, 20),
                FixedCurrency = "USD",
                SkipCheckCols = new[] { 7, 8, 9 } // IT / Com Code / Components Details
            },
            // "EOR NAM ĐÌNH VŨ _ HPH.xlsx" — sheet ESTIMATES_INVOICES OF REPAIR, header row 4, data từ row 5.
            ["EOR_NAM_DINH_VU"] = new EorColumnMap
            {
                DataStartRow = 5,
                ContainerCol = 3,
                SizeCol = 4,
                ItCol = 5,
                ComCodeCol = 6,
                ComponentDetailsCol = 7,
                LocCol = 8,
                DamCodeCol = 9,
                RepCodeCol = 10,
                LhtCol = 11,
                WdtCol = 12,
                HoursCol = 14,
                LaborCostCol = 15,
                MaterialCostCol = 16,
                CurrencyCell = (3, 2),
                FixedCurrency = "USD"
            },
            // "EOR SAO Á _ HPH.xlsx" — sheet EOR, header row 7, data từ row 8.
            ["EOR_SAO_A"] = new EorColumnMap
            {
                DataStartRow = 8,
                ContainerCol = 2,
                SizeCol = 3,
                ComCodeCol = 4,
                DamageDetailCol = 5,
                DamCodeCol = 6,
                LhtCol = 8,
                WdtCol = 9,
                LocCol = 11,
                RepCodeCol = 12,
                HoursCol = 13,
                LaborCostCol = 14,
                MaterialCostCol = 15,
                GradeCol = 18,
                RemarkCol = 19,
                FixedCurrency = "USD"
            },
            // "EOR - HDS - SAO A.xlsx" — header row 12, data từ row 13, Estimate Date chung tại J7.
            ["EOR_SAO_A_VSSL"] = new EorColumnMap
            {
                DataStartRow = 13,
                ContainerCol = 2,
                SizeCol = 3,
                ComCodeCol = 4,
                DamageDetailCol = 5,
                LocCol = 6,
                DamCodeCol = 7,
                RepCodeCol = 8,
                LhtCol = 9,
                WdtCol = 10,
                HoursCol = 13,
                LaborCostCol = 14,
                MaterialCostCol = 15,
                GlobalDateCell = (7, 10),
                FixedCurrency = "USD"
            },
            // "HDS _ NAM ĐÌNH VŨ.xlsx" — sheet ESTIMATES_INVOICES OF REPAIR RE, header row 8, data từ row 9.
            ["HDS_NAM_DINH_VU_VSSL"] = new EorColumnMap
            {
                DataStartRow = 9,
                DateOfEstCol = 4,
                EstNoCol = 5,
                ContainerCol = 6,
                SizeCol = 8,
                ItCol = 10,
                ComCodeCol = 11,
                ComponentDetailsCol = 12,
                LocCol = 13,
                DamCodeCol = 14,
                RepCodeCol = 15,
                LhtCol = 16,
                WdtCol = 17,
                HoursCol = 20,
                LaborCostCol = 21,
                MaterialCostCol = 22,
                CurrencyCell = (7, 2),
                FixedCurrency = "USD"
            },
            // "est-HDS _TAN VU.xlsx" — sheet Estimate, header row 5, data từ row 6, Estimate Date chung tại H2.
            ["EST_HDS_TAN_VU_VSSL"] = new EorColumnMap
            {
                DataStartRow = 6,
                ContainerCol = 2,
                SizeCol = 3,
                ItCol = 5,
                ComCodeCol = 6,
                ComponentDetailsCol = 7,
                LocCol = 8,
                DamCodeCol = 9,
                RepCodeCol = 10,
                LhtCol = 11,
                WdtCol = 12,
                HoursCol = 14,
                LaborCostCol = 15,
                MaterialCostCol = 16,
                GlobalDateCell = (2, 8),
                FixedCurrency = "USD"
            },
            // "HDS EOR -GFT.xls" (.xls cũ, đọc bằng ExcelDataReader) — header row 3, data từ row 4.
            // Cột "Dimension" gộp Length x Width (vd "240x120cm"); "Estimate date" theo từng dòng, định dạng yyyy.MM.dd.
            ["HDS_GFT_VSSL"] = new EorColumnMap
            {
                DataStartRow = 4,
                ContainerCol = 2,
                SizeCol = 3,
                ComCodeCol = 4,
                DamageDetailCol = 5,
                DamCodeCol = 6,
                DimensionCol = 8,
                LocCol = 9,
                RepCodeCol = 10,
                HoursCol = 11,
                LaborCostCol = 12,
                MaterialCostCol = 13,
                DateOfEstCol = 16,
                DateOfEstFormat = "yyyy.MM.dd",
                FixedCurrency = "USD",
                RequiresXls = true
            },
        };
    }

    /// <summary>Đọc file EOR/HDS excel theo <see cref="EorColumnMap"/> thành danh sách <see cref="EorImportRow"/>.</summary>
    public static class EorExcelParser
    {
        private static bool _encodingProviderRegistered;

        public static EorParseResult Parse(Stream fileStream, EorColumnMap map)
        {
            Func<int, int, object?> getCell;
            int lastRow;
            IDisposable? gridResource;

            if (map.RequiresXls)
            {
                RegisterEncodingProviderOnce();
                var reader = ExcelReaderFactory.CreateReader(fileStream);
                gridResource = reader;
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
                });
                var dt = dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null;
                if (dt is null || dt.Rows.Count == 0)
                {
                    reader.Dispose();
                    throw new InvalidOperationException("File Excel không có dữ liệu.");
                }

                lastRow = dt.Rows.Count;
                getCell = (row, col) =>
                {
                    if (row < 1 || col < 1 || row > dt.Rows.Count || col > dt.Columns.Count)
                        return null;
                    var v = dt.Rows[row - 1][col - 1];
                    return v is DBNull ? null : v;
                };
            }
            else
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var package = new ExcelPackage(fileStream);
                gridResource = package;
                var ws = package.Workbook.Worksheets.FirstOrDefault();
                if (ws?.Dimension == null)
                {
                    package.Dispose();
                    throw new InvalidOperationException("File Excel không có dữ liệu.");
                }

                lastRow = ws.Dimension.End.Row;
                getCell = (row, col) => ws.Cells[row, col].Value;
            }

            using (gridResource)
            {
                var currency = map.CurrencyCell is { } curCell
                    ? ParseCurrencyFromCell(getCell(curCell.Row, curCell.Col))
                    : null;
                if (string.IsNullOrWhiteSpace(currency))
                    currency = map.FixedCurrency ?? "USD";

                double? laborRate = map.LaborRateCell is { } lrCell
                    ? ReadCellDouble(getCell, lrCell.Row, lrCell.Col)
                    : null;

                var globalDate = map.GlobalDateCell is { } dateCell
                    ? ReadCellDate(getCell, dateCell.Row, dateCell.Col)
                    : null;

                var skipCols = map.SkipCheckCols is { Length: > 0 }
                    ? map.SkipCheckCols
                    : new[] { map.ComCodeCol, map.LocCol, map.DamCodeCol }.Where(c => c.HasValue).Select(c => c!.Value).ToArray();

                string? carryContainer = null;
                string? carrySize = null;
                var carryDateOfEst = globalDate;
                DateTime? carryDateInYard = null;
                string? carryManufacturingDate = null;
                string? carryBilling = null;
                string? carryOwner = null;
                string? carryLocation = null;

                var result = new EorParseResult();

                for (var row = map.DataStartRow; row <= lastRow; row++)
                {
                    var isBlankRow = skipCols.Length > 0 &&
                        skipCols.All(c => string.IsNullOrWhiteSpace(ReadCellText(getCell, row, c)));
                    if (isBlankRow)
                    {
                        result.SkippedRowCount++;
                        continue;
                    }

                    var containerNo = (map.ContainerCol is int coc ? TrimOrNull(ReadCellText(getCell, row, coc)) : null) ?? carryContainer;
                    carryContainer = containerNo ?? carryContainer;

                    var size = (map.SizeCol is int sc ? TrimOrNull(ReadCellText(getCell, row, sc)) : null) ?? carrySize;
                    carrySize = size ?? carrySize;

                    var dateOfEst = globalDate;
                    if (map.DateOfEstCol is int dec)
                    {
                        var rowDate = !string.IsNullOrWhiteSpace(map.DateOfEstFormat)
                            ? ReadCellDateExact(getCell, row, dec, map.DateOfEstFormat!)
                            : ReadCellDate(getCell, row, dec);
                        dateOfEst = rowDate ?? carryDateOfEst;
                        carryDateOfEst = dateOfEst ?? carryDateOfEst;
                    }

                    DateTime? dateInYard = null;
                    if (map.DateInYardCol is int diyc)
                    {
                        dateInYard = ReadCellDate(getCell, row, diyc) ?? carryDateInYard;
                        carryDateInYard = dateInYard ?? carryDateInYard;
                    }

                    string? manufacturingDate = null;
                    if (map.ManufacturingDateCol is int mdc)
                    {
                        manufacturingDate = FormatManufacturingDate(getCell(row, mdc)) ?? carryManufacturingDate;
                        carryManufacturingDate = manufacturingDate ?? carryManufacturingDate;
                    }

                    string? billing = null;
                    if (map.BillingCol is int bc)
                    {
                        billing = TrimOrNull(ReadCellText(getCell, row, bc)) ?? carryBilling;
                        carryBilling = billing ?? carryBilling;
                    }

                    string? owner = null;
                    if (map.OwnerCol is int oc)
                    {
                        owner = TrimOrNull(ReadCellText(getCell, row, oc)) ?? carryOwner;
                        carryOwner = owner ?? carryOwner;
                    }

                    string? location = null;
                    if (map.LocationCol is int locc)
                    {
                        location = TrimOrNull(ReadCellText(getCell, row, locc)) ?? carryLocation;
                        carryLocation = location ?? carryLocation;
                    }

                    double? lht = map.LhtCol is int lhc ? ReadCellDouble(getCell, row, lhc) : null;
                    double? wdt = map.WdtCol is int wdc ? ReadCellDouble(getCell, row, wdc) : null;
                    if (map.DimensionCol is int dimc)
                    {
                        var (parsedLht, parsedWdt) = ParseDimension(ReadCellText(getCell, row, dimc));
                        lht ??= parsedLht;
                        wdt ??= parsedWdt;
                    }

                    var laborCost = map.LaborCostCol is int lcc ? ReadCellDouble(getCell, row, lcc) : null;
                    var materialCost = map.MaterialCostCol is int mcc ? ReadCellDouble(getCell, row, mcc) : null;
                    if ((!laborCost.HasValue || laborCost == 0) && (!materialCost.HasValue || materialCost == 0))
                    {
                        result.SkippedRowCount++;
                        continue;
                    }

                    result.Rows.Add(new EorImportRow
                    {
                        ContainerNo = containerNo,
                        Size = size,
                        DateOfEst = dateOfEst,
                        DateInYard = dateInYard,
                        ManufacturingDate = manufacturingDate,
                        IT = map.ItCol is int itc ? TrimOrNull(ReadCellText(getCell, row, itc)) : null,
                        ComCode = map.ComCodeCol is int cc ? TrimOrNull(ReadCellText(getCell, row, cc)) : null,
                        ComponentDetails = map.ComponentDetailsCol is int cdc ? TrimOrNull(ReadCellText(getCell, row, cdc)) : null,
                        Loc = map.LocCol is int lc ? TrimOrNull(ReadCellText(getCell, row, lc)) : null,
                        DamCode = map.DamCodeCol is int dc ? TrimOrNull(ReadCellText(getCell, row, dc)) : null,
                        RepCode = map.RepCodeCol is int rpc ? TrimOrNull(ReadCellText(getCell, row, rpc)) : null,
                        Lht = lht,
                        Wdt = wdt,
                        R = map.RCol is int rc ? ReadCellDouble(getCell, row, rc) : null,
                        Hours = map.HoursCol is int hc ? ReadCellDouble(getCell, row, hc) : null,
                        LaborCost = laborCost,
                        MaterialCost = materialCost,
                        LaborRate = laborRate,
                        Billing = billing,
                        Owner = owner,
                        Location = location,
                        EstNo = map.EstNoCol is int enc ? TrimOrNull(ReadCellText(getCell, row, enc)) : null,
                        Grade = map.GradeCol is int gc ? TrimOrNull(ReadCellText(getCell, row, gc)) : null,
                        DamageDetail = map.DamageDetailCol is int ddc ? TrimOrNull(ReadCellText(getCell, row, ddc)) : null,
                        Remark = map.RemarkCol is int rmc ? TrimOrNull(ReadCellText(getCell, row, rmc)) : null,
                        Currency = currency!
                    });
                }

                return result;
            }
        }

        private static void RegisterEncodingProviderOnce()
        {
            if (_encodingProviderRegistered)
                return;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _encodingProviderRegistered = true;
        }

        private static (double? Length, double? Width) ParseDimension(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (null, null);
            var match = Regex.Match(text, @"([\d.,]+)\s*[xX]\s*([\d.,]+)");
            if (!match.Success)
                return (null, null);
            var l = double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lv) ? lv : (double?)null;
            var w = double.TryParse(match.Groups[2].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var wv) ? wv : (double?)null;
            return (l, w);
        }

        private static string? TrimOrNull(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? ParseCurrencyFromCell(object? value)
        {
            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var idx = text.LastIndexOf(':');
            if (idx >= 0 && idx < text.Length - 1)
                return text[(idx + 1)..].Trim();

            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[^1].Trim() : text;
        }

        private static string? FormatManufacturingDate(object? value)
        {
            if (value is DateTime dt)
                return dt.ToString("MM/yyyy", CultureInfo.InvariantCulture);

            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (DateTime.TryParseExact(text, new[] { "MM/yyyy", "M/yyyy", "MM-yyyy", "M-yyyy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.ToString("MM/yyyy", CultureInfo.InvariantCulture);

            return text;
        }

        private static string? ReadCellText(Func<int, int, object?> get, int row, int col) =>
            Convert.ToString(get(row, col))?.Trim();

        private static double? ReadCellDouble(Func<int, int, object?> get, int row, int col)
        {
            var value = get(row, col);
            if (value == null) return null;
            if (value is double d) return d;
            if (value is decimal m) return (double)m;
            if (value is int i) return i;
            if (value is long l) return l;
            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                || double.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out parsed)
                ? parsed
                : null;
        }

        private static DateTime? ReadCellDate(Func<int, int, object?> get, int row, int col)
        {
            var value = get(row, col);
            if (value is DateTime dt) return dt.Date;
            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (DateTime.TryParseExact(text, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                return parsed.Date;
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var oa) && oa > 0)
                return DateTime.FromOADate(oa).Date;
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                ? parsed.Date
                : null;
        }

        private static DateTime? ReadCellDateExact(Func<int, int, object?> get, int row, int col, string format)
        {
            var value = get(row, col);
            if (value is DateTime dt) return dt.Date;
            var text = Convert.ToString(value)?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return null;
            return DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.Date
                : ReadCellDate(get, row, col);
        }
    }
}
