using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using OfficeOpenXml;

namespace NVOAMASIS.Services;

public record StockListFilter(
    string? Location,
    string? ContainerNo,
    string? Consignee,
    string? Yom,
    DateTime? DateInFrom,
    DateTime? DateInTo);

public record StockImportResult(int Inserted, int SkippedDuplicate, int SkippedEmpty, string? ErrorMessage);

public class StockService(AppDbContext context)
{
    private const int StockDataStartRow = 20;
    private const string StockSheetName = "Stock";

    public async Task<List<string>> GetDistinctLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Stock.AsNoTracking()
            .Where(x => x.Location != null && x.Location != "")
            .Select(x => x.Location!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    public async Task<M_Stock?> GetByIdAsync(Guid stockId, CancellationToken cancellationToken = default)
    {
        return await context.Stock.AsNoTracking()
            .FirstOrDefaultAsync(x => x.StockID == stockId, cancellationToken);
    }

    public async Task<(bool Ok, string? Error)> UpdateAsync(M_Stock incoming, CancellationToken cancellationToken = default)
    {
        try
        {
            var tracked = await context.Stock.FirstOrDefaultAsync(x => x.StockID == incoming.StockID, cancellationToken);
            if (tracked == null)
                return (false, "Không tìm thấy bản ghi.");

            tracked.Container = incoming.Container;
            tracked.Type = incoming.Type;
            tracked.IsoType = incoming.IsoType;
            tracked.Opr = incoming.Opr;
            tracked.FE = incoming.FE;
            tracked.Move = incoming.Move;
            tracked.DateIn = incoming.DateIn;
            tracked.TimeIn = incoming.TimeIn;
            tracked.Consignee = incoming.Consignee;
            tracked.Position = incoming.Position;
            tracked.Days = incoming.Days;
            tracked.Location = incoming.Location;
            tracked.YOM = incoming.YOM;
            tracked.TareWeight = incoming.TareWeight;
            tracked.SealNo = incoming.SealNo;
            tracked.Note2 = incoming.Note2;
            tracked.Note3 = incoming.Note3;
            tracked.VGM = incoming.VGM;
            tracked.MaxGross = incoming.MaxGross;
            tracked.Remark = incoming.Remark;
            tracked.Status = incoming.Status;
            tracked.Grade = incoming.Grade;
            tracked.CleanMethod = incoming.CleanMethod;
            tracked.CleanStatus = incoming.CleanStatus;
            tracked.PtiDate = incoming.PtiDate;
            tracked.PtiSetting = incoming.PtiSetting;
            tracked.PtiStatus = incoming.PtiStatus;
            tracked.EstimatedDate = incoming.EstimatedDate;
            tracked.ApprovalDate = incoming.ApprovalDate;
            tracked.RejectDate = incoming.RejectDate;
            tracked.RepairedDate = incoming.RepairedDate;
            tracked.RegisteredDate = incoming.RegisteredDate;
            tracked.Shipper = incoming.Shipper;
            tracked.Booking = incoming.Booking;

            await context.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(Guid stockId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await context.Stock.FindAsync(new object[] { stockId }, cancellationToken);
            if (entity == null)
                return (false, "Không tìm thấy bản ghi.");

            context.Stock.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<M_Stock>> GetFilteredAsync(StockListFilter filter, CancellationToken cancellationToken = default)
    {
        var q = context.Stock.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Location))
            q = q.Where(x => x.Location == filter.Location);

        if (!string.IsNullOrWhiteSpace(filter.ContainerNo))
        {
            var t = filter.ContainerNo.Trim();
            q = q.Where(x => x.Container != null && x.Container.Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(filter.Consignee))
        {
            var t = filter.Consignee.Trim();
            q = q.Where(x => x.Consignee != null && x.Consignee.Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(filter.Yom))
        {
            var t = filter.Yom.Trim();
            q = q.Where(x => x.YOM != null && x.YOM.Contains(t));
        }

        if (filter.DateInFrom.HasValue)
        {
            var from = filter.DateInFrom.Value.Date;
            q = q.Where(x => x.DateIn >= from);
        }

        if (filter.DateInTo.HasValue)
        {
            var toExclusive = filter.DateInTo.Value.Date.AddDays(1);
            q = q.Where(x => x.DateIn < toExclusive);
        }

        return await q.OrderByDescending(x => x.DateIn).ThenBy(x => x.Container).ToListAsync(cancellationToken);
    }

    public async Task<StockImportResult> ImportFromExcelAsync(Stream excelStream, string? userImport = null, CancellationToken cancellationToken = default)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);
            var sheet = package.Workbook.Worksheets[StockSheetName]
                        ?? package.Workbook.Worksheets.FirstOrDefault(w =>
                            string.Equals(w.Name, StockSheetName, StringComparison.OrdinalIgnoreCase));

            if (sheet == null)
                return new StockImportResult(0, 0, 0, $"Không tìm thấy sheet \"{StockSheetName}\".");

            var existingRows = await context.Stock.AsNoTracking().ToListAsync(cancellationToken);
            var fingerprintSet = existingRows.Select(BuildFingerprint).ToHashSet(StringComparer.Ordinal);

            var toAdd = new List<M_Stock>();
            var skippedDup = 0;
            var skippedEmpty = 0;

            var maxRow = sheet.Dimension?.End.Row ?? 0;
            if (maxRow < StockDataStartRow)
                return new StockImportResult(0, 0, 0, "Sheet không có dòng dữ liệu (từ dòng 20).");

            var lastDataRow = StockDataStartRow - 1;
            for (var r = StockDataStartRow; r <= maxRow; r++)
            {
                if (!string.IsNullOrWhiteSpace(sheet.Cells[r, 2].Text))
                    lastDataRow = r;
            }

            if (lastDataRow < StockDataStartRow)
                return new StockImportResult(0, 0, 0, "Không có dòng nào có số container (cột B) từ dòng 20.");

            for (var row = StockDataStartRow; row <= lastDataRow; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var container = TrimToNull(sheet.Cells[row, 2].Text);
                if (string.IsNullOrEmpty(container))
                {
                    skippedEmpty++;
                    continue;
                }

                var entity = ReadRow(sheet, row);
                var fp = BuildFingerprint(entity);
                // Trùng (DB hoặc đã thêm trong cùng file): bỏ qua — không insert, không gán DateImport/UserImport.
                if (fingerprintSet.Contains(fp))
                {
                    skippedDup++;
                    continue;
                }

                entity.StockID = Guid.NewGuid();
                entity.DateImport = DateTime.Now;
                entity.UserImport = string.IsNullOrWhiteSpace(userImport) ? null : userImport.Trim();
                toAdd.Add(entity);
                fingerprintSet.Add(fp);
            }

            if (toAdd.Count > 0)
            {
                await context.Stock.AddRangeAsync(toAdd, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            return new StockImportResult(toAdd.Count, skippedDup, skippedEmpty, null);
        }
        catch (Exception ex)
        {
            return new StockImportResult(0, 0, 0, ex.Message);
        }
    }

    /// <summary>Fingerprint trùng dữ liệu nghiệp vụ — không gồm DateImport/UserImport (metadata lần import).</summary>
    internal static string BuildFingerprint(M_Stock s)
    {
        var inv = CultureInfo.InvariantCulture;
        static string N(string? v) => (v ?? string.Empty).Trim();

        string D(DateTime? d) =>
            d.HasValue ? d.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", inv) : string.Empty;

        string T(TimeSpan? t) =>
            t.HasValue ? t.Value.ToString(@"hh\:mm\:ss\.fffffff", inv) : string.Empty;

        string Db(double? d) =>
            d.HasValue ? Math.Round(d.Value, 6).ToString("G", inv) : string.Empty;

        string I(int? i) => i.HasValue ? i.Value.ToString(inv) : string.Empty;

        return string.Join('\u001F', new[]
        {
            N(s.Container), N(s.Type), N(s.IsoType), N(s.Opr), N(s.FE), N(s.Move),
            D(s.DateIn), T(s.TimeIn),
            N(s.Consignee), N(s.Position), I(s.Days), N(s.Location), N(s.YOM),
            Db(s.TareWeight), N(s.SealNo), N(s.Note2), N(s.Note3),
            Db(s.VGM), Db(s.MaxGross),
            N(s.Remark), N(s.Status), N(s.Grade), N(s.CleanMethod), N(s.CleanStatus),
            D(s.PtiDate), N(s.PtiSetting), N(s.PtiStatus),
            D(s.EstimatedDate), D(s.ApprovalDate), D(s.RejectDate), D(s.RepairedDate), D(s.RegisteredDate),
            N(s.Shipper), N(s.Booking)
        });
    }

    private static M_Stock ReadRow(ExcelWorksheet sheet, int row)
    {
        return new M_Stock
        {
            Container = TrimToNull(sheet.Cells[row, 2].Text) ?? string.Empty,
            Type = TrimToNull(sheet.Cells[row, 3].Text),
            IsoType = TrimToNull(sheet.Cells[row, 4].Text),
            Opr = TrimToNull(sheet.Cells[row, 5].Text),
            FE = TrimToNull(sheet.Cells[row, 6].Text),
            Move = TrimToNull(sheet.Cells[row, 7].Text),
            DateIn = ReadDate(sheet.Cells[row, 8]),
            TimeIn = ReadTime(sheet.Cells[row, 9]),
            Consignee = TrimToNull(sheet.Cells[row, 10].Text),
            Position = TrimToNull(sheet.Cells[row, 11].Text),
            Days = ReadInt(sheet.Cells[row, 12]),
            Location = TrimToNull(sheet.Cells[row, 13].Text),
            YOM = TrimToNull(sheet.Cells[row, 14].Text),
            TareWeight = ReadDouble(sheet.Cells[row, 15]),
            SealNo = TrimToNull(sheet.Cells[row, 16].Text),
            Note2 = TrimToNull(sheet.Cells[row, 17].Text),
            Note3 = TrimToNull(sheet.Cells[row, 18].Text),
            VGM = ReadDouble(sheet.Cells[row, 19]),
            MaxGross = ReadDouble(sheet.Cells[row, 20]),
            Remark = TrimToNull(sheet.Cells[row, 21].Text),
            Status = TrimToNull(sheet.Cells[row, 22].Text),
            Grade = TrimToNull(sheet.Cells[row, 23].Text),
            CleanMethod = TrimToNull(sheet.Cells[row, 24].Text),
            CleanStatus = TrimToNull(sheet.Cells[row, 25].Text),
            PtiDate = ReadDate(sheet.Cells[row, 26]),
            PtiSetting = TrimToNull(sheet.Cells[row, 27].Text),
            PtiStatus = TrimToNull(sheet.Cells[row, 28].Text),
            EstimatedDate = ReadDate(sheet.Cells[row, 29]),
            ApprovalDate = ReadDate(sheet.Cells[row, 30]),
            RejectDate = ReadDate(sheet.Cells[row, 31]),
            RepairedDate = ReadDate(sheet.Cells[row, 32]),
            RegisteredDate = ReadDate(sheet.Cells[row, 33]),
            Shipper = TrimToNull(sheet.Cells[row, 34].Text),
            Booking = TrimToNull(sheet.Cells[row, 35].Text)
        };
    }

    private static string? TrimToNull(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;
        return s.Trim();
    }

    private static DateTime? ReadDate(ExcelRange cell)
    {
        var dt = cell.GetValue<DateTime?>();
        if (dt.HasValue)
            return dt.Value;

        var d = cell.GetValue<double?>();
        if (d.HasValue)
        {
            try
            {
                return DateTime.FromOADate(d.Value);
            }
            catch
            {
                /* ignore */
            }
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;

        if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed))
            return parsed;
        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            return parsed;

        return null;
    }

    private static TimeSpan? ReadTime(ExcelRange cell)
    {
        var dt = cell.GetValue<DateTime?>();
        if (dt.HasValue)
            return dt.Value.TimeOfDay;

        var d = cell.GetValue<double?>();
        if (d.HasValue)
        {
            try
            {
                return TimeSpan.FromDays(d.Value);
            }
            catch
            {
                /* ignore */
            }
        }

        var text = cell.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;

        if (TimeSpan.TryParse(text, CultureInfo.CurrentCulture, out var ts))
            return ts;
        if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out ts))
            return ts;

        return null;
    }

    private static int? ReadInt(ExcelRange cell)
    {
        var i = cell.GetValue<int?>();
        if (i.HasValue)
            return i;

        var d = cell.GetValue<double?>();
        if (d.HasValue)
            return (int)d.Value;

        var text = cell.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;

        return int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    private static double? ReadDouble(ExcelRange cell)
    {
        var text = cell.Text;
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            return result;

        return null;
    }
}
