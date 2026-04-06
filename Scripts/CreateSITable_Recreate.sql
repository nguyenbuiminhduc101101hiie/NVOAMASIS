/*
  XÓA HẾT DỮ LIỆU bảng SI rồi tạo lại từ đầu.
  Chỉ chạy khi chấp nhận mất toàn bộ bản ghi SI.
  SQL Server 2016+ (DROP IF EXISTS).
*/

IF OBJECT_ID(N'dbo.[SI]', N'U') IS NOT NULL
    DROP TABLE dbo.[SI];
GO

CREATE TABLE dbo.[SI] (
    [SIID] uniqueidentifier NOT NULL CONSTRAINT PK_SI PRIMARY KEY,
    [mbl] nvarchar(max) NULL,
    [shipper] nvarchar(max) NULL,
    [consignee] nvarchar(max) NULL,
    [notify1] nvarchar(max) NULL,
    [vessel] nvarchar(max) NULL,
    [voy] nvarchar(max) NULL,
    [porname] nvarchar(max) NULL,
    [porcode] nvarchar(max) NULL,
    [polname] nvarchar(max) NULL,
    [polcode] nvarchar(max) NULL,
    [podname] nvarchar(max) NULL,
    [podcode] nvarchar(max) NULL,
    [delName] nvarchar(max) NULL,
    [delcode] nvarchar(max) NULL,
    [bkno] nvarchar(max) NULL,
    [hbl] nvarchar(max) NULL,
    [markAndNumbers] nvarchar(max) NULL,
    [NoOfPackages] nvarchar(max) NULL,
    [description] nvarchar(max) NULL,
    [gross] nvarchar(max) NULL,
    [cbm] nvarchar(max) NULL,
    [shipOnboard] nvarchar(max) NULL,
    [say] nvarchar(max) NULL,
    [freightPayableAt] nvarchar(max) NULL,
    [numberOfOriginal] nvarchar(max) NULL,
    [placeAndDate] nvarchar(max) NULL,
    [collectat] nvarchar(max) NULL,
    [dateLaden] nvarchar(max) NULL,
    [freightAmount] nvarchar(max) NULL,
    [forDelivery] nvarchar(max) NULL,
    [Air_type] nvarchar(max) NULL,
    [AttachmentHistoryJson] nvarchar(max) NULL
);
GO
