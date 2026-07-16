using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models;

/// <summary>Quản lý container (sheet VNSGN - QUAN LY CONT).</summary>
[Table("QuanLy_Cont")]
public class QuanLy_Cont
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>Principal (PRINCIPAL).</summary>
    [MaxLength(20)]
    public string? Principal { get; set; }

    /// <summary>Chủ container.</summary>
    [MaxLength(20)]
    public string? ChuCont { get; set; }

    /// <summary>Số container (VD: MEDU1189165).</summary>
    [MaxLength(20)]
    public string Container { get; set; } = string.Empty;

    /// <summary>Kích thước container (20, 40...).</summary>
    public int? ContSize { get; set; }

    /// <summary>Loại container (DC, HC...).</summary>
    [MaxLength(10)]
    public string? ContType { get; set; }

    /// <summary>Số HB/L.</summary>
    [MaxLength(30)]
    public string? HBL_No { get; set; }

    /// <summary>Port of Loading.</summary>
    [MaxLength(50)]
    public string? POL { get; set; }

    /// <summary>Port of Discharge.</summary>
    [MaxLength(50)]
    public string? POD { get; set; }

    /// <summary>Tên tàu.</summary>
    [MaxLength(50)]
    public string? Vessel { get; set; }

    /// <summary>Số chuyến.</summary>
    [MaxLength(20)]
    public string? Voy { get; set; }

    /// <summary>Ghi chú.</summary>
    [MaxLength(500)]
    public string? Remark { get; set; }

    /// <summary>Ngày đến dự kiến (Estimated Time of Arrival).</summary>
    public DateTime? ETA { get; set; }

    /// <summary>Ngày gate out.</summary>
    public DateTime? GateOut { get; set; }

    /// <summary>Ngày trả rỗng.</summary>
    public DateTime? EmptyDate { get; set; }

    /// <summary>Bãi container.</summary>
    [MaxLength(100)]
    public string? Yard { get; set; }

    /// <summary>Estimate.</summary>
    [MaxLength(200)]
    public string? Estimate { get; set; }

    /// <summary>Phí collect.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? Collect { get; set; }

    /// <summary>Trạng thái.</summary>
    [MaxLength(50)]
    public string? Status { get; set; }
}
