-- Menu permission for 5.13 E-Invoice (eHoadon)
-- Permission code == MenuName: E_Invoice
-- See = View (tìm kiếm, lịch sử HDDT), Add = xuất HDDT / Get Token, Del = xóa số HĐ online

IF NOT EXISTS (SELECT 1 FROM [MenuNames] WHERE MenuName = 'E_Invoice')
BEGIN
    INSERT INTO [MenuNames] (MenuID, MenuName, Title)
    VALUES (NEWID(), 'E_Invoice', '5.13 E-Invoice');
END
GO
