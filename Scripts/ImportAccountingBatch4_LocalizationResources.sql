/*
  Batch 4 — Menu 10:
  - 10.4.1 AccountingVouchers_5_8_4_Index.razor (child row grid)
  - 10.6.1 GeneralLedgerSummary_10_6_1.razor (grid column titles)
  - 10.5 TransactionTypes_Index.razor
  - 10.6.2 AccountLedger_10_6_2.razor
  - 10.13.3 TAccountLedger.razor
  - 10.4.4 FixedAssets.razor (Fa1044.*)
  - 10.4.5 FixedAssetDepreciationCalculation.razor (Fadc1045.*)
  - 10.9.1 AccountBalanceSnapshot_10_9_1.razor (Abs1091.*)
  - 10.12.1 VATInputReport (VatIn121.*)
  - 10.12.2 VATOutputReport (VatOut122.*)
  - 10.12.3 VATReconciliation (VatRec123.*)
  - 10.13.1 ExecutiveDashboard (Exd131.*)
  - 10.13.2 CustomerProfitReport (Cpr132.*)
  - 10.14.1 DebtDueNotification (Ddn141.*)
  - 10.14.2 StatementOfAccount (Soa142.*)
  - 10.14.3 DebtAging (Dag143.*)
  - 10.15 AccountFlowTree (Aft151.*)
  - 10.16 CashFlowT (Cft161.*)

  Reuses: GeneralLedgerEntries.*, Gls1061.* (shared filters/toolbar), NoPermission_View, Error_Message
  Safe to re-run.
*/
SET NOCOUNT ON;
BEGIN TRANSACTION;

;WITH src AS (
    -- Av584 child row grid
    SELECT N'Av584.Child_LineNo' AS ResourceKey, N'vi-VN' AS Culture, N'STT' AS Value UNION ALL
    SELECT N'Av584.Child_LineNo', N'en-US', N'#' UNION ALL
    SELECT N'Av584.Child_AccountCode', N'vi-VN', N'TK' UNION ALL
    SELECT N'Av584.Child_AccountCode', N'en-US', N'Account' UNION ALL
    SELECT N'Av584.Child_AccountName', N'vi-VN', N'Tên TK' UNION ALL
    SELECT N'Av584.Child_AccountName', N'en-US', N'Account name' UNION ALL
    SELECT N'Av584.Child_Description', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'Av584.Child_Description', N'en-US', N'Description' UNION ALL
    SELECT N'Av584.Child_Customer', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Av584.Child_Customer', N'en-US', N'Customer' UNION ALL
    SELECT N'Av584.Child_ShipmentHbl', N'vi-VN', N'Lô hàng/HBL' UNION ALL
    SELECT N'Av584.Child_ShipmentHbl', N'en-US', N'Shipment/HBL' UNION ALL
    SELECT N'Av584.Child_HblNo', N'vi-VN', N'Số HBL' UNION ALL
    SELECT N'Av584.Child_HblNo', N'en-US', N'HBL No' UNION ALL
    SELECT N'Av584.Child_BookingNo', N'vi-VN', N'Số booking' UNION ALL
    SELECT N'Av584.Child_BookingNo', N'en-US', N'Booking No' UNION ALL
    SELECT N'Av584.Child_POR', N'vi-VN', N'POR' UNION ALL
    SELECT N'Av584.Child_POR', N'en-US', N'POR' UNION ALL
    SELECT N'Av584.Child_POL', N'vi-VN', N'POL' UNION ALL
    SELECT N'Av584.Child_POL', N'en-US', N'POL' UNION ALL
    SELECT N'Av584.Child_POD', N'vi-VN', N'POD' UNION ALL
    SELECT N'Av584.Child_POD', N'en-US', N'POD' UNION ALL
    SELECT N'Av584.Child_DEL', N'vi-VN', N'DEL' UNION ALL
    SELECT N'Av584.Child_DEL', N'en-US', N'DEL' UNION ALL
    SELECT N'Av584.Child_ContainerNo', N'vi-VN', N'Số container' UNION ALL
    SELECT N'Av584.Child_ContainerNo', N'en-US', N'Container No' UNION ALL
    SELECT N'Av584.Child_SizeType', N'vi-VN', N'Kích cỡ/Loại' UNION ALL
    SELECT N'Av584.Child_SizeType', N'en-US', N'Size/Type' UNION ALL
    SELECT N'Av584.Child_Sale', N'vi-VN', N'Sale' UNION ALL
    SELECT N'Av584.Child_Sale', N'en-US', N'Sale' UNION ALL
    SELECT N'Av584.Child_Mode', N'vi-VN', N'Phương thức' UNION ALL
    SELECT N'Av584.Child_Mode', N'en-US', N'Mode' UNION ALL
    SELECT N'Av584.Child_DueDate', N'vi-VN', N'Hạn nợ' UNION ALL
    SELECT N'Av584.Child_DueDate', N'en-US', N'Due date' UNION ALL
    SELECT N'Av584.Child_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Av584.Child_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Av584.Child_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Av584.Child_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Av584.Child_Ledger', N'vi-VN', N'Sổ' UNION ALL
    SELECT N'Av584.Child_Ledger', N'en-US', N'Ledger' UNION ALL
    SELECT N'Av584.Child_Vat', N'vi-VN', N'VAT' UNION ALL
    SELECT N'Av584.Child_Vat', N'en-US', N'VAT' UNION ALL
    SELECT N'Av584.Child_InvoiceNo', N'vi-VN', N'Số HĐ' UNION ALL
    SELECT N'Av584.Child_InvoiceNo', N'en-US', N'Invoice No' UNION ALL
    SELECT N'Av584.Child_TotalLines', N'vi-VN', N'Tổng dòng: {0}' UNION ALL
    SELECT N'Av584.Child_TotalLines', N'en-US', N'Total lines: {0}' UNION ALL
    SELECT N'Av584.Child_TotalDebit', N'vi-VN', N'Tổng Nợ: {0}' UNION ALL
    SELECT N'Av584.Child_TotalDebit', N'en-US', N'Total debit: {0}' UNION ALL
    SELECT N'Av584.Child_TotalCredit', N'vi-VN', N'Tổng Có: {0}' UNION ALL
    SELECT N'Av584.Child_TotalCredit', N'en-US', N'Total credit: {0}' UNION ALL
    SELECT N'Av584.Child_Difference', N'vi-VN', N'Chênh lệch: {0}' UNION ALL
    SELECT N'Av584.Child_Difference', N'en-US', N'Difference: {0}' UNION ALL

    -- Gls1061 grid columns (preview + main)
    SELECT N'Gls1061.Col_RowNo', N'vi-VN', N'#' UNION ALL
    SELECT N'Gls1061.Col_RowNo', N'en-US', N'#' UNION ALL
    SELECT N'Gls1061.Col_Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'Gls1061.Col_Status', N'en-US', N'Status' UNION ALL
    SELECT N'Gls1061.Col_PostingDate', N'vi-VN', N'Ngày HT' UNION ALL
    SELECT N'Gls1061.Col_PostingDate', N'en-US', N'Posting date' UNION ALL
    SELECT N'Gls1061.Col_VoucherNoShort', N'vi-VN', N'Số CT' UNION ALL
    SELECT N'Gls1061.Col_VoucherNoShort', N'en-US', N'Voucher no' UNION ALL
    SELECT N'Gls1061.Col_VoucherDateShort', N'vi-VN', N'Ngày CT' UNION ALL
    SELECT N'Gls1061.Col_VoucherDateShort', N'en-US', N'Voucher date' UNION ALL
    SELECT N'Gls1061.Col_AccountCodeShort', N'vi-VN', N'TK' UNION ALL
    SELECT N'Gls1061.Col_AccountCodeShort', N'en-US', N'Account' UNION ALL
    SELECT N'Gls1061.Col_CustomerId', N'vi-VN', N'CustomerId' UNION ALL
    SELECT N'Gls1061.Col_CustomerId', N'en-US', N'CustomerId' UNION ALL
    SELECT N'Gls1061.Col_CustomerCode', N'vi-VN', N'Mã ĐT' UNION ALL
    SELECT N'Gls1061.Col_CustomerCode', N'en-US', N'Party code' UNION ALL
    SELECT N'Gls1061.Col_CustomerName', N'vi-VN', N'Đối tượng' UNION ALL
    SELECT N'Gls1061.Col_CustomerName', N'en-US', N'Party' UNION ALL
    SELECT N'Gls1061.Col_CompanyId', N'vi-VN', N'CompanyId' UNION ALL
    SELECT N'Gls1061.Col_CompanyId', N'en-US', N'CompanyId' UNION ALL
    SELECT N'Gls1061.Col_Branch', N'vi-VN', N'Chi nhánh' UNION ALL
    SELECT N'Gls1061.Col_Branch', N'en-US', N'Branch' UNION ALL
    SELECT N'Gls1061.Col_HblNo', N'vi-VN', N'HBL' UNION ALL
    SELECT N'Gls1061.Col_HblNo', N'en-US', N'HBL' UNION ALL
    SELECT N'Gls1061.Col_MblNo', N'vi-VN', N'MBL' UNION ALL
    SELECT N'Gls1061.Col_MblNo', N'en-US', N'MBL' UNION ALL
    SELECT N'Gls1061.Col_ContainerNo', N'vi-VN', N'Container' UNION ALL
    SELECT N'Gls1061.Col_ContainerNo', N'en-US', N'Container' UNION ALL
    SELECT N'Gls1061.Col_BookingNo', N'vi-VN', N'Booking' UNION ALL
    SELECT N'Gls1061.Col_BookingNo', N'en-US', N'Booking' UNION ALL
    SELECT N'Gls1061.Col_Message', N'vi-VN', N'Thông báo' UNION ALL
    SELECT N'Gls1061.Col_Message', N'en-US', N'Message' UNION ALL
    SELECT N'Gls1061.Col_VoucherType', N'vi-VN', N'Loại CT' UNION ALL
    SELECT N'Gls1061.Col_VoucherType', N'en-US', N'Voucher type' UNION ALL
    SELECT N'Gls1061.Col_AccountName', N'vi-VN', N'Tên tài khoản' UNION ALL
    SELECT N'Gls1061.Col_AccountName', N'en-US', N'Account name' UNION ALL
    SELECT N'Gls1061.Col_CustomerParty', N'vi-VN', N'Đối tượng / Khách hàng' UNION ALL
    SELECT N'Gls1061.Col_CustomerParty', N'en-US', N'Party / Customer' UNION ALL
    SELECT N'Gls1061.Col_SaleName', N'vi-VN', N'Sale' UNION ALL
    SELECT N'Gls1061.Col_SaleName', N'en-US', N'Sale' UNION ALL
    SELECT N'Gls1061.Col_PolName', N'vi-VN', N'POL' UNION ALL
    SELECT N'Gls1061.Col_PolName', N'en-US', N'POL' UNION ALL
    SELECT N'Gls1061.Col_PodName', N'vi-VN', N'POD' UNION ALL
    SELECT N'Gls1061.Col_PodName', N'en-US', N'POD' UNION ALL

    -- 10.5 Transaction types
    SELECT N'Tt105.Title', N'vi-VN', N'10.5 Loại giao dịch' UNION ALL
    SELECT N'Tt105.Title', N'en-US', N'10.5 Transaction types' UNION ALL
    SELECT N'Tt105.Subtitle', N'vi-VN', N'Loại giao dịch · cấu hình mẫu bút toán tự động' UNION ALL
    SELECT N'Tt105.Subtitle', N'en-US', N'Transaction types · configure automatic journal templates' UNION ALL
    SELECT N'Tt105.Add', N'vi-VN', N'Thêm loại CT' UNION ALL
    SELECT N'Tt105.Add', N'en-US', N'Add type' UNION ALL
    SELECT N'Tt105.Edit', N'vi-VN', N'Sửa' UNION ALL
    SELECT N'Tt105.Edit', N'en-US', N'Edit' UNION ALL
    SELECT N'Tt105.ConfigureLines', N'vi-VN', N'Cấu hình dòng' UNION ALL
    SELECT N'Tt105.ConfigureLines', N'en-US', N'Configure lines' UNION ALL
    SELECT N'Tt105.Kpi_Active', N'vi-VN', N'Loại đang hoạt động' UNION ALL
    SELECT N'Tt105.Kpi_Active', N'en-US', N'Active types' UNION ALL
    SELECT N'Tt105.Kpi_Inactive', N'vi-VN', N'Loại ngưng hoạt động' UNION ALL
    SELECT N'Tt105.Kpi_Inactive', N'en-US', N'Inactive types' UNION ALL
    SELECT N'Tt105.Kpi_MappingLines', N'vi-VN', N'Dòng mapping' UNION ALL
    SELECT N'Tt105.Kpi_MappingLines', N'en-US', N'Mapping lines' UNION ALL
    SELECT N'Tt105.Kpi_Selected', N'vi-VN', N'Đang chọn' UNION ALL
    SELECT N'Tt105.Kpi_Selected', N'en-US', N'Selected' UNION ALL
    SELECT N'Tt105.None', N'vi-VN', N'Chưa chọn' UNION ALL
    SELECT N'Tt105.None', N'en-US', N'None' UNION ALL
    SELECT N'Tt105.GuideTitle', N'vi-VN', N'Hướng dẫn ghi Nợ / Có' UNION ALL
    SELECT N'Tt105.GuideTitle', N'en-US', N'Debit / Credit guide' UNION ALL
    SELECT N'Tt105.GuideSubtitle', N'vi-VN', N'Dùng để kiểm tra nhanh cấu hình dòng hạch toán trước khi hệ thống sinh phiếu kế toán tự động.' UNION ALL
    SELECT N'Tt105.GuideSubtitle', N'en-US', N'Quick-check posting line configuration before the system generates accounting vouchers automatically.' UNION ALL
    SELECT N'Tt105.GuideChip', N'vi-VN', N'Hướng dẫn Nợ / Có' UNION ALL
    SELECT N'Tt105.GuideChip', N'en-US', N'Debit / Credit guide' UNION ALL
    SELECT N'Tt105.DebitGuideTitle', N'vi-VN', N'Bên Nợ thường dùng' UNION ALL
    SELECT N'Tt105.DebitGuideTitle', N'en-US', N'Common debit side' UNION ALL
    SELECT N'Tt105.DebitGuideBody', N'vi-VN', N'Ghi Nợ khi tăng tài sản, tăng chi phí, hoặc giảm công nợ phải trả.' UNION ALL
    SELECT N'Tt105.DebitGuideBody', N'en-US', N'Debit when assets or expenses increase, or payables decrease.' UNION ALL
    SELECT N'Tt105.DebitGuideHint', N'vi-VN', N'Ví dụ: 111, 112, 131, 1331, 156, 632, 641, 642.' UNION ALL
    SELECT N'Tt105.DebitGuideHint', N'en-US', N'Examples: 111, 112, 131, 1331, 156, 632, 641, 642.' UNION ALL
    SELECT N'Tt105.CreditGuideTitle', N'vi-VN', N'Bên Có thường dùng' UNION ALL
    SELECT N'Tt105.CreditGuideTitle', N'en-US', N'Common credit side' UNION ALL
    SELECT N'Tt105.CreditGuideBody', N'vi-VN', N'Ghi Có khi giảm tài sản, tăng doanh thu, hoặc tăng công nợ phải trả.' UNION ALL
    SELECT N'Tt105.CreditGuideBody', N'en-US', N'Credit when assets decrease, revenue increases, or payables increase.' UNION ALL
    SELECT N'Tt105.CreditGuideHint', N'vi-VN', N'Ví dụ: 111, 112, 131, 331, 33311, 511, 411, 421.' UNION ALL
    SELECT N'Tt105.CreditGuideHint', N'en-US', N'Examples: 111, 112, 131, 331, 33311, 511, 411, 421.' UNION ALL
    SELECT N'Tt105.BalanceGuideTitle', N'vi-VN', N'Nguyên tắc bắt buộc' UNION ALL
    SELECT N'Tt105.BalanceGuideTitle', N'en-US', N'Mandatory rule' UNION ALL
    SELECT N'Tt105.BalanceGuideBody', N'vi-VN', N'Mỗi nghiệp vụ phải cân: Tổng Nợ = Tổng Có.' UNION ALL
    SELECT N'Tt105.BalanceGuideBody', N'en-US', N'Each transaction must balance: total debit = total credit.' UNION ALL
    SELECT N'Tt105.BalanceGuideHint', N'vi-VN', N'Nếu lệch, phiếu không nên cho duyệt / ghi sổ.' UNION ALL
    SELECT N'Tt105.BalanceGuideHint', N'en-US', N'If unbalanced, the voucher should not be approved or posted.' UNION ALL
    SELECT N'Tt105.ExampleCol_Transaction', N'vi-VN', N'Nghiệp vụ' UNION ALL
    SELECT N'Tt105.ExampleCol_Transaction', N'en-US', N'Transaction' UNION ALL
    SELECT N'Tt105.ExampleCol_Entry', N'vi-VN', N'Định khoản mẫu' UNION ALL
    SELECT N'Tt105.ExampleCol_Entry', N'en-US', N'Sample entry' UNION ALL
    SELECT N'Tt105.ExampleCol_Explanation', N'vi-VN', N'Giải thích' UNION ALL
    SELECT N'Tt105.ExampleCol_Explanation', N'en-US', N'Explanation' UNION ALL
    SELECT N'Tt105.Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'Tt105.Search', N'en-US', N'Search' UNION ALL
    SELECT N'Tt105.SearchPlaceholder', N'vi-VN', N'Code, tên nghiệp vụ, mô tả...' UNION ALL
    SELECT N'Tt105.SearchPlaceholder', N'en-US', N'Code, transaction name, description...' UNION ALL
    SELECT N'Tt105.SelectedType', N'vi-VN', N'Loại chứng từ đang chọn: {0}' UNION ALL
    SELECT N'Tt105.SelectedType', N'en-US', N'Selected transaction type: {0}' UNION ALL
    SELECT N'Tt105.SelectedDetails', N'vi-VN', N'Chi tiết: {0} dòng | {1}' UNION ALL
    SELECT N'Tt105.SelectedDetails', N'en-US', N'Details: {0} line(s) | {1}' UNION ALL
    SELECT N'Tt105.Active', N'vi-VN', N'Đang hoạt động' UNION ALL
    SELECT N'Tt105.Active', N'en-US', N'Active' UNION ALL
    SELECT N'Tt105.Inactive', N'vi-VN', N'Ngưng hoạt động' UNION ALL
    SELECT N'Tt105.Inactive', N'en-US', N'Inactive' UNION ALL
    SELECT N'Tt105.Col_Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'Tt105.Col_Status', N'en-US', N'Status' UNION ALL
    SELECT N'Tt105.Col_Code', N'vi-VN', N'Mã nghiệp vụ' UNION ALL
    SELECT N'Tt105.Col_Code', N'en-US', N'Transaction code' UNION ALL
    SELECT N'Tt105.Col_Name', N'vi-VN', N'Tên nghiệp vụ' UNION ALL
    SELECT N'Tt105.Col_Name', N'en-US', N'Transaction name' UNION ALL
    SELECT N'Tt105.Col_Description', N'vi-VN', N'Mô tả' UNION ALL
    SELECT N'Tt105.Col_Description', N'en-US', N'Description' UNION ALL
    SELECT N'Tt105.Col_LineCount', N'vi-VN', N'Số dòng' UNION ALL
    SELECT N'Tt105.Col_LineCount', N'en-US', N'Line count' UNION ALL
    SELECT N'Tt105.Col_Action', N'vi-VN', N'Thao tác' UNION ALL
    SELECT N'Tt105.Col_Action', N'en-US', N'Action' UNION ALL
    SELECT N'Tt105.Details', N'vi-VN', N'Chi tiết' UNION ALL
    SELECT N'Tt105.Details', N'en-US', N'Details' UNION ALL
    SELECT N'Tt105.PostingConfig', N'vi-VN', N'Cấu hình hạch toán: {0} - {1}' UNION ALL
    SELECT N'Tt105.PostingConfig', N'en-US', N'Posting configuration: {0} - {1}' UNION ALL
    SELECT N'Tt105.NoMapping', N'vi-VN', N'Chưa có dòng mapping cho nghiệp vụ này.' UNION ALL
    SELECT N'Tt105.NoMapping', N'en-US', N'No mapping lines for this transaction type.' UNION ALL
    SELECT N'Tt105.MappingAlert', N'vi-VN', N'Mỗi dòng bên dưới là một dòng bút toán hệ thống sẽ sinh ra. Đọc theo thứ tự SortOrder: dòng nào ghi Nợ, dòng nào ghi Có, tài khoản nào bị tăng/giảm và vì sao phải ghi như vậy.' UNION ALL
    SELECT N'Tt105.MappingAlert', N'en-US', N'Each row below is a journal line the system will generate. Read by SortOrder: which side is debit/credit, which accounts move, and why.' UNION ALL
    SELECT N'Tt105.Col_LineNo', N'vi-VN', N'Dòng' UNION ALL
    SELECT N'Tt105.Col_LineNo', N'en-US', N'Line' UNION ALL
    SELECT N'Tt105.Col_LineType', N'vi-VN', N'Loại dòng' UNION ALL
    SELECT N'Tt105.Col_LineType', N'en-US', N'Line type' UNION ALL
    SELECT N'Tt105.Col_DebitCredit', N'vi-VN', N'Nợ / Có' UNION ALL
    SELECT N'Tt105.Col_DebitCredit', N'en-US', N'Debit / Credit' UNION ALL
    SELECT N'Tt105.Col_Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'Tt105.Col_Account', N'en-US', N'Account' UNION ALL
    SELECT N'Tt105.Col_AccountName', N'vi-VN', N'Tên tài khoản' UNION ALL
    SELECT N'Tt105.Col_AccountName', N'en-US', N'Account name' UNION ALL
    SELECT N'Tt105.Col_LineMeaning', N'vi-VN', N'Ý nghĩa dòng này' UNION ALL
    SELECT N'Tt105.Col_LineMeaning', N'en-US', N'Line meaning' UNION ALL
    SELECT N'Tt105.Col_PostingExplain', N'vi-VN', N'Giải thích cách hạch toán' UNION ALL
    SELECT N'Tt105.Col_PostingExplain', N'en-US', N'Posting explanation' UNION ALL
    SELECT N'Tt105.Col_SortOrder', N'vi-VN', N'Thứ tự' UNION ALL
    SELECT N'Tt105.Col_SortOrder', N'en-US', N'Sort order' UNION ALL
    SELECT N'Tt105.Validate_CodeName', N'vi-VN', N'Vui lòng nhập Code và Name.' UNION ALL
    SELECT N'Tt105.Validate_CodeName', N'en-US', N'Please enter Code and Name.' UNION ALL
    SELECT N'Tt105.NameExists', N'vi-VN', N'Tên nghiệp vụ đã tồn tại.' UNION ALL
    SELECT N'Tt105.NameExists', N'en-US', N'Transaction name already exists.' UNION ALL
    SELECT N'Tt105.Dialog_Add', N'vi-VN', N'Thêm loại giao dịch' UNION ALL
    SELECT N'Tt105.Dialog_Add', N'en-US', N'Add transaction type' UNION ALL
    SELECT N'Tt105.Dialog_Edit', N'vi-VN', N'Sửa loại giao dịch' UNION ALL
    SELECT N'Tt105.Dialog_Edit', N'en-US', N'Edit transaction type' UNION ALL
    SELECT N'Tt105.Dialog_Details', N'vi-VN', N'Cấu hình dòng chi tiết' UNION ALL
    SELECT N'Tt105.Dialog_Details', N'en-US', N'Configure mapping lines' UNION ALL
    SELECT N'Tt105.Side_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Tt105.Side_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Tt105.Side_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Tt105.Side_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Tt105.Side_Unknown', N'vi-VN', N'Chưa rõ' UNION ALL
    SELECT N'Tt105.Side_Unknown', N'en-US', N'Unknown' UNION ALL
    SELECT N'Tt105.Example1_Title', N'vi-VN', N'Bán dịch vụ / ghi nhận phải thu khách hàng' UNION ALL
    SELECT N'Tt105.Example1_Title', N'en-US', N'Service sale / recognize customer receivable' UNION ALL
    SELECT N'Tt105.Example1_Line1', N'vi-VN', N'Nợ 131 - Phải thu khách hàng' UNION ALL
    SELECT N'Tt105.Example1_Line1', N'en-US', N'Dr 131 - Accounts receivable' UNION ALL
    SELECT N'Tt105.Example1_Line2', N'vi-VN', N'Có 511 - Doanh thu' UNION ALL
    SELECT N'Tt105.Example1_Line2', N'en-US', N'Cr 511 - Revenue' UNION ALL
    SELECT N'Tt105.Example1_Line3', N'vi-VN', N'Có 33311 - Thuế GTGT đầu ra' UNION ALL
    SELECT N'Tt105.Example1_Line3', N'en-US', N'Cr 33311 - Output VAT' UNION ALL
    SELECT N'Tt105.Example1_Explain', N'vi-VN', N'Tăng công nợ khách hàng và ghi nhận doanh thu, thuế GTGT phải nộp.' UNION ALL
    SELECT N'Tt105.Example1_Explain', N'en-US', N'Increase customer receivable and recognize revenue and output VAT payable.' UNION ALL
    SELECT N'Tt105.Example2_Title', N'vi-VN', N'Khách hàng thanh toán' UNION ALL
    SELECT N'Tt105.Example2_Title', N'en-US', N'Customer payment' UNION ALL
    SELECT N'Tt105.Example2_Line1', N'vi-VN', N'Nợ 111/112 - Tiền mặt/ngân hàng' UNION ALL
    SELECT N'Tt105.Example2_Line1', N'en-US', N'Dr 111/112 - Cash/bank' UNION ALL
    SELECT N'Tt105.Example2_Line2', N'vi-VN', N'Có 131 - Phải thu khách hàng' UNION ALL
    SELECT N'Tt105.Example2_Line2', N'en-US', N'Cr 131 - Accounts receivable' UNION ALL
    SELECT N'Tt105.Example2_Explain', N'vi-VN', N'Tiền tăng, đồng thời giảm công nợ phải thu của khách hàng.' UNION ALL
    SELECT N'Tt105.Example2_Explain', N'en-US', N'Cash increases while customer receivable decreases.' UNION ALL
    SELECT N'Tt105.Example3_Title', N'vi-VN', N'Ghi nhận chi phí hãng tàu / vendor' UNION ALL
    SELECT N'Tt105.Example3_Title', N'en-US', N'Recognize carrier / vendor expense' UNION ALL
    SELECT N'Tt105.Example3_Line1', N'vi-VN', N'Nợ 632/641/642 - Chi phí' UNION ALL
    SELECT N'Tt105.Example3_Line1', N'en-US', N'Dr 632/641/642 - Expense' UNION ALL
    SELECT N'Tt105.Example3_Line2', N'vi-VN', N'Nợ 1331 - Thuế GTGT đầu vào' UNION ALL
    SELECT N'Tt105.Example3_Line2', N'en-US', N'Dr 1331 - Input VAT' UNION ALL
    SELECT N'Tt105.Example3_Line3', N'vi-VN', N'Có 331 - Phải trả vendor' UNION ALL
    SELECT N'Tt105.Example3_Line3', N'en-US', N'Cr 331 - Accounts payable' UNION ALL
    SELECT N'Tt105.Example3_Explain', N'vi-VN', N'Ghi nhận chi phí và thuế đầu vào, đồng thời tăng công nợ phải trả vendor.' UNION ALL
    SELECT N'Tt105.Example3_Explain', N'en-US', N'Recognize expense and input VAT while increasing vendor payable.' UNION ALL
    SELECT N'Tt105.Example4_Title', N'vi-VN', N'Thanh toán cho vendor / hãng tàu' UNION ALL
    SELECT N'Tt105.Example4_Title', N'en-US', N'Pay vendor / carrier' UNION ALL
    SELECT N'Tt105.Example4_Line1', N'vi-VN', N'Nợ 331 - Phải trả vendor' UNION ALL
    SELECT N'Tt105.Example4_Line1', N'en-US', N'Dr 331 - Accounts payable' UNION ALL
    SELECT N'Tt105.Example4_Line2', N'vi-VN', N'Có 111/112 - Tiền mặt/ngân hàng' UNION ALL
    SELECT N'Tt105.Example4_Line2', N'en-US', N'Cr 111/112 - Cash/bank' UNION ALL
    SELECT N'Tt105.Example4_Explain', N'vi-VN', N'Giảm công nợ phải trả, đồng thời giảm tiền.' UNION ALL
    SELECT N'Tt105.Example4_Explain', N'en-US', N'Payables decrease while cash decreases.' UNION ALL

    -- 10.6.2 Account ledger
    SELECT N'Al1062.Title', N'vi-VN', N'10.6.2 Sổ cái theo tài khoản' UNION ALL
    SELECT N'Al1062.Title', N'en-US', N'10.6.2 Account ledger' UNION ALL
    SELECT N'Al1062.Subtitle', N'vi-VN', N'Sổ cái tài khoản · xem chi tiết phát sinh và số dư theo từng tài khoản' UNION ALL
    SELECT N'Al1062.Subtitle', N'en-US', N'Account ledger · view movements and balance by account' UNION ALL
    SELECT N'Al1062.AccountLedgerTitle', N'vi-VN', N'SỔ CÁI TÀI KHOẢN {0}' UNION ALL
    SELECT N'Al1062.AccountLedgerTitle', N'en-US', N'ACCOUNT LEDGER {0}' UNION ALL
    SELECT N'Al1062.DateModeCaption', N'vi-VN', N'{0}: {1} - {2} | Sổ: {3}' UNION ALL
    SELECT N'Al1062.DateModeCaption', N'en-US', N'{0}: {1} - {2} | Book: {3}' UNION ALL
    SELECT N'Al1062.ClosingBalance', N'vi-VN', N'Số dư CK: {0} {1}' UNION ALL
    SELECT N'Al1062.ClosingBalance', N'en-US', N'Closing balance: {0} {1}' UNION ALL
    SELECT N'Al1062.OpeningBalance', N'vi-VN', N'Số dư đầu kỳ' UNION ALL
    SELECT N'Al1062.OpeningBalance', N'en-US', N'Opening balance' UNION ALL
    SELECT N'Al1062.TotalDebitMovement', N'vi-VN', N'Tổng phát sinh Nợ' UNION ALL
    SELECT N'Al1062.TotalDebitMovement', N'en-US', N'Total debit movement' UNION ALL
    SELECT N'Al1062.TotalCreditMovement', N'vi-VN', N'Tổng phát sinh Có' UNION ALL
    SELECT N'Al1062.TotalCreditMovement', N'en-US', N'Total credit movement' UNION ALL
    SELECT N'Al1062.ClosingBalanceStat', N'vi-VN', N'Số dư cuối kỳ' UNION ALL
    SELECT N'Al1062.ClosingBalanceStat', N'en-US', N'Closing balance' UNION ALL
    SELECT N'Al1062.DateModeHelpDateRange', N'vi-VN', N'Đang lọc theo ngày hạch toán PostingDate: chọn Từ ngày - Đến ngày, không ép FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Al1062.DateModeHelpDateRange', N'en-US', N'Filtering by PostingDate: choose from/to dates; FiscalYear/FiscalPeriod are not forced.' UNION ALL
    SELECT N'Al1062.Col_Voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'Al1062.Col_Voucher', N'en-US', N'Voucher' UNION ALL
    SELECT N'Al1062.Col_OppositeAccount', N'vi-VN', N'TK đối ứng' UNION ALL
    SELECT N'Al1062.Col_OppositeAccount', N'en-US', N'Contra account' UNION ALL
    SELECT N'Al1062.Col_Balance', N'vi-VN', N'Số dư' UNION ALL
    SELECT N'Al1062.Col_Balance', N'en-US', N'Balance' UNION ALL
    SELECT N'Al1062.Col_BalanceSide', N'vi-VN', N'Dư Nợ/Có' UNION ALL
    SELECT N'Al1062.Col_BalanceSide', N'en-US', N'Dr/Cr balance' UNION ALL
    SELECT N'Al1062.Col_Source', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'Al1062.Col_Source', N'en-US', N'Source' UNION ALL
    SELECT N'Al1062.NoRecords', N'vi-VN', N'Không có phát sinh cho tài khoản này.' UNION ALL
    SELECT N'Al1062.NoRecords', N'en-US', N'No movements for this account.' UNION ALL
    SELECT N'Al1062.NoPermissionView', N'vi-VN', N'Bạn không có quyền xem sổ cái.' UNION ALL
    SELECT N'Al1062.NoPermissionView', N'en-US', N'You do not have permission to view the ledger.' UNION ALL
    SELECT N'Al1062.SelectAccount', N'vi-VN', N'Vui lòng chọn tài khoản.' UNION ALL
    SELECT N'Al1062.SelectAccount', N'en-US', N'Please select an account.' UNION ALL
    SELECT N'Al1062.LoadedCount', N'vi-VN', N'Đã tải {0} dòng sổ cái.' UNION ALL
    SELECT N'Al1062.LoadedCount', N'en-US', N'Loaded {0} ledger rows.' UNION ALL
    SELECT N'Al1062.OpeningDescription', N'vi-VN', N'Số dư đầu kỳ' UNION ALL
    SELECT N'Al1062.OpeningDescription', N'en-US', N'Opening balance' UNION ALL
    SELECT N'Al1062.AccountBalanceSource', N'vi-VN', N'Số dư tài khoản' UNION ALL
    SELECT N'Al1062.AccountBalanceSource', N'en-US', N'Account balance' UNION ALL
    SELECT N'Al1062.Side_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Al1062.Side_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Al1062.Side_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Al1062.Side_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Al1062.Side_Balanced', N'vi-VN', N'Cân' UNION ALL
    SELECT N'Al1062.Side_Balanced', N'en-US', N'Balanced' UNION ALL
    SELECT N'Al1062.NoExportPermission', N'vi-VN', N'Bạn không có quyền xuất sổ chi tiết công nợ.' UNION ALL
    SELECT N'Al1062.NoExportPermission', N'en-US', N'You do not have permission to export the debt ledger.' UNION ALL
    SELECT N'Al1062.SelectDateRange', N'vi-VN', N'Vui lòng chọn Từ ngày / Đến ngày trước khi export.' UNION ALL
    SELECT N'Al1062.SelectDateRange', N'en-US', N'Please select from/to dates before export.' UNION ALL
    SELECT N'Al1062.NoExportData', N'vi-VN', N'Không có dữ liệu theo điều kiện search hiện tại để export.' UNION ALL
    SELECT N'Al1062.NoExportData', N'en-US', N'No data to export for current search filters.' UNION ALL
    SELECT N'Al1062.ExportSuccess', N'vi-VN', N'Đã export sổ chi tiết công nợ: {0} dòng phát sinh.' UNION ALL
    SELECT N'Al1062.ExportSuccess', N'en-US', N'Exported debt ledger detail: {0} movement row(s).' UNION ALL
    SELECT N'Al1062.ExportFailed', N'vi-VN', N'Export Excel thất bại: {0}' UNION ALL
    SELECT N'Al1062.ExportFailed', N'en-US', N'Excel export failed: {0}' UNION ALL

    -- 10.13.3 T-Account ledger
    SELECT N'Tal1063.Title', N'vi-VN', N'10.13.3 Sổ cái chữ T' UNION ALL
    SELECT N'Tal1063.Title', N'en-US', N'10.13.3 T-Account ledger' UNION ALL
    SELECT N'Tal1063.Subtitle', N'vi-VN', N'Sổ cái chữ T lấy chi tiết từ General Ledger' UNION ALL
    SELECT N'Tal1063.Subtitle', N'en-US', N'T-Account ledger sourced from General Ledger' UNION ALL
    SELECT N'Tal1063.Accounts', N'vi-VN', N'Danh mục TK' UNION ALL
    SELECT N'Tal1063.Accounts', N'en-US', N'Accounts' UNION ALL
    SELECT N'Tal1063.AccountCode', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'Tal1063.AccountCode', N'en-US', N'Account code' UNION ALL
    SELECT N'Tal1063.TotalDebit', N'vi-VN', N'Tổng Nợ' UNION ALL
    SELECT N'Tal1063.TotalDebit', N'en-US', N'Total debit' UNION ALL
    SELECT N'Tal1063.TotalCredit', N'vi-VN', N'Tổng Có' UNION ALL
    SELECT N'Tal1063.TotalCredit', N'en-US', N'Total credit' UNION ALL
    SELECT N'Tal1063.Balance', N'vi-VN', N'Số dư' UNION ALL
    SELECT N'Tal1063.Balance', N'en-US', N'Balance' UNION ALL
    SELECT N'Tal1063.BalanceSide', N'vi-VN', N'Bên dư' UNION ALL
    SELECT N'Tal1063.BalanceSide', N'en-US', N'Balance side' UNION ALL
    SELECT N'Tal1063.TAccountTitle', N'vi-VN', N'TK {0} - Sổ cái chữ T' UNION ALL
    SELECT N'Tal1063.TAccountTitle', N'en-US', N'Account {0} - T-Account ledger' UNION ALL
    SELECT N'Tal1063.DebitHeader', N'vi-VN', N'NỢ' UNION ALL
    SELECT N'Tal1063.DebitHeader', N'en-US', N'DEBIT' UNION ALL
    SELECT N'Tal1063.CreditHeader', N'vi-VN', N'CÓ' UNION ALL
    SELECT N'Tal1063.CreditHeader', N'en-US', N'CREDIT' UNION ALL
    SELECT N'Tal1063.NoDebit', N'vi-VN', N'Không có phát sinh Nợ' UNION ALL
    SELECT N'Tal1063.NoDebit', N'en-US', N'No debit movements' UNION ALL
    SELECT N'Tal1063.NoCredit', N'vi-VN', N'Không có phát sinh Có' UNION ALL
    SELECT N'Tal1063.NoCredit', N'en-US', N'No credit movements' UNION ALL
    SELECT N'Tal1063.InvoicePrefix', N'vi-VN', N'HĐ {0}' UNION ALL
    SELECT N'Tal1063.InvoicePrefix', N'en-US', N'Inv {0}' UNION ALL
    SELECT N'Tal1063.FooterDebit', N'vi-VN', N'Tổng Nợ: {0}' UNION ALL
    SELECT N'Tal1063.FooterDebit', N'en-US', N'Total debit: {0}' UNION ALL
    SELECT N'Tal1063.FooterCredit', N'vi-VN', N'Tổng Có: {0}' UNION ALL
    SELECT N'Tal1063.FooterCredit', N'en-US', N'Total credit: {0}' UNION ALL
    SELECT N'Tal1063.ClosingBalance', N'vi-VN', N'Số dư cuối kỳ: {0} {1}' UNION ALL
    SELECT N'Tal1063.ClosingBalance', N'en-US', N'Closing balance: {0} {1}' UNION ALL
    SELECT N'Tal1063.DetailTitle', N'vi-VN', N'Chi tiết General Ledger' UNION ALL
    SELECT N'Tal1063.DetailTitle', N'en-US', N'General Ledger detail' UNION ALL
    SELECT N'Tal1063.DateModeHelpDateRange', N'vi-VN', N'Đang lọc theo ngày: dùng PostingDate từ Từ ngày đến Đến ngày, không ép FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Tal1063.DateModeHelpDateRange', N'en-US', N'Filtering by date: uses PostingDate from/to only; FiscalYear/FiscalPeriod are not forced.' UNION ALL
    SELECT N'Tal1063.DateModeHelpPeriod', N'vi-VN', N'Đang lọc theo kỳ: dùng FiscalYear/FiscalPeriod và tự set Từ ngày - Đến ngày theo tháng.' UNION ALL
    SELECT N'Tal1063.DateModeHelpPeriod', N'en-US', N'Filtering by period: uses FiscalYear/FiscalPeriod and auto-sets from/to dates for the month.' UNION ALL
    SELECT N'Tal1063.NoPermissionView', N'vi-VN', N'Bạn không có quyền xem sổ cái.' UNION ALL
    SELECT N'Tal1063.NoPermissionView', N'en-US', N'You do not have permission to view the ledger.' UNION ALL
    SELECT N'Tal1063.EnterAccountCode', N'vi-VN', N'Vui lòng nhập Account Code.' UNION ALL
    SELECT N'Tal1063.EnterAccountCode', N'en-US', N'Please enter an account code.' UNION ALL
    SELECT N'Tal1063.AccountsLoaded', N'vi-VN', N'Đã tải {0} tài khoản.' UNION ALL
    SELECT N'Tal1063.AccountsLoaded', N'en-US', N'Loaded {0} account(s).' UNION ALL
    SELECT N'Tal1063.LinesLoaded', N'vi-VN', N'Đã tải {0} dòng sổ cái.' UNION ALL
    SELECT N'Tal1063.LinesLoaded', N'en-US', N'Loaded {0} ledger line(s).' UNION ALL
    SELECT N'Tal1063.OpeningDay', N'vi-VN', N'Số dư đầu ngày' UNION ALL
    SELECT N'Tal1063.OpeningDay', N'en-US', N'Opening balance (day)' UNION ALL
    SELECT N'Tal1063.OpeningPeriod', N'vi-VN', N'Số dư đầu kỳ' UNION ALL
    SELECT N'Tal1063.OpeningPeriod', N'en-US', N'Opening balance (period)' UNION ALL
    SELECT N'Tal1063.Col_PostingDate', N'vi-VN', N'Ngày ghi sổ' UNION ALL
    SELECT N'Tal1063.Col_PostingDate', N'en-US', N'Posting date' UNION ALL
    SELECT N'Tal1063.Col_VoucherNo', N'vi-VN', N'Số chứng từ' UNION ALL
    SELECT N'Tal1063.Col_VoucherNo', N'en-US', N'Voucher no' UNION ALL
    SELECT N'Tal1063.Col_Account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'Tal1063.Col_Account', N'en-US', N'Account' UNION ALL
    SELECT N'Tal1063.Col_InvoiceNo', N'vi-VN', N'Số HĐ' UNION ALL
    SELECT N'Tal1063.Col_InvoiceNo', N'en-US', N'Invoice no' UNION ALL
    SELECT N'Tal1063.Col_Description', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'Tal1063.Col_Description', N'en-US', N'Description' UNION ALL
    SELECT N'Tal1063.Col_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Tal1063.Col_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Tal1063.Col_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Tal1063.Col_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Tal1063.Side_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Tal1063.Side_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Tal1063.Side_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Tal1063.Side_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Tal1063.Side_Balanced', N'vi-VN', N'Cân' UNION ALL
    SELECT N'Tal1063.Side_Balanced', N'en-US', N'Balanced' UNION ALL

    -- Shared report grid/snackbar (reuse in code via Gls1061 where noted)
    SELECT N'Gls1061.Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'Gls1061.Search', N'en-US', N'Search' UNION ALL
    SELECT N'Gls1061.View', N'vi-VN', N'Xem' UNION ALL
    SELECT N'Gls1061.View', N'en-US', N'View' UNION ALL
    SELECT N'Gls1061.Col_Debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'Gls1061.Col_Debit', N'en-US', N'Debit' UNION ALL
    SELECT N'Gls1061.Col_Credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'Gls1061.Col_Credit', N'en-US', N'Credit' UNION ALL
    SELECT N'Gls1061.Col_Currency', N'vi-VN', N'Tiền tệ' UNION ALL
    SELECT N'Gls1061.Col_Currency', N'en-US', N'Currency' UNION ALL
    SELECT N'Gls1061.Col_ExchangeRate', N'vi-VN', N'Tỷ giá' UNION ALL
    SELECT N'Gls1061.Col_ExchangeRate', N'en-US', N'Exchange rate' UNION ALL
    SELECT N'Gls1061.Col_CreatedBy', N'vi-VN', N'Người tạo' UNION ALL
    SELECT N'Gls1061.Col_CreatedBy', N'en-US', N'Created by' UNION ALL
    SELECT N'Gls1061.Col_InvoiceNo', N'vi-VN', N'Số hóa đơn' UNION ALL
    SELECT N'Gls1061.Col_InvoiceNo', N'en-US', N'Invoice no' UNION ALL
    SELECT N'Gls1061.Col_InvoiceDate', N'vi-VN', N'Ngày hóa đơn' UNION ALL
    SELECT N'Gls1061.Col_InvoiceDate', N'en-US', N'Invoice date' UNION ALL
    SELECT N'Gls1061.NoLedgerTab', N'vi-VN', N'Không mở được tab 10.6.1 vì chưa truyền OnTabRequest.' UNION ALL
    SELECT N'Gls1061.NoLedgerTab', N'en-US', N'Cannot open 10.6.1 tab: OnTabRequest was not provided.' UNION ALL
    SELECT N'Gls1061.NoRowForLedger', N'vi-VN', N'Không có dòng để mở sổ cái.' UNION ALL
    SELECT N'Gls1061.NoRowForLedger', N'en-US', N'No row to open in ledger.' UNION ALL
    SELECT N'Gls1061.LedgerTabTitle', N'vi-VN', N'10.6.1 Sổ cái tổng hợp - TK {0}' UNION ALL
    SELECT N'Gls1061.LedgerTabTitle', N'en-US', N'10.6.1 General ledger - Acct {0}' UNION ALL
    SELECT N'Gls1061.Col_Action', N'vi-VN', N'Thao tác' UNION ALL
    SELECT N'Gls1061.Col_Action', N'en-US', N'Action' UNION ALL
    SELECT N'Gls1061.Col_Detail', N'vi-VN', N'Chi tiết' UNION ALL
    SELECT N'Gls1061.Col_Detail', N'en-US', N'Detail' UNION ALL
    SELECT N'Gls1061.Col_Type', N'vi-VN', N'Loại' UNION ALL
    SELECT N'Gls1061.Col_Type', N'en-US', N'Type' UNION ALL
    SELECT N'Gls1061.Simulator', N'vi-VN', N'Simulator' UNION ALL
    SELECT N'Gls1061.Simulator', N'en-US', N'Simulator' UNION ALL
    SELECT N'Gls1061.PrintPdf', N'vi-VN', N'In / PDF' UNION ALL
    SELECT N'Gls1061.PrintPdf', N'en-US', N'Print / PDF' UNION ALL

    -- 10.4.4 Fixed Assets (Fa1044.*)
    SELECT N'Fa1044.Title', N'vi-VN', N'Tài sản cố định TT99' UNION ALL
    SELECT N'Fa1044.Title', N'en-US', N'Fixed assets TT99' UNION ALL
    SELECT N'Fa1044.Heading', N'vi-VN', N'Tài sản cố định - TT99' UNION ALL
    SELECT N'Fa1044.Heading', N'en-US', N'Fixed assets - TT99' UNION ALL
    SELECT N'Fa1044.Subtitle', N'vi-VN', N'Khai báo hồ sơ tài sản, nguyên giá, tài khoản và thông tin khấu hao' UNION ALL
    SELECT N'Fa1044.Subtitle', N'en-US', N'Asset master data, original cost, accounts and depreciation settings' UNION ALL
    SELECT N'Fa1044.New', N'vi-VN', N'Tạo mới' UNION ALL
    SELECT N'Fa1044.New', N'en-US', N'New' UNION ALL
    SELECT N'Fa1044.Tab_General', N'vi-VN', N'Thông tin chung' UNION ALL
    SELECT N'Fa1044.Tab_General', N'en-US', N'General info' UNION ALL
    SELECT N'Fa1044.Tab_Cost', N'vi-VN', N'Nguyên giá & chứng từ' UNION ALL
    SELECT N'Fa1044.Tab_Cost', N'en-US', N'Cost & vouchers' UNION ALL
    SELECT N'Fa1044.Tab_Depreciation', N'vi-VN', N'Khấu hao & tài khoản' UNION ALL
    SELECT N'Fa1044.Tab_Depreciation', N'en-US', N'Depreciation & accounts' UNION ALL
    SELECT N'Fa1044.Tab_Status', N'vi-VN', N'Trạng thái & ghi chú' UNION ALL
    SELECT N'Fa1044.Tab_Status', N'en-US', N'Status & notes' UNION ALL
    SELECT N'Fa1044.AssetCode', N'vi-VN', N'Mã tài sản' UNION ALL
    SELECT N'Fa1044.AssetCode', N'en-US', N'Asset code' UNION ALL
    SELECT N'Fa1044.AssetName', N'vi-VN', N'Tên tài sản' UNION ALL
    SELECT N'Fa1044.AssetName', N'en-US', N'Asset name' UNION ALL
    SELECT N'Fa1044.AssetType', N'vi-VN', N'Loại tài sản' UNION ALL
    SELECT N'Fa1044.AssetType', N'en-US', N'Asset type' UNION ALL
    SELECT N'Fa1044.Type_Tangible', N'vi-VN', N'TSCĐ hữu hình' UNION ALL
    SELECT N'Fa1044.Type_Tangible', N'en-US', N'Tangible fixed asset' UNION ALL
    SELECT N'Fa1044.Type_FinanceLease', N'vi-VN', N'TSCĐ thuê tài chính' UNION ALL
    SELECT N'Fa1044.Type_FinanceLease', N'en-US', N'Finance lease asset' UNION ALL
    SELECT N'Fa1044.Type_Intangible', N'vi-VN', N'TSCĐ vô hình' UNION ALL
    SELECT N'Fa1044.Type_Intangible', N'en-US', N'Intangible fixed asset' UNION ALL
    SELECT N'Fa1044.AssetGroup', N'vi-VN', N'Nhóm tài sản' UNION ALL
    SELECT N'Fa1044.AssetGroup', N'en-US', N'Asset group' UNION ALL
    SELECT N'Fa1044.SerialNo', N'vi-VN', N'Số serial' UNION ALL
    SELECT N'Fa1044.SerialNo', N'en-US', N'Serial no' UNION ALL
    SELECT N'Fa1044.Model', N'vi-VN', N'Model' UNION ALL
    SELECT N'Fa1044.Model', N'en-US', N'Model' UNION ALL
    SELECT N'Fa1044.Manufacturer', N'vi-VN', N'Nhà sản xuất' UNION ALL
    SELECT N'Fa1044.Manufacturer', N'en-US', N'Manufacturer' UNION ALL
    SELECT N'Fa1044.Department', N'vi-VN', N'Bộ phận sử dụng' UNION ALL
    SELECT N'Fa1044.Department', N'en-US', N'Department' UNION ALL
    SELECT N'Fa1044.Location', N'vi-VN', N'Vị trí sử dụng' UNION ALL
    SELECT N'Fa1044.Location', N'en-US', N'Location' UNION ALL
    SELECT N'Fa1044.Custodian', N'vi-VN', N'Người quản lý' UNION ALL
    SELECT N'Fa1044.Custodian', N'en-US', N'Custodian' UNION ALL
    SELECT N'Fa1044.Description', N'vi-VN', N'Mô tả tài sản' UNION ALL
    SELECT N'Fa1044.Description', N'en-US', N'Asset description' UNION ALL
    SELECT N'Fa1044.PurchaseDate', N'vi-VN', N'Ngày mua' UNION ALL
    SELECT N'Fa1044.PurchaseDate', N'en-US', N'Purchase date' UNION ALL
    SELECT N'Fa1044.RecognitionDate', N'vi-VN', N'Ngày ghi nhận' UNION ALL
    SELECT N'Fa1044.RecognitionDate', N'en-US', N'Recognition date' UNION ALL
    SELECT N'Fa1044.SupplierCode', N'vi-VN', N'Mã nhà cung cấp' UNION ALL
    SELECT N'Fa1044.SupplierCode', N'en-US', N'Supplier code' UNION ALL
    SELECT N'Fa1044.InvoiceNo', N'vi-VN', N'Số hóa đơn' UNION ALL
    SELECT N'Fa1044.InvoiceNo', N'en-US', N'Invoice no' UNION ALL
    SELECT N'Fa1044.InvoiceDate', N'vi-VN', N'Ngày hóa đơn' UNION ALL
    SELECT N'Fa1044.InvoiceDate', N'en-US', N'Invoice date' UNION ALL
    SELECT N'Fa1044.VoucherNo', N'vi-VN', N'Số chứng từ' UNION ALL
    SELECT N'Fa1044.VoucherNo', N'en-US', N'Voucher no' UNION ALL
    SELECT N'Fa1044.VoucherDate', N'vi-VN', N'Ngày chứng từ' UNION ALL
    SELECT N'Fa1044.VoucherDate', N'en-US', N'Voucher date' UNION ALL
    SELECT N'Fa1044.IsOpeningBalance', N'vi-VN', N'Số dư đầu kỳ/chuyển đổi' UNION ALL
    SELECT N'Fa1044.IsOpeningBalance', N'en-US', N'Opening balance / conversion' UNION ALL
    SELECT N'Fa1044.PurchasePrice', N'vi-VN', N'Giá mua' UNION ALL
    SELECT N'Fa1044.PurchasePrice', N'en-US', N'Purchase price' UNION ALL
    SELECT N'Fa1044.NonRefundableTax', N'vi-VN', N'Thuế không được hoàn lại' UNION ALL
    SELECT N'Fa1044.NonRefundableTax', N'en-US', N'Non-refundable tax' UNION ALL
    SELECT N'Fa1044.TransportCost', N'vi-VN', N'Vận chuyển' UNION ALL
    SELECT N'Fa1044.TransportCost', N'en-US', N'Transport cost' UNION ALL
    SELECT N'Fa1044.InstallationCost', N'vi-VN', N'Lắp đặt/chạy thử' UNION ALL
    SELECT N'Fa1044.InstallationCost', N'en-US', N'Installation / testing' UNION ALL
    SELECT N'Fa1044.OtherDirectCost', N'vi-VN', N'Chi phí trực tiếp khác' UNION ALL
    SELECT N'Fa1044.OtherDirectCost', N'en-US', N'Other direct cost' UNION ALL
    SELECT N'Fa1044.DiscountAmount', N'vi-VN', N'Chiết khấu/giảm giá' UNION ALL
    SELECT N'Fa1044.DiscountAmount', N'en-US', N'Discount' UNION ALL
    SELECT N'Fa1044.RecoverableVat', N'vi-VN', N'VAT được khấu trừ' UNION ALL
    SELECT N'Fa1044.RecoverableVat', N'en-US', N'Recoverable VAT' UNION ALL
    SELECT N'Fa1044.EstimatedOriginalCost', N'vi-VN', N'Nguyên giá dự kiến' UNION ALL
    SELECT N'Fa1044.EstimatedOriginalCost', N'en-US', N'Estimated original cost' UNION ALL
    SELECT N'Fa1044.DepreciationStartDate', N'vi-VN', N'Bắt đầu khấu hao' UNION ALL
    SELECT N'Fa1044.DepreciationStartDate', N'en-US', N'Depreciation start' UNION ALL
    SELECT N'Fa1044.UsefulLifeMonths', N'vi-VN', N'Thời gian sử dụng (tháng)' UNION ALL
    SELECT N'Fa1044.UsefulLifeMonths', N'en-US', N'Useful life (months)' UNION ALL
    SELECT N'Fa1044.DepreciationMethod', N'vi-VN', N'Phương pháp khấu hao' UNION ALL
    SELECT N'Fa1044.DepreciationMethod', N'en-US', N'Depreciation method' UNION ALL
    SELECT N'Fa1044.Method_StraightLine', N'vi-VN', N'Đường thẳng' UNION ALL
    SELECT N'Fa1044.Method_StraightLine', N'en-US', N'Straight line' UNION ALL
    SELECT N'Fa1044.Method_DecliningBalance', N'vi-VN', N'Số dư giảm dần' UNION ALL
    SELECT N'Fa1044.Method_DecliningBalance', N'en-US', N'Declining balance' UNION ALL
    SELECT N'Fa1044.Method_UnitsOfProduction', N'vi-VN', N'Theo sản lượng' UNION ALL
    SELECT N'Fa1044.Method_UnitsOfProduction', N'en-US', N'Units of production' UNION ALL
    SELECT N'Fa1044.Method_NotDepreciated', N'vi-VN', N'Không trích khấu hao' UNION ALL
    SELECT N'Fa1044.Method_NotDepreciated', N'en-US', N'Not depreciated' UNION ALL
    SELECT N'Fa1044.ResidualValue', N'vi-VN', N'Giá trị thu hồi' UNION ALL
    SELECT N'Fa1044.ResidualValue', N'en-US', N'Residual value' UNION ALL
    SELECT N'Fa1044.OpeningAccumDep', N'vi-VN', N'Hao mòn lũy kế đầu kỳ' UNION ALL
    SELECT N'Fa1044.OpeningAccumDep', N'en-US', N'Opening accumulated depreciation' UNION ALL
    SELECT N'Fa1044.AssetAccount', N'vi-VN', N'TK nguyên giá' UNION ALL
    SELECT N'Fa1044.AssetAccount', N'en-US', N'Asset account' UNION ALL
    SELECT N'Fa1044.DepreciationAccount', N'vi-VN', N'TK hao mòn' UNION ALL
    SELECT N'Fa1044.DepreciationAccount', N'en-US', N'Depreciation account' UNION ALL
    SELECT N'Fa1044.ExpenseAccount', N'vi-VN', N'TK chi phí khấu hao' UNION ALL
    SELECT N'Fa1044.ExpenseAccount', N'en-US', N'Depreciation expense account' UNION ALL
    SELECT N'Fa1044.SourceAccount', N'vi-VN', N'TK đối ứng nguồn hình thành' UNION ALL
    SELECT N'Fa1044.SourceAccount', N'en-US', N'Source contra account' UNION ALL
    SELECT N'Fa1044.EstimatedMonthlyDep', N'vi-VN', N'Khấu hao tháng dự kiến' UNION ALL
    SELECT N'Fa1044.EstimatedMonthlyDep', N'en-US', N'Estimated monthly depreciation' UNION ALL
    SELECT N'Fa1044.RemainingValue', N'vi-VN', N'Giá trị còn lại' UNION ALL
    SELECT N'Fa1044.RemainingValue', N'en-US', N'Remaining value' UNION ALL
    SELECT N'Fa1044.Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'Fa1044.Status', N'en-US', N'Status' UNION ALL
    SELECT N'Fa1044.Status_Draft', N'vi-VN', N'Nháp' UNION ALL
    SELECT N'Fa1044.Status_Draft', N'en-US', N'Draft' UNION ALL
    SELECT N'Fa1044.Status_InUse', N'vi-VN', N'Đang sử dụng' UNION ALL
    SELECT N'Fa1044.Status_InUse', N'en-US', N'In use' UNION ALL
    SELECT N'Fa1044.Status_Suspended', N'vi-VN', N'Tạm ngưng' UNION ALL
    SELECT N'Fa1044.Status_Suspended', N'en-US', N'Suspended' UNION ALL
    SELECT N'Fa1044.Status_Disposed', N'vi-VN', N'Đã ghi giảm/thanh lý' UNION ALL
    SELECT N'Fa1044.Status_Disposed', N'en-US', N'Disposed' UNION ALL
    SELECT N'Fa1044.Notes', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'Fa1044.Notes', N'en-US', N'Notes' UNION ALL
    SELECT N'Fa1044.Save', N'vi-VN', N'LƯU' UNION ALL
    SELECT N'Fa1044.Save', N'en-US', N'SAVE' UNION ALL
    SELECT N'Fa1044.Saving', N'vi-VN', N'ĐANG LƯU...' UNION ALL
    SELECT N'Fa1044.Saving', N'en-US', N'SAVING...' UNION ALL
    SELECT N'Fa1044.SaveAndUse', N'vi-VN', N'LƯU & ĐƯA VÀO SỬ DỤNG' UNION ALL
    SELECT N'Fa1044.SaveAndUse', N'en-US', N'SAVE & PUT IN USE' UNION ALL
    SELECT N'Fa1044.ResetForm', N'vi-VN', N'LÀM MỚI' UNION ALL
    SELECT N'Fa1044.ResetForm', N'en-US', N'RESET' UNION ALL
    SELECT N'Fa1044.SearchPlaceholder', N'vi-VN', N'Tìm mã, tên, serial, bộ phận' UNION ALL
    SELECT N'Fa1044.SearchPlaceholder', N'en-US', N'Search code, name, serial, department' UNION ALL
    SELECT N'Fa1044.Reload', N'vi-VN', N'Tải lại' UNION ALL
    SELECT N'Fa1044.Reload', N'en-US', N'Reload' UNION ALL
    SELECT N'Fa1044.Col_RecognitionDate', N'vi-VN', N'Ngày ghi nhận' UNION ALL
    SELECT N'Fa1044.Col_RecognitionDate', N'en-US', N'Recognition date' UNION ALL
    SELECT N'Fa1044.Col_OriginalCost', N'vi-VN', N'Nguyên giá' UNION ALL
    SELECT N'Fa1044.Col_OriginalCost', N'en-US', N'Original cost' UNION ALL
    SELECT N'Fa1044.Col_Remaining', N'vi-VN', N'Còn lại' UNION ALL
    SELECT N'Fa1044.Col_Remaining', N'en-US', N'Remaining' UNION ALL
    SELECT N'Fa1044.NoRecords', N'vi-VN', N'Chưa có dữ liệu tài sản cố định.' UNION ALL
    SELECT N'Fa1044.NoRecords', N'en-US', N'No fixed asset data yet.' UNION ALL
    SELECT N'Fa1044.NotFound', N'vi-VN', N'Không tìm thấy tài sản.' UNION ALL
    SELECT N'Fa1044.NotFound', N'en-US', N'Asset not found.' UNION ALL
    SELECT N'Fa1044.ValidateRequired', N'vi-VN', N'Vui lòng kiểm tra lại các trường bắt buộc.' UNION ALL
    SELECT N'Fa1044.ValidateRequired', N'en-US', N'Please check required fields.' UNION ALL
    SELECT N'Fa1044.SaveSuccess', N'vi-VN', N'Đã lưu tài sản {0} - {1}.' UNION ALL
    SELECT N'Fa1044.SaveSuccess', N'en-US', N'Saved asset {0} - {1}.' UNION ALL
    SELECT N'Fa1044.SaveAndUseSuccess', N'vi-VN', N'Đã lưu và đưa tài sản {0} vào sử dụng.' UNION ALL
    SELECT N'Fa1044.SaveAndUseSuccess', N'en-US', N'Saved and put asset {0} in use.' UNION ALL
    SELECT N'Fa1044.ConcurrencyWarning', N'vi-VN', N'Tài sản đã được người khác cập nhật. Hãy tải lại dữ liệu.' UNION ALL
    SELECT N'Fa1044.ConcurrencyWarning', N'en-US', N'Asset was updated by another user. Please reload.' UNION ALL
    SELECT N'Fa1044.SaveFailed', N'vi-VN', N'Không thể lưu tài sản: {0}' UNION ALL
    SELECT N'Fa1044.SaveFailed', N'en-US', N'Cannot save asset: {0}' UNION ALL
    SELECT N'Fa1044.DeleteTitle', N'vi-VN', N'Xóa tài sản' UNION ALL
    SELECT N'Fa1044.DeleteTitle', N'en-US', N'Delete asset' UNION ALL
    SELECT N'Fa1044.DeleteConfirm', N'vi-VN', N'Xóa tài sản {0} - {1}?' UNION ALL
    SELECT N'Fa1044.DeleteConfirm', N'en-US', N'Delete asset {0} - {1}?' UNION ALL
    SELECT N'Fa1044.DeleteYes', N'vi-VN', N'Xóa' UNION ALL
    SELECT N'Fa1044.DeleteYes', N'en-US', N'Delete' UNION ALL
    SELECT N'Fa1044.Cancel', N'vi-VN', N'Hủy' UNION ALL
    SELECT N'Fa1044.Cancel', N'en-US', N'Cancel' UNION ALL
    SELECT N'Fa1044.DeleteSuccess', N'vi-VN', N'Đã xóa tài sản nháp.' UNION ALL
    SELECT N'Fa1044.DeleteSuccess', N'en-US', N'Draft asset deleted.' UNION ALL
    SELECT N'Fa1044.Val_AssetCodeRequired', N'vi-VN', N'Mã tài sản không được để trống.' UNION ALL
    SELECT N'Fa1044.Val_AssetCodeRequired', N'en-US', N'Asset code is required.' UNION ALL
    SELECT N'Fa1044.Val_AssetNameRequired', N'vi-VN', N'Tên tài sản không được để trống.' UNION ALL
    SELECT N'Fa1044.Val_AssetNameRequired', N'en-US', N'Asset name is required.' UNION ALL
    SELECT N'Fa1044.Val_RecognitionDate', N'vi-VN', N'Ngày ghi nhận tài sản không hợp lệ.' UNION ALL
    SELECT N'Fa1044.Val_RecognitionDate', N'en-US', N'Invalid asset recognition date.' UNION ALL
    SELECT N'Fa1044.Val_DepStartDate', N'vi-VN', N'Ngày bắt đầu khấu hao không hợp lệ.' UNION ALL
    SELECT N'Fa1044.Val_DepStartDate', N'en-US', N'Invalid depreciation start date.' UNION ALL
    SELECT N'Fa1044.Val_DepBeforeRecognition', N'vi-VN', N'Ngày bắt đầu khấu hao không được trước ngày ghi nhận tài sản.' UNION ALL
    SELECT N'Fa1044.Val_DepBeforeRecognition', N'en-US', N'Depreciation start cannot be before recognition date.' UNION ALL
    SELECT N'Fa1044.Val_NegativeMoney', N'vi-VN', N'Các giá trị tiền không được nhỏ hơn 0.' UNION ALL
    SELECT N'Fa1044.Val_NegativeMoney', N'en-US', N'Money values cannot be negative.' UNION ALL
    SELECT N'Fa1044.Val_DiscountTooLarge', N'vi-VN', N'Chiết khấu/giảm giá không được lớn hơn tổng chi phí hình thành tài sản.' UNION ALL
    SELECT N'Fa1044.Val_DiscountTooLarge', N'en-US', N'Discount cannot exceed total asset formation cost.' UNION ALL
    SELECT N'Fa1044.Val_OriginalCostPositive', N'vi-VN', N'Nguyên giá tài sản phải lớn hơn 0.' UNION ALL
    SELECT N'Fa1044.Val_OriginalCostPositive', N'en-US', N'Original cost must be greater than 0.' UNION ALL
    SELECT N'Fa1044.Val_ResidualTooLarge', N'vi-VN', N'Giá trị thu hồi không được lớn hơn nguyên giá.' UNION ALL
    SELECT N'Fa1044.Val_ResidualTooLarge', N'en-US', N'Residual value cannot exceed original cost.' UNION ALL
    SELECT N'Fa1044.Val_OpeningDepTooLarge', N'vi-VN', N'Hao mòn lũy kế đầu kỳ vượt quá giá trị được khấu hao.' UNION ALL
    SELECT N'Fa1044.Val_OpeningDepTooLarge', N'en-US', N'Opening accumulated depreciation exceeds depreciable amount.' UNION ALL
    SELECT N'Fa1044.Val_UsefulLife', N'vi-VN', N'Thời gian sử dụng phải lớn hơn 0 tháng.' UNION ALL
    SELECT N'Fa1044.Val_UsefulLife', N'en-US', N'Useful life must be greater than 0 months.' UNION ALL
    SELECT N'Fa1044.Val_AssetAccountRequired', N'vi-VN', N'Tài khoản nguyên giá không được để trống.' UNION ALL
    SELECT N'Fa1044.Val_AssetAccountRequired', N'en-US', N'Asset account is required.' UNION ALL
    SELECT N'Fa1044.Val_DepAccountRequired', N'vi-VN', N'Tài khoản hao mòn không được để trống.' UNION ALL
    SELECT N'Fa1044.Val_DepAccountRequired', N'en-US', N'Depreciation account is required.' UNION ALL
    SELECT N'Fa1044.Val_ExpenseAccountRequired', N'vi-VN', N'Tài khoản chi phí khấu hao không được để trống.' UNION ALL
    SELECT N'Fa1044.Val_ExpenseAccountRequired', N'en-US', N'Depreciation expense account is required.' UNION ALL
    SELECT N'Fa1044.TypeShort_Tangible', N'vi-VN', N'Hữu hình' UNION ALL
    SELECT N'Fa1044.TypeShort_Tangible', N'en-US', N'Tangible' UNION ALL
    SELECT N'Fa1044.TypeShort_FinanceLease', N'vi-VN', N'Thuê tài chính' UNION ALL
    SELECT N'Fa1044.TypeShort_FinanceLease', N'en-US', N'Finance lease' UNION ALL
    SELECT N'Fa1044.TypeShort_Intangible', N'vi-VN', N'Vô hình' UNION ALL
    SELECT N'Fa1044.TypeShort_Intangible', N'en-US', N'Intangible' UNION ALL
    SELECT N'Fa1044.TypeShort_Other', N'vi-VN', N'Khác' UNION ALL
    SELECT N'Fa1044.TypeShort_Other', N'en-US', N'Other' UNION ALL
    SELECT N'Fa1044.StatusShort_Draft', N'vi-VN', N'Nháp' UNION ALL
    SELECT N'Fa1044.StatusShort_Draft', N'en-US', N'Draft' UNION ALL
    SELECT N'Fa1044.StatusShort_InUse', N'vi-VN', N'Đang sử dụng' UNION ALL
    SELECT N'Fa1044.StatusShort_InUse', N'en-US', N'In use' UNION ALL
    SELECT N'Fa1044.StatusShort_Suspended', N'vi-VN', N'Tạm ngưng' UNION ALL
    SELECT N'Fa1044.StatusShort_Suspended', N'en-US', N'Suspended' UNION ALL
    SELECT N'Fa1044.StatusShort_Disposed', N'vi-VN', N'Đã ghi giảm' UNION ALL
    SELECT N'Fa1044.StatusShort_Disposed', N'en-US', N'Disposed' UNION ALL
    SELECT N'Fa1044.StatusShort_Unknown', N'vi-VN', N'Không xác định' UNION ALL
    SELECT N'Fa1044.StatusShort_Unknown', N'en-US', N'Unknown' UNION ALL

    -- 10.4.5 Fixed Asset Depreciation (Fadc1045.*)
    SELECT N'Fadc1045.Title', N'vi-VN', N'Tính khấu hao TSCĐ' UNION ALL
    SELECT N'Fadc1045.Title', N'en-US', N'Fixed asset depreciation' UNION ALL
    SELECT N'Fadc1045.Heading', N'vi-VN', N'10.4.5 Tính khấu hao TSCĐ' UNION ALL
    SELECT N'Fadc1045.Heading', N'en-US', N'10.4.5 Fixed asset depreciation' UNION ALL
    SELECT N'Fadc1045.Subtitle', N'vi-VN', N'Tính, lưu nháp và ghi sổ khấu hao theo từng kỳ kế toán' UNION ALL
    SELECT N'Fadc1045.Subtitle', N'en-US', N'Calculate, save draft and post depreciation by accounting period' UNION ALL
    SELECT N'Fadc1045.FiscalYear', N'vi-VN', N'Năm tài chính' UNION ALL
    SELECT N'Fadc1045.FiscalYear', N'en-US', N'Fiscal year' UNION ALL
    SELECT N'Fadc1045.FiscalPeriod', N'vi-VN', N'Kỳ kế toán' UNION ALL
    SELECT N'Fadc1045.FiscalPeriod', N'en-US', N'Accounting period' UNION ALL
    SELECT N'Fadc1045.MonthLabel', N'vi-VN', N'Tháng {0}' UNION ALL
    SELECT N'Fadc1045.MonthLabel', N'en-US', N'Month {0}' UNION ALL
    SELECT N'Fadc1045.ProrateByDay', N'vi-VN', N'Phân bổ theo ngày thực tế' UNION ALL
    SELECT N'Fadc1045.ProrateByDay', N'en-US', N'Prorate by actual days' UNION ALL
    SELECT N'Fadc1045.Calculate', N'vi-VN', N'Tính khấu hao' UNION ALL
    SELECT N'Fadc1045.Calculate', N'en-US', N'Calculate depreciation' UNION ALL
    SELECT N'Fadc1045.SaveDraft', N'vi-VN', N'Lưu nháp' UNION ALL
    SELECT N'Fadc1045.SaveDraft', N'en-US', N'Save draft' UNION ALL
    SELECT N'Fadc1045.Post', N'vi-VN', N'Ghi sổ' UNION ALL
    SELECT N'Fadc1045.Post', N'en-US', N'Post' UNION ALL
    SELECT N'Fadc1045.DeleteDraft', N'vi-VN', N'Xóa nháp' UNION ALL
    SELECT N'Fadc1045.DeleteDraft', N'en-US', N'Delete draft' UNION ALL
    SELECT N'Fadc1045.SearchPlaceholder', N'vi-VN', N'Tìm mã, tên tài sản' UNION ALL
    SELECT N'Fadc1045.SearchPlaceholder', N'en-US', N'Search asset code or name' UNION ALL
    SELECT N'Fadc1045.DepartmentFilter', N'vi-VN', N'Lọc bộ phận' UNION ALL
    SELECT N'Fadc1045.DepartmentFilter', N'en-US', N'Filter department' UNION ALL
    SELECT N'Fadc1045.InfoAlert', N'vi-VN', N'Bản này tính tự động phương pháp đường thẳng. Phương pháp số dư giảm dần và theo sản lượng được hiển thị để kiểm tra nhưng chưa tự động tính vì cần thêm hệ số hoặc dữ liệu sản lượng. Lưu nháp không làm thay đổi sổ cái. Chỉ nút Ghi sổ mới tạo chứng từ và GeneralLedgerEntries.' UNION ALL
    SELECT N'Fadc1045.InfoAlert', N'en-US', N'This version auto-calculates straight-line method only. Declining balance and units-of-production are shown for review but not auto-calculated yet. Saving draft does not change the ledger; only Post creates vouchers and GeneralLedgerEntries.' UNION ALL
    SELECT N'Fadc1045.Kpi_VisibleAssets', N'vi-VN', N'Tài sản hiển thị' UNION ALL
    SELECT N'Fadc1045.Kpi_VisibleAssets', N'en-US', N'Visible assets' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedSave', N'vi-VN', N'Đang chọn lưu' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedSave', N'en-US', N'Selected to save' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedOriginalCost', N'vi-VN', N'Tổng nguyên giá đã chọn' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedOriginalCost', N'en-US', N'Selected original cost' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedDepAmount', N'vi-VN', N'Khấu hao kỳ đã chọn' UNION ALL
    SELECT N'Fadc1045.Kpi_SelectedDepAmount', N'en-US', N'Selected period depreciation' UNION ALL
    SELECT N'Fadc1045.Col_Select', N'vi-VN', N'Chọn' UNION ALL
    SELECT N'Fadc1045.Col_Select', N'en-US', N'Select' UNION ALL
    SELECT N'Fadc1045.Col_DepStart', N'vi-VN', N'Bắt đầu KH' UNION ALL
    SELECT N'Fadc1045.Col_DepStart', N'en-US', N'Dep. start' UNION ALL
    SELECT N'Fadc1045.Col_OpeningDep', N'vi-VN', N'HM đầu kỳ' UNION ALL
    SELECT N'Fadc1045.Col_OpeningDep', N'en-US', N'Opening dep.' UNION ALL
    SELECT N'Fadc1045.Col_MonthlyDep', N'vi-VN', N'KH tháng chuẩn' UNION ALL
    SELECT N'Fadc1045.Col_MonthlyDep', N'en-US', N'Standard monthly dep.' UNION ALL
    SELECT N'Fadc1045.Col_DepDays', N'vi-VN', N'Ngày KH' UNION ALL
    SELECT N'Fadc1045.Col_DepDays', N'en-US', N'Dep. days' UNION ALL
    SELECT N'Fadc1045.Col_PeriodDep', N'vi-VN', N'KH kỳ này' UNION ALL
    SELECT N'Fadc1045.Col_PeriodDep', N'en-US', N'This period dep.' UNION ALL
    SELECT N'Fadc1045.Col_ClosingDep', N'vi-VN', N'HM cuối kỳ' UNION ALL
    SELECT N'Fadc1045.Col_ClosingDep', N'en-US', N'Closing dep.' UNION ALL
    SELECT N'Fadc1045.Col_DebitAccount', N'vi-VN', N'TK Nợ' UNION ALL
    SELECT N'Fadc1045.Col_DebitAccount', N'en-US', N'Debit acct' UNION ALL
    SELECT N'Fadc1045.Col_CreditAccount', N'vi-VN', N'TK Có' UNION ALL
    SELECT N'Fadc1045.Col_CreditAccount', N'en-US', N'Credit acct' UNION ALL
    SELECT N'Fadc1045.NoRecords', N'vi-VN', N'Chưa có dữ liệu. Chọn kỳ rồi bấm "Tính khấu hao".' UNION ALL
    SELECT N'Fadc1045.NoRecords', N'en-US', N'No data yet. Select a period and click Calculate depreciation.' UNION ALL
    SELECT N'Fadc1045.NoEligibleAssets', N'vi-VN', N'Không có tài sản đang sử dụng đủ điều kiện tính khấu hao.' UNION ALL
    SELECT N'Fadc1045.NoEligibleAssets', N'en-US', N'No in-use assets eligible for depreciation.' UNION ALL
    SELECT N'Fadc1045.NoSelectedSave', N'vi-VN', N'Chưa chọn tài sản cần lưu khấu hao.' UNION ALL
    SELECT N'Fadc1045.NoSelectedSave', N'en-US', N'No assets selected to save depreciation.' UNION ALL
    SELECT N'Fadc1045.MissingAccounts', N'vi-VN', N'Tài sản {0} chưa đủ tài khoản Nợ/Có.' UNION ALL
    SELECT N'Fadc1045.MissingAccounts', N'en-US', N'Asset {0} is missing debit/credit accounts.' UNION ALL
    SELECT N'Fadc1045.SaveDraftSuccess', N'vi-VN', N'Đã lưu nháp khấu hao cho {0} tài sản kỳ {1:00}/{2}.' UNION ALL
    SELECT N'Fadc1045.SaveDraftSuccess', N'en-US', N'Saved depreciation draft for {0} asset(s), period {1:00}/{2}.' UNION ALL
    SELECT N'Fadc1045.NoSelectedPost', N'vi-VN', N'Chưa chọn bản nháp khấu hao cần ghi sổ.' UNION ALL
    SELECT N'Fadc1045.NoSelectedPost', N'en-US', N'No depreciation draft selected to post.' UNION ALL
    SELECT N'Fadc1045.PostDialogTitle', N'vi-VN', N'Ghi sổ khấu hao' UNION ALL
    SELECT N'Fadc1045.PostDialogTitle', N'en-US', N'Post depreciation' UNION ALL
    SELECT N'Fadc1045.PostDialogMessage', N'vi-VN', N'Ghi sổ {0} tài sản kỳ {1:00}/{2}, tổng khấu hao {3}? Sau khi ghi sổ không thể xóa nháp.' UNION ALL
    SELECT N'Fadc1045.PostDialogMessage', N'en-US', N'Post {0} asset(s) for period {1:00}/{2}, total depreciation {3}? Draft cannot be deleted after posting.' UNION ALL
    SELECT N'Fadc1045.PostSuccess', N'vi-VN', N'Đã ghi sổ chứng từ {0}: {1} tài sản, {2} dòng sổ cái, tổng {3}.' UNION ALL
    SELECT N'Fadc1045.PostSuccess', N'en-US', N'Posted voucher {0}: {1} asset(s), {2} ledger line(s), total {3}.' UNION ALL
    SELECT N'Fadc1045.PostFailed', N'vi-VN', N'Không thể ghi sổ khấu hao: {0}' UNION ALL
    SELECT N'Fadc1045.PostFailed', N'en-US', N'Cannot post depreciation: {0}' UNION ALL
    SELECT N'Fadc1045.NoSelectedDelete', N'vi-VN', N'Chưa chọn bản nháp khấu hao cần xóa.' UNION ALL
    SELECT N'Fadc1045.NoSelectedDelete', N'en-US', N'No depreciation draft selected to delete.' UNION ALL
    SELECT N'Fadc1045.DeleteDialogTitle', N'vi-VN', N'Xóa bản nháp khấu hao' UNION ALL
    SELECT N'Fadc1045.DeleteDialogTitle', N'en-US', N'Delete depreciation draft' UNION ALL
    SELECT N'Fadc1045.DeleteDialogMessage', N'vi-VN', N'Xóa {0} bản nháp kỳ {1:00}/{2}?' UNION ALL
    SELECT N'Fadc1045.DeleteDialogMessage', N'en-US', N'Delete {0} draft(s) for period {1:00}/{2}?' UNION ALL
    SELECT N'Fadc1045.DeleteSuccess', N'vi-VN', N'Đã xóa {0} bản nháp khấu hao.' UNION ALL
    SELECT N'Fadc1045.DeleteSuccess', N'en-US', N'Deleted {0} depreciation draft(s).' UNION ALL
    SELECT N'Fadc1045.Status_Posted', N'vi-VN', N'Đã ghi sổ' UNION ALL
    SELECT N'Fadc1045.Status_Posted', N'en-US', N'Posted' UNION ALL
    SELECT N'Fadc1045.Status_Draft', N'vi-VN', N'Đã lưu nháp' UNION ALL
    SELECT N'Fadc1045.Status_Draft', N'en-US', N'Draft saved' UNION ALL
    SELECT N'Fadc1045.Status_NotCalculated', N'vi-VN', N'Không tính' UNION ALL
    SELECT N'Fadc1045.Status_NotCalculated', N'en-US', N'Not calculated' UNION ALL
    SELECT N'Fadc1045.Status_Unsaved', N'vi-VN', N'Chưa lưu' UNION ALL
    SELECT N'Fadc1045.Status_Unsaved', N'en-US', N'Unsaved' UNION ALL

    -- 10.9.1 Account Balance Snapshot (Abs1091.*)
    SELECT N'Abs1091.Title', N'vi-VN', N'10.9.1 Bảng cân đối tài khoản' UNION ALL
    SELECT N'Abs1091.Title', N'en-US', N'10.9.1 Account balance snapshot' UNION ALL
    SELECT N'Abs1091.Subtitle', N'vi-VN', N'Bảng cân đối tài khoản · Account balance report' UNION ALL
    SELECT N'Abs1091.Subtitle', N'en-US', N'Account balance report' UNION ALL
    SELECT N'Abs1091.RebuildBalance', N'vi-VN', N'Tính lại số dư' UNION ALL
    SELECT N'Abs1091.RebuildBalance', N'en-US', N'Rebuild balance' UNION ALL
    SELECT N'Abs1091.ExportExcel', N'vi-VN', N'Xuất Excel' UNION ALL
    SELECT N'Abs1091.ExportExcel', N'en-US', N'Export Excel' UNION ALL
    SELECT N'Abs1091.SearchPlaceholder', N'vi-VN', N'TK, tên tài khoản...' UNION ALL
    SELECT N'Abs1091.SearchPlaceholder', N'en-US', N'Account, account name...' UNION ALL
    SELECT N'Abs1091.Kpi_OpeningDebit', N'vi-VN', N'Số dư đầu kỳ Nợ' UNION ALL
    SELECT N'Abs1091.Kpi_OpeningDebit', N'en-US', N'Opening debit balance' UNION ALL
    SELECT N'Abs1091.Kpi_OpeningCredit', N'vi-VN', N'Số dư đầu kỳ Có' UNION ALL
    SELECT N'Abs1091.Kpi_OpeningCredit', N'en-US', N'Opening credit balance' UNION ALL
    SELECT N'Abs1091.Kpi_PeriodDebit', N'vi-VN', N'Phát sinh Nợ trong kỳ' UNION ALL
    SELECT N'Abs1091.Kpi_PeriodDebit', N'en-US', N'Period debit movement' UNION ALL
    SELECT N'Abs1091.Kpi_PeriodCredit', N'vi-VN', N'Phát sinh Có trong kỳ' UNION ALL
    SELECT N'Abs1091.Kpi_PeriodCredit', N'en-US', N'Period credit movement' UNION ALL
    SELECT N'Abs1091.Kpi_ClosingDebit', N'vi-VN', N'Số dư cuối kỳ Nợ' UNION ALL
    SELECT N'Abs1091.Kpi_ClosingDebit', N'en-US', N'Closing debit balance' UNION ALL
    SELECT N'Abs1091.Kpi_ClosingCredit', N'vi-VN', N'Số dư cuối kỳ Có' UNION ALL
    SELECT N'Abs1091.Kpi_ClosingCredit', N'en-US', N'Closing credit balance' UNION ALL
    SELECT N'Abs1091.ReportTitle', N'vi-VN', N'BẢNG CÂN ĐỐI TÀI KHOẢN' UNION ALL
    SELECT N'Abs1091.ReportTitle', N'en-US', N'ACCOUNT BALANCE REPORT' UNION ALL
    SELECT N'Abs1091.ReportPeriodDate', N'vi-VN', N'Từ ngày {0} - {1}' UNION ALL
    SELECT N'Abs1091.ReportPeriodDate', N'en-US', N'From {0} to {1}' UNION ALL
    SELECT N'Abs1091.ReportPeriodMonth', N'vi-VN', N'Kỳ {0}/{1}' UNION ALL
    SELECT N'Abs1091.ReportPeriodMonth', N'en-US', N'Period {0}/{1}' UNION ALL
    SELECT N'Abs1091.ReportMeta', N'vi-VN', N'{0} · Sổ {1} · Dòng: {2}' UNION ALL
    SELECT N'Abs1091.ReportMeta', N'en-US', N'{0} · Book {1} · Rows: {2}' UNION ALL
    SELECT N'Abs1091.MovementDifference', N'vi-VN', N'Chênh lệch phát sinh: {0}' UNION ALL
    SELECT N'Abs1091.MovementDifference', N'en-US', N'Movement difference: {0}' UNION ALL
    SELECT N'Abs1091.Col_AccountCode', N'vi-VN', N'Số hiệu TK' UNION ALL
    SELECT N'Abs1091.Col_AccountCode', N'en-US', N'Account code' UNION ALL
    SELECT N'Abs1091.Col_OpeningDebit', N'vi-VN', N'Đầu kỳ Nợ' UNION ALL
    SELECT N'Abs1091.Col_OpeningDebit', N'en-US', N'Opening debit' UNION ALL
    SELECT N'Abs1091.Col_OpeningCredit', N'vi-VN', N'Đầu kỳ Có' UNION ALL
    SELECT N'Abs1091.Col_OpeningCredit', N'en-US', N'Opening credit' UNION ALL
    SELECT N'Abs1091.Col_PeriodDebit', N'vi-VN', N'Phát sinh Nợ' UNION ALL
    SELECT N'Abs1091.Col_PeriodDebit', N'en-US', N'Period debit' UNION ALL
    SELECT N'Abs1091.Col_PeriodCredit', N'vi-VN', N'Phát sinh Có' UNION ALL
    SELECT N'Abs1091.Col_PeriodCredit', N'en-US', N'Period credit' UNION ALL
    SELECT N'Abs1091.Col_ClosingDebit', N'vi-VN', N'Cuối kỳ Nợ' UNION ALL
    SELECT N'Abs1091.Col_ClosingDebit', N'en-US', N'Closing debit' UNION ALL
    SELECT N'Abs1091.Col_ClosingCredit', N'vi-VN', N'Cuối kỳ Có' UNION ALL
    SELECT N'Abs1091.Col_ClosingCredit', N'en-US', N'Closing credit' UNION ALL
    SELECT N'Abs1091.DateModeHelpDateRange', N'vi-VN', N'Đang xem theo ngày: hệ thống tính số dư đầu từ đầu tháng đến trước Từ ngày, phát sinh theo PostingDate trong khoảng ngày chọn.' UNION ALL
    SELECT N'Abs1091.DateModeHelpDateRange', N'en-US', N'Viewing by date: opening balance from month start to day before From date; movements by PostingDate in selected range.' UNION ALL
    SELECT N'Abs1091.DateModeHelpPeriod', N'vi-VN', N'Đang xem theo kỳ: hệ thống lấy số liệu đã tính trong account_balance theo FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Abs1091.DateModeHelpPeriod', N'en-US', N'Viewing by period: uses calculated account_balance by FiscalYear/FiscalPeriod.' UNION ALL
    SELECT N'Abs1091.PeriodNotFound', N'vi-VN', N'Không tìm thấy kỳ kế toán {0}/{1}; dùng ngày mặc định theo tháng.' UNION ALL
    SELECT N'Abs1091.PeriodNotFound', N'en-US', N'Accounting period {0}/{1} not found; using default month dates.' UNION ALL
    SELECT N'Abs1091.PeriodLoadFailed', N'vi-VN', N'Không load được ngày kỳ kế toán: {0}' UNION ALL
    SELECT N'Abs1091.PeriodLoadFailed', N'en-US', N'Cannot load accounting period dates: {0}' UNION ALL
    SELECT N'Abs1091.RebuildPeriodOnly', N'vi-VN', N'Tính lại số dư chỉ áp dụng theo kỳ. Chọn Theo kỳ rồi rebuild.' UNION ALL
    SELECT N'Abs1091.RebuildPeriodOnly', N'en-US', N'Rebuild balance applies to period mode only. Switch to By period.' UNION ALL
    SELECT N'Abs1091.SelectYearMonth', N'vi-VN', N'Vui lòng chọn Year và Month trước khi Rebuild Balance.' UNION ALL
    SELECT N'Abs1091.SelectYearMonth', N'en-US', N'Please select Year and Month before rebuild.' UNION ALL
    SELECT N'Abs1091.RebuildSuccess', N'vi-VN', N'Tính lại số dư thành công kỳ {0}/{1} - Sổ {2}.' UNION ALL
    SELECT N'Abs1091.RebuildSuccess', N'en-US', N'Rebuild balance succeeded for period {0}/{1} - Book {2}.' UNION ALL
    SELECT N'Abs1091.ExportPlaceholder', N'vi-VN', N'Export Excel: gắn hàm export hiện có tại đây.' UNION ALL
    SELECT N'Abs1091.ExportPlaceholder', N'en-US', N'Export Excel: wire existing export function here.' UNION ALL
    SELECT N'Abs1091.NoAccountCode', N'vi-VN', N'Không có mã tài khoản để mở chi tiết.' UNION ALL
    SELECT N'Abs1091.NoAccountCode', N'en-US', N'No account code to open detail.' UNION ALL
    SELECT N'Abs1091.NoTabRequest', N'vi-VN', N'Chưa truyền OnTabRequest từ NavMenu.' UNION ALL
    SELECT N'Abs1091.NoTabRequest', N'en-US', N'OnTabRequest was not passed from NavMenu.'
)
MERGE dbo.LocalizationResources AS tgt
USING src ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN INSERT (ResourceKey, Culture, Value) VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
