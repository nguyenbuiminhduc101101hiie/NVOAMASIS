/*
  Template: CREATE dbo.account_balance like jnl (empty DB / new environment).
  Use Alter_account_balance_align_with_jnl.sql when table already exists on NVOAMASIS.
*/

USE [NVOAMASIS];
GO

IF OBJECT_ID(N'dbo.account_balance', N'U') IS NOT NULL
BEGIN
    RAISERROR(N'Table account_balance already exists. Use Alter_account_balance_align_with_jnl.sql instead.', 16, 1);
    RETURN;
END

CREATE TABLE [dbo].[account_balance](
    [id] [uniqueidentifier] NOT NULL,
    [account_code] [varchar](20) NOT NULL,
    [period_year] [int] NOT NULL,
    [period_month] [int] NOT NULL,
    [opening_balance] [decimal](18, 2) NOT NULL,
    [debit_total] [decimal](18, 2) NOT NULL,
    [credit_total] [decimal](18, 2) NOT NULL,
    [closing_balance] [decimal](18, 2) NOT NULL,
    [account_type] [varchar](20) NULL,
    [created_at] [datetime2](0) NOT NULL,
    [updated_at] [datetime2](0) NOT NULL,
    [company_id] [uniqueidentifier] NULL,
    [calculated_from] [datetime2](0) NULL,
    [calculated_to] [datetime2](0) NULL,
    [last_calculated_at] [datetime2](0) NULL,
    [book_code] [nvarchar](50) NULL,
    CONSTRAINT [PK_account_balance] PRIMARY KEY CLUSTERED ([id] ASC)
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[account_balance] ADD DEFAULT (NEWID()) FOR [id];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT ((0)) FOR [opening_balance];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT ((0)) FOR [debit_total];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT ((0)) FOR [credit_total];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT ((0)) FOR [closing_balance];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT (SYSUTCDATETIME()) FOR [created_at];
ALTER TABLE [dbo].[account_balance] ADD DEFAULT (SYSUTCDATETIME()) FOR [updated_at];
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_account_balance_account_period_book
    ON dbo.account_balance (account_code, period_year, period_month, book_code);
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_account_balance_company_account_period_book
    ON dbo.account_balance (company_id, account_code, period_year, period_month, book_code);
GO

IF OBJECT_ID(N'dbo.account_mapping', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.account_mapping', N'U')
          AND name = N'UQ_account_mapping_account_code'
    )
        CREATE UNIQUE NONCLUSTERED INDEX UQ_account_mapping_account_code
            ON dbo.account_mapping (account_code);

    ALTER TABLE dbo.account_balance WITH CHECK
        ADD CONSTRAINT FK_account_balance_account_mapping
        FOREIGN KEY (account_code) REFERENCES dbo.account_mapping (account_code);
END
GO
