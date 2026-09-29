using System.Globalization;
using System.Net;
using System.Net.Sockets;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>Quy tắc chấm công hằng ngày (hàm thuần, không truy cập DB — kiểm thử được).</summary>
    public static class HrAttendanceCalc
    {
        public const string Morning = "morning";
        public const string Afternoon = "afternoon";
        /// <summary>Khung chấm mỗi buổi ở chế độ theo buổi (như cũ: 8:00–9:00, 13:00–14:00).</summary>
        public static readonly TimeSpan SessionWindow = TimeSpan.FromHours(1);
        /// <summary>Chống bấm 2 lần liên tiếp.</summary>
        public static readonly TimeSpan MinGap = TimeSpan.FromMinutes(1);

        public readonly record struct Punch(TimeSpan Time, string? Type);

        public sealed record Times(TimeSpan? FirstIn, TimeSpan? LastOut, int LateMinutes, int EarlyMinutes);

        /// <summary>Giờ Việt Nam hiện tại (giống CheckInTime cũ: UTC+7).</summary>
        public static DateTime VnNow() => DateTime.UtcNow.AddHours(7);

        /// <summary>Chế độ theo buổi: buổi đang trong khung chấm, null nếu ngoài khung.</summary>
        public static string? SessionAt(TimeSpan t, HrWorkSettings s)
        {
            if (t >= s.WorkStart && t <= s.WorkStart + SessionWindow) return Morning;
            if (t >= s.LunchEnd && t <= s.LunchEnd + SessionWindow) return Afternoon;
            return null;
        }

        /// <summary>
        /// Chế độ giờ vào – giờ ra: buổi mà lần chấm xác nhận có mặt.
        /// Vào trước giờ nghỉ trưa → sáng (còn lại → chiều); ra trước giờ làm chiều → sáng (còn lại → chiều).
        /// Vd vào 8:00 + ra 17:00 → cả 2 buổi; vào 8:00 + ra 11:30 → chỉ buổi sáng; vào 12:45 → buổi chiều.
        /// </summary>
        public static string SessionFor(string? punchType, TimeSpan t, HrWorkSettings s) =>
            punchType == HrPunchTypes.Out
                ? (t < s.LunchEnd ? Morning : Afternoon)
                : (t < s.LunchStart ? Morning : Afternoon);

        /// <summary>Lần chấm tiếp theo: chưa có giờ vào → "in", ngược lại → "out".</summary>
        public static string NextPunchType(IEnumerable<Punch> today) =>
            today.Any(p => p.Type != HrPunchTypes.Out) ? HrPunchTypes.Out : HrPunchTypes.In;

        /// <summary>Giờ vào đầu tiên, giờ ra cuối cùng, số phút đi muộn / về sớm.</summary>
        /// <param name="morningExcused">Buổi sáng nghỉ phép → giờ phải vào là giờ làm chiều.</param>
        /// <param name="afternoonExcused">Buổi chiều nghỉ phép → giờ được ra là giờ nghỉ trưa.</param>
        public static Times Summarize(IReadOnlyCollection<Punch> punches, HrDayKind kind, HrWorkSettings s,
            bool morningExcused = false, bool afternoonExcused = false)
        {
            TimeSpan? firstIn = punches.Where(p => p.Type != HrPunchTypes.Out).Select(p => (TimeSpan?)p.Time).Min();
            TimeSpan? lastOut = punches.Where(p => p.Type == HrPunchTypes.Out).Select(p => (TimeSpan?)p.Time).Max();
            if (kind is not (HrDayKind.Work or HrDayKind.HalfWork)) return new Times(firstIn, lastOut, 0, 0);

            var halfDay = kind == HrDayKind.HalfWork;
            if (halfDay) afternoonExcused = true; // T7 chỉ làm sáng
            if (morningExcused && afternoonExcused) return new Times(firstIn, lastOut, 0, 0);

            var grace = TimeSpan.FromMinutes(Math.Max(0, s.LateGraceMinutes));
            var late = 0;
            if (firstIn.HasValue)
            {
                var expectedIn = morningExcused ? s.LunchEnd : s.WorkStart;
                var d = firstIn.Value - expectedIn;
                if (d > grace) late = (int)Math.Floor(d.TotalMinutes);
            }

            var early = 0;
            if (lastOut.HasValue)
            {
                var expectedOut = halfDay ? s.SaturdayEnd : afternoonExcused ? s.LunchStart : s.WorkEnd;
                var d = expectedOut - lastOut.Value;
                if (d > grace) early = (int)Math.Floor(d.TotalMinutes);
            }
            return new Times(firstIn, lastOut, late, early);
        }

        /// <summary>Khoảng cách 2 tọa độ (m) — công thức haversine.</summary>
        public static int DistanceM(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            const double r = 6371000;
            double Rad(decimal x) => (double)x * Math.PI / 180;
            var dLat = Rad(lat2 - lat1);
            var dLng = Rad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return (int)Math.Round(r * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
        }

        /// <summary>
        /// IP có thuộc danh sách IP văn phòng (CompanyInfomation.IPAddress, phân cách ; , |) không.
        /// Hỗ trợ IP đơn và dải IPv4 dạng CIDR (vd 115.77.188.0/24).
        /// </summary>
        public static bool IpAllowed(string? ip, string? whitelist)
        {
            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(whitelist)) return false;
            if (!IPAddress.TryParse(ip.Trim(), out var addr)) return false;
            foreach (var raw in whitelist.Split(new[] { ';', ',', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (string.Equals(raw, ip.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
                var slash = raw.IndexOf('/');
                if (slash <= 0) continue;
                if (!IPAddress.TryParse(raw[..slash], out var net) || net.AddressFamily != AddressFamily.InterNetwork
                    || addr.AddressFamily != AddressFamily.InterNetwork) continue;
                if (!int.TryParse(raw[(slash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var bits)
                    || bits is < 0 or > 32) continue;
                var mask = bits == 0 ? 0u : uint.MaxValue << (32 - bits);
                if ((ToUInt(addr) & mask) == (ToUInt(net) & mask)) return true;
            }
            return false;
        }

        private static uint ToUInt(IPAddress a)
        {
            var b = a.GetAddressBytes();
            return (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
        }

        private static readonly HashSet<string> LeaveCodes = new()
        {
            HrLeaveType.Code(HrLeaveType.Annual), HrLeaveType.Code(HrLeaveType.Sick),
            HrLeaveType.Code(HrLeaveType.Personal), HrLeaveType.Code(HrLeaveType.Unpaid)
        };

        public static bool IsLeaveCode(string? code) => code != null && LeaveCodes.Contains(code);

        /// <summary>Trạng thái trong ngày.</summary>
        public static HrAttendanceStatus StatusOf(HrDayKind kind, bool employed, bool hasAccount, string? morningCode,
            string? afternoonCode, int punchCount, Times times, string mode, DateTime date, DateTime today)
        {
            if (!employed) return HrAttendanceStatus.NotEmployed;
            if (kind == HrDayKind.Off) return punchCount > 0 ? HrAttendanceStatus.Present : HrAttendanceStatus.Off;
            if (kind == HrDayKind.Holiday) return HrAttendanceStatus.Holiday;
            if (!hasAccount) return HrAttendanceStatus.NoAccount;

            var halfDay = kind == HrDayKind.HalfWork;
            var mLeave = IsLeaveCode(morningCode);
            var aLeave = halfDay || IsLeaveCode(afternoonCode);
            if (punchCount == 0)
            {
                if (mLeave && aLeave) return HrAttendanceStatus.Leave;
                return date.Date >= today.Date ? HrAttendanceStatus.NotYet : HrAttendanceStatus.Absent;
            }
            if (mode == HrAttendanceModes.InOut && times.FirstIn.HasValue && !times.LastOut.HasValue && date.Date < today.Date)
                return HrAttendanceStatus.MissingOut;
            return times.LateMinutes > 0 ? HrAttendanceStatus.Late : HrAttendanceStatus.Present;
        }
    }
}
