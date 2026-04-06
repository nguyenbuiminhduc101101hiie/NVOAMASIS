using Microsoft.AspNetCore.Http;

public interface IClientIpService
{
    string GetClientIp(HttpContext context);
}

public class ClientIpService : IClientIpService
{
    public string GetClientIp(HttpContext context)
    {
        // Trường hợp có reverse proxy (Nginx, IIS, Cloudflare...)
        //var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        //if (!string.IsNullOrEmpty(ip))
        //{
        //    return ip.Split(',')[0]; // lấy IP đầu tiên
        //}

        //// Nếu không có proxy thì lấy IP trực tiếp
        //return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null && remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            remoteIp = System.Net.Dns.GetHostEntry(remoteIp).AddressList
                        .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
        return remoteIp?.ToString() ?? "unknown";

    }
}
