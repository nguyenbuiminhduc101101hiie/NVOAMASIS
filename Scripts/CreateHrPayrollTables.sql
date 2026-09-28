/*
  Module 12 - Quản lý nhân sự, GIAI ĐOẠN 3: bảng lương.
  - Chạy SAU CreateHrTables.sql và CreateHrLeaveTimesheetTables.sql.
  - Chạy trên TỪNG tenant DB + DB template (MultiTenant:TemplateDatabase, mặc định "nvoamasis").
  - Safe to re-run. Tương đương migration 20260930090000_AddHrPayroll.

  Tham số pháp lý mặc định (kiểm tra lại với văn bản hiện hành, sửa tại 12.9 > Tham số & tài khoản):
   - Lương tối thiểu vùng từ 01/01/2026 (NĐ 293/2025/NĐ-CP): I 5.310.000; II 4.730.000; III 4.140.000; IV 3.700.000.
   - Mức tham chiếu (= lương cơ sở): 2.340.000 đến 30/6/2026; 2.530.000 từ 01/7/2026.
     Trần BHXH/BHYT = 20 × mức tham chiếu; trần BHTN = 20 × lương tối thiểu vùng.
   - Tỷ lệ: NLĐ 8% BHXH, 1,5% BHYT, 1% BHTN; DN 17,5% BHXH (gồm 0,5% TNLĐ-BNN), 3% BHYT, 1% BHTN, 2% KPCĐ.
   - Thuế TNCN kỳ tính thuế 2026 (Luật Thuế TNCN 2025): biểu 5 bậc 5/10/20/30/35% với ngưỡng
     10/30/60/100 triệu/tháng; giảm trừ bản thân 15.500.000, người phụ thuộc 6.200.000;
     khấu trừ 10% khi chi trả từ 2.000.000 cho HĐ dưới 3 tháng / không HĐLĐ.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.HrPayrollParam', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrPayrollParam](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrPayrollParam_Id] DEFAULT NEWID(),
        [EffectiveFrom] datetime2 NOT NULL,
        [ReferenceWage] decimal(18,0) NOT NULL,
        [MinWageRegion1] decimal(18,0) NOT NULL,
        [MinWageRegion2] decimal(18,0) NOT NULL,
        [MinWageRegion3] decimal(18,0) NOT NULL,
        [MinWageRegion4] decimal(18,0) NOT NULL,
        [CapMultiplier] decimal(6,2) NOT NULL,
        [EmpSocialRate] decimal(6,2) NOT NULL,
        [EmpHealthRate] decimal(6,2) NOT NULL,
        [EmpUnemploymentRate] decimal(6,2) NOT NULL,
        [CoSocialRate] decimal(6,2) NOT NULL,
        [CoHealthRate] decimal(6,2) NOT NULL,
        [CoUnemploymentRate] decimal(6,2) NOT NULL,
        [CoUnionFeeRate] decimal(6,2) NOT NULL,
        [SelfDeduction] decimal(18,0) NOT NULL,
        [DependentDeduction] decimal(18,0) NOT NULL,
        [TaxBrackets] nvarchar(500) NOT NULL,
        [FlatTaxRate] decimal(6,2) NOT NULL,
        [FlatTaxThreshold] decimal(18,0) NOT NULL,
        [Note] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrPayrollParam_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrPayrollParam] PRIMARY KEY CLUSTERED ([Id])
    );
    CREATE UNIQUE INDEX [IX_HrPayrollParam_EffectiveFrom] ON [dbo].[HrPayrollParam]([EffectiveFrom]);
END;

IF OBJECT_ID(N'dbo.HrPayrollPeriod', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrPayrollPeriod](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrPayrollPeriod_Id] DEFAULT NEWID(),
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Status] int NOT NULL CONSTRAINT [DF_HrPayrollPeriod_Status] DEFAULT 1,
        [StandardDays] decimal(6,2) NOT NULL CONSTRAINT [DF_HrPayrollPeriod_StandardDays] DEFAULT 0,
        [ParamId] uniqueidentifier NULL,
        [VoucherId] uniqueidentifier NULL,
        [VoucherNo] nvarchar(50) NULL,
        [Note] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_HrPayrollPeriod_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100) NULL,
        [CalculatedAt] datetime2 NULL,
        [CalculatedBy] nvarchar(100) NULL,
        [LockedAt] datetime2 NULL,
        [LockedBy] nvarchar(100) NULL,
        [PostedAt] datetime2 NULL,
        [PostedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrPayrollPeriod] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_HrPayrollPeriod_HrPayrollParam] FOREIGN KEY ([ParamId]) REFERENCES [dbo].[HrPayrollParam]([Id])
    );
    CREATE UNIQUE INDEX [IX_HrPayrollPeriod_Year_Month] ON [dbo].[HrPayrollPeriod]([Year], [Month]);
END;

IF OBJECT_ID(N'dbo.HrPayslip', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[HrPayslip](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [DF_HrPayslip_Id] DEFAULT NEWID(),
        [PeriodId] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [EmployeeCode] nvarchar(30) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [DepartmentId] uniqueidentifier NULL,
        [DepartmentName] nvarchar(200) NULL,
        [Branch] nvarchar(10) NULL,
        [UserId] uniqueidentifier NULL,
        [ContractId] uniqueidentifier NULL,
        [ContractNo] nvarchar(50) NULL,
        [ContractType] int NULL,
        [BaseSalary] decimal(18,0) NOT NULL,
        [InsuranceSalary] decimal(18,0) NULL,
        [Allowance] decimal(18,0) NOT NULL,
        [StandardDays] decimal(6,2) NOT NULL,
        [TimesheetPaidDays] decimal(6,2) NOT NULL,
        [PaidDaysOverride] decimal(6,2) NULL,
        [PaidDays] decimal(6,2) NOT NULL,
        [NonPaidDays] decimal(6,2) NOT NULL,
        [SalaryByDays] decimal(18,0) NOT NULL,
        [AllowanceAmount] decimal(18,0) NOT NULL,
        [Overtime] decimal(18,0) NOT NULL,
        [Bonus] decimal(18,0) NOT NULL,
        [OtherIncome] decimal(18,0) NOT NULL,
        [NonTaxableIncome] decimal(18,0) NOT NULL,
        [GrossIncome] decimal(18,0) NOT NULL,
        [InsuranceBase] decimal(18,0) NOT NULL,
        [EmpSocial] decimal(18,0) NOT NULL,
        [EmpHealth] decimal(18,0) NOT NULL,
        [EmpUnemployment] decimal(18,0) NOT NULL,
        [CoSocial] decimal(18,0) NOT NULL,
        [CoHealth] decimal(18,0) NOT NULL,
        [CoUnemployment] decimal(18,0) NOT NULL,
        [CoUnionFee] decimal(18,0) NOT NULL,
        [Dependents] int NOT NULL,
        [FamilyDeduction] decimal(18,0) NOT NULL,
        [TaxMode] int NOT NULL,
        [TaxModeManual] bit NOT NULL CONSTRAINT [DF_HrPayslip_TaxModeManual] DEFAULT 0,
        [TaxableIncome] decimal(18,0) NOT NULL,
        [AssessableIncome] decimal(18,0) NOT NULL,
        [PersonalIncomeTax] decimal(18,0) NOT NULL,
        [Advance] decimal(18,0) NOT NULL,
        [OtherDeduction] decimal(18,0) NOT NULL,
        [NetPay] decimal(18,0) NOT NULL,
        [BankAccountNo] nvarchar(30) NULL,
        [BankName] nvarchar(200) NULL,
        [Warnings] nvarchar(1000) NULL,
        [Note] nvarchar(1000) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(100) NULL,
        CONSTRAINT [PK_HrPayslip] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_HrPayslip_HrPayrollPeriod] FOREIGN KEY ([PeriodId])
            REFERENCES [dbo].[HrPayrollPeriod]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_HrPayslip_PeriodId_EmployeeId] ON [dbo].[HrPayslip]([PeriodId], [EmployeeId]);
    CREATE INDEX [IX_HrPayslip_UserId] ON [dbo].[HrPayslip]([UserId]);
END;

-- Tham số pháp lý mặc định năm 2026 (chỉ thêm khi chưa có dòng cùng ngày hiệu lực)
;WITH src AS (
    SELECT CAST('2026-01-01' AS datetime2) AS EffectiveFrom, CAST(2340000 AS decimal(18,0)) AS ReferenceWage,
           N'Lương tối thiểu vùng theo NĐ 293/2025; mức tham chiếu 2.340.000 (đến 30/6/2026); thuế TNCN 5 bậc, giảm trừ 15,5tr/6,2tr' AS Note
    UNION ALL
    SELECT CAST('2026-07-01' AS datetime2), CAST(2530000 AS decimal(18,0)),
           N'Mức tham chiếu 2.530.000 từ 01/7/2026 (trần BHXH/BHYT 50.600.000)'
)
INSERT INTO [dbo].[HrPayrollParam] ([Id], [EffectiveFrom], [ReferenceWage], [MinWageRegion1], [MinWageRegion2], [MinWageRegion3], [MinWageRegion4],
    [CapMultiplier], [EmpSocialRate], [EmpHealthRate], [EmpUnemploymentRate], [CoSocialRate], [CoHealthRate], [CoUnemploymentRate], [CoUnionFeeRate],
    [SelfDeduction], [DependentDeduction], [TaxBrackets], [FlatTaxRate], [FlatTaxThreshold], [Note], [CreatedBy])
SELECT NEWID(), src.EffectiveFrom, src.ReferenceWage, 5310000, 4730000, 4140000, 3700000,
    20, 8, 1.5, 1, 17.5, 3, 1, 2,
    15500000, 6200000, N'10000000:5;30000000:10;60000000:20;100000000:30;0:35', 10, 2000000, src.Note, N'system'
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[HrPayrollParam] p WHERE p.EffectiveFrom = src.EffectiveFrom);

-- Cấu hình tính lương / tài khoản hạch toán (chỉ thêm khi chưa có)
IF OBJECT_ID(N'dbo.HrSetting', N'U') IS NOT NULL
BEGIN
    ;WITH s AS (
        SELECT N'PayrollRegion' AS [Key], N'1' AS [Value] UNION ALL
        SELECT N'PayrollUnionFeeEnabled', N'true' UNION ALL
        SELECT N'PayrollNoInsuranceUnpaidDays', N'14' UNION ALL
        SELECT N'PayrollTransactionTypeCode', N'' UNION ALL
        SELECT N'PayrollAccExpense', N'642' UNION ALL
        SELECT N'PayrollAccPayable', N'334' UNION ALL
        SELECT N'PayrollAccSocial', N'3383' UNION ALL
        SELECT N'PayrollAccHealth', N'3384' UNION ALL
        SELECT N'PayrollAccUnemployment', N'3386' UNION ALL
        SELECT N'PayrollAccUnionFee', N'3382' UNION ALL
        SELECT N'PayrollAccPit', N'3335' UNION ALL
        SELECT N'PayrollAccAdvance', N'141' UNION ALL
        SELECT N'PayrollAccOtherDeduction', N'1388'
    )
    INSERT INTO [dbo].[HrSetting] ([Key], [Value], [UpdatedAt], [UpdatedBy])
    SELECT s.[Key], s.[Value], SYSDATETIME(), N'system'
    FROM s
    WHERE NOT EXISTS (SELECT 1 FROM [dbo].[HrSetting] x WHERE x.[Key] = s.[Key]);
END;

COMMIT TRANSACTION;

PRINT N'HR phase 3 (payroll) tables ready.';
