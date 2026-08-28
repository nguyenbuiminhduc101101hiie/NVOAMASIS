-- 8.3.7 Yard Spltc Import
-- Drop old SP-ITC test table and recreate as 8_3_7_Yard_Spltc_Import.
-- Data is discarded on purpose.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.M_8_3_7_YARD_SP_ITC', N'U') IS NOT NULL
    DROP TABLE dbo.M_8_3_7_YARD_SP_ITC;
GO

IF OBJECT_ID(N'dbo.[8_3_7_Yard_Spltc_Import]', N'U') IS NOT NULL
    DROP TABLE dbo.[8_3_7_Yard_Spltc_Import];
GO

CREATE TABLE dbo.[8_3_7_Yard_Spltc_Import]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_8_3_7_Yard_Spltc_Import] PRIMARY KEY DEFAULT NEWID(),
    [ImportBatchNo] NVARCHAR(100) NULL,
    [ImportFileName] NVARCHAR(260) NULL,
    [SourceSheet] NVARCHAR(100) NULL,
    [ReportType] NVARCHAR(50) NULL,
    [Depot] NVARCHAR(150) NULL,
    [DepotId] UNIQUEIDENTIFIER NULL,
    [Line] NVARCHAR(50) NULL,
    [RowNo] INT NULL,
    [STT] INT NULL,
    [ContrNo] NVARCHAR(50) NULL,
    [ContainerType] NVARCHAR(50) NULL,
    [Size] NVARCHAR(20) NULL,
    [Category] NVARCHAR(50) NULL,
    [Movement] NVARCHAR(50) NULL,
    [Status] NVARCHAR(50) NULL,
    [FE] NVARCHAR(20) NULL,
    [IO] NVARCHAR(20) NULL,
    [DateIn] DATETIME2 NULL,
    [DateOut] DATETIME2 NULL,
    [DateInStuff] DATETIME2 NULL,
    [StuffingDate] DATETIME2 NULL,
    [UnStuffingDate] DATETIME2 NULL,
    [Days] DECIMAL(18,4) NULL,
    [Dwell] NVARCHAR(50) NULL,
    [ImpDays] DECIMAL(18,4) NULL,
    [GW] DECIMAL(18,4) NULL,
    [VGM] DECIMAL(18,4) NULL,
    [Booking] NVARCHAR(100) NULL,
    [BKBLNo] NVARCHAR(100) NULL,
    [Seal] NVARCHAR(100) NULL,
    [Vessel] NVARCHAR(200) NULL,
    [VesselVoyage] NVARCHAR(100) NULL,
    [VoyIn] NVARCHAR(50) NULL,
    [VoyOut] NVARCHAR(50) NULL,
    [PortCD] NVARCHAR(50) NULL,
    [POD] NVARCHAR(50) NULL,
    [FPOD] NVARCHAR(50) NULL,
    [Shipper] NVARCHAR(300) NULL,
    [Oper] NVARCHAR(100) NULL,
    [TruckBarge] NVARCHAR(100) NULL,
    [Truck] NVARCHAR(100) NULL,
    [SoXe] NVARCHAR(50) NULL,
    [Move] NVARCHAR(50) NULL,
    [Vent] NVARCHAR(50) NULL,
    [Location] NVARCHAR(200) NULL,
    [TTSC] NVARCHAR(100) NULL,
    [CustomsClear] NVARCHAR(100) NULL,
    [AVDM] NVARCHAR(100) NULL,
    [Remark] NVARCHAR(1000) NULL,
    [RowHash] NVARCHAR(64) NULL,
    [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_8_3_7_Yard_Spltc_Import_CreatedAt] DEFAULT SYSUTCDATETIME(),
    [CreatedBy] NVARCHAR(100) NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] NVARCHAR(100) NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_8_3_7_Yard_Spltc_Import_ContrNo]
    ON dbo.[8_3_7_Yard_Spltc_Import] ([ContrNo]);
GO

CREATE NONCLUSTERED INDEX [IX_8_3_7_Yard_Spltc_Import_DepotId]
    ON dbo.[8_3_7_Yard_Spltc_Import] ([DepotId]);
GO

CREATE NONCLUSTERED INDEX [IX_8_3_7_Yard_Spltc_Import_RowHash]
    ON dbo.[8_3_7_Yard_Spltc_Import] ([RowHash]);
GO
