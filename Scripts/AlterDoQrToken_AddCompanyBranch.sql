-- Bảng DoQrToken: thêm BillType, Branches; unique (HblId, Type, BillType, Branches).
-- Không dùng UPDATE cùng batch với ADD cột — đã tách batch bằng GO.

-- ========== BillType ==========
IF COL_LENGTH(N'dbo.DoQrToken', N'BillType') IS NULL
    ALTER TABLE [dbo].[DoQrToken] ADD [BillType] [nvarchar](50) NULL;
GO

UPDATE [dbo].[DoQrToken] SET [BillType] = N'PASL' WHERE [BillType] IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.DoQrToken') AND c.name = N'BillType' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[DoQrToken] ALTER COLUMN [BillType] [nvarchar](50) NOT NULL;
GO

-- ========== Branches ==========
IF COL_LENGTH(N'dbo.DoQrToken', N'Branches') IS NULL
    ALTER TABLE [dbo].[DoQrToken] ADD [Branches] [nvarchar](500) NULL;
GO

UPDATE [dbo].[DoQrToken] SET [Branches] = N'' WHERE [Branches] IS NULL;
GO

IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.DoQrToken') AND c.name = N'Branches' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[DoQrToken] ALTER COLUMN [Branches] [nvarchar](500) NOT NULL;
GO

-- ========== Index ==========
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    DROP INDEX [IX_DoQrToken_HblId_Type] ON [dbo].[DoQrToken];
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_DoQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[DoQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);
GO
