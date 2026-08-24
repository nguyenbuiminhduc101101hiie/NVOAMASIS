-- 8.3.5 VSS In/Out Yard: thêm DepotId (Terminal.TerminalID) cho bảng import Exp.
-- Chạy 1 lần trên database ứng dụng. Cột DEPOT (tên) giữ nguyên.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.YardMovement_VSS_26040808_Exp', N'DepotId') IS NULL
    ALTER TABLE [dbo].[YardMovement_VSS_26040808_Exp] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.YardMovement_VSS_26040808_Exp', N'DEPOT') IS NULL
    ALTER TABLE [dbo].[YardMovement_VSS_26040808_Exp] ADD [DEPOT] NVARCHAR(100) NULL;
GO

UPDATE y
SET y.DepotId = t.TerminalID
FROM [dbo].[YardMovement_VSS_26040808_Exp] y
INNER JOIN [dbo].[Terminal] t
    ON LTRIM(RTRIM(y.DEPOT)) = LTRIM(RTRIM(t.TermiNalName))
WHERE y.DepotId IS NULL
  AND y.DEPOT IS NOT NULL
  AND LTRIM(RTRIM(y.DEPOT)) <> N'';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_YardMovement_VSS_Exp_DepotId'
      AND object_id = OBJECT_ID(N'dbo.YardMovement_VSS_26040808_Exp'))
    CREATE NONCLUSTERED INDEX [IX_YardMovement_VSS_Exp_DepotId]
        ON [dbo].[YardMovement_VSS_26040808_Exp] ([DepotId]);
GO
