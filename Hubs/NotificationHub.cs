using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NVOAMASIS.Hubs
{
    /// <summary>
    /// Server đẩy thông báo qua IHubContext&lt;NotificationHub&gt;.Clients.User(...).
    /// Client (web: HubConnection phía server trong MainLayout; mobile) phải kết nối bằng JWT —
    /// không có method nào cho client gọi, để client không thể broadcast hoặc sửa badge của người khác.
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationHub : Hub
    {
    }
}
