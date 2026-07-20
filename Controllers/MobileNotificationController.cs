using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Hubs;
using NVOAMASIS.Models;
using NVOAMASIS.Services;

namespace NVOAMASIS.Controllers;

[ApiController]
[Route("api/mobile/notifications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken]
public class MobileNotificationController(
    AppDbContext db,
    IHubContext<NotificationHub> hub) : ControllerBase
{
    public record NotificationDto(
        Guid Id,
        Guid? SenderUserId,
        Guid? ReceiverUserId,
        string Message,
        DateTime CreatedAt,
        bool IsRead,
        bool IsPhieuApprove,
        string? PhieuLoai,
        string? PhieuToken);

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] bool unreadOnly = false, [FromQuery] int take = 50)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();

        take = Math.Clamp(take, 1, 200);
        db.ChangeTracker.Clear();
        var q = db.Notifications.AsNoTracking().Where(x => x.ReceiverUserId == userId);
        if (unreadOnly)
            q = q.Where(x => !x.IsRead);

        var items = await q.OrderByDescending(x => x.CreatedAt).Take(take).ToListAsync();
        var unreadCount = await db.Notifications.CountAsync(x => x.ReceiverUserId == userId && !x.IsRead);

        var dtos = items.Select(Map).ToList();
        return Ok(new { unreadCount, items = dtos });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<object>> MarkRead(Guid id)
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();

        db.ChangeTracker.Clear();
        var note = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.ReceiverUserId == userId);
        if (note == null) return NotFound(new { message = "Không tìm thấy notification." });

        note.IsRead = true;
        await db.SaveChangesAsync();

        var unreadCount = await db.Notifications.CountAsync(x => x.ReceiverUserId == userId && !x.IsRead);
        await hub.Clients.User(userId.Value.ToString()).SendAsync("UpdateUnreadEmailCount", unreadCount);

        return Ok(new { flag = true, unreadCount });
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<object>> MarkAllRead()
    {
        var userId = MobileJwtTokenService.GetUserId(User);
        if (userId == null) return Unauthorized();

        db.ChangeTracker.Clear();
        var list = await db.Notifications
            .Where(x => x.ReceiverUserId == userId && !x.IsRead)
            .ToListAsync();

        foreach (var n in list)
            n.IsRead = true;
        await db.SaveChangesAsync();

        await hub.Clients.User(userId.Value.ToString()).SendAsync("UpdateUnreadEmailCount", 0);
        return Ok(new { flag = true, unreadCount = 0, updated = list.Count });
    }

    private static NotificationDto Map(Notification n)
    {
        var isPhieu = PhieuApproveService.TryParsePhieuApproveNotification(n.Message, out var loai, out var token);
        return new NotificationDto(
            n.Id,
            n.SenderUserId,
            n.ReceiverUserId,
            n.Message,
            n.CreatedAt,
            n.IsRead,
            isPhieu,
            isPhieu ? loai : null,
            isPhieu ? token : null);
    }
}
