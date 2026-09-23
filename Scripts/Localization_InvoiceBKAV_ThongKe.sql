SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LocalizationResources', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.LocalizationResources does not exist.', 1;
END;
GO

DECLARE @Resources TABLE
(
    ResourceKey nvarchar(500) NOT NULL,
    Culture nvarchar(10) NOT NULL,
    Value nvarchar(max) NOT NULL,
    PRIMARY KEY (ResourceKey, Culture)
);

INSERT INTO @Resources (ResourceKey, Culture, Value)
VALUES
(N'BkavThongKe.Menu', N'en-US', N'BKAV Statistics'),
(N'BkavThongKe.Menu', N'vi-VN', N'Thống Kê BKAV'),
(N'BkavThongKe.Menu', N'zh-CN', N'BKAV Statistics'),
(N'BkavThongKe.Title', N'en-US', N'BKAV Invoice Statistics'),
(N'BkavThongKe.Title', N'vi-VN', N'Thống Kê Hóa Đơn BKAV'),
(N'BkavThongKe.Title', N'zh-CN', N'BKAV Invoice Statistics'),
(N'BkavThongKe.Subtitle', N'en-US', N'Find invoices that were drafted or already signed/issued, filter by collection (Thu ho)'),
(N'BkavThongKe.Subtitle', N'vi-VN', N'Tra cứu hóa đơn đã xuất nháp hoặc đã xuất hóa đơn thực (đã ký), lọc theo điều kiện thu hộ'),
(N'BkavThongKe.Subtitle', N'zh-CN', N'Find invoices that were drafted or already signed/issued'),

(N'BkavThongKe.Filter.Status', N'en-US', N'Invoice status'),
(N'BkavThongKe.Filter.Status', N'vi-VN', N'Trạng thái hóa đơn'),
(N'BkavThongKe.Filter.Status', N'zh-CN', N'Invoice status'),
(N'BkavThongKe.Filter.Status.All', N'en-US', N'All'),
(N'BkavThongKe.Filter.Status.All', N'vi-VN', N'Tất cả'),
(N'BkavThongKe.Filter.Status.All', N'zh-CN', N'All'),
(N'BkavThongKe.Filter.Status.Draft', N'en-US', N'Draft (not signed)'),
(N'BkavThongKe.Filter.Status.Draft', N'vi-VN', N'Đã xuất nháp (chưa ký)'),
(N'BkavThongKe.Filter.Status.Draft', N'zh-CN', N'Draft (not signed)'),
(N'BkavThongKe.Filter.Status.Signed', N'en-US', N'Issued (signed)'),
(N'BkavThongKe.Filter.Status.Signed', N'vi-VN', N'Đã xuất hóa đơn (đã ký)'),
(N'BkavThongKe.Filter.Status.Signed', N'zh-CN', N'Issued (signed)'),

(N'BkavThongKe.Filter.ThuHo', N'en-US', N'Collection (Thu ho)'),
(N'BkavThongKe.Filter.ThuHo', N'vi-VN', N'Thu hộ'),
(N'BkavThongKe.Filter.ThuHo', N'zh-CN', N'Collection (Thu ho)'),
(N'BkavThongKe.Filter.ThuHo.All', N'en-US', N'All'),
(N'BkavThongKe.Filter.ThuHo.All', N'vi-VN', N'Tất cả'),
(N'BkavThongKe.Filter.ThuHo.All', N'zh-CN', N'All'),
(N'BkavThongKe.Filter.ThuHo.Yes', N'en-US', N'Collection'),
(N'BkavThongKe.Filter.ThuHo.Yes', N'vi-VN', N'Có thu hộ'),
(N'BkavThongKe.Filter.ThuHo.Yes', N'zh-CN', N'Collection'),
(N'BkavThongKe.Filter.ThuHo.No', N'en-US', N'No collection'),
(N'BkavThongKe.Filter.ThuHo.No', N'vi-VN', N'Không thu hộ'),
(N'BkavThongKe.Filter.ThuHo.No', N'zh-CN', N'No collection'),

(N'BkavThongKe.Filter.FromDate', N'en-US', N'From date'),
(N'BkavThongKe.Filter.FromDate', N'vi-VN', N'Từ ngày'),
(N'BkavThongKe.Filter.FromDate', N'zh-CN', N'From date'),
(N'BkavThongKe.Filter.ToDate', N'en-US', N'To date'),
(N'BkavThongKe.Filter.ToDate', N'vi-VN', N'Đến ngày'),
(N'BkavThongKe.Filter.ToDate', N'zh-CN', N'To date'),
(N'BkavThongKe.Filter.Clear', N'en-US', N'Clear filters'),
(N'BkavThongKe.Filter.Clear', N'vi-VN', N'Xóa bộ lọc'),
(N'BkavThongKe.Filter.Clear', N'zh-CN', N'Clear filters'),

(N'BkavThongKe.Summary.Total', N'en-US', N'Total'),
(N'BkavThongKe.Summary.Total', N'vi-VN', N'Tổng số'),
(N'BkavThongKe.Summary.Total', N'zh-CN', N'Total'),
(N'BkavThongKe.Summary.Draft', N'en-US', N'Draft (not signed)'),
(N'BkavThongKe.Summary.Draft', N'vi-VN', N'Đã xuất nháp'),
(N'BkavThongKe.Summary.Draft', N'zh-CN', N'Draft'),
(N'BkavThongKe.Summary.Signed', N'en-US', N'Issued (signed)'),
(N'BkavThongKe.Summary.Signed', N'vi-VN', N'Đã ký'),
(N'BkavThongKe.Summary.Signed', N'zh-CN', N'Issued'),
(N'BkavThongKe.Summary.ThuHo', N'en-US', N'Collection (Thu ho)'),
(N'BkavThongKe.Summary.ThuHo', N'vi-VN', N'Thu hộ'),
(N'BkavThongKe.Summary.ThuHo', N'zh-CN', N'Collection'),

(N'BkavThongKe.Col.Status', N'en-US', N'Status'),
(N'BkavThongKe.Col.Status', N'vi-VN', N'Trạng thái'),
(N'BkavThongKe.Col.Status', N'zh-CN', N'Status'),
(N'BkavThongKe.Status.Draft', N'en-US', N'Draft'),
(N'BkavThongKe.Status.Draft', N'vi-VN', N'Nháp'),
(N'BkavThongKe.Status.Draft', N'zh-CN', N'Draft'),
(N'BkavThongKe.Status.WaitingSign', N'en-US', N'Waiting to sign'),
(N'BkavThongKe.Status.WaitingSign', N'vi-VN', N'Chờ ký'),
(N'BkavThongKe.Status.WaitingSign', N'zh-CN', N'Waiting to sign'),
(N'BkavThongKe.Status.Signed', N'en-US', N'Signed'),
(N'BkavThongKe.Status.Signed', N'vi-VN', N'Đã ký'),
(N'BkavThongKe.Status.Signed', N'zh-CN', N'Signed');

MERGE dbo.LocalizationResources AS target
USING @Resources AS source
    ON target.ResourceKey = source.ResourceKey
   AND target.Culture = source.Culture
WHEN MATCHED THEN
    UPDATE SET Value = source.Value
WHEN NOT MATCHED THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (source.ResourceKey, source.Culture, source.Value);
GO
