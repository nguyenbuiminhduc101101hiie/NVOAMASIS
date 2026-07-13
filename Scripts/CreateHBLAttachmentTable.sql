/*
  Bảng lưu file/link đính kèm HBL (varbinary(max) hoặc Link only).
  Chạy sau khi đã có bảng dbo.HBL.
*/

IF OBJECT_ID(N'dbo.[HBL_Attachment]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[HBL_Attachment] (
        [AttachmentId] uniqueidentifier NOT NULL CONSTRAINT PK_HBL_Attachment PRIMARY KEY,
        [hblID] uniqueidentifier NOT NULL,
        [FileName] nvarchar(500) NOT NULL,
        [Content] varbinary(max) NULL,
        [ContentType] nvarchar(255) NULL,
        [Link] nvarchar(2000) NULL,
        [UserUpload] nvarchar(100) NULL,
        [Note] nvarchar(500) NULL,
        [UploadedUtc] datetime2 NOT NULL,
        CONSTRAINT FK_HBL_Attachment_HBL
            FOREIGN KEY ([hblID]) REFERENCES dbo.[HBL] ([hblID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_HBL_Attachment_hblID ON dbo.[HBL_Attachment] ([hblID]);
END
