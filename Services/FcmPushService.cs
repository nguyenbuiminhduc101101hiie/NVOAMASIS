using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NVOAMASIS.Data;

namespace NVOAMASIS.Services;

public class FcmOptions
{
    /// <summary>Bật gửi FCM HTTP v1.</summary>
    public bool Enabled { get; set; }

    /// <summary>Firebase project_id (vd. lms-qrscan).</summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Đường dẫn file service account JSON
    /// (Firebase → Project settings → Service accounts → Generate new private key).
    /// Có thể absolute hoặc relative tới ContentRoot.
    /// </summary>
    public string? ServiceAccountPath { get; set; }

    /// <summary>Deprecated — Legacy Server Key. Không dùng với FCM V1.</summary>
    public string? ServerKey { get; set; }
}

/// <summary>Gửi push FCM HTTP v1 tới device đã đăng ký qua /api/mobile/device/push-token.</summary>
public class FcmPushService(
    AppDbContext db,
    IHttpClientFactory httpClientFactory,
    IOptions<FcmOptions> options,
    IWebHostEnvironment env,
    ILogger<FcmPushService> logger)
{
    private static readonly SemaphoreSlim CredentialLock = new(1, 1);
    private static GoogleCredential? CachedCredential;
    private static string? CachedCredentialPath;

    public async Task SendPhieuApproveAsync(
        IEnumerable<Guid> userIds,
        string loai,
        string phieuToken,
        string title,
        string body,
        CancellationToken ct = default)
    {
        var cfg = options.Value;
        if (!cfg.Enabled)
        {
            logger.LogDebug("FCM disabled — skip push.");
            return;
        }

        if (string.IsNullOrWhiteSpace(cfg.ProjectId) ||
            string.IsNullOrWhiteSpace(cfg.ServiceAccountPath))
        {
            logger.LogWarning(
                "FCM Enabled nhưng thiếu ProjectId hoặc ServiceAccountPath — skip push.");
            return;
        }

        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return;

        db.ChangeTracker.Clear();
        var tokens = await db.DevicePushTokens.AsNoTracking()
            .Where(x => ids.Contains(x.UserId) && x.Token != "")
            .Select(x => x.Token)
            .Distinct()
            .ToListAsync(ct);

        if (tokens.Count == 0)
        {
            logger.LogInformation("FCM: no device tokens for {Count} users.", ids.Count);
            return;
        }

        string accessToken;
        try
        {
            accessToken = await GetAccessTokenAsync(cfg, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FCM: không lấy được OAuth access token từ service account.");
            return;
        }

        var client = httpClientFactory.CreateClient(nameof(FcmPushService));
        var url = $"https://fcm.googleapis.com/v1/projects/{cfg.ProjectId.Trim()}/messages:send";

        var ok = 0;
        foreach (var deviceToken in tokens)
        {
            try
            {
                await SendOneAsync(
                    client, url, accessToken, deviceToken, loai, phieuToken, title, body, ct);
                ok++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "FCM send failed for token prefix {Prefix}",
                    deviceToken.Length > 12 ? deviceToken[..12] : deviceToken);
            }
        }

        logger.LogInformation(
            "FCM HTTP v1: sent {Ok}/{Total} for PHIEU_APPROVE loai={Loai} project={ProjectId}",
            ok, tokens.Count, loai, cfg.ProjectId);
    }

    private async Task<string> GetAccessTokenAsync(FcmOptions cfg, CancellationToken ct)
    {
        var path = ResolveServiceAccountPath(cfg.ServiceAccountPath!);
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Không tìm thấy Firebase service account JSON: {path}");

        await CredentialLock.WaitAsync(ct);
        try
        {
            if (CachedCredential == null ||
                !string.Equals(CachedCredentialPath, path, StringComparison.OrdinalIgnoreCase))
            {
                await using var stream = File.OpenRead(path);
                CachedCredential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                CachedCredentialPath = path;
                logger.LogInformation("FCM: loaded service account from {Path}", path);
            }

            return await CachedCredential.UnderlyingCredential
                .GetAccessTokenForRequestAsync(cancellationToken: ct);
        }
        finally
        {
            CredentialLock.Release();
        }
    }

    private string ResolveServiceAccountPath(string configured)
    {
        if (Path.IsPathRooted(configured))
            return configured;
        return Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
    }

    private static async Task SendOneAsync(
        HttpClient client,
        string url,
        string accessToken,
        string deviceToken,
        string loai,
        string phieuToken,
        string title,
        string body,
        CancellationToken ct)
    {
        var payload = new
        {
            message = new
            {
                token = deviceToken,
                notification = new { title, body },
                data = new Dictionary<string, string>
                {
                    ["type"] = "PHIEU_APPROVE",
                    ["loai"] = loai,
                    ["token"] = phieuToken,
                    ["title"] = title,
                    ["body"] = body
                },
                android = new
                {
                    priority = "HIGH",
                    notification = new
                    {
                        channel_id = "phieu_approve_channel",
                        sound = "default"
                    }
                },
                apns = new
                {
                    payload = new
                    {
                        aps = new
                        {
                            sound = "default",
                            contentAvailable = true
                        }
                    }
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        req.Content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var res = await client.SendAsync(req, ct);
        var text = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"FCM HTTP {(int)res.StatusCode}: {text}");
    }
}
