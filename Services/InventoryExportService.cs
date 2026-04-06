using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;

namespace NVOAMASIS.Services;

/// <summary>
/// DTO for inventory export rows. Used by InventoryExportService.
/// </summary>
public class InventoryExportRow
{
    public string? Location { get; set; }
    public int ContainerCount { get; set; }
    public IReadOnlyDictionary<string, int> SizeCounts { get; set; } = new Dictionary<string, int>();
    public int DecommissionCount { get; set; }

    public int GetSizeCount(string sizeType)
    {
        if (string.IsNullOrWhiteSpace(sizeType)) return 0;
        return SizeCounts.TryGetValue(sizeType, out var c) ? c : 0;
    }
}

public class InventoryExportService
{
    public byte[] ExportToExcel(
        IReadOnlyList<InventoryExportRow> depotRows,
        IReadOnlyList<InventoryExportRow> portRows,
        IReadOnlyList<string> sizeTypes,
        string locationColumnDepot = "IN DEPOT",
        string locationColumnPort = "IN PORT",
        string decommisionHeader = "Decommision")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            AddSheet(package, "IN DEPOT Containers", depotRows, sizeTypes, locationColumnDepot, decommisionHeader);
            AddSheet(package, "IN PORT Containers", portRows, sizeTypes, locationColumnPort, decommisionHeader);
            package.Save();
        }
        return stream.ToArray();
    }

    public byte[] ExportSingleTableToExcel(
        IReadOnlyList<InventoryExportRow> rows,
        IReadOnlyList<string> sizeTypes,
        string sheetName,
        string locationHeader,
        string decommisionHeader = "Decommision")
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var stream = new MemoryStream();
        using (var package = new ExcelPackage(stream))
        {
            AddSheet(package, sheetName, rows, sizeTypes, locationHeader, decommisionHeader);
            package.Save();
        }

        return stream.ToArray();
    }

    private static void AddSheet(
        ExcelPackage package,
        string sheetName,
        IReadOnlyList<InventoryExportRow> rows,
        IReadOnlyList<string> sizeTypes,
        string locationHeader,
        string decommisionHeader)
    {
        var ws = package.Workbook.Worksheets.Add(sheetName);
        int col = 1;
        int row = 1;

        ws.Cells[row, 1].Value = sheetName;
        ws.Cells[row, 1].Style.Font.Bold = true;
        ws.Cells[row, 1].Style.Font.Size = 14;
        row += 2;

        ws.Cells[row, col++].Value = locationHeader;
        ws.Cells[row, col++].Value = "CONTAINER COUNT";
        foreach (var st in sizeTypes)
            ws.Cells[row, col++].Value = st;
        ws.Cells[row, col].Value = decommisionHeader;
        var headerRow = row;
        row++;

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            col = 1;
            ws.Cells[row, col++].Value = r.Location ?? "";
            ws.Cells[row, col++].Value = r.ContainerCount;
            foreach (var st in sizeTypes)
                ws.Cells[row, col++].Value = r.GetSizeCount(st);
            ws.Cells[row, col].Value = r.DecommissionCount;
            row++;
        }

        if (row > headerRow + 1)
        {
            using (var range = ws.Cells[headerRow, 1, row - 1, 1 + 2 + sizeTypes.Count])
            {
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }
            ws.Cells[headerRow, 1, headerRow, 1 + 2 + sizeTypes.Count].Style.Font.Bold = true;
        }

        ws.Cells[ws.Dimension?.Address ?? "A1"].AutoFitColumns();
    }

    public string GetPrintHtml(
        IReadOnlyList<InventoryExportRow> depotRows,
        IReadOnlyList<InventoryExportRow> portRows,
        IReadOnlyList<string> sizeTypes,
        string locationColumnDepot = "IN DEPOT",
        string locationColumnPort = "IN PORT",
        string decommisionHeader = "Decommision")
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        sb.Append("<title>Container Inventory</title>");
        sb.Append("<style>");
        sb.Append("body{font-family:Segoe UI,Arial,sans-serif;margin:16px;}");
        sb.Append("h2{margin-top:24px;margin-bottom:8px;}");
        sb.Append("table{border-collapse:collapse;width:100%;margin-bottom:24px;}");
        sb.Append("th,td{border:1px solid #333;padding:6px 10px;text-align:left;}");
        sb.Append("th{background:#eee;font-weight:bold;}");
        sb.Append("</style></head><body>");

        AppendTable(sb, "IN DEPOT Containers", depotRows, sizeTypes, locationColumnDepot, decommisionHeader);
        AppendTable(sb, "IN PORT Containers", portRows, sizeTypes, locationColumnPort, decommisionHeader);

        sb.Append("</body></html>");
        return sb.ToString();
    }

    public string GetSingleTablePrintHtml(
        IReadOnlyList<InventoryExportRow> rows,
        IReadOnlyList<string> sizeTypes,
        string title,
        string locationHeader,
        string decommisionHeader = "Decommision")
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'/>");
        sb.Append("<title>");
        sb.Append(System.Net.WebUtility.HtmlEncode(title));
        sb.Append("</title>");
        sb.Append("<style>");
        sb.Append("body{font-family:Segoe UI,Arial,sans-serif;margin:16px;}");
        sb.Append("h2{margin-top:0;margin-bottom:8px;}");
        sb.Append("table{border-collapse:collapse;width:100%;margin-bottom:0;}");
        sb.Append("th,td{border:1px solid #333;padding:6px 10px;text-align:left;}");
        sb.Append("th{background:#eee;font-weight:bold;}");
        sb.Append("</style></head><body>");

        AppendTable(sb, title, rows, sizeTypes, locationHeader, decommisionHeader);

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static void AppendTable(
        StringBuilder sb,
        string title,
        IReadOnlyList<InventoryExportRow> rows,
        IReadOnlyList<string> sizeTypes,
        string locationHeader,
        string decommisionHeader)
    {
        sb.AppendFormat("<h2>{0}</h2>", System.Net.WebUtility.HtmlEncode(title));
        sb.Append("<table><thead><tr>");
        sb.AppendFormat("<th>{0}</th>", System.Net.WebUtility.HtmlEncode(locationHeader));
        sb.Append("<th>CONTAINER COUNT</th>");
        foreach (var st in sizeTypes)
            sb.AppendFormat("<th>{0}</th>", System.Net.WebUtility.HtmlEncode(st));
        sb.AppendFormat("<th>{0}</th>", System.Net.WebUtility.HtmlEncode(decommisionHeader));
        sb.Append("</tr></thead><tbody>");

        foreach (var r in rows)
        {
            sb.Append("<tr>");
            sb.AppendFormat("<td>{0}</td>", System.Net.WebUtility.HtmlEncode(r.Location ?? ""));
            sb.AppendFormat("<td>{0}</td>", r.ContainerCount);
            foreach (var st in sizeTypes)
                sb.AppendFormat("<td>{0}</td>", r.GetSizeCount(st));
            sb.AppendFormat("<td>{0}</td>", r.DecommissionCount);
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table>");
    }
}
