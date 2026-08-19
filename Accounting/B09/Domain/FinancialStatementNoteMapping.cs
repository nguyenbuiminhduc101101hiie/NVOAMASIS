using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Accounting.B09.Domain;

public class FinancialStatementNoteMapping
{
    public int Id { get; set; }
    public int NoteLineId { get; set; }

    public B09MappingMode Mode { get; set; }

    // CSV prefixes, e.g. "111,112,113" or "511".
    [MaxLength(1000)]
    public string? AccountPrefixesCsv { get; set; }

    // Debit-Credit result * SignMultiplier. Revenue/liability/equity normally use -1.
    public decimal SignMultiplier { get; set; } = 1m;

    // Optional threshold used later for detail disclosures, e.g. 10%.
    public decimal? DetailThresholdPercent { get; set; }

    // Custom SQL must return one scalar numeric value.
    // Supported parameters: @CompanyId, @FromDate, @ToDate.
    public string? CustomSql { get; set; }

    public bool IsEnabled { get; set; } = true;

    public FinancialStatementNoteLine NoteLine { get; set; } = null!;
}
