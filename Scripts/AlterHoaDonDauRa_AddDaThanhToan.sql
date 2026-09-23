SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.HoaDonDauRa', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.HoaDonDauRa does not exist.', 1;
END;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'dathanhtoan') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD dathanhtoan bit NULL CONSTRAINT DF_HoaDonDauRa_dathanhtoan DEFAULT (0);

COMMIT TRANSACTION;
GO

SELECT
    name,
    TYPE_NAME(system_type_id) AS system_type,
    max_length,
    is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.HoaDonDauRa')
  AND name = N'dathanhtoan';
GO
