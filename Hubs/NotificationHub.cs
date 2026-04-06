using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NVOAMASIS.Hubs
{
    public class NotificationHub : Hub
    {
        // Có thể mở rộng thêm method nếu cần

        public async Task TestBroadcast(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
        public async Task NotifyUnreadEmailChanged(string userId, int newCount)
        {
            await Clients.User(userId).SendAsync("UpdateUnreadEmailCount", newCount);
        }
    }
}