using System.ComponentModel.DataAnnotations;

namespace NVOAMASIS.Models;

public class M_8_3_IN_OUT_YARD_1 : IExcelImportEntity
{
    [Key]
    public Guid Id { get; set; }
    public string? DEPOT { get; set; }
    public string? METHOD { get; set; }
    public DateTime? EXEC_TS { get; set; }
    public string? LINE { get; set; }
    public string? ITEM_KEY { get; set; }
    public string? SOCONT { get; set; }
    public string? KICHCO { get; set; }
    public string? TRANGTHAI { get; set; }
    public decimal? TRONGLUONG { get; set; }
    public decimal? TRONGLUONG_VGM { get; set; }
    public string? BL_NO { get; set; }
    public string? BOOK_NO { get; set; }
    public string? RELEASE_NO { get; set; }
    public string? HUONG { get; set; }
    public string? HUONG1 { get; set; }
    public string? CANGCT { get; set; }
    public string? CANGDEN { get; set; }
    public string? GIAO { get; set; }
    public string? NHAN { get; set; }
    public string? DGS_CLASS { get; set; }
    public string? GHICHU { get; set; }
    public string? ENTRY_VOY_NO { get; set; }
    public string? ENTRY_VES_NAME { get; set; }
    public string? EXIT_VOY_NO { get; set; }
    public string? EXIT_VES_NAME { get; set; }
    public string? ENTRY_TRUCK_ID { get; set; }
    public string? EXIT_TRUCK_ID { get; set; }
    public string? SOSEAL { get; set; }
    public DateTime DateImport { get; set; }
    public string? UserImport { get; set; }
    public DateTime CreatedAt { get; set; }
}
