using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Controllers;

/// <summary>Tải / xem file đính kèm của báo giá 3.2 - 3.6 (yêu cầu đăng nhập).</summary>
[ApiController]
[Route("api/product-price-attachment")]
[Authorize]
public class ProductPriceAttachmentController(AppDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.ProductPriceAttachments.AsNoTracking()
            .Where(x => x.AttachmentId == id)
            .Select(x => new { x.FileName, x.ContentType, x.Content })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null || row.Content.Length == 0)
            return NotFound();

        var contentType = string.IsNullOrWhiteSpace(row.ContentType) ? "application/octet-stream" : row.ContentType;
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        // hình và PDF xem thẳng trên trình duyệt, các loại khác tải về
        var inline = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) || contentType == "application/pdf";
        return inline ? File(row.Content, contentType) : File(row.Content, contentType, row.FileName);
    }
}
