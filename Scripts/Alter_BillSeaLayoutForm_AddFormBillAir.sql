IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormBillAir') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormBillAir VARBINARY(MAX) NULL;
END
GO
