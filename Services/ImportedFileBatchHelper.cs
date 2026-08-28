using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public static class ImportedFileBatchHelper
{
    public static string QuoteName(string value)
        => "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";

    public static async Task EnsureNvarcharColumnAsync(
        DatabaseFacade database,
        string tableName,
        string columnName,
        int length = 260)
    {
        var table = QuoteName(tableName);
        var column = QuoteName(columnName);
        var colLiteral = columnName.Replace("'", "''", StringComparison.Ordinal);

        await database.ExecuteSqlRawAsync($@"
IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.{table}', N'{colLiteral}') IS NULL
    ALTER TABLE dbo.{table} ADD {column} NVARCHAR({length}) NULL;");
    }

    public static async Task<List<ImportedFileBatch>> LoadAsync(
        DatabaseFacade database,
        IReadOnlyList<ImportedFileSource> sources,
        CancellationToken ct = default)
    {
        var merged = new Dictionary<ImportedFileBatch, ImportedFileBatch>();
        var conn = database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        foreach (var source in sources)
        {
            if (!await TableExistsAsync(conn, source.TableName, ct))
                continue;

            var table = QuoteName(source.TableName);
            var fileCol = QuoteName(source.FileColumn);
            var dateCol = QuoteName(source.DateColumn);
            var userCol = QuoteName(source.UserColumn);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
SELECT LTRIM(RTRIM(CAST({fileCol} AS NVARCHAR(260)))) AS [FileName],
       CAST({dateCol} AS date) AS [ImportDate],
       COUNT_BIG(*) AS [BatchRows],
       MAX(CAST({userCol} AS NVARCHAR(100))) AS [UserImport]
FROM dbo.{table}
WHERE NULLIF(LTRIM(RTRIM(CAST({fileCol} AS NVARCHAR(260)))), N'') IS NOT NULL
  AND {dateCol} IS NOT NULL
GROUP BY LTRIM(RTRIM(CAST({fileCol} AS NVARCHAR(260)))), CAST({dateCol} AS date);";

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var batch = new ImportedFileBatch
                {
                    FileName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    ImportDate = reader.GetDateTime(1).Date,
                    RowCount = Convert.ToInt32(reader.GetValue(2)),
                    UserImport = reader.IsDBNull(3) ? null : reader.GetString(3)
                };

                if (string.IsNullOrWhiteSpace(batch.FileName))
                    continue;

                if (merged.TryGetValue(batch, out var existing))
                    existing.RowCount += batch.RowCount;
                else
                    merged[batch] = batch;
            }
        }

        return merged.Values
            .OrderByDescending(x => x.ImportDate)
            .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static async Task<int> DeleteAsync(
        DatabaseFacade database,
        IReadOnlyList<ImportedFileSource> sources,
        IEnumerable<ImportedFileBatch> batches,
        CancellationToken ct = default)
    {
        var targets = batches
            .Where(x => !string.IsNullOrWhiteSpace(x.FileName))
            .GroupBy(x => x, x => x)
            .Select(g => g.Key)
            .ToList();

        if (targets.Count == 0)
            return 0;

        var total = 0;
        foreach (var source in sources)
        {
            var table = QuoteName(source.TableName);
            var fileCol = QuoteName(source.FileColumn);
            var dateCol = QuoteName(source.DateColumn);

            foreach (var batch in targets)
            {
                total += await database.ExecuteSqlRawAsync($@"
IF OBJECT_ID(N'dbo.{table}', N'U') IS NOT NULL
DELETE FROM dbo.{table}
WHERE NULLIF(LTRIM(RTRIM(CAST({fileCol} AS NVARCHAR(260)))), N'') = @FileName
  AND CAST({dateCol} AS date) = @ImportDate;",
                    new SqlParameter("@FileName", SqlDbType.NVarChar, 260) { Value = batch.FileName.Trim() },
                    new SqlParameter("@ImportDate", SqlDbType.Date) { Value = batch.ImportDate.Date });
            }
        }

        return total;
    }

    private static async Task<bool> TableExistsAsync(DbConnection conn, string tableName, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CASE WHEN OBJECT_ID(@FullName, N'U') IS NULL THEN 0 ELSE 1 END;";
        var p = cmd.CreateParameter();
        p.ParameterName = "@FullName";
        p.Value = "dbo." + QuoteName(tableName);
        cmd.Parameters.Add(p);
        var result = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result) == 1;
    }
}
