-- 4.8 Bao gia hang Air (Air Freight Rate)
-- One row per Airlines/Destination rate tier, imported from an .xlsx quotation sheet.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.AirFreightRate', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AirFreightRate
    (
        [Id]             UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_AirFreightRate] PRIMARY KEY DEFAULT NEWID(),
        [SourceFileName] NVARCHAR(255)     NULL,
        [Airlines]       NVARCHAR(255)     NULL,
        [Destination]    NVARCHAR(255)     NULL,
        [Min]            DECIMAL(18,4)     NULL,
        [RateUnder45]    DECIMAL(18,4)     NULL,
        [RateOver45]     DECIMAL(18,4)     NULL,
        [RateOver100]    DECIMAL(18,4)     NULL,
        [RateOver300]    DECIMAL(18,4)     NULL,
        [RateOver500]    DECIMAL(18,4)     NULL,
        [RateOver1000]   DECIMAL(18,4)     NULL,
        [FscWsc]         NVARCHAR(MAX)     NULL,
        [Frequency]      NVARCHAR(MAX)     NULL,
        [Route]          NVARCHAR(MAX)     NULL,
        [TransitTime]    NVARCHAR(MAX)     NULL,
        [Surcharges]     NVARCHAR(MAX)     NULL,
        [Note]           NVARCHAR(MAX)     NULL,
        [DateImport]     DATETIME2         NOT NULL CONSTRAINT [DF_AirFreightRate_DateImport] DEFAULT SYSUTCDATETIME(),
        [UserImport]     NVARCHAR(MAX)     NULL,
        [CreatedAt]      DATETIME2         NOT NULL CONSTRAINT [DF_AirFreightRate_CreatedAt] DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AirFreightRate_SourceFileName' AND object_id = OBJECT_ID(N'dbo.AirFreightRate'))
    CREATE NONCLUSTERED INDEX [IX_AirFreightRate_SourceFileName] ON dbo.AirFreightRate ([SourceFileName]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AirFreightRate_Airlines' AND object_id = OBJECT_ID(N'dbo.AirFreightRate'))
    CREATE NONCLUSTERED INDEX [IX_AirFreightRate_Airlines] ON dbo.AirFreightRate ([Airlines]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AirFreightRate_Destination' AND object_id = OBJECT_ID(N'dbo.AirFreightRate'))
    CREATE NONCLUSTERED INDEX [IX_AirFreightRate_Destination] ON dbo.AirFreightRate ([Destination]);
GO
