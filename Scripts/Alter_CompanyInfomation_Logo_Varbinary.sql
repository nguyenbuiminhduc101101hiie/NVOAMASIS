-- Nếu đã chạy migration cũ với cột LogoUrl (nvarchar), chạy script này trước khi dùng Logo (varbinary).
IF COL_LENGTH('dbo.CompanyInfomation', 'LogoUrl') IS NOT NULL
BEGIN
    ALTER TABLE dbo.CompanyInfomation DROP COLUMN LogoUrl;
END
GO

IF COL_LENGTH('dbo.CompanyInfomation', 'Logo') IS NULL
BEGIN
    ALTER TABLE dbo.CompanyInfomation ADD Logo VARBINARY(MAX) NULL;
END
GO
