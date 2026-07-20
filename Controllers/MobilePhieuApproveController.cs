using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/mobile/phieu-approve")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken]
public class MobilePhieuApproveController(PhieuApproveService approveService) : ControllerBase
{
    public record DecideRequest(bool Approve, string? Remarks);

    [HttpGet("{token}")]
    public async Task<ActionResult<object>> GetDetail(string token)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token không hợp lệ." });

        var detail = await approveService.GetInAppDetailAsync(token.Trim(), userId.Value);
        if (detail == null)
            return NotFound(new { message = "Không tìm thấy yêu cầu duyệt." });

        return Ok(new
        {
            loai = detail.Loai,
            phieuId = detail.PhieuId,
            token = detail.Token,
            soPhieu = detail.SoPhieu,
            detailHtml = detail.DetailHtml,
            approve = detail.Approve,
            approveBy = detail.ApproveBy,
            approveDate = detail.ApproveDate,
            remarks = detail.Remarks,
            alreadyDecided = detail.AlreadyDecided,
            canDecide = detail.CanDecide
        });
    }

    [HttpPost("{token}/decide")]
    public async Task<ActionResult<object>> Decide(string token, [FromBody] DecideRequest req)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token không hợp lệ." });

        var rs = await approveService.DecideInAppAsync(token.Trim(), userId.Value, req.Approve, req.Remarks);
        if (!rs.Flag)
            return BadRequest(new { flag = false, message = rs.Message });

        return Ok(new { flag = true, message = rs.Message });
    }
}
