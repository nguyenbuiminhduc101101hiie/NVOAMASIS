-- Thêm cột type cho bảng template (khớp NVOAMASIS.Models.DebitCreditTemplate.type)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.DebitCreditTemplate') AND name = N'type')
BEGIN
    ALTER TABLE dbo.DebitCreditTemplate ADD [type] nvarchar(max) NULL;
END
GO
