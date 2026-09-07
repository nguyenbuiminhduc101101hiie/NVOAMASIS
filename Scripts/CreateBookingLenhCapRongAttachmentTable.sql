/*
  Bảng lưu file Import Lệnh Cấp Rỗng của Booking (Excel / Word / PDF / ...).
  Chạy trên SQL Server sau khi đã có bảng dbo.CONTAINEROUTBOUNDNOTIFY_sale.
*/

IF OBJECT_ID(N'dbo.[Booking_LenhCapRong_Attachment]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[Booking_LenhCapRong_Attachment] (
        [AttachmentId] uniqueidentifier NOT NULL CONSTRAINT PK_Booking_LenhCapRong_Attachment PRIMARY KEY,
        [BookingId] uniqueidentifier NOT NULL,
        [FileName] nvarchar(500) NOT NULL,
        [Content] varbinary(max) NULL,
        [ContentType] nvarchar(255) NULL,
        [FileSize] bigint NOT NULL CONSTRAINT DF_Booking_LenhCapRong_Attachment_FileSize DEFAULT (0),
        [UserUpload] nvarchar(100) NULL,
        [Note] nvarchar(500) NULL,
        [UploadedUtc] datetime2 NOT NULL CONSTRAINT DF_Booking_LenhCapRong_Attachment_UploadedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Booking_LenhCapRong_Attachment_Booking
            FOREIGN KEY ([BookingId]) REFERENCES dbo.[CONTAINEROUTBOUNDNOTIFY_sale] ([ContainerOutBoundNotifyId]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_Booking_LenhCapRong_Attachment_BookingId
        ON dbo.[Booking_LenhCapRong_Attachment] ([BookingId]);
END
GO
