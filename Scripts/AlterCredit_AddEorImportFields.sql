SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Credit', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.Credit does not exist.', 1;
END;

IF COL_LENGTH(N'dbo.Credit', N'EstNo') IS NULL
    ALTER TABLE dbo.Credit ADD EstNo nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.Credit', N'Grade') IS NULL
    ALTER TABLE dbo.Credit ADD Grade nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.Credit', N'DamageDetail') IS NULL
    ALTER TABLE dbo.Credit ADD DamageDetail nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.Credit', N'Remark') IS NULL
    ALTER TABLE dbo.Credit ADD Remark nvarchar(max) NULL;

COMMIT TRANSACTION;
GO

SELECT
    name,
    TYPE_NAME(system_type_id) AS system_type,
    max_length,
    is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.Credit')
  AND name IN (
      N'EstNo',
      N'Grade',
      N'DamageDetail',
      N'Remark'
  )
ORDER BY name;
GO
