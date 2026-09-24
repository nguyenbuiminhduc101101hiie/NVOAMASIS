-- Nguồn số tiền cho từng dòng của nghiệp vụ hạch toán (dùng khi hạch toán tự động hóa đơn)
-- Giá trị: TOTAL (tổng sau thuế), NET (tiền trước thuế), TAX (tiền thuế), MANUAL (nhập tay từng hóa đơn). NULL = hệ thống tự đề xuất.
-- Chạy 1 lần trước khi chạy ứng dụng bản mới. Có thể chạy lại nhiều lần an toàn.
IF COL_LENGTH(N'dbo.TransactionTypeMappings', N'AmountSource') IS NULL
    ALTER TABLE [dbo].[TransactionTypeMappings] ADD [AmountSource] nvarchar(10) NULL;
GO
