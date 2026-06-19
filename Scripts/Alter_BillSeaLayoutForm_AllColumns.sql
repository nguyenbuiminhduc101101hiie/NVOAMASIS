-- Chạy script này trên MỖI database tenant (không chỉ Acc_Multi_DB registry).
-- Bổ sung các cột còn thiếu cho bảng BillSeaLayoutForm.

IF OBJECT_ID(N'dbo.BillSeaLayoutForm', N'U') IS NULL
BEGIN
    RAISERROR(N'Bảng dbo.BillSeaLayoutForm chưa tồn tại. Chạy Scripts/Create_BillSeaLayoutForm.sql trước.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'AttachMrtContent') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD AttachMrtContent VARBINARY(MAX) NULL;
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'Logo') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD Logo VARBINARY(MAX) NULL;
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormKind') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormKind NVARCHAR(10) NOT NULL CONSTRAINT DF_BillSeaLayoutForm_FormKind DEFAULT (N'Sea');
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormBillAir') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormBillAir VARBINARY(MAX) NULL;
END
GO
