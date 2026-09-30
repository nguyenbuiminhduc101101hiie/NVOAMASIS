using NVOAMASIS.Models.Kho;

namespace NVOAMASIS.Services.Kho
{
    /// <summary>Một dòng phiếu kho đã ghi sổ, đưa vào tính giá.</summary>
    public sealed class KhoMove
    {
        public Guid DocId { get; init; }
        public Guid LineId { get; init; }
        public string DocType { get; init; } = KhoDocTypes.In;
        public string DocNo { get; init; } = "";
        public DateTime DocDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public int LineNo { get; init; }
        public Guid ItemId { get; init; }
        public Guid WarehouseId { get; init; }
        public Guid? ToWarehouseId { get; init; }
        public decimal Quantity { get; init; }
        /// <summary>Thành tiền người dùng nhập (tồn đầu, nhập). Bỏ qua với xuất / chuyển.</summary>
        public decimal Amount { get; init; }
    }

    public sealed record KhoCost(decimal UnitCost, decimal Amount);

    public sealed record KhoShortage(Guid DocId, string DocNo, DateTime DocDate, Guid ItemId, Guid WarehouseId,
        decimal Requested, decimal Available);

    public sealed class KhoCostingResult
    {
        /// <summary>Giá xuất tính được cho từng dòng xuất / chuyển kho (theo LineId).</summary>
        public Dictionary<Guid, KhoCost> Costs { get; } = new();
        public List<KhoShortage> Shortages { get; } = new();
        /// <summary>Tồn cuối cùng theo (vật tư, kho).</summary>
        public Dictionary<(Guid Item, Guid Warehouse), (decimal Qty, decimal Value)> Closing { get; } = new();
    }

    /// <summary>
    /// Tính giá xuất kho theo phương pháp BÌNH QUÂN GIA QUYỀN TỨC THỜI (sau mỗi lần nhập), riêng từng vật tư tại từng kho.
    /// Thứ tự: ngày chứng từ → trong cùng ngày: tồn đầu, nhập, chuyển, xuất → thời điểm lập phiếu → số phiếu → số dòng.
    ///   Giá xuất = giá trị tồn / số lượng tồn tại thời điểm xuất × SL xuất (làm tròn đồng).
    ///   Xuất hết số lượng tồn → lấy đúng toàn bộ giá trị tồn (không để lại số lẻ).
    ///   Chuyển kho: xuất kho đi theo giá bình quân, nhập kho đến đúng giá trị đó.
    /// </summary>
    public static class KhoCostingEngine
    {
        private const decimal QtyEps = 0.00005m;

        public static IOrderedEnumerable<KhoMove> Order(IEnumerable<KhoMove> moves) =>
            moves.OrderBy(m => m.DocDate.Date)
                .ThenBy(m => KhoDocTypes.Rank(m.DocType))
                .ThenBy(m => m.CreatedAt)
                .ThenBy(m => m.DocNo, StringComparer.Ordinal)
                .ThenBy(m => m.LineNo);

        public static KhoCostingResult Run(IEnumerable<KhoMove> moves)
        {
            var result = new KhoCostingResult();
            var state = result.Closing;

            foreach (var m in Order(moves))
            {
                var key = (m.ItemId, m.WarehouseId);
                state.TryGetValue(key, out var s);

                switch (m.DocType)
                {
                    case KhoDocTypes.Opening:
                    case KhoDocTypes.In:
                        state[key] = (s.Qty + m.Quantity, s.Value + m.Amount);
                        break;

                    case KhoDocTypes.Out:
                    case KhoDocTypes.Transfer:
                    {
                        var (cost, after) = Issue(s, m.Quantity);
                        if (m.Quantity > s.Qty + QtyEps)
                            result.Shortages.Add(new KhoShortage(m.DocId, m.DocNo, m.DocDate, m.ItemId, m.WarehouseId,
                                m.Quantity, s.Qty));
                        state[key] = after;
                        result.Costs[m.LineId] = new KhoCost(
                            m.Quantity == 0 ? 0 : decimal.Round(cost / m.Quantity, 4, MidpointRounding.AwayFromZero), cost);

                        if (m.DocType == KhoDocTypes.Transfer && m.ToWarehouseId.HasValue)
                        {
                            var toKey = (m.ItemId, m.ToWarehouseId.Value);
                            state.TryGetValue(toKey, out var t);
                            state[toKey] = (t.Qty + m.Quantity, t.Value + cost);
                        }
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>Giá trị xuất của SL <paramref name="qty"/> khi đang tồn <paramref name="s"/>.</summary>
        public static (decimal Cost, (decimal Qty, decimal Value) After) Issue((decimal Qty, decimal Value) s, decimal qty)
        {
            if (qty <= 0) return (0, s);
            decimal cost;
            if (s.Qty <= QtyEps)
                cost = 0; // hết hàng: không có giá (báo thiếu hàng ở ngoài)
            else if (qty >= s.Qty - QtyEps)
                cost = s.Value; // xuất hết → lấy toàn bộ giá trị còn lại
            else
                cost = decimal.Round(s.Value / s.Qty * qty, 0, MidpointRounding.AwayFromZero);

            var qtyAfter = s.Qty - qty;
            var valueAfter = s.Value - cost;
            if (Math.Abs(qtyAfter) <= QtyEps) { qtyAfter = 0; if (s.Qty > QtyEps) valueAfter = 0; }
            return (cost, (qtyAfter, valueAfter));
        }

        /// <summary>Đơn giá bình quân hiện tại (để gợi ý trên phiếu xuất nháp).</summary>
        public static decimal AverageCost((decimal Qty, decimal Value) s) =>
            s.Qty <= QtyEps ? 0 : decimal.Round(s.Value / s.Qty, 4, MidpointRounding.AwayFromZero);
    }
}
