using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using NVOAMASIS.Data;
using NVOAMASIS.Models;

namespace NVOAMASIS.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;
        private readonly HttpClient _http_ip;
        private readonly IClientIpService _clientIpService;
        private readonly ISnackbar _Snackbar;
        private readonly IJSRuntime _js;
       
        public AttendanceService(AppDbContext db, IHttpContextAccessor http, HttpClient http_ip, IClientIpService clientIpService, ISnackbar snackbar, IJSRuntime js)
        {
            _db = db;
            _http = http;
            _http_ip = http_ip;
            _clientIpService = clientIpService;
            _Snackbar = snackbar;
            _js = js;
        }
        public async Task<string> GetClientIpAsync()
        {
            var result = await _http_ip.GetFromJsonAsync<IpResponse>("api/ip/get");
            return result?.Ip ?? "unknown";
        }
        public class IpResponse
        {
            public string Ip { get; set; }
        }
        private string GetClientIp()
        {
            var ctx = _http.HttpContext;
            if (ctx == null) return string.Empty;

            var candidates = new List<string>();

            // 1. Forwarded (RFC 7239)
            if (ctx.Request.Headers.TryGetValue("Forwarded", out var fwdValues))
            {
                var fwd = fwdValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(fwd))
                {
                    var segs = fwd.Split(';', ',');
                    foreach (var seg in segs)
                    {
                        var part = seg.Trim();
                        if (!part.StartsWith("for=", StringComparison.OrdinalIgnoreCase)) continue;
                        var val = part.Substring(4).Trim().Trim('"');
                        string ipPort = val;
                        if (val.StartsWith("["))
                        {
                            var closing = val.IndexOf(']');
                            if (closing > 0) ipPort = val.Substring(1, closing - 1);
                        }
                        else if (val.Contains(':'))
                        {
                            var lastColon = val.LastIndexOf(':');
                            if (lastColon > 0)
                            {
                                var maybeIp = val.Substring(0, lastColon);
                                if (System.Net.IPAddress.TryParse(maybeIp, out _)) ipPort = maybeIp;
                            }
                        }
                        var norm = NormalizeIp(ipPort);
                        if (IsPublicIp(norm)) candidates.Add(norm);
                    }
                }
            }

            // 2. X-Forwarded-For chain
            if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xffValues))
            {
                var raw = xffValues.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    foreach (var candidate in raw.Split(','))
                    {
                        var ip = NormalizeIp(candidate.Trim());
                        if (IsPublicIp(ip) && !candidates.Contains(ip)) candidates.Add(ip);
                    }
                }
            }

            // 3. Vendor specific single-IP headers
            string[] headerKeys = ["CF-Connecting-IP", "True-Client-IP", "X-Real-IP"];
            foreach (var key in headerKeys)
            {
                if (ctx.Request.Headers.TryGetValue(key, out var values))
                {
                    var raw = values.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        var ip = NormalizeIp(raw.Split(',')[0].Trim());
                        if (IsPublicIp(ip) && !candidates.Contains(ip)) candidates.Add(ip);
                    }
                }
            }

            // Prefer IPv4
            var ipv4 = candidates.FirstOrDefault(ip => System.Net.IPAddress.TryParse(ip, out var a) && a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (!string.IsNullOrEmpty(ipv4)) return ipv4;
            if (candidates.Count > 0) return candidates[0];

            // 4. Fallback remote
            var direct = NormalizeIp(ctx.Connection.RemoteIpAddress?.ToString());
            return direct;
        }

        private static bool IsPublicIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return false;
            if (!System.Net.IPAddress.TryParse(ip, out var addr)) return false;
            if (System.Net.IPAddress.IsLoopback(addr)) return false;
            // Filter private ranges for IPv4
            if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var bytes = addr.GetAddressBytes();
                // 10.0.0.0/8
                if (bytes[0] == 10) return false;
                // 172.16.0.0 - 172.31.255.255
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168) return false;
                // 169.254.0.0/16 (link-local)
                if (bytes[0] == 169 && bytes[1] == 254) return false;
            }
            // For IPv6 you could add unique local / link-local filters if desired
            return true;
        }

        private static string NormalizeIp(string? ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return string.Empty;
            ip = ip.Trim();
            // Handle IPv6-mapped IPv4 (e.g., ::ffff:115.77.188.173)
            if (ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase))
            {
                var v4 = ip.Substring("::ffff:".Length);
                if (System.Net.IPAddress.TryParse(v4, out _)) return v4;
            }
            // Validate basic IP
            if (System.Net.IPAddress.TryParse(ip, out var addr))
            {
                return addr.ToString(); // normalized
            }
            return string.Empty;
        }

        private List<string> GetWhitelistedIpCidrs()
        {
            // For simplicity: split CompanyInfomation.IPAddress by ; or ,
            var info = _db.CompanyInfomation.FirstOrDefault();
            var raw = info?.IPAddress ?? string.Empty;
            return raw.Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim())
                      .Where(s => s.Length > 0)
                      .ToList();
        }

        private static bool IpInCidrs(string ip, List<string> cidrs)
        {
            // Basic exact match or single IP list; CIDR parsing optional (future)
            if (string.IsNullOrWhiteSpace(ip)) return false;
            return cidrs.Any(c => string.Equals(c, ip, StringComparison.OrdinalIgnoreCase));
        }

        //public async Task<bool> HasCheckedInTodayAsync(Guid userId)
        //{
        //    var today = DateTime.Today;
        //    return await _db.AttendanceLogs.AnyAsync(a => a.UserId == userId && a.LocalDate == today);
        //}

        public async Task<bool> HasCheckedInTodayAsync(Guid userId, string session)
        {
            var today = DateTime.Today;
            return await _db.AttendanceLogs.AnyAsync(a =>
                a.UserId == userId &&
                a.LocalDate == today &&
                a.Session == session);
        }


        public async Task<(bool Success, string Message, AttendanceLog? Log)> CheckInAsync(Guid userId)
        {
            //var today = DateTime.Today;
            //if (await HasCheckedInTodayAsync(userId))
            //    return (false, "Đã chấm công hôm nay", null);
            var now = DateTime.Now;
            var today = DateTime.Today;

            var morningStart = today.AddHours(8);
            var morningEnd = today.AddHours(9);
            var afternoonStart = today.AddHours(13);
            var afternoonEnd = today.AddHours(14);

            string session = "";
            if (now >= morningStart && now <= morningEnd)
                session = "morning";
            else if (now >= afternoonStart && now <= afternoonEnd)
                session = "afternoon";

            if (string.IsNullOrEmpty(session))
                return (false, "Đã quá thời gian chấm công", null);

            if (await HasCheckedInTodayAsync(userId, session))
                return (false, "Đã chấm công hôm nay", null);

            //var ip = GetClientIp();
            //var ip = _clientIpService.GetClientIp(_http.HttpContext!);
            var ip= await _js.InvokeAsync<string>("getPublicIp");
            _Snackbar.Add($"IP Client : {ip}", MudBlazor.Severity.Info); 

            var cidrs = GetWhitelistedIpCidrs();
            var onsite = IpInCidrs(ip, cidrs);

            var log = new AttendanceLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CheckInTime = DateTime.UtcNow.AddHours(7),
                IPAddress = ip,
                IsOnsite = onsite,
                SourceType = "WEB",
                LocalDate = today,
                Session = session,
            };
            _db.AttendanceLogs.Add(log);
            await _db.SaveChangesAsync();
            return (true, onsite ? "Chấm công tại văn phòng" : "Chấm công ngoài văn phòng", log);
        }

        public async Task<int> GetTodayOnsiteCountAsync()
        {
            var today = DateTime.Today;
            return await _db.AttendanceLogs.CountAsync(a => a.LocalDate == today && a.IsOnsite);
        }

        /// <summary>
        /// Gets list of attendance logs, optionally filtered by date range, user, and session.
        /// </summary>
        public async Task<List<AttendanceLog>> GetAttendanceLogsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            Guid? userId = null,
            string? session = null)
        {
            var query = _db.AttendanceLogs.AsNoTracking().AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(x => x.LocalDate >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(x => x.LocalDate <= toDate.Value.Date);
            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(session))
                query = query.Where(x => x.Session == session);

            return await query
                .OrderByDescending(x => x.CheckInTime)
                .ToListAsync();
        }
    }
}
