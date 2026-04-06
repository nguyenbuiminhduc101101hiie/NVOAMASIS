namespace NVOAMASIS.Hubs
{
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Security.Claims;
    using NVOAMASIS.Services;

    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.Sid)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // fallback: try get from query string
                var httpContext = connection.GetHttpContext();
                userId = httpContext?.Request.Query["userid"].ToString();
            }

            return userId;
        }
    }

}
