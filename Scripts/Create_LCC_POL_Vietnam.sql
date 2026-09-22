/*
  Bảng LCC_POL_Vietnam: lưu Local Charges POL Vietnam (nav 4.1.1 - "LCC POL VIETNAM").
  - Id là khóa chính (GUID), tự sinh bằng NEWID().
  - Mỗi dòng = 1 cặp (Carrier, ChargeType) với giá trị áp dụng cho container 20DC / 40HC.
  - Dữ liệu được nạp bằng chức năng Import Excel (chỉ đọc sheet 1 "POL LOCAL" của file
    "LOCAL CHARGE POL VIET NAM.xlsx"), sau đó có thể sửa/xóa/thêm thủ công qua giao diện CRUD.
  - Container20DC / Container40HC lưu dạng NVARCHAR để giữ nguyên định dạng gốc trong Excel
    (số có dấu phẩy, có hậu tố "USD", ghi chú nhiều dòng...).
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.LCC_POL_Vietnam', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LCC_POL_Vietnam (
        Id              UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_LCC_POL_Vietnam PRIMARY KEY CLUSTERED
            CONSTRAINT DF_LCC_POL_Vietnam_Id DEFAULT (NEWID()),
        Carrier         NVARCHAR(150)    NOT NULL,      -- Hãng tàu, vd: WAN HAI, COSCO, MSC...
        ChargeType      NVARCHAR(250)    NOT NULL,      -- Loại phí, vd: THC DRY, SEAL, B/L fee, TELEX...
        Container20DC   NVARCHAR(300)    NULL,          -- Giá trị áp dụng cho cont 20DC
        Container40HC   NVARCHAR(300)    NULL,          -- Giá trị áp dụng cho cont 40HC
        Remarks         NVARCHAR(1000)   NULL,
        Userupdate      NVARCHAR(200)    NULL,
        Dateupdate      NVARCHAR(50)     NULL,
        CreatedDate     DATETIME         NULL
            CONSTRAINT DF_LCC_POL_Vietnam_CreatedDate DEFAULT (GETDATE()),
        SourceFileName  NVARCHAR(260)    NULL,          -- Tên file Excel đã import ra dòng này (NULL nếu thêm thủ công)

        CONSTRAINT UQ_LCC_POL_Vietnam_Carrier_ChargeType UNIQUE (Carrier, ChargeType)
    );
END
GO

-- Bảng đã tạo trước đó (chưa có cột SourceFileName) thì thêm cột này vào:
IF COL_LENGTH(N'dbo.LCC_POL_Vietnam', N'SourceFileName') IS NULL
BEGIN
    ALTER TABLE dbo.LCC_POL_Vietnam ADD SourceFileName NVARCHAR(260) NULL;
END
GO

PRINT N'Create_LCC_POL_Vietnam: OK.';
GO
