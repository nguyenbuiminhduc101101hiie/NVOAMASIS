/*
  Bảng lưu nội dung file đính kèm SI (varbinary(max)).
  Chạy sau khi đã có bảng dbo.SI.
  Cột AttachmentsJson trên SI (nếu có) không còn dùng bởi app — có thể DROP sau khi migrate dữ liệu thủ công.
*/

IF OBJECT_ID(N'dbo.[SI_Attachment]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[SI_Attachment] (
        [AttachmentId] uniqueidentifier NOT NULL CONSTRAINT PK_SI_Attachment PRIMARY KEY,
        [SIID] uniqueidentifier NOT NULL,
        [FileName] nvarchar(500) NOT NULL,
        [Content] varbinary(max) NOT NULL,
        [ContentType] nvarchar(255) NULL,
        [UploadedUtc] datetime2 NOT NULL,
        CONSTRAINT FK_SI_Attachment_SI FOREIGN KEY ([SIID]) REFERENCES dbo.[SI] ([SIID]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX IX_SI_Attachment_SIID ON dbo.[SI_Attachment] ([SIID]);
END
