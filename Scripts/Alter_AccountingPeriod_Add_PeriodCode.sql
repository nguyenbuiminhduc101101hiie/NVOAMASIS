IF COL_LENGTH('dbo.accounting_period', 'period_code') IS NULL
BEGIN
    ALTER TABLE dbo.accounting_period
    ADD period_code INT IDENTITY(1,1) NOT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_accounting_period_period_code'
      AND object_id = OBJECT_ID('dbo.accounting_period')
)
BEGIN
    CREATE UNIQUE INDEX IX_accounting_period_period_code
        ON dbo.accounting_period(period_code);
END
GO
