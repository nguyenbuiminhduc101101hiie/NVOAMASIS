using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_8_3_Stuffed : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public int? SQ { get; set; }
    public string? CNTRNO { get; set; }
    public string? SealNo { get; set; }
    public string? CO { get; set; }
    public string? LEN { get; set; }
    public string? TY { get; set; }
    public decimal? WD { get; set; }
    public decimal? WN { get; set; }
    public string? Cond { get; set; }
    public string? ShipCons { get; set; }
    public string? BKDO { get; set; }
    public string? LOAD_VV { get; set; }
    public string? POD_PDest { get; set; }
    public DateTime? STUDT { get; set; }
    public string? PlugIn { get; set; }
    public string? JOB { get; set; }
    public string? Payment { get; set; }
    public string? SrvCode { get; set; }
    public string? MODE { get; set; }
    public string? Cargo { get; set; }
    public string? WeightRemarks { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
