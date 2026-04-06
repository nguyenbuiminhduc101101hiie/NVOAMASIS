/*
  Legacy: single AttachmentPath column.
  For multiple files use AttachmentsJson — run Scripts/AlterSI_AddAttachmentsJson.sql
  (adds column and migrates AttachmentPath into JSON when present).
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'SI' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND c.name = N'AttachmentPath'
)
BEGIN
    ALTER TABLE dbo.[SI] ADD [AttachmentPath] nvarchar(max) NULL;
END
