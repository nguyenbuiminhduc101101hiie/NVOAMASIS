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
        ADD FormKind NVARCHAR(50) NOT NULL CONSTRAINT DF_BillSeaLayoutForm_FormKind DEFAULT (N'Sea');
END
GO

-- Lệnh Cấp Cont Rỗng / Lệnh Điều Xe cần hơn 10 ký tự.
IF EXISTS (
    SELECT 1
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.BillSeaLayoutForm')
      AND c.name = N'FormKind'
      AND c.max_length <> -1
      AND c.max_length < 100
)
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ALTER COLUMN FormKind NVARCHAR(50) NOT NULL;
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormBillAir') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormBillAir VARBINARY(MAX) NULL;
END
GO
