-- Chạy trên MỖI database tenant.
-- FormKind NVARCHAR(10) bị truncate khi tạo Lệnh Cấp Cont Rỗng (LenhCapContRong / CapRong)
-- và Lệnh Điều Xe (LenhDieuXe = 11 ký tự).

IF OBJECT_ID(N'dbo.BillSeaLayoutForm', N'U') IS NULL
BEGIN
    RAISERROR(N'Bảng dbo.BillSeaLayoutForm chưa tồn tại. Chạy Scripts/Create_BillSeaLayoutForm.sql trước.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.BillSeaLayoutForm', N'FormKind') IS NULL
BEGIN
    ALTER TABLE dbo.BillSeaLayoutForm
        ADD FormKind NVARCHAR(50) NOT NULL CONSTRAINT DF_BillSeaLayoutForm_FormKind DEFAULT (N'Sea');
END
ELSE IF EXISTS (
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
