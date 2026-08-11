/*
  Menu 2.1 Shipment + ShipmentIndex grid/filter keys
  (keys keep spaces to match existing Localizer[...] in Razor)
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    -- Menu / page title
    SELECT N'Shipment' AS ResourceKey, N'en-US' AS Culture, N'Shipment' AS Value UNION ALL
    SELECT N'Shipment', N'vi-VN', N'Lô hàng' UNION ALL
    SELECT N'Shipment', N'zh-CN', N'货运' UNION ALL

    -- Grid columns
    SELECT N'Job No', N'en-US', N'Job No' UNION ALL
    SELECT N'Job No', N'vi-VN', N'Số Job' UNION ALL
    SELECT N'Job No', N'zh-CN', N'作业号' UNION ALL

    SELECT N'Selected Date Create Job', N'en-US', N'Select month to create Job No' UNION ALL
    SELECT N'Selected Date Create Job', N'vi-VN', N'Chọn tháng tạo số Job' UNION ALL
    SELECT N'Selected Date Create Job', N'zh-CN', N'选择创建作业号的月份' UNION ALL

    SELECT N'Total MBL', N'en-US', N'Total MBL' UNION ALL
    SELECT N'Total MBL', N'vi-VN', N'Tổng MBL' UNION ALL
    SELECT N'Total MBL', N'zh-CN', N'MBL 总数' UNION ALL

    SELECT N'Total HBL', N'en-US', N'Total HBL' UNION ALL
    SELECT N'Total HBL', N'vi-VN', N'Tổng HBL' UNION ALL
    SELECT N'Total HBL', N'zh-CN', N'HBL 总数' UNION ALL

    SELECT N'Total Debit (HBL)', N'en-US', N'Total Debit (HBL)' UNION ALL
    SELECT N'Total Debit (HBL)', N'vi-VN', N'Tổng Debit (HBL)' UNION ALL
    SELECT N'Total Debit (HBL)', N'zh-CN', N'借记合计 (HBL)' UNION ALL

    SELECT N'Total Credit (HBL)', N'en-US', N'Total Credit (HBL)' UNION ALL
    SELECT N'Total Credit (HBL)', N'vi-VN', N'Tổng Credit (HBL)' UNION ALL
    SELECT N'Total Credit (HBL)', N'zh-CN', N'贷记合计 (HBL)' UNION ALL

    SELECT N'Debit/Credit Done', N'en-US', N'Debit/Credit Done' UNION ALL
    SELECT N'Debit/Credit Done', N'vi-VN', N'Debit/Credit xong' UNION ALL
    SELECT N'Debit/Credit Done', N'zh-CN', N'借贷完成' UNION ALL

    SELECT N'MBL', N'en-US', N'MBL' UNION ALL
    SELECT N'MBL', N'vi-VN', N'MBL' UNION ALL
    SELECT N'MBL', N'zh-CN', N'MBL' UNION ALL

    SELECT N'HBL', N'en-US', N'HBL' UNION ALL
    SELECT N'HBL', N'vi-VN', N'HBL' UNION ALL
    SELECT N'HBL', N'zh-CN', N'HBL' UNION ALL

    SELECT N'HBL No', N'en-US', N'HBL No' UNION ALL
    SELECT N'HBL No', N'vi-VN', N'Số HBL' UNION ALL
    SELECT N'HBL No', N'zh-CN', N'HBL 号' UNION ALL

    SELECT N'FLC', N'en-US', N'FLC' UNION ALL
    SELECT N'FLC', N'vi-VN', N'FLC' UNION ALL
    SELECT N'FLC', N'zh-CN', N'FLC' UNION ALL

    SELECT N'Date Create', N'en-US', N'Date Create' UNION ALL
    SELECT N'Date Create', N'vi-VN', N'Ngày tạo' UNION ALL
    SELECT N'Date Create', N'zh-CN', N'创建日期' UNION ALL

    SELECT N'Approve', N'en-US', N'Approve' UNION ALL
    SELECT N'Approve', N'vi-VN', N'Duyệt' UNION ALL
    SELECT N'Approve', N'zh-CN', N'审批' UNION ALL

    SELECT N'Reload', N'en-US', N'Reload' UNION ALL
    SELECT N'Reload', N'vi-VN', N'Tải lại' UNION ALL
    SELECT N'Reload', N'zh-CN', N'刷新' UNION ALL

    SELECT N'Duplicate', N'en-US', N'Duplicate' UNION ALL
    SELECT N'Duplicate', N'vi-VN', N'Nhân bản' UNION ALL
    SELECT N'Duplicate', N'zh-CN', N'复制' UNION ALL

    SELECT N'Report', N'en-US', N'Report' UNION ALL
    SELECT N'Report', N'vi-VN', N'Báo cáo' UNION ALL
    SELECT N'Report', N'zh-CN', N'报告' UNION ALL

    SELECT N'Shipper', N'en-US', N'Shipper' UNION ALL
    SELECT N'Shipper', N'vi-VN', N'Shipper' UNION ALL
    SELECT N'Shipper', N'zh-CN', N'发货人' UNION ALL

    SELECT N'Consignee', N'en-US', N'Consignee' UNION ALL
    SELECT N'Consignee', N'vi-VN', N'Consignee' UNION ALL
    SELECT N'Consignee', N'zh-CN', N'收货人' UNION ALL

    SELECT N'POL', N'en-US', N'POL' UNION ALL
    SELECT N'POL', N'vi-VN', N'POL' UNION ALL
    SELECT N'POL', N'zh-CN', N'装货港' UNION ALL

    SELECT N'POD', N'en-US', N'POD' UNION ALL
    SELECT N'POD', N'vi-VN', N'POD' UNION ALL
    SELECT N'POD', N'zh-CN', N'卸货港' UNION ALL

    SELECT N'Description', N'en-US', N'Description' UNION ALL
    SELECT N'Description', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'Description', N'zh-CN', N'描述' UNION ALL

    SELECT N'Customer', N'en-US', N'Customer' UNION ALL
    SELECT N'Customer', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Customer', N'zh-CN', N'客户' UNION ALL

    SELECT N'Search', N'en-US', N'Search' UNION ALL
    SELECT N'Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'Search', N'zh-CN', N'搜索' UNION ALL

    -- Related menu 2 items with spaces (same issue)
    SELECT N'Public Client', N'en-US', N'Public Client' UNION ALL
    SELECT N'Public Client', N'vi-VN', N'Khách hàng công khai' UNION ALL
    SELECT N'Public Client', N'zh-CN', N'公开客户' UNION ALL

    SELECT N'Shipment Report', N'en-US', N'Shipment Report' UNION ALL
    SELECT N'Shipment Report', N'vi-VN', N'Báo cáo lô hàng' UNION ALL
    SELECT N'Shipment Report', N'zh-CN', N'货运报告' UNION ALL

    SELECT N'Client', N'en-US', N'Client' UNION ALL
    SELECT N'Client', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Client', N'zh-CN', N'客户' UNION ALL

    SELECT N'Vendor', N'en-US', N'Vendor' UNION ALL
    SELECT N'Vendor', N'vi-VN', N'Nhà cung cấp' UNION ALL
    SELECT N'Vendor', N'zh-CN', N'供应商'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;

PRINT N'Shipment menu + grid localization imported.';
