SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.HoaDonDauRa', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.HoaDonDauRa does not exist.', 1;
END;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavPartnerInvoiceID') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavPartnerInvoiceID bigint NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavPartnerInvoiceStringID') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavPartnerInvoiceStringID nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavInvoiceGUID') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavInvoiceGUID nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavInvoiceNo') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavInvoiceNo int NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavInvoiceForm') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavInvoiceForm nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavInvoiceSerial') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavInvoiceSerial nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavInvoiceLink') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavInvoiceLink nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavPdfPath') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavPdfPath nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavXmlPath') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavXmlPath nvarchar(max) NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavStatusID') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavStatusID int NULL;

IF COL_LENGTH(N'dbo.HoaDonDauRa', N'BkavLastMessage') IS NULL
    ALTER TABLE dbo.HoaDonDauRa ADD BkavLastMessage nvarchar(max) NULL;

COMMIT TRANSACTION;
GO

SELECT
    name,
    TYPE_NAME(system_type_id) AS system_type,
    max_length,
    is_nullable
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.HoaDonDauRa')
  AND name IN (
      N'BkavPartnerInvoiceID',
      N'BkavPartnerInvoiceStringID',
      N'BkavInvoiceGUID',
      N'BkavInvoiceNo',
      N'BkavInvoiceForm',
      N'BkavInvoiceSerial',
      N'BkavInvoiceLink',
      N'BkavPdfPath',
      N'BkavXmlPath',
      N'BkavStatusID',
      N'BkavLastMessage'
  )
ORDER BY name;
GO
