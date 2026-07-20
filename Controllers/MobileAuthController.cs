using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NVOAMASIS.Services;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/mobile/auth")]
[IgnoreAntiforgeryToken]
public class MobileAuthController(
    TenantAuthService tenantAuth,
    MobileJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>Health check — verify route đã publish (không cần auth).</summary>
    [HttpGet("/api/mobile/ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok(new
    {
        flag = true,
        message = "mobile api ok",
        utc = DateTime.UtcNow
    });

    public record MobileLoginRequest(
        string DatabaseName,
        string SqlUserId,
        string SqlPassword,
        string AppUserName,
        string AppPassword);

    public record MobileLoginResponse(
        bool Flag,
        string Message,
        string? AccessToken,
        DateTime? ExpiresAtUtc,
        Guid? TenantId,
        string? DatabaseName,
        Guid? UsrId,
        string? Usr,
        string? Name,
        string? Email,
        string? Department);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<MobileLoginResponse>> Login([FromBody] MobileLoginRequest req, CancellationToken ct)
    {
        var rs = await tenantAuth.LoginAsync(
            req.DatabaseName,
            req.SqlUserId,
            req.SqlPassword,
            req.AppUserName,
            req.AppPassword,
            ct);

        if (!rs.Flag || rs.AuthUser == null || rs.TenantId == null)
            return Unauthorized(new MobileLoginResponse(false, rs.Message, null, null, null, null, null, null, null, null, null));

        var token = jwtTokenService.CreateToken(
            rs.AuthUser,
            rs.TenantId.Value,
            rs.TenantDatabaseName ?? req.DatabaseName,
            out var expires);

        return Ok(new MobileLoginResponse(
            true,
            rs.Message,
            token,
            expires,
            rs.TenantId,
            rs.TenantDatabaseName,
            rs.AuthUser.UsrId,
            rs.AuthUser.Usr,
            rs.AuthUser.Name,
            rs.AuthUser.Email,
            rs.AuthUser.Department));
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public ActionResult<object> Me()
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        return Ok(new
        {
            usrId = userId,
            name = User.Identity?.Name,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            tenantId = User.FindFirst(TenantClaimTypes.TenantId)?.Value,
            databaseName = User.FindFirst(TenantClaimTypes.DatabaseName)?.Value,
            department = User.FindFirst("department")?.Value
        });
    }
}
