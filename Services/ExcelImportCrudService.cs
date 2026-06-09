using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public enum ExcelCrudTab
{
    HDS = 0,
    AG = 1,
    VSS = 2,
    AMS = 3
}

public record ExcelImportCrudResult(int Inserted, string? ErrorMessage);

public class ExcelImportCrudService(AppDbContext context)
{
    private static readonly HashSet<string> AllowedVssSheetNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "IN-OUT_YARD__Imp",
        "IN-OUT_YARD__Exp"
    };
    private static readonly HashSet<string> AllowedAmsSheetNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "IN-OUT_YARD_1",
        "Current_In_Yard2"
    }; 

    static ExcelImportCrudService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task<List<M_StockGateOut_HDS_08042026>> GetHdsAsync(CancellationToken ct = default) =>
        context.StockGateOut_HDS_08042026.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_Arrived>> GetAgArrivedAsync(CancellationToken ct = default) =>
        context.AG_8_3_Arrived.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_Exited>> GetAgExitedAsync(CancellationToken ct = default) =>
        context.AG_8_3_Exited.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_Unstuffed>> GetAgUnstuffedAsync(CancellationToken ct = default) =>
        context.AG_8_3_Unstuffed.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_Stuffed>> GetAgStuffedAsync(CancellationToken ct = default) =>
        context.AG_8_3_Stuffed.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_VSS_IN_OUT_YARD_Imp>> GetVssImpAsync(CancellationToken ct = default) =>
        context.YardMovement_VSS_26040808_Imp.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_VSS_IN_OUT_YARD_Exp>> GetVssExpAsync(CancellationToken ct = default) =>
        context.YardMovement_VSS_26040808_Exp.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_8_3_IN_OUT_YARD_1>> GetAmsImpAsync(CancellationToken ct = default) =>
        context.YardMovement_AMS_26040816_Imp.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_YardMovement_AMS_26040816_Current_In_Yard2>> GetAmsExpAsync(CancellationToken ct = default) =>
        context.YardMovement_AMS_26040816_Current_In_Yard2.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public async Task<(bool Ok, string? Error)> UpsertAsync<T>(T entity, CancellationToken ct = default)
        where T : class, IExcelImportEntity
    {
        try
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            var dbSet = context.Set<T>();
            var exists = await dbSet.AsNoTracking().AnyAsync(x => x.Id == entity.Id, ct);
            if (exists)
                dbSet.Update(entity);
            else
                await dbSet.AddAsync(entity, ct);

            await context.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync<T>(Guid id, CancellationToken ct = default)
        where T : class, IExcelImportEntity
    {
        try
        {
            var dbSet = context.Set<T>();
            var entity = await dbSet.FindAsync(new object[] { id }, ct);
            if (entity == null)
                return (false, "Không tìm thấy bản ghi.");

            dbSet.Remove(entity);
            await context.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<ExcelImportCrudResult> ImportAsync(ExcelCrudTab tab, Stream stream, string? importUser = null, CancellationToken ct = default, string? amsLineFilter = null)
    {
        try
        {
            DataSet ds;
            try
            {
                using var reader = ExcelReaderFactory.CreateReader(stream);
                ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
                });
            }
            catch (Exception) when (tab == ExcelCrudTab.AMS)
            {
                if (stream.CanSeek)
                    stream.Position = 0;
                return await ImportAmsXmlSpreadsheetFallbackAsync(stream, importUser, amsLineFilter, ct);
            }

            return tab switch
            {
                ExcelCrudTab.HDS => await ImportHdsAsync(ds, importUser, ct),
                ExcelCrudTab.AG => await ImportAgAsync(ds, importUser, ct),
                ExcelCrudTab.VSS => await ImportVssAsync(ds, importUser, ct),
                ExcelCrudTab.AMS => await ImportAmsAsync(ds, importUser, amsLineFilter, ct),
                _ => new ExcelImportCrudResult(0, "Tab không hợp lệ.")
            };
        }
        catch (Exception ex)
        {
            return new ExcelImportCrudResult(0, ex.Message);
        }
    }

    private static bool MatchesAmsLineFilter(string? line, string? amsLineFilter)
    {
        if (string.IsNullOrWhiteSpace(amsLineFilter))
            return true;
        return !string.IsNullOrWhiteSpace(line)
            && string.Equals(line.Trim(), amsLineFilter.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<ExcelImportCrudResult> ImportAmsXmlSpreadsheetFallbackAsync(Stream stream, string? importUser, string? amsLineFilter, CancellationToken ct)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        ct.ThrowIfCancellationRequested();

        var incomingImp = new List<M_8_3_IN_OUT_YARD_1>();
        var incomingExp = new List<M_YardMovement_AMS_26040816_Current_In_Yard2>();
        var importedAt = DateTime.UtcNow;

        var worksheetRegex = new Regex("<Worksheet\\b[^>]*ss:Name=\"(?<name>[^\"]+)\"[^>]*>(?<body>.*?)</Worksheet\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var rowRegex = new Regex("<Row\\b[^>]*>(?<row>.*?)</Row\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var cellRegex = new Regex("<Cell(?<attrs>[^>]*)>(?<cell>.*?)</Cell\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var dataRegex = new Regex("<Data\\b[^>]*>(?<data>.*?)</Data>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var indexRegex = new Regex("ss:Index\\s*=\\s*\"(?<idx>\\d+)\"", RegexOptions.IgnoreCase);

        foreach (Match wsMatch in worksheetRegex.Matches(content))
        {
            ct.ThrowIfCancellationRequested();
            var sheetName = NormalizeAmsSheetName(wsMatch.Groups["name"].Value);
            if (!IsAllowedAmsSheetName(sheetName))
                continue;

            var body = wsMatch.Groups["body"].Value;
            var rowMatches = rowRegex.Matches(body);
            if (rowMatches.Count <= 1)
                continue;

            var headers = ParseXmlSpreadsheetRow(rowMatches[0].Groups["row"].Value, cellRegex, dataRegex, indexRegex)
                .Select(x => NormalizeHeader(x))
                .ToList();

            for (var i = 1; i < rowMatches.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var values = ParseXmlSpreadsheetRow(rowMatches[i].Groups["row"].Value, cellRegex, dataRegex, indexRegex);

                string? GetValue(params string[] aliases)
                {
                    foreach (var key in aliases)
                    {
                        var idx = headers.FindIndex(h => string.Equals(h, NormalizeHeader(key), StringComparison.OrdinalIgnoreCase));
                        if (idx < 0 || idx >= values.Count)
                            continue;
                        var raw = values[idx]?.Trim();
                        if (!string.IsNullOrWhiteSpace(raw))
                            return raw;
                    }
                    return null;
                }

                // Imp/Exp sheets may have different column layouts.
                var soCont = IsAmsImpSheet(sheetName)
                    ? GetValue("SOCONT")
                    : GetValue("ITEM_NO", "SOCONT", "CONTAINER", "CNTRNO");
                if (string.IsNullOrWhiteSpace(soCont))
                    continue;

                var lineValue = GetValue("LINE");
                if (!MatchesAmsLineFilter(lineValue, amsLineFilter))
                    continue;

                if (IsAmsImpSheet(sheetName))
                {
                    incomingImp.Add(new M_8_3_IN_OUT_YARD_1
                    {
                        Id = Guid.NewGuid(),
                        METHOD = GetValue("METHOD"),
                        EXEC_TS = ParseDateValue(GetValue("EXEC_TS")),
                        LINE = lineValue,
                        ITEM_KEY = GetValue("ITEM_KEY"),
                        SOCONT = soCont,
                        KICHCO = GetValue("KICHCO"),
                        TRANGTHAI = GetValue("TRANGTHAI"),
                        TRONGLUONG = ParseDecimalValue(GetValue("TRONGLUONG")),
                        TRONGLUONG_VGM = ParseDecimalValue(GetValue("TRONGLUONG_VGM")),
                        BL_NO = GetValue("BL_NO"),
                        BOOK_NO = GetValue("BOOK_NO"),
                        RELEASE_NO = GetValue("RELEASE_NO"),
                        HUONG = GetValue("HUONG"),
                        HUONG1 = GetValue("HUONG1"),
                        CANGCT = GetValue("CANGCT"),
                        CANGDEN = GetValue("CANGDEN"),
                        GIAO = GetValue("GIAO"),
                        NHAN = GetValue("NHAN"),
                        DGS_CLASS = GetValue("DGS_CLASS"),
                        GHICHU = GetValue("GHICHU"),
                        ENTRY_VOY_NO = GetValue("ENTRY_VOY_NO"),
                        ENTRY_VES_NAME = GetValue("ENTRY_VES_NAME"),
                        EXIT_VOY_NO = GetValue("EXIT_VOY_NO"),
                        EXIT_VES_NAME = GetValue("EXIT_VES_NAME"),
                        ENTRY_TRUCK_ID = GetValue("ENTRY_TRUCK_ID"),
                        EXIT_TRUCK_ID = GetValue("EXIT_TRUCK_ID"),
                        SOSEAL = GetValue("SOSEAL"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else
                {
                    incomingExp.Add(new M_YardMovement_AMS_26040816_Current_In_Yard2
                    {
                        Id = Guid.NewGuid(),
                        AGENT = GetValue("AGENT"),
                        LINE = lineValue,
                        ITEM_KEY = GetValue("ITEM_KEY"),
                        ITEM_NO = soCont,
                        ISO = GetValue("ISO"),
                        FEL = GetValue("FEL"),
                        TEMP = ParseDecimalValue(GetValue("TEMP")),
                        WEIGHT = ParseDecimalValue(GetValue("WEIGHT")),
                        VGM_WEIGHT = ParseDecimalValue(GetValue("VGM_WEIGHT")),
                        BOOK_NO = GetValue("BOOK_NO"),
                        BILL_OF_LADING = GetValue("BILL_OF_LADING", "BL_NO"),
                        LOCATION = GetValue("LOCATION"),
                        CATEGORY = GetValue("CATEGORY"),
                        ARR_BY = GetValue("ARR_BY"),
                        ARR_CAR = GetValue("ARR_CAR"),
                        ARR_VES_NAME = GetValue("ARR_VES_NAME"),
                        ARR_TS = ParseDateValue(GetValue("ARR_TS")),
                        DEP_BY = GetValue("DEP_BY"),
                        DEP_CAR = GetValue("DEP_CAR"),
                        DEP_VES_NAME = GetValue("DEP_VES_NAME"),
                        DEP_TS = ParseDateValue(GetValue("DEP_TS")),
                        DISCH_PORT = GetValue("DISCH_PORT", "CANGCT"),
                        FINAL_DISCH_PORT = GetValue("FINAL_DISCH_PORT"),
                        PLACE_OF_RECEIPT = GetValue("PLACE_OF_RECEIPT"),
                        PLACE_OF_DELIVERY = GetValue("PLACE_OF_DELIVERY", "CANGDEN"),
                        CUSTOM_CLEARANCE = GetValue("CUSTOM_CLEARANCE"),
                        CC_TS = ParseDateValue(GetValue("CC_TS")),
                        DGS_CLASS = GetValue("DGS_CLASS"),
                        UN_NO = GetValue("UN_NO"),
                        DAM = GetValue("DAM"),
                        GHICHU = GetValue("GHICHU"),
                        SOSEAL = GetValue("SOSEAL"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
            }
        }

        if (incomingImp.Count == 0 && incomingExp.Count == 0)
            return new ExcelImportCrudResult(0, "Không đọc được dữ liệu AMS từ file XML Spreadsheet.");

        var existingImpSignatures = new HashSet<string>(
            (await context.YardMovement_AMS_26040816_Imp.ToListAsync(ct)).Select(BuildAmsImpSignature),
            StringComparer.OrdinalIgnoreCase);
        var existingExpSignatures = new HashSet<string>(
            (await context.YardMovement_AMS_26040816_Current_In_Yard2.ToListAsync(ct)).Select(BuildAmsExpSignature),
            StringComparer.OrdinalIgnoreCase);
        var inserted = 0;

        foreach (var inc in incomingImp)
        {
            var signature = BuildAmsImpSignature(inc);
            if (existingImpSignatures.Contains(signature))
                continue;
            await context.YardMovement_AMS_26040816_Imp.AddAsync(inc, ct);
            existingImpSignatures.Add(signature);
            inserted++;
        }

        foreach (var inc in incomingExp)
        {
            var signature = BuildAmsExpSignature(inc);
            if (existingExpSignatures.Contains(signature))
                continue;
            await context.YardMovement_AMS_26040816_Current_In_Yard2.AddAsync(inc, ct);
            existingExpSignatures.Add(signature);
            inserted++;
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted, null);
    }

    private static List<string?> ParseXmlSpreadsheetRow(string rowXml, Regex cellRegex, Regex dataRegex, Regex indexRegex)
    {
        var values = new List<string?>();
        var currentCol = 1;
        foreach (Match cell in cellRegex.Matches(rowXml))
        {
            var attrs = cell.Groups["attrs"].Value;
            var idxMatch = indexRegex.Match(attrs);
            if (idxMatch.Success && int.TryParse(idxMatch.Groups["idx"].Value, out var indexedCol) && indexedCol > 0)
            {
                while (currentCol < indexedCol)
                {
                    values.Add(null);
                    currentCol++;
                }
            }

            var data = dataRegex.Match(cell.Groups["cell"].Value);
            values.Add(RemoveXmlTags(data.Success ? data.Groups["data"].Value : string.Empty));
            currentCol++;
        }
        return values;
    }

    private static string RemoveXmlTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        var noTags = Regex.Replace(input, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(noTags).Trim();
    }

    private static DateTime? ParseDateValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var oa))
        {
            try { return DateTime.FromOADate(oa); } catch { }
        }
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt;
        if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dtCur))
            return dtCur;
        return null;
    }

    private static decimal? ParseDecimalValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
            return inv;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var cur))
            return cur;
        return null;
    }

    private async Task<ExcelImportCrudResult> ImportHdsAsync(DataSet ds, string? importUser, CancellationToken ct)
    {
        var table = ds.Tables.Cast<DataTable>().FirstOrDefault();
        if (table == null || table.Rows.Count == 0)
            return new ExcelImportCrudResult(0, "File không có dữ liệu.");

        var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
        var map = BuildHeaderMap(headers);

        var incoming = new List<M_StockGateOut_HDS_08042026>();
        for (var i = 1; i < table.Rows.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var row = table.Rows[i];
            var soCont = ReadString(row, map, "Số Cont");
            if (string.IsNullOrWhiteSpace(soCont))
                continue;

            incoming.Add(new M_StockGateOut_HDS_08042026
            {
                Id = Guid.NewGuid(),
                ChuHang = ReadString(row, map, "Chủ hàng"),
                TenTau = ReadString(row, map, "Tên tàu"),
                NgayCapBen = ReadDate(row, map, "Ngày cập bến"),
                ChuyenNhap = ReadString(row, map, "Chuyến nhập"),
                ChuyenXuat = ReadString(row, map, "Chuyến xuất"),
                SoCont = soCont,
                KichCo = ReadString(row, map, "Kích cỡ"),
                KichCoISO = ReadString(row, map, "Kích cỡ ISO"),
                SoNiemChi = ReadString(row, map, "Số Niêm Chì"),
                SoNiemChi01 = ReadString(row, map, "Số Niêm Chì 01"),
                SoNiemChi02 = ReadString(row, map, "Số Niêm Chì 02"),
                HangKhaiThac = ReadString(row, map, "Hãng khai thác"),
                HKTTau = ReadString(row, map, "HKT tàu"),
                FE = ReadString(row, map, "F/E"),
                TrongLuong = ReadDecimal(row, map, "Trọng lượng"),
                VGM = ReadDecimal(row, map, "VGM"),
                ContVaoBai = ReadDate(row, map, "Cont vào bãi"),
                ContRaBai = ReadDate(row, map, "Cont ra bãi"),
                XeVaoCong = ReadDate(row, map, "Xe vào cổng"),
                XeRaCong = ReadDate(row, map, "Xe ra cổng"),
                NgayHoanTat = ReadDate(row, map, "Ngày hoàn tất"),
                SoNgayLuuBai = ReadInt(row, map, "Số ngày lưu bãi"),
                SoXe = ReadString(row, map, "Số xe"),
                SoRomooc = ReadString(row, map, "Số Romooc"),
                PhuongAn = ReadString(row, map, "Phương án"),
                PhuongThucGiaoNhan = ReadString(row, map, "Phương thức giao nhận"),
                SoBooking = ReadString(row, map, "Số Booking"),
                SoBL = ReadString(row, map, "Số B/L"),
                GhiChu = ReadString(row, map, "Ghi Chú"),
                GhiChuCont = ReadString(row, map, "Ghi Chú Cont"),
                GhiChuTaiCong = ReadString(row, map, "Ghi Chú Tại Cổng"),
                GhiChuTaiCauTau = ReadString(row, map, "Ghi Chú Tại Cầu Tàu"),
                MaDTTT = ReadString(row, map, "Mã ĐTTT"),
                DoiTuongThanhToan = ReadString(row, map, "Đối tượng thanh toán"),
                ChuyenCang = ReadString(row, map, "Chuyển cảng"),
                CangGiaoNhan = ReadString(row, map, "Cảng giao/nhận"),
                TTHaiQuan = ReadString(row, map, "TT hải quan"),
                LoaiHang = ReadString(row, map, "Loại Hàng"),
                HangNgoaiNoi = ReadString(row, map, "Hàng Ngoại/Nội"),
                ContainerStatus = ReadString(row, map, "Container status"),
                TinhTrangVo = ReadString(row, map, "Tình trạng vỏ"),
                ContQuaCan = ReadString(row, map, "Cont qua cân"),
                SoLenh = ReadString(row, map, "Số lệnh"),
                DateImport = DateTime.UtcNow,
                UserImport = importUser,
                CreatedAt = DateTime.UtcNow
            });
        }

        var existing = await context.StockGateOut_HDS_08042026
            .ToListAsync(ct);
        var byKey = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.SoCont))
            .GroupBy(x => NormalizeKey(x.SoCont))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        var updated = 0;

        foreach (var inc in incoming)
        {
            ct.ThrowIfCancellationRequested();
            var key = NormalizeKey(inc.SoCont);
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!byKey.TryGetValue(key, out var cur))
            {
                // insert new
                inc.Id = Guid.NewGuid();
                inc.DateImport = DateTime.UtcNow;
                inc.UserImport = importUser;
                inc.CreatedAt = DateTime.UtcNow;
                await context.StockGateOut_HDS_08042026.AddAsync(inc, ct);
                byKey[key] = inc;
                inserted++;
                continue;
            }

            // compare ignoring Id/CreatedAt; if changed -> update
            if (!HdsEqualsIgnoringImportMeta(cur, inc))
            {
                CopyHdsFields(cur, inc);
                updated++;
            }
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted + updated, null);
    }

    private async Task<ExcelImportCrudResult> ImportAgAsync(DataSet ds, string? importUser, CancellationToken ct)
    {
        var firstFourSheets = ds.Tables.Cast<DataTable>().Take(4).ToList();
        if (firstFourSheets.Count == 0)
            return new ExcelImportCrudResult(0, "File không có sheet dữ liệu.");

        var incomingArrived = new List<M_8_3_Arrived>();
        var incomingExited = new List<M_8_3_Exited>();
        var incomingUnstuffed = new List<M_8_3_Unstuffed>();
        var incomingStuffed = new List<M_8_3_Stuffed>();
        var importedAt = DateTime.UtcNow;

        foreach (var table in firstFourSheets)
        {
            var normalizedSheet = NormalizeAgSheetName(table.TableName);
            var headerRowIndex = FindHeaderRow(table, new[] { "SQ", "CNTRNO." });
            if (headerRowIndex < 0)
                continue;

            var headers = table.Rows[headerRowIndex].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
            var map = BuildHeaderMap(headers);

            for (var i = headerRowIndex + 1; i < table.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var row = table.Rows[i];
                var container = ReadString(row, map, "CNTRNO.");
                if (string.IsNullOrWhiteSpace(container))
                    continue;

                if (string.Equals(normalizedSheet, "Arrived", StringComparison.OrdinalIgnoreCase))
                {
                    incomingArrived.Add(new M_8_3_Arrived
                    {
                        Id = Guid.NewGuid(),
                        SQ = ReadInt(row, map, "SQ"),
                        CNTRNO = container,
                        SZ = ReadString(row, map, "SZ"),
                        TP = ReadString(row, map, "TP"),
                        ST = ReadString(row, map, "ST"),
                        SealNo = ReadString(row, map, "SealNo"),
                        WD = ReadDecimal(row, map, "WD"),
                        WN = ReadDecimal(row, map, "WN"),
                        CC = ReadString(row, map, "CC"),
                        Location = ReadString(row, map, "Location"),
                        DoBkNo = ReadString(row, map, "Do/Bk No"),
                        POD_FDest = ReadString(row, map, "POD-FDest"),
                        Customer = ReadString(row, map, "CUSTOMER"),
                        Payment = ReadString(row, map, "Payment"),
                        TruckNo = ReadString(row, map, "Truck No"),
                        VslVoy = ReadString(row, map, "Vsl/Voy"),
                        DT = ReadDate(row, map, "D/T"),
                        Cargo = ReadString(row, map, "Cargo"),
                        Remarks = ReadString(row, map, "Remarks"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else if (string.Equals(normalizedSheet, "Exited", StringComparison.OrdinalIgnoreCase))
                {
                    incomingExited.Add(new M_8_3_Exited
                    {
                        Id = Guid.NewGuid(),
                        SQ = ReadInt(row, map, "SQ"),
                        CNTRNO = container,
                        SZ = ReadString(row, map, "SZ"),
                        TP = ReadString(row, map, "TP"),
                        ST = ReadString(row, map, "ST"),
                        SealNo = ReadString(row, map, "SealNo"),
                        WD = ReadDecimal(row, map, "WD"),
                        WN = ReadDecimal(row, map, "WN"),
                        CC = ReadString(row, map, "CC"),
                        Location = ReadString(row, map, "Location"),
                        DoBkNo = ReadString(row, map, "Do/Bk No"),
                        Days = ReadInt(row, map, "Days"),
                        Customer = ReadString(row, map, "CUSTOMER"),
                        Payment = ReadString(row, map, "Payment"),
                        TruckNo = ReadString(row, map, "Truck No"),
                        VslVoy = ReadString(row, map, "Vsl/Voy  -  POD.FDest"),
                        DT = ReadDate(row, map, "D/T"),
                        PlugIn = ReadString(row, map, "Plug In"),
                        Remarks = ReadString(row, map, "Remark"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else if (string.Equals(normalizedSheet, "Unstuffed", StringComparison.OrdinalIgnoreCase))
                {
                    incomingUnstuffed.Add(new M_8_3_Unstuffed
                    {
                        Id = Guid.NewGuid(),
                        SQ = ReadInt(row, map, "SQ"),
                        CNTRNO = container,
                        SealNo = ReadString(row, map, "SealNo"),
                        CO = ReadString(row, map, "CO"),
                        LEN = ReadString(row, map, "LEN"),
                        TY = ReadString(row, map, "TY"),
                        Cond = ReadString(row, map, "Cond"),
                        ShipCons = ReadString(row, map, "ShipCons"),
                        BKDO = ReadString(row, map, "BKDO"),
                        DISC_VV = ReadString(row, map, "DISC V/V"),
                        UNSDT = ReadDate(row, map, "UNSDT"),
                        CCDT = ReadDate(row, map, "CCDT"),
                        JOB = ReadString(row, map, "JOB"),
                        Payment = ReadString(row, map, "Payment"),
                        SrvCode = ReadString(row, map, "SrvCode"),
                        MODE = ReadString(row, map, "MODE"),
                        STV = ReadString(row, map, "STV"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else if (string.Equals(normalizedSheet, "Stuffed", StringComparison.OrdinalIgnoreCase))
                {
                    incomingStuffed.Add(new M_8_3_Stuffed
                    {
                        Id = Guid.NewGuid(),
                        SQ = ReadInt(row, map, "SQ"),
                        CNTRNO = container,
                        SealNo = ReadString(row, map, "SealNo"),
                        CO = ReadString(row, map, "CO"),
                        LEN = ReadString(row, map, "LEN"),
                        TY = ReadString(row, map, "TY"),
                        WD = ReadDecimal(row, map, "WD"),
                        WN = ReadDecimal(row, map, "WN"),
                        Cond = ReadString(row, map, "Cond"),
                        ShipCons = ReadString(row, map, "ShipCons"),
                        BKDO = ReadString(row, map, "BKDO"),
                        LOAD_VV = ReadString(row, map, "LOADV/V"),
                        POD_PDest = ReadString(row, map, "POD-PDest"),
                        STUDT = ReadDate(row, map, "STUDT"),
                        PlugIn = ReadString(row, map, "Plug In"),
                        JOB = ReadString(row, map, "JOB"),
                        Payment = ReadString(row, map, "Payment"),
                        SrvCode = ReadString(row, map, "SrvCode"),
                        MODE = ReadString(row, map, "MODE"),
                        Cargo = ReadString(row, map, "Cargo"),
                        WeightRemarks = ReadString(row, map, "Weight-Remarks"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
            }
        }

        var inserted = 0;

        var arrivedSignatures = new HashSet<string>((await context.AG_8_3_Arrived.ToListAsync(ct)).Select(BuildAgArrivedSignature), StringComparer.OrdinalIgnoreCase);
        foreach (var item in incomingArrived)
        {
            var signature = BuildAgArrivedSignature(item);
            if (arrivedSignatures.Contains(signature))
                continue;
            await context.AG_8_3_Arrived.AddAsync(item, ct);
            arrivedSignatures.Add(signature);
            inserted++;
        }

        var exitedSignatures = new HashSet<string>((await context.AG_8_3_Exited.ToListAsync(ct)).Select(BuildAgExitedSignature), StringComparer.OrdinalIgnoreCase);
        foreach (var item in incomingExited)
        {
            var signature = BuildAgExitedSignature(item);
            if (exitedSignatures.Contains(signature))
                continue;
            await context.AG_8_3_Exited.AddAsync(item, ct);
            exitedSignatures.Add(signature);
            inserted++;
        }

        var unstuffedSignatures = new HashSet<string>((await context.AG_8_3_Unstuffed.ToListAsync(ct)).Select(BuildAgUnstuffedSignature), StringComparer.OrdinalIgnoreCase);
        foreach (var item in incomingUnstuffed)
        {
            var signature = BuildAgUnstuffedSignature(item);
            if (unstuffedSignatures.Contains(signature))
                continue;
            await context.AG_8_3_Unstuffed.AddAsync(item, ct);
            unstuffedSignatures.Add(signature);
            inserted++;
        }

        var stuffedSignatures = new HashSet<string>((await context.AG_8_3_Stuffed.ToListAsync(ct)).Select(BuildAgStuffedSignature), StringComparer.OrdinalIgnoreCase);
        foreach (var item in incomingStuffed)
        {
            var signature = BuildAgStuffedSignature(item);
            if (stuffedSignatures.Contains(signature))
                continue;
            await context.AG_8_3_Stuffed.AddAsync(item, ct);
            stuffedSignatures.Add(signature);
            inserted++;
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted, null);
    }

    private async Task<ExcelImportCrudResult> ImportVssAsync(DataSet ds, string? importUser, CancellationToken ct)
    {
        var incomingImp = new List<M_8_3_VSS_IN_OUT_YARD_Imp>();
        var incomingExp = new List<M_8_3_VSS_IN_OUT_YARD_Exp>();
        var matchedSheets = new List<string>();
        var importedAt = DateTime.UtcNow;
        foreach (DataTable table in ds.Tables)
        {
            if (!IsAllowedVssSheetName(table.TableName))
                continue;
            var normalizedSheet = NormalizeVssSheetName(table.TableName);
            matchedSheets.Add(normalizedSheet);
            if (table.Rows.Count <= 1)
                continue;

            // Header is expected at row 1; data starts from row 2.
            var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
            var map = BuildHeaderMap(headers);

            for (var i = 1; i < table.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var row = table.Rows[i];
                if (string.IsNullOrWhiteSpace(ReadString(row, map, "SOCONT")))
                    continue;

                if (IsAmsImpSheet(normalizedSheet))
                {
                    incomingImp.Add(new M_8_3_VSS_IN_OUT_YARD_Imp
                    {
                        Id = Guid.NewGuid(),
                        METHOD = ReadString(row, map, "METHOD"),
                        OPERATION_METHOD = ReadString(row, map, "OPERATION_METHOD"),
                        EXEC_TS = ReadDate(row, map, "EXEC_TS"),
                        LINE = ReadString(row, map, "LINE"),
                        AGENT = ReadString(row, map, "AGENT"),
                        KHACHHANG = ReadString(row, map, "KHACHHANG"),
                        ITEM_KEY = ReadString(row, map, "ITEM_KEY"),
                        SOCONT = ReadString(row, map, "SOCONT"),
                        KICHCO = ReadString(row, map, "KICHCO"),
                        PORT_GRADE = ReadString(row, map, "PORT_GRADE"),
                        LINE_GRADE = ReadString(row, map, "LINE_GRADE"),
                        TRANGTHAI = ReadString(row, map, "TRANGTHAI"),
                        TRONGLUONG = ReadDecimal(row, map, "TRONGLUONG"),
                        TRONGLUONG_VGM = ReadDecimal(row, map, "TRONGLUONG_VGM"),
                        BL_NO = ReadString(row, map, "BL_NO"),
                        BOOK_NO = ReadString(row, map, "BOOK_NO"),
                        RELEASE_NO = ReadString(row, map, "RELEASE_NO"),
                        HUONG = ReadString(row, map, "HUONG"),
                        HUONG1 = ReadString(row, map, "HUONG1"),
                        CANGCT = ReadString(row, map, "CANGCT"),
                        CANGDEN = ReadString(row, map, "CANGDEN"),
                        GIAO = ReadString(row, map, "GIAO"),
                        NHAN = ReadString(row, map, "NHAN"),
                        DGS_CLASS = ReadString(row, map, "DGS_CLASS"),
                        GHICHU = ReadString(row, map, "GHICHU"),
                        ENTRY_VOY_NO = ReadString(row, map, "ENTRY_VOY_NO"),
                        ENTRY_VES_NAME = ReadString(row, map, "ENTRY_VES_NAME"),
                        EXIT_VOY_NO = ReadString(row, map, "EXIT_VOY_NO"),
                        EXIT_VES_NAME = ReadString(row, map, "EXIT_VES_NAME"),
                        ENTRY_TRUCK_ID = ReadString(row, map, "ENTRY_TRUCK_ID"),
                        EXIT_TRUCK_ID = ReadString(row, map, "EXIT_TRUCK_ID"),
                        SOSEAL = ReadString(row, map, "SOSEAL"),
                        STORAGEDAY = ReadInt(row, map, "STORAGEDAY"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else
                {
                    incomingExp.Add(new M_8_3_VSS_IN_OUT_YARD_Exp
                    {
                        Id = Guid.NewGuid(),
                        METHOD = ReadString(row, map, "METHOD"),
                        OPERATION_METHOD = ReadString(row, map, "OPERATION_METHOD"),
                        EXEC_TS = ReadDate(row, map, "EXEC_TS"),
                        LINE = ReadString(row, map, "LINE"),
                        AGENT = ReadString(row, map, "AGENT"),
                        KHACHHANG = ReadString(row, map, "KHACHHANG"),
                        ITEM_KEY = ReadString(row, map, "ITEM_KEY"),
                        SOCONT = ReadString(row, map, "SOCONT"),
                        KICHCO = ReadString(row, map, "KICHCO"),
                        PORT_GRADE = ReadString(row, map, "PORT_GRADE"),
                        LINE_GRADE = ReadString(row, map, "LINE_GRADE"),
                        TRANGTHAI = ReadString(row, map, "TRANGTHAI"),
                        TRONGLUONG = ReadDecimal(row, map, "TRONGLUONG"),
                        TRONGLUONG_VGM = ReadDecimal(row, map, "TRONGLUONG_VGM"),
                        BL_NO = ReadString(row, map, "BL_NO"),
                        BOOK_NO = ReadString(row, map, "BOOK_NO"),
                        RELEASE_NO = ReadString(row, map, "RELEASE_NO"),
                        HUONG = ReadString(row, map, "HUONG"),
                        HUONG1 = ReadString(row, map, "HUONG1"),
                        CANGCT = ReadString(row, map, "CANGCT"),
                        CANGDEN = ReadString(row, map, "CANGDEN"),
                        GIAO = ReadString(row, map, "GIAO"),
                        NHAN = ReadString(row, map, "NHAN"),
                        DGS_CLASS = ReadString(row, map, "DGS_CLASS"),
                        GHICHU = ReadString(row, map, "GHICHU"),
                        ENTRY_VOY_NO = ReadString(row, map, "ENTRY_VOY_NO"),
                        ENTRY_VES_NAME = ReadString(row, map, "ENTRY_VES_NAME"),
                        EXIT_VOY_NO = ReadString(row, map, "EXIT_VOY_NO"),
                        EXIT_VES_NAME = ReadString(row, map, "EXIT_VES_NAME"),
                        ENTRY_TRUCK_ID = ReadString(row, map, "ENTRY_TRUCK_ID"),
                        EXIT_TRUCK_ID = ReadString(row, map, "EXIT_TRUCK_ID"),
                        SOSEAL = ReadString(row, map, "SOSEAL"),
                        STORAGEDAY = ReadInt(row, map, "STORAGEDAY"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
            }
        }

        if (matchedSheets.Count == 0)
            return new ExcelImportCrudResult(0, "Không tìm thấy sheet VSS hợp lệ (chỉ nhận IN-OUT_YARD__Imp và IN-OUT_YARD__Exp).");

        var existingImpSignatures = new HashSet<string>(
            (await context.YardMovement_VSS_26040808_Imp.ToListAsync(ct)).Select(BuildVssImpSignature),
            StringComparer.OrdinalIgnoreCase);
        var existingExpSignatures = new HashSet<string>(
            (await context.YardMovement_VSS_26040808_Exp.ToListAsync(ct)).Select(BuildVssExpSignature),
            StringComparer.OrdinalIgnoreCase);
        var inserted = 0;

        foreach (var inc in incomingImp)
        {
            ct.ThrowIfCancellationRequested();
            var signature = BuildVssImpSignature(inc);
            if (existingImpSignatures.Contains(signature))
                continue;

            inc.Id = Guid.NewGuid();
            inc.DateImport = importedAt;
            inc.UserImport = importUser;
            inc.CreatedAt = importedAt;
            await context.YardMovement_VSS_26040808_Imp.AddAsync(inc, ct);
            existingImpSignatures.Add(signature);
            inserted++;
        }

        foreach (var inc in incomingExp)
        {
            ct.ThrowIfCancellationRequested();
            var signature = BuildVssExpSignature(inc);
            if (existingExpSignatures.Contains(signature))
                continue;

            inc.Id = Guid.NewGuid();
            inc.DateImport = importedAt;
            inc.UserImport = importUser;
            inc.CreatedAt = importedAt;
            await context.YardMovement_VSS_26040808_Exp.AddAsync(inc, ct);
            existingExpSignatures.Add(signature);
            inserted++;
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted, null);
    }

    private static bool IsAllowedVssSheetName(string? rawSheetName)
    {
        if (string.IsNullOrWhiteSpace(rawSheetName))
            return false;

        return AllowedVssSheetNames.Contains(NormalizeVssSheetName(rawSheetName));
    }

    private static string NormalizeVssSheetName(string? rawSheetName)
    {
        var normalized = (rawSheetName ?? string.Empty).Trim();
        if (normalized.EndsWith("$", StringComparison.Ordinal))
            normalized = normalized[..^1].Trim();
        return normalized;
    }

    private static bool IsAllowedAmsSheetName(string? rawSheetName)
    {
        if (string.IsNullOrWhiteSpace(rawSheetName))
            return false;
        return AllowedAmsSheetNames.Contains(NormalizeAmsSheetName(rawSheetName));
    }

    private static string NormalizeAmsSheetName(string? rawSheetName)
    {
        var normalized = (rawSheetName ?? string.Empty).Trim();
        if (normalized.EndsWith("$", StringComparison.Ordinal))
            normalized = normalized[..^1].Trim();
        return normalized;
    }

    private static bool IsAmsImpSheet(string normalizedSheetName)
    {
        return string.Equals(normalizedSheetName, "IN-OUT_YARD__Imp", StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedSheetName, "IN-OUT_YARD_1", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeAgSheetName(string? rawSheetName)
    {
        var normalized = (rawSheetName ?? string.Empty).Trim();
        if (normalized.EndsWith("$", StringComparison.Ordinal))
            normalized = normalized[..^1].Trim();
        return normalized;
    }

    private async Task<ExcelImportCrudResult> ImportAmsAsync(DataSet ds, string? importUser, string? amsLineFilter, CancellationToken ct)
    {
        var incomingImp = new List<M_8_3_IN_OUT_YARD_1>();
        var incomingExp = new List<M_YardMovement_AMS_26040816_Current_In_Yard2>();
        var matchedSheets = new List<string>();
        var importedAt = DateTime.UtcNow;

        foreach (DataTable table in ds.Tables)
        {
            if (!IsAllowedAmsSheetName(table.TableName))
                continue;

            var normalizedSheet = NormalizeAmsSheetName(table.TableName);
            matchedSheets.Add(normalizedSheet);

            if (table.Rows.Count <= 1)
                continue;

            var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
            var map = BuildHeaderMap(headers);

            for (var i = 1; i < table.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var row = table.Rows[i];
                var containerNo = IsAmsImpSheet(normalizedSheet)
                    ? ReadString(row, map, "SOCONT")
                    : ReadString(row, map, "ITEM_NO", "SOCONT", "CONTAINER", "CNTRNO");
                if (string.IsNullOrWhiteSpace(containerNo))
                    continue;

                var lineValue = ReadString(row, map, "LINE");
                if (!MatchesAmsLineFilter(lineValue, amsLineFilter))
                    continue;

                if (IsAmsImpSheet(normalizedSheet))
                {
                    incomingImp.Add(new M_8_3_IN_OUT_YARD_1
                    {
                        Id = Guid.NewGuid(),
                        METHOD = ReadString(row, map, "METHOD"),
                        EXEC_TS = ReadDate(row, map, "EXEC_TS"),
                        LINE = lineValue,
                        ITEM_KEY = ReadString(row, map, "ITEM_KEY"),
                        SOCONT = containerNo,
                        KICHCO = ReadString(row, map, "KICHCO"),
                        TRANGTHAI = ReadString(row, map, "TRANGTHAI"),
                        TRONGLUONG = ReadDecimal(row, map, "TRONGLUONG"),
                        TRONGLUONG_VGM = ReadDecimal(row, map, "TRONGLUONG_VGM"),
                        BL_NO = ReadString(row, map, "BL_NO"),
                        BOOK_NO = ReadString(row, map, "BOOK_NO"),
                        RELEASE_NO = ReadString(row, map, "RELEASE_NO"),
                        HUONG = ReadString(row, map, "HUONG"),
                        HUONG1 = ReadString(row, map, "HUONG1"),
                        CANGCT = ReadString(row, map, "CANGCT"),
                        CANGDEN = ReadString(row, map, "CANGDEN"),
                        GIAO = ReadString(row, map, "GIAO"),
                        NHAN = ReadString(row, map, "NHAN"),
                        DGS_CLASS = ReadString(row, map, "DGS_CLASS"),
                        GHICHU = ReadString(row, map, "GHICHU"),
                        ENTRY_VOY_NO = ReadString(row, map, "ENTRY_VOY_NO"),
                        ENTRY_VES_NAME = ReadString(row, map, "ENTRY_VES_NAME"),
                        EXIT_VOY_NO = ReadString(row, map, "EXIT_VOY_NO"),
                        EXIT_VES_NAME = ReadString(row, map, "EXIT_VES_NAME"),
                        ENTRY_TRUCK_ID = ReadString(row, map, "ENTRY_TRUCK_ID"),
                        EXIT_TRUCK_ID = ReadString(row, map, "EXIT_TRUCK_ID"),
                        SOSEAL = ReadString(row, map, "SOSEAL"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
                else
                {
                    incomingExp.Add(new M_YardMovement_AMS_26040816_Current_In_Yard2
                    {
                        Id = Guid.NewGuid(),
                        AGENT = ReadString(row, map, "AGENT"),
                        LINE = lineValue,
                        ITEM_KEY = ReadString(row, map, "ITEM_KEY"),
                        ITEM_NO = containerNo,
                        ISO = ReadString(row, map, "ISO"),
                        FEL = ReadString(row, map, "FEL"),
                        TEMP = ReadDecimal(row, map, "TEMP"),
                        WEIGHT = ReadDecimal(row, map, "WEIGHT"),
                        VGM_WEIGHT = ReadDecimal(row, map, "VGM_WEIGHT"),
                        BOOK_NO = ReadString(row, map, "BOOK_NO"),
                        BILL_OF_LADING = ReadString(row, map, "BILL_OF_LADING"),
                        LOCATION = ReadString(row, map, "LOCATION"),
                        CATEGORY = ReadString(row, map, "CATEGORY"),
                        ARR_BY = ReadString(row, map, "ARR_BY"),
                        ARR_CAR = ReadString(row, map, "ARR_CAR"),
                        ARR_VES_NAME = ReadString(row, map, "ARR_VES_NAME"),
                        ARR_TS = ReadDate(row, map, "ARR_TS"),
                        DEP_BY = ReadString(row, map, "DEP_BY"),
                        DEP_CAR = ReadString(row, map, "DEP_CAR"),
                        DEP_VES_NAME = ReadString(row, map, "DEP_VES_NAME"),
                        DEP_TS = ReadDate(row, map, "DEP_TS"),
                        DISCH_PORT = ReadString(row, map, "DISCH_PORT"),
                        FINAL_DISCH_PORT = ReadString(row, map, "FINAL_DISCH_PORT"),
                        PLACE_OF_RECEIPT = ReadString(row, map, "PLACE_OF_RECEIPT"),
                        PLACE_OF_DELIVERY = ReadString(row, map, "PLACE_OF_DELIVERY"),
                        CUSTOM_CLEARANCE = ReadString(row, map, "CUSTOM_CLEARANCE"),
                        CC_TS = ReadDate(row, map, "CC_TS"),
                        DGS_CLASS = ReadString(row, map, "DGS_CLASS"),
                        UN_NO = ReadString(row, map, "UN_NO"),
                        DAM = ReadString(row, map, "DAM"),
                        GHICHU = ReadString(row, map, "GHICHU"),
                        SOSEAL = ReadString(row, map, "SOSEAL"),
                        DateImport = importedAt,
                        UserImport = importUser,
                        CreatedAt = importedAt
                    });
                }
            }
        }

        if (matchedSheets.Count == 0)
            return new ExcelImportCrudResult(0, "Không tìm thấy sheet AMS hợp lệ (nhận: IN-OUT_YARD_1, Current_In_Yard2, IN-OUT_YARD__Imp, IN-OUT_YARD__Exp).");

        var existingImpSignatures = new HashSet<string>(
            (await context.YardMovement_AMS_26040816_Imp.ToListAsync(ct)).Select(BuildAmsImpSignature),
            StringComparer.OrdinalIgnoreCase);
        var existingExpSignatures = new HashSet<string>(
            (await context.YardMovement_AMS_26040816_Current_In_Yard2.ToListAsync(ct)).Select(BuildAmsExpSignature),
            StringComparer.OrdinalIgnoreCase);
        var inserted = 0;

        foreach (var inc in incomingImp)
        {
            ct.ThrowIfCancellationRequested();
            var signature = BuildAmsImpSignature(inc);
            if (existingImpSignatures.Contains(signature))
                continue;
            await context.YardMovement_AMS_26040816_Imp.AddAsync(inc, ct);
            existingImpSignatures.Add(signature);
            inserted++;
        }

        foreach (var inc in incomingExp)
        {
            ct.ThrowIfCancellationRequested();
            var signature = BuildAmsExpSignature(inc);
            if (existingExpSignatures.Contains(signature))
                continue;
            await context.YardMovement_AMS_26040816_Current_In_Yard2.AddAsync(inc, ct);
            existingExpSignatures.Add(signature);
            inserted++;
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted, null);
    }

    private static string NormalizeKey(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeDecimalKey(decimal? value)
        => value.HasValue ? value.Value.ToString("G29", CultureInfo.InvariantCulture) : string.Empty;


    private static string BuildAgArrivedSignature(M_8_3_Arrived x)
        => string.Join("|",
            x.SQ?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.CNTRNO),
            NormalizeKey(x.SZ),
            NormalizeKey(x.TP),
            NormalizeKey(x.ST),
            NormalizeKey(x.SealNo),
            NormalizeDecimalKey(x.WD),
            NormalizeDecimalKey(x.WN),
            NormalizeKey(x.CC),
            NormalizeKey(x.Location),
            NormalizeKey(x.DoBkNo),
            NormalizeKey(x.POD_FDest),
            NormalizeKey(x.Customer),
            NormalizeKey(x.Payment),
            NormalizeKey(x.TruckNo),
            NormalizeKey(x.VslVoy),
            x.DT?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.Cargo),
            NormalizeKey(x.Remarks));

    private static string BuildAgExitedSignature(M_8_3_Exited x)
        => string.Join("|",
            x.SQ?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.CNTRNO),
            NormalizeKey(x.SZ),
            NormalizeKey(x.TP),
            NormalizeKey(x.ST),
            NormalizeKey(x.SealNo),
            NormalizeDecimalKey(x.WD),
            NormalizeDecimalKey(x.WN),
            NormalizeKey(x.CC),
            NormalizeKey(x.Location),
            NormalizeKey(x.DoBkNo),
            x.Days?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.Customer),
            NormalizeKey(x.Payment),
            NormalizeKey(x.TruckNo),
            NormalizeKey(x.VslVoy),
            x.DT?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.PlugIn),
            NormalizeKey(x.Remarks));

    private static string BuildAgUnstuffedSignature(M_8_3_Unstuffed x)
        => string.Join("|",
            x.SQ?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.CNTRNO),
            NormalizeKey(x.SealNo),
            NormalizeKey(x.CO),
            NormalizeKey(x.LEN),
            NormalizeKey(x.TY),
            NormalizeKey(x.Cond),
            NormalizeKey(x.ShipCons),
            NormalizeKey(x.BKDO),
            NormalizeKey(x.DISC_VV),
            x.UNSDT?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            x.CCDT?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.JOB),
            NormalizeKey(x.Payment),
            NormalizeKey(x.SrvCode),
            NormalizeKey(x.MODE),
            NormalizeKey(x.STV));

    private static string BuildAgStuffedSignature(M_8_3_Stuffed x)
        => string.Join("|",
            x.SQ?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.CNTRNO),
            NormalizeKey(x.SealNo),
            NormalizeKey(x.CO),
            NormalizeKey(x.LEN),
            NormalizeKey(x.TY),
            NormalizeDecimalKey(x.WD),
            NormalizeDecimalKey(x.WN),
            NormalizeKey(x.Cond),
            NormalizeKey(x.ShipCons),
            NormalizeKey(x.BKDO),
            NormalizeKey(x.LOAD_VV),
            NormalizeKey(x.POD_PDest),
            x.STUDT?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.PlugIn),
            NormalizeKey(x.JOB),
            NormalizeKey(x.Payment),
            NormalizeKey(x.SrvCode),
            NormalizeKey(x.MODE),
            NormalizeKey(x.Cargo),
            NormalizeKey(x.WeightRemarks));

    private static string MakeAgKey(string? sourceSheet, string? cntrNo, string? doBkNo)
        => $"{NormalizeKey(sourceSheet)}|{NormalizeKey(cntrNo)}|{NormalizeKey(doBkNo)}";

    private static string MakeVssKey(string? sourceSheet, string? itemKey, string? soCont, DateTime? execTs, string? method)
    {
        var ik = NormalizeKey(itemKey);
        if (!string.IsNullOrWhiteSpace(ik))
            return $"IK|{NormalizeKey(sourceSheet)}|{ik}";
        var cont = NormalizeKey(soCont);
        if (string.IsNullOrWhiteSpace(cont))
            return string.Empty;
        var ts = execTs?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        return $"FALLBACK|{NormalizeKey(sourceSheet)}|{cont}|{ts}|{NormalizeKey(method)}";
    }

    private static string BuildVssImpSignature(M_8_3_VSS_IN_OUT_YARD_Imp x)
    {
        return BuildVssSignatureCore(
            x.METHOD, x.OPERATION_METHOD, x.EXEC_TS, x.LINE, x.AGENT, x.KHACHHANG, x.ITEM_KEY, x.SOCONT, x.KICHCO,
            x.PORT_GRADE, x.LINE_GRADE, x.TRANGTHAI, x.TRONGLUONG, x.TRONGLUONG_VGM, x.BL_NO, x.BOOK_NO, x.RELEASE_NO,
            x.HUONG, x.HUONG1, x.CANGCT, x.CANGDEN, x.GIAO, x.NHAN, x.DGS_CLASS, x.GHICHU, x.ENTRY_VOY_NO, x.ENTRY_VES_NAME,
            x.EXIT_VOY_NO, x.EXIT_VES_NAME, x.ENTRY_TRUCK_ID, x.EXIT_TRUCK_ID, x.SOSEAL, x.STORAGEDAY);
    }

    private static string BuildVssExpSignature(M_8_3_VSS_IN_OUT_YARD_Exp x)
    {
        return BuildVssSignatureCore(
            x.METHOD, x.OPERATION_METHOD, x.EXEC_TS, x.LINE, x.AGENT, x.KHACHHANG, x.ITEM_KEY, x.SOCONT, x.KICHCO,
            x.PORT_GRADE, x.LINE_GRADE, x.TRANGTHAI, x.TRONGLUONG, x.TRONGLUONG_VGM, x.BL_NO, x.BOOK_NO, x.RELEASE_NO,
            x.HUONG, x.HUONG1, x.CANGCT, x.CANGDEN, x.GIAO, x.NHAN, x.DGS_CLASS, x.GHICHU, x.ENTRY_VOY_NO, x.ENTRY_VES_NAME,
            x.EXIT_VOY_NO, x.EXIT_VES_NAME, x.ENTRY_TRUCK_ID, x.EXIT_TRUCK_ID, x.SOSEAL, x.STORAGEDAY);
    }

    private static string BuildVssSignatureCore(
        string? method, string? operationMethod, DateTime? execTs, string? line, string? agent, string? khachHang, string? itemKey,
        string? soCont, string? kichCo, string? portGrade, string? lineGrade, string? trangThai, decimal? trongLuong, decimal? trongLuongVgm,
        string? blNo, string? bookNo, string? releaseNo, string? huong, string? huong1, string? cangCt, string? cangDen, string? giao,
        string? nhan, string? dgsClass, string? ghiChu, string? entryVoyNo, string? entryVesName, string? exitVoyNo, string? exitVesName,
        string? entryTruckId, string? exitTruckId, string? soSeal, int? storageDay)
    {
        var execTsValue = execTs?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        return string.Join("|",
            NormalizeKey(method),
            NormalizeKey(operationMethod),
            execTsValue,
            NormalizeKey(line),
            NormalizeKey(agent),
            NormalizeKey(khachHang),
            NormalizeKey(itemKey),
            NormalizeKey(soCont),
            NormalizeKey(kichCo),
            NormalizeKey(portGrade),
            NormalizeKey(lineGrade),
            NormalizeKey(trangThai),
            NormalizeDecimalKey(trongLuong),
            NormalizeDecimalKey(trongLuongVgm),
            NormalizeKey(blNo),
            NormalizeKey(bookNo),
            NormalizeKey(releaseNo),
            NormalizeKey(huong),
            NormalizeKey(huong1),
            NormalizeKey(cangCt),
            NormalizeKey(cangDen),
            NormalizeKey(giao),
            NormalizeKey(nhan),
            NormalizeKey(dgsClass),
            NormalizeKey(ghiChu),
            NormalizeKey(entryVoyNo),
            NormalizeKey(entryVesName),
            NormalizeKey(exitVoyNo),
            NormalizeKey(exitVesName),
            NormalizeKey(entryTruckId),
            NormalizeKey(exitTruckId),
            NormalizeKey(soSeal),
            storageDay?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
    }

    private static string BuildAmsImpSignature(M_8_3_IN_OUT_YARD_1 x)
        => BuildAmsSignatureCore(
            x.METHOD, x.EXEC_TS, x.LINE, x.ITEM_KEY, x.SOCONT, x.KICHCO, x.TRANGTHAI, x.TRONGLUONG, x.TRONGLUONG_VGM,
            x.BL_NO, x.BOOK_NO, x.RELEASE_NO, x.HUONG, x.HUONG1, x.CANGCT, x.CANGDEN, x.GIAO, x.NHAN, x.DGS_CLASS,
            x.GHICHU, x.ENTRY_VOY_NO, x.ENTRY_VES_NAME, x.EXIT_VOY_NO, x.EXIT_VES_NAME, x.ENTRY_TRUCK_ID, x.EXIT_TRUCK_ID, x.SOSEAL);

    private static string BuildAmsExpSignature(M_YardMovement_AMS_26040816_Current_In_Yard2 x)
        => string.Join("|",
            NormalizeKey(x.AGENT),
            NormalizeKey(x.LINE),
            NormalizeKey(x.ITEM_KEY),
            NormalizeKey(x.ITEM_NO),
            NormalizeKey(x.ISO),
            NormalizeKey(x.FEL),
            NormalizeDecimalKey(x.TEMP),
            NormalizeDecimalKey(x.WEIGHT),
            NormalizeDecimalKey(x.VGM_WEIGHT),
            NormalizeKey(x.BOOK_NO),
            NormalizeKey(x.BILL_OF_LADING),
            NormalizeKey(x.LOCATION),
            NormalizeKey(x.CATEGORY),
            NormalizeKey(x.ARR_BY),
            NormalizeKey(x.ARR_CAR),
            NormalizeKey(x.ARR_VES_NAME),
            x.ARR_TS?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.DEP_BY),
            NormalizeKey(x.DEP_CAR),
            NormalizeKey(x.DEP_VES_NAME),
            x.DEP_TS?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.DISCH_PORT),
            NormalizeKey(x.FINAL_DISCH_PORT),
            NormalizeKey(x.PLACE_OF_RECEIPT),
            NormalizeKey(x.PLACE_OF_DELIVERY),
            NormalizeKey(x.CUSTOM_CLEARANCE),
            x.CC_TS?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeKey(x.DGS_CLASS),
            NormalizeKey(x.UN_NO),
            NormalizeKey(x.DAM),
            NormalizeKey(x.GHICHU),
            NormalizeKey(x.SOSEAL));

    private static string BuildAmsSignatureCore(
        string? method, DateTime? execTs, string? line, string? itemKey, string? soCont, string? kichCo, string? trangThai, decimal? trongLuong,
        decimal? trongLuongVgm, string? blNo, string? bookNo, string? releaseNo, string? huong, string? huong1, string? cangCt, string? cangDen,
        string? giao, string? nhan, string? dgsClass, string? ghiChu, string? entryVoyNo, string? entryVesName, string? exitVoyNo,
        string? exitVesName, string? entryTruckId, string? exitTruckId, string? soSeal)
    {
        var execTsValue = execTs?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        return string.Join("|",
            NormalizeKey(method),
            execTsValue,
            NormalizeKey(line),
            NormalizeKey(itemKey),
            NormalizeKey(soCont),
            NormalizeKey(kichCo),
            NormalizeKey(trangThai),
            NormalizeDecimalKey(trongLuong),
            NormalizeDecimalKey(trongLuongVgm),
            NormalizeKey(blNo),
            NormalizeKey(bookNo),
            NormalizeKey(releaseNo),
            NormalizeKey(huong),
            NormalizeKey(huong1),
            NormalizeKey(cangCt),
            NormalizeKey(cangDen),
            NormalizeKey(giao),
            NormalizeKey(nhan),
            NormalizeKey(dgsClass),
            NormalizeKey(ghiChu),
            NormalizeKey(entryVoyNo),
            NormalizeKey(entryVesName),
            NormalizeKey(exitVoyNo),
            NormalizeKey(exitVesName),
            NormalizeKey(entryTruckId),
            NormalizeKey(exitTruckId),
            NormalizeKey(soSeal));
    }

    private static string MakeAmsKey(string? itemKey, string? soCont, DateTime? execTs, string? method)
    {
        var ik = NormalizeKey(itemKey);
        if (!string.IsNullOrWhiteSpace(ik))
            return $"IK|{ik}";
        var cont = NormalizeKey(soCont);
        if (string.IsNullOrWhiteSpace(cont))
            return string.Empty;
        var ts = execTs?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        return $"FALLBACK|{cont}|{ts}|{NormalizeKey(method)}";
    }

    private static bool HdsEqualsIgnoringImportMeta(M_StockGateOut_HDS_08042026 a, M_StockGateOut_HDS_08042026 b) =>
        string.Equals(a.ChuHang, b.ChuHang, StringComparison.Ordinal) &&
        string.Equals(a.TenTau, b.TenTau, StringComparison.Ordinal) &&
        a.NgayCapBen == b.NgayCapBen &&
        string.Equals(a.ChuyenNhap, b.ChuyenNhap, StringComparison.Ordinal) &&
        string.Equals(a.ChuyenXuat, b.ChuyenXuat, StringComparison.Ordinal) &&
        string.Equals(a.SoCont, b.SoCont, StringComparison.Ordinal) &&
        string.Equals(a.KichCo, b.KichCo, StringComparison.Ordinal) &&
        string.Equals(a.KichCoISO, b.KichCoISO, StringComparison.Ordinal) &&
        string.Equals(a.SoNiemChi, b.SoNiemChi, StringComparison.Ordinal) &&
        string.Equals(a.SoNiemChi01, b.SoNiemChi01, StringComparison.Ordinal) &&
        string.Equals(a.SoNiemChi02, b.SoNiemChi02, StringComparison.Ordinal) &&
        string.Equals(a.HangKhaiThac, b.HangKhaiThac, StringComparison.Ordinal) &&
        string.Equals(a.HKTTau, b.HKTTau, StringComparison.Ordinal) &&
        string.Equals(a.FE, b.FE, StringComparison.Ordinal) &&
        a.TrongLuong == b.TrongLuong &&
        a.VGM == b.VGM &&
        a.ContVaoBai == b.ContVaoBai &&
        a.ContRaBai == b.ContRaBai &&
        a.XeVaoCong == b.XeVaoCong &&
        a.XeRaCong == b.XeRaCong &&
        a.NgayHoanTat == b.NgayHoanTat &&
        a.SoNgayLuuBai == b.SoNgayLuuBai &&
        string.Equals(a.SoXe, b.SoXe, StringComparison.Ordinal) &&
        string.Equals(a.SoRomooc, b.SoRomooc, StringComparison.Ordinal) &&
        string.Equals(a.PhuongAn, b.PhuongAn, StringComparison.Ordinal) &&
        string.Equals(a.PhuongThucGiaoNhan, b.PhuongThucGiaoNhan, StringComparison.Ordinal) &&
        string.Equals(a.SoBooking, b.SoBooking, StringComparison.Ordinal) &&
        string.Equals(a.SoBL, b.SoBL, StringComparison.Ordinal) &&
        string.Equals(a.GhiChu, b.GhiChu, StringComparison.Ordinal) &&
        string.Equals(a.GhiChuCont, b.GhiChuCont, StringComparison.Ordinal) &&
        string.Equals(a.GhiChuTaiCong, b.GhiChuTaiCong, StringComparison.Ordinal) &&
        string.Equals(a.GhiChuTaiCauTau, b.GhiChuTaiCauTau, StringComparison.Ordinal) &&
        string.Equals(a.MaDTTT, b.MaDTTT, StringComparison.Ordinal) &&
        string.Equals(a.DoiTuongThanhToan, b.DoiTuongThanhToan, StringComparison.Ordinal) &&
        string.Equals(a.ChuyenCang, b.ChuyenCang, StringComparison.Ordinal) &&
        string.Equals(a.CangGiaoNhan, b.CangGiaoNhan, StringComparison.Ordinal) &&
        string.Equals(a.TTHaiQuan, b.TTHaiQuan, StringComparison.Ordinal) &&
        string.Equals(a.LoaiHang, b.LoaiHang, StringComparison.Ordinal) &&
        string.Equals(a.HangNgoaiNoi, b.HangNgoaiNoi, StringComparison.Ordinal) &&
        string.Equals(a.ContainerStatus, b.ContainerStatus, StringComparison.Ordinal) &&
        string.Equals(a.TinhTrangVo, b.TinhTrangVo, StringComparison.Ordinal) &&
        string.Equals(a.ContQuaCan, b.ContQuaCan, StringComparison.Ordinal) &&
        string.Equals(a.SoLenh, b.SoLenh, StringComparison.Ordinal);

    private static void CopyHdsFields(M_StockGateOut_HDS_08042026 target, M_StockGateOut_HDS_08042026 src)
    {
        target.ChuHang = src.ChuHang;
        target.TenTau = src.TenTau;
        target.NgayCapBen = src.NgayCapBen;
        target.ChuyenNhap = src.ChuyenNhap;
        target.ChuyenXuat = src.ChuyenXuat;
        target.SoCont = src.SoCont;
        target.KichCo = src.KichCo;
        target.KichCoISO = src.KichCoISO;
        target.SoNiemChi = src.SoNiemChi;
        target.SoNiemChi01 = src.SoNiemChi01;
        target.SoNiemChi02 = src.SoNiemChi02;
        target.HangKhaiThac = src.HangKhaiThac;
        target.HKTTau = src.HKTTau;
        target.FE = src.FE;
        target.TrongLuong = src.TrongLuong;
        target.VGM = src.VGM;
        target.ContVaoBai = src.ContVaoBai;
        target.ContRaBai = src.ContRaBai;
        target.XeVaoCong = src.XeVaoCong;
        target.XeRaCong = src.XeRaCong;
        target.NgayHoanTat = src.NgayHoanTat;
        target.SoNgayLuuBai = src.SoNgayLuuBai;
        target.SoXe = src.SoXe;
        target.SoRomooc = src.SoRomooc;
        target.PhuongAn = src.PhuongAn;
        target.PhuongThucGiaoNhan = src.PhuongThucGiaoNhan;
        target.SoBooking = src.SoBooking;
        target.SoBL = src.SoBL;
        target.GhiChu = src.GhiChu;
        target.GhiChuCont = src.GhiChuCont;
        target.GhiChuTaiCong = src.GhiChuTaiCong;
        target.GhiChuTaiCauTau = src.GhiChuTaiCauTau;
        target.MaDTTT = src.MaDTTT;
        target.DoiTuongThanhToan = src.DoiTuongThanhToan;
        target.ChuyenCang = src.ChuyenCang;
        target.CangGiaoNhan = src.CangGiaoNhan;
        target.TTHaiQuan = src.TTHaiQuan;
        target.LoaiHang = src.LoaiHang;
        target.HangNgoaiNoi = src.HangNgoaiNoi;
        target.ContainerStatus = src.ContainerStatus;
        target.TinhTrangVo = src.TinhTrangVo;
        target.ContQuaCan = src.ContQuaCan;
        target.SoLenh = src.SoLenh;
        target.DateImport = src.DateImport;
        target.UserImport = src.UserImport;
    }


    private static int FindHeaderRow(DataTable table, IReadOnlyCollection<string> expectedTokens)
    {
        for (var r = 0; r < table.Rows.Count; r++)
        {
            var values = table.Rows[r].ItemArray.Select(x => NormalizeHeader(x?.ToString())).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (expectedTokens.All(token => values.Any(v => string.Equals(v, NormalizeHeader(token), StringComparison.OrdinalIgnoreCase))))
                return r;
        }
        return -1;
    }

    private static Dictionary<string, int> BuildHeaderMap(IReadOnlyList<string?> headers)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < headers.Count; i++)
        {
            var key = NormalizeHeader(headers[i]);
            if (string.IsNullOrWhiteSpace(key) || map.ContainsKey(key))
                continue;
            map[key] = i;
        }
        return map;
    }

    private static string NormalizeHeader(string? header) =>
        (header ?? string.Empty).Trim();

    private static string? ReadString(DataRow row, IReadOnlyDictionary<string, int> map, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (map.TryGetValue(NormalizeHeader(alias), out var idx) && idx < row.ItemArray.Length)
            {
                var value = row[idx]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }
        return null;
    }

    private static DateTime? ReadDate(DataRow row, IReadOnlyDictionary<string, int> map, params string[] aliases)
    {
        var text = ReadString(row, map, aliases);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var oa))
        {
            try { return DateTime.FromOADate(oa); } catch { return null; }
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var inv))
            return inv;
        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var cur))
            return cur;
        return null;
    }

    private static decimal? ReadDecimal(DataRow row, IReadOnlyDictionary<string, int> map, params string[] aliases)
    {
        var text = ReadString(row, map, aliases);
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
            return inv;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var cur))
            return cur;
        return null;
    }

    private static int? ReadInt(DataRow row, IReadOnlyDictionary<string, int> map, params string[] aliases)
    {
        var text = ReadString(row, map, aliases);
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            return value;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            return (int)dec;
        return null;
    }
}
