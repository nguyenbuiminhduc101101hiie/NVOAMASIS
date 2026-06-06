IF COL_LENGTH('dbo.CompanyInfomation', 'CompanyCode') IS NOT NULL
BEGIN
    ALTER TABLE dbo.CompanyInfomation DROP COLUMN CompanyCode;
END
GO
