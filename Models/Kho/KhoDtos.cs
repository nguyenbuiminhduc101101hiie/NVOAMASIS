namespace NVOAMASIS.Models.Kho
{
    /// <summary>Kết quả ghi sổ / bỏ ghi sổ / tính lại giá.</summary>
    public sealed class KhoPostResult
    {
        public string DocNo { get; set; } = "";
        public string? VoucherNo { get; set; }
        public int GlLines { get; set; }
        /// <summary>Số phiếu xuất / chuyển khác được cập nhật lại giá xuất.</summary>
        public int RecostedDocs { get; set; }
        public List<string> RecostedDocNos { get; set; } = new();
    }

    /// <summary>Dòng báo cáo tổng hợp nhập xuất tồn (S11-DN).</summary>
    public sealed class KhoNxtRow
    {
        public Guid ItemId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public string Account { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal? MinQty { get; set; }
        public decimal OpenQty { get; set; }
        public decimal OpenValue { get; set; }
        public decimal InQty { get; set; }
        public decimal InValue { get; set; }
        public decimal OutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal CloseQty => OpenQty + InQty - OutQty;
        public decimal CloseValue => OpenValue + InValue - OutValue;
        public bool IsZero => OpenQty == 0 && OpenValue == 0 && InQty == 0 && InValue == 0 && OutQty == 0 && OutValue == 0;
        public bool BelowMin => MinQty.HasValue && MinQty.Value > 0 && CloseQty < MinQty.Value;
    }

    /// <summary>Dòng sổ chi tiết vật tư / thẻ kho (S10-DN).</summary>
    public sealed class KhoCardLine
    {
        public Guid DocId { get; set; }
        public DateTime Date { get; set; }
        public string DocNo { get; set; } = "";
        public string DocType { get; set; } = "";
        public string Description { get; set; } = "";
        public string Warehouse { get; set; } = "";
        public string Contra { get; set; } = "";
        public decimal UnitCost { get; set; }
        public decimal InQty { get; set; }
        public decimal InValue { get; set; }
        public decimal OutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal BalQty { get; set; }
        public decimal BalValue { get; set; }
    }

    public sealed class KhoCard
    {
        public Guid ItemId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public string Account { get; set; } = "";
        public decimal OpenQty { get; set; }
        public decimal OpenValue { get; set; }
        public List<KhoCardLine> Lines { get; set; } = new();
        public decimal InQty => Lines.Sum(l => l.InQty);
        public decimal InValue => Lines.Sum(l => l.InValue);
        public decimal OutQty => Lines.Sum(l => l.OutQty);
        public decimal OutValue => Lines.Sum(l => l.OutValue);
        public decimal CloseQty => OpenQty + InQty - OutQty;
        public decimal CloseValue => OpenValue + InValue - OutValue;
    }

    /// <summary>Đối chiếu giá trị tồn kho với số dư sổ cái theo TK.</summary>
    public sealed class KhoGlCheck
    {
        public string Account { get; set; } = "";
        public string AccountName { get; set; } = "";
        /// <summary>Giá trị tồn cuối theo kho (mọi kho).</summary>
        public decimal StockValue { get; set; }
        /// <summary>Tổng giá trị phiếu tồn đầu kỳ (không ghi sổ cái — số dư đầu nằm ở 10.9).</summary>
        public decimal OpeningDocs { get; set; }
        /// <summary>Nợ − Có trong sổ cái (mọi nguồn) đến ngày.</summary>
        public decimal GlBalance { get; set; }
        /// <summary>Phần sổ cái do phiếu kho sinh ra.</summary>
        public decimal GlFromStock { get; set; }
        public decimal Difference => StockValue - OpeningDocs - GlBalance;
    }

    /// <summary>Tồn hiện tại (gợi ý khi lập phiếu xuất).</summary>
    public sealed record KhoStockHint(decimal Qty, decimal Value)
    {
        public decimal AvgCost => Qty <= 0 ? 0 : decimal.Round(Value / Qty, 4, MidpointRounding.AwayFromZero);
    }
}
