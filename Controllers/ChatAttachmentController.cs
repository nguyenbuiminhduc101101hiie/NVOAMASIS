using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services.Chat;

namespace NVOAMASIS.Controllers;

/// <summary>Tải file đính kèm chat — chỉ thành viên hội thoại (user lấy từ cookie claim, không từ query).</summary>
[ApiController]
[Route("api/chat/attachments")]
[Authorize]
public class ChatAttachmentController(
    ChatService chatService,
    ChatIdentity chatIdentity,
    ChatFileStorage fileStorage) : ControllerBase
{
    [HttpGet("{messageId:guid}")]
    public async Task<IActionResult> Download(Guid messageId, [FromQuery] bool inline = false)
    {
        if (chatIdentity.Resolve(User) is not { } caller)
            return Unauthorized();

        var attachment = await chatService.FindAttachmentAsync(caller, messageId);
        if (attachment is not { } file)
            return NotFound();

        Stream stream;
        try
        {
            stream = fileStorage.OpenRead(file.Path);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (DirectoryNotFoundException)
        {
            return NotFound();
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        // Chỉ hiển thị inline với ảnh; loại khác luôn tải về để tránh chạy HTML/SVG do user upload.
        var canInline = inline &&
                        file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                        !file.ContentType.Contains("svg", StringComparison.OrdinalIgnoreCase);
        return canInline
            ? File(stream, file.ContentType, enableRangeProcessing: true)
            : File(stream, file.ContentType, fileDownloadName: file.Name, enableRangeProcessing: true);
    }
}
