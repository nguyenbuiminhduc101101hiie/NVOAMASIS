IF COL_LENGTH('dbo.CompanyInfomation', 'BillSeaLayoutMrt') IS NULL
BEGIN
    ALTER TABLE dbo.CompanyInfomation ADD BillSeaLayoutMrt VARBINARY(MAX) NULL;
END
GO

IF COL_LENGTH('dbo.CompanyInfomation', 'BillSeaLayoutAttMrt') IS NULL
BEGIN
    ALTER TABLE dbo.CompanyInfomation ADD BillSeaLayoutAttMrt VARBINARY(MAX) NULL;
END
GO
