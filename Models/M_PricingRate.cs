using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

/// <summary>
/// One row of ocean freight rate. <see cref="TradeLane"/> (see <see cref="PricingRateLane"/>)
/// identifies which of the 7 sheets in "PRICING- EX HO CHI MINH-GOOGLE SHEET.xlsx" the row
/// belongs to; all 7 sheets share this exact same column layout.
/// </summary>
public class M_PricingRate : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public string TradeLane { get; set; } = string.Empty;
    /// <summary>Name of the .xlsx file this row was imported from; null for rows added manually.</summary>
    public string? SourceFileName { get; set; }
    public string? ExtraNote { get; set; }
    public DateTime? EffDate { get; set; }
    public DateTime? ValidDate { get; set; }
    public string? Carrier { get; set; }
    public string? Country { get; set; }
    public string? Pol { get; set; }
    public string? Pod { get; set; }
    public decimal? OfRateCom20 { get; set; }
    public decimal? OfRateCom40 { get; set; }
    public decimal? OfRateCom40Hc { get; set; }
    public decimal? CommissionHdl20 { get; set; }
    public decimal? CommissionHdl40 { get; set; }
    public decimal? CommissionHdl40Hc { get; set; }
    public string? Remark { get; set; }
    public string? FreeTimeAtPod { get; set; }
    public decimal? BasicOfNet20 { get; set; }
    public decimal? BasicOfNet40 { get; set; }
    public decimal? BasicOfNet40Hc { get; set; }
    public decimal? SurchargeFuelEnv20 { get; set; }
    public decimal? SurchargeFuelEnv40 { get; set; }
    public decimal? SurchargeFuelEnv40Hc { get; set; }
    public decimal? SurchargeMisc20 { get; set; }
    public decimal? SurchargeMisc40 { get; set; }
    public decimal? SurchargeMisc40Hc { get; set; }
    public decimal? SurchargeSecurityEnv20 { get; set; }
    public decimal? SurchargeSecurityEnv40 { get; set; }
    public decimal? SurchargeSecurityEnv40Hc { get; set; }
    public decimal? TotalOf20 { get; set; }
    public decimal? TotalOf40 { get; set; }
    public decimal? TotalOf40Hc { get; set; }
    public decimal? VatCom20 { get; set; }
    public decimal? VatCom40 { get; set; }
    public decimal? VatCom40Hc { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
