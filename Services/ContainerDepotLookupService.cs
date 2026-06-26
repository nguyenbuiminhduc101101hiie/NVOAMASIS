using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

public sealed class ContainerDepotLookupResult
{
    public string Depot { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty;
    public DateTime? EventDate { get; set; }
    public bool Found => !string.IsNullOrWhiteSpace(Depot);
}

public class ContainerDepotLookupService
{
    // Mapping cột số cont theo từng bảng:
    // - YardMovement_AMS_26040816_Current_In_Yard2 -> ITEM_NO
    // - YardMovement_AMS_26040816_Imp            -> SOCONT
    // - YardMovement_VSS_26040808_Exp            -> SOCONT
    // - M_8_3_7_YARD_SP_ITC                      -> ContrNo
    // - M_8_3_6_YARD_APS_GATEIN/GATEOUT/STOCK    -> CONTAINER
    //
    // Không tra: M_8_3_6_YARD_APS_GENERAL, M_8_3_6_YARD_APS_GENERAL_STATUS (không có cột số cont).
    //
    // Cột ngày sự kiện APS khác nhau theo bảng (GATEOUT có DATE_OUT; GATEIN/STOCK không có).
    private static readonly (string TableName, string ContainerColumn, string EventDateExpr)[] ApsContainerTables =
    {
        ("M_8_3_6_YARD_APS_GATEIN", "CONTAINER", "COALESCE(DATE_IN, DATE_IMPORT, CREATED_AT)"),
        ("M_8_3_6_YARD_APS_GATEOUT", "CONTAINER", "COALESCE(DATE_OUT, DATE_IN, DATE_IMPORT, CREATED_AT)"),
        ("M_8_3_6_YARD_APS_STOCK", "CONTAINER", "COALESCE(DATE_IN, DATE_IMPORT, CREATED_AT)")
    };

    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ContainerDepotLookupService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ContainerDepotLookupResult?> FindLatestDepotAsync(string? containerNo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(containerNo))
            return null;

        var normalized = containerNo.Trim();
        var candidates = new List<ContainerDepotLookupResult>();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        AddCandidate(candidates, await QueryAmsCurrentAsync(db, normalized, cancellationToken));
        AddCandidate(candidates, await QueryAmsImpAsync(db, normalized, cancellationToken));
        AddCandidate(candidates, await QueryVssExpAsync(db, normalized, cancellationToken));
        AddCandidate(candidates, await QuerySpItcAsync(db, normalized, cancellationToken));

        await using var conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        foreach (var (tableName, containerColumn, eventDateExpr) in ApsContainerTables)
        {
            AddCandidate(candidates, await QueryApsTableAsync(conn, tableName, containerColumn, eventDateExpr, normalized, cancellationToken));
        }

        return candidates
            .Where(x => x.Found)
            .OrderByDescending(x => x.EventDate ?? DateTime.MinValue)
            .FirstOrDefault();
    }

    public static string? ResolveTerminalName(string? depot, IEnumerable<Terminal_> terminals)
    {
        if (string.IsNullOrWhiteSpace(depot))
            return null;

        var value = depot.Trim();
        var match = terminals.FirstOrDefault(t =>
            string.Equals(t.TermiNalName, value, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(t.Code, value, StringComparison.OrdinalIgnoreCase));

        return match?.TermiNalName ?? value;
    }

    private static void AddCandidate(List<ContainerDepotLookupResult> list, ContainerDepotLookupResult? candidate)
    {
        if (candidate is { Found: true })
            list.Add(candidate);
    }

    private static async Task<ContainerDepotLookupResult?> QueryAmsCurrentAsync(AppDbContext db, string containerNo, CancellationToken cancellationToken)
    {
        var row = await db.YardMovement_AMS_26040816_Current_In_Yard2.AsNoTracking()
            .Where(x => x.ITEM_NO == containerNo && !string.IsNullOrWhiteSpace(x.DEPOT))
            .OrderByDescending(x => x.DateImport)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.ARR_TS)
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ContainerDepotLookupResult
            {
                Depot = row.DEPOT!.Trim(),
                SourceTable = "YardMovement_AMS_26040816_Current_In_Yard2",
                EventDate = row.ARR_TS ?? row.DateImport
            };
    }

    private static async Task<ContainerDepotLookupResult?> QueryAmsImpAsync(AppDbContext db, string containerNo, CancellationToken cancellationToken)
    {
        var row = await db.YardMovement_AMS_26040816_Imp.AsNoTracking()
            .Where(x => x.SOCONT == containerNo && !string.IsNullOrWhiteSpace(x.DEPOT))
            .OrderByDescending(x => x.DateImport)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.EXEC_TS)
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ContainerDepotLookupResult
            {
                Depot = row.DEPOT!.Trim(),
                SourceTable = "YardMovement_AMS_26040816_Imp",
                EventDate = row.EXEC_TS ?? row.DateImport
            };
    }

    private static async Task<ContainerDepotLookupResult?> QueryVssExpAsync(AppDbContext db, string containerNo, CancellationToken cancellationToken)
    {
        var row = await db.YardMovement_VSS_26040808_Exp.AsNoTracking()
            .Where(x => x.SOCONT == containerNo && !string.IsNullOrWhiteSpace(x.DEPOT))
            .OrderByDescending(x => x.DateImport)
            .ThenByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.EXEC_TS)
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ContainerDepotLookupResult
            {
                Depot = row.DEPOT!.Trim(),
                SourceTable = "YardMovement_VSS_26040808_Exp",
                EventDate = row.EXEC_TS ?? row.DateImport
            };
    }

    private static async Task<ContainerDepotLookupResult?> QuerySpItcAsync(AppDbContext db, string containerNo, CancellationToken cancellationToken)
    {
        var row = await db.M_8_3_7_YARD_SP_ITC.AsNoTracking()
            .Where(x => x.ContrNo == containerNo && !string.IsNullOrWhiteSpace(x.Depot))
            .OrderByDescending(x => x.DateOut)
            .ThenByDescending(x => x.DateIn)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ContainerDepotLookupResult
            {
                Depot = row.Depot!.Trim(),
                SourceTable = "M_8_3_7_YARD_SP_ITC",
                EventDate = row.DateOut ?? row.DateIn ?? row.CreatedAt
            };
    }

    private static async Task<ContainerDepotLookupResult?> QueryApsTableAsync(DbConnection conn, string tableName, string containerColumn, string eventDateExpr, string containerNo, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
IF OBJECT_ID(N'dbo.{tableName}', N'U') IS NOT NULL
BEGIN
    SELECT TOP (1)
        LTRIM(RTRIM(DEPOT)) AS Depot,
        {eventDateExpr} AS EventDate
    FROM dbo.[{tableName}] WITH (NOLOCK)
    WHERE UPPER(LTRIM(RTRIM([{containerColumn}]))) = @ContainerNo
      AND DEPOT IS NOT NULL
      AND LTRIM(RTRIM(DEPOT)) <> N''
    ORDER BY {eventDateExpr} DESC;
END";
        AddParameter(cmd, "@ContainerNo", containerNo.ToUpperInvariant());

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var depot = reader.IsDBNull(0) ? null : reader.GetString(0);
        if (string.IsNullOrWhiteSpace(depot))
            return null;

        DateTime? eventDate = null;
        if (!reader.IsDBNull(1))
        {
            var raw = reader.GetValue(1);
            if (raw is DateTime dt)
                eventDate = dt;
        }

        return new ContainerDepotLookupResult
        {
            Depot = depot.Trim(),
            SourceTable = tableName,
            EventDate = eventDate
        };
    }

    private static void AddParameter(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}
