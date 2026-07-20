-- Email approval flow for PhieuThu / PhieuChi
IF COL_LENGTH('dbo.Phieuthu', 'ApproveToken') IS NULL
    ALTER TABLE dbo.Phieuthu ADD ApproveToken NVARCHAR(64) NULL;
IF COL_LENGTH('dbo.Phieuthu', 'ApproveBy') IS NULL
    ALTER TABLE dbo.Phieuthu ADD ApproveBy NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.Phieuthu', 'ApproveDate') IS NULL
    ALTER TABLE dbo.Phieuthu ADD ApproveDate DATETIME2 NULL;
IF COL_LENGTH('dbo.Phieuthu', 'Remarks') IS NULL
    ALTER TABLE dbo.Phieuthu ADD Remarks NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.Phieuchi', 'ApproveToken') IS NULL
    ALTER TABLE dbo.Phieuchi ADD ApproveToken NVARCHAR(64) NULL;
IF COL_LENGTH('dbo.Phieuchi', 'ApproveBy') IS NULL
    ALTER TABLE dbo.Phieuchi ADD ApproveBy NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.Phieuchi', 'ApproveDate') IS NULL
    ALTER TABLE dbo.Phieuchi ADD ApproveDate DATETIME2 NULL;
IF COL_LENGTH('dbo.Phieuchi', 'Remarks') IS NULL
    ALTER TABLE dbo.Phieuchi ADD Remarks NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.UserList', 'Duyet_Phieu_Thu') IS NULL
    ALTER TABLE dbo.UserList ADD Duyet_Phieu_Thu BIT NULL;
IF COL_LENGTH('dbo.UserList', 'Duyet_Phieu_Chi') IS NULL
    ALTER TABLE dbo.UserList ADD Duyet_Phieu_Chi BIT NULL;

IF COL_LENGTH('dbo.CompanyInfomation', 'ListEmail_nhanTB_Approve_Thu_Chi') IS NULL
    ALTER TABLE dbo.CompanyInfomation ADD ListEmail_nhanTB_Approve_Thu_Chi NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.CompanyInfomation', 'Email_GuiTB') IS NULL
    ALTER TABLE dbo.CompanyInfomation ADD Email_GuiTB NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.CompanyInfomation', 'PasswordEmail_GuiTB') IS NULL
    ALTER TABLE dbo.CompanyInfomation ADD PasswordEmail_GuiTB NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.CompanyInfomation', 'SmtpServer') IS NULL
    ALTER TABLE dbo.CompanyInfomation ADD SmtpServer NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.CompanyInfomation', 'SmtpPort') IS NULL
    ALTER TABLE dbo.CompanyInfomation ADD SmtpPort INT NULL;

IF COL_LENGTH('dbo.Phieuthu', 'ApproveByUserId') IS NULL
    ALTER TABLE dbo.Phieuthu ADD ApproveByUserId UNIQUEIDENTIFIER NULL;
IF COL_LENGTH('dbo.Phieuchi', 'ApproveByUserId') IS NULL
    ALTER TABLE dbo.Phieuchi ADD ApproveByUserId UNIQUEIDENTIFIER NULL;
