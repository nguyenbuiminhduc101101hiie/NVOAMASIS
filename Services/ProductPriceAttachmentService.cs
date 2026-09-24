using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services;

/// <summary>Lưu / đọc / xóa file đính kèm (dạng byte trong DB) cho các màn hình báo giá 3.2 - 3.6.</summary>
public class ProductPriceAttachmentService(IDbContextFactory<AppDbContext> dbFactory)
{
    public const long MaxFileBytes = 20L * 1024 * 1024;

    public static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv"];

    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public static bool IsAllowed(string fileName)
        => AllowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);

    public static string GetContentType(string fileName)
        => ContentTypes.TryGetContentType(fileName, out var type) ? type : "application/octet-stream";

    public async Task<M_ProductPriceAttachmentItem> UploadAsync(string ownerType, IBrowserFile file, string? user, CancellationToken cancellationToken = default)
    {
        await using var input = file.OpenReadStream(MaxFileBytes, cancellationToken);
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();

        var name = Path.GetFileName(file.Name);
        var row = new M_ProductPriceAttachment
        {
            AttachmentId = Guid.NewGuid(),
            OwnerType = ownerType,
            FileName = name.Length > 500 ? name[^500..] : name,
            Content = bytes,
            // loại nội dung suy ra từ đuôi file, không tin giá trị client gửi lên
            ContentType = GetContentType(name),
            FileSize = bytes.LongLength,
            UserUpload = user,
            UploadedUtc = DateTime.UtcNow
        };

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        db.ProductPriceAttachments.Add(row);
        await db.SaveChangesAsync(cancellationToken);

        return new M_ProductPriceAttachmentItem { AttachmentId = row.AttachmentId, FileName = row.FileName, ContentType = row.ContentType, FileSize = row.FileSize };
    }

    /// <summary>Đọc metadata theo danh sách Id. Truy vấn từng Id thay vì Contains(list) vì DB mức tương thích thấp không hỗ trợ OPENJSON.</summary>
    public async Task<List<M_ProductPriceAttachmentItem>> GetItemsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var result = new List<M_ProductPriceAttachmentItem>();
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        foreach (var id in ids)
        {
            var item = await db.ProductPriceAttachments.AsNoTracking()
                .Where(x => x.AttachmentId == id)
                .Select(x => new M_ProductPriceAttachmentItem { AttachmentId = x.AttachmentId, FileName = x.FileName, ContentType = x.ContentType, FileSize = x.FileSize })
                .FirstOrDefaultAsync(cancellationToken);
            if (item != null)
                result.Add(item);
        }

        return result;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ProductPriceAttachments.FirstOrDefaultAsync(x => x.AttachmentId == id, cancellationToken);
        if (row == null)
            return;

        db.ProductPriceAttachments.Remove(row);
        await db.SaveChangesAsync(cancellationToken);
    }
}
