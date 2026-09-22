using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;

namespace NVOAMASIS.Services
{
    public class LccPolVietnamImportResult
    {
        public bool Flag { get; set; }
        public string Message { get; set; } = "";
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
    }

    public class LccPolVietnamImportedFile
    {
        public string FileName { get; set; } = "";
        public int RowCount { get; set; }
        public DateTime? LastImportDate { get; set; }
    }

    /// <summary>Một dòng dữ liệu đã đọc được từ Excel, chưa ghi vào DB.</summary>
    public class LccPolVietnamParsedRow
    {
        public string Carrier { get; set; } = "";
        public string ChargeType { get; set; } = "";
        public string? Container20DC { get; set; }
        public string? Container40HC { get; set; }
    }

    /// <summary>Kết quả đọc file, dùng để hiển thị preview trước khi commit vào DB.</summary>
    public class LccPolVietnamParseResult
    {
        public bool Flag { get; set; }
        public string Message { get; set; } = "";
        public List<LccPolVietnamParsedRow> Rows { get; set; } = new();
    }

    /// <summary>Một dòng preview: so sánh dữ liệu mới đọc từ Excel với dữ liệu hiện có trong DB.</summary>
    public class LccPolVietnamPreviewRow
    {
        public string Carrier { get; set; } = "";
        public string ChargeType { get; set; } = "";
        public string? NewContainer20DC { get; set; }
        public string? NewContainer40HC { get; set; }
        public string? OldContainer20DC { get; set; }
        public string? OldContainer40HC { get; set; }
        public bool IsNew { get; set; }
        public bool HasChange { get; set; }
    }

    public class LccPolVietnamPreview
    {
        public bool Flag { get; set; }
        public string Message { get; set; } = "";
        public List<LccPolVietnamPreviewRow> Rows { get; set; } = new();
        public int InsertCount { get; set; }
        public int UpdateCount { get; set; }
        public int UnchangedCount { get; set; }
    }

    public class LccPolVietnamServices(AppDbContext _context, HistoryLogService HistoryLogService)
    {
        public async Task<List<M_LccPolVietnam>> GetList()
        {
            try
            {
                _context.ChangeTracker.Clear();
                return await _context.LccPolVietnam
                    .OrderBy(x => x.ChargeType)
                    .ThenBy(x => x.Carrier)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<M_LccPolVietnam>();
            }
        }

        public async Task<BoolandMessReponse> Create(M_LccPolVietnam item, string usr)
        {
            try
            {
                _context.ChangeTracker.Clear();
                item.Id = Guid.NewGuid();
                item.Userupdate = usr;
                item.Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                item.CreatedDate = DateTime.Now;
                _context.LccPolVietnam.Add(item);
                await _context.SaveChangesAsync();

                await HistoryLogService.LogAsync(usr, "ADD LCC POL Vietnam", "LCC_POL_Vietnam", item.Id, item.Carrier, item);
                return new BoolandMessReponse(true, "Tạo mới thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không thể tạo mới, lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> Update(M_LccPolVietnam item, M_LccPolVietnam oldItem, string usr)
        {
            try
            {
                _context.ChangeTracker.Clear();
                item.Userupdate = usr;
                item.Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                _context.LccPolVietnam.Update(item);
                await _context.SaveChangesAsync();

                await HistoryLogService.LogAsync(usr, "UPDATE LCC POL Vietnam", "LCC_POL_Vietnam", item.Id, item.Carrier, new { OldData = oldItem, NewData = item });
                return new BoolandMessReponse(true, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không thể cập nhật, lỗi: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> Delete(M_LccPolVietnam item, string usr)
        {
            try
            {
                if (item?.Id == null || item.Id == Guid.Empty)
                    return new BoolandMessReponse(false, "Không có dữ liệu để xóa");

                _context.ChangeTracker.Clear();
                _context.LccPolVietnam.Remove(item);
                await _context.SaveChangesAsync();

                await HistoryLogService.LogAsync(usr, "DELETE LCC POL Vietnam", "LCC_POL_Vietnam", item.Id, item.Carrier, new { OldData = item });
                return new BoolandMessReponse(true, "Xóa thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không thể xóa, lỗi: " + ex.Message);
            }
        }

        public async Task<List<LccPolVietnamImportedFile>> GetImportedFiles()
        {
            try
            {
                _context.ChangeTracker.Clear();
                return await _context.LccPolVietnam
                    .Where(x => x.SourceFileName != null && x.SourceFileName != "")
                    .GroupBy(x => x.SourceFileName)
                    .Select(g => new LccPolVietnamImportedFile
                    {
                        FileName = g.Key!,
                        RowCount = g.Count(),
                        LastImportDate = g.Max(x => x.CreatedDate)
                    })
                    .OrderByDescending(x => x.LastImportDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<LccPolVietnamImportedFile>();
            }
        }

        public async Task<BoolandMessReponse> DeleteByFileName(string fileName, string usr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return new BoolandMessReponse(false, "Không có tên file để xóa");

                _context.ChangeTracker.Clear();
                var rows = await _context.LccPolVietnam
                    .Where(x => x.SourceFileName == fileName)
                    .ToListAsync();

                if (rows.Count == 0)
                    return new BoolandMessReponse(false, "Không tìm thấy dữ liệu của file này");

                _context.LccPolVietnam.RemoveRange(rows);
                await _context.SaveChangesAsync();

                await HistoryLogService.LogAsync(usr, "DELETE BY FILE LCC POL Vietnam", "LCC_POL_Vietnam", null, fileName, new { DeletedCount = rows.Count, FileName = fileName });
                return new BoolandMessReponse(true, $"Đã xóa {rows.Count} dòng thuộc file '{fileName}'");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không thể xóa, lỗi: " + ex.Message);
            }
        }

        /// <summary>
        /// Đọc (không ghi DB) dữ liệu từ sheet đầu tiên ("POL LOCAL") của file Excel "LOCAL CHARGE POL VIET NAM".
        /// Cấu trúc: dòng 1 = tên hãng tàu (merge theo cặp cột), dòng 2 = "20DC"/"40HC", cột A từ dòng 3 = tên loại phí.
        /// </summary>
        public LccPolVietnamParseResult ParseExcelSheet1(Stream excelStream)
        {
            try
            {
                using var workbook = new XLWorkbook(excelStream);
                if (workbook.Worksheets.Count == 0)
                    return new LccPolVietnamParseResult { Flag = false, Message = "File Excel không có sheet nào." };

                var worksheet = workbook.Worksheet(1); // chỉ import sheet 1

                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

                if (lastRow < 3 || lastColumn < 2)
                    return new LccPolVietnamParseResult { Flag = false, Message = "Sheet 1 không đủ dữ liệu để import." };

                // Build map: column -> (Carrier, IsDC)
                var columnMap = new List<(int Col, string Carrier, bool IsDC)>();
                string currentCarrier = "";
                for (int col = 2; col <= lastColumn; col++)
                {
                    var carrierCell = worksheet.Cell(1, col).GetFormattedString().Trim();
                    if (!string.IsNullOrWhiteSpace(carrierCell))
                        currentCarrier = carrierCell;

                    var sizeLabel = worksheet.Cell(2, col).GetFormattedString().Trim().ToUpperInvariant();
                    if (sizeLabel != "20DC" && sizeLabel != "40HC")
                        continue;

                    if (string.IsNullOrWhiteSpace(currentCarrier))
                        continue;

                    columnMap.Add((col, currentCarrier, sizeLabel == "20DC"));
                }

                if (columnMap.Count == 0)
                    return new LccPolVietnamParseResult { Flag = false, Message = "Không tìm thấy cột hãng tàu hợp lệ (20DC/40HC) trong dòng 1-2 của sheet 1." };

                var rows = new List<LccPolVietnamParsedRow>();
                int blankStreak = 0;

                for (int row = 3; row <= lastRow; row++)
                {
                    var chargeType = worksheet.Cell(row, 1).GetFormattedString().Trim();
                    if (string.IsNullOrWhiteSpace(chargeType))
                    {
                        blankStreak++;
                        if (blankStreak >= 2)
                            break; // hết bảng chính, phần bên dưới (FWD, HECNY...) không thuộc bảng giá theo hãng tàu
                        continue;
                    }
                    blankStreak = 0;

                    foreach (var carrierGroup in columnMap.GroupBy(x => x.Carrier))
                    {
                        var dcCol = carrierGroup.FirstOrDefault(x => x.IsDC).Col;
                        var hcCol = carrierGroup.FirstOrDefault(x => !x.IsDC).Col;

                        var dcValue = dcCol > 0 ? worksheet.Cell(row, dcCol).GetFormattedString().Trim() : "";
                        var hcValue = hcCol > 0 ? worksheet.Cell(row, hcCol).GetFormattedString().Trim() : "";

                        if (string.IsNullOrWhiteSpace(dcValue) && string.IsNullOrWhiteSpace(hcValue))
                            continue;

                        rows.Add(new LccPolVietnamParsedRow
                        {
                            Carrier = carrierGroup.Key.Trim(),
                            ChargeType = chargeType,
                            Container20DC = string.IsNullOrWhiteSpace(dcValue) ? null : dcValue,
                            Container40HC = string.IsNullOrWhiteSpace(hcValue) ? null : hcValue
                        });
                    }
                }

                if (rows.Count == 0)
                    return new LccPolVietnamParseResult { Flag = false, Message = "Không đọc được dòng dữ liệu nào từ sheet 1." };

                return new LccPolVietnamParseResult { Flag = true, Message = $"Đọc được {rows.Count} dòng.", Rows = rows };
            }
            catch (Exception ex)
            {
                return new LccPolVietnamParseResult { Flag = false, Message = "Đọc file thất bại, lỗi: " + ex.Message };
            }
        }

        /// <summary>So sánh các dòng đã đọc từ Excel với dữ liệu hiện có trong DB để hiển thị preview trước khi import.</summary>
        public async Task<LccPolVietnamPreview> BuildPreview(List<LccPolVietnamParsedRow> parsedRows)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var existing = await _context.LccPolVietnam.AsNoTracking().ToListAsync();
                var existingMap = existing.ToDictionary(
                    x => (x.Carrier ?? "").Trim().ToUpperInvariant() + "|" + (x.ChargeType ?? "").Trim().ToUpperInvariant(),
                    x => x);

                var preview = new LccPolVietnamPreview { Flag = true };

                foreach (var row in parsedRows)
                {
                    var key = row.Carrier.Trim().ToUpperInvariant() + "|" + row.ChargeType.Trim().ToUpperInvariant();
                    var previewRow = new LccPolVietnamPreviewRow
                    {
                        Carrier = row.Carrier,
                        ChargeType = row.ChargeType,
                        NewContainer20DC = row.Container20DC,
                        NewContainer40HC = row.Container40HC
                    };

                    if (existingMap.TryGetValue(key, out var found))
                    {
                        previewRow.IsNew = false;
                        previewRow.OldContainer20DC = found.Container20DC;
                        previewRow.OldContainer40HC = found.Container40HC;

                        var newDc = string.IsNullOrWhiteSpace(row.Container20DC) ? found.Container20DC : row.Container20DC;
                        var newHc = string.IsNullOrWhiteSpace(row.Container40HC) ? found.Container40HC : row.Container40HC;
                        previewRow.HasChange = newDc != found.Container20DC || newHc != found.Container40HC;

                        if (previewRow.HasChange) preview.UpdateCount++;
                        else preview.UnchangedCount++;
                    }
                    else
                    {
                        previewRow.IsNew = true;
                        previewRow.HasChange = true;
                        preview.InsertCount++;
                    }

                    preview.Rows.Add(previewRow);
                }

                preview.Message = $"{preview.InsertCount} dòng mới, {preview.UpdateCount} dòng thay đổi, {preview.UnchangedCount} dòng không đổi.";
                return preview;
            }
            catch (Exception ex)
            {
                return new LccPolVietnamPreview { Flag = false, Message = "Không thể tạo preview, lỗi: " + ex.Message };
            }
        }

        /// <summary>
        /// Ghi các dòng đã đọc/đã preview vào DB. Upsert theo cặp (Carrier, ChargeType);
        /// không xóa các dòng CRUD thủ công không khớp. Mỗi dòng ghi nhận SourceFileName.
        /// </summary>
        public async Task<LccPolVietnamImportResult> CommitImport(List<LccPolVietnamParsedRow> parsedRows, string usr, string fileName)
        {
            try
            {
                if (parsedRows == null || parsedRows.Count == 0)
                    return new LccPolVietnamImportResult { Flag = false, Message = "Không có dữ liệu để import." };

                _context.ChangeTracker.Clear();
                var existing = await _context.LccPolVietnam.ToListAsync();
                var existingMap = existing.ToDictionary(
                    x => (x.Carrier ?? "").Trim().ToUpperInvariant() + "|" + (x.ChargeType ?? "").Trim().ToUpperInvariant(),
                    x => x);

                int inserted = 0, updated = 0;

                foreach (var row in parsedRows)
                {
                    var key = row.Carrier.Trim().ToUpperInvariant() + "|" + row.ChargeType.Trim().ToUpperInvariant();
                    if (existingMap.TryGetValue(key, out var found))
                    {
                        found.Container20DC = string.IsNullOrWhiteSpace(row.Container20DC) ? found.Container20DC : row.Container20DC;
                        found.Container40HC = string.IsNullOrWhiteSpace(row.Container40HC) ? found.Container40HC : row.Container40HC;
                        found.Userupdate = usr;
                        found.Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy");
                        found.SourceFileName = fileName;
                        updated++;
                    }
                    else
                    {
                        var newItem = new M_LccPolVietnam
                        {
                            Id = Guid.NewGuid(),
                            Carrier = row.Carrier,
                            ChargeType = row.ChargeType,
                            Container20DC = row.Container20DC,
                            Container40HC = row.Container40HC,
                            Userupdate = usr,
                            Dateupdate = DateTime.Now.ToString("dd/MMM/yyyy"),
                            CreatedDate = DateTime.Now,
                            SourceFileName = fileName
                        };
                        _context.LccPolVietnam.Add(newItem);
                        existingMap[key] = newItem;
                        inserted++;
                    }
                }

                await _context.SaveChangesAsync();
                await HistoryLogService.LogAsync(usr, "IMPORT EXCEL LCC POL Vietnam", "LCC_POL_Vietnam", null, fileName, new { Inserted = inserted, Updated = updated, FileName = fileName });

                return new LccPolVietnamImportResult
                {
                    Flag = true,
                    Message = $"Import thành công: {inserted} dòng mới, {updated} dòng cập nhật.",
                    InsertedCount = inserted,
                    UpdatedCount = updated
                };
            }
            catch (Exception ex)
            {
                return new LccPolVietnamImportResult { Flag = false, Message = "Import thất bại, lỗi: " + ex.Message };
            }
        }
    }
}
