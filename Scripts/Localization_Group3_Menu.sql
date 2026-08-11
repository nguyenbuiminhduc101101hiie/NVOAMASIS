-- Menu 3: Pricing (Train Schedule, Import/Export/Truck/KTCL/Custom price)
BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey IN (
    'TrainSchedule_Create','TrainSchedule_Edit','TrainSchedule_Add',
    'PricingImport_Subtitle','PricingImport_Create','PricingImport_Edit','PricingImport_AddDetail',
    'PricingExport_Subtitle','PricingExport_Create','PricingExport_Edit','PricingExport_AddDetail',
    'PricingTruck_Subtitle','PricingTruck_Create','PricingTruck_Edit','PricingTruck_AddDetail','PricingTruck_Title',
    'PricingKTCL_Subtitle','PricingKTCL_Create','PricingKTCL_Edit','PricingKTCL_AddDetail',
    'PricingCustom_Subtitle','PricingCustom_Create','PricingCustom_Edit','PricingCustom_AddDetail',
    'RequestPricingAlreadyApproved',
    'EmailEmpty','UserEmpty','NoDataChart',
    'FileContentNotFound','AttachFileBeforeEmail','NoValidEmailSkipped',
    'EmailRecipient','CustomerEmail','AdditionalEmails','Customer'
);

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('TrainSchedule_Create','en-US',N'Train Schedule — Create'),('TrainSchedule_Create','vi-VN',N'Train Schedule — Tạo mới'),('TrainSchedule_Create','zh-CN',N'列车时刻表 — 新建'),
('TrainSchedule_Edit','en-US',N'Train Schedule — Edit'),('TrainSchedule_Edit','vi-VN',N'Train Schedule — Sửa'),('TrainSchedule_Edit','zh-CN',N'列车时刻表 — 编辑'),
('TrainSchedule_Add','en-US',N'Train Schedule — Add'),('TrainSchedule_Add','vi-VN',N'Train Schedule — Thêm'),('TrainSchedule_Add','zh-CN',N'列车时刻表 — 添加'),
('PricingImport_Subtitle','en-US',N'Import pricing request management'),('PricingImport_Subtitle','vi-VN',N'Request pricing import / quản lý báo giá import'),('PricingImport_Subtitle','zh-CN',N'进口报价请求管理'),
('PricingImport_Create','en-US',N'Request Pricing Import — Create'),('PricingImport_Create','vi-VN',N'Request Pricing Import — Tạo mới'),('PricingImport_Create','zh-CN',N'进口报价请求 — 新建'),
('PricingImport_Edit','en-US',N'Request Pricing Import — Edit'),('PricingImport_Edit','vi-VN',N'Request Pricing Import — Sửa'),('PricingImport_Edit','zh-CN',N'进口报价请求 — 编辑'),
('PricingImport_AddDetail','en-US',N'Add Detail Price Import'),('PricingImport_AddDetail','vi-VN',N'Thêm chi tiết báo giá Import'),('PricingImport_AddDetail','zh-CN',N'添加进口报价明细'),
('PricingExport_Subtitle','en-US',N'Export pricing request management'),('PricingExport_Subtitle','vi-VN',N'Quản lý báo giá export'),('PricingExport_Subtitle','zh-CN',N'出口报价请求管理'),
('PricingExport_Create','en-US',N'Request Pricing Export — Create'),('PricingExport_Create','vi-VN',N'Request Pricing Export — Tạo mới'),('PricingExport_Create','zh-CN',N'出口报价请求 — 新建'),
('PricingExport_Edit','en-US',N'Request Pricing Export — Edit'),('PricingExport_Edit','vi-VN',N'Request Pricing Export — Sửa'),('PricingExport_Edit','zh-CN',N'出口报价请求 — 编辑'),
('PricingExport_AddDetail','en-US',N'Add Detail Price Export'),('PricingExport_AddDetail','vi-VN',N'Thêm chi tiết báo giá Export'),('PricingExport_AddDetail','zh-CN',N'添加出口报价明细'),
('PricingTruck_Subtitle','en-US',N'Trucking pricing request management'),('PricingTruck_Subtitle','vi-VN',N'Request pricing truck / quản lý báo giá vận chuyển truck'),('PricingTruck_Subtitle','zh-CN',N'卡车运输报价请求管理'),
('PricingTruck_Create','en-US',N'Request Pricing Truck — Create'),('PricingTruck_Create','vi-VN',N'Request Pricing Truck — Tạo mới'),('PricingTruck_Create','zh-CN',N'卡车报价请求 — 新建'),
('PricingTruck_Edit','en-US',N'Request Pricing Truck — Edit'),('PricingTruck_Edit','vi-VN',N'Request Pricing Truck — Sửa'),('PricingTruck_Edit','zh-CN',N'卡车报价请求 — 编辑'),
('PricingTruck_AddDetail','en-US',N'Add Detail Price Truck'),('PricingTruck_AddDetail','vi-VN',N'Thêm chi tiết báo giá Truck'),('PricingTruck_AddDetail','zh-CN',N'添加卡车报价明细'),
('PricingTruck_Title','en-US',N'Request Pricing Truck'),('PricingTruck_Title','vi-VN',N'Request Pricing Truck'),('PricingTruck_Title','zh-CN',N'卡车报价请求'),
('PricingKTCL_Subtitle','en-US',N'Quality control cost request management'),('PricingKTCL_Subtitle','vi-VN',N'Cost request quality control / quản lý báo giá kiểm tra chất lượng'),('PricingKTCL_Subtitle','zh-CN',N'质检费用请求管理'),
('PricingKTCL_Create','en-US',N'Cost Request Quality Control — Create'),('PricingKTCL_Create','vi-VN',N'Cost Request Quality Control — Tạo mới'),('PricingKTCL_Create','zh-CN',N'质检费用请求 — 新建'),
('PricingKTCL_Edit','en-US',N'Cost Request Quality Control — Edit'),('PricingKTCL_Edit','vi-VN',N'Cost Request Quality Control — Sửa'),('PricingKTCL_Edit','zh-CN',N'质检费用请求 — 编辑'),
('PricingKTCL_AddDetail','en-US',N'Add Detail Cost Request Quality Control'),('PricingKTCL_AddDetail','vi-VN',N'Thêm chi tiết báo giá KTCL'),('PricingKTCL_AddDetail','zh-CN',N'添加质检费用明细'),
('PricingCustom_Subtitle','en-US',N'Customs procedure cost price management'),('PricingCustom_Subtitle','vi-VN',N'Customs procedure cost price / quản lý báo giá thủ tục hải quan'),('PricingCustom_Subtitle','zh-CN',N'报关费用管理'),
('PricingCustom_Create','en-US',N'Customs Procedure Cost Price — Create'),('PricingCustom_Create','vi-VN',N'Customs Procedure Cost Price — Tạo mới'),('PricingCustom_Create','zh-CN',N'报关费用 — 新建'),
('PricingCustom_Edit','en-US',N'Customs Procedure Cost Price — Edit'),('PricingCustom_Edit','vi-VN',N'Customs Procedure Cost Price — Sửa'),('PricingCustom_Edit','zh-CN',N'报关费用 — 编辑'),
('PricingCustom_AddDetail','en-US',N'Add Detail Customs Procedure Cost Price'),('PricingCustom_AddDetail','vi-VN',N'Thêm chi tiết báo giá thủ tục hải quan'),('PricingCustom_AddDetail','zh-CN',N'添加报关费用明细'),
('RequestPricingAlreadyApproved','en-US',N'Request pricing is already approved. Please un-approve first.'),('RequestPricingAlreadyApproved','vi-VN',N'Request Pricing đã được Approve!!! , vui lòng bỏ Approve'),('RequestPricingAlreadyApproved','zh-CN',N'报价请求已审批，请先取消审批。'),
('EmailEmpty','en-US',N'Email is empty.'),('EmailEmpty','vi-VN',N'Email trống.'),('EmailEmpty','zh-CN',N'邮箱为空。'),
('UserEmpty','en-US',N'User is empty.'),('UserEmpty','vi-VN',N'User trống.'),('UserEmpty','zh-CN',N'用户为空。'),
('NoDataChart','en-US',N'No chart data.'),('NoDataChart','vi-VN',N'Không có dữ liệu biểu đồ.'),('NoDataChart','zh-CN',N'无图表数据。'),
('FileContentNotFound','en-US',N'File content not found.'),('FileContentNotFound','vi-VN',N'Không tìm thấy nội dung file.'),('FileContentNotFound','zh-CN',N'未找到文件内容。'),
('AttachFileBeforeEmail','en-US',N'Please attach at least one file before sending email.'),('AttachFileBeforeEmail','vi-VN',N'Vui lòng đính kèm ít nhất một file trước khi gửi email.'),('AttachFileBeforeEmail','zh-CN',N'发送邮件前请至少附加一个文件。'),
('NoValidEmailSkipped','en-US',N'No valid email found; skipped sending attachment email.'),('NoValidEmailSkipped','vi-VN',N'Không có email hợp lệ, bỏ qua gửi email đính kèm.'),('NoValidEmailSkipped','zh-CN',N'未找到有效邮箱，已跳过发送附件邮件。'),
('EmailRecipient','en-US',N'Email recipient user'),('EmailRecipient','vi-VN',N'User Nhận Email'),('EmailRecipient','zh-CN',N'邮件接收用户'),
('CustomerEmail','en-US',N'Customer email'),('CustomerEmail','vi-VN',N'Email khách hàng'),('CustomerEmail','zh-CN',N'客户邮箱'),
('AdditionalEmails','en-US',N'Additional emails'),('AdditionalEmails','vi-VN',N'Email bổ sung'),('AdditionalEmails','zh-CN',N'附加邮箱'),
('Customer','en-US',N'Customer'),('Customer','vi-VN',N'Khách hàng'),('Customer','zh-CN',N'客户');

COMMIT;
