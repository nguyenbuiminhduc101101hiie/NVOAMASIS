using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using NVOAMASIS.Data;
using NVOAMASIS.Models.Hr;

namespace NVOAMASIS.Services.Hr
{
    /// <summary>
    /// Bảng công tháng (ký hiệu: X làm tại VP, TX làm từ xa, P phép năm, Ô ốm, R việc riêng, KL không lương,
    /// L lễ, V vắng không phép), tổng hợp từ:
    ///  - AttendanceLogs (chấm công buổi sáng / chiều, onsite / remote),
    ///  - LeaveRequests đã duyệt (phép năm, ốm, việc riêng, không lương, xin ra ngoài),
    ///  - OutRequests đã duyệt (xin ra ngoài kiểu cũ),
    ///  - lịch làm việc + ngày lễ (HrSetting / HrHoliday).
    /// Mỗi ngày chia 2 buổi, mỗi buổi 0,5 công.
    /// </summary>
    public sealed class HrTimesheetService(IDbContextFactory<AppDbContext> dbFactory)
    {
        private static readonly TimeSpan Noon = new(12, 0, 0);

        private enum Seg { None, Worked, Remote, Leave, Holiday, Absent, Future, NotEmployed }

        private readonly record struct SegState(Seg Kind, int LeaveType = 0);

        public async Task<HrTimesheet> BuildAsync(HrTimesheetFilter f)
        {
            await using var db = await dbFactory.CreateDbContextAsync();
            var s = await HrSettingsService.GetAsync(db);

            var from = new DateTime(f.Year, f.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);
            var today = DateTime.Today;

            var sheet = new HrTimesheet { Year = f.Year, Month = f.Month };
            sheet.Days = await HrSettingsService.BuildCalendarAsync(db, s, from, to);

            // Nhân viên có làm việc trong tháng
            var empQ = db.HrEmployees.AsNoTracking()
                .Where(e => (e.JoinDate == null || e.JoinDate <= to)
                            && (e.Status != HrEmployeeStatus.Resigned || e.ResignDate == null || e.ResignDate >= from));
            if (f.DepartmentId.HasValue) empQ = empQ.Where(e => e.DepartmentId == f.DepartmentId);
            if (!string.IsNullOrWhiteSpace(f.Branch)) empQ = empQ.Where(e => e.Branch == f.Branch);
            if (!string.IsNullOrWhiteSpace(f.Search))
            {
                var t = f.Search.Trim();
                empQ = empQ.Where(e => e.EmployeeCode.Contains(t) || e.FullName.Contains(t));
            }
            var emps = await (
                from e in empQ
                join d in db.Department.AsNoTracking() on e.DepartmentId equals d.DepartmentId into dj
                from d in dj.DefaultIfEmpty()
                orderby e.EmployeeCode
                select new
                {
                    e.Id, e.EmployeeCode, e.FullName, e.UserId, e.Branch, e.JoinDate, e.ResignDate, e.Status,
                    DeptName = d != null ? d.Name : null
                }).ToListAsync();

            var userIds = emps.Where(x => x.UserId.HasValue).Select(x => x.UserId!.Value).ToHashSet();
            var end = to.AddDays(1);

            // Chấm công
            var logs = await db.AttendanceLogs.AsNoTracking()
                .Where(a => a.LocalDate >= from && a.LocalDate < end)
                .Select(a => new { a.UserId, a.LocalDate, a.Session, a.IsOnsite })
                .ToListAsync();
            var attendance = logs.Where(a => userIds.Contains(a.UserId))
                .GroupBy(a => (a.UserId, a.LocalDate.Date))
                .ToDictionary(g => g.Key, g => (
                    Morning: g.Where(x => x.Session == "morning").Select(x => (bool?)x.IsOnsite).Max(),
                    Afternoon: g.Where(x => x.Session == "afternoon").Select(x => (bool?)x.IsOnsite).Max()));

            // Đơn nghỉ đã duyệt
            var leaves = (await db.LeaveRequests.AsNoTracking()
                    .Where(l => l.Status == HrLeaveStatus.Approved && l.StartDate < end && l.EndDate >= from)
                    .Select(l => new { l.EmployeeId, l.LeaveType, l.DurationType, l.StartDate, l.EndDate, l.StartTime, l.EndTime })
                    .ToListAsync())
                .Where(l => userIds.Contains(l.EmployeeId))
                .ToList();

            // Xin ra ngoài kiểu cũ (OutRequests)
            var outs = (await db.OutRequests.AsNoTracking()
                    .Where(o => o.Status == "Approved" && o.FromTime < end && o.FromTime >= from)
                    .Select(o => new { o.UserId, o.FromTime })
                    .ToListAsync())
                .Where(o => userIds.Contains(o.UserId))
                .ToList();

            foreach (var e in emps)
            {
                var uid = e.UserId ?? Guid.Empty;
                var info = new EmpInfo(e.Id, e.EmployeeCode, e.FullName, e.DeptName, e.Branch, e.UserId,
                    e.JoinDate, e.Status == HrEmployeeStatus.Resigned ? e.ResignDate : null);
                var att = attendance.Where(kv => kv.Key.UserId == uid)
                    .ToDictionary(kv => kv.Key.Item2, kv => (kv.Value.Morning, kv.Value.Afternoon));
                var myLeaves = leaves.Where(l => l.EmployeeId == uid)
                    .Select(l => new LeaveInfo(l.LeaveType, l.DurationType, l.StartDate, l.EndDate, l.StartTime, l.EndTime))
                    .ToList();
                sheet.Rows.Add(ComputeRow(info, sheet.Days, att, myLeaves, outs.Count(o => o.UserId == uid), today));
            }

            return sheet;
        }

        public sealed record EmpInfo(Guid Id, string Code, string Name, string? Dept, string? Branch, Guid? UserId,
            DateTime? JoinDate, DateTime? ResignDate);

        public sealed record LeaveInfo(int LeaveType, int DurationType, DateTime StartDate, DateTime EndDate,
            TimeSpan? StartTime, TimeSpan? EndTime);

        /// <summary>Tính 1 dòng bảng công (hàm thuần, không truy cập DB — để kiểm thử được).</summary>
        /// <param name="attendance">Ngày → (buổi sáng, buổi chiều): null = không chấm, true = tại VP, false = từ xa.</param>
        public static HrTimesheetRow ComputeRow(EmpInfo e, IReadOnlyList<HrTimesheetDay> days,
            IReadOnlyDictionary<DateTime, (bool? Morning, bool? Afternoon)> attendance,
            IReadOnlyList<LeaveInfo> leaves, int outRequestCount, DateTime today)
        {
            var row = new HrTimesheetRow
            {
                EmployeeId = e.Id,
                EmployeeCode = e.Code,
                FullName = e.Name,
                DepartmentName = e.Dept,
                Branch = e.Branch,
                HasAccount = e.UserId.HasValue,
                Cells = new HrTimesheetCell[days.Count]
            };

            for (var i = 0; i < days.Count; i++)
            {
                var day = days[i];
                var date = day.Date;
                var employed = (e.JoinDate is null || e.JoinDate.Value.Date <= date)
                               && !(e.ResignDate.HasValue && e.ResignDate.Value.Date < date);

                if (!employed)
                {
                    row.Cells[i] = new HrTimesheetCell { Code = "-" };
                    continue;
                }
                if (day.Kind == HrDayKind.Off)
                {
                    row.Cells[i] = new HrTimesheetCell { Code = "" };
                    continue;
                }
                if (day.Kind == HrDayKind.Holiday)
                {
                    row.Holiday += 1;
                    row.Standard += 1;
                    row.Cells[i] = new HrTimesheetCell { Code = "L", Tip = day.HolidayName };
                    continue;
                }

                row.Standard += day.Standard;
                if (!e.UserId.HasValue)
                {
                    row.Cells[i] = new HrTimesheetCell { Code = "?", Tip = "no account" };
                    continue;
                }

                attendance.TryGetValue(date, out var att);
                var dayLeaves = leaves.Where(l => l.StartDate.Date <= date && l.EndDate.Date >= date).ToList();
                row.GoOutCount += dayLeaves.Count(l => l.LeaveType == HrLeaveType.GoOut);

                SegState Resolve(bool morning)
                {
                    var logged = morning ? att.Morning : att.Afternoon;
                    if (logged.HasValue) return new SegState(logged.Value ? Seg.Worked : Seg.Remote);

                    foreach (var l in dayLeaves)
                    {
                        if (l.LeaveType == HrLeaveType.GoOut) continue;
                        var covers = l.DurationType switch
                        {
                            HrLeaveDuration.FullDay => true,
                            HrLeaveDuration.HalfDay => l.StartTime is null
                                ? morning // nửa ngày kiểu cũ không rõ buổi → tính buổi sáng
                                : (l.StartTime.Value < Noon) == morning,
                            // Nghỉ theo giờ: chỉ tính khi nghỉ từ 2 giờ trở lên trong buổi đó.
                            _ => l.StartTime.HasValue && l.EndTime.HasValue &&
                                 HoursIn(l.StartTime.Value, l.EndTime.Value, morning) >= 2
                        };
                        if (covers) return new SegState(Seg.Leave, l.LeaveType);
                    }
                    return date >= today.Date ? new SegState(Seg.Future) : new SegState(Seg.Absent);
                }

                var m = Resolve(true);
                var a = day.Kind == HrDayKind.HalfWork ? new SegState(Seg.None) : Resolve(false);

                foreach (var seg in new[] { m, a })
                {
                    switch (seg.Kind)
                    {
                        case Seg.Worked: row.Worked += 0.5m; break;
                        case Seg.Remote: row.Worked += 0.5m; row.Remote += 0.5m; break;
                        case Seg.Absent: row.Absent += 0.5m; break;
                        case Seg.Leave:
                            switch (seg.LeaveType)
                            {
                                case HrLeaveType.Annual: row.Annual += 0.5m; break;
                                case HrLeaveType.Sick: row.Sick += 0.5m; break;
                                case HrLeaveType.Personal: row.Personal += 0.5m; break;
                                case HrLeaveType.Unpaid: row.Unpaid += 0.5m; break;
                            }
                            break;
                    }
                }

                row.Cells[i] = BuildCell(m, a, day.Kind == HrDayKind.HalfWork);
            }

            row.GoOutCount += outRequestCount;
            return row;
        }

        private static double HoursIn(TimeSpan start, TimeSpan end, bool morning)
        {
            var segStart = morning ? new TimeSpan(8, 0, 0) : new TimeSpan(13, 0, 0);
            var segEnd = morning ? Noon : new TimeSpan(17, 0, 0);
            var s = start > segStart ? start : segStart;
            var e = end < segEnd ? end : segEnd;
            return e > s ? (e - s).TotalHours : 0;
        }

        private static string SegCode(SegState s) => s.Kind switch
        {
            Seg.Worked => "X",
            Seg.Remote => "TX",
            Seg.Absent => "V",
            Seg.Leave => HrLeaveType.Code(s.LeaveType),
            Seg.Future => "",
            _ => ""
        };

        private static HrTimesheetCell BuildCell(SegState m, SegState a, bool halfDay)
        {
            var cm = SegCode(m);
            if (halfDay)
                return new HrTimesheetCell { Code = cm, Warn = m.Kind == Seg.Absent };
            var ca = SegCode(a);
            var code = cm == ca ? cm : $"{(cm == "" ? "·" : cm)}/{(ca == "" ? "·" : ca)}";
            return new HrTimesheetCell { Code = code, Warn = m.Kind == Seg.Absent || a.Kind == Seg.Absent };
        }

        // ───────────── Xuất Excel ─────────────

        public static byte[] ExportExcel(HrTimesheet sheet, Func<string, string> L)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add($"Cong_{sheet.Month:D2}_{sheet.Year}");

            ws.Cell(1, 1).Value = $"{L("hr_timesheet")} {sheet.Month:D2}/{sheet.Year}";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"{L("hr_ts_standard_days")}: {sheet.StandardDays:0.##}";

            const int headerRow = 4;
            var col = 1;
            ws.Cell(headerRow, col++).Value = L("hr_employee_code");
            ws.Cell(headerRow, col++).Value = L("hr_full_name");
            ws.Cell(headerRow, col++).Value = L("hr_department");
            var firstDayCol = col;
            foreach (var d in sheet.Days)
            {
                var c = ws.Cell(headerRow, col++);
                c.Value = d.Date.Day;
                if (d.Kind is HrDayKind.Off or HrDayKind.Holiday)
                    ws.Column(c.Address.ColumnNumber).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEEEEE");
            }
            string[] totals =
            {
                L("hr_ts_standard"), L("hr_ts_worked"), L("hr_ts_remote"), L("hr_ts_annual"), L("hr_ts_sick"),
                L("hr_ts_personal"), L("hr_ts_unpaid"), L("hr_ts_holiday"), L("hr_ts_absent"), L("hr_ts_goout"),
                L("hr_ts_paid")
            };
            var firstTotalCol = col;
            foreach (var t in totals) ws.Cell(headerRow, col++).Value = t;
            var lastCol = col - 1;

            var r = headerRow + 1;
            foreach (var row in sheet.Rows)
            {
                col = 1;
                ws.Cell(r, col++).Value = row.EmployeeCode;
                ws.Cell(r, col++).Value = row.FullName;
                ws.Cell(r, col++).Value = row.DepartmentName ?? "";
                foreach (var cell in row.Cells)
                {
                    var c = ws.Cell(r, col++);
                    c.Value = cell.Code;
                    if (cell.Warn) c.Style.Font.FontColor = XLColor.Red;
                }
                decimal[] vals =
                {
                    row.Standard, row.Worked, row.Remote, row.Annual, row.Sick, row.Personal, row.Unpaid,
                    row.Holiday, row.Absent, row.GoOutCount, row.Paid
                };
                foreach (var v in vals)
                {
                    var c = ws.Cell(r, col++);
                    c.Value = v;
                    c.Style.NumberFormat.Format = "0.##";
                }
                r++;
            }

            var header = ws.Range(headerRow, 1, headerRow, lastCol);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            var table = ws.Range(headerRow, 1, Math.Max(headerRow, r - 1), lastCol);
            table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(headerRow, firstDayCol, Math.Max(headerRow, r - 1), firstTotalCol - 1)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.SheetView.FreezeRows(headerRow);
            ws.SheetView.FreezeColumns(2);
            ws.Columns(1, 3).AdjustToContents();
            for (var c = firstDayCol; c < firstTotalCol; c++) ws.Column(c).Width = 5;
            ws.Columns(firstTotalCol, lastCol).AdjustToContents();

            // Chú thích
            var legend = wb.Worksheets.Add(L("hr_ts_legend"));
            var items = new (string Code, string Key)[]
            {
                ("X", "hr_ts_code_x"), ("TX", "hr_ts_code_tx"), ("P", "hr_leave_type_annual"), ("Ô", "hr_leave_type_sick"),
                ("R", "hr_leave_type_personal"), ("KL", "hr_leave_type_unpaid"), ("L", "hr_ts_code_l"),
                ("V", "hr_ts_code_v"), ("?", "hr_ts_code_q"), ("-", "hr_ts_code_dash"), ("a/b", "hr_ts_code_split")
            };
            var lr = 1;
            foreach (var (code, key) in items)
            {
                legend.Cell(lr, 1).Value = code;
                legend.Cell(lr, 2).Value = L(key);
                lr++;
            }
            legend.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
