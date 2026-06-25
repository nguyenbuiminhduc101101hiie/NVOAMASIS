using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NVOAMASIS.Data;
using QRCoder;

namespace NVOAMASIS.Services.MultiTenant;

public class TenantLoginQrService(
    RegistryDbContext registryDb,
    AppDbContext appDb,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor)
{
    public const string TokenCachePrefix = "tenant-login-qr:token:";
    public const string ActiveCachePrefix = "tenant-login-qr:active:";

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(365);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task<TenantLoginQrResult?> GetOrCreateActiveQrAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        => BuildQrAsync(user, regenerate: false, cancellationToken);

    public Task<TenantLoginQrResult?> RegenerateQrAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        => BuildQrAsync(user, regenerate: true, cancellationToken);

    public async Task<TenantLoginQrPayload?> TryResolveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var bytes = await distributedCache.GetStringAsync(TokenCachePrefix + token.Trim(), cancellationToken);
        if (string.IsNullOrWhiteSpace(bytes))
            return null;

        try
        {
            return JsonSerializer.Deserialize<TenantLoginQrTokenRecord>(bytes, JsonOptions)?.Credentials;
        }
        catch
        {
            return null;
        }
    }

    private async Task<TenantLoginQrResult?> BuildQrAsync(
        ClaimsPrincipal user,
        bool regenerate,
        CancellationToken cancellationToken)
    {
        var context = await TryResolveCredentialsAsync(user, cancellationToken);
        if (context == null)
            return null;

        string token;
        var isRegenerated = false;

        if (regenerate)
        {
            await InvalidateActiveTokenAsync(context.TenantId, context.UserId, cancellationToken);
            token = await CreateAndStoreTokenAsync(context, cancellationToken);
            isRegenerated = true;
        }
        else
        {
            var activeKey = BuildActiveCacheKey(context.TenantId, context.UserId);
            var existingToken = await distributedCache.GetStringAsync(activeKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(existingToken)
                && await TokenExistsAsync(existingToken, cancellationToken))
            {
                token = existingToken;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(existingToken))
                    await distributedCache.RemoveAsync(TokenCachePrefix + existingToken, cancellationToken);

                token = await CreateAndStoreTokenAsync(context, cancellationToken);
            }
        }

        return BuildQrResult(context.Credentials, token, isRegenerated);
    }

    private async Task<TenantLoginCredentialContext?> TryResolveCredentialsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (user.Identity?.IsAuthenticated != true)
            return null;

        var tenantIdClaim = user.FindFirst(TenantClaimTypes.TenantId)?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return null;

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(ClaimTypes.Sid)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return null;

        var appUserName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity?.Name;
        if (string.IsNullOrWhiteSpace(appUserName))
            return null;

        var tenant = await registryDb.TenantDatabases.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IsActive, cancellationToken);
        if (tenant == null)
            return null;

        var appUser = await appDb.UserList.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UsrId == userId
                    || (x.Usr != null && x.Usr.ToLower() == appUserName.Trim().ToLower()),
                cancellationToken);
        if (appUser == null || string.IsNullOrWhiteSpace(appUser.Pass_viettel))
            return null;

        var credentials = new TenantLoginQrPayload
        {
            Version = 2,
            DatabaseName = tenant.DatabaseName.Trim(),
            SqlUserId = tenant.SqlUserId.Trim(),
            SqlPassword = tenant.SqlPassword.Trim(),
            AppUserName = (appUser.Usr ?? appUserName).Trim(),
            AppPassword = appUser.Pass_viettel.Trim()
        };

        credentials.LoginUrl = BuildLoginUrl(
            credentials.DatabaseName,
            credentials.SqlUserId,
            credentials.SqlPassword,
            credentials.AppUserName,
            credentials.AppPassword);

        return new TenantLoginCredentialContext(tenantId, userId, credentials);
    }

    private async Task<string> CreateAndStoreTokenAsync(
        TenantLoginCredentialContext context,
        CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString("N");
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TokenLifetime
        };

        var record = new TenantLoginQrTokenRecord
        {
            TenantId = context.TenantId,
            UserId = context.UserId,
            Credentials = context.Credentials,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await distributedCache.SetStringAsync(
            TokenCachePrefix + token,
            JsonSerializer.Serialize(record, JsonOptions),
            cacheOptions,
            cancellationToken);

        await distributedCache.SetStringAsync(
            BuildActiveCacheKey(context.TenantId, context.UserId),
            token,
            cacheOptions,
            cancellationToken);

        return token;
    }

    private async Task InvalidateActiveTokenAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        var activeKey = BuildActiveCacheKey(tenantId, userId);
        var existingToken = await distributedCache.GetStringAsync(activeKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(existingToken))
            await distributedCache.RemoveAsync(TokenCachePrefix + existingToken, cancellationToken);

        await distributedCache.RemoveAsync(activeKey, cancellationToken);
    }

    private async Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken)
    {
        var value = await distributedCache.GetStringAsync(TokenCachePrefix + token, cancellationToken);
        return !string.IsNullOrWhiteSpace(value);
    }

    private TenantLoginQrResult BuildQrResult(TenantLoginQrPayload credentials, string token, bool isRegenerated)
    {
        var tokenUrl = $"{GetBaseUrl()}/api/tenant-login-qr/{token}";
        var qrPayload = new TenantLoginQrScanPayload
        {
            Version = 2,
            Token = token,
            TokenUrl = tokenUrl
        };

        var json = JsonSerializer.Serialize(qrPayload, JsonOptions);

        return new TenantLoginQrResult
        {
            Payload = credentials,
            Json = json,
            LoginUrl = credentials.LoginUrl,
            TokenUrl = tokenUrl,
            Token = token,
            QrImageBase64 = GenerateQrCodeBase64(json),
            IsRegenerated = isRegenerated
        };
    }

    public string BuildLoginUrl(
        string databaseName,
        string sqlUserId,
        string sqlPassword,
        string appUserName,
        string appPassword)
    {
        var q = new List<string> { "tab=login" };
        q.Add($"db={Uri.EscapeDataString(databaseName.Trim())}");
        q.Add($"sqlUser={Uri.EscapeDataString(sqlUserId.Trim())}");
        q.Add($"sqlPass={Uri.EscapeDataString(sqlPassword.Trim())}");
        q.Add($"appUser={Uri.EscapeDataString(appUserName.Trim())}");
        q.Add($"appPass={Uri.EscapeDataString(appPassword.Trim())}");

        var baseUrl = GetBaseUrl();
        return $"{baseUrl}/Account/Login?{string.Join("&", q)}";
    }

    private static string BuildActiveCacheKey(Guid tenantId, Guid userId)
        => $"{ActiveCachePrefix}{tenantId:N}:{userId:N}";

    private string GetBaseUrl()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return "https://localhost:5001";

        return $"{request.Scheme}://{request.Host.Value}";
    }

    private static string GenerateQrCodeBase64(string content, int pixelsPerModule = 6)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var pngBytes = qrCode.GetGraphic(pixelsPerModule);
        return Convert.ToBase64String(pngBytes);
    }

    private sealed record TenantLoginCredentialContext(Guid TenantId, Guid UserId, TenantLoginQrPayload Credentials);

    private sealed class TenantLoginQrTokenRecord
    {
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public TenantLoginQrPayload Credentials { get; set; } = new();
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}

public sealed class TenantLoginQrPayload
{
    public int Version { get; set; } = 2;
    public string LoginUrl { get; set; } = "";
    public string DatabaseName { get; set; } = "";
    public string SqlUserId { get; set; } = "";
    public string SqlPassword { get; set; } = "";
    public string AppUserName { get; set; } = "";
    public string AppPassword { get; set; } = "";
}

public sealed class TenantLoginQrScanPayload
{
    public int Version { get; set; } = 2;
    public string Token { get; set; } = "";
    public string TokenUrl { get; set; } = "";
}

public sealed class TenantLoginQrResult
{
    public TenantLoginQrPayload Payload { get; set; } = new();
    public string Json { get; set; } = "";
    public string LoginUrl { get; set; } = "";
    public string TokenUrl { get; set; } = "";
    public string Token { get; set; } = "";
    public string QrImageBase64 { get; set; } = "";
    public bool IsRegenerated { get; set; }
}
