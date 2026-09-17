using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

/// <summary>
/// One row of an air freight quotation sheet (hãng bay / điểm đến / bậc giá theo kg).
/// Each import is tagged with <see cref="SourceFileName"/> so a whole file can be
/// bulk-deleted later without touching rows imported from a different file.
/// </summary>
public class M_AirFreightRate : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    /// <summary>Name of the .xlsx file this row was imported from; null for rows added manually.</summary>
    public string? SourceFileName { get; set; }
    public string? Airlines { get; set; }
    public string? Destination { get; set; }
    public decimal? Min { get; set; }
    public decimal? RateUnder45 { get; set; }
    public decimal? RateOver45 { get; set; }
    public decimal? RateOver100 { get; set; }
    public decimal? RateOver300 { get; set; }
    public decimal? RateOver500 { get; set; }
    public decimal? RateOver1000 { get; set; }
    public string? FscWsc { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public string? TransitTime { get; set; }
    public string? Surcharges { get; set; }
    public string? Note { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
