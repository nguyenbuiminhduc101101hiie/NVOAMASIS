using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>12.3 / 12.9 — lương NET / GROSS và tách phần thử việc trong tháng — tương đương Scripts/AlterHrPayroll_NetGross_ThuViec.sql.</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261003090000_AddHrPayrollNetProbationSplit")]
    public partial class AddHrPayrollNetProbationSplit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
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
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
