using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_YardMovement_AMS_26040816_Current_In_Yard2 : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public string? DEPOT { get; set; }
    public Guid? DepotId { get; set; }
    public string? AGENT { get; set; }
    public string? LINE { get; set; }
    public string? ITEM_KEY { get; set; }
    public string? ITEM_NO { get; set; }
    public string? ISO { get; set; }
    public string? FEL { get; set; }
    public decimal? TEMP { get; set; }
    public decimal? WEIGHT { get; set; }
    public decimal? VGM_WEIGHT { get; set; }
    public string? BOOK_NO { get; set; }
    public string? BILL_OF_LADING { get; set; }
    public string? LOCATION { get; set; }
    public string? CATEGORY { get; set; }
    public string? ARR_BY { get; set; }
    public string? ARR_CAR { get; set; }
    public string? ARR_VES_NAME { get; set; }
    public DateTime? ARR_TS { get; set; }
    public string? DEP_BY { get; set; }
    public string? DEP_CAR { get; set; }
    public string? DEP_VES_NAME { get; set; }
    public DateTime? DEP_TS { get; set; }
    public string? DISCH_PORT { get; set; }
    public string? FINAL_DISCH_PORT { get; set; }
    public string? PLACE_OF_RECEIPT { get; set; }
    public string? PLACE_OF_DELIVERY { get; set; }
    public string? CUSTOM_CLEARANCE { get; set; }
    public DateTime? CC_TS { get; set; }
    public string? DGS_CLASS { get; set; }
    public string? UN_NO { get; set; }
    public string? DAM { get; set; }
    public string? GHICHU { get; set; }
    public string? SOSEAL { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
