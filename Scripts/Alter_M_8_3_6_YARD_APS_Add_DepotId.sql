-- 8.3.6 APS Stock / In-Out: thêm DepotId (Terminal.TerminalID) cho 5 bảng import.
-- Chạy 1 lần trên database ứng dụng. Cột DEPOT (tên) giữ nguyên.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ========== GATEIN ==========
IF COL_LENGTH(N'dbo.M_8_3_6_YARD_APS_GATEIN', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_6_YARD_APS_GATEIN] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[M_8_3_6_YARD_APS_GATEIN] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_6_YARD_APS_GATEIN_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_6_YARD_APS_GATEIN'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_6_YARD_APS_GATEIN_DepotId]
        ON [dbo].[M_8_3_6_YARD_APS_GATEIN] ([DepotId]);
GO

-- ========== GATEOUT ==========
IF COL_LENGTH(N'dbo.M_8_3_6_YARD_APS_GATEOUT', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_6_YARD_APS_GATEOUT] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[M_8_3_6_YARD_APS_GATEOUT] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_6_YARD_APS_GATEOUT_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_6_YARD_APS_GATEOUT'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_6_YARD_APS_GATEOUT_DepotId]
        ON [dbo].[M_8_3_6_YARD_APS_GATEOUT] ([DepotId]);
GO

-- ========== STOCK ==========
IF COL_LENGTH(N'dbo.M_8_3_6_YARD_APS_STOCK', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_6_YARD_APS_STOCK] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[M_8_3_6_YARD_APS_STOCK] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_6_YARD_APS_STOCK_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_6_YARD_APS_STOCK'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_6_YARD_APS_STOCK_DepotId]
        ON [dbo].[M_8_3_6_YARD_APS_STOCK] ([DepotId]);
GO

-- ========== GENERAL ==========
IF COL_LENGTH(N'dbo.M_8_3_6_YARD_APS_GENERAL', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_6_YARD_APS_GENERAL] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[M_8_3_6_YARD_APS_GENERAL] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_6_YARD_APS_GENERAL_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_6_YARD_APS_GENERAL'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_6_YARD_APS_GENERAL_DepotId]
        ON [dbo].[M_8_3_6_YARD_APS_GENERAL] ([DepotId]);
GO

-- ========== GENERAL_STATUS ==========
IF COL_LENGTH(N'dbo.M_8_3_6_YARD_APS_GENERAL_STATUS', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_6_YARD_APS_GENERAL_STATUS] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[M_8_3_6_YARD_APS_GENERAL_STATUS] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_6_YARD_APS_GENERAL_STATUS_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_6_YARD_APS_GENERAL_STATUS'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_6_YARD_APS_GENERAL_STATUS_DepotId]
        ON [dbo].[M_8_3_6_YARD_APS_GENERAL_STATUS] ([DepotId]);
GO
