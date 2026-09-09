-- Cấu hình eHoadon cho bảng CompanyInfomation (5.13 E-Invoice)
IF COL_LENGTH('CompanyInfomation', 'Email_E_invoice') IS NULL
    ALTER TABLE CompanyInfomation ADD Email_E_invoice NVARCHAR(MAX) NULL;

IF COL_LENGTH('CompanyInfomation', 'Password_E_invoice') IS NULL
    ALTER TABLE CompanyInfomation ADD Password_E_invoice NVARCHAR(MAX) NULL;

IF COL_LENGTH('CompanyInfomation', 'TenantID') IS NULL
    ALTER TABLE CompanyInfomation ADD TenantID NVARCHAR(MAX) NULL;

IF COL_LENGTH('CompanyInfomation', 'TaxNumber_E_Invoice') IS NULL
    ALTER TABLE CompanyInfomation ADD TaxNumber_E_Invoice NVARCHAR(MAX) NULL;

IF COL_LENGTH('CompanyInfomation', 'Serial_E_Invoice') IS NULL
    ALTER TABLE CompanyInfomation ADD Serial_E_Invoice NVARCHAR(MAX) NULL;
GO
