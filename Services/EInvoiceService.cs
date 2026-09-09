using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Response;
using Microsoft.EntityFrameworkCore;

namespace NVOAMASIS.Services
{
    public class EInvoiceService
    {
        private readonly AppDbContext _context;

        public EInvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<M_EInvoiceExportLog>> GetExportLogsAsync(Guid? hblId = null, int take = 200)
        {
            _context.ChangeTracker.Clear();
            var query = _context.EInvoiceExportLog
                .Where(x => x.Continued)
                .AsQueryable();

            if (hblId.HasValue && hblId.Value != Guid.Empty)
                query = query.Where(x => x.HblId == hblId);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<BoolandMessReponse> CreateExportLogAsync(M_EInvoiceExportLog log)
        {
            try
            {
                _context.ChangeTracker.Clear();
                if (log.EInvoiceExportLogId == Guid.Empty)
                    log.EInvoiceExportLogId = Guid.NewGuid();

                if (log.CreatedAt == default)
                    log.CreatedAt = DateTime.Now;

                _context.EInvoiceExportLog.Add(log);
                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Lưu lịch sử HDDT thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không lưu được lịch sử HDDT: " + ex.Message);
            }
        }

        public async Task<HashSet<string>> GetPublishedSoHoaDonNoiBoAsync(Guid? hblId = null)
        {
            _context.ChangeTracker.Clear();
            var query = _context.EInvoiceExportLog
                .Where(x => x.Continued && x.IsPublished && !string.IsNullOrEmpty(x.SoHoaDonNoiBo))
                .AsQueryable();

            if (hblId.HasValue && hblId.Value != Guid.Empty)
                query = query.Where(x => x.HblId == hblId);

            var list = await query
                .Select(x => x.SoHoaDonNoiBo!)
                .ToListAsync();

            return list
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<BoolandMessReponse> MarkAsPublishedAsync(IReadOnlyList<string> invoiceIds, string publishedBy)
        {
            if (invoiceIds.Count == 0)
                return new BoolandMessReponse(true, "Không có invoiceId để đánh dấu phát hành.");

            try
            {
                _context.ChangeTracker.Clear();
                var idSet = invoiceIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var logs = await _context.EInvoiceExportLog
                    .Where(x => x.Continued && x.InvoiceId != null && idSet.Contains(x.InvoiceId))
                    .ToListAsync();

                if (logs.Count == 0)
                    return new BoolandMessReponse(true, "Không tìm thấy lịch sử HDDT tương ứng.");

                var now = DateTime.Now;
                foreach (var log in logs)
                {
                    log.IsPublished = true;
                    log.PublishedAt = now;
                    log.PublishedBy = publishedBy;
                }

                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, $"Đã đánh dấu phát hành {logs.Count} hóa đơn.");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không đánh dấu được trạng thái phát hành: " + ex.Message);
            }
        }

        public async Task<List<M_EInvoiceExportLog>> GetExportLogsByInvoiceIdsAsync(IReadOnlyList<string> invoiceIds)
        {
            if (invoiceIds.Count == 0)
                return new List<M_EInvoiceExportLog>();

            _context.ChangeTracker.Clear();
            var idSet = invoiceIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return await _context.EInvoiceExportLog
                .Where(x => x.Continued && x.InvoiceId != null && idSet.Contains(x.InvoiceId))
                .ToListAsync();
        }

        public async Task<BoolandMessReponse> UpdateExportLogFilesAsync(M_EInvoiceExportLog log)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var existing = await _context.EInvoiceExportLog
                    .FirstOrDefaultAsync(x => x.EInvoiceExportLogId == log.EInvoiceExportLogId);

                if (existing == null)
                    return new BoolandMessReponse(false, "Không tìm thấy lịch sử HDDT để cập nhật file.");

                existing.PdfFileName = log.PdfFileName;
                existing.PdfFileContent = log.PdfFileContent;
                existing.XmlFileName = log.XmlFileName;
                existing.XmlFileContent = log.XmlFileContent;

                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, "Cập nhật file PDF/XML thành công");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không cập nhật được file PDF/XML: " + ex.Message);
            }
        }

        public async Task<BoolandMessReponse> DeactivateExportLogsByInvoiceIdsAsync(IReadOnlyList<string> invoiceIds)
        {
            if (invoiceIds.Count == 0)
                return new BoolandMessReponse(true, "Không có invoiceId để cập nhật lịch sử.");

            try
            {
                _context.ChangeTracker.Clear();
                var idSet = invoiceIds
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var logs = await _context.EInvoiceExportLog
                    .Where(x => x.Continued && x.InvoiceId != null && idSet.Contains(x.InvoiceId))
                    .ToListAsync();

                if (logs.Count == 0)
                    return new BoolandMessReponse(true, "Không tìm thấy lịch sử HDDT tương ứng.");

                foreach (var log in logs)
                    log.Continued = false;

                await _context.SaveChangesAsync();
                return new BoolandMessReponse(true, $"Đã ẩn {logs.Count} bản ghi lịch sử HDDT.");
            }
            catch (Exception ex)
            {
                return new BoolandMessReponse(false, "Không cập nhật được lịch sử HDDT: " + ex.Message);
            }
        }
    }
}
