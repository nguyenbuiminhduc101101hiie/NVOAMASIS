using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>Cấu hình nhân sự (HrSetting), ngày lễ (HrHoliday) và lịch làm việc.</summary>
    public sealed class HrSettingsService(IDbContextFactory<AppDbContext> dbFactory)
    {
        // ───────────── Cấu hình ─────────────

        public async Task<HrWorkSettings> GetAsync()
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            return await GetAsync(db);
        }

        internal static async Task<HrWorkSettings> GetAsync(AppDbContext db)
        {
            var s = new HrWorkSettings();
            Dictionary<string, string?> map;
            try
            {
                map = await db.HrSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
            }
            catch
            {
                // Bảng chưa tạo → dùng mặc định.
                return s;
            }

            if (map.TryGetValue(HrSettingKeys.WorkDays, out var wd) && !string.IsNullOrWhiteSpace(wd))
            {
                var days = wd.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => int.TryParse(x, out var n) ? n : -1)
                    .Where(n => n is >= 0 and <= 6)
                    .Select(n => (DayOfWeek)n)
                    .ToHashSet();
                if (days.Count > 0) s.WorkDays = days;
            }
            s.SaturdayHalfDay = Bool(map, HrSettingKeys.SaturdayHalfDay, s.SaturdayHalfDay);
            s.HoursPerDay = Dec(map, HrSettingKeys.HoursPerDay, s.HoursPerDay);
            if (s.HoursPerDay <= 0) s.HoursPerDay = 8;
            s.AnnualLeaveBaseDays = Dec(map, HrSettingKeys.AnnualLeaveBaseDays, s.AnnualLeaveBaseDays);
            s.SeniorityStepYears = (int)Dec(map, HrSettingKeys.SeniorityStepYears, s.SeniorityStepYears);
            s.MaxCarryOverDays = Dec(map, HrSettingKeys.MaxCarryOverDays, s.MaxCarryOverDays);
            s.AllowNegativeAnnualLeave = Bool(map, HrSettingKeys.AllowNegativeAnnualLeave, s.AllowNegativeAnnualLeave);
            if (map.TryGetValue(HrSettingKeys.AttendanceIpSource, out var ip) && ip is "client" or "server")
                s.AttendanceIpSource = ip;
            return s;
        }

        private static bool Bool(Dictionary<string, string?> m, string k, bool def) =>
            m.TryGetValue(k, out var v) && bool.TryParse(v, out var b) ? b : def;

        private static decimal Dec(Dictionary<string, string?> m, string k, decimal def) =>
            m.TryGetValue(k, out var v) && decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : def;

        public async Task<HrResult> SaveAsync(HrWorkSettings s, string actor)
        {
            if (s.WorkDays.Count == 0) return HrResult.Fail("hr_err_workdays_required");
            if (s.HoursPerDay is <= 0 or > 24) return HrResult.Fail("hr_err_hours_per_day");
            if (s.AnnualLeaveBaseDays < 0 || s.MaxCarryOverDays < 0 || s.SeniorityStepYears < 0)
                return HrResult.Fail("hr_err_negative_value");

            var values = new Dictionary<string, string>
            {
                [HrSettingKeys.WorkDays] = string.Join(",", s.WorkDays.Select(d => (int)d).OrderBy(x => x)),
                [HrSettingKeys.SaturdayHalfDay] = s.SaturdayHalfDay.ToString().ToLowerInvariant(),
                [HrSettingKeys.HoursPerDay] = s.HoursPerDay.ToString(CultureInfo.InvariantCulture),
                [HrSettingKeys.AnnualLeaveBaseDays] = s.AnnualLeaveBaseDays.ToString(CultureInfo.InvariantCulture),
                [HrSettingKeys.SeniorityStepYears] = s.SeniorityStepYears.ToString(CultureInfo.InvariantCulture),
                [HrSettingKeys.MaxCarryOverDays] = s.MaxCarryOverDays.ToString(CultureInfo.InvariantCulture),
                [HrSettingKeys.AllowNegativeAnnualLeave] = s.AllowNegativeAnnualLeave.ToString().ToLowerInvariant(),
                [HrSettingKeys.AttendanceIpSource] = s.AttendanceIpSource == "server" ? "server" : "client"
            };

            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var existing = await db.HrSettings.ToDictionaryAsync(x => x.Key);
                var now = DateTime.Now;
                foreach (var (key, value) in values)
                {
                    if (existing.TryGetValue(key, out var row))
                    {
                        row.Value = value;
                        row.UpdatedAt = now;
                        row.UpdatedBy = actor;
                    }
                    else
                    {
                        db.HrSettings.Add(new HrSetting { Key = key, Value = value, UpdatedAt = now, UpdatedBy = actor });
                    }
                }
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ───────────── Ngày lễ ─────────────

        public async Task<List<HrHoliday>> GetHolidaysAsync(int year)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var from = new DateTime(year, 1, 1);
            var to = from.AddYears(1);
            return await db.HrHolidays.AsNoTracking()
                .Where(x => x.Date >= from && x.Date < to)
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<HrResult> SaveHolidayAsync(Guid id, DateTime date, string name, string actor)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return HrResult.Fail("hr_err_name_required");
            date = date.Date;
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                if (await db.HrHolidays.AnyAsync(x => x.Date == date && x.Id != id))
                    return HrResult.Fail("hr_err_holiday_exists");

                if (id == Guid.Empty)
                {
                    db.HrHolidays.Add(new HrHoliday
                    {
                        Id = Guid.NewGuid(), Date = date, Name = name, CreatedAt = DateTime.Now, CreatedBy = actor
                    });
                }
                else
                {
                    var h = await db.HrHolidays.FirstOrDefaultAsync(x => x.Id == id);
                    if (h is null) return HrResult.Fail("hr_err_not_found");
                    h.Date = date;
                    h.Name = name;
                }
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_saved");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        public async Task<HrResult> DeleteHolidayAsync(Guid id)
        {
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var h = await db.HrHolidays.FirstOrDefaultAsync(x => x.Id == id);
                if (h is null) return HrResult.Fail("hr_err_not_found");
                db.HrHolidays.Remove(h);
                await db.SaveChangesAsync();
                return HrResult.Ok("hr_deleted");
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_delete", ex.GetBaseException().Message);
            }
        }

        /// <summary>
        /// Thêm các ngày lễ dương lịch cố định (1/1, 30/4, 1/5, 2/9) cho năm được chọn.
        /// Tết Âm lịch, Giỗ Tổ và ngày nghỉ bù thay đổi theo thông báo hằng năm → nhập tay.
        /// </summary>
        public async Task<HrResult> AddFixedHolidaysAsync(int year, string actor)
        {
            var fixedDays = new (int M, int D, string Name)[]
            {
                (1, 1, "Tết Dương lịch"),
                (4, 30, "Ngày Giải phóng miền Nam"),
                (5, 1, "Quốc tế Lao động"),
                (9, 2, "Quốc khánh")
            };
            try
            {
                await using var db = await dbFactory.CreateDbContextAsync();
                var dates = fixedDays.Select(f => new DateTime(year, f.M, f.D)).ToList();
                var existing = await db.HrHolidays.Where(x => dates.Contains(x.Date)).Select(x => x.Date).ToListAsync();
                var added = 0;
                foreach (var f in fixedDays)
                {
                    var d = new DateTime(year, f.M, f.D);
                    if (existing.Contains(d)) continue;
                    db.HrHolidays.Add(new HrHoliday
                    {
                        Id = Guid.NewGuid(), Date = d, Name = f.Name, CreatedAt = DateTime.Now, CreatedBy = actor
                    });
                    added++;
                }
                await db.SaveChangesAsync();
                return new HrResult(true, "hr_holidays_added", null, added.ToString());
            }
            catch (Exception ex)
            {
                return HrResult.Fail("hr_err_save", ex.GetBaseException().Message);
            }
        }

        // ───────────── Lịch làm việc ─────────────

        /// <summary>Lịch từng ngày trong [from, to] (bao gồm cả 2 đầu).</summary>
        internal static async Task<List<HrTimesheetDay>> BuildCalendarAsync(AppDbContext db, HrWorkSettings s,
            DateTime from, DateTime to)
        {
            from = from.Date;
            to = to.Date;
            var end = to.AddDays(1);
            Dictionary<DateTime, string> holidays;
            try
            {
                holidays = (await db.HrHolidays.AsNoTracking()
                        .Where(x => x.Date >= from && x.Date < end)
                        .Select(x => new { x.Date, x.Name })
                        .ToListAsync())
                    .GroupBy(x => x.Date.Date)
                    .ToDictionary(g => g.Key, g => g.First().Name);
            }
            catch
            {
                holidays = new();
            }

            var list = new List<HrTimesheetDay>();
            for (var d = from; d <= to; d = d.AddDays(1))
                list.Add(Classify(d, s, holidays));
            return list;
        }

        internal static HrTimesheetDay Classify(DateTime d, HrWorkSettings s, IReadOnlyDictionary<DateTime, string> holidays)
        {
            var day = new HrTimesheetDay { Date = d.Date };
            if (!s.WorkDays.Contains(d.DayOfWeek))
                day.Kind = HrDayKind.Off;
            else if (holidays.TryGetValue(d.Date, out var name))
            {
                day.Kind = HrDayKind.Holiday;
                day.HolidayName = name;
            }
            else if (d.DayOfWeek == DayOfWeek.Saturday && s.SaturdayHalfDay)
                day.Kind = HrDayKind.HalfWork;
            else
                day.Kind = HrDayKind.Work;
            return day;
        }
    }
}
