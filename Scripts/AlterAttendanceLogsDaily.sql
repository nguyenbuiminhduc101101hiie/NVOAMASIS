/*
  Module 12 - Chấm công hằng ngày (giờ vào / giờ ra, chấm bù, chấm công trên điện thoại).
  - Thêm cột vào AttendanceLogs (dữ liệu cũ giữ nguyên: PunchType = NULL = chấm theo buổi).
  - Thêm mã quyền HR_Attendance (12.10): See = xem công theo ngày mọi NV; Edit = chấm bù / xóa chấm bù.
  - Cấu hình (chế độ chấm, giờ làm, GPS văn phòng) lưu ở HrSetting, sửa tại 12.8 — không cần chèn sẵn.
  Tương đương migration 20261001090000_AddHrAttendanceDaily. Chạy trên từng tenant DB + DB template. Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

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

COMMIT TRANSACTION;

PRINT N'Attendance daily ready.';
