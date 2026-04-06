/*
  Run once if table dbo.SI was created with a [Customer] column and you no longer use it.
*/

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'SI' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND c.name = N'Customer'
)
BEGIN
    ALTER TABLE dbo.[SI] DROP COLUMN [Customer];
END
