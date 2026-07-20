using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/mobile/device")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken]
public class MobileDeviceController(AppDbContext db) : ControllerBase
{
    public record RegisterDeviceRequest(string Token, string? Platform);

    [HttpPost("push-token")]
    public async Task<ActionResult<object>> Register([FromBody] RegisterDeviceRequest req)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { message = "Token rỗng." });

        db.ChangeTracker.Clear();
        var token = req.Token.Trim();
        var existing = await db.DevicePushTokens.FirstOrDefaultAsync(x => x.Token == token);
        if (existing == null)
        {
            db.DevicePushTokens.Add(new DevicePushToken
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                Token = token,
                Platform = req.Platform?.Trim()?.ToLowerInvariant(),
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.UserId = userId.Value;
            existing.Platform = req.Platform?.Trim()?.ToLowerInvariant();
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return Ok(new { flag = true, message = "Đã đăng ký device token." });
    }

    [HttpDelete("push-token")]
    public async Task<ActionResult<object>> Unregister([FromBody] RegisterDeviceRequest req)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { message = "Token rỗng." });

        db.ChangeTracker.Clear();
        var existing = await db.DevicePushTokens
            .Where(x => x.Token == req.Token.Trim() && x.UserId == userId)
            .ToListAsync();
        if (existing.Count > 0)
        {
            db.DevicePushTokens.RemoveRange(existing);
            await db.SaveChangesAsync();
        }

        return Ok(new { flag = true, message = "Đã gỡ device token." });
    }
}
