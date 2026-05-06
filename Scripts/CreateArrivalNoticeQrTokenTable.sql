-- Bảng lưu token QR Arrival Notice: mỗi (HblId, Type, BillType, Branches) một token còn hạn; cùng lựa chọn thì tái dùng QR.
-- Chạy script này 1 lần trên database (cùng DB với DefaultConnection).

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ArrivalNoticeQrToken] (
        [Token] [nvarchar](449) NOT NULL,
        [HblId] [uniqueidentifier] NOT NULL,
        [Type] [nvarchar](10) NOT NULL,
        [BillType] [nvarchar](50) NOT NULL CONSTRAINT [DF_ArrivalNoticeQrToken_BillType] DEFAULT (N'PASL'),
        [Branches] [nvarchar](500) NOT NULL CONSTRAINT [DF_ArrivalNoticeQrToken_Branches] DEFAULT (N''),
        [ExpiresAt] [datetimeoffset](7) NOT NULL,
        CONSTRAINT [PK_ArrivalNoticeQrToken] PRIMARY KEY CLUSTERED ([Token] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[ArrivalNoticeQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);
END
GO

