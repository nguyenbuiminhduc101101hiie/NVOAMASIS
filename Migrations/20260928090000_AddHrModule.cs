using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Module 12 Quản lý nhân sự (giai đoạn 1) — tương đương Scripts/CreateHrTables.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260928090000_AddHrModule")]
    public partial class AddHrModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.HrPosition', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HrPosition](
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrPosition_Id] DEFAULT NEWID(),
                        [Code] nvarchar(30) NOT NULL,
                        [Name] nvarchar(200) NOT NULL,
                        [SortOrder] int NOT NULL CONSTRAINT [DF_HrPosition_SortOrder] DEFAULT 0,
                        [Description] nvarchar(1000) NULL,
                        [IsActive] bit NOT NULL CONSTRAINT [DF_HrPosition_IsActive] DEFAULT 1,
                        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrPosition_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy] nvarchar(100) NULL,
                        CONSTRAINT [PK_HrPosition] PRIMARY KEY CLUSTERED ([Id])
                    );
                    CREATE UNIQUE INDEX [IX_HrPosition_Code] ON [dbo].[HrPosition]([Code]);
                END;

                IF OBJECT_ID(N'dbo.HrEmployee', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HrEmployee](
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrEmployee_Id] DEFAULT NEWID(),
                        [EmployeeCode] nvarchar(30) NOT NULL,
                        [FullName] nvarchar(200) NOT NULL,
                        [Gender] int NOT NULL CONSTRAINT [DF_HrEmployee_Gender] DEFAULT 0,
                        [DateOfBirth] datetime2 NULL,
                        [PlaceOfBirth] nvarchar(200) NULL,
                        [IdCardNo] nvarchar(20) NULL,
                        [IdCardIssueDate] datetime2 NULL,
                        [IdCardIssuePlace] nvarchar(200) NULL,
                        [Phone] nvarchar(30) NULL,
                        [PersonalEmail] nvarchar(200) NULL,
                        [WorkEmail] nvarchar(200) NULL,
                        [PermanentAddress] nvarchar(500) NULL,
                        [CurrentAddress] nvarchar(500) NULL,
                        [UserId] uniqueidentifier NULL,
                        [DepartmentId] uniqueidentifier NULL,
                        [PositionId] uniqueidentifier NULL,
                        [Branch] nvarchar(10) NULL,
                        [CompanyCode] nvarchar(50) NULL,
                        [ManagerEmployeeId] uniqueidentifier NULL,
                        [JoinDate] datetime2 NULL,
                        [ProbationEndDate] datetime2 NULL,
                        [OfficialDate] datetime2 NULL,
                        [Status] int NOT NULL CONSTRAINT [DF_HrEmployee_Status] DEFAULT 1,
                        [ResignDate] datetime2 NULL,
                        [ResignReason] nvarchar(500) NULL,
                        [PersonalTaxCode] nvarchar(20) NULL,
                        [SocialInsuranceNo] nvarchar(20) NULL,
                        [BankAccountNo] nvarchar(30) NULL,
                        [BankName] nvarchar(200) NULL,
                        [BankBranch] nvarchar(200) NULL,
                        [DependentCount] int NOT NULL CONSTRAINT [DF_HrEmployee_DependentCount] DEFAULT 0,
                        [EmergencyContactName] nvarchar(200) NULL,
                        [EmergencyContactPhone] nvarchar(30) NULL,
                        [Note] nvarchar(2000) NULL,
                        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrEmployee_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy] nvarchar(100) NULL,
                        [UpdatedAt] datetime2 NULL,
                        [UpdatedBy] nvarchar(100) NULL,
                        CONSTRAINT [PK_HrEmployee] PRIMARY KEY CLUSTERED ([Id]),
                        CONSTRAINT [FK_HrEmployee_HrPosition] FOREIGN KEY ([PositionId])
                            REFERENCES [dbo].[HrPosition]([Id]),
                        CONSTRAINT [FK_HrEmployee_Manager] FOREIGN KEY ([ManagerEmployeeId])
                            REFERENCES [dbo].[HrEmployee]([Id])
                    );
                    CREATE UNIQUE INDEX [IX_HrEmployee_EmployeeCode] ON [dbo].[HrEmployee]([EmployeeCode]);
                    CREATE UNIQUE INDEX [IX_HrEmployee_UserId] ON [dbo].[HrEmployee]([UserId]) WHERE [UserId] IS NOT NULL;
                    CREATE INDEX [IX_HrEmployee_DepartmentId] ON [dbo].[HrEmployee]([DepartmentId]);
                    CREATE INDEX [IX_HrEmployee_ManagerEmployeeId] ON [dbo].[HrEmployee]([ManagerEmployeeId]);
                    CREATE INDEX [IX_HrEmployee_Status] ON [dbo].[HrEmployee]([Status]);
                END;

                IF OBJECT_ID(N'dbo.HrContract', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HrContract](
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrContract_Id] DEFAULT NEWID(),
                        [EmployeeId] uniqueidentifier NOT NULL,
                        [ContractNo] nvarchar(50) NOT NULL,
                        [ContractType] int NOT NULL,
                        [SignDate] datetime2 NULL,
                        [StartDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NULL,
                        [BaseSalary] decimal(18,0) NULL,
                        [InsuranceSalary] decimal(18,0) NULL,
                        [Allowance] decimal(18,0) NULL,
                        [Status] int NOT NULL CONSTRAINT [DF_HrContract_Status] DEFAULT 1,
                        [TerminatedDate] datetime2 NULL,
                        [Note] nvarchar(2000) NULL,
                        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrContract_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy] nvarchar(100) NULL,
                        [UpdatedAt] datetime2 NULL,
                        [UpdatedBy] nvarchar(100) NULL,
                        CONSTRAINT [PK_HrContract] PRIMARY KEY CLUSTERED ([Id]),
                        CONSTRAINT [FK_HrContract_HrEmployee] FOREIGN KEY ([EmployeeId])
                            REFERENCES [dbo].[HrEmployee]([Id])
                    );
                    CREATE UNIQUE INDEX [IX_HrContract_ContractNo] ON [dbo].[HrContract]([ContractNo]);
                    CREATE INDEX [IX_HrContract_EmployeeId_StartDate] ON [dbo].[HrContract]([EmployeeId], [StartDate]);
                    CREATE INDEX [IX_HrContract_EndDate] ON [dbo].[HrContract]([EndDate]);
                END;

                IF OBJECT_ID(N'dbo.HrEmployeeHistory', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HrEmployeeHistory](
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrEmployeeHistory_Id] DEFAULT NEWID(),
                        [EmployeeId] uniqueidentifier NOT NULL,
                        [ChangeType] nvarchar(50) NOT NULL,
                        [OldValue] nvarchar(500) NULL,
                        [NewValue] nvarchar(500) NULL,
                        [EffectiveDate] datetime2 NOT NULL,
                        [Note] nvarchar(1000) NULL,
                        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrEmployeeHistory_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy] nvarchar(100) NULL,
                        CONSTRAINT [PK_HrEmployeeHistory] PRIMARY KEY CLUSTERED ([Id]),
                        CONSTRAINT [FK_HrEmployeeHistory_HrEmployee] FOREIGN KEY ([EmployeeId])
                            REFERENCES [dbo].[HrEmployee]([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_HrEmployeeHistory_EmployeeId_CreatedAt] ON [dbo].[HrEmployeeHistory]([EmployeeId], [CreatedAt]);
                END;

                IF OBJECT_ID(N'dbo.HrDocument', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[HrDocument](
                        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrDocument_Id] DEFAULT NEWID(),
                        [EmployeeId] uniqueidentifier NOT NULL,
                        [DocType] nvarchar(50) NOT NULL,
                        [Title] nvarchar(200) NULL,
                        [FileName] nvarchar(260) NOT NULL,
                        [FilePath] nvarchar(500) NOT NULL,
                        [ContentType] nvarchar(150) NOT NULL,
                        [FileSize] bigint NOT NULL,
                        [UploadedAt] datetime2 NOT NULL CONSTRAINT [DF_HrDocument_UploadedAt] DEFAULT SYSDATETIME(),
                        [UploadedBy] nvarchar(100) NULL,
                        CONSTRAINT [PK_HrDocument] PRIMARY KEY CLUSTERED ([Id]),
                        CONSTRAINT [FK_HrDocument_HrEmployee] FOREIGN KEY ([EmployeeId])
                            REFERENCES [dbo].[HrEmployee]([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_HrDocument_EmployeeId] ON [dbo].[HrDocument]([EmployeeId]);
                END;

                -- Chức vụ mẫu (chỉ thêm khi bảng đang trống)
                IF NOT EXISTS (SELECT 1 FROM [dbo].[HrPosition])
                BEGIN
                    INSERT INTO [dbo].[HrPosition] ([Id], [Code], [Name], [SortOrder], [IsActive], [CreatedBy]) VALUES
                        (NEWID(), N'GD',   N'Giám đốc',            10, 1, N'system'),
                        (NEWID(), N'PGD',  N'Phó giám đốc',        20, 1, N'system'),
                        (NEWID(), N'TP',   N'Trưởng phòng',        30, 1, N'system'),
                        (NEWID(), N'PP',   N'Phó phòng',           40, 1, N'system'),
                        (NEWID(), N'TN',   N'Trưởng nhóm',         50, 1, N'system'),
                        (NEWID(), N'NV',   N'Nhân viên',           60, 1, N'system'),
                        (NEWID(), N'TTS',  N'Thực tập sinh',       70, 1, N'system');
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.HrDocument', N'U') IS NOT NULL DROP TABLE [dbo].[HrDocument];
                IF OBJECT_ID(N'dbo.HrEmployeeHistory', N'U') IS NOT NULL DROP TABLE [dbo].[HrEmployeeHistory];
                IF OBJECT_ID(N'dbo.HrContract', N'U') IS NOT NULL DROP TABLE [dbo].[HrContract];
                IF OBJECT_ID(N'dbo.HrEmployee', N'U') IS NOT NULL DROP TABLE [dbo].[HrEmployee];
                IF OBJECT_ID(N'dbo.HrPosition', N'U') IS NOT NULL DROP TABLE [dbo].[HrPosition];
                """);
        }
    }
}
