using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_YardReport_AG_2026040307 : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public string SourceSheet { get; set; } = string.Empty;
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
    public int? Days { get; set; }
    public string? Customer { get; set; }
    public string? Payment { get; set; }
    public string? TruckNo { get; set; }
    public string? VslVoy { get; set; }
    public DateTime? DT { get; set; }
    public string? PlugIn { get; set; }
    public string? Cargo { get; set; }
    public string? Remarks { get; set; }
    public string? LEN { get; set; }
    public string? TY { get; set; }
    public string? Cond { get; set; }
    public string? ShipCons { get; set; }
    public string? BKDO { get; set; }
    public string? DISC_VV { get; set; }
    public string? LOAD_VV { get; set; }
    public DateTime? UNSDT { get; set; }
    public DateTime? CCDT { get; set; }
    public DateTime? STUDT { get; set; }
    public string? JOB { get; set; }
    public string? SrvCode { get; set; }
    public string? MODE { get; set; }
    public string? STV { get; set; }
    public string? Type { get; set; }
    public string? Condition { get; set; }
    public string? POL { get; set; }
    public string? LoadVV { get; set; }
    public string? POD { get; set; }
    public string? DischVV { get; set; }
    public string? DayStatus { get; set; }
    public int? DayStorage { get; set; }
    public DateTime? DisArrDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
