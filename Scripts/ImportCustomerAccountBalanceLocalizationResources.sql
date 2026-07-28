/*
  Upsert localization resources for:
  - Components/Accounting/Pages/CustomerAccountBalance.razor
  - NavMenu item 10.4.6

  Target table: dbo.LocalizationResources (ResourceKey, Culture, Value)
  Cultures: vi-VN, en-US
  Safe to re-run.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'CustomerAccountBalance.Title' AS ResourceKey, N'vi-VN' AS Culture, N'Số dư công nợ khách hàng' AS Value UNION ALL
    SELECT N'CustomerAccountBalance.Title', N'en-US', N'Customer Account Balance' UNION ALL

    SELECT N'CustomerAccountBalance.Heading', N'vi-VN', N'Số dư công nợ khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.Heading', N'en-US', N'Customer Account Balance' UNION ALL

    SELECT N'CustomerAccountBalance.Subtitle', N'vi-VN', N'Quản lý số dư chi tiết theo khách hàng, tài khoản, năm, tháng và mã sổ' UNION ALL
    SELECT N'CustomerAccountBalance.Subtitle', N'en-US', N'Manage detailed balances by customer, account, year, month and book code' UNION ALL

    SELECT N'CustomerAccountBalance.CloseImport', N'vi-VN', N'Đóng import' UNION ALL
    SELECT N'CustomerAccountBalance.CloseImport', N'en-US', N'Close import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportExcel', N'vi-VN', N'Import Excel' UNION ALL
    SELECT N'CustomerAccountBalance.ImportExcel', N'en-US', N'Import Excel' UNION ALL

    SELECT N'CustomerAccountBalance.Reload', N'vi-VN', N'Làm mới' UNION ALL
    SELECT N'CustomerAccountBalance.Reload', N'en-US', N'Reload' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyNotResolved', N'vi-VN', N'Không lấy được CompanyId từ claim đăng nhập. Hãy chọn công ty ở bộ lọc bên dưới.' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyNotResolved', N'en-US', N'Could not resolve CompanyId from login claims. Please select a company in the filters below.' UNION ALL

    SELECT N'CustomerAccountBalance.Filters', N'vi-VN', N'Bộ lọc' UNION ALL
    SELECT N'CustomerAccountBalance.Filters', N'en-US', N'Filters' UNION ALL

    SELECT N'CustomerAccountBalance.Company', N'vi-VN', N'Công ty' UNION ALL
    SELECT N'CustomerAccountBalance.Company', N'en-US', N'Company' UNION ALL

    SELECT N'CustomerAccountBalance.Year', N'vi-VN', N'Năm' UNION ALL
    SELECT N'CustomerAccountBalance.Year', N'en-US', N'Year' UNION ALL

    SELECT N'CustomerAccountBalance.Month', N'vi-VN', N'Tháng' UNION ALL
    SELECT N'CustomerAccountBalance.Month', N'en-US', N'Month' UNION ALL

    SELECT N'CustomerAccountBalance.MonthItem', N'vi-VN', N'Tháng {0}' UNION ALL
    SELECT N'CustomerAccountBalance.MonthItem', N'en-US', N'Month {0}' UNION ALL

    SELECT N'CustomerAccountBalance.Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'CustomerAccountBalance.Account', N'en-US', N'Account' UNION ALL

    SELECT N'CustomerAccountBalance.AccountPlaceholder', N'vi-VN', N'Ví dụ: 131' UNION ALL
    SELECT N'CustomerAccountBalance.AccountPlaceholder', N'en-US', N'Example: 131' UNION ALL

    SELECT N'CustomerAccountBalance.BookCode', N'vi-VN', N'Mã sổ' UNION ALL
    SELECT N'CustomerAccountBalance.BookCode', N'en-US', N'Book code' UNION ALL

    SELECT N'CustomerAccountBalance.BookCodeAllPlaceholder', N'vi-VN', N'Để trống = tất cả' UNION ALL
    SELECT N'CustomerAccountBalance.BookCodeAllPlaceholder', N'en-US', N'Leave blank = all' UNION ALL

    SELECT N'CustomerAccountBalance.Customer', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.Customer', N'en-US', N'Customer' UNION ALL

    SELECT N'CustomerAccountBalance.Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'CustomerAccountBalance.Search', N'en-US', N'Search' UNION ALL

    SELECT N'CustomerAccountBalance.ClearFilter', N'vi-VN', N'Xóa lọc' UNION ALL
    SELECT N'CustomerAccountBalance.ClearFilter', N'en-US', N'Clear filters' UNION ALL

    SELECT N'CustomerAccountBalance.SyncAccountBalance', N'vi-VN', N'Cập nhật Account Balance' UNION ALL
    SELECT N'CustomerAccountBalance.SyncAccountBalance', N'en-US', N'Update Account Balance' UNION ALL

    SELECT N'CustomerAccountBalance.ImportTitle', N'vi-VN', N'Import Excel số dư công nợ khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.ImportTitle', N'en-US', N'Import customer account balance Excel' UNION ALL

    SELECT N'CustomerAccountBalance.ImportSubtitle', N'vi-VN', N'Mã KH trong Excel được đối chiếu với mã số thuế Customer.TaxCode, sau đó lưu từng khách hàng vào customer_account_balance' UNION ALL
    SELECT N'CustomerAccountBalance.ImportSubtitle', N'en-US', N'Customer codes in Excel are matched to Customer.TaxCode, then each customer is saved to customer_account_balance' UNION ALL

    SELECT N'CustomerAccountBalance.ResetImport', N'vi-VN', N'Xóa dữ liệu import' UNION ALL
    SELECT N'CustomerAccountBalance.ResetImport', N'en-US', N'Clear import data' UNION ALL

    SELECT N'CustomerAccountBalance.ExistingMode', N'vi-VN', N'Xử lý dữ liệu đã tồn tại' UNION ALL
    SELECT N'CustomerAccountBalance.ExistingMode', N'en-US', N'Existing data handling' UNION ALL

    SELECT N'CustomerAccountBalance.ExistingModeReject', N'vi-VN', N'An toàn - Báo lỗi nếu đã tồn tại' UNION ALL
    SELECT N'CustomerAccountBalance.ExistingModeReject', N'en-US', N'Safe - Error if already exists' UNION ALL

    SELECT N'CustomerAccountBalance.ExistingModeUpdate', N'vi-VN', N'Cập nhật số dư khách hàng hiện có' UNION ALL
    SELECT N'CustomerAccountBalance.ExistingModeUpdate', N'en-US', N'Update existing customer balances' UNION ALL

    SELECT N'CustomerAccountBalance.ExistingModeReplace', N'vi-VN', N'Xóa và thay thế toàn bộ tài khoản/kỳ/sổ' UNION ALL
    SELECT N'CustomerAccountBalance.ExistingModeReplace', N'en-US', N'Delete and replace entire account/period/book' UNION ALL

    SELECT N'CustomerAccountBalance.ImportBookCode', N'vi-VN', N'Mã sổ khi import' UNION ALL
    SELECT N'CustomerAccountBalance.ImportBookCode', N'en-US', N'Book code for import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportBookCodePlaceholder', N'vi-VN', N'Để trống = NULL' UNION ALL
    SELECT N'CustomerAccountBalance.ImportBookCodePlaceholder', N'en-US', N'Leave blank = NULL' UNION ALL

    SELECT N'CustomerAccountBalance.SyncAfterImport', N'vi-VN', N'Sau khi import, tự cộng tổng và cập nhật account_balance' UNION ALL
    SELECT N'CustomerAccountBalance.SyncAfterImport', N'en-US', N'After import, aggregate and update account_balance' UNION ALL

    SELECT N'CustomerAccountBalance.ReadingFile', N'vi-VN', N'Đang đọc file và đối chiếu mã số thuế...' UNION ALL
    SELECT N'CustomerAccountBalance.ReadingFile', N'en-US', N'Reading file and matching tax codes...' UNION ALL

    SELECT N'CustomerAccountBalance.ImportInfoAlert', N'vi-VN', N'Dữ liệu gốc được lưu vào <b>customer_account_balance</b> theo Customer_ID. Bảng <b>account_balance</b> chỉ nhận một dòng tổng hợp theo tài khoản, kỳ và mã sổ.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportInfoAlert', N'en-US', N'Source data is stored in <b>customer_account_balance</b> by Customer_ID. The <b>account_balance</b> table receives one aggregated row per account, period and book code.' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyInFile', N'vi-VN', N'Đơn vị trong file' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyInFile', N'en-US', N'Company in file' UNION ALL

    SELECT N'CustomerAccountBalance.TaxCodeInFile', N'vi-VN', N'MST đơn vị' UNION ALL
    SELECT N'CustomerAccountBalance.TaxCodeInFile', N'en-US', N'Company tax code' UNION ALL

    SELECT N'CustomerAccountBalance.MatchedCustomers', N'vi-VN', N'Khách hàng đã khớp MST' UNION ALL
    SELECT N'CustomerAccountBalance.MatchedCustomers', N'en-US', N'Customers matched by tax code' UNION ALL

    SELECT N'CustomerAccountBalance.UnmatchedCustomers', N'vi-VN', N'Không khớp / trùng MST' UNION ALL
    SELECT N'CustomerAccountBalance.UnmatchedCustomers', N'en-US', N'Unmatched / duplicate tax codes' UNION ALL

    SELECT N'CustomerAccountBalance.ExcelErrors', N'vi-VN', N'Lỗi dữ liệu Excel' UNION ALL
    SELECT N'CustomerAccountBalance.ExcelErrors', N'en-US', N'Excel data errors' UNION ALL

    SELECT N'CustomerAccountBalance.WriteRowsAfterMerge', N'vi-VN', N'Số dòng sẽ ghi sau khi gộp KH' UNION ALL
    SELECT N'CustomerAccountBalance.WriteRowsAfterMerge', N'en-US', N'Rows to write after customer merge' UNION ALL

    SELECT N'CustomerAccountBalance.ExcelRowPrefix', N'vi-VN', N'Dòng Excel {0}:' UNION ALL
    SELECT N'CustomerAccountBalance.ExcelRowPrefix', N'en-US', N'Excel row {0}:' UNION ALL

    SELECT N'CustomerAccountBalance.PreviewTitle', N'vi-VN', N'Xem trước và đối chiếu khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.PreviewTitle', N'en-US', N'Preview and match customers' UNION ALL

    SELECT N'CustomerAccountBalance.ShowOnlyProblems', N'vi-VN', N'Chỉ hiện dòng lỗi/cảnh báo' UNION ALL
    SELECT N'CustomerAccountBalance.ShowOnlyProblems', N'en-US', N'Show only error/warning rows' UNION ALL

    SELECT N'CustomerAccountBalance.ColRow', N'vi-VN', N'Dòng' UNION ALL
    SELECT N'CustomerAccountBalance.ColRow', N'en-US', N'Row' UNION ALL

    SELECT N'CustomerAccountBalance.ColCustomerTax', N'vi-VN', N'Mã KH/MST' UNION ALL
    SELECT N'CustomerAccountBalance.ColCustomerTax', N'en-US', N'Customer / Tax code' UNION ALL

    SELECT N'CustomerAccountBalance.ColNameInFile', N'vi-VN', N'Tên trong file' UNION ALL
    SELECT N'CustomerAccountBalance.ColNameInFile', N'en-US', N'Name in file' UNION ALL

    SELECT N'CustomerAccountBalance.ColCustomerId', N'vi-VN', N'Customer_ID' UNION ALL
    SELECT N'CustomerAccountBalance.ColCustomerId', N'en-US', N'Customer_ID' UNION ALL

    SELECT N'CustomerAccountBalance.ColSystemCustomer', N'vi-VN', N'Khách hàng hệ thống' UNION ALL
    SELECT N'CustomerAccountBalance.ColSystemCustomer', N'en-US', N'System customer' UNION ALL

    SELECT N'CustomerAccountBalance.ColOpeningDebit', N'vi-VN', N'ĐK Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.ColOpeningDebit', N'en-US', N'Opening Debit' UNION ALL

    SELECT N'CustomerAccountBalance.ColOpeningCredit', N'vi-VN', N'ĐK Có' UNION ALL
    SELECT N'CustomerAccountBalance.ColOpeningCredit', N'en-US', N'Opening Credit' UNION ALL

    SELECT N'CustomerAccountBalance.ColPeriodDebit', N'vi-VN', N'PS Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.ColPeriodDebit', N'en-US', N'Period Debit' UNION ALL

    SELECT N'CustomerAccountBalance.ColPeriodCredit', N'vi-VN', N'PS Có' UNION ALL
    SELECT N'CustomerAccountBalance.ColPeriodCredit', N'en-US', N'Period Credit' UNION ALL

    SELECT N'CustomerAccountBalance.ColClosingDebit', N'vi-VN', N'CK Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.ColClosingDebit', N'en-US', N'Closing Debit' UNION ALL

    SELECT N'CustomerAccountBalance.ColClosingCredit', N'vi-VN', N'CK Có' UNION ALL
    SELECT N'CustomerAccountBalance.ColClosingCredit', N'en-US', N'Closing Credit' UNION ALL

    SELECT N'CustomerAccountBalance.ColResult', N'vi-VN', N'Kết quả' UNION ALL
    SELECT N'CustomerAccountBalance.ColResult', N'en-US', N'Result' UNION ALL

    SELECT N'CustomerAccountBalance.TaxCodePrefix', N'vi-VN', N'MST: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.TaxCodePrefix', N'en-US', N'Tax: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.NotMatchedYet', N'vi-VN', N'Chưa đối chiếu' UNION ALL
    SELECT N'CustomerAccountBalance.NotMatchedYet', N'en-US', N'Not matched yet' UNION ALL

    SELECT N'CustomerAccountBalance.CustomerNotMatched', N'vi-VN', N'Không khớp khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.CustomerNotMatched', N'en-US', N'Customer not matched' UNION ALL

    SELECT N'CustomerAccountBalance.ReadyToImport', N'vi-VN', N'Đã khớp mã số thuế và sẵn sàng import' UNION ALL
    SELECT N'CustomerAccountBalance.ReadyToImport', N'en-US', N'Tax code matched and ready to import' UNION ALL

    SELECT N'CustomerAccountBalance.Importing', N'vi-VN', N'ĐANG IMPORT...' UNION ALL
    SELECT N'CustomerAccountBalance.Importing', N'en-US', N'IMPORTING...' UNION ALL

    SELECT N'CustomerAccountBalance.ImportCommit', N'vi-VN', N'IMPORT VÀO CUSTOMER ACCOUNT BALANCE' UNION ALL
    SELECT N'CustomerAccountBalance.ImportCommit', N'en-US', N'IMPORT INTO CUSTOMER ACCOUNT BALANCE' UNION ALL

    SELECT N'CustomerAccountBalance.CancelPreview', N'vi-VN', N'Hủy dữ liệu xem trước' UNION ALL
    SELECT N'CustomerAccountBalance.CancelPreview', N'en-US', N'Cancel preview data' UNION ALL

    SELECT N'CustomerAccountBalance.ImportSuccessResult', N'vi-VN', N'Import thành công: thêm {0}, cập nhật {1}, xóa {2} dòng; tổng {3} khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportSuccessResult', N'en-US', N'Import successful: inserted {0}, updated {1}, deleted {2} rows; total {3} customers.' UNION ALL

    SELECT N'CustomerAccountBalance.AccountBalanceSynced', N'vi-VN', N'Đã cập nhật account_balance.' UNION ALL
    SELECT N'CustomerAccountBalance.AccountBalanceSynced', N'en-US', N'account_balance has been updated.' UNION ALL

    SELECT N'CustomerAccountBalance.CustomerCount', N'vi-VN', N'Số khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.CustomerCount', N'en-US', N'Customers' UNION ALL

    SELECT N'CustomerAccountBalance.BalanceRowsCount', N'vi-VN', N'{0} dòng số dư' UNION ALL
    SELECT N'CustomerAccountBalance.BalanceRowsCount', N'en-US', N'{0} balance rows' UNION ALL

    SELECT N'CustomerAccountBalance.Opening', N'vi-VN', N'Đầu kỳ' UNION ALL
    SELECT N'CustomerAccountBalance.Opening', N'en-US', N'Opening' UNION ALL

    SELECT N'CustomerAccountBalance.Period', N'vi-VN', N'Phát sinh' UNION ALL
    SELECT N'CustomerAccountBalance.Period', N'en-US', N'Period activity' UNION ALL

    SELECT N'CustomerAccountBalance.Closing', N'vi-VN', N'Cuối kỳ' UNION ALL
    SELECT N'CustomerAccountBalance.Closing', N'en-US', N'Closing' UNION ALL

    SELECT N'CustomerAccountBalance.DebitColon', N'vi-VN', N'Nợ: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.DebitColon', N'en-US', N'Debit: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.CreditColon', N'vi-VN', N'Có: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.CreditColon', N'en-US', N'Credit: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.EditorTitle', N'vi-VN', N'Thêm hoặc chỉnh sửa số dư khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.EditorTitle', N'en-US', N'Add or edit customer balance' UNION ALL

    SELECT N'CustomerAccountBalance.CustomerRequired', N'vi-VN', N'Chưa chọn khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.CustomerRequired', N'en-US', N'Customer is required' UNION ALL

    SELECT N'CustomerAccountBalance.AccountRequired', N'vi-VN', N'Chưa nhập tài khoản' UNION ALL
    SELECT N'CustomerAccountBalance.AccountRequired', N'en-US', N'Account is required' UNION ALL

    SELECT N'CustomerAccountBalance.OpeningDebit', N'vi-VN', N'Đầu kỳ Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.OpeningDebit', N'en-US', N'Opening Debit' UNION ALL

    SELECT N'CustomerAccountBalance.OpeningCredit', N'vi-VN', N'Đầu kỳ Có' UNION ALL
    SELECT N'CustomerAccountBalance.OpeningCredit', N'en-US', N'Opening Credit' UNION ALL

    SELECT N'CustomerAccountBalance.PeriodDebit', N'vi-VN', N'Phát sinh Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.PeriodDebit', N'en-US', N'Period Debit' UNION ALL

    SELECT N'CustomerAccountBalance.PeriodCredit', N'vi-VN', N'Phát sinh Có' UNION ALL
    SELECT N'CustomerAccountBalance.PeriodCredit', N'en-US', N'Period Credit' UNION ALL

    SELECT N'CustomerAccountBalance.ClosingDebit', N'vi-VN', N'Cuối kỳ Nợ' UNION ALL
    SELECT N'CustomerAccountBalance.ClosingDebit', N'en-US', N'Closing Debit' UNION ALL

    SELECT N'CustomerAccountBalance.ClosingCredit', N'vi-VN', N'Cuối kỳ Có' UNION ALL
    SELECT N'CustomerAccountBalance.ClosingCredit', N'en-US', N'Closing Credit' UNION ALL

    SELECT N'CustomerAccountBalance.EditorOpeningBothSides', N'vi-VN', N'Số dư đầu kỳ không được đồng thời có cả bên Nợ và bên Có.' UNION ALL
    SELECT N'CustomerAccountBalance.EditorOpeningBothSides', N'en-US', N'Opening balance cannot have both Debit and Credit at the same time.' UNION ALL

    SELECT N'CustomerAccountBalance.Saving', N'vi-VN', N'ĐANG LƯU...' UNION ALL
    SELECT N'CustomerAccountBalance.Saving', N'en-US', N'SAVING...' UNION ALL

    SELECT N'CustomerAccountBalance.Update', N'vi-VN', N'CẬP NHẬT' UNION ALL
    SELECT N'CustomerAccountBalance.Update', N'en-US', N'UPDATE' UNION ALL

    SELECT N'CustomerAccountBalance.AddNew', N'vi-VN', N'THÊM MỚI' UNION ALL
    SELECT N'CustomerAccountBalance.AddNew', N'en-US', N'ADD NEW' UNION ALL

    SELECT N'CustomerAccountBalance.CancelEdit', N'vi-VN', N'Hủy chỉnh sửa' UNION ALL
    SELECT N'CustomerAccountBalance.CancelEdit', N'en-US', N'Cancel edit' UNION ALL

    SELECT N'CustomerAccountBalance.DetailTitle', N'vi-VN', N'Chi tiết số dư khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.DetailTitle', N'en-US', N'Customer balance details' UNION ALL

    SELECT N'CustomerAccountBalance.TotalRows', N'vi-VN', N'Tổng {0} dòng' UNION ALL
    SELECT N'CustomerAccountBalance.TotalRows', N'en-US', N'Total {0} rows' UNION ALL

    SELECT N'CustomerAccountBalance.ColCustomerCode', N'vi-VN', N'Mã KH' UNION ALL
    SELECT N'CustomerAccountBalance.ColCustomerCode', N'en-US', N'Customer code' UNION ALL

    SELECT N'CustomerAccountBalance.ColCustomerName', N'vi-VN', N'Tên khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.ColCustomerName', N'en-US', N'Customer name' UNION ALL

    SELECT N'CustomerAccountBalance.ColPeriod', N'vi-VN', N'Kỳ' UNION ALL
    SELECT N'CustomerAccountBalance.ColPeriod', N'en-US', N'Period' UNION ALL

    SELECT N'CustomerAccountBalance.ColUpdated', N'vi-VN', N'Cập nhật' UNION ALL
    SELECT N'CustomerAccountBalance.ColUpdated', N'en-US', N'Updated' UNION ALL

    SELECT N'CustomerAccountBalance.ColActions', N'vi-VN', N'Thao tác' UNION ALL
    SELECT N'CustomerAccountBalance.ColActions', N'en-US', N'Actions' UNION ALL

    SELECT N'CustomerAccountBalance.NoRecords', N'vi-VN', N'Không có dữ liệu phù hợp bộ lọc.' UNION ALL
    SELECT N'CustomerAccountBalance.NoRecords', N'en-US', N'No data matches the filters.' UNION ALL

    SELECT N'CustomerAccountBalance.Loading', N'vi-VN', N'Đang tải dữ liệu...' UNION ALL
    SELECT N'CustomerAccountBalance.Loading', N'en-US', N'Loading data...' UNION ALL

    SELECT N'CustomerAccountBalance.DefaultBookCode', N'vi-VN', N'(Mặc định)' UNION ALL
    SELECT N'CustomerAccountBalance.DefaultBookCode', N'en-US', N'(Default)' UNION ALL

    SELECT N'CustomerAccountBalance.NoCustomerName', N'vi-VN', N'(Không có tên)' UNION ALL
    SELECT N'CustomerAccountBalance.NoCustomerName', N'en-US', N'(No name)' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyHintClaimValue', N'vi-VN', N'Lấy từ claim đăng nhập: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyHintClaimValue', N'en-US', N'From login claim: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyHintDefault', N'vi-VN', N'Đã chọn mặc định: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyHintDefault', N'en-US', N'Default selected: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyHintNone', N'vi-VN', N'Không có dữ liệu Customer để xác định công ty.' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyHintNone', N'en-US', N'No Customer data available to resolve company.' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyHintManual', N'vi-VN', N'Đã chọn CompanyId thủ công.' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyHintManual', N'en-US', N'CompanyId selected manually.' UNION ALL

    SELECT N'CustomerAccountBalance.CompanyHintSelected', N'vi-VN', N'Đã chọn: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.CompanyHintSelected', N'en-US', N'Selected: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.LoadFailed', N'vi-VN', N'Không thể tải Customer Account Balance: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.LoadFailed', N'en-US', N'Failed to load Customer Account Balance: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.SaveCustomerRequired', N'vi-VN', N'Chưa chọn khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveCustomerRequired', N'en-US', N'Please select a customer.' UNION ALL

    SELECT N'CustomerAccountBalance.SaveOpeningBothSides', N'vi-VN', N'Đầu kỳ không được đồng thời có cả số dư Nợ và số dư Có.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveOpeningBothSides', N'en-US', N'Opening cannot have both Debit and Credit balances.' UNION ALL

    SELECT N'CustomerAccountBalance.SaveUpdated', N'vi-VN', N'Đã cập nhật số dư khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveUpdated', N'en-US', N'Customer balance updated.' UNION ALL

    SELECT N'CustomerAccountBalance.SaveAdded', N'vi-VN', N'Đã thêm số dư khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveAdded', N'en-US', N'Customer balance added.' UNION ALL

    SELECT N'CustomerAccountBalance.SaveDuplicate', N'vi-VN', N'Khách hàng đã có số dư cho cùng công ty, tài khoản, năm, tháng và mã sổ.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveDuplicate', N'en-US', N'Customer already has a balance for the same company, account, year, month and book code.' UNION ALL

    SELECT N'CustomerAccountBalance.SaveNotFound', N'vi-VN', N'Không tìm thấy dòng cần cập nhật hoặc dòng thuộc công ty khác.' UNION ALL
    SELECT N'CustomerAccountBalance.SaveNotFound', N'en-US', N'Row not found or belongs to another company.' UNION ALL

    SELECT N'CustomerAccountBalance.DeleteTitle', N'vi-VN', N'Xóa số dư khách hàng' UNION ALL
    SELECT N'CustomerAccountBalance.DeleteTitle', N'en-US', N'Delete customer balance' UNION ALL

    SELECT N'CustomerAccountBalance.DeleteConfirm', N'vi-VN', N'Xóa số dư của {0} - {1}, tài khoản {2}, tháng {3}/{4}?' UNION ALL
    SELECT N'CustomerAccountBalance.DeleteConfirm', N'en-US', N'Delete balance of {0} - {1}, account {2}, month {3}/{4}?' UNION ALL

    SELECT N'CustomerAccountBalance.Delete', N'vi-VN', N'Xóa' UNION ALL
    SELECT N'CustomerAccountBalance.Delete', N'en-US', N'Delete' UNION ALL

    SELECT N'CustomerAccountBalance.Cancel', N'vi-VN', N'Hủy' UNION ALL
    SELECT N'CustomerAccountBalance.Cancel', N'en-US', N'Cancel' UNION ALL

    SELECT N'CustomerAccountBalance.Deleted', N'vi-VN', N'Đã xóa số dư khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.Deleted', N'en-US', N'Customer balance deleted.' UNION ALL

    SELECT N'CustomerAccountBalance.DeleteNotFound', N'vi-VN', N'Không tìm thấy dòng cần xóa hoặc dòng thuộc công ty khác.' UNION ALL
    SELECT N'CustomerAccountBalance.DeleteNotFound', N'en-US', N'Row not found or belongs to another company.' UNION ALL

    SELECT N'CustomerAccountBalance.SyncTitle', N'vi-VN', N'Cập nhật Account Balance' UNION ALL
    SELECT N'CustomerAccountBalance.SyncTitle', N'en-US', N'Update Account Balance' UNION ALL

    SELECT N'CustomerAccountBalance.SyncConfirm', N'vi-VN', N'Cộng toàn bộ số dư khách hàng của tài khoản {0}, tháng {1}/{2}, sổ {3} và ghi một dòng tổng hợp vào account_balance?' UNION ALL
    SELECT N'CustomerAccountBalance.SyncConfirm', N'en-US', N'Sum all customer balances for account {0}, month {1}/{2}, book {3} and write one aggregated row to account_balance?' UNION ALL

    SELECT N'CustomerAccountBalance.SyncNoData', N'vi-VN', N'Không có số dư khách hàng phù hợp để cộng tổng.' UNION ALL
    SELECT N'CustomerAccountBalance.SyncNoData', N'en-US', N'No matching customer balances to aggregate.' UNION ALL

    SELECT N'CustomerAccountBalance.SyncDuplicateAccountBalance', N'vi-VN', N'account_balance đang có nhiều hơn một dòng cho cùng tài khoản/kỳ/mã sổ. Hãy xử lý dữ liệu trùng trước khi cập nhật.' UNION ALL
    SELECT N'CustomerAccountBalance.SyncDuplicateAccountBalance', N'en-US', N'account_balance has more than one row for the same account/period/book. Resolve duplicates before updating.' UNION ALL

    SELECT N'CustomerAccountBalance.SyncSuccess', N'vi-VN', N'Đã cập nhật account_balance từ {0} dòng của {1} khách hàng.' UNION ALL
    SELECT N'CustomerAccountBalance.SyncSuccess', N'en-US', N'Updated account_balance from {0} rows of {1} customers.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportReadSuccess', N'vi-VN', N'Đã đọc {0} dòng; khớp MST {1}, không khớp/trùng {2}, lỗi Excel {3}.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportReadSuccess', N'en-US', N'Read {0} rows; matched tax {1}, unmatched/duplicate {2}, Excel errors {3}.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportReadFailed', N'vi-VN', N'Không thể đọc file: {0}' UNION ALL
    SELECT N'CustomerAccountBalance.ImportReadFailed', N'en-US', N'Cannot read file: {0}' UNION ALL

    SELECT N'CustomerAccountBalance.ImportEmptyTax', N'vi-VN', N'Mã KH/MST trong file đang trống.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportEmptyTax', N'en-US', N'Customer/tax code in file is empty.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportTaxNotFound', N'vi-VN', N'Không tìm thấy Customer có TaxCode = {0}.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportTaxNotFound', N'en-US', N'No Customer found with TaxCode = {0}.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportTaxDuplicate', N'vi-VN', N'TaxCode {0} đang trùng trên {1} Customer.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportTaxDuplicate', N'en-US', N'TaxCode {0} is duplicated across {1} Customers.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportNoValidRows', N'vi-VN', N'Không có dòng khách hàng hợp lệ để import.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportNoValidRows', N'en-US', N'No valid customer rows to import.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportConfirmTitle', N'vi-VN', N'Xác nhận import Customer Account Balance' UNION ALL
    SELECT N'CustomerAccountBalance.ImportConfirmTitle', N'en-US', N'Confirm Customer Account Balance import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportActionReject', N'vi-VN', N'thêm mới và báo lỗi nếu đã tồn tại' UNION ALL
    SELECT N'CustomerAccountBalance.ImportActionReject', N'en-US', N'insert and error if already exists' UNION ALL

    SELECT N'CustomerAccountBalance.ImportActionUpdate', N'vi-VN', N'thêm mới hoặc cập nhật dữ liệu hiện có' UNION ALL
    SELECT N'CustomerAccountBalance.ImportActionUpdate', N'en-US', N'insert or update existing data' UNION ALL

    SELECT N'CustomerAccountBalance.ImportActionReplace', N'vi-VN', N'xóa toàn bộ dữ liệu cùng tài khoản/kỳ/sổ rồi nhập lại' UNION ALL
    SELECT N'CustomerAccountBalance.ImportActionReplace', N'en-US', N'delete all data for the same account/period/book then re-import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportActionDefault', N'vi-VN', N'import' UNION ALL
    SELECT N'CustomerAccountBalance.ImportActionDefault', N'en-US', N'import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportConfirmBody', N'vi-VN', N'Hệ thống sẽ {0} cho {1} khách hàng, tài khoản {2}, tháng {3}/{4}, sổ {5}. {6}' UNION ALL
    SELECT N'CustomerAccountBalance.ImportConfirmBody', N'en-US', N'The system will {0} for {1} customers, account {2}, month {3}/{4}, book {5}. {6}' UNION ALL

    SELECT N'CustomerAccountBalance.ImportConfirmSyncYes', N'vi-VN', N'Sau đó hệ thống tự cập nhật một dòng tổng hợp vào account_balance.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportConfirmSyncYes', N'en-US', N'Then the system will auto-update one aggregated row in account_balance.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportConfirmSyncNo', N'vi-VN', N'Không tự cập nhật account_balance.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportConfirmSyncNo', N'en-US', N'Will not auto-update account_balance.' UNION ALL

    SELECT N'CustomerAccountBalance.Import', N'vi-VN', N'Import' UNION ALL
    SELECT N'CustomerAccountBalance.Import', N'en-US', N'Import' UNION ALL

    SELECT N'CustomerAccountBalance.ImportSuccessSnackbar', N'vi-VN', N'Import thành công {0} khách hàng: thêm {1}, cập nhật {2}, xóa {3}.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportSuccessSnackbar', N'en-US', N'Successfully imported {0} customers: inserted {1}, updated {2}, deleted {3}.' UNION ALL

    SELECT N'CustomerAccountBalance.PreviewMissing', N'vi-VN', N'Dữ liệu preview không còn tồn tại.' UNION ALL
    SELECT N'CustomerAccountBalance.PreviewMissing', N'en-US', N'Preview data no longer exists.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportDuplicateRows', N'vi-VN', N'Khách hàng MST {0} có nhiều dòng trùng trong customer_account_balance. Hãy xử lý dữ liệu trùng trước khi import.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportDuplicateRows', N'en-US', N'Customer tax code {0} has duplicate rows in customer_account_balance. Resolve duplicates before import.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportExistingReject', N'vi-VN', N'Khách hàng MST {0} đã có số dư cho tài khoản {1}, tháng {2}/{3}, sổ {4}.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportExistingReject', N'en-US', N'Customer tax code {0} already has a balance for account {1}, month {2}/{3}, book {4}.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportNoDetailForSync', N'vi-VN', N'Không có dữ liệu chi tiết để cập nhật account_balance.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportNoDetailForSync', N'en-US', N'No detail data available to update account_balance.' UNION ALL

    SELECT N'CustomerAccountBalance.TableMissing', N'vi-VN', N'Chưa có bảng dbo.customer_account_balance hoặc bảng thiếu cột. Hãy chạy Create_CustomerAccountBalance.sql trước.' UNION ALL
    SELECT N'CustomerAccountBalance.TableMissing', N'en-US', N'Table dbo.customer_account_balance is missing or incomplete. Run Create_CustomerAccountBalance.sql first.' UNION ALL

    SELECT N'CustomerAccountBalance.ImportUpdateNotFound', N'vi-VN', N'Không tìm thấy dòng Customer Account Balance cần cập nhật.' UNION ALL
    SELECT N'CustomerAccountBalance.ImportUpdateNotFound', N'en-US', N'Customer Account Balance row to update was not found.'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;

PRINT N'CustomerAccountBalance localization resources imported successfully.';
