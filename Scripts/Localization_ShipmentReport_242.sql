-- Màn 2.4.2 Báo cáo lô hàng: tiêu đề cột ngày giao + chú thích ô khoảng thời gian.
-- Không bắt buộc (màn hình đã có câu mặc định); chạy để sửa được nội dung qua bảng dịch.
-- Chạy xong: DbStringLocalizerFactory.ClearCache() hoặc khởi động lại app.
BEGIN TRANSACTION;
DELETE FROM LocalizationResources WHERE ResourceKey IN ('Rpt242_ActualDeliveryDate','Rpt242_DateRangeHint');
INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Rpt242_ActualDeliveryDate','vi-VN',N'Ngày giao (ADD)'),
('Rpt242_ActualDeliveryDate','en-US',N'Actual Delivery (ADD)'),
('Rpt242_ActualDeliveryDate','zh-CN',N'实际交付日期 (ADD)'),
('Rpt242_DateRangeHint','vi-VN',N'Lọc theo Date Report của HBL'),
('Rpt242_DateRangeHint','en-US',N'Filtered by HBL Date Report'),
('Rpt242_DateRangeHint','zh-CN',N'按 HBL 报告日期 (Date Report) 筛选');
COMMIT TRANSACTION;

-- Cột chứng từ (Số HĐ online / Số phiếu thu / Số phiếu chi). Không bắt buộc.
BEGIN TRANSACTION;
DELETE FROM LocalizationResources WHERE ResourceKey IN ('Rpt242_EInvoiceNo','Rpt242_ReceiptNo','Rpt242_PaymentNo');
INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Rpt242_EInvoiceNo','vi-VN',N'Số HĐ online'),
('Rpt242_EInvoiceNo','en-US',N'E-Invoice No.'),
('Rpt242_EInvoiceNo','zh-CN',N'电子发票号'),
('Rpt242_ReceiptNo','vi-VN',N'Số phiếu thu'),
('Rpt242_ReceiptNo','en-US',N'Receipt No.'),
('Rpt242_ReceiptNo','zh-CN',N'收款单号'),
('Rpt242_PaymentNo','vi-VN',N'Số phiếu chi'),
('Rpt242_PaymentNo','en-US',N'Payment No.'),
('Rpt242_PaymentNo','zh-CN',N'付款单号');
COMMIT TRANSACTION;

-- Cột Agent / Loại hàng / Đối tượng Credit / Profit NVOCC. Không bắt buộc.
BEGIN TRANSACTION;
DELETE FROM LocalizationResources WHERE ResourceKey IN ('Rpt242_Agent','Rpt242_CargoKind','Rpt242_CreditParties','Rpt242_ProfitNvocc');
INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Rpt242_Agent','vi-VN',N'Agent'),('Rpt242_Agent','en-US',N'Agent'),('Rpt242_Agent','zh-CN',N'代理'),
('Rpt242_CargoKind','vi-VN',N'Loại hàng'),('Rpt242_CargoKind','en-US',N'Freight / NVOCC'),('Rpt242_CargoKind','zh-CN',N'货物类型'),
('Rpt242_CreditParties','vi-VN',N'Đối tượng Credit'),('Rpt242_CreditParties','en-US',N'Credit Parties'),('Rpt242_CreditParties','zh-CN',N'应付对象'),
('Rpt242_ProfitNvocc','vi-VN',N'Profit NVOCC'),('Rpt242_ProfitNvocc','en-US',N'Profit NVOCC'),('Rpt242_ProfitNvocc','zh-CN',N'NVOCC 利润');
COMMIT TRANSACTION;

-- Cột Carrier (từ Booking của HBL). Không bắt buộc.
DELETE FROM LocalizationResources WHERE ResourceKey = 'Rpt242_Carrier';
INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Rpt242_Carrier','vi-VN',N'Carrier'),('Rpt242_Carrier','en-US',N'Carrier'),('Rpt242_Carrier','zh-CN',N'承运人');

-- Cột Số Booking. Không bắt buộc.
DELETE FROM LocalizationResources WHERE ResourceKey = 'Rpt242_BookingNo';
INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Rpt242_BookingNo','vi-VN',N'Số Booking'),('Rpt242_BookingNo','en-US',N'Booking No.'),('Rpt242_BookingNo','zh-CN',N'订舱号');
