SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockGateOut]') AND type IN (N'U'))
BEGIN
    CREATE TABLE [dbo].[StockGateOut] (
        [StockGateOutID] [uniqueidentifier] NOT NULL,
        [Container]      [nvarchar](50)     NULL,
        [Booking]        [nvarchar](200)    NULL,
        [DateOut]        [datetime2](7)     NULL,
        [TotalDays]      [int]              NULL,
        [DateImport]     [datetime2](7)     NULL,
        [UserImport]     [nvarchar](256)    NULL,
        CONSTRAINT [PK_StockGateOut] PRIMARY KEY CLUSTERED ([StockGateOutID] ASC)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_StockGateOut_StockGateOutID' AND parent_object_id = OBJECT_ID(N'[dbo].[StockGateOut]'))
BEGIN
    ALTER TABLE [dbo].[StockGateOut] ADD CONSTRAINT [DF_StockGateOut_StockGateOutID] DEFAULT (NEWID()) FOR [StockGateOutID];
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_StockGateOut_Container' AND object_id = OBJECT_ID(N'[dbo].[StockGateOut]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_StockGateOut_Container]
        ON [dbo].[StockGateOut]([Container] ASC)
        WHERE [Container] IS NOT NULL;
END
GO