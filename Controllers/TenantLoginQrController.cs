using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Controllers;

/// <summary>
/// Public API: điện thoại quét QR gọi endpoint này để lấy thông tin đăng nhập theo token.
/// Token cũ sẽ trả 404 sau khi user bấm "Tạo lại QR" trên màn 1.18.
/// </summary>
[ApiController]
[Route("api/tenant-login-qr")]
public class TenantLoginQrController(TenantLoginQrService loginQrService) : ControllerBase
{
    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLoginInfo(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token không hợp lệ." });

        var payload = await loginQrService.TryResolveTokenAsync(token, cancellationToken);
        if (payload == null)
            return NotFound(new { message = "Mã QR đã hết hiệu lực hoặc đã được tạo lại. Vui lòng quét mã QR mới trên máy tính." });

        return Ok(payload);
    }
}
