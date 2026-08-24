-- 8.3.7 SP-ITC Yard: thêm DepotId (Terminal.TerminalID = 47DEC189-7AED-4AF0-94AF-DAA2B1C14F9A).
-- Chạy 1 lần trên database ứng dụng. Cột Depot (tên) giữ nguyên.

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH(N'dbo.M_8_3_7_YARD_SP_ITC', N'DepotId') IS NULL
    ALTER TABLE [dbo].[M_8_3_7_YARD_SP_ITC] ADD [DepotId] UNIQUEIDENTIFIER NULL;
GO

UPDATE [dbo].[M_8_3_7_YARD_SP_ITC]
SET [DepotId] = '47DEC189-7AED-4AF0-94AF-DAA2B1C14F9A'
WHERE [DepotId] IS NULL
  AND (
        [Depot] IS NULL
        OR LTRIM(RTRIM([Depot])) = N''
        OR UPPER(LTRIM(RTRIM([Depot]))) = N'SP-ITC'
      );
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_M_8_3_7_YARD_SP_ITC_DepotId'
      AND object_id = OBJECT_ID(N'dbo.M_8_3_7_YARD_SP_ITC'))
    CREATE NONCLUSTERED INDEX [IX_M_8_3_7_YARD_SP_ITC_DepotId]
        ON [dbo].[M_8_3_7_YARD_SP_ITC] ([DepotId]);
GO
