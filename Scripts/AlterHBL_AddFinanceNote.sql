-- Ghi chú tài chính của lô hàng (HBL): tổng hợp tick thanh toán Debit/Credit,
-- phiếu thu/chi đã duyệt/thanh toán/hạch toán, hoá đơn đã phát hành/hạch toán.
-- BẮT BUỘC chạy trên SQL Server TRƯỚC khi deploy code mới.

IF COL_LENGTH('dbo.HBL', 'financeNote') IS NULL
BEGIN
    ALTER TABLE dbo.HBL ADD financeNote nvarchar(max) NULL;
END
GO

IF COL_LENGTH('dbo.HBL', 'financeNoteDate') IS NULL
BEGIN
    ALTER TABLE dbo.HBL ADD financeNoteDate datetime NULL;
END
GO
