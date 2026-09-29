using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVOAMASIS.Models.Hr;
using NVOAMASIS.Services;
using NVOAMASIS.Services.Hr;

namespace NVOAMASIS.Controllers;

/// <summary>
/// Chấm công trên điện thoại. Cùng quy tắc với nút chấm công trên web (HrAttendanceService):
///  - Chế độ theo buổi / giờ vào – giờ ra, khung giờ: cấu hình tại 12.8.
///  - Tại văn phòng khi: IP public của điện thoại (qua WiFi văn phòng) nằm trong danh sách IP công ty,
///    HOẶC tọa độ GPS gửi lên nằm trong bán kính văn phòng.
///  - Bật "Điện thoại chỉ chấm được tại văn phòng" → ngoài văn phòng bị từ chối; tắt → vẫn ghi nhận, đánh dấu "từ xa".
/// </summary>
[ApiController]
[Route("api/mobile/attendance")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken]
public class MobileAttendanceController(HrAttendanceService attendance) : ControllerBase
{
    /// <summary>Tình trạng hôm nay: các lần đã chấm, lần tiếp theo là vào hay ra, giờ làm, có dùng GPS không.</summary>
    [HttpGet("today")]
    public async Task<ActionResult<HrAttendanceToday>> Today()
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        return Ok(await attendance.GetTodayAsync(userId.Value));
    }

    public record PunchRequest(string? PunchType, decimal? Latitude, decimal? Longitude, int? AccuracyM);

    /// <summary>Chấm công. PunchType: "in" / "out" / null (tự chọn). Gửi kèm tọa độ GPS nếu có.</summary>
    [HttpPost("punch")]
    public async Task<ActionResult<object>> Punch([FromBody] PunchRequest req)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        if (req.Latitude is < -90 or > 90 || req.Longitude is < -180 or > 180 || req.Latitude.HasValue != req.Longitude.HasValue)
            return BadRequest(new { flag = false, message = "Tọa độ GPS không hợp lệ." });

        var rs = await attendance.PunchAsync(userId.Value, new HrPunchInput
        {
            Source = HrAttendanceSources.Mobile,
            PunchType = req.PunchType,
            Ip = AttendanceService.ExtractClientIp(HttpContext),
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            AccuracyM = req.AccuracyM
        });
        var body = new
        {
            flag = rs.Success,
            message = rs.Message,
            punchType = rs.PunchType,
            time = rs.Time,
            isOnsite = rs.IsOnsite,
            distanceM = rs.DistanceM
        };
        return rs.Success ? Ok(body) : UnprocessableEntity(body);
    }

    /// <summary>Công từng ngày trong tháng của chính mình.</summary>
    [HttpGet("history")]
    public async Task<ActionResult<object>> History([FromQuery] int? year, [FromQuery] int? month)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();
        var today = HrAttendanceCalc.VnNow();
        var y = year is >= 2000 and <= 2100 ? year.Value : today.Year;
        var m = month is >= 1 and <= 12 ? month.Value : today.Month;
        var from = new DateTime(y, m, 1);
        var rows = await attendance.GetDaysAsync(new HrAttendanceFilter
        {
            From = from, To = from.AddMonths(1).AddDays(-1), UserId = userId
        });
        return Ok(new
        {
            flag = true,
            year = y,
            month = m,
            lateCount = rows.Count(r => r.LateMinutes > 0),
            lateMinutes = rows.Sum(r => r.LateMinutes),
            days = rows.Select(r => new
            {
                date = r.Date.ToString("yyyy-MM-dd"),
                code = r.Code,
                firstIn = r.FirstIn?.ToString(@"hh\:mm"),
                lastOut = r.LastOut?.ToString(@"hh\:mm"),
                lateMinutes = r.LateMinutes,
                earlyMinutes = r.EarlyMinutes,
                status = r.Status.ToString(),
                dayKind = r.DayKind.ToString()
            })
        });
    }
}
