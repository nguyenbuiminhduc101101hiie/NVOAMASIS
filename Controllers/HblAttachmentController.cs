using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/hbl-attachment")]
[Authorize]
public class HblAttachmentController : ControllerBase
{
    private readonly AppDbContext _db;

    public HblAttachmentController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var row = await _db.HBL_Attachment.AsNoTracking()
            .Where(x => x.AttachmentId == id)
            .Select(x => new
            {
                x.FileName,
                x.ContentType,
                x.Content,
                x.Link
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return NotFound();

        if (row.Content is { Length: > 0 })
        {
            var contentType = string.IsNullOrWhiteSpace(row.ContentType)
                ? "application/octet-stream"
                : row.ContentType!;
            return File(row.Content, contentType, fileDownloadName: row.FileName);
        }

        if (!string.IsNullOrWhiteSpace(row.Link))
        {
            var link = row.Link.Trim();
            if (Uri.TryCreate(link, UriKind.Absolute, out var absolute)
                && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return Redirect(absolute.ToString());
            }

            // Đường dẫn tương đối / UNC / file path — mở qua redirect nếu là URL tương đối trên app
            if (link.StartsWith('/') || link.StartsWith("~/"))
                return Redirect(link.TrimStart('~'));

            return Content(link, "text/plain");
        }

        return NotFound();
    }
}
