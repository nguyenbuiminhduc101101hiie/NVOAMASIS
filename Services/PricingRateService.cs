using System.Data;
using System.Globalization;
using System.Text;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public record PricingRateImportResult(int Imported, string? ErrorMessage);
public record PricingRateParsePreview(List<M_PricingRate> Rows, string? ErrorMessage);
public record PricingRateImportProgress(string Stage, int Current, int Total, string? Detail);

/// <summary>
/// Reads "PRICING- EX HO CHI MINH-GOOGLE SHEET.xlsx" (7 fixed sheets, one per trade lane)
/// and exposes CRUD access to the single PricingRate table it feeds (rows are told apart
/// by TradeLane, see <see cref="PricingRateLane"/>).
/// The workbook has no reliable column headers (merged, blank or inconsistent across
/// sheets), so rows are read by fixed column position (B..AG) instead of by header name.
/// Import is split in two steps so the caller can show a preview before committing:
/// <see cref="ParseAsync"/> only reads the file, <see cref="CommitImportAsync"/> writes it.
/// Each import is tagged with its source file name (<see cref="M_PricingRate.SourceFileName"/>)
/// and is purely additive - it never touches rows from a different file. Re-importing the
/// same file name is rejected; the old data must be deleted first (see
/// <see cref="GetImportedFileBatchesAsync"/> / <see cref="DeleteImportedFileBatchesAsync"/> for
/// whole-file bulk delete, or <see cref="DeleteByFileAndLaneAsync"/> for a single sheet/lane).
/// </summary>
public class PricingRateService(AppDbContext context)
{
    private static readonly ImportedFileSource[] ImportedFileSources =
    {
        new("PricingRate", "SourceFileName", "DateImport", "UserImport")
    };

    static PricingRateService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task<List<M_PricingRate>> GetByLaneAsync(string tradeLane, CancellationToken ct = default) =>
        context.PricingRate.AsNoTracking()
            .Where(x => x.TradeLane == tradeLane)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<(bool Ok, string? Error)> UpsertAsync(M_PricingRate entity, CancellationToken ct = default)
    {
        try
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            var exists = await context.PricingRate.AsNoTracking().AnyAsync(x => x.Id == entity.Id, ct);
            if (exists)
                context.PricingRate.Update(entity);
            else
                await context.PricingRate.AddAsync(entity, ct);

            await context.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Ok, string? Error)> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.PricingRate.FindAsync(new object[] { id }, ct);
            if (entity == null)
                return (false, "Không tìm thấy bản ghi.");

            context.PricingRate.Remove(entity);
            await context.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<bool> FileNameExistsAsync(string fileName, CancellationToken ct = default) =>
        context.PricingRate.AsNoTracking().AnyAsync(x => x.SourceFileName == fileName, ct);

    /// <summary>Imported-file list for the shared <c>ImportedFileDeleteCard</c> component.</summary>
    public Task<List<ImportedFileBatch>> GetImportedFileBatchesAsync(CancellationToken ct = default) =>
        ImportedFileBatchHelper.LoadAsync(context.Database, ImportedFileSources, ct);

    /// <summary>Bulk-deletes every row belonging to any of the given imported files (all lanes).</summary>
    public Task<int> DeleteImportedFileBatchesAsync(IEnumerable<ImportedFileBatch> batches, CancellationToken ct = default) =>
        ImportedFileBatchHelper.DeleteAsync(context.Database, ImportedFileSources, batches, ct);

    /// <summary>Distinct file names currently in the table, for the "delete one lane" picker.</summary>
    public Task<List<string>> GetDistinctFileNamesAsync(CancellationToken ct = default) =>
        context.PricingRate.AsNoTracking()
            .Where(x => x.SourceFileName != null)
            .Select(x => x.SourceFileName!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);

    /// <summary>Deletes just one lane's rows belonging to one imported file.</summary>
    public async Task<(bool Ok, string? Error)> DeleteByFileAndLaneAsync(string fileName, string tradeLane, CancellationToken ct = default)
    {
        try
        {
            var deleted = await context.PricingRate.Where(x => x.SourceFileName == fileName && x.TradeLane == tradeLane).ExecuteDeleteAsync(ct);
            if (deleted == 0)
                return (false, "Không tìm thấy dữ liệu của tuyến này trong file đã chọn.");
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>Reads the workbook into memory only; nothing is written to the database.</summary>
    public async Task<PricingRateParsePreview> ParseAsync(Stream stream, string fileName, Func<PricingRateImportProgress, Task>? onProgress = null, CancellationToken ct = default)
    {
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });

            if (ds.Tables.Count < PricingRateLane.All.Count)
            {
                return new PricingRateParsePreview(new List<M_PricingRate>(),
                    $"File chỉ có {ds.Tables.Count} sheet, cần đủ {PricingRateLane.All.Count} sheet (đúng thứ tự gốc).");
            }

            var importedAt = DateTime.UtcNow;
            var rows = new List<M_PricingRate>();
            for (var i = 0; i < PricingRateLane.All.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var lane = PricingRateLane.All[i];
                rows.AddRange(ParseSheet(ds.Tables[i], lane.Code, fileName, importedAt));

                if (onProgress != null)
                    await onProgress(new PricingRateImportProgress("parse", i + 1, PricingRateLane.All.Count, lane.Code));
            }

            return new PricingRateParsePreview(rows, null);
        }
        catch (Exception ex)
        {
            return new PricingRateParsePreview(new List<M_PricingRate>(), ex.Message);
        }
    }

    /// <summary>
    /// Writes previously-parsed rows (see <see cref="ParseAsync"/>). Purely additive: only
    /// inserts, never deletes or touches rows from a different <c>SourceFileName</c>. Fails
    /// if the file name was already imported (delete it first via <see cref="DeleteByFileNamesAsync"/>).
    /// </summary>
    public async Task<PricingRateImportResult> CommitImportAsync(List<M_PricingRate> rows, string fileName, string? importUser, Func<PricingRateImportProgress, Task>? onProgress = null, CancellationToken ct = default)
    {
        try
        {
            if (await FileNameExistsAsync(fileName, ct))
                return new PricingRateImportResult(0, $"File '{fileName}' đã được import trước đó. Hãy xoá dữ liệu cũ của file này (mục Quản lý dữ liệu đã nhập) trước khi nhập lại.");

            var importedAt = DateTime.UtcNow;
            foreach (var row in rows)
            {
                row.SourceFileName = fileName;
                row.UserImport = importUser;
                row.DateImport = importedAt;
                row.CreatedAt = importedAt;
            }

            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var byLane = rows.ToLookup(x => x.TradeLane);
            for (var i = 0; i < PricingRateLane.All.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var lane = PricingRateLane.All[i];
                var laneRows = byLane[lane.Code].ToList();
                if (laneRows.Count > 0)
                    await context.PricingRate.AddRangeAsync(laneRows, ct);

                if (onProgress != null)
                    await onProgress(new PricingRateImportProgress("save", i + 1, PricingRateLane.All.Count, lane.Code));
            }

            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return new PricingRateImportResult(rows.Count, null);
        }
        catch (Exception ex)
        {
            return new PricingRateImportResult(0, ex.Message);
        }
    }

    private static IEnumerable<M_PricingRate> ParseSheet(DataTable table, string tradeLane, string fileName, DateTime importedAt)
    {
        // Row 1-2 are the (merged / partly blank) header rows; real data starts at row 3.
        for (var r = 2; r < table.Rows.Count; r++)
        {
            var row = table.Rows[r];

            var carrier = ReadString(row, 3);
            var pol = ReadString(row, 5);
            var pod = ReadString(row, 6);
            if (string.IsNullOrWhiteSpace(carrier) && string.IsNullOrWhiteSpace(pol) && string.IsNullOrWhiteSpace(pod))
                continue; // blank trailing row

            yield return new M_PricingRate
            {
                Id = Guid.NewGuid(),
                TradeLane = tradeLane,
                SourceFileName = fileName,
                ExtraNote = ReadString(row, 0),
                EffDate = ReadDate(row, 1),
                ValidDate = ReadDate(row, 2),
                Carrier = carrier,
                Country = ReadString(row, 4),
                Pol = pol,
                Pod = pod,
                OfRateCom20 = ReadDecimal(row, 7),
                OfRateCom40 = ReadDecimal(row, 8),
                OfRateCom40Hc = ReadDecimal(row, 9),
                CommissionHdl20 = ReadDecimal(row, 10),
                CommissionHdl40 = ReadDecimal(row, 11),
                CommissionHdl40Hc = ReadDecimal(row, 12),
                Remark = ReadString(row, 13),
                FreeTimeAtPod = ReadString(row, 14),
                BasicOfNet20 = ReadDecimal(row, 15),
                BasicOfNet40 = ReadDecimal(row, 16),
                BasicOfNet40Hc = ReadDecimal(row, 17),
                SurchargeFuelEnv20 = ReadDecimal(row, 18),
                SurchargeFuelEnv40 = ReadDecimal(row, 19),
                SurchargeFuelEnv40Hc = ReadDecimal(row, 20),
                SurchargeMisc20 = ReadDecimal(row, 21),
                SurchargeMisc40 = ReadDecimal(row, 22),
                SurchargeMisc40Hc = ReadDecimal(row, 23),
                SurchargeSecurityEnv20 = ReadDecimal(row, 24),
                SurchargeSecurityEnv40 = ReadDecimal(row, 25),
                SurchargeSecurityEnv40Hc = ReadDecimal(row, 26),
                TotalOf20 = ReadDecimal(row, 27),
                TotalOf40 = ReadDecimal(row, 28),
                TotalOf40Hc = ReadDecimal(row, 29),
                VatCom20 = ReadDecimal(row, 30),
                VatCom40 = ReadDecimal(row, 31),
                VatCom40Hc = ReadDecimal(row, 32),
                DateImport = importedAt,
                CreatedAt = importedAt
            };
        }
    }

    private static string? ReadString(DataRow row, int colIndex)
    {
        if (colIndex >= row.ItemArray.Length)
            return null;
        var value = row[colIndex];
        if (value == null || value is DBNull)
            return null;
        var text = value is DateTime dt ? dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : value.ToString();
        text = text?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static DateTime? ReadDate(DataRow row, int colIndex)
    {
        if (colIndex >= row.ItemArray.Length)
            return null;
        var value = row[colIndex];
        if (value == null || value is DBNull)
            return null;
        if (value is DateTime dt)
            return dt;

        var text = value.ToString();
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

    private static decimal? ReadDecimal(DataRow row, int colIndex)
    {
        if (colIndex >= row.ItemArray.Length)
            return null;
        var value = row[colIndex];
        if (value == null || value is DBNull)
            return null;
        if (value is double d)
            return (decimal)d;
        if (value is decimal m)
            return m;

        var text = value.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
            return inv;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var cur))
            return cur;
        return null;
    }
}
