/*
  Batch 2 — Menu 10 localization:
  - 10.4.1 AccountingVouchers_5_8_4_Index.razor
  - 10.4.2 AccountingVoucherExcelImport.razor
  - 10.4.3 AccountingVoucherLines_10_4_3_Index.razor
  - 10.6.0 GeneralLedgerEntries_5_8_6_Index.razor (UI chrome + extra columns)

  Depends on: ImportGeneralLedgerEntriesLocalizationResources.sql (column keys)
  Safe to re-run.
*/
SET NOCOUNT ON;
BEGIN TRANSACTION;

;WITH src AS (
    -- 10.4.3 Voucher lines
    SELECT N'Vl1043.Title' AS ResourceKey, N'vi-VN' AS Culture, N'10.4.3 Dòng chứng từ · Tax / Management Book' AS Value UNION ALL
    SELECT N'Vl1043.Title', N'en-US', N'10.4.3 Voucher lines · Tax / Management Book' UNION ALL
    SELECT N'Vl1043.Subtitle', N'vi-VN', N'Xem AccountingVoucherLines · chỉ sửa IsTaxBook / IsManagementBook trên lưới' UNION ALL
    SELECT N'Vl1043.Subtitle', N'en-US', N'View AccountingVoucherLines · edit IsTaxBook / IsManagementBook on grid only' UNION ALL
    SELECT N'Vl1043.Load', N'vi-VN', N'Tải' UNION ALL
    SELECT N'Vl1043.Load', N'en-US', N'Load' UNION ALL
    SELECT N'Vl1043.Reset', N'vi-VN', N'Đặt lại' UNION ALL
    SELECT N'Vl1043.Reset', N'en-US', N'Reset' UNION ALL
    SELECT N'Vl1043.FromVoucherDate', N'vi-VN', N'Từ ngày CT' UNION ALL
    SELECT N'Vl1043.FromVoucherDate', N'en-US', N'Voucher date from' UNION ALL
    SELECT N'Vl1043.ToVoucherDate', N'vi-VN', N'Đến ngày CT' UNION ALL
    SELECT N'Vl1043.ToVoucherDate', N'en-US', N'Voucher date to' UNION ALL
    SELECT N'Vl1043.VoucherNo', N'vi-VN', N'Số chứng từ' UNION ALL
    SELECT N'Vl1043.VoucherNo', N'en-US', N'Voucher no' UNION ALL
    SELECT N'Vl1043.Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'Vl1043.Account', N'en-US', N'Account' UNION ALL
    SELECT N'Vl1043.InvoiceNo', N'vi-VN', N'Invoice No / SoHD' UNION ALL
    SELECT N'Vl1043.InvoiceNo', N'en-US', N'Invoice No / SoHD' UNION ALL
    SELECT N'Vl1043.BookFilter', N'vi-VN', N'Book filter' UNION ALL
    SELECT N'Vl1043.BookFilter', N'en-US', N'Book filter' UNION ALL
    SELECT N'Vl1043.Book_ALL', N'vi-VN', N'ALL' UNION ALL
    SELECT N'Vl1043.Book_ALL', N'en-US', N'ALL' UNION ALL
    SELECT N'Vl1043.Book_TAX', N'vi-VN', N'Tax only' UNION ALL
    SELECT N'Vl1043.Book_TAX', N'en-US', N'Tax only' UNION ALL
    SELECT N'Vl1043.Book_MGMT', N'vi-VN', N'Management only' UNION ALL
    SELECT N'Vl1043.Book_MGMT', N'en-US', N'Management only' UNION ALL
    SELECT N'Vl1043.Book_NONE', N'vi-VN', N'Chưa gán book' UNION ALL
    SELECT N'Vl1043.Book_NONE', N'en-US', N'No book assigned' UNION ALL
    SELECT N'Vl1043.VoucherStatus', N'vi-VN', N'Trạng thái CT' UNION ALL
    SELECT N'Vl1043.VoucherStatus', N'en-US', N'Voucher status' UNION ALL
    SELECT N'Vl1043.Search', N'vi-VN', N'Tìm (số CT, diễn giải, TK, HBL, Invoice...)' UNION ALL
    SELECT N'Vl1043.Search', N'en-US', N'Search (voucher no, description, account, HBL, invoice...)' UNION ALL
    SELECT N'Vl1043.PermissionHint', N'vi-VN', N'Quyền menu Modify_voucherlines: View để xem · Edit để tick sửa book trên lưới.' UNION ALL
    SELECT N'Vl1043.PermissionHint', N'en-US', N'Menu permission Modify_voucherlines: View to browse · Edit to toggle book flags on grid.' UNION ALL
    SELECT N'Vl1043.ViewOnly', N'vi-VN', N'(Bạn chỉ được xem)' UNION ALL
    SELECT N'Vl1043.ViewOnly', N'en-US', N'(View only)' UNION ALL
    SELECT N'Vl1043.NoRows', N'vi-VN', N'Không có dòng chứng từ theo điều kiện lọc.' UNION ALL
    SELECT N'Vl1043.NoRows', N'en-US', N'No voucher lines match the filter.' UNION ALL
    SELECT N'Vl1043.NoPermissionView', N'vi-VN', N'Bạn không có quyền xem (Modify_voucherlines / View).' UNION ALL
    SELECT N'Vl1043.NoPermissionView', N'en-US', N'You do not have view permission (Modify_voucherlines / View).' UNION ALL
    SELECT N'Vl1043.NoPermissionEdit', N'vi-VN', N'Bạn không có quyền Edit trên Modify_voucherlines.' UNION ALL
    SELECT N'Vl1043.NoPermissionEdit', N'en-US', N'You do not have edit permission on Modify_voucherlines.' UNION ALL
    SELECT N'Vl1043.LoadedCount', N'vi-VN', N'Đã tải {0} dòng chứng từ.' UNION ALL
    SELECT N'Vl1043.LoadedCount', N'en-US', N'Loaded {0} voucher lines.' UNION ALL
    SELECT N'Vl1043.RowNotFound', N'vi-VN', N'Không tìm thấy dòng chứng từ.' UNION ALL
    SELECT N'Vl1043.RowNotFound', N'en-US', N'Voucher line not found.' UNION ALL
    SELECT N'Vl1043.BookUpdated', N'vi-VN', N'Đã cập nhật book: {0} / line {1}.' UNION ALL
    SELECT N'Vl1043.BookUpdated', N'en-US', N'Book updated: {0} / line {1}.' UNION ALL
    SELECT N'Vl1043.Col_VoucherDate', N'vi-VN', N'Ngày CT' UNION ALL
    SELECT N'Vl1043.Col_VoucherDate', N'en-US', N'Voucher date' UNION ALL
    SELECT N'Vl1043.Col_PostingDate', N'vi-VN', N'Ngày HT' UNION ALL
    SELECT N'Vl1043.Col_PostingDate', N'en-US', N'Posting date' UNION ALL
    SELECT N'Vl1043.Col_TransType', N'vi-VN', N'Loại CT' UNION ALL
    SELECT N'Vl1043.Col_TransType', N'en-US', N'Trans. type' UNION ALL
    SELECT N'Vl1043.Col_VoucherStatus', N'vi-VN', N'TT CT' UNION ALL
    SELECT N'Vl1043.Col_VoucherStatus', N'en-US', N'Voucher status' UNION ALL
    SELECT N'Vl1043.Col_Description', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'Vl1043.Col_Description', N'en-US', N'Description' UNION ALL

    -- 10.4.1 Accounting vouchers
    SELECT N'Av584.Title', N'vi-VN', N'10.4 Chứng Từ Kế Toán' UNION ALL
    SELECT N'Av584.Title', N'en-US', N'10.4 Accounting vouchers' UNION ALL
    SELECT N'Av584.Subtitle', N'vi-VN', N'Nhập liệu và quản lý phiếu kế toán kiểu MISA' UNION ALL
    SELECT N'Av584.Subtitle', N'en-US', N'Enter and manage MISA-style accounting vouchers' UNION ALL
    SELECT N'Av584.Add', N'vi-VN', N'Thêm chứng từ' UNION ALL
    SELECT N'Av584.Add', N'en-US', N'Add voucher' UNION ALL
    SELECT N'Av584.Edit', N'vi-VN', N'Sửa' UNION ALL
    SELECT N'Av584.Edit', N'en-US', N'Edit' UNION ALL
    SELECT N'Av584.Approve', N'vi-VN', N'Approve' UNION ALL
    SELECT N'Av584.Approve', N'en-US', N'Approve' UNION ALL
    SELECT N'Av584.Unapprove', N'vi-VN', N'Hủy Approve' UNION ALL
    SELECT N'Av584.Unapprove', N'en-US', N'Unapprove' UNION ALL
    SELECT N'Av584.Unpost', N'vi-VN', N'Hủy ghi sổ' UNION ALL
    SELECT N'Av584.Unpost', N'en-US', N'Unpost' UNION ALL
    SELECT N'Av584.Kpi_Total', N'vi-VN', N'Tổng chứng từ' UNION ALL
    SELECT N'Av584.Kpi_Total', N'en-US', N'Total vouchers' UNION ALL
    SELECT N'Av584.Kpi_TotalHint', N'vi-VN', N'Trong dữ liệu đang tải' UNION ALL
    SELECT N'Av584.Kpi_TotalHint', N'en-US', N'In loaded data' UNION ALL
    SELECT N'Av584.Kpi_Posted', N'vi-VN', N'Đã ghi sổ' UNION ALL
    SELECT N'Av584.Kpi_Posted', N'en-US', N'Posted' UNION ALL
    SELECT N'Av584.Kpi_PostedHint', N'vi-VN', N'Dòng màu đỏ, không sửa/xóa' UNION ALL
    SELECT N'Av584.Kpi_PostedHint', N'en-US', N'Red rows — cannot edit/delete' UNION ALL
    SELECT N'Av584.Kpi_TotalDebit', N'vi-VN', N'Tổng Nợ lines' UNION ALL
    SELECT N'Av584.Kpi_TotalDebit', N'en-US', N'Total debit (lines)' UNION ALL
    SELECT N'Av584.Kpi_TotalCredit', N'vi-VN', N'Tổng Có lines' UNION ALL
    SELECT N'Av584.Kpi_TotalCredit', N'en-US', N'Total credit (lines)' UNION ALL
    SELECT N'Av584.Kpi_FromLines', N'vi-VN', N'Từ AccountingVoucherLines' UNION ALL
    SELECT N'Av584.Kpi_FromLines', N'en-US', N'From AccountingVoucherLines' UNION ALL
    SELECT N'Av584.Kpi_Diff', N'vi-VN', N'Chênh lệch: {0}' UNION ALL
    SELECT N'Av584.Kpi_Diff', N'en-US', N'Difference: {0}' UNION ALL
    SELECT N'Av584.Search', N'vi-VN', N'Tìm số CT, khách hàng, tài khoản, diễn giải...' UNION ALL
    SELECT N'Av584.Search', N'en-US', N'Search voucher no, customer, account, description...' UNION ALL
    SELECT N'Av584.NoSelection', N'vi-VN', N'Chưa chọn chứng từ' UNION ALL
    SELECT N'Av584.NoSelection', N'en-US', N'No voucher selected' UNION ALL
    SELECT N'Av584.Selected', N'vi-VN', N'Đang chọn: {0}' UNION ALL
    SELECT N'Av584.Selected', N'en-US', N'Selected: {0}' UNION ALL
    SELECT N'Av584.Col_Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'Av584.Col_Status', N'en-US', N'Status' UNION ALL
    SELECT N'Av584.Col_Posted', N'vi-VN', N'Ghi sổ' UNION ALL
    SELECT N'Av584.Col_Posted', N'en-US', N'Posted' UNION ALL
    SELECT N'Av584.Col_Party', N'vi-VN', N'Đối tượng / công ty' UNION ALL
    SELECT N'Av584.Col_Party', N'en-US', N'Party / company' UNION ALL
    SELECT N'Av584.Col_AccountAmount', N'vi-VN', N'TK / Số tiền' UNION ALL
    SELECT N'Av584.Col_AccountAmount', N'en-US', N'Account / amount' UNION ALL
    SELECT N'Av584.Col_TotalDebit', N'vi-VN', N'Tổng Nợ Lines' UNION ALL
    SELECT N'Av584.Col_TotalDebit', N'en-US', N'Total debit lines' UNION ALL
    SELECT N'Av584.Col_TotalCredit', N'vi-VN', N'Tổng Có Lines' UNION ALL
    SELECT N'Av584.Col_TotalCredit', N'en-US', N'Total credit lines' UNION ALL
    SELECT N'Av584.Col_Balance', N'vi-VN', N'Cân đối' UNION ALL
    SELECT N'Av584.Col_Balance', N'en-US', N'Balance' UNION ALL
    SELECT N'Av584.Col_PostingDate', N'vi-VN', N'Ngày hạch toán' UNION ALL
    SELECT N'Av584.Col_PostingDate', N'en-US', N'Posting date' UNION ALL
    SELECT N'Av584.Col_Period', N'vi-VN', N'Kỳ' UNION ALL
    SELECT N'Av584.Col_Period', N'en-US', N'Period' UNION ALL
    SELECT N'Av584.Col_TransType', N'vi-VN', N'Loại nghiệp vụ' UNION ALL
    SELECT N'Av584.Col_TransType', N'en-US', N'Transaction type' UNION ALL
    SELECT N'Av584.Col_ReferenceNo', N'vi-VN', N'Số tham chiếu' UNION ALL
    SELECT N'Av584.Col_ReferenceNo', N'en-US', N'Reference no' UNION ALL
    SELECT N'Av584.Col_ReferenceDate', N'vi-VN', N'Ngày tham chiếu' UNION ALL
    SELECT N'Av584.Col_ReferenceDate', N'en-US', N'Reference date' UNION ALL
    SELECT N'Av584.Col_Source', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'Av584.Col_Source', N'en-US', N'Source' UNION ALL
    SELECT N'Av584.Col_ModifiedBy', N'vi-VN', N'Người sửa' UNION ALL
    SELECT N'Av584.Col_ModifiedBy', N'en-US', N'Modified by' UNION ALL
    SELECT N'Av584.Col_ModifiedDate', N'vi-VN', N'Ngày sửa' UNION ALL
    SELECT N'Av584.Col_ModifiedDate', N'en-US', N'Modified date' UNION ALL
    SELECT N'Av584.Col_PostedBy', N'vi-VN', N'Người ghi sổ' UNION ALL
    SELECT N'Av584.Col_PostedBy', N'en-US', N'Posted by' UNION ALL
    SELECT N'Av584.Col_PostedDate', N'vi-VN', N'Ngày ghi sổ' UNION ALL
    SELECT N'Av584.Col_PostedDate', N'en-US', N'Posted date' UNION ALL
    SELECT N'Av584.NoLines', N'vi-VN', N'Chứng từ này chưa có dòng bút toán.' UNION ALL
    SELECT N'Av584.NoLines', N'en-US', N'This voucher has no journal lines yet.' UNION ALL
    SELECT N'Av584.Balance_None', N'vi-VN', N'Chưa có dòng' UNION ALL
    SELECT N'Av584.Balance_None', N'en-US', N'No lines' UNION ALL
    SELECT N'Av584.Balance_Ok', N'vi-VN', N'Cân' UNION ALL
    SELECT N'Av584.Balance_Ok', N'en-US', N'Balanced' UNION ALL
    SELECT N'Av584.Posted_Open', N'vi-VN', N'Open' UNION ALL
    SELECT N'Av584.Posted_Open', N'en-US', N'Open' UNION ALL
    SELECT N'Av584.Posted_Yes', N'vi-VN', N'Posted' UNION ALL
    SELECT N'Av584.Posted_Yes', N'en-US', N'Posted' UNION ALL
    SELECT N'Av584.AlreadyApproved', N'vi-VN', N'Chứng từ đã được Approve.' UNION ALL
    SELECT N'Av584.AlreadyApproved', N'en-US', N'Voucher is already approved.' UNION ALL
    SELECT N'Av584.NoApprovePermission', N'vi-VN', N'Không có quyền Approve.' UNION ALL
    SELECT N'Av584.NoApprovePermission', N'en-US', N'No approve permission.' UNION ALL
    SELECT N'Av584.ApprovedSuccess', N'vi-VN', N'Đã Approve.' UNION ALL
    SELECT N'Av584.ApprovedSuccess', N'en-US', N'Approved.' UNION ALL
    SELECT N'Av584.UnapprovedSuccess', N'vi-VN', N'Đã hủy Approve.' UNION ALL
    SELECT N'Av584.UnapprovedSuccess', N'en-US', N'Unapproved.' UNION ALL
    SELECT N'Av584.UnpostAdminOnly', N'vi-VN', N'Chỉ ADMIN hoặc MANAGER mới được hủy ghi sổ.' UNION ALL
    SELECT N'Av584.UnpostAdminOnly', N'en-US', N'Only ADMIN or MANAGER can unpost.' UNION ALL
    SELECT N'Av584.NotPostedYet', N'vi-VN', N'Chứng từ chưa ghi sổ, không cần hủy ghi sổ.' UNION ALL
    SELECT N'Av584.NotPostedYet', N'en-US', N'Voucher is not posted; unpost not needed.' UNION ALL
    SELECT N'Av584.UnpostTitle', N'vi-VN', N'Hủy ghi sổ' UNION ALL
    SELECT N'Av584.UnpostTitle', N'en-US', N'Unpost' UNION ALL
    SELECT N'Av584.UnpostConfirm', N'vi-VN', N'Hủy ghi sổ sẽ chuyển chứng từ về trạng thái Draft/Open và xóa các dòng đã post trong GeneralLedgerEntries của chứng từ này. Bạn có chắc muốn tiếp tục không?' UNION ALL
    SELECT N'Av584.UnpostConfirm', N'en-US', N'Unposting will revert the voucher to Draft/Open and delete its GeneralLedgerEntries rows. Continue?' UNION ALL
    SELECT N'Av584.UnpostSuccess', N'vi-VN', N'Đã hủy ghi sổ. Chứng từ đã chuyển về Draft/Open. Nếu chứng từ đang Approve thì cần Hủy Approve trước khi sửa.' UNION ALL
    SELECT N'Av584.UnpostSuccess', N'en-US', N'Unposted. Voucher reverted to Draft/Open. Unapprove first if you need to edit an approved voucher.' UNION ALL
    SELECT N'Av584.UnpostFailed', N'vi-VN', N'Không thể hủy ghi sổ: {0}' UNION ALL
    SELECT N'Av584.UnpostFailed', N'en-US', N'Cannot unpost: {0}' UNION ALL

    -- 10.4.2 Voucher Excel import (UI + snackbars)
    SELECT N'AvImp.Title', N'vi-VN', N'Import voucher từ sổ công nợ' UNION ALL
    SELECT N'AvImp.Title', N'en-US', N'Import vouchers from debt ledger' UNION ALL
    SELECT N'AvImp.Heading', N'vi-VN', N'Import voucher từ sổ chi tiết công nợ' UNION ALL
    SELECT N'AvImp.Heading', N'en-US', N'Import vouchers from detailed debt ledger' UNION ALL
    SELECT N'AvImp.Subtitle', N'vi-VN', N'Đọc file sổ 131/331, gom theo số chứng từ và tạo AccountingVouchers + AccountingVoucherLines ở trạng thái nháp.' UNION ALL
    SELECT N'AvImp.Subtitle', N'en-US', N'Read 131/331 ledger Excel, group by voucher no and create draft AccountingVouchers + lines.' UNION ALL
    SELECT N'AvImp.NoAutoPost', N'vi-VN', N'Không tự ghi sổ' UNION ALL
    SELECT N'AvImp.NoAutoPost', N'en-US', N'No auto-posting' UNION ALL
    SELECT N'AvImp.Step1', N'vi-VN', N'1. Chọn file Excel' UNION ALL
    SELECT N'AvImp.Step1', N'en-US', N'1. Select Excel file' UNION ALL
    SELECT N'AvImp.Step1Hint', N'vi-VN', N'Hỗ trợ đúng mẫu có 2 dòng tiêu đề "Ngày tháng chứng từ / Sổ phát sinh".' UNION ALL
    SELECT N'AvImp.Step1Hint', N'en-US', N'Supports template with header rows "Voucher date / Ledger".' UNION ALL
    SELECT N'AvImp.ChooseFile', N'vi-VN', N'Chọn file .xlsx' UNION ALL
    SELECT N'AvImp.ChooseFile', N'en-US', N'Choose .xlsx file' UNION ALL
    SELECT N'AvImp.MaxSize', N'vi-VN', N'Tối đa 20 MB' UNION ALL
    SELECT N'AvImp.MaxSize', N'en-US', N'Max 20 MB' UNION ALL
    SELECT N'AvImp.FileName', N'vi-VN', N'Tên file:' UNION ALL
    SELECT N'AvImp.FileName', N'en-US', N'File name:' UNION ALL
    SELECT N'AvImp.FileSize', N'vi-VN', N'Dung lượng:' UNION ALL
    SELECT N'AvImp.FileSize', N'en-US', N'Size:' UNION ALL
    SELECT N'AvImp.Sheet', N'vi-VN', N'Sheet:' UNION ALL
    SELECT N'AvImp.Sheet', N'en-US', N'Sheet:' UNION ALL
    SELECT N'AvImp.HeaderRow', N'vi-VN', N'Dòng tiêu đề:' UNION ALL
    SELECT N'AvImp.HeaderRow', N'en-US', N'Header row:' UNION ALL
    SELECT N'AvImp.LedgerAccount', N'vi-VN', N'Tài khoản sổ:' UNION ALL
    SELECT N'AvImp.LedgerAccount', N'en-US', N'Ledger account:' UNION ALL
    SELECT N'AvImp.Customer', N'vi-VN', N'Khách hàng:' UNION ALL
    SELECT N'AvImp.Customer', N'en-US', N'Customer:' UNION ALL
    SELECT N'AvImp.Step2', N'vi-VN', N'2. Thiết lập chứng từ' UNION ALL
    SELECT N'AvImp.Step2', N'en-US', N'2. Voucher settings' UNION ALL
    SELECT N'AvImp.Step2Hint', N'vi-VN', N'Các voucher được tạo với Status = 0, Ghiso = 0, Approve = 0.' UNION ALL
    SELECT N'AvImp.Step2Hint', N'en-US', N'Vouchers created with Status = 0, Ghiso = 0, Approve = 0.' UNION ALL
    SELECT N'AvImp.CheckDuplicate', N'vi-VN', N'Kiểm tra trùng' UNION ALL
    SELECT N'AvImp.CheckDuplicate', N'en-US', N'Check duplicates' UNION ALL
    SELECT N'AvImp.TransType', N'vi-VN', N'Loại chứng từ *' UNION ALL
    SELECT N'AvImp.TransType', N'en-US', N'Transaction type *' UNION ALL
    SELECT N'AvImp.Currency', N'vi-VN', N'Tiền tệ' UNION ALL
    SELECT N'AvImp.Currency', N'en-US', N'Currency' UNION ALL
    SELECT N'AvImp.DebtAccount', N'vi-VN', N'TK công nợ' UNION ALL
    SELECT N'AvImp.DebtAccount', N'en-US', N'Debt account' UNION ALL
    SELECT N'AvImp.CreatedBy', N'vi-VN', N'Người tạo' UNION ALL
    SELECT N'AvImp.CreatedBy', N'en-US', N'Created by' UNION ALL
    SELECT N'AvImp.TaxBook', N'vi-VN', N'Sổ thuế' UNION ALL
    SELECT N'AvImp.TaxBook', N'en-US', N'Tax book' UNION ALL
    SELECT N'AvImp.MgmtBook', N'vi-VN', N'Sổ quản trị' UNION ALL
    SELECT N'AvImp.MgmtBook', N'en-US', N'Management book' UNION ALL
    SELECT N'AvImp.Kpi_Vouchers', N'vi-VN', N'Voucher nhận diện' UNION ALL
    SELECT N'AvImp.Kpi_Vouchers', N'en-US', N'Vouchers detected' UNION ALL
    SELECT N'AvImp.Kpi_Selected', N'vi-VN', N'Voucher đã chọn' UNION ALL
    SELECT N'AvImp.Kpi_Selected', N'en-US', N'Vouchers selected' UNION ALL
    SELECT N'AvImp.Kpi_TotalDebit', N'vi-VN', N'Tổng Nợ' UNION ALL
    SELECT N'AvImp.Kpi_TotalDebit', N'en-US', N'Total debit' UNION ALL
    SELECT N'AvImp.Kpi_TotalCredit', N'vi-VN', N'Tổng Có' UNION ALL
    SELECT N'AvImp.Kpi_TotalCredit', N'en-US', N'Total credit' UNION ALL
    SELECT N'AvImp.Step3', N'vi-VN', N'3. Preview voucher' UNION ALL
    SELECT N'AvImp.Step3', N'en-US', N'3. Preview vouchers' UNION ALL
    SELECT N'AvImp.SelectValid', N'vi-VN', N'Chọn hợp lệ' UNION ALL
    SELECT N'AvImp.SelectValid', N'en-US', N'Select valid' UNION ALL
    SELECT N'AvImp.ClearSelection', N'vi-VN', N'Bỏ chọn' UNION ALL
    SELECT N'AvImp.ClearSelection', N'en-US', N'Clear selection' UNION ALL
    SELECT N'AvImp.SearchPlaceholder', N'vi-VN', N'Tìm voucher, hóa đơn, diễn giải...' UNION ALL
    SELECT N'AvImp.SearchPlaceholder', N'en-US', N'Search voucher, invoice, description...' UNION ALL
    SELECT N'AvImp.NoPreview', N'vi-VN', N'Chưa có voucher để xem trước' UNION ALL
    SELECT N'AvImp.NoPreview', N'en-US', N'No vouchers to preview' UNION ALL
    SELECT N'AvImp.NoPreviewHint', N'vi-VN', N'Chọn file Excel ở bước 1.' UNION ALL
    SELECT N'AvImp.NoPreviewHint', N'en-US', N'Select an Excel file in step 1.' UNION ALL
    SELECT N'AvImp.Status_Exists', N'vi-VN', N'Đã tồn tại' UNION ALL
    SELECT N'AvImp.Status_Exists', N'en-US', N'Exists' UNION ALL
    SELECT N'AvImp.Status_Error', N'vi-VN', N'Lỗi' UNION ALL
    SELECT N'AvImp.Status_Error', N'en-US', N'Error' UNION ALL
    SELECT N'AvImp.Status_Valid', N'vi-VN', N'Hợp lệ' UNION ALL
    SELECT N'AvImp.Status_Valid', N'en-US', N'Valid' UNION ALL
    SELECT N'AvImp.Step4', N'vi-VN', N'4. Các dòng AccountingVoucherLines sẽ tạo' UNION ALL
    SELECT N'AvImp.Step4', N'en-US', N'4. AccountingVoucherLines to be created' UNION ALL
    SELECT N'AvImp.FooterHint', N'vi-VN', N'Import chỉ tạo chứng từ nháp. Sau khi kiểm tra, dùng quy trình duyệt/post hiện tại để sinh GeneralLedgerEntries.' UNION ALL
    SELECT N'AvImp.FooterHint', N'en-US', N'Import creates draft vouchers only. Use existing approve/post flow for GeneralLedgerEntries.' UNION ALL
    SELECT N'AvImp.Reset', N'vi-VN', N'Làm lại' UNION ALL
    SELECT N'AvImp.Reset', N'en-US', N'Reset' UNION ALL
    SELECT N'AvImp.CreateDraft', N'vi-VN', N'Tạo {0} voucher nháp' UNION ALL
    SELECT N'AvImp.CreateDraft', N'en-US', N'Create {0} draft voucher(s)' UNION ALL
    SELECT N'AvImp.OnlyXlsx', N'vi-VN', N'Chỉ hỗ trợ file Excel .xlsx.' UNION ALL
    SELECT N'AvImp.OnlyXlsx', N'en-US', N'Only .xlsx Excel files are supported.' UNION ALL
    SELECT N'AvImp.FileTooLarge', N'vi-VN', N'File vượt quá 20 MB.' UNION ALL
    SELECT N'AvImp.FileTooLarge', N'en-US', N'File exceeds 20 MB.' UNION ALL
    SELECT N'AvImp.ReadSuccess', N'vi-VN', N'Đã nhận diện {0} dòng Excel và {1} voucher.' UNION ALL
    SELECT N'AvImp.ReadSuccess', N'en-US', N'Detected {0} Excel rows and {1} voucher(s).' UNION ALL
    SELECT N'AvImp.ReadFailed', N'vi-VN', N'Không đọc được file. Kiểm tra lại đúng mẫu sổ chi tiết công nợ.' UNION ALL
    SELECT N'AvImp.ReadFailed', N'en-US', N'Cannot read file. Check detailed debt ledger template.' UNION ALL
    SELECT N'AvImp.InvalidCompanyId', N'vi-VN', N'CompanyId không đúng định dạng GUID.' UNION ALL
    SELECT N'AvImp.InvalidCompanyId', N'en-US', N'CompanyId is not a valid GUID.' UNION ALL
    SELECT N'AvImp.NoDuplicate', N'vi-VN', N'Không phát hiện voucher trùng trong database.' UNION ALL
    SELECT N'AvImp.NoDuplicate', N'en-US', N'No duplicate vouchers found in database.' UNION ALL
    SELECT N'AvImp.DuplicateFound', N'vi-VN', N'Có {0} voucher đã tồn tại và đã được bỏ chọn.' UNION ALL
    SELECT N'AvImp.DuplicateFound', N'en-US', N'{0} existing voucher(s) found and deselected.' UNION ALL
    SELECT N'AvImp.CheckDuplicateFailed', N'vi-VN', N'Không kiểm tra được voucher trùng: {0}' UNION ALL
    SELECT N'AvImp.CheckDuplicateFailed', N'en-US', N'Cannot check duplicates: {0}' UNION ALL
    SELECT N'AvImp.SaveSuccess', N'vi-VN', N'Đã tạo thành công {0} voucher nháp, gồm {1} dòng hạch toán.' UNION ALL
    SELECT N'AvImp.SaveSuccess', N'en-US', N'Created {0} draft voucher(s) with {1} journal line(s).' UNION ALL
    SELECT N'AvImp.SaveFailed', N'vi-VN', N'Import thất bại, toàn bộ giao dịch đã rollback: {0}' UNION ALL
    SELECT N'AvImp.SaveFailed', N'en-US', N'Import failed, transaction rolled back: {0}' UNION ALL
    SELECT N'AvImp.Aggregated', N'vi-VN', N'Tổng hợp' UNION ALL
    SELECT N'AvImp.Aggregated', N'en-US', N'Aggregated' UNION ALL

    -- 10.6.0 GLE UI chrome
    SELECT N'GeneralLedgerEntries.Subtitle', N'vi-VN', N'Bút toán sổ cái (GL postings)' UNION ALL
    SELECT N'GeneralLedgerEntries.Subtitle', N'en-US', N'General ledger postings' UNION ALL
    SELECT N'GeneralLedgerEntries.ShowColumns', N'vi-VN', N'Hiển thị cột:' UNION ALL
    SELECT N'GeneralLedgerEntries.ShowColumns', N'en-US', N'Show columns:' UNION ALL
    SELECT N'GeneralLedgerEntries.FilterLedger', N'vi-VN', N'Lọc sổ cái: Year={0}, Month={1}, Book={2}, Account={3}' UNION ALL
    SELECT N'GeneralLedgerEntries.FilterLedger', N'en-US', N'Ledger filter: Year={0}, Month={1}, Book={2}, Account={3}' UNION ALL
    SELECT N'GeneralLedgerEntries.EntryId', N'vi-VN', N'Entry Id' UNION ALL
    SELECT N'GeneralLedgerEntries.EntryId', N'en-US', N'Entry Id' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherId', N'vi-VN', N'Voucher Id' UNION ALL
    SELECT N'GeneralLedgerEntries.VoucherId', N'en-US', N'Voucher Id' UNION ALL
    SELECT N'GeneralLedgerEntries.LineId', N'vi-VN', N'Line Id' UNION ALL
    SELECT N'GeneralLedgerEntries.LineId', N'en-US', N'Line Id' UNION ALL
    SELECT N'GeneralLedgerEntries.Type', N'vi-VN', N'Loại' UNION ALL
    SELECT N'GeneralLedgerEntries.Type', N'en-US', N'Type' UNION ALL
    SELECT N'GeneralLedgerEntries.Rate', N'vi-VN', N'Tỷ giá' UNION ALL
    SELECT N'GeneralLedgerEntries.Rate', N'en-US', N'Rate' UNION ALL
    SELECT N'GeneralLedgerEntries.Currency', N'vi-VN', N'Tiền tệ' UNION ALL
    SELECT N'GeneralLedgerEntries.Currency', N'en-US', N'Currency' UNION ALL
    SELECT N'GeneralLedgerEntries.CompanyOwner', N'vi-VN', N'Công ty / Owner' UNION ALL
    SELECT N'GeneralLedgerEntries.CompanyOwner', N'en-US', N'Company / Owner' UNION ALL
    SELECT N'GeneralLedgerEntries.SupplierId', N'vi-VN', N'SupplierId' UNION ALL
    SELECT N'GeneralLedgerEntries.SupplierId', N'en-US', N'SupplierId' UNION ALL
    SELECT N'GeneralLedgerEntries.EmployeeId', N'vi-VN', N'EmployeeId' UNION ALL
    SELECT N'GeneralLedgerEntries.EmployeeId', N'en-US', N'EmployeeId' UNION ALL
    SELECT N'GeneralLedgerEntries.InvoiceNo', N'vi-VN', N'Số HĐ' UNION ALL
    SELECT N'GeneralLedgerEntries.InvoiceNo', N'en-US', N'Invoice no' UNION ALL
    SELECT N'GeneralLedgerEntries.ShipmentId', N'vi-VN', N'ShipmentId' UNION ALL
    SELECT N'GeneralLedgerEntries.ShipmentId', N'en-US', N'ShipmentId' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceName', N'vi-VN', N'Tên nguồn' UNION ALL
    SELECT N'GeneralLedgerEntries.SourceName', N'en-US', N'Source name' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Ids', N'vi-VN', N'IDs' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Ids', N'en-US', N'IDs' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Voucher', N'en-US', N'Voucher' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Period', N'vi-VN', N'Kỳ' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Period', N'en-US', N'Period' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Account', N'en-US', N'Account' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Party', N'vi-VN', N'Khách hàng / Công ty' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Party', N'en-US', N'Customer / Company' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Amount', N'vi-VN', N'Số tiền' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Amount', N'en-US', N'Amount' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Book', N'vi-VN', N'Sổ' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Book', N'en-US', N'Book' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Source', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Source', N'en-US', N'Source' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Audit', N'vi-VN', N'Audit' UNION ALL
    SELECT N'GeneralLedgerEntries.ColGroup_Audit', N'en-US', N'Audit' UNION ALL
    SELECT N'GeneralLedgerEntries.Tax', N'vi-VN', N'Tax' UNION ALL
    SELECT N'GeneralLedgerEntries.Tax', N'en-US', N'Tax' UNION ALL
    SELECT N'GeneralLedgerEntries.Mgmt', N'vi-VN', N'MGMT' UNION ALL
    SELECT N'GeneralLedgerEntries.Mgmt', N'en-US', N'MGMT' UNION ALL
    SELECT N'GeneralLedgerEntries.Year', N'vi-VN', N'Năm' UNION ALL
    SELECT N'GeneralLedgerEntries.Year', N'en-US', N'Year' UNION ALL
    SELECT N'GeneralLedgerEntries.Month', N'vi-VN', N'Tháng' UNION ALL
    SELECT N'GeneralLedgerEntries.Month', N'en-US', N'Month' UNION ALL
    SELECT N'GeneralLedgerEntries.All', N'vi-VN', N'ALL' UNION ALL
    SELECT N'GeneralLedgerEntries.All', N'en-US', N'ALL'
)
MERGE dbo.LocalizationResources AS tgt
USING src ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN INSERT (ResourceKey, Culture, Value) VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
