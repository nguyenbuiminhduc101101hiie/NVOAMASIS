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
            ArrivalNoticeTokenPayload? payload = null;

            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    payload = JsonSerializer.Deserialize<ArrivalNoticeTokenPayload>(bytes, JsonOptions);
                }
                catch
                {
                    try
                    {
                        var legacy = JsonSerializer.Deserialize<DoTokenPayload>(bytes, JsonOptions);
                        if (legacy != null)
                        {
                            payload = new ArrivalNoticeTokenPayload
                            {
                                HblId = legacy.HblId,
                                Type = legacy.Type ?? "",
                                BillType = "PASL",
                                Branches = ""
                            };
                        }
                    }
                    catch { /* use DB */ }
                }

                if (payload != null)
                {
                    if (string.IsNullOrWhiteSpace(payload.BillType))
                        payload.BillType = "PASL";
                    payload.Branches ??= "";
                }
            }

            if (payload == null || payload.HblId == default)
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

            var billType = string.IsNullOrWhiteSpace(payload.BillType) ? "PASL" : payload.BillType.Trim();
            var branches = payload.Branches?.Trim() ?? "";

            byte[]? pdfBytes;
            string fileName = $"AN_{hbl.hbl ?? "document"}.pdf";

            if (payload.Type == "SI" || payload.Type == "SE")
                pdfBytes = await _shipmentService.GetArrivalNoticePdfBytesAsync(hbl, billType, branches);
            else if (payload.Type == "AI" || payload.Type == "AE")
                pdfBytes = await _shipmentService.GetArrivalNoticeAirPdfBytesAsync(hbl, billType, branches);
            else
                return BadRequest("Invalid type.");

            if (pdfBytes == null || pdfBytes.Length == 0)
                return StatusCode(500, "Failed to generate Arrival Notice document.");

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
