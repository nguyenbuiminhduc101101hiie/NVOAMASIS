-- Bảng lưu token QR Arrival Notice: một HBL + loại (Sea/Air) chỉ có một token, xuất lại thì dùng cùng QR.
-- Chạy script này 1 lần trên database (cùng DB với DefaultConnection).

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ArrivalNoticeQrToken] (
        [Token] [nvarchar](449) NOT NULL,
        [HblId] [uniqueidentifier] NOT NULL,
        [Type] [nvarchar](10) NOT NULL,
        [ExpiresAt] [datetimeoffset](7) NOT NULL,
        CONSTRAINT [PK_ArrivalNoticeQrToken] PRIMARY KEY CLUSTERED ([Token] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_ArrivalNoticeQrToken_HblId_Type]
        ON [dbo].[ArrivalNoticeQrToken] ([HblId] ASC, [Type] ASC);
END
GO

