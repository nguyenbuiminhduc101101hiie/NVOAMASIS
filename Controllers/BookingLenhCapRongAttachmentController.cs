using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/booking-lenh-cap-rong-attachment")]
[Authorize]
public class BookingLenhCapRongAttachmentController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingLenhCapRongAttachmentController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var row = await _db.Booking_LenhCapRong_Attachment.AsNoTracking()
            .Where(x => x.AttachmentId == id)
            .Select(x => new
            {
                x.FileName,
                x.ContentType,
                x.Content
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null || row.Content is not { Length: > 0 })
            return NotFound();

        var contentType = string.IsNullOrWhiteSpace(row.ContentType)
            ? "application/octet-stream"
            : row.ContentType!;

        return File(row.Content, contentType, fileDownloadName: row.FileName);
    }
}
