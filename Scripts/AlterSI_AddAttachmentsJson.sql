/*
  Adds AttachmentsJson (multi-file) and migrates legacy AttachmentPath (single) if present.
  Run once on existing databases.
*/

IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    INNER JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = N'SI' AND SCHEMA_NAME(t.schema_id) = N'dbo' AND c.name = N'AttachmentsJson'
)
BEGIN
    ALTER TABLE dbo.[SI] ADD [AttachmentsJson] nvarchar(max) NULL;
END

-- Migrate single AttachmentPath -> JSON array (only if legacy column exists)
IF COL_LENGTH(N'dbo.SI', N'AttachmentPath') IS NOT NULL
BEGIN
    UPDATE dbo.[SI]
    SET [AttachmentsJson] = N'["' + REPLACE(REPLACE(ISNULL([AttachmentPath], N''), N'\', N'/'), N'"', N'\"') + N'"]'
    WHERE [AttachmentPath] IS NOT NULL
      AND LEN(LTRIM(RTRIM([AttachmentPath]))) > 0
      AND ([AttachmentsJson] IS NULL OR LEN(LTRIM(RTRIM([AttachmentsJson]))) = 0);
END
