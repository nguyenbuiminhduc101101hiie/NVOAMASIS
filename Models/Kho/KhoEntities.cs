using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NVOAMASIS.Models.Kho
{
    /// <summary>10.18.1 Danh mục vật tư, công cụ dụng cụ, thành phẩm, hàng hóa (TK 151–156).</summary>
    [Table("KhoVatTu")]
    public class KhoVatTu
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = string.Empty;
        [MaxLength(250)] public string Name { get; set; } = string.Empty;
        [MaxLength(30)] public string Unit { get; set; } = string.Empty;
        /// <summary>TK kho mặc định: 151, 152, 153, 154, 155, 156 hoặc TK con (1561...).</summary>
        [MaxLength(20)] public string InventoryAccount { get; set; } = "1561";
        [MaxLength(100)] public string? Category { get; set; }
        /// <summary>Tồn tối thiểu — báo cáo NXT tô màu khi tồn cuối thấp hơn.</summary>
        public decimal? MinQty { get; set; }
        [MaxLength(500)] public string? Note { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    /// <summary>10.18.1 Danh mục kho.</summary>
    [Table("KhoHang")]
    public class KhoHang
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = string.Empty;
        [MaxLength(250)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string? Address { get; set; }
        [MaxLength(150)] public string? Keeper { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }

    /// <summary>10.18.2 Phiếu kho: tồn đầu kỳ / nhập / xuất / chuyển kho.</summary>
    [Table("PhieuKho")]
    public class PhieuKho
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        /// <summary><see cref="KhoDocTypes"/>: OPEN, IN, OUT, TRANSFER.</summary>
        [MaxLength(10)] public string DocType { get; set; } = KhoDocTypes.In;
        [MaxLength(30)] public string DocNo { get; set; } = string.Empty;
        public DateTime DocDate { get; set; } = DateTime.Today;
        /// <summary>Lý do nhập/xuất — <see cref="KhoReasons"/>.</summary>
        [MaxLength(30)] public string? Reason { get; set; }
        public Guid WarehouseId { get; set; }
        /// <summary>Kho nhận (chỉ phiếu chuyển kho).</summary>
        public Guid? ToWarehouseId { get; set; }
        /// <summary>Nhà cung cấp / khách hàng (bảng Customer).</summary>
        public Guid? PartnerId { get; set; }
        [MaxLength(250)] public string? PartnerName { get; set; }
        /// <summary>Người giao hàng (phiếu nhập) / người nhận hàng (phiếu xuất).</summary>
        [MaxLength(150)] public string? ContactName { get; set; }
        /// <summary>TK đối ứng mặc định cho các dòng (331, 1111, 621, 632...).</summary>
        [MaxLength(20)] public string? ContraAccount { get; set; }
        /// <summary>TK thuế GTGT đầu vào (phiếu nhập mua) — mặc định 1331.</summary>
        [MaxLength(20)] public string? VatAccount { get; set; }
        [MaxLength(50)] public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        /// <summary>0 = nháp, 1 = đã ghi sổ.</summary>
        public int Status { get; set; }
        /// <summary>Chứng từ kế toán sinh ra khi ghi sổ (AccountingVouchers.Id, SourceModule = INVENTORY).</summary>
        public Guid? VoucherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
        public DateTime? PostedAt { get; set; }
        [MaxLength(100)] public string? PostedBy { get; set; }
    }

    /// <summary>Dòng hàng của phiếu kho.</summary>
    [Table("PhieuKhoChiTiet")]
    public class PhieuKhoChiTiet
    {
        [Key] public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PhieuKhoId { get; set; }
        public int LineNo { get; set; }
        public Guid VatTuId { get; set; }
        /// <summary>TK kho của dòng (lấy từ danh mục vật tư khi lập phiếu).</summary>
        [MaxLength(20)] public string InventoryAccount { get; set; } = string.Empty;
        /// <summary>TK đối ứng riêng của dòng — trống = dùng TK đối ứng của phiếu.</summary>
        [MaxLength(20)] public string? ContraAccount { get; set; }
        public decimal Quantity { get; set; }
        /// <summary>Phiếu nhập / tồn đầu: người dùng nhập. Phiếu xuất / chuyển: phần mềm tính (bình quân tức thời).</summary>
        public decimal UnitCost { get; set; }
        public decimal Amount { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        [MaxLength(500)] public string? Note { get; set; }
    }

    public static class KhoDocTypes
    {
        public const string Opening = "OPEN";
        public const string In = "IN";
        public const string Out = "OUT";
        public const string Transfer = "TRANSFER";

        public static readonly string[] All = { Opening, In, Out, Transfer };

        public static string Label(string? t) => t switch
        {
            Opening => "Tồn đầu kỳ",
            In => "Phiếu nhập kho",
            Out => "Phiếu xuất kho",
            Transfer => "Phiếu chuyển kho",
            _ => t ?? ""
        };

        public static string Prefix(string? t) => t switch
        {
            Opening => "DK",
            In => "NK",
            Out => "XK",
            Transfer => "CK",
            _ => "PK"
        };

        /// <summary>Thứ tự xử lý trong cùng 1 ngày: tồn đầu → nhập → chuyển → xuất.</summary>
        public static int Rank(string? t) => t switch
        {
            Opening => 0,
            In => 1,
            Transfer => 2,
            Out => 3,
            _ => 4
        };

        /// <summary>Phiếu tự nhập đơn giá (nhập, tồn đầu) hay phần mềm tính (xuất, chuyển).</summary>
        public static bool UserPriced(string? t) => t is Opening or In;
    }

    public static class KhoStatus
    {
        public const int Draft = 0;
        public const int Posted = 1;
    }

    public sealed record KhoReason(string Code, string DocType, string Label, string ContraAccount);

    /// <summary>Lý do nhập/xuất và TK đối ứng gợi ý (người dùng sửa được trên phiếu).</summary>
    public static class KhoReasons
    {
        public static readonly IReadOnlyList<KhoReason> All = new List<KhoReason>
        {
            new("IN_BUY", KhoDocTypes.In, "Mua hàng chưa thanh toán", "331"),
            new("IN_CASH", KhoDocTypes.In, "Mua hàng trả tiền mặt", "1111"),
            new("IN_BANK", KhoDocTypes.In, "Mua hàng chuyển khoản", "1121"),
            new("IN_TRANSIT", KhoDocTypes.In, "Hàng mua đang đi đường về nhập kho", "151"),
            new("IN_PRODUCT", KhoDocTypes.In, "Nhập thành phẩm từ sản xuất", "154"),
            new("IN_RETURN", KhoDocTypes.In, "Nhập hàng bán bị trả lại", "632"),
            new("IN_UNUSED", KhoDocTypes.In, "Nhập lại vật tư dùng không hết", "621"),
            new("IN_OTHER", KhoDocTypes.In, "Nhập khác", "3381"),

            new("OUT_SALE", KhoDocTypes.Out, "Xuất bán (giá vốn)", "632"),
            new("OUT_PROD", KhoDocTypes.Out, "Xuất cho sản xuất", "621"),
            new("OUT_GENPROD", KhoDocTypes.Out, "Xuất dùng chung phân xưởng", "627"),
            new("OUT_SELLING", KhoDocTypes.Out, "Xuất dùng cho bán hàng", "641"),
            new("OUT_ADMIN", KhoDocTypes.Out, "Xuất dùng cho quản lý", "642"),
            new("OUT_PREPAID", KhoDocTypes.Out, "Xuất CCDC chờ phân bổ", "242"),
            new("OUT_SERVICE", KhoDocTypes.Out, "Xuất cho dịch vụ / công trình dở dang", "154"),
            new("OUT_RETURN", KhoDocTypes.Out, "Xuất trả lại nhà cung cấp", "331"),
            new("OUT_OTHER", KhoDocTypes.Out, "Xuất khác", "811"),
        };

        public static IEnumerable<KhoReason> For(string docType) => All.Where(r => r.DocType == docType);

        public static KhoReason? Find(string? code) => All.FirstOrDefault(r => r.Code == code);
    }

    public static class KhoAccounts
    {
        /// <summary>Các TK hàng tồn kho được theo dõi bằng module này.</summary>
        public static readonly string[] Roots = { "151", "152", "153", "154", "155", "156" };

        public static bool IsInventory(string? account)
        {
            var a = account?.Trim() ?? "";
            return Roots.Any(r => a.StartsWith(r, StringComparison.Ordinal));
        }

        public static string Root(string? account)
        {
            var a = account?.Trim() ?? "";
            return Roots.FirstOrDefault(r => a.StartsWith(r, StringComparison.Ordinal)) ?? a;
        }

        public static string RootName(string root) => root switch
        {
            "151" => "Hàng mua đang đi đường",
            "152" => "Nguyên liệu, vật liệu",
            "153" => "Công cụ, dụng cụ",
            "154" => "Chi phí sản xuất, kinh doanh dở dang",
            "155" => "Sản phẩm",
            "156" => "Hàng hóa",
            _ => ""
        };
    }
}
