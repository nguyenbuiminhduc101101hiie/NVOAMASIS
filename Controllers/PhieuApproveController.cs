using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

/// <summary>
/// Public email approval page for PhieuThu / PhieuChi.
/// URL: /api/phieu-approve/{token}?uid={userId}
/// </summary>
[ApiController]
[Route("api/phieu-approve")]
public class PhieuApproveController(PhieuApproveService approveService) : ControllerBase
{
    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage(string token, [FromQuery] string? uid)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Content("<h3>Token không hợp lệ</h3>", "text/html; charset=utf-8");

        Guid? userId = null;
        if (Guid.TryParse(uid, out var parsed))
            userId = parsed;

        var html = await approveService.BuildDecisionPageHtmlAsync(token, userId);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("{token}/decide")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Decide(
        string token,
        [FromForm] string? uid,
        [FromForm] string? action,
        [FromForm] string? remarks)
    {
        if (string.IsNullOrWhiteSpace(token) || !Guid.TryParse(uid, out var userId))
            return Content("<h3>Thiếu thông tin người duyệt</h3>", "text/html; charset=utf-8");

        var html = await approveService.ProcessDecisionHtmlAsync(
            token,
            userId,
            action ?? "",
            remarks);

        return Content(html, "text/html; charset=utf-8");
    }
}
