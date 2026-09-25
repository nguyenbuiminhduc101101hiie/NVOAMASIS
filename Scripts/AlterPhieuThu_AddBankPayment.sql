SET XACT_ABORT ON;
GO

-- 1) Phieuthu: cờ đã thanh toán + ngày thanh toán
IF COL_LENGTH(N'dbo.Phieuthu', N'dathanhtoan') IS NULL
    ALTER TABLE [dbo].[Phieuthu]
    ADD [dathanhtoan] bit NULL
        CONSTRAINT [DF_Phieuthu_dathanhtoan] DEFAULT CAST(0 AS bit);
GO

IF COL_LENGTH(N'dbo.Phieuthu', N'NgayThanhToan') IS NULL
    ALTER TABLE [dbo].[Phieuthu] ADD [NgayThanhToan] datetime2 NULL;
GO

-- 2) BankTransaction: giao dịch tiền vào nhận từ webhook SePay
IF OBJECT_ID(N'dbo.BankTransaction', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[BankTransaction]
    (
        [Id]              uniqueidentifier NOT NULL CONSTRAINT [PK_BankTransaction] PRIMARY KEY,
        [Provider]        nvarchar(20)     NOT NULL,
        [ProviderTxnId]   nvarchar(50)     NOT NULL,
        [Gateway]         nvarchar(100)    NULL,
        [AccountNumber]   nvarchar(50)     NULL,
        [SubAccount]      nvarchar(50)     NULL,
        [TransactionDate] datetime2        NULL,
        [TransferType]    nvarchar(10)     NULL,
        [Amount]          decimal(18,2)    NOT NULL,
        [Content]         nvarchar(1000)   NULL,
        [ReferenceCode]   nvarchar(100)    NULL,
        [RawJson]         nvarchar(max)    NULL,
        [PhieuthuID]      uniqueidentifier NULL,
        [MatchStatus]     nvarchar(20)     NOT NULL,
        [ReceivedAt]      datetime2        NOT NULL
    );

    CREATE UNIQUE INDEX [UX_BankTransaction_Provider_TxnId]
        ON [dbo].[BankTransaction] ([Provider], [ProviderTxnId]);
    CREATE INDEX [IX_BankTransaction_PhieuthuID]
        ON [dbo].[BankTransaction] ([PhieuthuID]);
END;
GO
