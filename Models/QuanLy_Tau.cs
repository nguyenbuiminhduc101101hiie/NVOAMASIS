using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models;

/// <summary>Quản lý tàu / PDA (sheet PDA - QUAN LY TAU). Mỗi dòng chi phí PDA là 1 record.</summary>
[Table("QuanLy_Tau")]
public class QuanLy_Tau
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>Tên tàu (VD: TONGAN).</summary>
    [MaxLength(100)]
    public string? VesselName { get; set; }

    /// <summary>Thông tin chuyến (VD: M/V : TONGAN (14STPCHN - 2513W)).</summary>
    [MaxLength(200)]
    public string? VoyageInfo { get; set; }

    /// <summary>Deadweight tonnage.</summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? DWT { get; set; }

    /// <summary>Gross register tonnage.</summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? GRT { get; set; }

    /// <summary>Net register tonnage.</summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? NRT { get; set; }

    /// <summary>Length overall.</summary>
    [Column(TypeName = "decimal(12,2)")]
    public decimal? LOA { get; set; }

    /// <summary>Actual Time of Arrival.</summary>
    public DateTime? ATA { get; set; }

    /// <summary>Actual Time of Berthing.</summary>
    public DateTime? ATB { get; set; }

    /// <summary>Actual Time of Departure.</summary>
    public DateTime? ATD { get; set; }

    /// <summary>Số giờ estimate.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal? EstimateHours { get; set; }

    /// <summary>Tên khoản mục PDA.</summary>
    [MaxLength(100)]
    public string? Items { get; set; }

    /// <summary>Cơ sở tính (GRT, LOA...).</summary>
    [MaxLength(20)]
    public string? BasedOn { get; set; }

    /// <summary>Đơn giá / công thức tính.</summary>
    [MaxLength(200)]
    public string? Rate { get; set; }

    /// <summary>Turn/Time.</summary>
    [MaxLength(10)]
    public string? TurnTime { get; set; }

    /// <summary>Hệ số nhân / số lượng.</summary>
    [Column(TypeName = "decimal(12,4)")]
    public decimal? Qty { get; set; }

    /// <summary>Thành tiền USD.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? AmountUSD { get; set; }

    /// <summary>Thành tiền VND.</summary>
    [Column(TypeName = "decimal(18,0)")]
    public decimal? AmountVND { get; set; }

    /// <summary>Ghi chú.</summary>
    [MaxLength(100)]
    public string? Remarks { get; set; }

    /// <summary>Số hóa đơn.</summary>
    [MaxLength(50)]
    public string? Invoice { get; set; }
}
