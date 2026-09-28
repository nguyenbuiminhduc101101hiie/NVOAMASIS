/*
  Module 12 - Quản lý nhân sự, GIAI ĐOẠN 2: nghỉ phép theo quản lý trực tiếp, quỹ phép năm,
  ngày lễ, cấu hình nhân sự, bảng công.
  - Chạy SAU Scripts/CreateHrTables.sql (giai đoạn 1).
  - Chạy trên TỪNG tenant DB + DB template (MultiTenant:TemplateDatabase, mặc định "nvoamasis").
  - Safe to re-run. Tương đương migration 20260929090000_AddHrLeaveTimesheet.
  - QUAN TRỌNG: phải chạy script này TRƯỚC khi deploy code mới, vì model LeaveRequest có thêm
    cột AssignedApproverId — thiếu cột thì mọi màn hình nghỉ phép (cũ và mới) đều lỗi.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- 1) LeaveRequests: người được giao duyệt (quản lý trực tiếp)
IF COL_LENGTH(N'dbo.LeaveRequests', N'AssignedApproverId') IS NULL
BEGIN
    ALTER TABLE [dbo].[LeaveRequests] ADD [AssignedApproverId] uniqueidentifier NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LeaveRequests_AssignedApproverId'
               AND object_id = OBJECT_ID(N'dbo.LeaveRequests'))
BEGIN
    EXEC(N'CREATE INDEX [IX_LeaveRequests_AssignedApproverId] ON [dbo].[LeaveRequests]([AssignedApproverId]);');
END;

-- 2) Quỹ phép năm
IF OBJECT_ID(N'dbo.HrLeaveBalance', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrLeaveBalance](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrLeaveBalance_Id] DEFAULT NEWID(),
        [EmployeeId] uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [Entitled] decimal(6,2) NOT NULL CONSTRAINT [DF_HrLeaveBalance_Entitled] DEFAULT 0,
        [SeniorityBonus] decimal(6,2) NOT NULL CONSTRAINT [DF_HrLeaveBalance_SeniorityBonus] DEFAULT 0,
        [CarriedOver] decimal(6,2) NOT NULL CONSTRAINT [DF_HrLeaveBalance_CarriedOver] DEFAULT 0,
        [Adjustment] decimal(6,2) NOT NULL CONSTRAINT [DF_HrLeaveBalance_Adjustment] DEFAULT 0,
        [Note] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrLeaveBalance_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrLeaveBalance] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_HrLeaveBalance_HrEmployee] FOREIGN KEY ([EmployeeId])
            REFERENCES [dbo].[HrEmployee]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_HrLeaveBalance_EmployeeId_Year] ON [dbo].[HrLeaveBalance]([EmployeeId], [Year]);
END;

-- 3) Ngày lễ / nghỉ bù
IF OBJECT_ID(N'dbo.HrHoliday', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrHoliday](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrHoliday_Id] DEFAULT NEWID(),
        [Date] datetime2 NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrHoliday_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrHoliday] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE UNIQUE INDEX [IX_HrHoliday_Date] ON [dbo].[HrHoliday]([Date]);
END;

-- 4) Cấu hình nhân sự (key/value)
IF OBJECT_ID(N'dbo.HrSetting', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrSetting](
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(500) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrSetting] PRIMARY KEY CLUSTERED ([Key])
    );
END;

-- Giá trị mặc định (chỉ thêm khi chưa có). Sửa tại màn hình 12.8 Cài đặt nhân sự.
;WITH src AS (
    SELECT N'WorkDays' AS [Key], N'1,2,3,4,5' AS [Value] UNION ALL        -- T2..T6 (0=CN, 6=T7)
    SELECT N'SaturdayHalfDay', N'false' UNION ALL
    SELECT N'HoursPerDay', N'8' UNION ALL
    SELECT N'AnnualLeaveBaseDays', N'12' UNION ALL
    SELECT N'SeniorityStepYears', N'5' UNION ALL
    SELECT N'MaxCarryOverDays', N'0' UNION ALL
    SELECT N'AllowNegativeAnnualLeave', N'false' UNION ALL
    SELECT N'AttendanceIpSource', N'client'
)
INSERT INTO [dbo].[HrSetting] ([Key], [Value], [UpdatedAt], [UpdatedBy])
SELECT src.[Key], src.[Value], SYSDATETIME(), N'system'
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[HrSetting] s WHERE s.[Key] = src.[Key]);

COMMIT TRANSACTION;

PRINT N'HR phase 2 tables ready.';
