-- Bảng lưu token QR D/O: một HBL + loại DO chỉ có một token, xuất lại thì dùng cùng QR.
-- Chạy script này 1 lần trên database (cùng DB với DefaultConnection).

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoQrToken]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoQrToken] (
        [Token] [nvarchar](449) NOT NULL,
        [HblId] [uniqueidentifier] NOT NULL,
        [Type] [nvarchar](10) NOT NULL,
        [ExpiresAt] [datetimeoffset](7) NOT NULL,
        CONSTRAINT [PK_DoQrToken] PRIMARY KEY CLUSTERED ([Token] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_DoQrToken_HblId_Type]
        ON [dbo].[DoQrToken] ([HblId] ASC, [Type] ASC);
END
GO
