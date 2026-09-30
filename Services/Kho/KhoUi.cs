using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.JSInterop;
using NVOAMASIS.Models.Kho;

namespace NVOAMASIS.Services.Kho
{
    /// <summary>Tiện ích dùng chung cho các màn hình 10.18 (Excel, định dạng số, thông báo lỗi).</summary>
    public static class KhoUi
    {
        public static string Error(Exception ex)
        {
            while (ex.InnerException is not null) ex = ex.InnerException;
            var m = ex.Message;
            if (m.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase)
                && (m.Contains("KhoVatTu") || m.Contains("KhoHang") || m.Contains("PhieuKho")))
                return "Database chưa có bảng kho — chạy Scripts/CreateKhoTables.sql trên database này rồi tải lại trang.";
            return m;
        }

        public static string Qty(decimal v, bool showZero = false) =>
            v == 0 && !showZero ? "" : v.ToString("#,##0.####", CultureInfo.InvariantCulture);

        public static string Money(decimal v, bool showZero = false) =>
            v == 0 && !showZero ? "" : v.ToString("#,##0;-#,##0;0", CultureInfo.InvariantCulture);

        public static string Price(decimal v) =>
            v == 0 ? "" : v.ToString("#,##0.##", CultureInfo.InvariantCulture);

        public static async Task DownloadAsync(IJSRuntime js, XLWorkbook wb, string fileName)
        {
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            await js.InvokeVoidAsync("downloadFileFromStream", fileName, Convert.ToBase64String(ms.ToArray()));
        }

        // ───── Đọc Excel ─────

        private static string Norm(string s)
        {
            var d = s.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in d)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.ToString();
        }

        private static Dictionary<string, int> Headers(IXLWorksheet ws)
        {
            var map = new Dictionary<string, int>();
            var last = ws.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (var c = 1; c <= last; c++)
            {
                var h = Norm(ws.Cell(1, c).GetString());
                if (h.Length > 0 && !map.ContainsKey(h)) map[h] = c;
            }
            return map;
        }

        private static int Col(Dictionary<string, int> h, params string[] names)
        {
            foreach (var n in names)
                if (h.TryGetValue(Norm(n), out var c)) return c;
            return 0;
        }

        private static string Text(IXLWorksheet ws, int row, int col) => col == 0 ? "" : ws.Cell(row, col).GetFormattedString().Trim();

        private static decimal? Number(IXLWorksheet ws, int row, int col)
        {
            if (col == 0) return null;
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return null;
            if (cell.DataType == XLDataType.Number) return (decimal)cell.GetDouble();
            var s = cell.GetString().Trim().Replace(" ", "");
            if (s.Length == 0) return null;
            // chấp nhận 1.234,5 và 1,234.5
            if (s.Contains(',') && s.Contains('.'))
                s = s.LastIndexOf(',') > s.LastIndexOf('.') ? s.Replace(".", "").Replace(',', '.') : s.Replace(",", "");
            else if (s.Contains(',')) s = s.Replace(',', '.');
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : throw new InvalidOperationException($"Dòng {row}: '{cell.GetString()}' không phải số.");
        }

        /// <summary>Danh mục vật tư: Mã, Tên, ĐVT, TK kho, Nhóm, Tồn tối thiểu, Ghi chú.</summary>
        public static List<KhoVatTu> ReadItemsFromExcel(Stream stream)
        {
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();
            var h = Headers(ws);
            int cCode = Col(h, "Mã", "Mã vật tư", "Mã hàng", "Code"), cName = Col(h, "Tên", "Tên vật tư", "Tên hàng", "Name"),
                cUnit = Col(h, "ĐVT", "Đơn vị tính", "Đơn vị", "Unit"), cAcc = Col(h, "TK kho", "Tài khoản", "TK", "Account"),
                cCat = Col(h, "Nhóm", "Nhóm vật tư", "Category"), cMin = Col(h, "Tồn tối thiểu", "Min"), cNote = Col(h, "Ghi chú", "Note");
            if (cCode == 0 || cName == 0 || cAcc == 0)
                throw new InvalidOperationException("Dòng 1 của file phải có các cột: Mã, Tên, TK kho (tải file mẫu để xem).");
            var list = new List<KhoVatTu>();
            var last = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (var r = 2; r <= last; r++)
            {
                var code = Text(ws, r, cCode);
                var name = Text(ws, r, cName);
                if (code.Length == 0 && name.Length == 0) continue;
                list.Add(new KhoVatTu
                {
                    Code = code,
                    Name = name,
                    Unit = Text(ws, r, cUnit),
                    InventoryAccount = Text(ws, r, cAcc),
                    Category = Text(ws, r, cCat),
                    MinQty = Number(ws, r, cMin),
                    Note = Text(ws, r, cNote)
                });
            }
            return list;
        }

        public sealed record LineRow(int Row, string Code, decimal Quantity, decimal? UnitCost, decimal? Amount, decimal? VatRate, string? Note);

        /// <summary>Dòng hàng của phiếu: Mã vật tư, Số lượng, Đơn giá, Thành tiền, % thuế, Ghi chú.</summary>
        public static List<LineRow> ReadLinesFromExcel(Stream stream)
        {
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();
            var h = Headers(ws);
            int cCode = Col(h, "Mã", "Mã vật tư", "Mã hàng", "Code"), cQty = Col(h, "Số lượng", "SL", "Quantity"),
                cPrice = Col(h, "Đơn giá", "Giá", "Price"), cAmt = Col(h, "Thành tiền", "Giá trị", "Amount"),
                cVat = Col(h, "% thuế", "Thuế suất", "VAT"), cNote = Col(h, "Ghi chú", "Note");
            if (cCode == 0 || cQty == 0)
                throw new InvalidOperationException("Dòng 1 của file phải có các cột: Mã, Số lượng (và Đơn giá hoặc Thành tiền với phiếu nhập).");
            var list = new List<LineRow>();
            var last = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (var r = 2; r <= last; r++)
            {
                var code = Text(ws, r, cCode);
                if (code.Length == 0) continue;
                list.Add(new LineRow(r, code, Number(ws, r, cQty) ?? 0, Number(ws, r, cPrice), Number(ws, r, cAmt), Number(ws, r, cVat),
                    Text(ws, r, cNote) is { Length: > 0 } n ? n : null));
            }
            return list;
        }
    }
}
