-- Bảng Stock (model M_Stock). Chạy 1 lần trên database ứng dụng (cùng DB với DefaultConnection).
-- Đổi USE [TênDatabase] nếu cần.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND type IN (N'U'))
BEGIN
    CREATE TABLE [dbo].[Stock] (
        [StockID]       [uniqueidentifier] NOT NULL,
        [Container]     [nvarchar](50)     NULL,
        [Type]          [nvarchar](50)     NULL,
        [IsoType]       [nvarchar](50)     NULL,
        [Opr]           [nvarchar](50)     NULL,
        [FE]            [nvarchar](20)     NULL,
        [Move]          [nvarchar](50)     NULL,
        [DateIn]        [datetime2](7)     NULL,
        [TimeIn]        [time](7)          NULL,
        [Consignee]     [nvarchar](500)    NULL,
        [Position]      [nvarchar](100)   NULL,
        [Days]          [int]              NULL,
        [Location]      [nvarchar](200)    NULL,
        [YOM]           [nvarchar](20)     NULL,
        [TareWeight]    [float]            NULL,
        [SealNo]        [nvarchar](100)   NULL,
        [Note2]         [nvarchar](500)    NULL,
        [Note3]         [nvarchar](500)    NULL,
        [VGM]           [float]            NULL,
        [MaxGross]      [float]            NULL,
        [Remark]        [nvarchar](2000)   NULL,
        [Status]        [nvarchar](100)   NULL,
        [Grade]         [nvarchar](100)   NULL,
        [CleanMethod]   [nvarchar](200)   NULL,
        [CleanStatus]   [nvarchar](200)   NULL,
        [PtiDate]       [datetime2](7)     NULL,
        [PtiSetting]    [nvarchar](200)   NULL,
        [PtiStatus]     [nvarchar](200)   NULL,
        [EstimatedDate] [datetime2](7)     NULL,
        [ApprovalDate]  [datetime2](7)     NULL,
        [RejectDate]    [datetime2](7)     NULL,
        [RepairedDate]  [datetime2](7)     NULL,
        [RegisteredDate] [datetime2](7)    NULL,
        [Shipper]       [nvarchar](500)    NULL,
        [Booking]       [nvarchar](200)    NULL,
        [DateImport]    [datetime2](7)     NULL,
        [UserImport]    [nvarchar](256)    NULL,
        CONSTRAINT [PK_Stock] PRIMARY KEY CLUSTERED ([StockID] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Stock_StockID' AND parent_object_id = OBJECT_ID(N'[dbo].[Stock]'))
BEGIN
    ALTER TABLE [dbo].[Stock] ADD CONSTRAINT [DF_Stock_StockID] DEFAULT (NEWID()) FOR [StockID];
END
GO

-- Bảng Stock đã tạo trước khi có cột import: thêm cột (chạy an toàn nhiều lần)
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND type IN (N'U'))
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND name = N'DateImport')
        ALTER TABLE [dbo].[Stock] ADD [DateImport] [datetime2](7) NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Stock]') AND name = N'UserImport')
        ALTER TABLE [dbo].[Stock] ADD [UserImport] [nvarchar](256) NULL;
END
GO
