/*
  TỔNG HỢP THAY ĐỔI DATABASE — NVOAMASIS (28–29/09/2026, có thêm kho 10.18)
  Chạy trên TỪNG tenant DB + DB template. Chạy lại nhiều lần vẫn an toàn (có kiểm tra tồn tại).
  Sau khi chạy: khởi động lại app (localizer cache nhãn giao diện).

  1. AttendanceLogs  — THÊM 7 CỘT + 1 INDEX (chấm công hằng ngày 12.10, giờ vào/ra, chấm bù, mobile)
       PunchType nvarchar(10) NULL      'in' / 'out' / NULL = chấm theo buổi (dữ liệu cũ)
       Latitude  decimal(9,6) NULL      tọa độ GPS khi chấm bằng điện thoại
       Longitude decimal(9,6) NULL
       DistanceM int NULL               khoảng cách tới văn phòng (m)
       IsManual  bit NOT NULL DEFAULT 0 1 = HR chấm bù
       Note      nvarchar(500) NULL     lý do chấm bù
       CreatedBy nvarchar(100) NULL     người chấm bù
       IX_AttendanceLogs_LocalDate_UserId (LocalDate, UserId)
  2. MenuNames       — THÊM DÒNG mã quyền HR_Attendance (12.10 Chấm công hằng ngày)
     Permissions     — cấp HR_Attendance cho user phòng ADMIN (nếu chưa có)
  3. HrSetting       — THÊM DÒNG cấu hình mới (giá trị mặc định = như hệ thống đang chạy, sửa ở 12.8)
  4. LocalizationResources — THÊM / CẬP NHẬT 87 khóa nhãn giao diện (12.8, 12.10)
  5. KhoVatTu, KhoHang, PhieuKho, PhieuKhoChiTiet — 4 BẢNG MỚI (10.18 kho vật tư, hàng hóa TK 151–156)
     MenuNames / Permissions — mã quyền KHO_DanhMuc, KHO_Phieu, KHO_BaoCao (cấp cho ADMIN)
     (= Scripts/CreateKhoTables.sql. Phiếu kho khi ghi sổ tạo dòng trong AccountingVouchers / AccountingVoucherLines /
      GeneralLedgerEntries có sẵn, SourceModule = 'INVENTORY' — không đổi cấu trúc các bảng đó)
  6. HrContract  — THÊM 1 CỘT IsNetSalary (lương NET / GROSS, 12.3)
     HrPayslip   — THÊM 20 CỘT: IsNet, GrossUp, Pro* (tách phần thử việc khi trong tháng đổi hợp đồng, 12.9)
     LocalizationResources — thêm nhãn cho 2 chức năng trên
     (= Scripts/AlterHrPayroll_NetGross_ThuViec.sql)

  KHÔNG đổi cấu trúc bảng nào khác. Các chức năng còn lại chỉ ghi DỮ LIỆU vào bảng có sẵn:
   - HistoryLogs: lịch sử khóa / mở kỳ kế toán (EntityName = 'accounting_period')
   - HrSetting: các khóa ở mục 3 được ghi khi bấm Lưu tại 12.8
   - MBL: số MBL tự tạo dạng MBL + yyMM + 5 số (cột Mbl có sẵn)
  Tương đương: migration 20261001090000_AddHrAttendanceDaily + Scripts/AlterAttendanceLogsDaily.sql
             + phần "Bổ sung" cuối Scripts/Localization_HR.sql.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- ═════════ 1 + 2. AttendanceLogs, MenuNames, Permissions ═════════
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.AttendanceLogs', N'PunchType') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [PunchType] nvarchar(10) NULL;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'Latitude') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [Latitude] decimal(9,6) NULL;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'Longitude') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [Longitude] decimal(9,6) NULL;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'DistanceM') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [DistanceM] int NULL;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'IsManual') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [IsManual] bit NOT NULL CONSTRAINT [DF_AttendanceLogs_IsManual] DEFAULT 0;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'Note') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [Note] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.AttendanceLogs', N'CreatedBy') IS NULL
    ALTER TABLE [dbo].[AttendanceLogs] ADD [CreatedBy] nvarchar(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceLogs_LocalDate_UserId'
               AND object_id = OBJECT_ID(N'dbo.AttendanceLogs'))
    CREATE INDEX [IX_AttendanceLogs_LocalDate_UserId] ON [dbo].[AttendanceLogs]([LocalDate], [UserId]);

IF OBJECT_ID(N'dbo.MenuNames', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[MenuNames] WHERE MenuName = N'HR_Attendance')
    INSERT INTO [dbo].[MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), N'HR_Attendance', N'12.10 Cham cong hang ngay');

-- Cấp quyền 12.10 cho user phòng ADMIN (giống Add_HR_Menu_Permissions.sql)
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, u.Usr, 1, 1, 1, 0, 1
FROM [UserList] u
CROSS JOIN [MenuNames] m
WHERE u.Department = N'ADMIN'
  AND u.Usr IS NOT NULL
  AND m.MenuName = N'HR_Attendance'
  AND NOT EXISTS (SELECT 1 FROM [Permissions] p WHERE p.UserName = u.Usr AND p.MenuName = m.MenuName);

INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, u.Usr, 1, 1, 1, 0, 1
FROM [UserList] u
CROSS JOIN [MenuNames] m
WHERE u.Department = N'ADMIN'
  AND u.Usr IS NOT NULL
  AND m.MenuName = N'HR_Attendance'
  AND NOT EXISTS (SELECT 1 FROM [Permissions] p WHERE p.UserName = u.Usr AND p.MenuName = m.MenuName);

COMMIT TRANSACTION;
GO

-- ═════════ 3. HrSetting — cấu hình mới (chỉ thêm nếu chưa có; không ghi đè giá trị đã cài) ═════════
IF OBJECT_ID(N'dbo.HrSetting', N'U') IS NOT NULL
BEGIN
    ;WITH s([Key], [Value]) AS (
        SELECT N'SaturdayHalfDayFullCredit', N'false' UNION ALL  -- 12.8: sáng T7 tính 1 công
        SELECT N'FixedStandardDays',         N'0'     UNION ALL  -- 12.8: công chuẩn cố định (0 = theo lịch)
        SELECT N'AttendanceMode',            N'session' UNION ALL -- session = theo buổi (như cũ) / inout = giờ vào–ra
        SELECT N'WorkStart',                 N'08:00' UNION ALL
        SELECT N'LunchStart',                N'12:00' UNION ALL
        SELECT N'LunchEnd',                  N'13:00' UNION ALL
        SELECT N'WorkEnd',                   N'17:00' UNION ALL
        SELECT N'SaturdayEnd',               N'12:00' UNION ALL
        SELECT N'LateGraceMinutes',          N'5'     UNION ALL
        SELECT N'OfficeLatitude',            N''      UNION ALL  -- trống = không dùng GPS
        SELECT N'OfficeLongitude',           N''      UNION ALL
        SELECT N'OfficeRadiusM',             N'200'   UNION ALL
        SELECT N'MobileRequireOnsite',       N'false'
    )
    INSERT INTO [dbo].[HrSetting] ([Key], [Value], [UpdatedAt], [UpdatedBy])
    SELECT s.[Key], s.[Value], SYSDATETIME(), N'system'
    FROM s
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[HrSetting] x WHERE x.[Key] = s.[Key]);
END;
GO

-- ═════════ 4. LocalizationResources — nhãn giao diện mới ═════════
BEGIN TRANSACTION;

-- Bổ sung: sáng T7 tính 1 công, công chuẩn cố định
;WITH src AS (
    SELECT N'hr_set_saturday_full_credit' AS ResourceKey, N'en-US' AS Culture, N'Count Saturday morning as 1 full day' AS Value UNION ALL
    SELECT N'hr_set_saturday_full_credit', N'vi-VN', N'Tính buổi sáng T7 là 1 công' UNION ALL
    SELECT N'hr_set_saturday_full_credit', N'zh-CN', N'周六上午按 1 天计' UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint' AS ResourceKey, N'en-US' AS Culture, N'For Mon – Sat morning schedules paid on ~26 days/month. Saturday morning worked = 1 day, leave on Saturday morning = 1 day.' AS Value UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint', N'vi-VN', N'Dùng khi làm T2 – sáng T7, công chuẩn ~26 công/tháng. Đi làm sáng T7 = 1 công, nghỉ phép sáng T7 = trừ 1 ngày phép.' UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint', N'zh-CN', N'适用于周一至周六上午、每月约 26 个工日。周六上午出勤 = 1 天，周六上午请假 = 1 天。' UNION ALL
    SELECT N'hr_set_fixed_standard' AS ResourceKey, N'en-US' AS Culture, N'Fixed standard days for payroll' AS Value UNION ALL
    SELECT N'hr_set_fixed_standard', N'vi-VN', N'Công chuẩn cố định khi tính lương' UNION ALL
    SELECT N'hr_set_fixed_standard', N'zh-CN', N'计薪固定标准工日' UNION ALL
    SELECT N'hr_set_fixed_standard_hint' AS ResourceKey, N'en-US' AS Culture, N'0 = actual working days of each month. E.g. 24: full attendance = full salary, each missing day deducts Salary / 24.' AS Value UNION ALL
    SELECT N'hr_set_fixed_standard_hint', N'vi-VN', N'0 = theo lịch thực tế từng tháng. Vd 24: đi làm đủ = đủ lương, mỗi ngày nghỉ không lương trừ Lương / 24.' UNION ALL
    SELECT N'hr_set_fixed_standard_hint', N'zh-CN', N'0 = 按每月实际工作日。例如 24：全勤 = 全薪，每缺勤一天扣 工资 / 24。' UNION ALL
    SELECT N'hr_err_fixed_standard_days' AS ResourceKey, N'en-US' AS Culture, N'Fixed standard days must be between 0 and 31' AS Value UNION ALL
    SELECT N'hr_err_fixed_standard_days', N'vi-VN', N'Công chuẩn cố định phải từ 0 đến 31' UNION ALL
    SELECT N'hr_err_fixed_standard_days', N'zh-CN', N'固定标准工日须在 0 到 31 之间'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Bổ sung: chấm công hằng ngày (12.10)
;WITH src AS (
    SELECT N'hr_attendance' AS ResourceKey, N'en-US' AS Culture, N'Daily attendance' AS Value UNION ALL
    SELECT N'hr_attendance', N'vi-VN', N'Chấm công hằng ngày' UNION ALL
    SELECT N'hr_attendance', N'zh-CN', N'每日考勤' UNION ALL
    SELECT N'hr_attendance_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out by day, late arrivals, HR corrections' AS Value UNION ALL
    SELECT N'hr_attendance_subtitle', N'vi-VN', N'Giờ vào / ra từng ngày, đi muộn, HR chấm bù' UNION ALL
    SELECT N'hr_attendance_subtitle', N'zh-CN', N'每日上下班打卡、迟到、人事补卡' UNION ALL
    SELECT N'hr_att_tab_daily' AS ResourceKey, N'en-US' AS Culture, N'By day' AS Value UNION ALL
    SELECT N'hr_att_tab_daily', N'vi-VN', N'Theo ngày' UNION ALL
    SELECT N'hr_att_tab_daily', N'zh-CN', N'按日' UNION ALL
    SELECT N'hr_att_tab_mine' AS ResourceKey, N'en-US' AS Culture, N'My attendance' AS Value UNION ALL
    SELECT N'hr_att_tab_mine', N'vi-VN', N'Công của tôi' UNION ALL
    SELECT N'hr_att_tab_mine', N'zh-CN', N'我的考勤' UNION ALL
    SELECT N'hr_att_today' AS ResourceKey, N'en-US' AS Culture, N'Today' AS Value UNION ALL
    SELECT N'hr_att_today', N'vi-VN', N'Hôm nay' UNION ALL
    SELECT N'hr_att_today', N'zh-CN', N'今天' UNION ALL
    SELECT N'hr_att_code' AS ResourceKey, N'en-US' AS Culture, N'Timesheet' AS Value UNION ALL
    SELECT N'hr_att_code', N'vi-VN', N'Công' UNION ALL
    SELECT N'hr_att_code', N'zh-CN', N'考勤' UNION ALL
    SELECT N'hr_att_first_in' AS ResourceKey, N'en-US' AS Culture, N'In' AS Value UNION ALL
    SELECT N'hr_att_first_in', N'vi-VN', N'Giờ vào' UNION ALL
    SELECT N'hr_att_first_in', N'zh-CN', N'上班' UNION ALL
    SELECT N'hr_att_last_out' AS ResourceKey, N'en-US' AS Culture, N'Out' AS Value UNION ALL
    SELECT N'hr_att_last_out', N'vi-VN', N'Giờ ra' UNION ALL
    SELECT N'hr_att_last_out', N'zh-CN', N'下班' UNION ALL
    SELECT N'hr_att_late' AS ResourceKey, N'en-US' AS Culture, N'Late (min)' AS Value UNION ALL
    SELECT N'hr_att_late', N'vi-VN', N'Muộn (phút)' UNION ALL
    SELECT N'hr_att_late', N'zh-CN', N'迟到(分)' UNION ALL
    SELECT N'hr_att_early' AS ResourceKey, N'en-US' AS Culture, N'Early leave (min)' AS Value UNION ALL
    SELECT N'hr_att_early', N'vi-VN', N'Về sớm (phút)' UNION ALL
    SELECT N'hr_att_early', N'zh-CN', N'早退(分)' UNION ALL
    SELECT N'hr_att_status' AS ResourceKey, N'en-US' AS Culture, N'Status' AS Value UNION ALL
    SELECT N'hr_att_status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'hr_att_status', N'zh-CN', N'状态' UNION ALL
    SELECT N'hr_att_source' AS ResourceKey, N'en-US' AS Culture, N'Source' AS Value UNION ALL
    SELECT N'hr_att_source', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'hr_att_source', N'zh-CN', N'来源' UNION ALL
    SELECT N'hr_att_time' AS ResourceKey, N'en-US' AS Culture, N'Time' AS Value UNION ALL
    SELECT N'hr_att_time', N'vi-VN', N'Giờ' UNION ALL
    SELECT N'hr_att_time', N'zh-CN', N'时间' UNION ALL
    SELECT N'hr_att_type' AS ResourceKey, N'en-US' AS Culture, N'Type' AS Value UNION ALL
    SELECT N'hr_att_type', N'vi-VN', N'Loại' UNION ALL
    SELECT N'hr_att_type', N'zh-CN', N'类型' UNION ALL
    SELECT N'hr_att_session' AS ResourceKey, N'en-US' AS Culture, N'Session' AS Value UNION ALL
    SELECT N'hr_att_session', N'vi-VN', N'Buổi' UNION ALL
    SELECT N'hr_att_session', N'zh-CN', N'时段' UNION ALL
    SELECT N'hr_att_place' AS ResourceKey, N'en-US' AS Culture, N'Location' AS Value UNION ALL
    SELECT N'hr_att_place', N'vi-VN', N'Vị trí' UNION ALL
    SELECT N'hr_att_place', N'zh-CN', N'位置' UNION ALL
    SELECT N'hr_att_in' AS ResourceKey, N'en-US' AS Culture, N'Check-in' AS Value UNION ALL
    SELECT N'hr_att_in', N'vi-VN', N'Vào' UNION ALL
    SELECT N'hr_att_in', N'zh-CN', N'上班' UNION ALL
    SELECT N'hr_att_out' AS ResourceKey, N'en-US' AS Culture, N'Check-out' AS Value UNION ALL
    SELECT N'hr_att_out', N'vi-VN', N'Ra' UNION ALL
    SELECT N'hr_att_out', N'zh-CN', N'下班' UNION ALL
    SELECT N'hr_att_by_session' AS ResourceKey, N'en-US' AS Culture, N'By session' AS Value UNION ALL
    SELECT N'hr_att_by_session', N'vi-VN', N'Theo buổi' UNION ALL
    SELECT N'hr_att_by_session', N'zh-CN', N'按时段' UNION ALL
    SELECT N'hr_att_morning' AS ResourceKey, N'en-US' AS Culture, N'Morning' AS Value UNION ALL
    SELECT N'hr_att_morning', N'vi-VN', N'Sáng' UNION ALL
    SELECT N'hr_att_morning', N'zh-CN', N'上午' UNION ALL
    SELECT N'hr_att_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Afternoon' AS Value UNION ALL
    SELECT N'hr_att_afternoon', N'vi-VN', N'Chiều' UNION ALL
    SELECT N'hr_att_afternoon', N'zh-CN', N'下午' UNION ALL
    SELECT N'hr_att_onsite' AS ResourceKey, N'en-US' AS Culture, N'At office' AS Value UNION ALL
    SELECT N'hr_att_onsite', N'vi-VN', N'Tại văn phòng' UNION ALL
    SELECT N'hr_att_onsite', N'zh-CN', N'在办公室' UNION ALL
    SELECT N'hr_att_remote' AS ResourceKey, N'en-US' AS Culture, N'Remote' AS Value UNION ALL
    SELECT N'hr_att_remote', N'vi-VN', N'Từ xa' UNION ALL
    SELECT N'hr_att_remote', N'zh-CN', N'远程' UNION ALL
    SELECT N'hr_att_src_web' AS ResourceKey, N'en-US' AS Culture, N'Web' AS Value UNION ALL
    SELECT N'hr_att_src_web', N'vi-VN', N'Web' UNION ALL
    SELECT N'hr_att_src_web', N'zh-CN', N'网页' UNION ALL
    SELECT N'hr_att_src_mobile' AS ResourceKey, N'en-US' AS Culture, N'Mobile' AS Value UNION ALL
    SELECT N'hr_att_src_mobile', N'vi-VN', N'Điện thoại' UNION ALL
    SELECT N'hr_att_src_mobile', N'zh-CN', N'手机' UNION ALL
    SELECT N'hr_att_src_manual' AS ResourceKey, N'en-US' AS Culture, N'HR correction' AS Value UNION ALL
    SELECT N'hr_att_src_manual', N'vi-VN', N'HR chấm bù' UNION ALL
    SELECT N'hr_att_src_manual', N'zh-CN', N'人事补卡' UNION ALL
    SELECT N'hr_att_punches' AS ResourceKey, N'en-US' AS Culture, N'Attendance records' AS Value UNION ALL
    SELECT N'hr_att_punches', N'vi-VN', N'Các lần chấm công' UNION ALL
    SELECT N'hr_att_punches', N'zh-CN', N'打卡记录' UNION ALL
    SELECT N'hr_att_no_punch' AS ResourceKey, N'en-US' AS Culture, N'No records' AS Value UNION ALL
    SELECT N'hr_att_no_punch', N'vi-VN', N'Chưa có lần chấm nào' UNION ALL
    SELECT N'hr_att_no_punch', N'zh-CN', N'无打卡记录' UNION ALL
    SELECT N'hr_att_manual' AS ResourceKey, N'en-US' AS Culture, N'HR correction' AS Value UNION ALL
    SELECT N'hr_att_manual', N'vi-VN', N'HR chấm bù / sửa công' UNION ALL
    SELECT N'hr_att_manual', N'zh-CN', N'人事补卡 / 修改' UNION ALL
    SELECT N'hr_att_manual_reason' AS ResourceKey, N'en-US' AS Culture, N'Reason (required)' AS Value UNION ALL
    SELECT N'hr_att_manual_reason', N'vi-VN', N'Lý do (bắt buộc)' UNION ALL
    SELECT N'hr_att_manual_reason', N'zh-CN', N'原因（必填）' UNION ALL
    SELECT N'hr_att_manual_reason_hint' AS ResourceKey, N'en-US' AS Culture, N'E.g. forgot to check in, business trip, device error' AS Value UNION ALL
    SELECT N'hr_att_manual_reason_hint', N'vi-VN', N'Vd: quên chấm, đi công tác, lỗi mạng' UNION ALL
    SELECT N'hr_att_manual_reason_hint', N'zh-CN', N'例如：忘记打卡、出差、设备故障' UNION ALL
    SELECT N'hr_att_preset_full' AS ResourceKey, N'en-US' AS Culture, N'Full day' AS Value UNION ALL
    SELECT N'hr_att_preset_full', N'vi-VN', N'Bù cả ngày' UNION ALL
    SELECT N'hr_att_preset_full', N'zh-CN', N'补全天' UNION ALL
    SELECT N'hr_att_preset_morning' AS ResourceKey, N'en-US' AS Culture, N'Morning' AS Value UNION ALL
    SELECT N'hr_att_preset_morning', N'vi-VN', N'Bù buổi sáng' UNION ALL
    SELECT N'hr_att_preset_morning', N'zh-CN', N'补上午' UNION ALL
    SELECT N'hr_att_preset_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Afternoon' AS Value UNION ALL
    SELECT N'hr_att_preset_afternoon', N'vi-VN', N'Bù buổi chiều' UNION ALL
    SELECT N'hr_att_preset_afternoon', N'zh-CN', N'补下午' UNION ALL
    SELECT N'hr_att_add_punch' AS ResourceKey, N'en-US' AS Culture, N'Add' AS Value UNION ALL
    SELECT N'hr_att_add_punch', N'vi-VN', N'Thêm lần chấm' UNION ALL
    SELECT N'hr_att_add_punch', N'zh-CN', N'添加' UNION ALL
    SELECT N'hr_att_manual_hint' AS ResourceKey, N'en-US' AS Culture, N'Corrections are saved with your name and reason. Only corrections can be deleted; employees'' own records are kept. Months with a locked payroll cannot be changed.' AS Value UNION ALL
    SELECT N'hr_att_manual_hint', N'vi-VN', N'Lần chấm bù được lưu kèm tên người sửa và lý do. Chỉ xóa được lần chấm bù; lần chấm thật của nhân viên được giữ nguyên. Tháng đã chốt lương không sửa được.' UNION ALL
    SELECT N'hr_att_manual_hint', N'zh-CN', N'补卡记录会保存修改人和原因。只能删除补卡记录；员工本人的打卡记录保留。已锁定工资的月份不能修改。' UNION ALL
    SELECT N'hr_att_click_hint' AS ResourceKey, N'en-US' AS Culture, N'Click a row to see each check-in / check-out.' AS Value UNION ALL
    SELECT N'hr_att_click_hint', N'vi-VN', N'Bấm vào 1 dòng để xem các lần chấm công.' UNION ALL
    SELECT N'hr_att_click_hint', N'zh-CN', N'点击一行查看打卡记录。' UNION ALL
    SELECT N'hr_att_click_hint_edit' AS ResourceKey, N'en-US' AS Culture, N'Click a row to see records and add corrections.' AS Value UNION ALL
    SELECT N'hr_att_click_hint_edit', N'vi-VN', N'Bấm vào 1 dòng để xem các lần chấm và chấm bù.' UNION ALL
    SELECT N'hr_att_click_hint_edit', N'zh-CN', N'点击一行查看记录并补卡。' UNION ALL
    SELECT N'hr_att_mine_summary' AS ResourceKey, N'en-US' AS Culture, N'Late {0} times ({1} min) · Early leave {2} times · Absent / missing check-out {3} days' AS Value UNION ALL
    SELECT N'hr_att_mine_summary', N'vi-VN', N'Đi muộn {0} lần ({1} phút) · Về sớm {2} lần · Vắng / quên chấm ra {3} ngày' UNION ALL
    SELECT N'hr_att_mine_summary', N'zh-CN', N'迟到 {0} 次（{1} 分钟）· 早退 {2} 次 · 缺勤 / 漏打下班卡 {3} 天' UNION ALL
    SELECT N'hr_att_mine_hint' AS ResourceKey, N'en-US' AS Culture, N'Wrong or missing record? Contact HR for a correction.' AS Value UNION ALL
    SELECT N'hr_att_mine_hint', N'vi-VN', N'Thiếu hoặc sai công? Liên hệ HR để chấm bù.' UNION ALL
    SELECT N'hr_att_mine_hint', N'zh-CN', N'记录缺失或有误？请联系人事补卡。' UNION ALL
    SELECT N'hr_att_no_profile' AS ResourceKey, N'en-US' AS Culture, N'Your account is not linked to an employee profile.' AS Value UNION ALL
    SELECT N'hr_att_no_profile', N'vi-VN', N'Tài khoản của bạn chưa gắn với hồ sơ nhân viên.' UNION ALL
    SELECT N'hr_att_no_profile', N'zh-CN', N'您的账号尚未关联员工档案。' UNION ALL
    SELECT N'hr_att_st_present' AS ResourceKey, N'en-US' AS Culture, N'Present' AS Value UNION ALL
    SELECT N'hr_att_st_present', N'vi-VN', N'Có mặt' UNION ALL
    SELECT N'hr_att_st_present', N'zh-CN', N'出勤' UNION ALL
    SELECT N'hr_att_st_late' AS ResourceKey, N'en-US' AS Culture, N'Late' AS Value UNION ALL
    SELECT N'hr_att_st_late', N'vi-VN', N'Đi muộn' UNION ALL
    SELECT N'hr_att_st_late', N'zh-CN', N'迟到' UNION ALL
    SELECT N'hr_att_st_missing_out' AS ResourceKey, N'en-US' AS Culture, N'No check-out' AS Value UNION ALL
    SELECT N'hr_att_st_missing_out', N'vi-VN', N'Quên chấm ra' UNION ALL
    SELECT N'hr_att_st_missing_out', N'zh-CN', N'漏打下班卡' UNION ALL
    SELECT N'hr_att_st_not_yet' AS ResourceKey, N'en-US' AS Culture, N'Not yet' AS Value UNION ALL
    SELECT N'hr_att_st_not_yet', N'vi-VN', N'Chưa chấm' UNION ALL
    SELECT N'hr_att_st_not_yet', N'zh-CN', N'未打卡' UNION ALL
    SELECT N'hr_att_st_leave' AS ResourceKey, N'en-US' AS Culture, N'On leave' AS Value UNION ALL
    SELECT N'hr_att_st_leave', N'vi-VN', N'Nghỉ phép' UNION ALL
    SELECT N'hr_att_st_leave', N'zh-CN', N'请假' UNION ALL
    SELECT N'hr_att_st_absent' AS ResourceKey, N'en-US' AS Culture, N'Absent' AS Value UNION ALL
    SELECT N'hr_att_st_absent', N'vi-VN', N'Vắng' UNION ALL
    SELECT N'hr_att_st_absent', N'zh-CN', N'缺勤' UNION ALL
    SELECT N'hr_att_st_off' AS ResourceKey, N'en-US' AS Culture, N'Day off' AS Value UNION ALL
    SELECT N'hr_att_st_off', N'vi-VN', N'Ngày nghỉ' UNION ALL
    SELECT N'hr_att_st_off', N'zh-CN', N'休息日' UNION ALL
    SELECT N'hr_att_st_holiday' AS ResourceKey, N'en-US' AS Culture, N'Holiday' AS Value UNION ALL
    SELECT N'hr_att_st_holiday', N'vi-VN', N'Nghỉ lễ' UNION ALL
    SELECT N'hr_att_st_holiday', N'zh-CN', N'节假日' UNION ALL
    SELECT N'hr_att_st_not_employed' AS ResourceKey, N'en-US' AS Culture, N'Not employed' AS Value UNION ALL
    SELECT N'hr_att_st_not_employed', N'vi-VN', N'Chưa vào làm / đã nghỉ' UNION ALL
    SELECT N'hr_att_st_not_employed', N'zh-CN', N'未在职' UNION ALL
    SELECT N'hr_att_st_no_account' AS ResourceKey, N'en-US' AS Culture, N'No login account' AS Value UNION ALL
    SELECT N'hr_att_st_no_account', N'vi-VN', N'Chưa có tài khoản' UNION ALL
    SELECT N'hr_att_st_no_account', N'zh-CN', N'无账号' UNION ALL
    SELECT N'hr_att_sum_total' AS ResourceKey, N'en-US' AS Culture, N'Employees' AS Value UNION ALL
    SELECT N'hr_att_sum_total', N'vi-VN', N'Nhân viên' UNION ALL
    SELECT N'hr_att_sum_total', N'zh-CN', N'员工' UNION ALL
    SELECT N'hr_att_sum_present' AS ResourceKey, N'en-US' AS Culture, N'Present' AS Value UNION ALL
    SELECT N'hr_att_sum_present', N'vi-VN', N'Có mặt' UNION ALL
    SELECT N'hr_att_sum_present', N'zh-CN', N'出勤' UNION ALL
    SELECT N'hr_att_sum_late' AS ResourceKey, N'en-US' AS Culture, N'Late' AS Value UNION ALL
    SELECT N'hr_att_sum_late', N'vi-VN', N'Đi muộn' UNION ALL
    SELECT N'hr_att_sum_late', N'zh-CN', N'迟到' UNION ALL
    SELECT N'hr_att_sum_not_yet' AS ResourceKey, N'en-US' AS Culture, N'Not yet' AS Value UNION ALL
    SELECT N'hr_att_sum_not_yet', N'vi-VN', N'Chưa chấm' UNION ALL
    SELECT N'hr_att_sum_not_yet', N'zh-CN', N'未打卡' UNION ALL
    SELECT N'hr_att_sum_leave' AS ResourceKey, N'en-US' AS Culture, N'Leave' AS Value UNION ALL
    SELECT N'hr_att_sum_leave', N'vi-VN', N'Nghỉ phép' UNION ALL
    SELECT N'hr_att_sum_leave', N'zh-CN', N'请假' UNION ALL
    SELECT N'hr_att_sum_absent' AS ResourceKey, N'en-US' AS Culture, N'Absent' AS Value UNION ALL
    SELECT N'hr_att_sum_absent', N'vi-VN', N'Vắng' UNION ALL
    SELECT N'hr_att_sum_absent', N'zh-CN', N'缺勤' UNION ALL
    SELECT N'hr_att_sum_missing_out' AS ResourceKey, N'en-US' AS Culture, N'No check-out' AS Value UNION ALL
    SELECT N'hr_att_sum_missing_out', N'vi-VN', N'Quên chấm ra' UNION ALL
    SELECT N'hr_att_sum_missing_out', N'zh-CN', N'漏打下班卡' UNION ALL
    SELECT N'hr_att_sum_remote' AS ResourceKey, N'en-US' AS Culture, N'Remote' AS Value UNION ALL
    SELECT N'hr_att_sum_remote', N'vi-VN', N'Từ xa' UNION ALL
    SELECT N'hr_att_sum_remote', N'zh-CN', N'远程' UNION ALL
    SELECT N'hr_att_err_note_required' AS ResourceKey, N'en-US' AS Culture, N'Please enter a reason' AS Value UNION ALL
    SELECT N'hr_att_err_note_required', N'vi-VN', N'Vui lòng nhập lý do chấm bù' UNION ALL
    SELECT N'hr_att_err_note_required', N'zh-CN', N'请输入补卡原因' UNION ALL
    SELECT N'hr_att_err_time_required' AS ResourceKey, N'en-US' AS Culture, N'Please choose the date and time' AS Value UNION ALL
    SELECT N'hr_att_err_time_required', N'vi-VN', N'Vui lòng chọn ngày và giờ' UNION ALL
    SELECT N'hr_att_err_time_required', N'zh-CN', N'请选择日期和时间' UNION ALL
    SELECT N'hr_att_err_future' AS ResourceKey, N'en-US' AS Culture, N'Cannot add a record in the future' AS Value UNION ALL
    SELECT N'hr_att_err_future', N'vi-VN', N'Không chấm bù cho thời điểm trong tương lai' UNION ALL
    SELECT N'hr_att_err_future', N'zh-CN', N'不能补未来的打卡' UNION ALL
    SELECT N'hr_att_err_only_manual' AS ResourceKey, N'en-US' AS Culture, N'Only HR corrections can be deleted' AS Value UNION ALL
    SELECT N'hr_att_err_only_manual', N'vi-VN', N'Chỉ xóa được lần chấm bù của HR' UNION ALL
    SELECT N'hr_att_err_only_manual', N'zh-CN', N'只能删除人事补卡记录' UNION ALL
    SELECT N'hr_att_err_period_locked' AS ResourceKey, N'en-US' AS Culture, N'Payroll for this month is locked. Unlock it before changing attendance.' AS Value UNION ALL
    SELECT N'hr_att_err_period_locked', N'vi-VN', N'Tháng này đã chốt lương. Mở chốt bảng lương trước khi sửa công.' UNION ALL
    SELECT N'hr_att_err_period_locked', N'zh-CN', N'本月工资已锁定，请先解锁再修改考勤。' UNION ALL
    SELECT N'hr_set_att_mode' AS ResourceKey, N'en-US' AS Culture, N'Attendance mode' AS Value UNION ALL
    SELECT N'hr_set_att_mode', N'vi-VN', N'Chế độ chấm công' UNION ALL
    SELECT N'hr_set_att_mode', N'zh-CN', N'考勤模式' UNION ALL
    SELECT N'hr_set_att_mode_session' AS ResourceKey, N'en-US' AS Culture, N'By session (morning 1h window, afternoon 1h window)' AS Value UNION ALL
    SELECT N'hr_set_att_mode_session', N'vi-VN', N'Theo buổi (khung 1 giờ đầu buổi sáng / chiều)' UNION ALL
    SELECT N'hr_set_att_mode_session', N'zh-CN', N'按时段（上午/下午开始后1小时内）' UNION ALL
    SELECT N'hr_set_att_mode_inout' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out at any time' AS Value UNION ALL
    SELECT N'hr_set_att_mode_inout', N'vi-VN', N'Giờ vào – giờ ra (chấm bất kỳ lúc nào)' UNION ALL
    SELECT N'hr_set_att_mode_inout', N'zh-CN', N'上下班打卡（任意时间）' UNION ALL
    SELECT N'hr_set_att_mode_hint' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out: 8:00 in + 17:00 out counts both sessions; late / early minutes are tracked.' AS Value UNION ALL
    SELECT N'hr_set_att_mode_hint', N'vi-VN', N'Giờ vào – giờ ra: vào 8:00 + ra 17:00 = đủ 2 buổi; tính số phút đi muộn / về sớm.' UNION ALL
    SELECT N'hr_set_att_mode_hint', N'zh-CN', N'上下班打卡：8:00上班 + 17:00下班 = 两个时段；统计迟到/早退分钟。' UNION ALL
    SELECT N'hr_set_work_start' AS ResourceKey, N'en-US' AS Culture, N'Start' AS Value UNION ALL
    SELECT N'hr_set_work_start', N'vi-VN', N'Giờ vào làm' UNION ALL
    SELECT N'hr_set_work_start', N'zh-CN', N'上班时间' UNION ALL
    SELECT N'hr_set_lunch_start' AS ResourceKey, N'en-US' AS Culture, N'Lunch start' AS Value UNION ALL
    SELECT N'hr_set_lunch_start', N'vi-VN', N'Nghỉ trưa từ' UNION ALL
    SELECT N'hr_set_lunch_start', N'zh-CN', N'午休开始' UNION ALL
    SELECT N'hr_set_lunch_end' AS ResourceKey, N'en-US' AS Culture, N'Lunch end' AS Value UNION ALL
    SELECT N'hr_set_lunch_end', N'vi-VN', N'Làm chiều từ' UNION ALL
    SELECT N'hr_set_lunch_end', N'zh-CN', N'午休结束' UNION ALL
    SELECT N'hr_set_work_end' AS ResourceKey, N'en-US' AS Culture, N'End' AS Value UNION ALL
    SELECT N'hr_set_work_end', N'vi-VN', N'Giờ tan làm' UNION ALL
    SELECT N'hr_set_work_end', N'zh-CN', N'下班时间' UNION ALL
    SELECT N'hr_set_saturday_end' AS ResourceKey, N'en-US' AS Culture, N'Saturday end' AS Value UNION ALL
    SELECT N'hr_set_saturday_end', N'vi-VN', N'Tan làm T7' UNION ALL
    SELECT N'hr_set_saturday_end', N'zh-CN', N'周六下班' UNION ALL
    SELECT N'hr_set_late_grace' AS ResourceKey, N'en-US' AS Culture, N'Grace (min)' AS Value UNION ALL
    SELECT N'hr_set_late_grace', N'vi-VN', N'Cho phép muộn (phút)' UNION ALL
    SELECT N'hr_set_late_grace', N'zh-CN', N'宽限(分)' UNION ALL
    SELECT N'hr_set_mobile' AS ResourceKey, N'en-US' AS Culture, N'Mobile attendance' AS Value UNION ALL
    SELECT N'hr_set_mobile', N'vi-VN', N'Chấm công trên điện thoại' UNION ALL
    SELECT N'hr_set_mobile', N'zh-CN', N'手机打卡' UNION ALL
    SELECT N'hr_set_office_lat' AS ResourceKey, N'en-US' AS Culture, N'Office latitude' AS Value UNION ALL
    SELECT N'hr_set_office_lat', N'vi-VN', N'Vĩ độ văn phòng' UNION ALL
    SELECT N'hr_set_office_lat', N'zh-CN', N'办公室纬度' UNION ALL
    SELECT N'hr_set_office_lng' AS ResourceKey, N'en-US' AS Culture, N'Office longitude' AS Value UNION ALL
    SELECT N'hr_set_office_lng', N'vi-VN', N'Kinh độ văn phòng' UNION ALL
    SELECT N'hr_set_office_lng', N'zh-CN', N'办公室经度' UNION ALL
    SELECT N'hr_set_office_radius' AS ResourceKey, N'en-US' AS Culture, N'Radius (m)' AS Value UNION ALL
    SELECT N'hr_set_office_radius', N'vi-VN', N'Bán kính (m)' UNION ALL
    SELECT N'hr_set_office_radius', N'zh-CN', N'半径(米)' UNION ALL
    SELECT N'hr_set_office_hint' AS ResourceKey, N'en-US' AS Culture, N'Get coordinates from Google Maps (right-click the office). Phones on the office WiFi are also counted as at the office (IP list in company info). Leave empty to disable GPS.' AS Value UNION ALL
    SELECT N'hr_set_office_hint', N'vi-VN', N'Lấy tọa độ từ Google Maps (bấm chuột phải vào văn phòng). Điện thoại dùng WiFi văn phòng cũng được tính là tại văn phòng (theo danh sách IP trong thông tin công ty). Để trống = không dùng GPS.' UNION ALL
    SELECT N'hr_set_office_hint', N'zh-CN', N'在谷歌地图右键办公室获取坐标。连接办公室WiFi的手机也算在办公室（公司信息中的IP列表）。留空 = 不使用GPS。' UNION ALL
    SELECT N'hr_set_mobile_require_onsite' AS ResourceKey, N'en-US' AS Culture, N'Phones can only check in at the office' AS Value UNION ALL
    SELECT N'hr_set_mobile_require_onsite', N'vi-VN', N'Điện thoại chỉ chấm được khi ở văn phòng' UNION ALL
    SELECT N'hr_set_mobile_require_onsite', N'zh-CN', N'手机只能在办公室打卡' UNION ALL
    SELECT N'hr_err_work_hours' AS ResourceKey, N'en-US' AS Culture, N'Working hours are invalid (start < lunch start ≤ lunch end < end)' AS Value UNION ALL
    SELECT N'hr_err_work_hours', N'vi-VN', N'Giờ làm không hợp lệ (vào < nghỉ trưa ≤ làm chiều < tan làm)' UNION ALL
    SELECT N'hr_err_work_hours', N'zh-CN', N'工作时间无效' UNION ALL
    SELECT N'hr_err_office_location' AS ResourceKey, N'en-US' AS Culture, N'Office coordinates are invalid (enter both latitude and longitude)' AS Value UNION ALL
    SELECT N'hr_err_office_location', N'vi-VN', N'Tọa độ văn phòng không hợp lệ (nhập đủ vĩ độ và kinh độ)' UNION ALL
    SELECT N'hr_err_office_location', N'zh-CN', N'办公室坐标无效（请同时填写纬度和经度）'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
GO

-- ═════════ 5. Kho vật tư, hàng hóa (10.18) ═════════
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.KhoVatTu', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KhoVatTu] (
        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_KhoVatTu] PRIMARY KEY,
        [Code]             nvarchar(50)     NOT NULL,
        [Name]             nvarchar(250)    NOT NULL,
        [Unit]             nvarchar(30)     NOT NULL CONSTRAINT [DF_KhoVatTu_Unit] DEFAULT N'',
        [InventoryAccount] nvarchar(20)     NOT NULL,
        [Category]         nvarchar(100)    NULL,
        [MinQty]           decimal(18,4)    NULL,
        [Note]             nvarchar(500)    NULL,
        [IsActive]         bit              NOT NULL CONSTRAINT [DF_KhoVatTu_IsActive] DEFAULT 1,
        [CreatedAt]        datetime2        NOT NULL CONSTRAINT [DF_KhoVatTu_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy]        nvarchar(100)    NULL,
        [UpdatedAt]        datetime2        NULL,
        [UpdatedBy]        nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_KhoVatTu_Code] ON [dbo].[KhoVatTu]([Code]);
END;

IF OBJECT_ID(N'dbo.KhoHang', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KhoHang] (
        [Id]        uniqueidentifier NOT NULL CONSTRAINT [PK_KhoHang] PRIMARY KEY,
        [Code]      nvarchar(50)     NOT NULL,
        [Name]      nvarchar(250)    NOT NULL,
        [Address]   nvarchar(500)    NULL,
        [Keeper]    nvarchar(150)    NULL,
        [IsActive]  bit              NOT NULL CONSTRAINT [DF_KhoHang_IsActive] DEFAULT 1,
        [CreatedAt] datetime2        NOT NULL CONSTRAINT [DF_KhoHang_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100)    NULL,
        [UpdatedAt] datetime2        NULL,
        [UpdatedBy] nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_KhoHang_Code] ON [dbo].[KhoHang]([Code]);
END;

IF OBJECT_ID(N'dbo.PhieuKho', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PhieuKho] (
        [Id]            uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKho] PRIMARY KEY,
        [DocType]       nvarchar(10)     NOT NULL,
        [DocNo]         nvarchar(30)     NOT NULL,
        [DocDate]       datetime2        NOT NULL,
        [Reason]        nvarchar(30)     NULL,
        [WarehouseId]   uniqueidentifier NOT NULL,
        [ToWarehouseId] uniqueidentifier NULL,
        [PartnerId]     uniqueidentifier NULL,
        [PartnerName]   nvarchar(250)    NULL,
        [ContactName]   nvarchar(150)    NULL,
        [ContraAccount] nvarchar(20)     NULL,
        [VatAccount]    nvarchar(20)     NULL,
        [InvoiceNo]     nvarchar(50)     NULL,
        [InvoiceDate]   datetime2        NULL,
        [Description]   nvarchar(500)    NULL,
        [Status]        int              NOT NULL CONSTRAINT [DF_PhieuKho_Status] DEFAULT 0,
        [VoucherId]     uniqueidentifier NULL,
        [CreatedAt]     datetime2        NOT NULL CONSTRAINT [DF_PhieuKho_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy]     nvarchar(100)    NULL,
        [UpdatedAt]     datetime2        NULL,
        [UpdatedBy]     nvarchar(100)    NULL,
        [PostedAt]      datetime2        NULL,
        [PostedBy]      nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_PhieuKho_DocNo] ON [dbo].[PhieuKho]([DocNo]);
    CREATE INDEX [IX_PhieuKho_DocDate_DocType] ON [dbo].[PhieuKho]([DocDate], [DocType]);
END;

IF OBJECT_ID(N'dbo.PhieuKhoChiTiet', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PhieuKhoChiTiet] (
        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKhoChiTiet] PRIMARY KEY,
        [PhieuKhoId]       uniqueidentifier NOT NULL,
        [LineNo]           int              NOT NULL,
        [VatTuId]          uniqueidentifier NOT NULL,
        [InventoryAccount] nvarchar(20)     NOT NULL,
        [ContraAccount]    nvarchar(20)     NULL,
        [Quantity]         decimal(18,4)    NOT NULL,
        [UnitCost]         decimal(18,4)    NOT NULL,
        [Amount]           decimal(18,2)    NOT NULL,
        [VatRate]          decimal(5,2)     NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatRate] DEFAULT 0,
        [VatAmount]        decimal(18,2)    NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatAmount] DEFAULT 0,
        [Note]             nvarchar(500)    NULL
    );
    CREATE INDEX [IX_PhieuKhoChiTiet_PhieuKhoId] ON [dbo].[PhieuKhoChiTiet]([PhieuKhoId]);
    CREATE INDEX [IX_PhieuKhoChiTiet_VatTuId] ON [dbo].[PhieuKhoChiTiet]([VatTuId]);
END;

-- Mã quyền
;WITH src AS (
    SELECT N'KHO_DanhMuc' AS MenuName, N'10.18.1 Danh muc vat tu, kho' AS Title UNION ALL
    SELECT N'KHO_Phieu',   N'10.18.2 Phieu nhap, xuat kho' UNION ALL
    SELECT N'KHO_BaoCao',  N'10.18.3-4 Bao cao nhap xuat ton, the kho'
)
INSERT INTO [MenuNames] (MenuID, MenuName, Title)
SELECT NEWID(), src.MenuName, src.Title
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [MenuNames] m WHERE m.MenuName = src.MenuName);

-- Cấp toàn quyền cho user phòng ADMIN (bỏ đoạn này nếu không muốn)
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, u.Usr, 1, 1, 1, 1, 1
FROM [UserList] u
CROSS JOIN [MenuNames] m
WHERE u.Department = N'ADMIN'
  AND u.Usr IS NOT NULL
  AND m.MenuName IN (N'KHO_DanhMuc', N'KHO_Phieu', N'KHO_BaoCao')
  AND NOT EXISTS (SELECT 1 FROM [Permissions] p WHERE p.UserName = u.Usr AND p.MenuName = m.MenuName);

-- Mẫu 1 kho mặc định (để lập phiếu ngay; đổi tên ở 10.18.1)
IF NOT EXISTS (SELECT 1 FROM [dbo].[KhoHang])
    INSERT INTO [dbo].[KhoHang] (Id, Code, Name, IsActive, CreatedAt, CreatedBy)
    VALUES (NEWID(), N'KHO01', N'Kho chính', 1, SYSDATETIME(), N'system');

COMMIT TRANSACTION;
GO

-- ═════════ 6. Lương NET / GROSS, tách thử việc (12.3, 12.9) ═════════
BEGIN TRANSACTION;

IF COL_LENGTH(N'dbo.HrContract', N'IsNetSalary') IS NULL
    ALTER TABLE [dbo].[HrContract] ADD [IsNetSalary] bit NOT NULL CONSTRAINT [DF_HrContract_IsNetSalary] DEFAULT 0;

IF COL_LENGTH(N'dbo.HrPayslip', N'IsNet') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [IsNet] bit NOT NULL CONSTRAINT [DF_HrPayslip_IsNet] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'GrossUp') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [GrossUp] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_GrossUp] DEFAULT 0;

IF COL_LENGTH(N'dbo.HrPayslip', N'ProContractId') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProContractId] uniqueidentifier NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProContractNo') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProContractNo] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProContractType') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProContractType] int NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProIsNet') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProIsNet] bit NOT NULL CONSTRAINT [DF_HrPayslip_ProIsNet] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProBaseSalary') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProBaseSalary] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProBaseSalary] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProAllowance') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProAllowance] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProAllowance] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProFrom') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProFrom] datetime2 NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProTo') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProTo] datetime2 NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProCalendarDays') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProCalendarDays] decimal(6,2) NOT NULL CONSTRAINT [DF_HrPayslip_ProCalendarDays] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProTimesheetPaidDays') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProTimesheetPaidDays] decimal(6,2) NOT NULL CONSTRAINT [DF_HrPayslip_ProTimesheetPaidDays] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProPaidDaysOverride') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProPaidDaysOverride] decimal(6,2) NULL;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProPaidDays') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProPaidDays] decimal(6,2) NOT NULL CONSTRAINT [DF_HrPayslip_ProPaidDays] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProSalaryByDays') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProSalaryByDays] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProSalaryByDays] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProAllowanceAmount') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProAllowanceAmount] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProAllowanceAmount] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProGrossUp') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProGrossUp] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProGrossUp] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProTax') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProTax] decimal(18,0) NOT NULL CONSTRAINT [DF_HrPayslip_ProTax] DEFAULT 0;
IF COL_LENGTH(N'dbo.HrPayslip', N'ProTaxMode') IS NULL
    ALTER TABLE [dbo].[HrPayslip] ADD [ProTaxMode] int NOT NULL CONSTRAINT [DF_HrPayslip_ProTaxMode] DEFAULT 0;

COMMIT TRANSACTION;
GO

-- Nhãn giao diện mới
BEGIN TRANSACTION;

-- Bổ sung: lương NET / GROSS, tháng có cả thử việc và chính thức (12.3, 12.9)
;WITH src AS (
    SELECT N'hr_net_salary' AS ResourceKey, N'en-US' AS Culture, N'Net salary (amounts are take-home)' AS Value UNION ALL
    SELECT N'hr_net_salary', N'vi-VN', N'Lương NET (lương cơ bản, phụ cấp là số thực nhận)' UNION ALL
    SELECT N'hr_net_salary', N'zh-CN', N'净工资（基本工资、津贴为实得金额）' UNION ALL
    SELECT N'hr_net_salary_hint' AS ResourceKey, N'en-US' AS Culture, N'Company pays employee insurance and PIT on top; payroll converts net to gross automatically.' AS Value UNION ALL
    SELECT N'hr_net_salary_hint', N'vi-VN', N'Công ty chịu BH phần người lao động và thuế TNCN; bảng lương tự quy đổi NET → GROSS.' UNION ALL
    SELECT N'hr_net_salary_hint', N'zh-CN', N'公司承担员工保险和个税；工资表自动由净额换算为税前。' UNION ALL
    SELECT N'hr_gross_salary_hint' AS ResourceKey, N'en-US' AS Culture, N'Gross salary: insurance and PIT are deducted from the amounts above.' AS Value UNION ALL
    SELECT N'hr_gross_salary_hint', N'vi-VN', N'Lương GROSS: BH và thuế TNCN trừ vào số tiền trên.' UNION ALL
    SELECT N'hr_gross_salary_hint', N'zh-CN', N'税前工资：保险和个税从上述金额中扣除。' UNION ALL
    SELECT N'hr_pay_salary_kind' AS ResourceKey, N'en-US' AS Culture, N'Net/Gross' AS Value UNION ALL
    SELECT N'hr_pay_salary_kind', N'vi-VN', N'NET/GROSS' UNION ALL
    SELECT N'hr_pay_salary_kind', N'zh-CN', N'净/税前' UNION ALL
    SELECT N'hr_pay_pro_short' AS ResourceKey, N'en-US' AS Culture, N'Probation / prev. contract' AS Value UNION ALL
    SELECT N'hr_pay_pro_short', N'vi-VN', N'Thử việc / HĐ trước' UNION ALL
    SELECT N'hr_pay_pro_short', N'zh-CN', N'试用/前合同' UNION ALL
    SELECT N'hr_pay_main_short' AS ResourceKey, N'en-US' AS Culture, N'Main contract' AS Value UNION ALL
    SELECT N'hr_pay_main_short', N'vi-VN', N'HĐ chính thức' UNION ALL
    SELECT N'hr_pay_main_short', N'zh-CN', N'正式合同' UNION ALL
    SELECT N'hr_pay_pro_contract' AS ResourceKey, N'en-US' AS Culture, N'Prev. contract (probation)' AS Value UNION ALL
    SELECT N'hr_pay_pro_contract', N'vi-VN', N'HĐ trước trong tháng (thử việc)' UNION ALL
    SELECT N'hr_pay_pro_contract', N'zh-CN', N'本月前一合同（试用）' UNION ALL
    SELECT N'hr_pay_pro_base_salary' AS ResourceKey, N'en-US' AS Culture, N'Probation salary' AS Value UNION ALL
    SELECT N'hr_pay_pro_base_salary', N'vi-VN', N'Lương thử việc' UNION ALL
    SELECT N'hr_pay_pro_base_salary', N'zh-CN', N'试用工资' UNION ALL
    SELECT N'hr_pay_pro_paid_days' AS ResourceKey, N'en-US' AS Culture, N'Probation paid days' AS Value UNION ALL
    SELECT N'hr_pay_pro_paid_days', N'vi-VN', N'Công thử việc' UNION ALL
    SELECT N'hr_pay_pro_paid_days', N'zh-CN', N'试用计薪天数' UNION ALL
    SELECT N'hr_pay_pro_salary_by_days' AS ResourceKey, N'en-US' AS Culture, N'Probation salary + allowance by days' AS Value UNION ALL
    SELECT N'hr_pay_pro_salary_by_days', N'vi-VN', N'Lương + phụ cấp thử việc theo công' UNION ALL
    SELECT N'hr_pay_pro_salary_by_days', N'zh-CN', N'试用期按天工资+津贴' UNION ALL
    SELECT N'hr_pay_pro_tax' AS ResourceKey, N'en-US' AS Culture, N'PIT on probation part' AS Value UNION ALL
    SELECT N'hr_pay_pro_tax', N'vi-VN', N'Thuế TNCN phần thử việc' UNION ALL
    SELECT N'hr_pay_pro_tax', N'zh-CN', N'试用部分个税' UNION ALL
    SELECT N'hr_pay_gross_up' AS ResourceKey, N'en-US' AS Culture, N'Gross-up (paid by company)' AS Value UNION ALL
    SELECT N'hr_pay_gross_up', N'vi-VN', N'Gross-up (công ty chịu thay BH, thuế)' UNION ALL
    SELECT N'hr_pay_gross_up', N'zh-CN', N'税费补贴（公司承担）' UNION ALL
    SELECT N'hr_pay_net_note' AS ResourceKey, N'en-US' AS Culture, N'NET salary: the employee receives the agreed amount; insurance and PIT shown are borne by the company (gross-up).' AS Value UNION ALL
    SELECT N'hr_pay_net_note', N'vi-VN', N'Lương NET: người lao động nhận đúng số thỏa thuận; BH và thuế ở trên do công ty chịu (dòng Gross-up).' UNION ALL
    SELECT N'hr_pay_net_note', N'zh-CN', N'净工资：员工按约定金额领取；上述保险和个税由公司承担（税费补贴）。' UNION ALL
    SELECT N'hr_pay_net_input_hint' AS ResourceKey, N'en-US' AS Culture, N'NET contract: overtime, bonus and other income entered here are take-home amounts; tax on them is also grossed up.' AS Value UNION ALL
    SELECT N'hr_pay_net_input_hint', N'vi-VN', N'HĐ lương NET: làm thêm, thưởng, thu nhập khác nhập ở đây là số thực nhận — thuế của các khoản này cũng được quy đổi (công ty chịu).' UNION ALL
    SELECT N'hr_pay_net_input_hint', N'zh-CN', N'净工资合同：此处加班、奖金、其他收入为实得金额，其税费也由公司承担。' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation' AS ResourceKey, N'en-US' AS Culture, N'Probation/unpaid days in the month reach the threshold — insurance starts next month' AS Value UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation', N'vi-VN', N'Số ngày thử việc + nghỉ không lương trong tháng từ 14 ngày — chưa đóng BH tháng này (đóng từ tháng sau)' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation', N'zh-CN', N'试用+无薪天数达14天——本月不缴保险，下月开始' UNION ALL
    SELECT N'hr_pay_warn_many_contracts' AS ResourceKey, N'en-US' AS Culture, N'More than 2 contracts in the month — only the latest 2 are used' AS Value UNION ALL
    SELECT N'hr_pay_warn_many_contracts', N'vi-VN', N'Trong tháng có hơn 2 hợp đồng — chỉ tính 2 hợp đồng gần nhất' UNION ALL
    SELECT N'hr_pay_warn_many_contracts', N'zh-CN', N'本月合同超过2份——仅计算最近2份' UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary' AS ResourceKey, N'en-US' AS Culture, N'NET contract without insurance salary — insurance is calculated on the net salary' AS Value UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary', N'vi-VN', N'HĐ lương NET chưa nhập lương đóng BH — BH đang tính trên lương NET (nên nhập lương đóng BH)' UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary', N'zh-CN', N'净工资合同未填写社保工资——保险按净工资计算'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
GO

PRINT N'Đã áp dụng xong thay đổi database (AttendanceLogs, HR_Attendance, HrSetting, nhãn giao diện, bảng kho 10.18, lương NET / thử việc).';
