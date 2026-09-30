/*
  12.3 / 12.9 — Lương NET / GROSS và tháng có cả ngày thử việc + ngày chính thức.
  Chạy trên TỪNG tenant DB + DB template. Chạy lại nhiều lần vẫn an toàn.

  HrContract  + IsNetSalary  bit NOT NULL DEFAULT 0   (1 = lương NET: lương cơ bản + phụ cấp là số thực nhận)
  HrPayslip   + IsNet, GrossUp                         (phần công ty chịu thay khi HĐ chính là NET)
              + Pro* (15 cột)                          (phần hợp đồng trước trong cùng tháng — thường là thử việc)
  Sau khi chạy: vào 12.9 bấm "Tính lại" kỳ lương đang nháp.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

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

PRINT N'Đã thêm cột lương NET (HrContract.IsNetSalary) và tách phần thử việc (HrPayslip.Pro*). Tính lại kỳ lương nháp ở 12.9.';
