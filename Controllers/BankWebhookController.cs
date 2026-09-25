using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NVOAMASIS.Models;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

/// <summary>
/// Webhook nhận biến động số dư từ SePay.
/// URL cấu hình trên SePay: https://{domain}/api/webhook/bank/sepay (xác thực kiểu API Key).
/// </summary>
[ApiController]
[Route("api/webhook/bank")]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class BankWebhookController(
    BankPaymentService paymentService,
    IOptions<SePaySettings> options,
    ILogger<BankWebhookController> logger) : ControllerBase
{
    [HttpPost("sepay")]
    public async Task<IActionResult> SePay()
    {
        var apiKey = options.Value.WebhookApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) || !IsAuthorized(apiKey))
            return Unauthorized(new { success = false });

        string raw;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            raw = await reader.ReadToEndAsync();

        SePayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SePayWebhookPayload>(raw);
        }
        catch (JsonException)
        {
            return BadRequest(new { success = false });
        }

        if (payload == null || payload.Id == 0)
            return BadRequest(new { success = false });

        try
        {
            await paymentService.ProcessSePayAsync(payload, raw);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SePay webhook failed for txn {TxnId}", payload.Id);
            return StatusCode(500, new { success = false });
        }
    }

    private bool IsAuthorized(string apiKey)
    {
        var header = Request.Headers.Authorization.ToString();
        var expected = Encoding.UTF8.GetBytes($"Apikey {apiKey}");
        var actual = Encoding.UTF8.GetBytes(header);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
