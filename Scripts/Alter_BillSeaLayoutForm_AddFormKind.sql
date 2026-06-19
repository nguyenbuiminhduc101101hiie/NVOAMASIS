IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormKind') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormKind NVARCHAR(10) NOT NULL CONSTRAINT DF_BillSeaLayoutForm_FormKind DEFAULT (N'Sea');
END
GO
