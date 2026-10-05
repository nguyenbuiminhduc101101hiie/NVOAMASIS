-- Thêm cột Carrier (hãng tàu) cho MBL, lấy từ Booking theo số Booking (MBL.Bkno).
-- BẮT BUỘC chạy trên SQL Server TRƯỚC khi deploy code mới (code đọc cột này ở mọi truy vấn MBL).

IF COL_LENGTH('dbo.MBL', 'Carrier') IS NULL
BEGIN
    ALTER TABLE dbo.MBL ADD Carrier nvarchar(max) NULL;
END
GO

-- Điền sẵn Carrier cho MBL cũ từ Booking (bản cập nhật mới nhất của mỗi số Booking).
UPDATE m
SET m.Carrier = bk.Carrier
FROM dbo.MBL m
CROSS APPLY (
    SELECT TOP 1 LTRIM(RTRIM(b.Carrier)) AS Carrier
    FROM dbo.CONTAINEROUTBOUNDNOTIFY_sale b
    WHERE b.BookingNo = LTRIM(RTRIM(m.Bkno))
      AND b.Carrier IS NOT NULL AND LTRIM(RTRIM(b.Carrier)) <> ''
    ORDER BY b.UpdateTime DESC
) bk
WHERE (m.Carrier IS NULL OR LTRIM(RTRIM(m.Carrier)) = '')
  AND m.Bkno IS NOT NULL AND LTRIM(RTRIM(m.Bkno)) <> '';
GO
