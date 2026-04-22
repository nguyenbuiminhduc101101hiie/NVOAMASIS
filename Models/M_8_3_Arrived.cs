using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_8_3_Arrived : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public int? SQ { get; set; }
    public string? CNTRNO { get; set; }
    public string? SZ { get; set; }
    public string? TP { get; set; }
    public string? ST { get; set; }
    public string? SealNo { get; set; }
    public decimal? WD { get; set; }
    public decimal? WN { get; set; }
    public string? CC { get; set; }
    public string? Location { get; set; }
    public string? DoBkNo { get; set; }
    public string? POD_FDest { get; set; }
    public string? Customer { get; set; }
    public string? Payment { get; set; }
    public string? TruckNo { get; set; }
    public string? VslVoy { get; set; }
    public DateTime? DT { get; set; }
    public string? Cargo { get; set; }
    public string? Remarks { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
