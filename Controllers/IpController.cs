using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class IpController : ControllerBase
{
    private readonly IClientIpService _ipService;

    public IpController(IClientIpService ipService)
    {
        _ipService = ipService;
    }

    [HttpGet("get")]
    public IActionResult GetClientIp()
    {
        var ip = _ipService.GetClientIp(HttpContext);
        return Ok(new { ip });
    }
}
