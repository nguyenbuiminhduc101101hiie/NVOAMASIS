using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using QRCoder;

namespace NVOAMASIS.Services
{
    /// <summary>
    /// QR D/O theo (HblId, loại). Single company — logo/connection inject lúc render PDF.
    /// </summary>
    public class DeliveryOrderQrService
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _db;

        public DeliveryOrderQrService(
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext db)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public async Task<DoQrResult?> GetExistingQrAsync(Guid hblId, string type)
        {
            var now = DateTimeOffset.UtcNow;
            var record = await _db.DoQrTokens
                .Where(x => x.HblId == hblId && x.Type == type && x.ExpiresAt > now)
                .OrderByDescending(x => x.ExpiresAt)
                .FirstOrDefaultAsync();

            if (record == null)
                return null;

            var url = BuildDoUrl(record.Token);
            var qrImageBase64 = GenerateQrCodeBase64(url);
            var hoursLeft = (int)(record.ExpiresAt - now).TotalHours;
            if (hoursLeft < 1) hoursLeft = 1;

            return new DoQrResult
            {
                Token = record.Token,
                Url = url,
                QrImageBase64 = qrImageBase64,
                ExpiresAt = record.ExpiresAt,
                ExpiresInHours = hoursLeft,
                IsExisting = true
            };
        }

        public async Task<DoQrResult> CreateQrAsync(Guid hblId, string type, DateTimeOffset expiresAt)
        {
            var now = DateTimeOffset.UtcNow;
            if (expiresAt <= now)
                expiresAt = now.AddHours(24);

            var token = Guid.NewGuid().ToString("N");

            var existing = await _db.DoQrTokens
                .Where(x => x.HblId == hblId && x.Type == type)
                .ToListAsync();
            if (existing.Count > 0)
                _db.DoQrTokens.RemoveRange(existing);

            _db.DoQrTokens.Add(new DoQrTokenRecord
            {
                Token = token,
                HblId = hblId,
                Type = type,
                BillType = "",
                Branches = "",
                ExpiresAt = expiresAt
            });
            await _db.SaveChangesAsync();

            var cacheKey = DoQrCacheKeys.Prefix + token;
            var payload = new DoTokenPayload { HblId = hblId, Type = type };
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiresAt
            };
            await _cache.SetAsync(cacheKey, bytes, options);

            var url = BuildDoUrl(token);
            var qrImageBase64 = GenerateQrCodeBase64(url);
            var expiresInHours = (int)(expiresAt - now).TotalHours;
            if (expiresInHours < 1) expiresInHours = 1;

            return new DoQrResult
            {
                Token = token,
                Url = url,
                QrImageBase64 = qrImageBase64,
                ExpiresAt = expiresAt,
                ExpiresInHours = expiresInHours,
                IsExisting = false
            };
        }

        public async Task<(DoTokenPayload? payload, DateTimeOffset? expiresAt)> GetPayloadAndExpiryByTokenAsync(string token)
        {
            var record = await _db.DoQrTokens
                .Where(x => x.Token == token && x.ExpiresAt > DateTimeOffset.UtcNow)
                .FirstOrDefaultAsync();

            if (record == null)
                return (null, null);

            var payload = new DoTokenPayload
            {
                HblId = record.HblId,
                Type = record.Type
            };
            return (payload, record.ExpiresAt);
        }

        public async Task RepopulateCacheAsync(string token, DoTokenPayload payload, DateTimeOffset expiresAt)
        {
            var cacheKey = DoQrCacheKeys.Prefix + token;
            var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            var options = new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAt };
            await _cache.SetAsync(cacheKey, bytes, options);
        }

        private string BuildDoUrl(string token)
        {
            var baseUrl = GetBaseUrl();
            return $"{baseUrl}/api/deliveryorder/{token}";
        }

        private string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
                return "https://localhost:5001";
            return $"{request.Scheme}://{request.Host.Value}";
        }

        private static string GenerateQrCodeBase64(string url, int pixelsPerModule = 8)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            byte[] pngBytes = qrCode.GetGraphic(pixelsPerModule);
            return Convert.ToBase64String(pngBytes);
        }
    }

    public class DoQrResult
    {
        public string Token { get; set; } = "";
        public string Url { get; set; } = "";
        public string QrImageBase64 { get; set; } = "";
        public int ExpiresInHours { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsExisting { get; set; }
    }
}
