using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using NVOAMASIS.Models;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers
{
    /// <summary>
    /// API for Arrival Notice: serve Arrival Notice PDF by token when user scans QR code.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ArrivalNoticeController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ShipmentService _shipmentService;
        private readonly IDistributedCache _cache;
        private readonly ArrivalNoticeQrService _qrService;

        public ArrivalNoticeController(ShipmentService shipmentService, IDistributedCache cache, ArrivalNoticeQrService qrService)
        {
            _shipmentService = shipmentService;
            _cache = cache;
            _qrService = qrService;
        }

        [HttpGet("{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArrivalNoticePdf(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Invalid token.");

            var cacheKey = ArrivalNoticeQrCacheKeys.Prefix + token;
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

            if (payload == null)
            {
                var (payloadFromDb, expiresAt) = await _qrService.GetPayloadAndExpiryByTokenAsync(token);
                if (payloadFromDb == null || !expiresAt.HasValue)
                    return NotFound("Link has expired or is invalid. Please request a new QR code.");
                payload = payloadFromDb;
                await _qrService.RepopulateCacheAsync(token, payload, expiresAt.Value);
            }

            var hbl = await _shipmentService.GetHBL_byHBLid(payload.HblId);
            if (hbl == null)
                return NotFound("HBL not found.");

            byte[]? pdfBytes;
            string fileName = $"AN_{hbl.hbl ?? "document"}.pdf";

            if (payload.Type == "SI" || payload.Type == "SE")
                pdfBytes = await _shipmentService.GetArrivalNoticePdfBytesAsync(hbl);
            else if (payload.Type == "AI" || payload.Type == "AE")
                pdfBytes = await _shipmentService.GetArrivalNoticeAirPdfBytesAsync(hbl);
            else
                return BadRequest("Invalid type.");

            if (pdfBytes == null || pdfBytes.Length == 0)
                return StatusCode(500, "Failed to generate Arrival Notice document.");

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}

