-- Thêm field XML hóa đơn điện tử cho Hóa đơn đầu vào (5.15)
-- Tương đương Migrations/20260928090000_AddXmlFieldsToHoaDonDauVao.cs
SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mausohoadon') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mausohoadon] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'kyhieuhoadon') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [kyhieuhoadon] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'loaihoadon') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [loaihoadon] nvarchar(255) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'hinhthucthanhtoan') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [hinhthucthanhtoan] nvarchar(255) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlDocId') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [xmlDocId] nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoiban') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tennguoiban] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoiban') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mstnguoiban] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoiban') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [diachinguoiban] nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tennguoimua') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tennguoimua] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'mstnguoimua') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [mstnguoimua] nvarchar(50) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'diachinguoimua') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [diachinguoimua] nvarchar(1000) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienthue') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tienthue] float NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'chietkhau') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [chietkhau] float NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tienbangchu') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tienbangchu] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'billno') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [billno] nvarchar(100) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'vesselvoyage') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [vesselvoyage] nvarchar(255) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pol') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [pol] nvarchar(255) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'pod') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [pod] nvarchar(255) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'ghichu') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [ghichu] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'nguonimport') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [nguonimport] nvarchar(20) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'tenfile') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [tenfile] nvarchar(500) NULL;
IF COL_LENGTH(N'dbo.HoaDonDauVao', N'xmlcontent') IS NULL
    ALTER TABLE [dbo].[HoaDonDauVao] ADD [xmlcontent] nvarchar(max) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDonDauVao_xmlDocId' AND object_id = OBJECT_ID(N'dbo.HoaDonDauVao'))
    EXEC(N'CREATE INDEX [IX_HoaDonDauVao_xmlDocId] ON [dbo].[HoaDonDauVao] ([xmlDocId]) WHERE [xmlDocId] IS NOT NULL');
GO
