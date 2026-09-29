using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Module 12 — chấm công hằng ngày (giờ vào / ra, chấm bù, mobile) — tương đương Scripts/AlterAttendanceLogsDaily.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261001090000_AddHrAttendanceDaily")]
    public partial class AddHrAttendanceDaily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AttendanceLogs_LocalDate_UserId' AND object_id = OBJECT_ID(N'dbo.AttendanceLogs'))
                    DROP INDEX [IX_AttendanceLogs_LocalDate_UserId] ON [dbo].[AttendanceLogs];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'IsManual') IS NOT NULL
                BEGIN
                    ALTER TABLE [dbo].[AttendanceLogs] DROP CONSTRAINT [DF_AttendanceLogs_IsManual];
                    ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [IsManual];
                END;
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'PunchType') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [PunchType];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'Latitude') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [Latitude];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'Longitude') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [Longitude];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'DistanceM') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [DistanceM];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'Note') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [Note];
                IF COL_LENGTH(N'dbo.AttendanceLogs', N'CreatedBy') IS NOT NULL ALTER TABLE [dbo].[AttendanceLogs] DROP COLUMN [CreatedBy];
                """);
        }
    }
}
