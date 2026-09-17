using System.Data;
using System.Globalization;
using System.Text;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public record AirFreightRateImportResult(int Imported, string? ErrorMessage);
public record AirFreightRateParsePreview(List<M_AirFreightRate> Rows, string? ErrorMessage);
public record AirFreightRateImportProgress(int Current, int Total);

/// <summary>
/// Reads an air-freight quotation workbook (Airlines / Destination / Min / -45kg / +45kg /
/// +100kg / +300kg / +500kg / +1000kg / FSC &amp; WSC / FRE / ROUTE / TT / Surcharges / Note)
/// and exposes CRUD access to the single AirFreightRate table it feeds. Only the first sheet
/// is read; columns are matched by header name (case-insensitive), not by fixed position, so
/// re-ordered or slightly renamed columns still import correctly.
/// Import is split in two steps so the caller can show a preview before committing:
/// <see cref="ParseAsync"/> only reads the file, <see cref="CommitImportAsync"/> writes it.
/// Each import is tagged with its source file name (<see cref="M_AirFreightRate.SourceFileName"/>)
/// and is purely additive. Re-importing the same file name is rejected; the old data must be
/// deleted first (see <see cref="GetImportedFileBatchesAsync"/> / <see cref="DeleteImportedFileBatchesAsync"/>).
/// </summary>
public class AirFreightRateService(AppDbContext context)
{
    private static readonly ImportedFileSource[] ImportedFileSources =
    {
        new("AirFreightRate", "SourceFileName", "DateImport", "UserImport")
    };

    static AirFreightRateService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public Task<List<M_AirFreightRate>> GetAllAsync(CancellationToken ct = default) =>
        context.AirFreightRate.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<(bool Ok, string? Error)> UpsertAsync(M_AirFreightRate entity, CancellationToken ct = default)
    {
        try
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            var exists = await context.AirFreightRate.AsNoTracking().AnyAsync(x => x.Id == entity.Id, ct);
            if (exists)
                context.AirFreightRate.Update(entity);
            else
                await context.AirFreightRate.AddAsync(entity, ct);

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
            var entity = await context.AirFreightRate.FindAsync(new object[] { id }, ct);
            if (entity == null)
                return (false, "Không tìm thấy bản ghi.");

            context.AirFreightRate.Remove(entity);
            await context.SaveChangesAsync(ct);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Task<bool> FileNameExistsAsync(string fileName, CancellationToken ct = default) =>
        context.AirFreightRate.AsNoTracking().AnyAsync(x => x.SourceFileName == fileName, ct);

    /// <summary>Imported-file list for the shared <c>ImportedFileDeleteCard</c> component.</summary>
    public Task<List<ImportedFileBatch>> GetImportedFileBatchesAsync(CancellationToken ct = default) =>
        ImportedFileBatchHelper.LoadAsync(context.Database, ImportedFileSources, ct);

    /// <summary>Bulk-deletes every row belonging to any of the given imported files.</summary>
    public Task<int> DeleteImportedFileBatchesAsync(IEnumerable<ImportedFileBatch> batches, CancellationToken ct = default) =>
        ImportedFileBatchHelper.DeleteAsync(context.Database, ImportedFileSources, batches, ct);

    /// <summary>Reads the workbook into memory only; nothing is written to the database.</summary>
    public async Task<AirFreightRateParsePreview> ParseAsync(Stream stream, string fileName, Func<AirFreightRateImportProgress, Task>? onProgress = null, CancellationToken ct = default)
    {
        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var ds = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
            });

            var table = ds.Tables.Cast<DataTable>().FirstOrDefault();
            if (table == null || table.Rows.Count == 0)
                return new AirFreightRateParsePreview(new List<M_AirFreightRate>(), "File không có dữ liệu.");

            var headers = table.Rows[0].ItemArray.Select(x => NormalizeHeader(x?.ToString())).ToList();
            var map = BuildHeaderMap(headers);

            var importedAt = DateTime.UtcNow;
            var rows = new List<M_AirFreightRate>();
            var totalDataRows = table.Rows.Count - 1;
            for (var i = 1; i < table.Rows.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var row = table.Rows[i];

                var airlines = ReadString(row, map, "Airlines", "Airline");
                var destination = ReadString(row, map, "Destination");
                var route = ReadString(row, map, "ROUTE");
                var surcharges = ReadString(row, map, "Surcharges", "Surcharge");
                var note = ReadString(row, map, "Note", "Notes");
                if (string.IsNullOrWhiteSpace(airlines) && string.IsNullOrWhiteSpace(destination)
                    && string.IsNullOrWhiteSpace(route) && string.IsNullOrWhiteSpace(surcharges) && string.IsNullOrWhiteSpace(note))
                    continue; // blank trailing row

                rows.Add(new M_AirFreightRate
                {
                    Id = Guid.NewGuid(),
                    SourceFileName = fileName,
                    Airlines = airlines,
                    Destination = destination,
                    Min = ReadDecimal(row, map, "Min"),
                    RateUnder45 = ReadDecimal(row, map, "-45kg", "-45KG"),
                    RateOver45 = ReadDecimal(row, map, "+45kg", "+45KG"),
                    RateOver100 = ReadDecimal(row, map, "+100kg", "+100KG"),
                    RateOver300 = ReadDecimal(row, map, "+300kg", "+300KG"),
                    RateOver500 = ReadDecimal(row, map, "+500kg", "+500KG"),
                    RateOver1000 = ReadDecimal(row, map, "+1000kg", "+1000KG"),
                    FscWsc = ReadString(row, map, "FSC & WSC", "FSC&WSC", "FSC AND WSC"),
                    Frequency = ReadString(row, map, "FRE", "Frequency"),
                    Route = route,
                    TransitTime = ReadString(row, map, "TT", "Transit Time"),
                    Surcharges = surcharges,
                    Note = note,
                    DateImport = importedAt,
                    CreatedAt = importedAt
                });

                if (onProgress != null)
                    await onProgress(new AirFreightRateImportProgress(i, totalDataRows));
            }

            return new AirFreightRateParsePreview(rows, null);
        }
        catch (Exception ex)
        {
            return new AirFreightRateParsePreview(new List<M_AirFreightRate>(), ex.Message);
        }
    }

    /// <summary>
    /// Writes previously-parsed rows (see <see cref="ParseAsync"/>). Purely additive: only
    /// inserts, never deletes or touches rows from a different <c>SourceFileName</c>. Fails
    /// if the file name was already imported (delete it first via <see cref="DeleteImportedFileBatchesAsync"/>).
    /// </summary>
    public async Task<AirFreightRateImportResult> CommitImportAsync(List<M_AirFreightRate> rows, string fileName, string? importUser, CancellationToken ct = default)
    {
        try
        {
            if (await FileNameExistsAsync(fileName, ct))
                return new AirFreightRateImportResult(0, $"File '{fileName}' đã được import trước đó. Hãy xoá dữ liệu cũ của file này (mục Quản lý dữ liệu đã nhập) trước khi nhập lại.");

            var importedAt = DateTime.UtcNow;
            foreach (var row in rows)
            {
                row.SourceFileName = fileName;
                row.UserImport = importUser;
                row.DateImport = importedAt;
                row.CreatedAt = importedAt;
            }

            await context.AirFreightRate.AddRangeAsync(rows, ct);
            await context.SaveChangesAsync(ct);

            return new AirFreightRateImportResult(rows.Count, null);
        }
        catch (Exception ex)
        {
            return new AirFreightRateImportResult(0, ex.Message);
        }
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

    private static decimal? ReadDecimal(DataRow row, IReadOnlyDictionary<string, int> map, params string[] aliases)
    {
        var text = ReadString(row, map, aliases);
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
            return inv;
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var cur))
            return cur;
        return null; // e.g. "NIL" - not a rate, treated as not applicable
    }
}
