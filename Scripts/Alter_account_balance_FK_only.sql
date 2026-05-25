/*
  Run this ONLY if Alter_account_balance_align_with_jnl.sql already:
    - dropped UQ_account_balance_account_period
    - created both UQ_*_book indexes
  but FK failed with Msg 1776 (account_mapping.account_code not unique/PK).
*/

USE [NVOAMASIS];
GO

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.account_mapping', N'U') IS NULL
BEGIN
    RAISERROR(N'dbo.account_mapping missing.', 16, 1);
    RETURN;
END

IF EXISTS (
    SELECT 1 FROM dbo.account_mapping GROUP BY account_code HAVING COUNT(*) > 1
)
BEGIN
    SELECT account_code, COUNT(*) AS cnt
    FROM dbo.account_mapping
    GROUP BY account_code
    HAVING COUNT(*) > 1;
    RAISERROR(N'Fix duplicate account_code in account_mapping first.', 16, 1);
    RETURN;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes AS i
    INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
    WHERE i.object_id = OBJECT_ID(N'dbo.account_mapping', N'U')
      AND i.is_unique = 1 AND i.type IN (1, 2) AND c.name = N'account_code'
      AND (SELECT COUNT(*) FROM sys.index_columns ic2
           WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 0) = 1
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_account_mapping_account_code
        ON dbo.account_mapping (account_code);
    PRINT N'Created UQ_account_mapping_account_code.';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_account_balance_account_mapping'
)
BEGIN
    ALTER TABLE dbo.account_balance WITH CHECK
        ADD CONSTRAINT FK_account_balance_account_mapping
        FOREIGN KEY (account_code) REFERENCES dbo.account_mapping (account_code);
    ALTER TABLE dbo.account_balance CHECK CONSTRAINT FK_account_balance_account_mapping;
    PRINT N'Created FK_account_balance_account_mapping.';
END
ELSE
    PRINT N'FK already exists.';

GO
