-- File đính kèm (lưu dạng byte trong DB) cho các màn hình báo giá 3.2 - 3.6 (Import, Export, Truck, KTCL, Custom)
-- Chạy 1 lần trước khi chạy ứng dụng bản mới. Có thể chạy lại nhiều lần an toàn.

-- 1) Bảng chứa file
IF OBJECT_ID(N'dbo.ProductPriceAttachments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductPriceAttachments]
    (
        [AttachmentId] uniqueidentifier NOT NULL CONSTRAINT [PK_ProductPriceAttachments] PRIMARY KEY,
        [OwnerType]    nvarchar(20)     NOT NULL,
        [FileName]     nvarchar(500)    NOT NULL,
        [Content]      varbinary(max)   NOT NULL,
        [ContentType]  nvarchar(255)    NULL,
        [FileSize]     bigint           NOT NULL,
        [UserUpload]   nvarchar(100)    NULL,
        [UploadedUtc]  datetime2        NOT NULL
    );
END
GO

-- 2) Cột lưu danh sách Id file đính kèm (ngăn cách bằng dấu ;) ở 5 bảng báo giá
IF COL_LENGTH(N'dbo.ImportCostRequest', N'AttachmentIds') IS NULL
    ALTER TABLE [dbo].[ImportCostRequest] ADD [AttachmentIds] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.ExportCostRequest', N'AttachmentIds') IS NULL
    ALTER TABLE [dbo].[ExportCostRequest] ADD [AttachmentIds] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.TruckingCostRequest', N'AttachmentIds') IS NULL
    ALTER TABLE [dbo].[TruckingCostRequest] ADD [AttachmentIds] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.KTCLCostRequest', N'AttachmentIds') IS NULL
    ALTER TABLE [dbo].[KTCLCostRequest] ADD [AttachmentIds] nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.CustomCostRequest', N'AttachmentIds') IS NULL
    ALTER TABLE [dbo].[CustomCostRequest] ADD [AttachmentIds] nvarchar(max) NULL;
GO

-- 3) (Tùy chọn) Nếu trước đó đã chạy file AlterProductPriceRequests_AddFilesPath.sql thì cột FilesPath không còn dùng, có thể xóa:
-- IF COL_LENGTH(N'dbo.ImportCostRequest', N'FilesPath') IS NOT NULL ALTER TABLE [dbo].[ImportCostRequest] DROP COLUMN [FilesPath];
-- IF COL_LENGTH(N'dbo.ExportCostRequest', N'FilesPath') IS NOT NULL ALTER TABLE [dbo].[ExportCostRequest] DROP COLUMN [FilesPath];
-- IF COL_LENGTH(N'dbo.TruckingCostRequest', N'FilesPath') IS NOT NULL ALTER TABLE [dbo].[TruckingCostRequest] DROP COLUMN [FilesPath];
-- IF COL_LENGTH(N'dbo.KTCLCostRequest', N'FilesPath') IS NOT NULL ALTER TABLE [dbo].[KTCLCostRequest] DROP COLUMN [FilesPath];
-- IF COL_LENGTH(N'dbo.CustomCostRequest', N'FilesPath') IS NOT NULL ALTER TABLE [dbo].[CustomCostRequest] DROP COLUMN [FilesPath];
