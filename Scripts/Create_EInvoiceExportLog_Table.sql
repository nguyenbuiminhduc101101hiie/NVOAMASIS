IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EInvoiceExportLog')
BEGIN
    CREATE TABLE EInvoiceExportLog (
        EInvoiceExportLogId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InvoiceId NVARCHAR(128) NULL,
        EInvoiceGuid NVARCHAR(128) NULL,
        LookupCode NVARCHAR(128) NULL,
        ViewUrl NVARCHAR(1000) NULL,
        SoHoaDonNoiBo NVARCHAR(128) NULL,
        HblId UNIQUEIDENTIFIER NULL,
        HblCode NVARCHAR(128) NULL,
        CustomerId UNIQUEIDENTIFIER NULL,
        CustomerName NVARCHAR(500) NULL,
        ExportType NVARCHAR(32) NULL,
        LineCount INT NOT NULL DEFAULT 0,
        CreatedBy NVARCHAR(128) NULL,
        CreatedAt DATETIME2 NOT NULL,
        Continued BIT NOT NULL DEFAULT 1,
        PdfFileName NVARCHAR(256) NULL,
        PdfFileContent NVARCHAR(MAX) NULL,
        XmlFileName NVARCHAR(256) NULL,
        XmlFileContent NVARCHAR(MAX) NULL,
        IsPublished BIT NOT NULL DEFAULT 0,
        PublishedAt DATETIME2 NULL,
        PublishedBy NVARCHAR(128) NULL
    );

    CREATE INDEX IX_EInvoiceExportLog_CreatedAt ON EInvoiceExportLog (CreatedAt DESC);
    CREATE INDEX IX_EInvoiceExportLog_HblId ON EInvoiceExportLog (HblId);
    CREATE INDEX IX_EInvoiceExportLog_InvoiceId ON EInvoiceExportLog (InvoiceId);
END
GO
