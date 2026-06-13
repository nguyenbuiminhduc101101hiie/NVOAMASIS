using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NVOAMASIS.Models;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers
{
    /// <summary>
    /// API for Delivery Order: serve DO PDF by token when user scans QR code.
    /// Uses distributed cache (SQL Server) so tokens survive app restart.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryOrderController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ShipmentService _shipmentService;
        private readonly IDistributedCache _cache;
        private readonly DeliveryOrderQrService _doQrService;

        public DeliveryOrderController(ShipmentService shipmentService, IDistributedCache cache, DeliveryOrderQrService doQrService)
        {
            _shipmentService = shipmentService;
            _cache = cache;
            _doQrService = doQrService;
        }

        /// <summary>
        /// Public endpoint: return DO PDF for the given token (used when user scans QR).
        /// Fallback to DB if cache miss (e.g. after app restart).
        /// </summary>
        [HttpGet("{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoPdf(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid token.");

            var cacheKey = DoQrCacheKeys.Prefix + token;
            var bytes = await _cache.GetAsync(cacheKey);
            DoTokenPayload? payload = null;

            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    payload = JsonSerializer.Deserialize<DoTokenPayload>(bytes, JsonOptions);
                }
                catch { /* fallback to DB */ }
            }

            if (payload == null || payload.HblId == default)
            {
                var (payloadFromDb, expiresAt) = await _doQrService.GetPayloadAndExpiryByTokenAsync(token);
                if (payloadFromDb == null || !expiresAt.HasValue)
                    return NotFound("Link has expired or is invalid. Please request a new QR code.");
                payload = payloadFromDb;
                await _doQrService.RepopulateCacheAsync(token, payload, expiresAt.Value);
            }

            var hbl = await _shipmentService.GetHBL_byHBLid(payload.HblId);
            if (hbl == null)
                return NotFound("HBL not found.");

            byte[]? pdfBytes;
            var fileName = $"DO_{hbl.hbl ?? "document"}.pdf";

            if (payload.Type == "AI" || payload.Type == "AE")
                pdfBytes = await _shipmentService.GetDOAirPdfBytesAsync(hbl);
            else
                pdfBytes = await _shipmentService.GetDOPdfBytesAsync(hbl);

            if (pdfBytes == null || pdfBytes.Length == 0)
                return StatusCode(500, "Failed to generate DO document.");

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
