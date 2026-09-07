using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ExcelDataReader;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public sealed class DepotHaiPhongPreview
{
    public string FileName { get; set; } = string.Empty;
    public List<DepotHaiPhongSheetPreview> Sheets { get; } = new();
    public int TotalRows => Sheets.Sum(x => x.Rows.Count);
    public int ImportableRows => Sheets.Sum(x => x.Rows.Count(r => r.CanImport));
    public int ExistingRows => Sheets.Sum(x => x.Rows.Count(r => r.IsExisting));
    public bool HasImportableRows => ImportableRows > 0;
}

public sealed class DepotHaiPhongSheetPreview
{
    public string SheetName { get; set; } = string.Empty;
    public List<string> Headers { get; } = new();
    public List<DepotHaiPhongPreviewRow> Rows { get; } = new();
}

public sealed class DepotHaiPhongPreviewRow
{
    public int ExcelRow { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public string? SectionName { get; set; }
    public string? ContainerNo { get; set; }
    public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> BySqlColumn { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Extra { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string Signature { get; set; } = string.Empty;
    public bool IsExisting { get; set; }
    public bool IsBatchDuplicate { get; set; }
    public bool IsInvalid { get; set; }
    public string? StatusMessage { get; set; }
    public bool CanImport => !IsInvalid && !IsExisting && !IsBatchDuplicate && !string.IsNullOrWhiteSpace(ContainerNo);
}

public sealed class DepotHaiPhongSavedRow
{
    public Dictionary<string, string> Values { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DepotHaiPhongImportService
{
    private static readonly HashSet<string> ContainerKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "CONTAINERNO", "CONTAINER", "SOCONT", "SOCONTAINER", "ITEMNO", "CONTRNO",
        "CNTRNO", "SOCONTAINE", "CONTAINERNUMBER"
    };

    private static readonly HashSet<string> DateHeaderHints = new(StringComparer.OrdinalIgnoreCase)
    {
        "DATE", "GATEIN", "GATEOUT", "NGAY", "TIME", "FULIMP", "TRIMP", "EXECTS",
        "ARRTS", "DEPTS", "INSDATE", "UNSTUFF", "STUFF"
    };

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    static DepotHaiPhongImportService()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public DepotHaiPhongImportService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public DepotHaiPhongPreview ReadPreview(byte[] bytes, string fileName, DepotHaiPhongReport report)
    {
        bytes = NormalizeExcelBytes(bytes);
        using var ms = new MemoryStream(bytes);
        using var reader = ExcelReaderFactory.CreateReader(ms);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false,
                EmptyColumnNamePrefix = "Column"
            }
        });

        var preview = new DepotHaiPhongPreview { FileName = fileName };
        foreach (DataTable table in dataSet.Tables)
        {
            if (ShouldSkipSheet(report, table.TableName))
                continue;

            var sheet = report.UseSectionParser
                ? ReadSectionSheet(table, report)
                : ReadStandardSheet(table, report);

            if (sheet.Rows.Count > 0 || sheet.Headers.Count > 0)
                preview.Sheets.Add(sheet);
        }

        return preview;
    }

    public void ComputeSignatures(DepotHaiPhongPreview preview, DepotHaiPhongReport report, string depot)
    {
        foreach (var row in preview.Sheets.SelectMany(x => x.Rows))
            row.Signature = ComputeSignature(report, depot, row);
    }

    public async Task MarkDuplicatesAsync(DepotHaiPhongPreview preview, DepotHaiPhongReport report, CancellationToken ct = default)
    {
        var fileSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var signatures = preview.Sheets.SelectMany(x => x.Rows)
            .Select(x => x.Signature)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await LoadExistingSignaturesAsync(report.TableName, signatures, ct);

        foreach (var row in preview.Sheets.SelectMany(x => x.Rows))
        {
            if (string.IsNullOrWhiteSpace(row.ContainerNo))
            {
                row.IsInvalid = true;
                row.StatusMessage = "Thiếu số container.";
                continue;
            }

            if (existing.Contains(row.Signature))
            {
                row.IsExisting = true;
                row.StatusMessage = "Đã có trong database.";
                continue;
            }

            if (!fileSeen.Add(row.Signature))
            {
                row.IsBatchDuplicate = true;
                row.StatusMessage = "Trùng dòng khác trong cùng file.";
            }
        }
    }

    public async Task EnsureTableExistsAsync(DepotHaiPhongReport report, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var check = conn.CreateCommand();
        check.CommandText = $"SELECT CASE WHEN OBJECT_ID(N'dbo.[{report.TableName}]', N'U') IS NULL THEN 0 ELSE 1 END;";
        var exists = Convert.ToInt32(await check.ExecuteScalarAsync(ct));
        if (exists != 1)
        {
            throw new InvalidOperationException(
                $"Bảng dbo.[{report.TableName}] chưa tồn tại. Hãy chạy Scripts/Create_8_3_9_Depot_Hai_Phong.sql bằng tài khoản DBA rồi import lại.");
        }
    }

    public async Task<int> InsertAsync(
        DepotHaiPhongPreview preview,
        DepotHaiPhongReport report,
        string depot,
        Guid? depotId,
        string user,
        CancellationToken ct = default)
    {
        var rows = preview.Sheets.SelectMany(x => x.Rows).Where(x => x.CanImport).ToList();
        if (rows.Count == 0)
            return 0;

        await EnsureTableExistsAsync(report, ct);

        var inserted = 0;
        var now = DateTime.Now;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var tran = await db.Database.BeginTransactionAsync(ct);
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        var table = ImportedFileBatchHelper.QuoteName(report.TableName);
        var dataCols = report.Columns.ToList();
        var insertCols = new List<string>
        {
            "Id", "Depot", "DepotId", "SOURCE_FILE", "SHEET_NAME", "SECTION_NAME", "EXCEL_ROW",
            "ContainerNo", "ExtraJson", "ROW_SIGNATURE", "DATE_IMPORT", "USER_IMPORT", "CREATED_AT"
        };
        insertCols.AddRange(dataCols);

        var colSql = string.Join(", ", insertCols.Select(ImportedFileBatchHelper.QuoteName));
        var paramSql = string.Join(", ", insertCols.Select(c => "@" + SafeParam(c)));

        foreach (var row in rows)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
            cmd.CommandText = $@"
IF NOT EXISTS (SELECT 1 FROM dbo.{table} WITH (UPDLOCK, HOLDLOCK) WHERE ROW_SIGNATURE = @ROW_SIGNATURE)
INSERT INTO dbo.{table} ({colSql})
VALUES ({paramSql});";

            Add(cmd, "@Id", Guid.NewGuid());
            Add(cmd, "@Depot", depot);
            Add(cmd, "@DepotId", depotId is Guid id && id != Guid.Empty ? id : null);
            Add(cmd, "@SOURCE_FILE", preview.FileName);
            Add(cmd, "@SHEET_NAME", row.SheetName);
            Add(cmd, "@SECTION_NAME", row.SectionName);
            Add(cmd, "@EXCEL_ROW", row.ExcelRow);
            Add(cmd, "@ContainerNo", row.ContainerNo);
            Add(cmd, "@ExtraJson", row.Extra.Count == 0 ? null : JsonSerializer.Serialize(row.Extra));
            Add(cmd, "@ROW_SIGNATURE", row.Signature);
            Add(cmd, "@DATE_IMPORT", now);
            Add(cmd, "@USER_IMPORT", user);
            Add(cmd, "@CREATED_AT", now);

            foreach (var col in dataCols)
            {
                row.BySqlColumn.TryGetValue(col, out var value);
                Add(cmd, "@" + SafeParam(col), Truncate(value, col.Contains("Remark", StringComparison.OrdinalIgnoreCase) || col.Contains("Note", StringComparison.OrdinalIgnoreCase) || col.Contains("Ghi", StringComparison.OrdinalIgnoreCase) ? 2000 : 400));
            }

            inserted += await cmd.ExecuteNonQueryAsync(ct);
        }

        await tran.CommitAsync(ct);
        return inserted;
    }

    public async Task<List<DepotHaiPhongSavedRow>> LoadSavedAsync(DepotHaiPhongReport report, int take = 300, CancellationToken ct = default)
    {
        var result = new List<DepotHaiPhongSavedRow>();
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        if (!await TableExistsAsync(conn, report.TableName, ct))
            return result;

        var table = ImportedFileBatchHelper.QuoteName(report.TableName);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT TOP ({take}) * FROM dbo.{table} ORDER BY DATE_IMPORT DESC, CREATED_AT DESC;";
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var row = new DepotHaiPhongSavedRow();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (name.Equals("Id", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("DepotId", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("ExtraJson", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("ROW_SIGNATURE", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = reader.IsDBNull(i) ? string.Empty : Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture) ?? string.Empty;
                row.Values[name] = value;
            }
            result.Add(row);
        }

        return result;
    }

    public async Task<int> CountSavedAsync(DepotHaiPhongReport report, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);
        if (!await TableExistsAsync(conn, report.TableName, ct))
            return 0;

        var table = ImportedFileBatchHelper.QuoteName(report.TableName);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT_BIG(*) FROM dbo.{table};";
        var value = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(value);
    }

    public async Task<List<ImportedFileBatch>> LoadImportedFilesAsync(DepotHaiPhongReport report, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await ImportedFileBatchHelper.LoadAsync(db.Database, FileSources(report), ct);
    }

    public async Task<int> DeleteSelectedFilesAsync(DepotHaiPhongReport report, IEnumerable<ImportedFileBatch> batches, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await ImportedFileBatchHelper.DeleteAsync(db.Database, FileSources(report), batches, ct);
    }

    public async Task<int> DeleteAllAsync(DepotHaiPhongReport report, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Database.ExecuteSqlRawAsync(
            $"IF OBJECT_ID(N'dbo.[{report.TableName}]', N'U') IS NOT NULL DELETE FROM dbo.[{report.TableName}];");
    }

    private static ImportedFileSource[] FileSources(DepotHaiPhongReport report) =>
        new[] { new ImportedFileSource(report.TableName, "SOURCE_FILE", "DATE_IMPORT", "USER_IMPORT") };

    private DepotHaiPhongSheetPreview ReadStandardSheet(DataTable table, DepotHaiPhongReport report)
    {
        var sheet = new DepotHaiPhongSheetPreview { SheetName = table.TableName };
        if (table.Rows.Count == 0)
            return sheet;

        var headerRowIndex = FindHeaderRow(table);
        if (headerRowIndex < 0)
            return sheet;

        var headers = ReadHeaders(table, headerRowIndex);
        sheet.Headers.AddRange(headers.Where(h => !string.IsNullOrWhiteSpace(h)));

        for (var r = headerRowIndex + 1; r < table.Rows.Count; r++)
        {
            var dataRow = table.Rows[r];
            if (IsEmptyRow(dataRow) || IsStopRow(dataRow))
                continue;

            var previewRow = MapDataRow(table, dataRow, headers, r + 1, table.TableName, null, report);
            if (previewRow is null)
                continue;
            sheet.Rows.Add(previewRow);
        }

        return sheet;
    }

    private DepotHaiPhongSheetPreview ReadSectionSheet(DataTable table, DepotHaiPhongReport report)
    {
        var sheet = new DepotHaiPhongSheetPreview { SheetName = table.TableName };
        string? section = null;
        List<string>? headers = null;
        var skipSection = false;

        for (var r = 0; r < table.Rows.Count; r++)
        {
            var dataRow = table.Rows[r];
            var first = FirstCellText(dataRow);
            var sectionName = DetectSection(first);
            if (sectionName is not null)
            {
                section = sectionName;
                headers = null;
                skipSection = section.Contains("SUMMARY", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (skipSection || string.IsNullOrWhiteSpace(section))
                continue;

            if (headers is null)
            {
                if (LooksLikeHeaderRow(dataRow))
                {
                    headers = ReadHeaders(table, r);
                    foreach (var h in headers.Where(x => !string.IsNullOrWhiteSpace(x) && !sheet.Headers.Contains(x, StringComparer.OrdinalIgnoreCase)))
                        sheet.Headers.Add(h);
                }
                continue;
            }

            if (IsEmptyRow(dataRow) || IsStopRow(dataRow))
            {
                if (IsStopRow(dataRow))
                    headers = null;
                continue;
            }

            var previewRow = MapDataRow(table, dataRow, headers, r + 1, table.TableName, section, report);
            if (previewRow is null)
                continue;
            sheet.Rows.Add(previewRow);
        }

        return sheet;
    }

    private DepotHaiPhongPreviewRow? MapDataRow(
        DataTable table,
        DataRow dataRow,
        List<string> headers,
        int excelRow,
        string sheetName,
        string? section,
        DepotHaiPhongReport report)
    {
        var sqlByKey = report.Columns
            .GroupBy(DepotHaiPhongCatalog.NormalizeKey)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var row = new DepotHaiPhongPreviewRow
        {
            ExcelRow = excelRow,
            SheetName = sheetName,
            SectionName = section
        };

        for (var c = 0; c < headers.Count && c < table.Columns.Count; c++)
        {
            var header = headers[c];
            if (string.IsNullOrWhiteSpace(header))
                continue;

            var text = FormatValue(dataRow[c], header);
            row.Values[header] = text;

            var key = DepotHaiPhongCatalog.NormalizeKey(header);
            if (sqlByKey.TryGetValue(key, out var sqlCol))
                row.BySqlColumn[sqlCol] = text;
            else if (!string.IsNullOrWhiteSpace(text))
                row.Extra[header] = text;
        }

        row.ContainerNo = ExtractContainer(row);
        if (string.IsNullOrWhiteSpace(row.ContainerNo))
            return null;

        return row;
    }

    private static string? ExtractContainer(DepotHaiPhongPreviewRow row)
    {
        foreach (var pair in row.Values)
        {
            var key = DepotHaiPhongCatalog.NormalizeKey(pair.Key);
            if (!ContainerKeys.Contains(key) && key != "ITEMKEY")
                continue;
            var value = NormalizeContainer(pair.Value);
            if (!string.IsNullOrWhiteSpace(value) && value.Length >= 4)
                return value;
        }

        foreach (var pair in row.BySqlColumn)
        {
            var key = DepotHaiPhongCatalog.NormalizeKey(pair.Key);
            if (!ContainerKeys.Contains(key))
                continue;
            var value = NormalizeContainer(pair.Value);
            if (!string.IsNullOrWhiteSpace(value) && value.Length >= 4)
                return value;
        }

        return null;
    }

    private static string NormalizeContainer(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return Regex.Replace(value.Trim().ToUpperInvariant(), @"\s+", string.Empty);
    }

    private static List<string> ReadHeaders(DataTable table, int headerRowIndex)
    {
        var headers = new List<string>();
        for (var col = 0; col < table.Columns.Count; col++)
        {
            var header = Convert.ToString(table.Rows[headerRowIndex][col], CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(header))
                header = string.Empty;
            headers.Add(MakeUniqueHeader(headers, header));
        }
        return headers;
    }

    private static int FindHeaderRow(DataTable table)
    {
        var limit = Math.Min(table.Rows.Count, 40);
        var best = -1;
        var bestScore = 0;

        for (var row = 0; row < limit; row++)
        {
            var filled = 0;
            var hasContainer = false;
            for (var col = 0; col < table.Columns.Count; col++)
            {
                var text = Convert.ToString(table.Rows[row][col], CultureInfo.InvariantCulture)?.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                filled++;
                var key = DepotHaiPhongCatalog.NormalizeKey(text);
                if (ContainerKeys.Contains(key))
                    hasContainer = true;
            }

            if (filled < 3)
                continue;

            var score = filled + (hasContainer ? 100 : 0);
            if (score > bestScore)
            {
                bestScore = score;
                best = row;
            }

            if (hasContainer && filled >= 5)
                return row;
        }

        return best;
    }

    private static bool LooksLikeHeaderRow(DataRow row)
    {
        var filled = 0;
        var hasSeqOrContainer = false;
        foreach (var item in row.ItemArray)
        {
            var text = Convert.ToString(item, CultureInfo.InvariantCulture)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                continue;
            filled++;
            var key = DepotHaiPhongCatalog.NormalizeKey(text);
            if (key is "SEQ" or "SQ" or "NO" || ContainerKeys.Contains(key))
                hasSeqOrContainer = true;
        }
        return filled >= 3 && hasSeqOrContainer;
    }

    private static string? DetectSection(string first)
    {
        if (string.IsNullOrWhiteSpace(first))
            return null;
        var upper = first.ToUpperInvariant();
        if (upper.Contains("TURN IN"))
            return "TURN IN";
        if (upper.Contains("TURN OUT"))
            return "TURN OUT";
        if (upper.Contains("REPAIR"))
            return "REPAIR COMPLETE";
        if (upper.Contains("INVENTORY") && !upper.Contains("SUMMARY"))
            return "INVENTORY";
        if (upper.Contains("SUMMARY"))
            return "SUMMARY";
        return null;
    }

    private static bool ShouldSkipSheet(DepotHaiPhongReport report, string sheetName)
    {
        return report.SkipSheets.Any(x =>
            string.Equals(x, sheetName, StringComparison.OrdinalIgnoreCase)
            || sheetName.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsStopRow(DataRow row)
    {
        var first = FirstCellText(row);
        if (string.IsNullOrWhiteSpace(first))
            return false;
        var upper = first.Trim().ToUpperInvariant();
        return upper.StartsWith("TOTAL") || upper.StartsWith("SUM") || upper.Equals("SIZE/TYLE") || upper.Equals("SIZETYPE");
    }

    private static bool IsEmptyRow(DataRow row) =>
        row.ItemArray.All(x => x is null || x == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(x)));

    private static string FirstCellText(DataRow row)
    {
        foreach (var item in row.ItemArray)
        {
            var text = Convert.ToString(item, CultureInfo.InvariantCulture)?.Trim();
            if (!string.IsNullOrWhiteSpace(text))
                return text;
        }
        return string.Empty;
    }

    private static string FormatValue(object? value, string header)
    {
        if (value is null || value == DBNull.Value)
            return string.Empty;

        if (value is DateTime dt)
            return dt.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        if (value is double d)
        {
            var key = DepotHaiPhongCatalog.NormalizeKey(header);
            if (d > 20000 && d < 80000 && LooksLikeDate(key))
            {
                try
                {
                    return DateTime.FromOADate(d).ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                }
                catch
                {
                    // fall through
                }
            }

            if (Math.Abs(d - Math.Round(d)) < 0.0000001)
                return Convert.ToInt64(Math.Round(d)).ToString(CultureInfo.InvariantCulture);
            return d.ToString("0.####", CultureInfo.InvariantCulture);
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
    }

    private static bool LooksLikeDate(string key) =>
        DateHeaderHints.Any(h => key.Contains(h, StringComparison.OrdinalIgnoreCase));

    private static string MakeUniqueHeader(List<string> existing, string header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return header;
        if (!existing.Contains(header, StringComparer.OrdinalIgnoreCase))
            return header;
        var i = 2;
        var candidate = $"{header}_{i}";
        while (existing.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            i++;
            candidate = $"{header}_{i}";
        }
        return candidate;
    }

    private static string ComputeSignature(DepotHaiPhongReport report, string depot, DepotHaiPhongPreviewRow row)
    {
        var sb = new StringBuilder();
        sb.Append(report.TableName).Append('|')
            .Append(depot.Trim().ToUpperInvariant()).Append('|')
            .Append(row.SheetName).Append('|')
            .Append(row.SectionName).Append('|');
        foreach (var col in report.Columns)
        {
            row.BySqlColumn.TryGetValue(col, out var value);
            sb.Append(col).Append('=').Append(value?.Trim().ToUpperInvariant()).Append('|');
        }
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes);
    }

    private async Task<HashSet<string>> LoadExistingSignaturesAsync(string tableName, List<string> signatures, CancellationToken ct)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (signatures.Count == 0)
            return result;

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);
        if (!await TableExistsAsync(conn, tableName, ct))
            return result;

        var table = ImportedFileBatchHelper.QuoteName(tableName);
        foreach (var chunk in Chunk(signatures, 200))
        {
            await using var cmd = conn.CreateCommand();
            var names = new List<string>();
            for (var i = 0; i < chunk.Count; i++)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = "@s" + i;
                p.Value = chunk[i];
                cmd.Parameters.Add(p);
                names.Add(p.ParameterName);
            }

            cmd.CommandText = $"SELECT ROW_SIGNATURE FROM dbo.{table} WITH (NOLOCK) WHERE ROW_SIGNATURE IN ({string.Join(",", names)});";
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add(reader.GetString(0));
        }

        return result;
    }

    private static async Task<bool> TableExistsAsync(DbConnection conn, string tableName, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN OBJECT_ID(@FullName, N'U') IS NULL THEN 0 ELSE 1 END;";
        var p = cmd.CreateParameter();
        p.ParameterName = "@FullName";
        p.Value = "dbo." + ImportedFileBatchHelper.QuoteName(tableName);
        cmd.Parameters.Add(p);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) == 1;
    }

    private static byte[] NormalizeExcelBytes(byte[] bytes)
    {
        if (bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B)
        {
            using var input = new MemoryStream(bytes);
            using var zip = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: false);
            var hasProper = zip.Entries.Any(e => e.FullName.Equals("[Content_Types].xml", StringComparison.Ordinal));
            var lower = zip.Entries.FirstOrDefault(e => e.FullName.Equals("[content_types].xml", StringComparison.OrdinalIgnoreCase));
            if (hasProper || lower is null)
                return bytes;

            using var output = new MemoryStream();
            using (var outZip = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                foreach (var entry in zip.Entries)
                {
                    var name = entry.FullName.Equals("[content_types].xml", StringComparison.OrdinalIgnoreCase)
                        ? "[Content_Types].xml"
                        : entry.FullName;
                    var newEntry = outZip.CreateEntry(name);
                    using var src = entry.Open();
                    using var dst = newEntry.Open();
                    src.CopyTo(dst);
                }
            }
            return output.ToArray();
        }

        var headLen = Math.Min(80, bytes.Length);
        var head = Encoding.UTF8.GetString(bytes, 0, headLen).TrimStart();
        if (head.StartsWith("<xml version>", StringComparison.OrdinalIgnoreCase))
        {
            var text = Encoding.UTF8.GetString(bytes);
            text = text.Replace("<xml version>", "<?xml version=\"1.0\"?>", StringComparison.OrdinalIgnoreCase);
            return Encoding.UTF8.GetBytes(text);
        }

        return bytes;
    }

    private static string SafeParam(string column) =>
        Regex.Replace(column, @"[^A-Za-z0-9_]", "_");

    private static string? Truncate(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }

    private static void Add(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }

    private static IEnumerable<List<T>> Chunk<T>(List<T> items, int size)
    {
        for (var i = 0; i < items.Count; i += size)
            yield return items.GetRange(i, Math.Min(size, items.Count - i));
    }
}
