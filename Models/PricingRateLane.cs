namespace NVOAMASIS.Models;

public record PricingRateLaneInfo(string Code, string TitleKey);

/// <summary>
/// The 7 trade lanes covered by "PRICING- EX HO CHI MINH-GOOGLE SHEET.xlsx".
/// <see cref="All"/> is in the same fixed order as the sheets in that workbook.
/// </summary>
public static class PricingRateLane
{
    public const string Intrasia = "INTRASIA";
    public const string IntrasiaRF = "INTRASIA_RF";
    public const string MiddleEastRedSeaIndia = "MIDEAST_REDSEA_INDIA";
    public const string LatinWestEastCoastAus = "LATIN_WEST_EAST_AUS";
    public const string MiddleEastRedSeaIndiaRF = "MIDEAST_REDSEA_INDIA_RF";
    public const string EuUsaAfricaNamMy = "EU_USA_AFRICA_NAMMY";
    public const string EuUsaAfricaNamMyRF = "EU_USA_AFRICA_NAMMY_RF";

    public static readonly IReadOnlyList<PricingRateLaneInfo> All = new List<PricingRateLaneInfo>
    {
        new(Intrasia, "PricingOceanFreight_Tab_Intrasia"),
        new(IntrasiaRF, "PricingOceanFreight_Tab_IntrasiaRF"),
        new(MiddleEastRedSeaIndia, "PricingOceanFreight_Tab_MiddleEast"),
        new(LatinWestEastCoastAus, "PricingOceanFreight_Tab_LatinWestEastAus"),
        new(MiddleEastRedSeaIndiaRF, "PricingOceanFreight_Tab_MiddleEastRF"),
        new(EuUsaAfricaNamMy, "PricingOceanFreight_Tab_EuUsaAfrica"),
        new(EuUsaAfricaNamMyRF, "PricingOceanFreight_Tab_EuUsaAfricaRF"),
    };
}
