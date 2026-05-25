/*
  NVOAMASIS: align dbo.account_balance with jnl (correct) schema.

  WRONG (current NVOAMASIS CREATE):
    UNIQUE UQ_account_balance_account_period (account_code, period_year, period_month)  -- no book_code

  CORRECT (jnl + indexes from SSMS):
    - No 3-column unique on CREATE TABLE
    - FK_account_balance_account_mapping -> account_mapping(account_code)
    - DEFAULT newid() on id
    - UNIQUE UQ_account_balance_account_period_book (account_code, period_year, period_month, book_code)
    - UNIQUE UQ_account_balance_company_account_period_book (company_id, account_code, period_year, period_month, book_code)

  Run on database NVOAMASIS. Review orphan account_code rows before FK step.
*/

USE [NVOAMASIS];
GO

SET NOCOUNT ON;

-- ========== 0) Orphan account_code (must fix before FK) ==========
PRINT N'--- account_balance rows without account_mapping ---';
SELECT DISTINCT ab.account_code
FROM dbo.account_balance AS ab
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.account_mapping AS m WHERE m.account_code = ab.account_code
);

-- Optional: auto-create minimal mapping for orphans (uncomment if acceptable)
/*
INSERT INTO dbo.account_mapping (account_code, account_name, statement_type, normal_balance, is_active, display_order)
SELECT DISTINCT
    ab.account_code,
    ab.account_code,
    N'BalanceSheet',
    N'Debit',
    1,
    0
FROM dbo.account_balance AS ab
WHERE NOT EXISTS (SELECT 1 FROM dbo.account_mapping AS m WHERE m.account_code = ab.account_code);
*/

IF EXISTS (
    SELECT 1
    FROM dbo.account_balance AS ab
    WHERE NOT EXISTS (SELECT 1 FROM dbo.account_mapping AS m WHERE m.account_code = ab.account_code)
)
BEGIN
    RAISERROR(N'Stop: account_balance has account_code not in account_mapping. Fix orphans then re-run.', 16, 1);
    RETURN;
END

-- ========== 1) book_code ==========
IF COL_LENGTH(N'dbo.account_balance', N'book_code') IS NULL
    ALTER TABLE dbo.account_balance ADD book_code NVARCHAR(50) NULL;

UPDATE dbo.account_balance
SET book_code = N'TAX'
WHERE book_code IS NULL OR LTRIM(RTRIM(book_code)) = N'';

-- ========== 2) Drop WRONG unique (3 columns) ==========
IF EXISTS (
    SELECT 1 FROM sys.key_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.account_balance', N'U')
      AND name = N'UQ_account_balance_account_period'
)
BEGIN
    ALTER TABLE dbo.account_balance DROP CONSTRAINT UQ_account_balance_account_period;
    PRINT N'Dropped UQ_account_balance_account_period.';
END
ELSE IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.account_balance', N'U')
      AND name = N'UQ_account_balance_account_period'
)
BEGIN
    DROP INDEX UQ_account_balance_account_period ON dbo.account_balance;
    PRINT N'Dropped index UQ_account_balance_account_period.';
END

-- ========== 3) DEFAULT newid() on id (jnl) ==========
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.account_balance', N'U')
      AND c.name = N'id'
)
    ALTER TABLE dbo.account_balance ADD DEFAULT (NEWID()) FOR [id];

-- ========== 4) Duplicate check before new uniques ==========
IF EXISTS (
    SELECT 1 FROM dbo.account_balance
    GROUP BY account_code, period_year, period_month, book_code
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(N'Duplicate (account_code, period_year, period_month, book_code). Merge/delete extras.', 16, 1);
    RETURN;
END

IF EXISTS (
    SELECT 1 FROM dbo.account_balance
    GROUP BY company_id, account_code, period_year, period_month, book_code
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(N'Duplicate (company_id, account_code, period_year, period_month, book_code).', 16, 1);
    RETURN;
END

-- ========== 5) CORRECT unique indexes ==========
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.account_balance', N'U')
      AND name = N'UQ_account_balance_account_period_book'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_account_balance_account_period_book
        ON dbo.account_balance (account_code, period_year, period_month, book_code);
    PRINT N'Created UQ_account_balance_account_period_book.';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.account_balance', N'U')
      AND name = N'UQ_account_balance_company_account_period_book'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_account_balance_company_account_period_book
        ON dbo.account_balance (company_id, account_code, period_year, period_month, book_code);
    PRINT N'Created UQ_account_balance_company_account_period_book.';
END

-- ========== 6) account_mapping.account_code must be UNIQUE/PK (jnl: PK on account_code) ==========
IF OBJECT_ID(N'dbo.account_mapping', N'U') IS NULL
BEGIN
    PRINT N'Skip FK: dbo.account_mapping does not exist.';
END
ELSE
BEGIN
    IF EXISTS (
        SELECT 1
        FROM dbo.account_mapping
        GROUP BY account_code
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR(N'dbo.account_mapping has duplicate account_code. Resolve before FK.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes AS i
        INNER JOIN sys.index_columns AS ic
            ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        INNER JOIN sys.columns AS c
            ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE i.object_id = OBJECT_ID(N'dbo.account_mapping', N'U')
          AND i.is_unique = 1
          AND i.type IN (1, 2)
          AND c.name = N'account_code'
          AND (
              SELECT COUNT(*)
              FROM sys.index_columns AS ic2
              WHERE ic2.object_id = i.object_id AND ic2.index_id = i.index_id AND ic2.is_included_column = 0
          ) = 1
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UQ_account_mapping_account_code
            ON dbo.account_mapping (account_code);
        PRINT N'Created UQ_account_mapping_account_code (required for FK).';
    END

    -- ========== 7) FK to account_mapping (jnl) ==========
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_account_balance_account_mapping'
          AND parent_object_id = OBJECT_ID(N'dbo.account_balance', N'U')
    )
    BEGIN
        ALTER TABLE dbo.account_balance WITH CHECK
            ADD CONSTRAINT FK_account_balance_account_mapping
            FOREIGN KEY (account_code) REFERENCES dbo.account_mapping (account_code);
        ALTER TABLE dbo.account_balance CHECK CONSTRAINT FK_account_balance_account_mapping;
        PRINT N'Created FK_account_balance_account_mapping.';
    END
END

-- ========== 8) Verify ==========
PRINT N'--- Unique indexes on account_balance ---';
SELECT i.name AS index_name, c.name AS column_name, ic.key_ordinal
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID(N'dbo.account_balance', N'U')
  AND i.is_unique = 1
  AND i.type > 0
ORDER BY i.name, ic.key_ordinal;

PRINT N'Done.';
GO
