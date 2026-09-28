using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Models.Hr;
using NVOAMASIS.Services.Hr;

namespace NVOAMASIS.Controllers;

/// <summary>Tải giấy tờ nhân sự — yêu cầu đăng nhập + quyền View trên HR_Employee.</summary>
[ApiController]
[Route("api/hr/documents")]
[Authorize]
public class HrDocumentController(
    HrEmployeeService employeeService,
    HrPermissionService permissionService,
    HrFileStorage fileStorage) : ControllerBase
{
    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> Download(Guid documentId, [FromQuery] bool inline = false)
    {
        var perm = await permissionService.GetAsync(User.Identity?.Name, HrMenus.Employee);
        if (!perm.View)
            return StatusCode(StatusCodes.Status403Forbidden);

        var doc = await employeeService.FindDocumentAsync(documentId);
        if (doc is null)
            return NotFound();

        Stream stream;
        try
        {
            stream = fileStorage.OpenRead(doc.FilePath);
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
        Response.Headers["Cache-Control"] = "no-store";
        // Chỉ xem inline với ảnh / PDF; loại khác luôn tải về.
        var canInline = inline &&
                        (doc.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ||
                         doc.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)) &&
                        !doc.ContentType.Contains("svg", StringComparison.OrdinalIgnoreCase);
        return canInline
            ? File(stream, doc.ContentType, enableRangeProcessing: true)
            : File(stream, doc.ContentType, fileDownloadName: doc.FileName, enableRangeProcessing: true);
    }
}
