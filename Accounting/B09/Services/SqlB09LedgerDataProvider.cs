using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NVOAMASIS.Accounting.B09.Domain;
using NVOAMASIS.Accounting.B09.Options;
using NVOAMASIS.Data;

namespace NVOAMASIS.Accounting.B09.Services;

public sealed class SqlB09LedgerDataProvider : IB09LedgerDataProvider
{
    private static readonly Regex SafeIdentifier = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);
    private readonly AppDbContext _db;
    private readonly B09Options _options;

    public SqlB09LedgerDataProvider(AppDbContext db, IOptions<B09Options> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<decimal?> ExecuteMappingAsync(
        Guid companyId,
        DateTime fromDate,
        DateTime toDate,
        FinancialStatementNoteMapping mapping,
        CancellationToken cancellationToken = default)
    {
        if (!mapping.IsEnabled)
            return null;

        return mapping.Mode switch
        {
            B09MappingMode.AccountBalance => await ExecuteAccountQueryAsync(companyId, null, toDate, mapping, cancellationToken),
            B09MappingMode.PeriodActivity => await ExecuteAccountQueryAsync(companyId, fromDate, toDate, mapping, cancellationToken),
            B09MappingMode.CustomSql => await ExecuteCustomSqlAsync(companyId, fromDate, toDate, mapping, cancellationToken),
            _ => null
        };
    }

    private async Task<decimal?> ExecuteAccountQueryAsync(
        Guid companyId,
        DateTime? fromDate,
        DateTime toDate,
        FinancialStatementNoteMapping mapping,
        CancellationToken cancellationToken)
    {
        var prefixes = ParsePrefixes(mapping.AccountPrefixesCsv);
        if (prefixes.Count == 0)
            return null;

        var schema = Id(_options.Schema);
        var table = Id(_options.GeneralLedgerTable);
        var company = Id(_options.CompanyIdColumn);
        var account = Id(_options.AccountCodeColumn);
        var date = Id(_options.PostingDateColumn);
        var debit = Id(_options.DebitColumn);
        var credit = Id(_options.CreditColumn);

        var prefixConditions = new List<string>();
        for (var i = 0; i < prefixes.Count; i++)
            prefixConditions.Add($"[{account}] LIKE @p{i}");

        var dateFilter = fromDate.HasValue
            ? $"[{date}] >= @FromDate AND [{date}] <= @ToDate"
            : $"[{date}] <= @ToDate";

        var sql = $@"
SELECT CAST(COALESCE(SUM(COALESCE([{debit}],0) - COALESCE([{credit}],0)),0) AS decimal(28,4))
FROM [{schema}].[{table}]
WHERE [{company}] = @CompanyId
  AND {dateFilter}
  AND ({string.Join(" OR ", prefixConditions)});";

        await using var cmd = await CreateCommandAsync(sql, cancellationToken);
        AddParameter(cmd, "@CompanyId", companyId);
        if (fromDate.HasValue) AddParameter(cmd, "@FromDate", fromDate.Value);
        AddParameter(cmd, "@ToDate", toDate);
        for (var i = 0; i < prefixes.Count; i++) AddParameter(cmd, $"@p{i}", prefixes[i] + "%");

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var value = result is null or DBNull ? 0m : Convert.ToDecimal(result);
        return value * mapping.SignMultiplier;
    }

    private async Task<decimal?> ExecuteCustomSqlAsync(
        Guid companyId,
        DateTime fromDate,
        DateTime toDate,
        FinancialStatementNoteMapping mapping,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(mapping.CustomSql))
            return null;

        // Custom SQL is administrator-defined configuration, not end-user input.
        await using var cmd = await CreateCommandAsync(mapping.CustomSql, cancellationToken);
        AddParameter(cmd, "@CompanyId", companyId);
        AddParameter(cmd, "@FromDate", fromDate);
        AddParameter(cmd, "@ToDate", toDate);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull) return null;
        return Convert.ToDecimal(result) * mapping.SignMultiplier;
    }

    private async Task<DbCommand> CreateCommandAsync(string sql, CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        return cmd;
    }

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static List<string> ParsePrefixes(string? csv) =>
        (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.All(char.IsDigit))
            .Distinct()
            .ToList();

    private static string Id(string value)
    {
        if (!SafeIdentifier.IsMatch(value))
            throw new InvalidOperationException($"Unsafe SQL identifier in B09 configuration: {value}");
        return value;
    }
}
