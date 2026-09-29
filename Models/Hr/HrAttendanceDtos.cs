namespace NVOAMASIS.Models.Hr
{
    // ───────────── Chấm công hằng ngày ─────────────

    public sealed class HrAttendanceFilter
    {
        public DateTime From { get; set; } = DateTime.Today;
        public DateTime To { get; set; } = DateTime.Today;
        public Guid? DepartmentId { get; set; }
        public string? Branch { get; set; }
        public string? Search { get; set; }
        /// <summary>Chỉ 1 người (UserList.UsrId) — dùng cho "Công của tôi" / mobile.</summary>
        public Guid? UserId { get; set; }
    }

    /// <summary>Trạng thái 1 ngày của 1 nhân viên.</summary>
    public enum HrAttendanceStatus
    {
        Present,
        Late,
        /// <summary>Có giờ vào nhưng quên chấm giờ ra (chế độ giờ vào – giờ ra, ngày đã qua).</summary>
        MissingOut,
        /// <summary>Hôm nay / tương lai, chưa chấm.</summary>
        NotYet,
        Leave,
        Absent,
        Off,
        Holiday,
        NotEmployed,
        NoAccount
    }

    public sealed class HrAttendanceDayRow
    {
        public DateTime Date { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? Branch { get; set; }
        public Guid? UserId { get; set; }
        public HrDayKind DayKind { get; set; }
        /// <summary>Ký hiệu như bảng công (X, TX, P, V, X/P ...).</summary>
        public string Code { get; set; } = string.Empty;
        public TimeSpan? FirstIn { get; set; }
        public TimeSpan? LastOut { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyMinutes { get; set; }
        public int PunchCount { get; set; }
        public int ManualCount { get; set; }
        /// <summary>WEB / MOBILE / MANUAL (các nguồn đã chấm trong ngày).</summary>
        public string Sources { get; set; } = string.Empty;
        public bool AnyRemote { get; set; }
        public HrAttendanceStatus Status { get; set; }
        public bool Warn => Status is HrAttendanceStatus.Absent or HrAttendanceStatus.MissingOut or HrAttendanceStatus.Late
                            || EarlyMinutes > 0;
    }

    public sealed class HrAttendanceDaySummary
    {
        public int Total { get; set; }
        public int Present { get; set; }
        public int Late { get; set; }
        public int NotYet { get; set; }
        public int Leave { get; set; }
        public int Absent { get; set; }
        public int MissingOut { get; set; }
        public int Remote { get; set; }

        public static HrAttendanceDaySummary From(IEnumerable<HrAttendanceDayRow> rows)
        {
            var s = new HrAttendanceDaySummary();
            foreach (var r in rows)
            {
                if (r.Status is HrAttendanceStatus.Off or HrAttendanceStatus.Holiday or HrAttendanceStatus.NotEmployed) continue;
                s.Total++;
                switch (r.Status)
                {
                    case HrAttendanceStatus.Present: s.Present++; break;
                    case HrAttendanceStatus.Late: s.Present++; s.Late++; break;
                    case HrAttendanceStatus.MissingOut: s.Present++; s.MissingOut++; break;
                    case HrAttendanceStatus.NotYet: s.NotYet++; break;
                    case HrAttendanceStatus.Leave: s.Leave++; break;
                    case HrAttendanceStatus.Absent: s.Absent++; break;
                }
                if (r.AnyRemote) s.Remote++;
            }
            return s;
        }
    }

    /// <summary>1 lần chấm công (để hiển thị).</summary>
    public sealed record HrPunchView(Guid Id, DateTime Time, string? PunchType, string Session, string Source,
        bool IsOnsite, string Ip, int? DistanceM, bool IsManual, string? Note, string? CreatedBy);

    /// <summary>Yêu cầu chấm công từ web / điện thoại.</summary>
    public sealed class HrPunchInput
    {
        /// <summary>"in" / "out"; null = tự chọn (chưa có giờ vào → vào, ngược lại → ra).</summary>
        public string? PunchType { get; set; }
        public string Source { get; set; } = HrAttendanceSources.Web;
        public string? Ip { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        /// <summary>Độ chính xác GPS (m) do điện thoại báo.</summary>
        public int? AccuracyM { get; set; }
    }

    public sealed record HrPunchResult(bool Success, string Message, string? PunchType = null, DateTime? Time = null,
        bool IsOnsite = false, int? DistanceM = null);

    /// <summary>Tình trạng chấm công hôm nay của 1 người (nút chấm công web / màn hình mobile).</summary>
    public sealed class HrAttendanceToday
    {
        public string Mode { get; set; } = HrAttendanceModes.Session;
        public DateTime Date { get; set; }
        public HrDayKind DayKind { get; set; }
        public List<HrPunchView> Punches { get; set; } = new();
        /// <summary>Có bấm được lúc này không.</summary>
        public bool CanPunch { get; set; }
        /// <summary>"in" / "out" ở chế độ giờ vào – giờ ra; null ở chế độ theo buổi.</summary>
        public string? NextPunchType { get; set; }
        /// <summary>Buổi đang trong khung chấm (chế độ theo buổi).</summary>
        public string? CurrentSession { get; set; }
        /// <summary>Nên nhắc chấm công ngay.</summary>
        public bool Remind { get; set; }
        public TimeSpan? FirstIn { get; set; }
        public TimeSpan? LastOut { get; set; }
        public int LateMinutes { get; set; }
        public string ButtonText { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
        // Cấu hình để app mobile hiển thị
        public TimeSpan WorkStart { get; set; }
        public TimeSpan WorkEnd { get; set; }
        public bool HasOfficeLocation { get; set; }
        public int OfficeRadiusM { get; set; }
        public bool RequireOnsite { get; set; }
    }

    /// <summary>HR chấm bù.</summary>
    public sealed class HrManualPunchInput
    {
        public Guid UserId { get; set; }
        public DateTime? Date { get; set; } = DateTime.Today;
        public TimeSpan? Time { get; set; }
        public string PunchType { get; set; } = HrPunchTypes.In;
        public bool IsOnsite { get; set; } = true;
        public string? Note { get; set; }
    }

    /// <summary>Chấm bù nhanh.</summary>
    public enum HrManualPreset { Morning, Afternoon, FullDay }
}
