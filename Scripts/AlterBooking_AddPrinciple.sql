-- 6.1 Booking: thêm thuộc tính Principle (lưu giống Agency Name).
-- Chạy trên database trước khi cập nhật code, vì model Booking đã có cột principle.
IF COL_LENGTH('dbo.CONTAINEROUTBOUNDNOTIFY_sale', 'principle') IS NULL
    ALTER TABLE dbo.CONTAINEROUTBOUNDNOTIFY_sale ADD principle NVARCHAR(MAX) NULL;
GO
