-- Thêm BillType / Branches và đổi unique index: mỗi (HblId, Type, Company, Branch) có QR riêng.
-- Chạy trên DB đã có bảng ArrivalNoticeQrToken (bản cũ chỉ có Token, HblId, Type, ExpiresAt).
--
-- Lưu ý SQL Server: không được UPDATE cột vừa ADD trong cùng một batch — phải có GO giữa ADD và UPDATE.
--
-- BẮT BUỘC sau khi deploy code Arrival Notice QR (company/branch).
-- Thực hiện: SSMS -> chọn đúng database -> Execute toàn bộ file (F5).

-- ========== BillType ==========
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'BillType') IS NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ADD [BillType] [nvarchar](50) NULL;
GO

UPDATE [dbo].[ArrivalNoticeQrToken] SET [BillType] = N'PASL' WHERE [BillType] IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.ArrivalNoticeQrToken') AND c.name = N'BillType' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ALTER COLUMN [BillType] [nvarchar](50) NOT NULL;
GO

-- ========== Branches ==========
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'Branches') IS NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ADD [Branches] [nvarchar](500) NULL;
GO

UPDATE [dbo].[ArrivalNoticeQrToken] SET [Branches] = N'' WHERE [Branches] IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.ArrivalNoticeQrToken') AND c.name = N'Branches' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ALTER COLUMN [Branches] [nvarchar](500) NOT NULL;
GO

-- ========== Index ==========
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    DROP INDEX [IX_ArrivalNoticeQrToken_HblId_Type] ON [dbo].[ArrivalNoticeQrToken];
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[ArrivalNoticeQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);
GO
