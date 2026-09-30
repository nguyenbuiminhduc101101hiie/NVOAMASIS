/*
  10.18 KHO VẬT TƯ, HÀNG HÓA (TK 151, 152, 153, 154, 155, 156) — nhập xuất tồn kho.
  Chạy trên TỪNG tenant DB + DB template. Chạy lại nhiều lần vẫn an toàn.

  Bảng mới:
    KhoVatTu         Danh mục vật tư, CCDC, thành phẩm, hàng hóa (mã, tên, ĐVT, TK kho 151–156)
    KhoHang          Danh mục kho
    PhieuKho         Phiếu tồn đầu kỳ / nhập kho / xuất kho / chuyển kho
    PhieuKhoChiTiet  Dòng hàng của phiếu (SL, đơn giá, thành tiền, thuế GTGT)

  Mã quyền (MenuNames — cấp cho user ở 1.6 Phân quyền):
    KHO_DanhMuc  10.18.1 Danh mục vật tư, kho      Xem / Thêm / Sửa / Xóa
    KHO_Phieu    10.18.2 Phiếu nhập, xuất kho      Xem / Thêm / Sửa / Xóa (phiếu nháp); Duyệt = Ghi sổ / Bỏ ghi sổ
    KHO_BaoCao   10.18.3 Tổng hợp nhập xuất tồn, 10.18.4 Sổ chi tiết vật tư (thẻ kho)   Xem

  Không đổi cấu trúc bảng cũ. Khi ghi sổ, phiếu nhập/xuất sinh chứng từ vào các bảng có sẵn:
    AccountingVouchers (SourceModule = 'INVENTORY', SourceId = PhieuKho.Id), AccountingVoucherLines, GeneralLedgerEntries.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.KhoVatTu', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KhoVatTu] (
        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_KhoVatTu] PRIMARY KEY,
        [Code]             nvarchar(50)     NOT NULL,
        [Name]             nvarchar(250)    NOT NULL,
        [Unit]             nvarchar(30)     NOT NULL CONSTRAINT [DF_KhoVatTu_Unit] DEFAULT N'',
        [InventoryAccount] nvarchar(20)     NOT NULL,
        [Category]         nvarchar(100)    NULL,
        [MinQty]           decimal(18,4)    NULL,
        [Note]             nvarchar(500)    NULL,
        [IsActive]         bit              NOT NULL CONSTRAINT [DF_KhoVatTu_IsActive] DEFAULT 1,
        [CreatedAt]        datetime2        NOT NULL CONSTRAINT [DF_KhoVatTu_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy]        nvarchar(100)    NULL,
        [UpdatedAt]        datetime2        NULL,
        [UpdatedBy]        nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_KhoVatTu_Code] ON [dbo].[KhoVatTu]([Code]);
END;

IF OBJECT_ID(N'dbo.KhoHang', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KhoHang] (
        [Id]        uniqueidentifier NOT NULL CONSTRAINT [PK_KhoHang] PRIMARY KEY,
        [Code]      nvarchar(50)     NOT NULL,
        [Name]      nvarchar(250)    NOT NULL,
        [Address]   nvarchar(500)    NULL,
        [Keeper]    nvarchar(150)    NULL,
        [IsActive]  bit              NOT NULL CONSTRAINT [DF_KhoHang_IsActive] DEFAULT 1,
        [CreatedAt] datetime2        NOT NULL CONSTRAINT [DF_KhoHang_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy] nvarchar(100)    NULL,
        [UpdatedAt] datetime2        NULL,
        [UpdatedBy] nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_KhoHang_Code] ON [dbo].[KhoHang]([Code]);
END;

IF OBJECT_ID(N'dbo.PhieuKho', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PhieuKho] (
        [Id]            uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKho] PRIMARY KEY,
        [DocType]       nvarchar(10)     NOT NULL,
        [DocNo]         nvarchar(30)     NOT NULL,
        [DocDate]       datetime2        NOT NULL,
        [Reason]        nvarchar(30)     NULL,
        [WarehouseId]   uniqueidentifier NOT NULL,
        [ToWarehouseId] uniqueidentifier NULL,
        [PartnerId]     uniqueidentifier NULL,
        [PartnerName]   nvarchar(250)    NULL,
        [ContactName]   nvarchar(150)    NULL,
        [ContraAccount] nvarchar(20)     NULL,
        [VatAccount]    nvarchar(20)     NULL,
        [InvoiceNo]     nvarchar(50)     NULL,
        [InvoiceDate]   datetime2        NULL,
        [Description]   nvarchar(500)    NULL,
        [Status]        int              NOT NULL CONSTRAINT [DF_PhieuKho_Status] DEFAULT 0,
        [VoucherId]     uniqueidentifier NULL,
        [CreatedAt]     datetime2        NOT NULL CONSTRAINT [DF_PhieuKho_CreatedAt] DEFAULT SYSDATETIME(),
        [CreatedBy]     nvarchar(100)    NULL,
        [UpdatedAt]     datetime2        NULL,
        [UpdatedBy]     nvarchar(100)    NULL,
        [PostedAt]      datetime2        NULL,
        [PostedBy]      nvarchar(100)    NULL
    );
    CREATE UNIQUE INDEX [IX_PhieuKho_DocNo] ON [dbo].[PhieuKho]([DocNo]);
    CREATE INDEX [IX_PhieuKho_DocDate_DocType] ON [dbo].[PhieuKho]([DocDate], [DocType]);
END;

IF OBJECT_ID(N'dbo.PhieuKhoChiTiet', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PhieuKhoChiTiet] (
        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKhoChiTiet] PRIMARY KEY,
        [PhieuKhoId]       uniqueidentifier NOT NULL,
        [LineNo]           int              NOT NULL,
        [VatTuId]          uniqueidentifier NOT NULL,
        [InventoryAccount] nvarchar(20)     NOT NULL,
        [ContraAccount]    nvarchar(20)     NULL,
        [Quantity]         decimal(18,4)    NOT NULL,
        [UnitCost]         decimal(18,4)    NOT NULL,
        [Amount]           decimal(18,2)    NOT NULL,
        [VatRate]          decimal(5,2)     NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatRate] DEFAULT 0,
        [VatAmount]        decimal(18,2)    NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatAmount] DEFAULT 0,
        [Note]             nvarchar(500)    NULL
    );
    CREATE INDEX [IX_PhieuKhoChiTiet_PhieuKhoId] ON [dbo].[PhieuKhoChiTiet]([PhieuKhoId]);
    CREATE INDEX [IX_PhieuKhoChiTiet_VatTuId] ON [dbo].[PhieuKhoChiTiet]([VatTuId]);
END;

-- Mã quyền
;WITH src AS (
    SELECT N'KHO_DanhMuc' AS MenuName, N'10.18.1 Danh muc vat tu, kho' AS Title UNION ALL
    SELECT N'KHO_Phieu',   N'10.18.2 Phieu nhap, xuat kho' UNION ALL
    SELECT N'KHO_BaoCao',  N'10.18.3-4 Bao cao nhap xuat ton, the kho'
)
INSERT INTO [MenuNames] (MenuID, MenuName, Title)
SELECT NEWID(), src.MenuName, src.Title
FROM src
WHERE NOT EXISTS (SELECT 1 FROM [MenuNames] m WHERE m.MenuName = src.MenuName);

-- Cấp toàn quyền cho user phòng ADMIN (bỏ đoạn này nếu không muốn)
INSERT INTO [Permissions] (PermissionId, MenuId, MenuName, UserName, See, Edit, Del, Approve, [Add])
SELECT NEWID(), m.MenuID, m.MenuName, u.Usr, 1, 1, 1, 1, 1
FROM [UserList] u
CROSS JOIN [MenuNames] m
WHERE u.Department = N'ADMIN'
  AND u.Usr IS NOT NULL
  AND m.MenuName IN (N'KHO_DanhMuc', N'KHO_Phieu', N'KHO_BaoCao')
  AND NOT EXISTS (SELECT 1 FROM [Permissions] p WHERE p.UserName = u.Usr AND p.MenuName = m.MenuName);

-- Mẫu 1 kho mặc định (để lập phiếu ngay; đổi tên ở 10.18.1)
IF NOT EXISTS (SELECT 1 FROM [dbo].[KhoHang])
    INSERT INTO [dbo].[KhoHang] (Id, Code, Name, IsActive, CreatedAt, CreatedBy)
    VALUES (NEWID(), N'KHO01', N'Kho chính', 1, SYSDATETIME(), N'system');

COMMIT TRANSACTION;
GO

PRINT N'Đã tạo bảng kho vật tư (KhoVatTu, KhoHang, PhieuKho, PhieuKhoChiTiet) và mã quyền KHO_*. Người dùng F5 để thấy menu 10.18.';
