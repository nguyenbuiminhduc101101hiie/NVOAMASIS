using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/si-attachment")]
[Authorize]
public class SiAttachmentController : ControllerBase
{
    private readonly AppDbContext _db;

    public SiAttachmentController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var row = await _db.SI_Attachment.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AttachmentId == id, cancellationToken);
        if (row is null)
            return NotFound();

        var contentType = string.IsNullOrWhiteSpace(row.ContentType)
            ? "application/octet-stream"
            : row.ContentType!;

        return File(row.Content, contentType, fileDownloadName: row.FileName);
    }
}
