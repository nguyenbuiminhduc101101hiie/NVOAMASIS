using System.Data;
using System.Globalization;
using System.Text;
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
    static ExcelImportCrudService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task<List<M_StockGateOut_HDS_08042026>> GetHdsAsync(CancellationToken ct = default) =>
        context.StockGateOut_HDS_08042026.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_YardReport_AG_2026040307>> GetAgAsync(CancellationToken ct = default) =>
        context.YardReport_AG_2026040307.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_YardMovement_VSS_26040808>> GetVssAsync(CancellationToken ct = default) =>
        context.YardMovement_VSS_26040808.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

    public Task<List<M_YardMovement_AMS_26040816>> GetAmsAsync(CancellationToken ct = default) =>
        context.YardMovement_AMS_26040816.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct);

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

    public async Task<ExcelImportCrudResult> ImportAsync(ExcelCrudTab tab, Stream stream, CancellationToken ct = default)
    {
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });

            return tab switch
            {
                ExcelCrudTab.HDS => await ImportHdsAsync(ds, ct),
                ExcelCrudTab.AG => await ImportAgAsync(ds, ct),
                ExcelCrudTab.VSS => await ImportVssAsync(ds, ct),
                ExcelCrudTab.AMS => await ImportAmsAsync(ds, ct),
                _ => new ExcelImportCrudResult(0, "Tab không hợp lệ.")
            };
        }
        catch (Exception ex)
        {
            return new ExcelImportCrudResult(0, ex.Message);
        }
    }

    private async Task<ExcelImportCrudResult> ImportHdsAsync(DataSet ds, CancellationToken ct)
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

    private async Task<ExcelImportCrudResult> ImportAgAsync(DataSet ds, CancellationToken ct)
    {
        var incoming = new List<M_YardReport_AG_2026040307>();
        foreach (DataTable table in ds.Tables)
        {
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

                incoming.Add(new M_YardReport_AG_2026040307
                {
                    Id = Guid.NewGuid(),
                    SourceSheet = table.TableName,
                    SQ = ReadInt(row, map, "SQ"),
                    CNTRNO = container,
                    SZ = ReadString(row, map, "SZ", "LEN"),
                    TP = ReadString(row, map, "TP", "Type"),
                    ST = ReadString(row, map, "ST"),
                    SealNo = ReadString(row, map, "SealNo"),
                    WD = ReadDecimal(row, map, "WD"),
                    WN = ReadDecimal(row, map, "WN"),
                    CC = ReadString(row, map, "CC", "CO"),
                    Location = ReadString(row, map, "Location"),
                    DoBkNo = ReadString(row, map, "Do/Bk No", "BKDO", "POL/Bno."),
                    POD_FDest = ReadString(row, map, "POD-FDest", "POD.FDest", "POD/FD/BNo.", "POD"),
                    Days = ReadInt(row, map, "Days", "DayStorage"),
                    Customer = ReadString(row, map, "CUSTOMER", "Customers", "ShipCons"),
                    Payment = ReadString(row, map, "Payment"),
                    TruckNo = ReadString(row, map, "Truck No"),
                    VslVoy = ReadString(row, map, "Vsl/Voy", "Vsl/Voy  -  POD.FDest", "LoadVV"),
                    DT = ReadDate(row, map, "D/T", "DisArrDate"),
                    PlugIn = ReadString(row, map, "Plug In"),
                    Cargo = ReadString(row, map, "Cargo"),
                    Remarks = ReadString(row, map, "Remarks", "Remark"),
                    LEN = ReadString(row, map, "LEN"),
                    TY = ReadString(row, map, "TY"),
                    Cond = ReadString(row, map, "Cond"),
                    ShipCons = ReadString(row, map, "ShipCons"),
                    BKDO = ReadString(row, map, "BKDO"),
                    DISC_VV = ReadString(row, map, "DISC V/V", "DischVV"),
                    LOAD_VV = ReadString(row, map, "LOADV/V", "LoadVV"),
                    UNSDT = ReadDate(row, map, "UNSDT"),
                    CCDT = ReadDate(row, map, "CCDT"),
                    STUDT = ReadDate(row, map, "STUDT"),
                    JOB = ReadString(row, map, "JOB"),
                    SrvCode = ReadString(row, map, "SrvCode"),
                    MODE = ReadString(row, map, "MODE"),
                    STV = ReadString(row, map, "STV"),
                    Type = ReadString(row, map, "Type"),
                    Condition = ReadString(row, map, "Condition"),
                    POL = ReadString(row, map, "POL"),
                    LoadVV = ReadString(row, map, "LoadVV"),
                    POD = ReadString(row, map, "POD"),
                    DischVV = ReadString(row, map, "DischVV"),
                    DayStatus = ReadString(row, map, "DayStatus"),
                    DayStorage = ReadInt(row, map, "DayStorage"),
                    DisArrDate = ReadDate(row, map, "DisArrDate"),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var existing = await context.YardReport_AG_2026040307.ToListAsync(ct);
        var byKey = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.CNTRNO))
            .GroupBy(x => MakeAgKey(x.SourceSheet, x.CNTRNO, x.DoBkNo))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        var updated = 0;

        foreach (var inc in incoming)
        {
            ct.ThrowIfCancellationRequested();
            var key = MakeAgKey(inc.SourceSheet, inc.CNTRNO, inc.DoBkNo);
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!byKey.TryGetValue(key, out var cur))
            {
                inc.Id = Guid.NewGuid();
                inc.CreatedAt = DateTime.UtcNow;
                await context.YardReport_AG_2026040307.AddAsync(inc, ct);
                byKey[key] = inc;
                inserted++;
                continue;
            }

            if (!AgEqualsIgnoringImportMeta(cur, inc))
            {
                CopyAgFields(cur, inc);
                updated++;
            }
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted + updated, null);
    }

    private async Task<ExcelImportCrudResult> ImportVssAsync(DataSet ds, CancellationToken ct)
    {
        var incoming = new List<M_YardMovement_VSS_26040808>();
        foreach (DataTable table in ds.Tables)
        {
            if (!table.TableName.StartsWith("IN-OUT_YARD", StringComparison.OrdinalIgnoreCase))
                continue;
            if (table.Rows.Count <= 1)
                continue;

            var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
            var map = BuildHeaderMap(headers);

            for (var i = 1; i < table.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var row = table.Rows[i];
                if (string.IsNullOrWhiteSpace(ReadString(row, map, "SOCONT")))
                    continue;

                incoming.Add(new M_YardMovement_VSS_26040808
                {
                    Id = Guid.NewGuid(),
                    SourceSheet = table.TableName,
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
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        var existing = await context.YardMovement_VSS_26040808.ToListAsync(ct);
        var byKey = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.ITEM_KEY) || !string.IsNullOrWhiteSpace(x.SOCONT))
            .GroupBy(x => MakeVssKey(x.SourceSheet, x.ITEM_KEY, x.SOCONT, x.EXEC_TS, x.METHOD))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        var updated = 0;

        foreach (var inc in incoming)
        {
            ct.ThrowIfCancellationRequested();
            var key = MakeVssKey(inc.SourceSheet, inc.ITEM_KEY, inc.SOCONT, inc.EXEC_TS, inc.METHOD);
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!byKey.TryGetValue(key, out var cur))
            {
                inc.Id = Guid.NewGuid();
                inc.CreatedAt = DateTime.UtcNow;
                await context.YardMovement_VSS_26040808.AddAsync(inc, ct);
                byKey[key] = inc;
                inserted++;
                continue;
            }

            if (!VssEqualsIgnoringImportMeta(cur, inc))
            {
                CopyVssFields(cur, inc);
                updated++;
            }
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted + updated, null);
    }

    private async Task<ExcelImportCrudResult> ImportAmsAsync(DataSet ds, CancellationToken ct)
    {
        var table = ds.Tables.Cast<DataTable>().FirstOrDefault();
        if (table == null || table.Rows.Count <= 1)
            return new ExcelImportCrudResult(0, "File AMS không có dữ liệu.");

        var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
        var map = BuildHeaderMap(headers);
        var incoming = new List<M_YardMovement_AMS_26040816>();

        for (var i = 1; i < table.Rows.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var row = table.Rows[i];
            if (string.IsNullOrWhiteSpace(ReadString(row, map, "SOCONT")))
                continue;

            incoming.Add(new M_YardMovement_AMS_26040816
            {
                Id = Guid.NewGuid(),
                METHOD = ReadString(row, map, "METHOD"),
                EXEC_TS = ReadDate(row, map, "EXEC_TS"),
                LINE = ReadString(row, map, "LINE"),
                ITEM_KEY = ReadString(row, map, "ITEM_KEY"),
                SOCONT = ReadString(row, map, "SOCONT"),
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
                CreatedAt = DateTime.UtcNow
            });
        }

        var existing = await context.YardMovement_AMS_26040816.ToListAsync(ct);
        var byKey = existing
            .Where(x => !string.IsNullOrWhiteSpace(x.ITEM_KEY) || !string.IsNullOrWhiteSpace(x.SOCONT))
            .GroupBy(x => MakeAmsKey(x.ITEM_KEY, x.SOCONT, x.EXEC_TS, x.METHOD))
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.CreatedAt).First(), StringComparer.OrdinalIgnoreCase);

        var inserted = 0;
        var updated = 0;

        foreach (var inc in incoming)
        {
            ct.ThrowIfCancellationRequested();
            var key = MakeAmsKey(inc.ITEM_KEY, inc.SOCONT, inc.EXEC_TS, inc.METHOD);
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (!byKey.TryGetValue(key, out var cur))
            {
                inc.Id = Guid.NewGuid();
                inc.CreatedAt = DateTime.UtcNow;
                await context.YardMovement_AMS_26040816.AddAsync(inc, ct);
                byKey[key] = inc;
                inserted++;
                continue;
            }

            if (!AmsEqualsIgnoringImportMeta(cur, inc))
            {
                CopyAmsFields(cur, inc);
                updated++;
            }
        }

        await context.SaveChangesAsync(ct);
        return new ExcelImportCrudResult(inserted + updated, null);
    }

    private static string NormalizeKey(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string MakeAgKey(string? sourceSheet, string? cntrNo, string? doBkNo)
        => $"{NormalizeKey(sourceSheet)}|{NormalizeKey(cntrNo)}|{NormalizeKey(doBkNo)}";

    private static string MakeVssKey(string? sourceSheet, string? itemKey, string? soCont, DateTime? execTs, string? method)
    {
        var ik = NormalizeKey(itemKey);
        if (!string.IsNullOrWhiteSpace(ik))
            return $"IK|{ik}";
        var cont = NormalizeKey(soCont);
        if (string.IsNullOrWhiteSpace(cont))
            return string.Empty;
        var ts = execTs?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty;
        return $"FALLBACK|{NormalizeKey(sourceSheet)}|{cont}|{ts}|{NormalizeKey(method)}";
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
    }

    private static bool AgEqualsIgnoringImportMeta(M_YardReport_AG_2026040307 a, M_YardReport_AG_2026040307 b) =>
        string.Equals(a.SourceSheet, b.SourceSheet, StringComparison.Ordinal) &&
        a.SQ == b.SQ &&
        string.Equals(a.CNTRNO, b.CNTRNO, StringComparison.Ordinal) &&
        string.Equals(a.SZ, b.SZ, StringComparison.Ordinal) &&
        string.Equals(a.TP, b.TP, StringComparison.Ordinal) &&
        string.Equals(a.ST, b.ST, StringComparison.Ordinal) &&
        string.Equals(a.SealNo, b.SealNo, StringComparison.Ordinal) &&
        a.WD == b.WD &&
        a.WN == b.WN &&
        string.Equals(a.CC, b.CC, StringComparison.Ordinal) &&
        string.Equals(a.Location, b.Location, StringComparison.Ordinal) &&
        string.Equals(a.DoBkNo, b.DoBkNo, StringComparison.Ordinal) &&
        string.Equals(a.POD_FDest, b.POD_FDest, StringComparison.Ordinal) &&
        a.Days == b.Days &&
        string.Equals(a.Customer, b.Customer, StringComparison.Ordinal) &&
        string.Equals(a.Payment, b.Payment, StringComparison.Ordinal) &&
        string.Equals(a.TruckNo, b.TruckNo, StringComparison.Ordinal) &&
        string.Equals(a.VslVoy, b.VslVoy, StringComparison.Ordinal) &&
        a.DT == b.DT &&
        string.Equals(a.PlugIn, b.PlugIn, StringComparison.Ordinal) &&
        string.Equals(a.Cargo, b.Cargo, StringComparison.Ordinal) &&
        string.Equals(a.Remarks, b.Remarks, StringComparison.Ordinal) &&
        string.Equals(a.LEN, b.LEN, StringComparison.Ordinal) &&
        string.Equals(a.TY, b.TY, StringComparison.Ordinal) &&
        string.Equals(a.Cond, b.Cond, StringComparison.Ordinal) &&
        string.Equals(a.ShipCons, b.ShipCons, StringComparison.Ordinal) &&
        string.Equals(a.BKDO, b.BKDO, StringComparison.Ordinal) &&
        string.Equals(a.DISC_VV, b.DISC_VV, StringComparison.Ordinal) &&
        string.Equals(a.LOAD_VV, b.LOAD_VV, StringComparison.Ordinal) &&
        a.UNSDT == b.UNSDT &&
        a.CCDT == b.CCDT &&
        a.STUDT == b.STUDT &&
        string.Equals(a.JOB, b.JOB, StringComparison.Ordinal) &&
        string.Equals(a.SrvCode, b.SrvCode, StringComparison.Ordinal) &&
        string.Equals(a.MODE, b.MODE, StringComparison.Ordinal) &&
        string.Equals(a.STV, b.STV, StringComparison.Ordinal) &&
        string.Equals(a.Type, b.Type, StringComparison.Ordinal) &&
        string.Equals(a.Condition, b.Condition, StringComparison.Ordinal) &&
        string.Equals(a.POL, b.POL, StringComparison.Ordinal) &&
        string.Equals(a.LoadVV, b.LoadVV, StringComparison.Ordinal) &&
        string.Equals(a.POD, b.POD, StringComparison.Ordinal) &&
        string.Equals(a.DischVV, b.DischVV, StringComparison.Ordinal) &&
        string.Equals(a.DayStatus, b.DayStatus, StringComparison.Ordinal) &&
        a.DayStorage == b.DayStorage &&
        a.DisArrDate == b.DisArrDate;

    private static void CopyAgFields(M_YardReport_AG_2026040307 target, M_YardReport_AG_2026040307 src)
    {
        target.SourceSheet = src.SourceSheet;
        target.SQ = src.SQ;
        target.CNTRNO = src.CNTRNO;
        target.SZ = src.SZ;
        target.TP = src.TP;
        target.ST = src.ST;
        target.SealNo = src.SealNo;
        target.WD = src.WD;
        target.WN = src.WN;
        target.CC = src.CC;
        target.Location = src.Location;
        target.DoBkNo = src.DoBkNo;
        target.POD_FDest = src.POD_FDest;
        target.Days = src.Days;
        target.Customer = src.Customer;
        target.Payment = src.Payment;
        target.TruckNo = src.TruckNo;
        target.VslVoy = src.VslVoy;
        target.DT = src.DT;
        target.PlugIn = src.PlugIn;
        target.Cargo = src.Cargo;
        target.Remarks = src.Remarks;
        target.LEN = src.LEN;
        target.TY = src.TY;
        target.Cond = src.Cond;
        target.ShipCons = src.ShipCons;
        target.BKDO = src.BKDO;
        target.DISC_VV = src.DISC_VV;
        target.LOAD_VV = src.LOAD_VV;
        target.UNSDT = src.UNSDT;
        target.CCDT = src.CCDT;
        target.STUDT = src.STUDT;
        target.JOB = src.JOB;
        target.SrvCode = src.SrvCode;
        target.MODE = src.MODE;
        target.STV = src.STV;
        target.Type = src.Type;
        target.Condition = src.Condition;
        target.POL = src.POL;
        target.LoadVV = src.LoadVV;
        target.POD = src.POD;
        target.DischVV = src.DischVV;
        target.DayStatus = src.DayStatus;
        target.DayStorage = src.DayStorage;
        target.DisArrDate = src.DisArrDate;
    }

    private static bool VssEqualsIgnoringImportMeta(M_YardMovement_VSS_26040808 a, M_YardMovement_VSS_26040808 b) =>
        string.Equals(a.SourceSheet, b.SourceSheet, StringComparison.Ordinal) &&
        string.Equals(a.METHOD, b.METHOD, StringComparison.Ordinal) &&
        string.Equals(a.OPERATION_METHOD, b.OPERATION_METHOD, StringComparison.Ordinal) &&
        a.EXEC_TS == b.EXEC_TS &&
        string.Equals(a.LINE, b.LINE, StringComparison.Ordinal) &&
        string.Equals(a.AGENT, b.AGENT, StringComparison.Ordinal) &&
        string.Equals(a.KHACHHANG, b.KHACHHANG, StringComparison.Ordinal) &&
        string.Equals(a.ITEM_KEY, b.ITEM_KEY, StringComparison.Ordinal) &&
        string.Equals(a.SOCONT, b.SOCONT, StringComparison.Ordinal) &&
        string.Equals(a.KICHCO, b.KICHCO, StringComparison.Ordinal) &&
        string.Equals(a.PORT_GRADE, b.PORT_GRADE, StringComparison.Ordinal) &&
        string.Equals(a.LINE_GRADE, b.LINE_GRADE, StringComparison.Ordinal) &&
        string.Equals(a.TRANGTHAI, b.TRANGTHAI, StringComparison.Ordinal) &&
        a.TRONGLUONG == b.TRONGLUONG &&
        a.TRONGLUONG_VGM == b.TRONGLUONG_VGM &&
        string.Equals(a.BL_NO, b.BL_NO, StringComparison.Ordinal) &&
        string.Equals(a.BOOK_NO, b.BOOK_NO, StringComparison.Ordinal) &&
        string.Equals(a.RELEASE_NO, b.RELEASE_NO, StringComparison.Ordinal) &&
        string.Equals(a.HUONG, b.HUONG, StringComparison.Ordinal) &&
        string.Equals(a.HUONG1, b.HUONG1, StringComparison.Ordinal) &&
        string.Equals(a.CANGCT, b.CANGCT, StringComparison.Ordinal) &&
        string.Equals(a.CANGDEN, b.CANGDEN, StringComparison.Ordinal) &&
        string.Equals(a.GIAO, b.GIAO, StringComparison.Ordinal) &&
        string.Equals(a.NHAN, b.NHAN, StringComparison.Ordinal) &&
        string.Equals(a.DGS_CLASS, b.DGS_CLASS, StringComparison.Ordinal) &&
        string.Equals(a.GHICHU, b.GHICHU, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_VOY_NO, b.ENTRY_VOY_NO, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_VES_NAME, b.ENTRY_VES_NAME, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_VOY_NO, b.EXIT_VOY_NO, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_VES_NAME, b.EXIT_VES_NAME, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_TRUCK_ID, b.ENTRY_TRUCK_ID, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_TRUCK_ID, b.EXIT_TRUCK_ID, StringComparison.Ordinal) &&
        string.Equals(a.SOSEAL, b.SOSEAL, StringComparison.Ordinal) &&
        a.STORAGEDAY == b.STORAGEDAY;

    private static void CopyVssFields(M_YardMovement_VSS_26040808 target, M_YardMovement_VSS_26040808 src)
    {
        target.SourceSheet = src.SourceSheet;
        target.METHOD = src.METHOD;
        target.OPERATION_METHOD = src.OPERATION_METHOD;
        target.EXEC_TS = src.EXEC_TS;
        target.LINE = src.LINE;
        target.AGENT = src.AGENT;
        target.KHACHHANG = src.KHACHHANG;
        target.ITEM_KEY = src.ITEM_KEY;
        target.SOCONT = src.SOCONT;
        target.KICHCO = src.KICHCO;
        target.PORT_GRADE = src.PORT_GRADE;
        target.LINE_GRADE = src.LINE_GRADE;
        target.TRANGTHAI = src.TRANGTHAI;
        target.TRONGLUONG = src.TRONGLUONG;
        target.TRONGLUONG_VGM = src.TRONGLUONG_VGM;
        target.BL_NO = src.BL_NO;
        target.BOOK_NO = src.BOOK_NO;
        target.RELEASE_NO = src.RELEASE_NO;
        target.HUONG = src.HUONG;
        target.HUONG1 = src.HUONG1;
        target.CANGCT = src.CANGCT;
        target.CANGDEN = src.CANGDEN;
        target.GIAO = src.GIAO;
        target.NHAN = src.NHAN;
        target.DGS_CLASS = src.DGS_CLASS;
        target.GHICHU = src.GHICHU;
        target.ENTRY_VOY_NO = src.ENTRY_VOY_NO;
        target.ENTRY_VES_NAME = src.ENTRY_VES_NAME;
        target.EXIT_VOY_NO = src.EXIT_VOY_NO;
        target.EXIT_VES_NAME = src.EXIT_VES_NAME;
        target.ENTRY_TRUCK_ID = src.ENTRY_TRUCK_ID;
        target.EXIT_TRUCK_ID = src.EXIT_TRUCK_ID;
        target.SOSEAL = src.SOSEAL;
        target.STORAGEDAY = src.STORAGEDAY;
    }

    private static bool AmsEqualsIgnoringImportMeta(M_YardMovement_AMS_26040816 a, M_YardMovement_AMS_26040816 b) =>
        string.Equals(a.METHOD, b.METHOD, StringComparison.Ordinal) &&
        a.EXEC_TS == b.EXEC_TS &&
        string.Equals(a.LINE, b.LINE, StringComparison.Ordinal) &&
        string.Equals(a.ITEM_KEY, b.ITEM_KEY, StringComparison.Ordinal) &&
        string.Equals(a.SOCONT, b.SOCONT, StringComparison.Ordinal) &&
        string.Equals(a.KICHCO, b.KICHCO, StringComparison.Ordinal) &&
        string.Equals(a.TRANGTHAI, b.TRANGTHAI, StringComparison.Ordinal) &&
        a.TRONGLUONG == b.TRONGLUONG &&
        a.TRONGLUONG_VGM == b.TRONGLUONG_VGM &&
        string.Equals(a.BL_NO, b.BL_NO, StringComparison.Ordinal) &&
        string.Equals(a.BOOK_NO, b.BOOK_NO, StringComparison.Ordinal) &&
        string.Equals(a.RELEASE_NO, b.RELEASE_NO, StringComparison.Ordinal) &&
        string.Equals(a.HUONG, b.HUONG, StringComparison.Ordinal) &&
        string.Equals(a.HUONG1, b.HUONG1, StringComparison.Ordinal) &&
        string.Equals(a.CANGCT, b.CANGCT, StringComparison.Ordinal) &&
        string.Equals(a.CANGDEN, b.CANGDEN, StringComparison.Ordinal) &&
        string.Equals(a.GIAO, b.GIAO, StringComparison.Ordinal) &&
        string.Equals(a.NHAN, b.NHAN, StringComparison.Ordinal) &&
        string.Equals(a.DGS_CLASS, b.DGS_CLASS, StringComparison.Ordinal) &&
        string.Equals(a.GHICHU, b.GHICHU, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_VOY_NO, b.ENTRY_VOY_NO, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_VES_NAME, b.ENTRY_VES_NAME, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_VOY_NO, b.EXIT_VOY_NO, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_VES_NAME, b.EXIT_VES_NAME, StringComparison.Ordinal) &&
        string.Equals(a.ENTRY_TRUCK_ID, b.ENTRY_TRUCK_ID, StringComparison.Ordinal) &&
        string.Equals(a.EXIT_TRUCK_ID, b.EXIT_TRUCK_ID, StringComparison.Ordinal) &&
        string.Equals(a.SOSEAL, b.SOSEAL, StringComparison.Ordinal);

    private static void CopyAmsFields(M_YardMovement_AMS_26040816 target, M_YardMovement_AMS_26040816 src)
    {
        target.METHOD = src.METHOD;
        target.EXEC_TS = src.EXEC_TS;
        target.LINE = src.LINE;
        target.ITEM_KEY = src.ITEM_KEY;
        target.SOCONT = src.SOCONT;
        target.KICHCO = src.KICHCO;
        target.TRANGTHAI = src.TRANGTHAI;
        target.TRONGLUONG = src.TRONGLUONG;
        target.TRONGLUONG_VGM = src.TRONGLUONG_VGM;
        target.BL_NO = src.BL_NO;
        target.BOOK_NO = src.BOOK_NO;
        target.RELEASE_NO = src.RELEASE_NO;
        target.HUONG = src.HUONG;
        target.HUONG1 = src.HUONG1;
        target.CANGCT = src.CANGCT;
        target.CANGDEN = src.CANGDEN;
        target.GIAO = src.GIAO;
        target.NHAN = src.NHAN;
        target.DGS_CLASS = src.DGS_CLASS;
        target.GHICHU = src.GHICHU;
        target.ENTRY_VOY_NO = src.ENTRY_VOY_NO;
        target.ENTRY_VES_NAME = src.ENTRY_VES_NAME;
        target.EXIT_VOY_NO = src.EXIT_VOY_NO;
        target.EXIT_VES_NAME = src.EXIT_VES_NAME;
        target.ENTRY_TRUCK_ID = src.ENTRY_TRUCK_ID;
        target.EXIT_TRUCK_ID = src.EXIT_TRUCK_ID;
        target.SOSEAL = src.SOSEAL;
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
