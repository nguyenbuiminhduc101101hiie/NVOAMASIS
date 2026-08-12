/*
  Batch 3 — Menu 10:
  - 10.6.4 GeneralLedgerExcelImport.razor
  - 10.6.1 GeneralLedgerSummary_10_6_1.razor (header, filters, import panel, snackbars)
  Reuses: AvImp.* (shared Excel import UI), GeneralLedgerEntries.*, Vl1043.* book filters
*/
SET NOCOUNT ON;
BEGIN TRANSACTION;

;WITH src AS (
    -- 10.6.4 GL Excel import
    SELECT N'GleImp.Title' AS ResourceKey, N'vi-VN' AS Culture, N'Import Excel vào Sổ cái' AS Value UNION ALL
    SELECT N'GleImp.Title', N'en-US', N'Import Excel into General Ledger' UNION ALL
    SELECT N'GleImp.Heading', N'vi-VN', N'Import Excel vào GeneralLedgerEntries' UNION ALL
    SELECT N'GleImp.Heading', N'en-US', N'Import Excel into GeneralLedgerEntries' UNION ALL
    SELECT N'GleImp.Subtitle', N'vi-VN', N'Đọc sổ chi tiết công nợ, cân Nợ/Có và ghi trực tiếp vào bảng GeneralLedgerEntries.' UNION ALL
    SELECT N'GleImp.Subtitle', N'en-US', N'Read detailed debt ledger, balance debit/credit and post directly to GeneralLedgerEntries.' UNION ALL
    SELECT N'GleImp.DirectPost', N'vi-VN', N'Ghi trực tiếp vào Sổ cái' UNION ALL
    SELECT N'GleImp.DirectPost', N'en-US', N'Direct post to ledger' UNION ALL
    SELECT N'GleImp.InfoAlert', N'vi-VN', N'Mẫu hiện tại: nếu số chứng từ ở cột Có thì tạo Nợ TK đối ứng và Có TK công nợ. Nếu số chứng từ ở cột Nợ thì tạo Nợ TK công nợ và Có TK đối ứng.' UNION ALL
    SELECT N'GleImp.InfoAlert', N'en-US', N'Template: voucher no in Credit column → debit contra / credit debt account; in Debit column → debit debt / credit contra.' UNION ALL
    SELECT N'GleImp.Step2', N'vi-VN', N'2. Thiết lập ghi sổ' UNION ALL
    SELECT N'GleImp.Step2', N'en-US', N'2. Posting settings' UNION ALL
    SELECT N'GleImp.Step2Hint', N'vi-VN', N'VoucherId sinh cố định theo CompanyId + năm + số chứng từ. VoucherLineId = NULL.' UNION ALL
    SELECT N'GleImp.Step2Hint', N'en-US', N'VoucherId is deterministic from CompanyId + year + voucher no. VoucherLineId = NULL.' UNION ALL
    SELECT N'GleImp.CustomerOptional', N'vi-VN', N'Khách hàng (tùy chọn)' UNION ALL
    SELECT N'GleImp.CustomerOptional', N'en-US', N'Customer (optional)' UNION ALL
    SELECT N'GleImp.CustomerNotFound', N'vi-VN', N'Không tìm thấy khách hàng.' UNION ALL
    SELECT N'GleImp.CustomerNotFound', N'en-US', N'Customer not found.' UNION ALL
    SELECT N'GleImp.CustomerSearchHint', N'vi-VN', N'Gõ mã hoặc tên khách hàng để tìm' UNION ALL
    SELECT N'GleImp.CustomerSearchHint', N'en-US', N'Type code or name to search' UNION ALL
    SELECT N'GleImp.CustomerIdHint', N'vi-VN', N'CustomerId: {0}' UNION ALL
    SELECT N'GleImp.CustomerIdHint', N'en-US', N'CustomerId: {0}' UNION ALL
    SELECT N'GleImp.PostingDateFromVoucher', N'vi-VN', N'PostingDate = VoucherDate' UNION ALL
    SELECT N'GleImp.PostingDateFromVoucher', N'en-US', N'PostingDate = VoucherDate' UNION ALL
    SELECT N'GleImp.CommonPostingDate', N'vi-VN', N'PostingDate chung' UNION ALL
    SELECT N'GleImp.CommonPostingDate', N'en-US', N'Common posting date' UNION ALL
    SELECT N'GleImp.Kpi_Vouchers', N'vi-VN', N'Chứng từ nhận diện' UNION ALL
    SELECT N'GleImp.Kpi_Vouchers', N'en-US', N'Vouchers detected' UNION ALL
    SELECT N'GleImp.Kpi_Lines', N'vi-VN', N'Dòng sổ cái' UNION ALL
    SELECT N'GleImp.Kpi_Lines', N'en-US', N'Ledger lines' UNION ALL
    SELECT N'GleImp.Step3', N'vi-VN', N'3. Chứng từ nhận diện' UNION ALL
    SELECT N'GleImp.Step3', N'en-US', N'3. Detected vouchers' UNION ALL
    SELECT N'GleImp.Difference', N'vi-VN', N'Chênh lệch: {0}' UNION ALL
    SELECT N'GleImp.Difference', N'en-US', N'Difference: {0}' UNION ALL
    SELECT N'GleImp.Col_Select', N'vi-VN', N'Chọn' UNION ALL
    SELECT N'GleImp.Col_Select', N'en-US', N'Select' UNION ALL
    SELECT N'GleImp.Col_Direction', N'vi-VN', N'Hướng' UNION ALL
    SELECT N'GleImp.Col_Direction', N'en-US', N'Direction' UNION ALL
    SELECT N'GleImp.Direction_Credit', N'vi-VN', N'Nợ đối ứng / Có công nợ' UNION ALL
    SELECT N'GleImp.Direction_Credit', N'en-US', N'Debit contra / Credit debt' UNION ALL
    SELECT N'GleImp.Direction_Debit', N'vi-VN', N'Nợ công nợ / Có đối ứng' UNION ALL
    SELECT N'GleImp.Direction_Debit', N'en-US', N'Debit debt / Credit contra' UNION ALL
    SELECT N'GleImp.Col_LineCount', N'vi-VN', N'Số dòng' UNION ALL
    SELECT N'GleImp.Col_LineCount', N'en-US', N'Line count' UNION ALL
    SELECT N'GleImp.Status_Exists', N'vi-VN', N'Đã có' UNION ALL
    SELECT N'GleImp.Status_Exists', N'en-US', N'Exists' UNION ALL
    SELECT N'GleImp.NoVoucherData', N'vi-VN', N'Chưa có dữ liệu. Hãy chọn file Excel.' UNION ALL
    SELECT N'GleImp.NoVoucherData', N'en-US', N'No data yet. Select an Excel file.' UNION ALL
    SELECT N'GleImp.Step4', N'vi-VN', N'4. Preview GeneralLedgerEntries' UNION ALL
    SELECT N'GleImp.Step4', N'en-US', N'4. Preview GeneralLedgerEntries' UNION ALL
    SELECT N'GleImp.Aggregated', N'vi-VN', N'Gộp' UNION ALL
    SELECT N'GleImp.Aggregated', N'en-US', N'Aggregated' UNION ALL
    SELECT N'GleImp.NoLinePreview', N'vi-VN', N'Chưa có dòng sổ cái để preview.' UNION ALL
    SELECT N'GleImp.NoLinePreview', N'en-US', N'No ledger lines to preview.' UNION ALL
    SELECT N'GleImp.FooterHint', N'vi-VN', N'Chứng từ phải hợp lệ, chưa tồn tại, cân Nợ/Có và có CompanyId. Khách hàng có thể để trống; khi đó CustomerId = NULL.' UNION ALL
    SELECT N'GleImp.FooterHint', N'en-US', N'Vouchers must be valid, not exist, balanced and have CompanyId. Customer is optional (CustomerId NULL).' UNION ALL
    SELECT N'GleImp.ImportToLedger', N'vi-VN', N'Import vào Sổ cái' UNION ALL
    SELECT N'GleImp.ImportToLedger', N'en-US', N'Import to ledger' UNION ALL
    SELECT N'GleImp.ReadSuccess', N'vi-VN', N'Đã nhận diện {0} dòng Excel, {1} chứng từ và {2} dòng sổ cái.' UNION ALL
    SELECT N'GleImp.ReadSuccess', N'en-US', N'Detected {0} Excel rows, {1} voucher(s) and {2} ledger line(s).' UNION ALL
    SELECT N'GleImp.ReadFailed', N'vi-VN', N'Không đọc được file Excel. Kiểm tra lại đúng mẫu sổ chi tiết công nợ.' UNION ALL
    SELECT N'GleImp.ReadFailed', N'en-US', N'Cannot read Excel file. Check detailed debt ledger template.' UNION ALL
    SELECT N'GleImp.InvalidCompanyId', N'vi-VN', N'CompanyId không hợp lệ.' UNION ALL
    SELECT N'GleImp.InvalidCompanyId', N'en-US', N'Invalid CompanyId.' UNION ALL
    SELECT N'GleImp.CheckDuplicateResult', N'vi-VN', N'Đã kiểm tra trùng. Có {0} chứng từ đã tồn tại.' UNION ALL
    SELECT N'GleImp.CheckDuplicateResult', N'en-US', N'Duplicate check done. {0} voucher(s) already exist.' UNION ALL
    SELECT N'GleImp.CheckDuplicateFailed', N'vi-VN', N'Không kiểm tra được dữ liệu trùng: {0}' UNION ALL
    SELECT N'GleImp.CheckDuplicateFailed', N'en-US', N'Cannot check duplicates: {0}' UNION ALL
    SELECT N'GleImp.SelectCustomer', N'vi-VN', N'Vui lòng chọn khách hàng trước khi import.' UNION ALL
    SELECT N'GleImp.SelectCustomer', N'en-US', N'Please select a customer before import.' UNION ALL
    SELECT N'GleImp.ExchangeRateInvalid', N'vi-VN', N'ExchangeRate phải lớn hơn 0.' UNION ALL
    SELECT N'GleImp.ExchangeRateInvalid', N'en-US', N'ExchangeRate must be greater than 0.' UNION ALL
    SELECT N'GleImp.NoValidVouchers', N'vi-VN', N'Không có chứng từ hợp lệ để import.' UNION ALL
    SELECT N'GleImp.NoValidVouchers', N'en-US', N'No valid vouchers to import.' UNION ALL
    SELECT N'GleImp.UnbalancedVouchers', N'vi-VN', N'Có chứng từ chưa cân Nợ/Có.' UNION ALL
    SELECT N'GleImp.UnbalancedVouchers', N'en-US', N'Some vouchers are not balanced.' UNION ALL
    SELECT N'GleImp.DuplicateImportCancelled', N'vi-VN', N'Có chứng từ vừa được import trước đó. Hệ thống đã hủy toàn bộ lần import này.' UNION ALL
    SELECT N'GleImp.DuplicateImportCancelled', N'en-US', N'A voucher was imported earlier. This import batch was cancelled.' UNION ALL
    SELECT N'GleImp.ImportSuccess', N'vi-VN', N'Đã import {0} chứng từ / {1} dòng vào GeneralLedgerEntries.' UNION ALL
    SELECT N'GleImp.ImportSuccess', N'en-US', N'Imported {0} voucher(s) / {1} line(s) into GeneralLedgerEntries.' UNION ALL
    SELECT N'GleImp.ImportSuccessOptional', N'vi-VN', N'Đã import {0} chứng từ / {1} dòng ({2}).' UNION ALL
    SELECT N'GleImp.ImportSuccessOptional', N'en-US', N'Imported {0} voucher(s) / {1} line(s) ({2}).' UNION ALL
    SELECT N'GleImp.ImportFailed', N'vi-VN', N'Import thất bại, dữ liệu đã rollback: {0}' UNION ALL
    SELECT N'GleImp.ImportFailed', N'en-US', N'Import failed, rolled back: {0}' UNION ALL
    SELECT N'GleImp.CustomerIdNull', N'vi-VN', N'CustomerId NULL' UNION ALL
    SELECT N'GleImp.CustomerIdNull', N'en-US', N'CustomerId NULL' UNION ALL
    SELECT N'GleImp.CustomerIdValue', N'vi-VN', N'CustomerId {0}' UNION ALL
    SELECT N'GleImp.CustomerIdValue', N'en-US', N'CustomerId {0}' UNION ALL
    SELECT N'GleImp.NoValidSelected', N'vi-VN', N'Không có chứng từ hợp lệ đang được chọn để import.' UNION ALL
    SELECT N'GleImp.NoValidSelected', N'en-US', N'No valid selected vouchers to import.' UNION ALL
    SELECT N'GleImp.InvalidCompanyIdDetail', N'vi-VN', N'CompanyId đang trống hoặc không đúng định dạng GUID.' UNION ALL
    SELECT N'GleImp.InvalidCompanyIdDetail', N'en-US', N'CompanyId is empty or not a valid GUID.' UNION ALL
    SELECT N'GleImp.UnbalancedDiff', N'vi-VN', N'Tổng Nợ và Tổng Có đang lệch {0}.' UNION ALL
    SELECT N'GleImp.UnbalancedDiff', N'en-US', N'Total debit and credit differ by {0}.' UNION ALL
    SELECT N'GleImp.CustomerNotFromExcel', N'vi-VN', N'Không nhận diện được tên khách hàng từ file Excel.' UNION ALL
    SELECT N'GleImp.CustomerNotFromExcel', N'en-US', N'Could not detect customer name from Excel file.' UNION ALL

    -- 10.6.1 General Ledger Summary
    SELECT N'Gls1061.Title', N'vi-VN', N'10.6.1 Sổ cái tổng hợp' UNION ALL
    SELECT N'Gls1061.Title', N'en-US', N'10.6.1 General ledger summary' UNION ALL
    SELECT N'Gls1061.Subtitle', N'vi-VN', N'General Ledger · xem toàn bộ phát sinh đã ghi sổ' UNION ALL
    SELECT N'Gls1061.Subtitle', N'en-US', N'General Ledger · view all posted entries' UNION ALL
    SELECT N'Gls1061.Load', N'vi-VN', N'Tải' UNION ALL
    SELECT N'Gls1061.Load', N'en-US', N'Load' UNION ALL
    SELECT N'Gls1061.Reset', N'vi-VN', N'Đặt lại' UNION ALL
    SELECT N'Gls1061.Reset', N'en-US', N'Reset' UNION ALL
    SELECT N'Gls1061.ImportExcel', N'vi-VN', N'Import Excel' UNION ALL
    SELECT N'Gls1061.ImportExcel', N'en-US', N'Import Excel' UNION ALL
    SELECT N'Gls1061.Template', N'vi-VN', N'Mẫu Excel' UNION ALL
    SELECT N'Gls1061.Template', N'en-US', N'Excel template' UNION ALL
    SELECT N'Gls1061.ExportReimport', N'vi-VN', N'Export file import lại' UNION ALL
    SELECT N'Gls1061.ExportReimport', N'en-US', N'Export re-import file' UNION ALL
    SELECT N'Gls1061.Export', N'vi-VN', N'Export' UNION ALL
    SELECT N'Gls1061.Export', N'en-US', N'Export' UNION ALL
    SELECT N'Gls1061.FilterBy', N'vi-VN', N'Chọn theo' UNION ALL
    SELECT N'Gls1061.FilterBy', N'en-US', N'Filter by' UNION ALL
    SELECT N'Gls1061.ByDate', N'vi-VN', N'Theo ngày' UNION ALL
    SELECT N'Gls1061.ByDate', N'en-US', N'By date' UNION ALL
    SELECT N'Gls1061.ByPeriod', N'vi-VN', N'Theo kỳ' UNION ALL
    SELECT N'Gls1061.ByPeriod', N'en-US', N'By period' UNION ALL
    SELECT N'Gls1061.FromDate', N'vi-VN', N'Từ ngày' UNION ALL
    SELECT N'Gls1061.FromDate', N'en-US', N'From date' UNION ALL
    SELECT N'Gls1061.ToDate', N'vi-VN', N'Đến ngày' UNION ALL
    SELECT N'Gls1061.ToDate', N'en-US', N'To date' UNION ALL
    SELECT N'Gls1061.VoucherNo', N'vi-VN', N'Số chứng từ' UNION ALL
    SELECT N'Gls1061.VoucherNo', N'en-US', N'Voucher no' UNION ALL
    SELECT N'Gls1061.VoucherNoPlaceholder', N'vi-VN', N'VoucherNo...' UNION ALL
    SELECT N'Gls1061.VoucherNoPlaceholder', N'en-US', N'VoucherNo...' UNION ALL
    SELECT N'Gls1061.Customer', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Gls1061.Customer', N'en-US', N'Customer' UNION ALL
    SELECT N'Gls1061.CustomerPlaceholder', N'vi-VN', N'Tên đối tượng / khách hàng...' UNION ALL
    SELECT N'Gls1061.CustomerPlaceholder', N'en-US', N'Party / customer name...' UNION ALL
    SELECT N'Gls1061.Description', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'Gls1061.Description', N'en-US', N'Description' UNION ALL
    SELECT N'Gls1061.DescriptionPlaceholder', N'vi-VN', N'Nội dung diễn giải...' UNION ALL
    SELECT N'Gls1061.DescriptionPlaceholder', N'en-US', N'Description text...' UNION ALL
    SELECT N'Gls1061.Source', N'vi-VN', N'Nguồn phát sinh' UNION ALL
    SELECT N'Gls1061.Source', N'en-US', N'Source' UNION ALL
    SELECT N'Gls1061.SourcePlaceholder', N'vi-VN', N'Source...' UNION ALL
    SELECT N'Gls1061.SourcePlaceholder', N'en-US', N'Source...' UNION ALL
    SELECT N'Gls1061.ImportTitle', N'vi-VN', N'IMPORT SỔ CÁI TỪ EXCEL' UNION ALL
    SELECT N'Gls1061.ImportTitle', N'en-US', N'IMPORT LEDGER FROM EXCEL' UNION ALL
    SELECT N'Gls1061.ImportSubtitle', N'vi-VN', N'Tool nhập liệu: chọn file → đọc thử → kiểm tra cân đối Nợ/Có → import vào GeneralLedgerEntries.' UNION ALL
    SELECT N'Gls1061.ImportSubtitle', N'en-US', N'Workflow: select file → preview → balance check → import to GeneralLedgerEntries.' UNION ALL
    SELECT N'Gls1061.TotalRows', N'vi-VN', N'Tổng dòng: {0}' UNION ALL
    SELECT N'Gls1061.TotalRows', N'en-US', N'Total rows: {0}' UNION ALL
    SELECT N'Gls1061.ValidRows', N'vi-VN', N'Hợp lệ: {0}' UNION ALL
    SELECT N'Gls1061.ValidRows', N'en-US', N'Valid: {0}' UNION ALL
    SELECT N'Gls1061.ErrorRows', N'vi-VN', N'Lỗi: {0}' UNION ALL
    SELECT N'Gls1061.ErrorRows', N'en-US', N'Errors: {0}' UNION ALL
    SELECT N'Gls1061.ImportAlert', N'vi-VN', N'File Excel cần dòng tiêu đề ở dòng 1. Cột bắt buộc: PostingDate, VoucherNo, AccountCode, Debit, Credit. Nếu DB chưa có CompanyId mặc định thì phải có cột CompanyId. Có thể dùng tên cột tiếng Việt (Ngày HT, Số chứng từ, TK, Nợ, Có).' UNION ALL
    SELECT N'Gls1061.ImportAlert', N'en-US', N'Excel row 1 must be headers. Required: PostingDate, VoucherNo, AccountCode, Debit, Credit. CompanyId column required if no default. Vietnamese column names supported.' UNION ALL
    SELECT N'Gls1061.ChooseExcel', N'vi-VN', N'Chọn file Excel' UNION ALL
    SELECT N'Gls1061.ChooseExcel', N'en-US', N'Choose Excel file' UNION ALL
    SELECT N'Gls1061.NoFileChosen', N'vi-VN', N'Chưa chọn file' UNION ALL
    SELECT N'Gls1061.NoFileChosen', N'en-US', N'No file chosen' UNION ALL
    SELECT N'Gls1061.AllowDuplicate', N'vi-VN', N'Cho phép trùng số CT' UNION ALL
    SELECT N'Gls1061.AllowDuplicate', N'en-US', N'Allow duplicate voucher no' UNION ALL
    SELECT N'Gls1061.RebuildBalance', N'vi-VN', N'Rebuild Balance sau import' UNION ALL
    SELECT N'Gls1061.RebuildBalance', N'en-US', N'Rebuild balance after import' UNION ALL
    SELECT N'Gls1061.PreviewValidate', N'vi-VN', N'Đọc thử / Validate' UNION ALL
    SELECT N'Gls1061.PreviewValidate', N'en-US', N'Preview / Validate' UNION ALL
    SELECT N'Gls1061.ImportToLedger', N'vi-VN', N'Import vào sổ cái' UNION ALL
    SELECT N'Gls1061.ImportToLedger', N'en-US', N'Import to ledger' UNION ALL
    SELECT N'Gls1061.DownloadTemplate', N'vi-VN', N'Tải mẫu' UNION ALL
    SELECT N'Gls1061.DownloadTemplate', N'en-US', N'Download template' UNION ALL
    SELECT N'Gls1061.ExportTemplate', N'vi-VN', N'Export theo mẫu import' UNION ALL
    SELECT N'Gls1061.ExportTemplate', N'en-US', N'Export import template' UNION ALL
    SELECT N'Gls1061.ClearPreview', N'vi-VN', N'Xóa preview' UNION ALL
    SELECT N'Gls1061.ClearPreview', N'en-US', N'Clear preview' UNION ALL
    SELECT N'Gls1061.VoucherCount', N'vi-VN', N'Số chứng từ' UNION ALL
    SELECT N'Gls1061.VoucherCount', N'en-US', N'Voucher count' UNION ALL
    SELECT N'Gls1061.NoPermissionView', N'vi-VN', N'Bạn không có quyền xem sổ cái.' UNION ALL
    SELECT N'Gls1061.NoPermissionView', N'en-US', N'You do not have permission to view the ledger.' UNION ALL
    SELECT N'Gls1061.LoadedCount', N'vi-VN', N'Đã tải {0} dòng sổ cái.' UNION ALL
    SELECT N'Gls1061.LoadedCount', N'en-US', N'Loaded {0} ledger lines.' UNION ALL
    SELECT N'Gls1061.SelectFileFirst', N'vi-VN', N'Vui lòng chọn file Excel trước khi đọc thử.' UNION ALL
    SELECT N'Gls1061.SelectFileFirst', N'en-US', N'Please select an Excel file before preview.' UNION ALL
    SELECT N'Gls1061.EmptyExcel', N'vi-VN', N'File Excel không có dữ liệu.' UNION ALL
    SELECT N'Gls1061.EmptyExcel', N'en-US', N'Excel file has no data.' UNION ALL
    SELECT N'Gls1061.MissingColumns', N'vi-VN', N'File thiếu cột bắt buộc: {0}' UNION ALL
    SELECT N'Gls1061.MissingColumns', N'en-US', N'Missing required columns: {0}' UNION ALL
    SELECT N'Gls1061.PreviewResult', N'vi-VN', N'Đã đọc {0} dòng Excel. Hợp lệ {1}, lỗi {2}.' UNION ALL
    SELECT N'Gls1061.PreviewResult', N'en-US', N'Read {0} Excel rows. Valid {1}, errors {2}.' UNION ALL
    SELECT N'Gls1061.NoPreviewToImport', N'vi-VN', N'Chưa có dữ liệu preview để import.' UNION ALL
    SELECT N'Gls1061.NoPreviewToImport', N'en-US', N'No preview data to import.' UNION ALL
    SELECT N'Gls1061.FixErrorsFirst', N'vi-VN', N'File còn dòng lỗi. Vui lòng sửa Excel rồi đọc thử lại trước khi import.' UNION ALL
    SELECT N'Gls1061.FixErrorsFirst', N'en-US', N'File has error rows. Fix Excel and preview again before import.' UNION ALL
    SELECT N'Gls1061.ImportSuccess', N'vi-VN', N'Import thành công {0} dòng sổ cái. Batch: {1}' UNION ALL
    SELECT N'Gls1061.ImportSuccess', N'en-US', N'Imported {0} ledger lines. Batch: {1}' UNION ALL
    SELECT N'Gls1061.NoExportPermission', N'vi-VN', N'Bạn không có quyền export sổ cái.' UNION ALL
    SELECT N'Gls1061.NoExportPermission', N'en-US', N'No permission to export ledger.' UNION ALL
    SELECT N'Gls1061.NoDataExport', N'vi-VN', N'Không có dữ liệu để export theo điều kiện lọc hiện tại.' UNION ALL
    SELECT N'Gls1061.NoDataExport', N'en-US', N'No data to export for current filters.' UNION ALL
    SELECT N'Gls1061.NoDataAfterVouchers', N'vi-VN', N'Không có dữ liệu để export sau khi lấy đủ chứng từ.' UNION ALL
    SELECT N'Gls1061.NoDataAfterVouchers', N'en-US', N'No data to export after resolving vouchers.' UNION ALL
    SELECT N'Gls1061.ExportUnbalanced', N'vi-VN', N'Đã export {0} dòng nhưng còn {1} chứng từ lệch Nợ/Có trong dữ liệu gốc.' UNION ALL
    SELECT N'Gls1061.ExportUnbalanced', N'en-US', N'Exported {0} lines but {1} voucher(s) unbalanced in source data.' UNION ALL
    SELECT N'Gls1061.ExportSuccess', N'vi-VN', N'Đã export {0} dòng đủ chứng từ, file có thể import lại.' UNION ALL
    SELECT N'Gls1061.ExportSuccess', N'en-US', N'Exported {0} lines with complete vouchers; file is re-importable.' UNION ALL
    SELECT N'Gls1061.ExportFailed', N'vi-VN', N'Export Excel thất bại: {0}' UNION ALL
    SELECT N'Gls1061.ExportFailed', N'en-US', N'Excel export failed: {0}' UNION ALL
    SELECT N'Gls1061.NoDebtExportPermission', N'vi-VN', N'Bạn không có quyền xuất sổ công nợ tổng hợp.' UNION ALL
    SELECT N'Gls1061.NoDebtExportPermission', N'en-US', N'No permission to export consolidated debt ledger.' UNION ALL
    SELECT N'Gls1061.SelectDateRange', N'vi-VN', N'Vui lòng chọn Từ ngày / Đến ngày trước khi export.' UNION ALL
    SELECT N'Gls1061.SelectDateRange', N'en-US', N'Please select from/to dates before export.' UNION ALL
    SELECT N'Gls1061.NoDebtData', N'vi-VN', N'Không có dữ liệu công nợ theo điều kiện lọc hiện tại.' UNION ALL
    SELECT N'Gls1061.NoDebtData', N'en-US', N'No debt data for current filters.' UNION ALL
    SELECT N'Gls1061.NoCustomersExport', N'vi-VN', N'Không có khách hàng thỏa điều kiện export.' UNION ALL
    SELECT N'Gls1061.NoCustomersExport', N'en-US', N'No customers match export criteria.' UNION ALL
    SELECT N'Gls1061.DebtExportSuccess', N'vi-VN', N'Đã export sổ công nợ tổng hợp: {0} khách hàng.' UNION ALL
    SELECT N'Gls1061.DebtExportSuccess', N'en-US', N'Exported consolidated debt ledger: {0} customer(s).' UNION ALL
    SELECT N'Gls1061.DebtExportFailed', N'vi-VN', N'Export sổ công nợ thất bại: {0}' UNION ALL
    SELECT N'Gls1061.DebtExportFailed', N'en-US', N'Debt ledger export failed: {0}' UNION ALL
    SELECT N'Gls1061.TemplateFailed', N'vi-VN', N'Không tạo được file mẫu: {0}' UNION ALL
    SELECT N'Gls1061.TemplateFailed', N'en-US', N'Cannot create template file: {0}' UNION ALL

    SELECT N'Gls1061.Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'Gls1061.Account', N'en-US', N'Account' UNION ALL
    SELECT N'Gls1061.Year', N'vi-VN', N'Năm' UNION ALL
    SELECT N'Gls1061.Year', N'en-US', N'Year' UNION ALL
    SELECT N'Gls1061.Month', N'vi-VN', N'Tháng' UNION ALL
    SELECT N'Gls1061.Month', N'en-US', N'Month' UNION ALL
    SELECT N'Gls1061.Book', N'vi-VN', N'Sổ' UNION ALL
    SELECT N'Gls1061.Book', N'en-US', N'Book' UNION ALL
    SELECT N'Gls1061.DateModeHelpPeriod', N'vi-VN', N'Đang lọc theo kỳ kế toán: hệ thống dùng FiscalYear/FiscalPeriod và tự set Từ ngày - Đến ngày theo tháng.' UNION ALL
    SELECT N'Gls1061.DateModeHelpPeriod', N'en-US', N'Filtering by accounting period: uses FiscalYear/FiscalPeriod and auto-sets from/to dates for the month.' UNION ALL
    SELECT N'Gls1061.DateModeHelpDateRange', N'vi-VN', N'Đang lọc theo ngày hạch toán: hệ thống chỉ dùng PostingDate từ ngày - đến ngày, không ép FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Gls1061.DateModeHelpDateRange', N'en-US', N'Filtering by posting date: uses PostingDate from/to only, not FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Gls1061.TotalDebit', N'vi-VN', N'Tổng Nợ' UNION ALL
    SELECT N'Gls1061.TotalDebit', N'en-US', N'Total debit' UNION ALL
    SELECT N'Gls1061.TotalCredit', N'vi-VN', N'Tổng Có' UNION ALL
    SELECT N'Gls1061.TotalCredit', N'en-US', N'Total credit' UNION ALL
    SELECT N'Gls1061.Difference', N'vi-VN', N'Chênh lệch' UNION ALL
    SELECT N'Gls1061.Difference', N'en-US', N'Difference' UNION ALL
    SELECT N'Gls1061.Lines', N'vi-VN', N'Số dòng' UNION ALL
    SELECT N'Gls1061.Lines', N'en-US', N'Lines' UNION ALL
    SELECT N'Gls1061.NoRecords', N'vi-VN', N'Không có dữ liệu sổ cái theo điều kiện lọc hiện tại.' UNION ALL
    SELECT N'Gls1061.NoRecords', N'en-US', N'No ledger data for current filters.' UNION ALL
    SELECT N'Gls1061.PreviewLimit', N'vi-VN', N'Preview tối đa 300 dòng đầu. Khi import sẽ nhập toàn bộ dòng hợp lệ trong file.' UNION ALL
    SELECT N'Gls1061.PreviewLimit', N'en-US', N'Preview shows first 300 rows. Import will load all valid rows in the file.' UNION ALL

    SELECT N'GleImp.AutoSelectCustomer', N'vi-VN', N'Đã tự chọn khách hàng: {0}' UNION ALL
    SELECT N'GleImp.AutoSelectCustomer', N'en-US', N'Auto-selected customer: {0}' UNION ALL
    SELECT N'GleImp.CustomerNotInCatalog', N'vi-VN', N'Không tìm thấy khách hàng ''{0}'' trong danh mục Customer. Import với CustomerId = NULL.' UNION ALL
    SELECT N'GleImp.CustomerNotInCatalog', N'en-US', N'Customer ''{0}'' not found in Customer catalog. Import with CustomerId = NULL.' UNION ALL
    SELECT N'GleImp.CustomerNoExactMatch', N'vi-VN', N'Không có khách hàng khớp chính xác với ''{0}''. CustomerId = NULL; chọn thủ công nếu cần.' UNION ALL
    SELECT N'GleImp.CustomerNoExactMatch', N'en-US', N'No exact match for ''{0}''. CustomerId = NULL; select manually if needed.' UNION ALL
    SELECT N'GleImp.AutoDetectCustomerFailed', N'vi-VN', N'Không thể tự dò khách hàng: {0}' UNION ALL
    SELECT N'GleImp.AutoDetectCustomerFailed', N'en-US', N'Cannot auto-detect customer: {0}' UNION ALL
    SELECT N'GleImp.CompanyIdResolved', N'vi-VN', N'Đã tự lấy CompanyId: {0}' UNION ALL
    SELECT N'GleImp.CompanyIdResolved', N'en-US', N'Resolved CompanyId: {0}' UNION ALL
    SELECT N'GleImp.CompanyIdNotFound', N'vi-VN', N'Không tìm thấy CompanyId trong database. Nhập đúng GUID trước khi import.' UNION ALL
    SELECT N'GleImp.CompanyIdNotFound', N'en-US', N'CompanyId not found in database. Enter a valid GUID before import.' UNION ALL
    SELECT N'GleImp.CompanyIdResolveFailed', N'vi-VN', N'Không thể tự lấy CompanyId: {0}' UNION ALL
    SELECT N'GleImp.CompanyIdResolveFailed', N'en-US', N'Cannot resolve CompanyId: {0}'
)
MERGE dbo.LocalizationResources AS tgt
USING src ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN INSERT (ResourceKey, Culture, Value) VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
