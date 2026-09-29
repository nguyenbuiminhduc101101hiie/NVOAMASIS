using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Chấm công hằng ngày: chấm vào / ra (web, điện thoại), xem công theo ngày, HR chấm bù.
    /// Mọi lần chấm ghi vào AttendanceLogs kèm Session (sáng / chiều) → bảng công tháng và bảng lương dùng lại được.
    /// </summary>
    public sealed class HrAttendanceService(IDbContextFactory<AppDbContext> dbFactory, HrTimesheetService timesheet)
    {
        private const int MaxRangeDays = 62;

        // ───────────── Chấm công (web / mobile) ─────────────

        public async Task<HrAttendanceToday> GetTodayAsync(Guid userId)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var now = HrAttendanceCalc.VnNow();
            return await BuildTodayAsync(db, s, userId, now);
        }

        private static async Task<HrAttendanceToday> BuildTodayAsync(AppDbContext db, HrWorkSettings s, Guid userId, DateTime now)
        {
            var today = now.Date;
            var t = now.TimeOfDay;
            var day = (await HrSettingsService.BuildCalendarAsync(db, s, today, today))[0];
            var logs = await db.AttendanceLogs.AsNoTracking()
                .Where(a => a.UserId == userId && a.LocalDate == today)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();
            var punches = logs.Select(l => new HrAttendanceCalc.Punch(l.CheckInTime.TimeOfDay, l.PunchType)).ToList();
            var times = HrAttendanceCalc.Summarize(punches, day.Kind, s);
            var workday = day.Kind is HrDayKind.Work or HrDayKind.HalfWork;

            var r = new HrAttendanceToday
            {
                Mode = s.AttendanceMode,
                Date = today,
                DayKind = day.Kind,
                Punches = logs.Select(ToView).ToList(),
                FirstIn = times.FirstIn,
                LastOut = times.LastOut,
                LateMinutes = times.LateMinutes,
                WorkStart = s.WorkStart,
                WorkEnd = day.Kind == HrDayKind.HalfWork ? s.SaturdayEnd : s.WorkEnd,
                HasOfficeLocation = s.HasOfficeLocation,
                OfficeRadiusM = s.OfficeRadiusM,
                RequireOnsite = s.MobileRequireOnsite
            };

            if (s.AttendanceMode == HrAttendanceModes.InOut)
            {
                r.NextPunchType = HrAttendanceCalc.NextPunchType(punches);
                r.CanPunch = true;
                r.ButtonText = r.NextPunchType == HrPunchTypes.In ? "Chấm công vào" : "Chấm công ra";
                r.Remind = workday && r.NextPunchType == HrPunchTypes.In
                           && t >= s.WorkStart - TimeSpan.FromMinutes(30) && t < s.LunchStart;
                r.Hint = times.FirstIn.HasValue
                    ? $"Vào {times.FirstIn:hh\\:mm}" + (times.LastOut.HasValue ? $" · Ra {times.LastOut:hh\\:mm}" : "")
                    : "";
            }
            else
            {
                r.CurrentSession = HrAttendanceCalc.SessionAt(t, s);
                var done = r.CurrentSession != null && logs.Any(l => l.Session == r.CurrentSession);
                r.CanPunch = r.CurrentSession != null && !done;
                r.Remind = r.CanPunch;
                r.ButtonText = r.CanPunch ? "Chấm công" : "Đã chấm";
                r.Hint = $"Sáng {s.WorkStart:hh\\:mm}–{s.WorkStart + HrAttendanceCalc.SessionWindow:hh\\:mm}, " +
                         $"chiều {s.LunchEnd:hh\\:mm}–{s.LunchEnd + HrAttendanceCalc.SessionWindow:hh\\:mm}";
            }
            return r;
        }

        public async Task<HrPunchResult> PunchAsync(Guid userId, HrPunchInput input)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var now = HrAttendanceCalc.VnNow();
            var today = now.Date;
            var t = now.TimeOfDay;

            var existing = await db.AttendanceLogs.AsNoTracking()
                .Where(a => a.UserId == userId && a.LocalDate == today)
                .Select(a => new { a.CheckInTime, a.PunchType, a.Session, a.IsManual })
                .ToListAsync();

            // Có mặt tại văn phòng: IP thuộc danh sách IP văn phòng, hoặc GPS trong bán kính.
            var whitelist = await db.CompanyInfomation.AsNoTracking().Select(c => c.IPAddress).FirstOrDefaultAsync();
            var ip = (input.Ip ?? string.Empty).Trim();
            var onsite = HrAttendanceCalc.IpAllowed(ip, whitelist);
            int? distance = null;
            if (input.Latitude.HasValue && input.Longitude.HasValue && s.HasOfficeLocation)
            {
                distance = HrAttendanceCalc.DistanceM(input.Latitude.Value, input.Longitude.Value,
                    s.OfficeLatitude!.Value, s.OfficeLongitude!.Value);
                if (distance <= s.OfficeRadiusM) onsite = true;
            }

            string session;
            string? type = null;
            if (s.AttendanceMode == HrAttendanceModes.InOut)
            {
                type = input.PunchType?.Trim().ToLowerInvariant() switch
                {
                    HrPunchTypes.In => HrPunchTypes.In,
                    HrPunchTypes.Out => HrPunchTypes.Out,
                    _ => HrAttendanceCalc.NextPunchType(existing.Select(e =>
                        new HrAttendanceCalc.Punch(e.CheckInTime.TimeOfDay, e.PunchType)))
                };
                var firstIn = existing.Where(e => e.PunchType != HrPunchTypes.Out)
                    .Select(e => (DateTime?)e.CheckInTime).Min();
                if (type == HrPunchTypes.In && firstIn.HasValue)
                    return new HrPunchResult(false, $"Bạn đã chấm công vào lúc {firstIn:HH:mm}.");
                if (existing.Any(e => !e.IsManual && (now - e.CheckInTime).Duration() < HrAttendanceCalc.MinGap))
                    return new HrPunchResult(false, "Bạn vừa chấm công, vui lòng đợi 1 phút.");
                session = HrAttendanceCalc.SessionFor(type, t, s);
            }
            else
            {
                var at = HrAttendanceCalc.SessionAt(t, s);
                if (at is null) return new HrPunchResult(false, "Đã quá thời gian chấm công");
                if (existing.Any(e => e.Session == at)) return new HrPunchResult(false, "Đã chấm công hôm nay");
                session = at;
            }

            if (input.Source == HrAttendanceSources.Mobile && s.MobileRequireOnsite && !onsite)
            {
                var why = distance.HasValue
                    ? $"Bạn đang cách văn phòng khoảng {distance} m (cho phép {s.OfficeRadiusM} m)."
                    : "Không xác định được vị trí. Hãy bật GPS hoặc kết nối WiFi văn phòng.";
                return new HrPunchResult(false, $"Chỉ chấm công được tại văn phòng. {why}", type, null, false, distance);
            }

            var log = new AttendanceLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CheckInTime = now,
                IPAddress = ip.Length > 64 ? ip[..64] : ip,
                IsOnsite = onsite,
                SourceType = input.Source == HrAttendanceSources.Mobile ? HrAttendanceSources.Mobile : HrAttendanceSources.Web,
                LocalDate = today,
                Session = session,
                PunchType = type,
                Latitude = input.Latitude.HasValue ? Math.Round(input.Latitude.Value, 6) : null,
                Longitude = input.Longitude.HasValue ? Math.Round(input.Longitude.Value, 6) : null,
                DistanceM = distance
            };
            db.AttendanceLogs.Add(log);
            await db.SaveChangesAsync();

            var what = type switch
            {
                HrPunchTypes.In => $"Chấm công vào lúc {now:HH:mm}",
                HrPunchTypes.Out => $"Chấm công ra lúc {now:HH:mm}",
                _ => "Chấm công"
            };
            var where = onsite ? "tại văn phòng" : "ngoài văn phòng";
            if (type == HrPunchTypes.In)
            {
                var day = (await HrSettingsService.BuildCalendarAsync(db, s, today, today))[0];
                var late = HrAttendanceCalc.Summarize(new[] { new HrAttendanceCalc.Punch(t, type) }, day.Kind, s).LateMinutes;
                if (late > 0) where += $" · đi muộn {late} phút";
            }
            return new HrPunchResult(true, $"{what} {where}", type, now, onsite, distance);
        }

        // ───────────── Xem công theo ngày ─────────────

        /// <summary>Công từng ngày của từng nhân viên trong [From, To] (tối đa 62 ngày).</summary>
        public async Task<List<HrAttendanceDayRow>> GetDaysAsync(HrAttendanceFilter f)
        {
            var from = f.From.Date;
            var to = f.To.Date < from ? from : f.To.Date;
            if ((to - from).TotalDays > MaxRangeDays) to = from.AddDays(MaxRangeDays);

            var sheet = await timesheet.BuildRangeAsync(from, to, new HrTimesheetFilter
            {
                UserId = f.UserId, DepartmentId = f.DepartmentId, Branch = f.Branch, Search = f.Search
            });

            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var today = HrAttendanceCalc.VnNow().Date;
            var end = to.AddDays(1);

            var userIds = sheet.Rows.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).ToHashSet();
            var logs = (await db.AttendanceLogs.AsNoTracking()
                    .Where(a => a.LocalDate >= from && a.LocalDate < end)
                    .Select(a => new { a.UserId, a.LocalDate, a.CheckInTime, a.PunchType, a.SourceType, a.IsOnsite, a.IsManual })
                    .ToListAsync())
                .Where(a => userIds.Contains(a.UserId))
                .ToLookup(a => (a.UserId, a.LocalDate.Date));

            var list = new List<HrAttendanceDayRow>();
            foreach (var r in sheet.Rows)
            {
                for (var i = 0; i < sheet.Days.Count; i++)
                {
                    var day = sheet.Days[i];
                    var cell = r.Cells[i];
                    var dayLogs = r.UserId.HasValue ? logs[(r.UserId.Value, day.Date)].ToList() : new();
                    var punches = dayLogs.Select(l => new HrAttendanceCalc.Punch(l.CheckInTime.TimeOfDay, l.PunchType)).ToList();
                    var mLeave = HrAttendanceCalc.IsLeaveCode(cell.MorningCode);
                    var aLeave = HrAttendanceCalc.IsLeaveCode(cell.AfternoonCode);
                    var times = HrAttendanceCalc.Summarize(punches, day.Kind, s, mLeave, aLeave);
                    var employed = cell.Code != "-";
                    list.Add(new HrAttendanceDayRow
                    {
                        Date = day.Date,
                        EmployeeId = r.EmployeeId,
                        EmployeeCode = r.EmployeeCode,
                        FullName = r.FullName,
                        DepartmentName = r.DepartmentName,
                        Branch = r.Branch,
                        UserId = r.UserId,
                        DayKind = day.Kind,
                        Code = cell.Code,
                        FirstIn = times.FirstIn,
                        LastOut = times.LastOut,
                        LateMinutes = times.LateMinutes,
                        EarlyMinutes = times.EarlyMinutes,
                        PunchCount = dayLogs.Count,
                        ManualCount = dayLogs.Count(l => l.IsManual),
                        Sources = string.Join(", ", dayLogs.Select(l => l.SourceType).Distinct().OrderBy(x => x)),
                        AnyRemote = dayLogs.Any(l => !l.IsOnsite),
                        Status = HrAttendanceCalc.StatusOf(day.Kind, employed, r.HasAccount, cell.MorningCode,
                            cell.AfternoonCode, dayLogs.Count, times, s.AttendanceMode, day.Date, today)
                    });
                }
            }
            return list;
        }

        public async Task<List<HrPunchView>> GetPunchesAsync(Guid userId, DateTime date)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var d = date.Date;
            var logs = await db.AttendanceLogs.AsNoTracking()
                .Where(a => a.UserId == userId && a.LocalDate == d)
                .OrderBy(a => a.CheckInTime)
                .ToListAsync();
            return logs.Select(ToView).ToList();
        }

        private static HrPunchView ToView(AttendanceLog l) => new(l.Id, l.CheckInTime, l.PunchType, l.Session,
            l.SourceType, l.IsOnsite, l.IPAddress, l.DistanceM, l.IsManual, l.Note, l.CreatedBy);

        // ───────────── HR chấm bù / sửa công ─────────────

        public async Task<HrResult> AddManualAsync(HrManualPunchInput input, string actor)
        {
            var note = (input.Note ?? "").Trim();
            if (note.Length == 0) return HrResult.Fail("hr_att_err_note_required");
            if (input.Date is null || input.Time is null) return HrResult.Fail("hr_att_err_time_required");
            var type = input.PunchType == HrPunchTypes.Out ? HrPunchTypes.Out : HrPunchTypes.In;

            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var err = await ValidateManualAsync(db, input.UserId, input.Date.Value.Date);
            if (err != null) return HrResult.Fail(err);

            var at = input.Date.Value.Date + input.Time.Value;
            if (at > HrAttendanceCalc.VnNow()) return HrResult.Fail("hr_att_err_future");
            db.AttendanceLogs.Add(NewManual(input.UserId, at, type, s, input.IsOnsite, note, actor));
            await db.SaveChangesAsync();
            return HrResult.Ok("hr_saved");
        }

        /// <summary>Chấm bù nhanh: sáng (vào giờ bắt đầu), chiều (vào giờ làm chiều), cả ngày (vào + ra).</summary>
        public async Task<HrResult> AddManualPresetAsync(Guid userId, DateTime date, HrManualPreset preset, string? note,
            string actor)
        {
            note = (note ?? "").Trim();
            if (note.Length == 0) return HrResult.Fail("hr_att_err_note_required");
            date = date.Date;

            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);
            var err = await ValidateManualAsync(db, userId, date);
            if (err != null) return HrResult.Fail(err);
            if (date > HrAttendanceCalc.VnNow().Date) return HrResult.Fail("hr_att_err_future");

            var day = (await HrSettingsService.BuildCalendarAsync(db, s, date, date))[0];
            var halfDay = day.Kind == HrDayKind.HalfWork;
            if (halfDay && preset == HrManualPreset.Afternoon) return HrResult.Fail("hr_err_leave_saturday_afternoon");

            switch (preset)
            {
                case HrManualPreset.Morning:
                    db.AttendanceLogs.Add(NewManual(userId, date + s.WorkStart, HrPunchTypes.In, s, true, note, actor));
                    break;
                case HrManualPreset.Afternoon:
                    db.AttendanceLogs.Add(NewManual(userId, date + s.LunchEnd, HrPunchTypes.In, s, true, note, actor));
                    break;
                default:
                    db.AttendanceLogs.Add(NewManual(userId, date + s.WorkStart, HrPunchTypes.In, s, true, note, actor));
                    db.AttendanceLogs.Add(NewManual(userId, date + (halfDay ? s.SaturdayEnd : s.WorkEnd),
                        HrPunchTypes.Out, s, true, note, actor));
                    break;
            }
            await db.SaveChangesAsync();
            return HrResult.Ok("hr_saved");
        }

        /// <summary>Xóa lần chấm bù (chỉ xóa được bản ghi do HR nhập; lần chấm thật của nhân viên giữ nguyên để đối chiếu).</summary>
        public async Task<HrResult> DeleteManualAsync(Guid id)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var log = await db.AttendanceLogs.FirstOrDefaultAsync(a => a.Id == id);
            if (log is null) return HrResult.Fail("hr_err_not_found");
            if (!log.IsManual) return HrResult.Fail("hr_att_err_only_manual");
            var err = await ValidateManualAsync(db, log.UserId, log.LocalDate.Date);
            if (err != null) return HrResult.Fail(err);
            db.AttendanceLogs.Remove(log);
            await db.SaveChangesAsync();
            return HrResult.Ok("hr_deleted");
        }

        private static AttendanceLog NewManual(Guid userId, DateTime at, string type, HrWorkSettings s, bool onsite,
            string note, string actor) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CheckInTime = at,
            LocalDate = at.Date,
            Session = HrAttendanceCalc.SessionFor(type, at.TimeOfDay, s),
            PunchType = type,
            IsOnsite = onsite,
            IPAddress = string.Empty,
            SourceType = HrAttendanceSources.Manual,
            IsManual = true,
            Note = note.Length > 500 ? note[..500] : note,
            CreatedBy = actor.Length > 100 ? actor[..100] : actor
        };

        /// <summary>Không cho sửa công của tháng đã chốt lương.</summary>
        private static async Task<string?> ValidateManualAsync(AppDbContext db, Guid userId, DateTime date)
        {
            if (userId == Guid.Empty) return "hr_err_not_found";
            try
            {
                var locked = await db.HrPayrollPeriods.AsNoTracking()
                    .AnyAsync(p => p.Year == date.Year && p.Month == date.Month && p.Status != HrPayrollStatus.Draft);
                if (locked) return "hr_att_err_period_locked";
            }
            catch
            {
                // Chưa có bảng lương → bỏ qua.
            }
            return null;
        }

        // ───────────── Xuất Excel ─────────────

        public static byte[] ExportExcel(IReadOnlyList<HrAttendanceDayRow> rows, DateTime from, DateTime to, Func<string, string> L)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("ChamCong");
            ws.Cell(1, 1).Value = $"{L("hr_attendance")} {from:dd/MM/yyyy}" + (to.Date != from.Date ? $" – {to:dd/MM/yyyy}" : "");
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            string[] headers =
            {
                L("hr_date"), L("hr_employee_code"), L("hr_full_name"), L("hr_department"), L("hr_att_code"),
                L("hr_att_first_in"), L("hr_att_last_out"), L("hr_att_late"), L("hr_att_early"), L("hr_att_status"),
                L("hr_att_source")
            };
            const int hr = 3;
            for (var c = 0; c < headers.Length; c++) ws.Cell(hr, c + 1).Value = headers[c];
            var r = hr + 1;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.Date;
                ws.Cell(r, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(r, 2).Value = x.EmployeeCode;
                ws.Cell(r, 3).Value = x.FullName;
                ws.Cell(r, 4).Value = x.DepartmentName ?? "";
                ws.Cell(r, 5).Value = x.Code;
                ws.Cell(r, 6).Value = x.FirstIn?.ToString(@"hh\:mm") ?? "";
                ws.Cell(r, 7).Value = x.LastOut?.ToString(@"hh\:mm") ?? "";
                if (x.LateMinutes > 0) ws.Cell(r, 8).Value = x.LateMinutes;
                if (x.EarlyMinutes > 0) ws.Cell(r, 9).Value = x.EarlyMinutes;
                ws.Cell(r, 10).Value = L(StatusKey(x.Status));
                ws.Cell(r, 11).Value = x.Sources;
                if (x.Warn) ws.Range(r, 5, r, 10).Style.Font.FontColor = XLColor.Red;
                r++;
            }
            var header = ws.Range(hr, 1, hr, headers.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            var table = ws.Range(hr, 1, Math.Max(hr, r - 1), headers.Length);
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.SheetView.FreezeRows(hr);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        public static string StatusKey(HrAttendanceStatus s) => s switch
        {
            HrAttendanceStatus.Present => "hr_att_st_present",
            HrAttendanceStatus.Late => "hr_att_st_late",
            HrAttendanceStatus.MissingOut => "hr_att_st_missing_out",
            HrAttendanceStatus.NotYet => "hr_att_st_not_yet",
            HrAttendanceStatus.Leave => "hr_att_st_leave",
            HrAttendanceStatus.Absent => "hr_att_st_absent",
            HrAttendanceStatus.Off => "hr_att_st_off",
            HrAttendanceStatus.Holiday => "hr_att_st_holiday",
            HrAttendanceStatus.NotEmployed => "hr_att_st_not_employed",
            HrAttendanceStatus.NoAccount => "hr_att_st_no_account",
            _ => "hr_unknown"
        };
    }
}
