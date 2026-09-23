using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NVOAMASIS.Models;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Services;

public class MobileJwtTokenService(IOptions<JwtSettings> jwtOptions)
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public string CreateToken(AuthUser user, Guid tenantId, string databaseName, out DateTime expiresAtUtc)
    {
        var minutes = _settings.ExpiryMinutes > 0 ? _settings.ExpiryMinutes : 60 * 24 * 7;
        expiresAtUtc = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name ?? user.Usr ?? ""),
            new(ClaimTypes.NameIdentifier, user.UsrId.ToString()),
            new(ClaimTypes.Sid, user.UsrId.ToString()),
            new("usr", user.Usr ?? ""),
            new(TenantClaimTypes.TenantId, tenantId.ToString()),
            new(TenantClaimTypes.DatabaseName, databaseName ?? "")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrWhiteSpace(user.Department))
            claims.Add(new Claim("department", user.Department));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// JWT ngắn hạn để HubConnection phía server (MainLayout) xác thực với /notificationhub
    /// thay cho ?userid= (giả mạo được). Token chỉ tồn tại trên server, không gửi xuống trình duyệt.
    /// </summary>
    public string CreateHubToken(ClaimsPrincipal principal)
    {
        var userId = GetUserId(principal) ?? throw new InvalidOperationException("User id claim missing.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, principal.Identity?.Name ?? ""),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Sid, userId.ToString())
        };
        foreach (var type in new[] { TenantClaimTypes.TenantId, TenantClaimTypes.DatabaseName })
        {
            var value = principal.FindFirstValue(type);
            if (!string.IsNullOrWhiteSpace(value))
                claims.Add(new Claim(type, value));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static Guid? GetUserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.Sid)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
