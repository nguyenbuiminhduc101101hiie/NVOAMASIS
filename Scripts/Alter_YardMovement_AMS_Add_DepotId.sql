-- 8.3.4 Yard Movement AMS: thêm DepotId (Terminal.TerminalID) cho 2 bảng import.
-- Chạy 1 lần trên database ứng dụng. Cột DEPOT (tên) giữ nguyên.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ========== YardMovement_AMS_26040816_Current_In_Yard2 ==========
IF COL_LENGTH(N'dbo.YardMovement_AMS_26040816_Current_In_Yard2', N'DepotId') IS NULL
    ALTER TABLE [dbo].[YardMovement_AMS_26040816_Current_In_Yard2] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.YardMovement_AMS_26040816_Current_In_Yard2', N'DEPOT') IS NULL
    ALTER TABLE [dbo].[YardMovement_AMS_26040816_Current_In_Yard2] ADD [DEPOT] NVARCHAR(100) NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[YardMovement_AMS_26040816_Current_In_Yard2] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_YardMovement_AMS_Current_In_Yard2_DepotId'
      AND object_id = OBJECT_ID(N'dbo.YardMovement_AMS_26040816_Current_In_Yard2'))
    CREATE NONCLUSTERED INDEX [IX_YardMovement_AMS_Current_In_Yard2_DepotId]
        ON [dbo].[YardMovement_AMS_26040816_Current_In_Yard2] ([DepotId]);
GO

-- ========== YardMovement_AMS_26040816_Imp ==========
IF COL_LENGTH(N'dbo.YardMovement_AMS_26040816_Imp', N'DepotId') IS NULL
    ALTER TABLE [dbo].[YardMovement_AMS_26040816_Imp] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.YardMovement_AMS_26040816_Imp', N'DEPOT') IS NULL
    ALTER TABLE [dbo].[YardMovement_AMS_26040816_Imp] ADD [DEPOT] NVARCHAR(100) NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[YardMovement_AMS_26040816_Imp] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_YardMovement_AMS_Imp_DepotId'
      AND object_id = OBJECT_ID(N'dbo.YardMovement_AMS_26040816_Imp'))
    CREATE NONCLUSTERED INDEX [IX_YardMovement_AMS_Imp_DepotId]
        ON [dbo].[YardMovement_AMS_26040816_Imp] ([DepotId]);
GO
