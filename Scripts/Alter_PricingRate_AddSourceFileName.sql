/*
  Adds SourceFileName to the already-created dbo.PricingRate table, so imports can be
  managed/deleted by which .xlsx file they came from. Existing rows (imported before this
  column existed) will have SourceFileName = NULL - they are not tied to any file and can
  only be deleted individually from the grid, not via "manage imported files".
  Does not drop the table, no data loss. Safe to re-run.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PricingRate') AND name = 'SourceFileName')
BEGIN
    ALTER TABLE dbo.PricingRate ADD [SourceFileName] NVARCHAR(255) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PricingRate_SourceFileName' AND object_id = OBJECT_ID('dbo.PricingRate'))
BEGIN
    CREATE INDEX IX_PricingRate_SourceFileName ON dbo.PricingRate(SourceFileName);
END
GO

PRINT N'dbo.PricingRate.SourceFileName added.';
