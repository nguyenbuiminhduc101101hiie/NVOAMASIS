namespace NVOAMASIS.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using System.Security.Claims;

    public class CustomUserIdProvider : IUserIdProvider
    {
        // Chỉ tin claim đã xác thực; không fallback sang query string (?userid=) vì ai cũng giả mạo được.
        public string? GetUserId(HubConnectionContext connection) =>
            connection.User?.FindFirst(ClaimTypes.Sid)?.Value
            ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
