/* =====================================================================
   Kiểm tra trước khi bật tự tạo chứng từ kế toán cho phiếu thu / phiếu chi
   (PhieuThuChiVoucherService). Chỉ ĐỌC dữ liệu, không sửa gì.
   Chạy trên từng database tenant.
   ===================================================================== */

-- 1. Hai thủ tục mà nút "Tạo Voucher" đang dùng có tồn tại và đủ tham số không?
SELECT OBJECT_NAME(object_id) AS ThuTuc, name AS ThamSo, TYPE_NAME(user_type_id) AS KieuDuLieu
FROM sys.parameters
WHERE object_id IN (OBJECT_ID('dbo.usp_CreateAccountingVoucher_FromPhieuThu'),
                    OBJECT_ID('dbo.usp_CreateAccountingVoucher_FromPhieuChi'))
ORDER BY ThuTuc, parameter_id;
-- Cần thấy: @PhieuthuID / @PhieuchiID, @CompanyId, @CreatedBy, @AutoPost, @ClearOld

-- 2. Thủ tục có xử lý @AutoPost = 0 (tạo nháp, không ghi sổ) không?
--    Mở định nghĩa và tìm đoạn dùng @AutoPost: khi = 0 phải để Status = 1, Ghiso = 0 và KHÔNG ghi GeneralLedgerEntries.
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.usp_CreateAccountingVoucher_FromPhieuThu')) AS DinhNghia_PhieuThu;
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.usp_CreateAccountingVoucher_FromPhieuChi')) AS DinhNghia_PhieuChi;

-- 3. Chứng từ do thủ tục tạo có gắn SourceId = Id phiếu không?
--    (Phần mềm dựa vào SourceId để biết phiếu đã có chứng từ và để hủy chứng từ khi bỏ duyệt.)
SELECT TOP 20 'Thu' AS Loai, p.SoPhieuthu AS SoPhieu, v.VoucherNo, v.SourceModule, v.SourceId, v.Status, v.Ghiso
FROM AccountingVouchers v
JOIN Phieuthu p ON v.SourceId = CAST(p.PhieuthuID AS nvarchar(50))
UNION ALL
SELECT TOP 20 'Chi', c.Sophieuchi, v.VoucherNo, v.SourceModule, v.SourceId, v.Status, v.Ghiso
FROM AccountingVouchers v
JOIN Phieuchi c ON v.SourceId = CAST(c.PhieuchiID AS nvarchar(50));
-- Nếu không ra dòng nào dù đã từng bấm "Tạo Voucher": thủ tục gắn liên kết theo cách khác → báo lại để chỉnh service.

-- 4. Phiếu đã duyệt nhưng chưa có chứng từ (sẽ hiện "Chưa hạch toán" và được nút "Hạch toán nháp" xử lý)
SELECT 'Thu' AS Loai, p.SoPhieuthu AS SoPhieu, ISNULL(p.Ngayhachtoan, p.Ngay) AS NgayHachToan, p.Sotien, p.Currency, p.TKNo, p.TKCo
FROM Phieuthu p
WHERE p.Approve = 1
  AND NOT EXISTS (SELECT 1 FROM AccountingVouchers v
                  WHERE v.SourceId = CAST(p.PhieuthuID AS nvarchar(50)) AND v.Status <> 3)
UNION ALL
SELECT 'Chi', c.Sophieuchi, c.Ngay, c.Sotien, c.Currency, c.TKnophieuchi, c.TKCophieuchi
FROM Phieuchi c
WHERE c.Approve = 1
  AND NOT EXISTS (SELECT 1 FROM AccountingVouchers v
                  WHERE v.SourceId = CAST(c.PhieuchiID AS nvarchar(50)) AND v.Status <> 3)
ORDER BY NgayHachToan;
