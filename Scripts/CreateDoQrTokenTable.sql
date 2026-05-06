-- Bảng lưu token QR D/O: unique (HblId, Type, BillType, Branches).
-- DB cũ: chạy Scripts/AlterDoQrToken_AddCompanyBranch.sql

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoQrToken]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoQrToken] (
        [Token] [nvarchar](449) NOT NULL,
        [HblId] [uniqueidentifier] NOT NULL,
        [Type] [nvarchar](10) NOT NULL,
        [BillType] [nvarchar](50) NOT NULL CONSTRAINT [DF_DoQrToken_BillType] DEFAULT (N'PASL'),
        [Branches] [nvarchar](500) NOT NULL CONSTRAINT [DF_DoQrToken_Branches] DEFAULT (N''),
        [ExpiresAt] [datetimeoffset](7) NOT NULL,
        CONSTRAINT [PK_DoQrToken] PRIMARY KEY CLUSTERED ([Token] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_DoQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[DoQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);
END
GO
