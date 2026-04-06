/*
  Adds AttachmentHistoryJson to dbo.SI if missing (lịch sử tên file attach/remove).
  Run once on existing databases.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'SI' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND c.name = N'AttachmentHistoryJson'
)
BEGIN
    ALTER TABLE dbo.[SI] ADD [AttachmentHistoryJson] nvarchar(max) NULL;
END
