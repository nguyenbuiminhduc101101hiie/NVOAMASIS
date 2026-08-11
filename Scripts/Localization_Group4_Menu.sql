-- Menu 4: Price schedule (LCC, Nâng hạ, Rút ruột, KDTV, KTCL, Lưu kho)
BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey IN (
    'PriceListAlreadyApproved',
    'LCC_Create','LCC_Edit','InputRemarksPlaceholder',
    'BGNH_Create','BGNH_Edit',
    'RutRuot_Create','RutRuot_Edit',
    'KDTV_Create','KDTV_Edit',
    'BGKTCL_Create','BGKTCL_Edit',
    'LuuKho_Create','LuuKho_Edit','LuuKho_AddDetail'
);

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('PriceListAlreadyApproved','en-US',N'Price list is already approved. Please un-approve first.'),('PriceListAlreadyApproved','vi-VN',N'Biểu giá đã được Approve!!! , vui lòng bỏ Approve'),('PriceListAlreadyApproved','zh-CN',N'价格表已审批，请先取消审批。'),
('LCC_Create','en-US',N'LCC Shipping Line — Create'),('LCC_Create','vi-VN',N'LCC Hãng tàu — Tạo mới'),('LCC_Create','zh-CN',N'船公司本地费 — 新建'),
('LCC_Edit','en-US',N'LCC Shipping Line — Edit'),('LCC_Edit','vi-VN',N'LCC Hãng tàu — Sửa'),('LCC_Edit','zh-CN',N'船公司本地费 — 编辑'),
('InputRemarksPlaceholder','en-US',N'Input remarks...'),('InputRemarksPlaceholder','vi-VN',N'Nhập ghi chú...'),('InputRemarksPlaceholder','zh-CN',N'输入备注...'),
('BGNH_Create','en-US',N'Lifting and Lowering Price List — Create'),('BGNH_Create','vi-VN',N'Biểu giá nâng hạ — Tạo mới'),('BGNH_Create','zh-CN',N'装卸价格表 — 新建'),
('BGNH_Edit','en-US',N'Lifting and Lowering Price List — Edit'),('BGNH_Edit','vi-VN',N'Biểu giá nâng hạ — Sửa'),('BGNH_Edit','zh-CN',N'装卸价格表 — 编辑'),
('RutRuot_Create','en-US',N'Stripping Price List — Create'),('RutRuot_Create','vi-VN',N'Biểu giá rút ruột — Tạo mới'),('RutRuot_Create','zh-CN',N'拆箱价格表 — 新建'),
('RutRuot_Edit','en-US',N'Stripping Price List — Edit'),('RutRuot_Edit','vi-VN',N'Biểu giá rút ruột — Sửa'),('RutRuot_Edit','zh-CN',N'拆箱价格表 — 编辑'),
('KDTV_Create','en-US',N'Plant Quarantine Price List — Create'),('KDTV_Create','vi-VN',N'Biểu giá KDTV — Tạo mới'),('KDTV_Create','zh-CN',N'植物检疫价格表 — 新建'),
('KDTV_Edit','en-US',N'Plant Quarantine Price List — Edit'),('KDTV_Edit','vi-VN',N'Biểu giá KDTV — Sửa'),('KDTV_Edit','zh-CN',N'植物检疫价格表 — 编辑'),
('BGKTCL_Create','en-US',N'Plant Quality Control Price List — Create'),('BGKTCL_Create','vi-VN',N'Biểu giá KTCL — Tạo mới'),('BGKTCL_Create','zh-CN',N'植物质检价格表 — 新建'),
('BGKTCL_Edit','en-US',N'Plant Quality Control Price List — Edit'),('BGKTCL_Edit','vi-VN',N'Biểu giá KTCL — Sửa'),('BGKTCL_Edit','zh-CN',N'植物质检价格表 — 编辑'),
('LuuKho_Create','en-US',N'Warehouse Price List — Create'),('LuuKho_Create','vi-VN',N'Biểu giá lưu kho — Tạo mới'),('LuuKho_Create','zh-CN',N'仓储价格表 — 新建'),
('LuuKho_Edit','en-US',N'Warehouse Price List — Edit'),('LuuKho_Edit','vi-VN',N'Biểu giá lưu kho — Sửa'),('LuuKho_Edit','zh-CN',N'仓储价格表 — 编辑'),
('LuuKho_AddDetail','en-US',N'Warehouse Price List — Details'),('LuuKho_AddDetail','vi-VN',N'Biểu giá lưu kho — Chi tiết'),('LuuKho_AddDetail','zh-CN',N'仓储价格表 — 明细');

COMMIT;
