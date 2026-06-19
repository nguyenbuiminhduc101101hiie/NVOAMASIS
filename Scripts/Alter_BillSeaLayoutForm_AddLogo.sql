IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'Logo') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD Logo VARBINARY(MAX) NULL;
END
GO
