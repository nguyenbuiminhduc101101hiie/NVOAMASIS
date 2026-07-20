using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

/// <summary>
/// Public email approval page for PhieuThu / PhieuChi.
/// URL: /api/phieu-approve/{tenantId}/{token}?uid={userId}
/// </summary>
[ApiController]
[Route("api/phieu-approve")]
public class PhieuApproveController(PhieuApproveService approveService) : ControllerBase
{
    [HttpGet("{tenantId:guid}/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage(Guid tenantId, string token, [FromQuery] string? uid)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Content("<h3>Token không hợp lệ</h3>", "text/html; charset=utf-8");

        Guid? userId = null;
        if (Guid.TryParse(uid, out var parsed))
            userId = parsed;

        var html = await approveService.BuildDecisionPageHtmlAsync(tenantId, token, userId);
        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("{tenantId:guid}/{token}/decide")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Decide(
        Guid tenantId,
        string token,
        [FromForm] string? uid,
        [FromForm] string? action,
        [FromForm] string? remarks)
    {
        if (string.IsNullOrWhiteSpace(token) || !Guid.TryParse(uid, out var userId))
            return Content("<h3>Thiếu thông tin người duyệt</h3>", "text/html; charset=utf-8");

        var html = await approveService.ProcessDecisionHtmlAsync(
            tenantId,
            token,
            userId,
            action ?? "",
            remarks);

        return Content(html, "text/html; charset=utf-8");
    }
}
