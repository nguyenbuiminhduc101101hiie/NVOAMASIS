using System.Security.Claims;
using NVOAMASIS.Services.MultiTenant;

namespace NVOAMASIS.Services.Chat;

public readonly record struct ChatCaller(Guid UserId, string TenantKey);

/// <summary>Lấy user + tenant key từ claims (cookie web hoặc JWT) — không dùng query string.</summary>
public sealed class ChatIdentity(IConfiguration configuration)
{
    public const string DefaultTenantKey = "default";

    public ChatCaller? Resolve(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return null;

        var userId = MobileJwtTokenService.GetUserId(principal);
        if (userId is null || userId == Guid.Empty)
            return null;

        return new ChatCaller(userId.Value, GetTenantKey(principal));
    }

    private string GetTenantKey(ClaimsPrincipal principal)
    {
        // MultiTenant tắt → mọi user dùng DefaultConnection, bỏ qua claim tenant cũ (giống TenantContext).
        if (!configuration.GetValue("MultiTenant:Enabled", true))
            return DefaultTenantKey;

        var raw = principal.FindFirst(TenantClaimTypes.TenantId)?.Value;
        return Guid.TryParse(raw, out var tenantId) ? tenantId.ToString("N") : DefaultTenantKey;
    }
}
