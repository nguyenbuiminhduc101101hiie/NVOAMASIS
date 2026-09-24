IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceID') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPartnerInvoiceID] bigint NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceStringID') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPartnerInvoiceStringID] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceNo') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceNo] int NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceForm') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceForm] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceSerial') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceSerial] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceLink') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceLink] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPdfPath') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPdfPath] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavXmlPath') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavXmlPath] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavStatusID') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavStatusID] int NULL;
IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavLastMessage') IS NULL
    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavLastMessage] nvarchar(max) NULL;
GO
