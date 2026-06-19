IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'AttachMrtContent') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD AttachMrtContent VARBINARY(MAX) NULL;
END
GO
