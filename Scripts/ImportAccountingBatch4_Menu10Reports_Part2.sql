/*
  Batch 4 Part 2 — Menu 10 report pages (append after Part 1 MERGE or merge manually)
  VatIn121, VatOut122, VatRec123, Exd131, Cpr132, Ddn141, Soa142, Dag143, Aft151, Cft161
*/
SET NOCOUNT ON;
BEGIN TRANSACTION;

;WITH src AS (
    -- 10.12.1 VAT Input
    SELECT N'VatIn121.Title' AS ResourceKey, N'vi-VN' AS Culture, N'10.12.1 Thuế GTGT đầu vào (1331)' AS Value UNION ALL
    SELECT N'VatIn121.Title', N'en-US', N'10.12.1 VAT Input (1331)' UNION ALL
    SELECT N'VatIn121.Subtitle', N'vi-VN', N'Báo cáo thuế GTGT đầu vào' UNION ALL
    SELECT N'VatIn121.Subtitle', N'en-US', N'Input VAT report' UNION ALL
    SELECT N'VatIn121.VatAccount', N'vi-VN', N'TK VAT' UNION ALL
    SELECT N'VatIn121.VatAccount', N'en-US', N'VAT account' UNION ALL
    SELECT N'VatIn121.SearchPlaceholder', N'vi-VN', N'Chứng từ, khách hàng, hóa đơn...' UNION ALL
    SELECT N'VatIn121.SearchPlaceholder', N'en-US', N'Voucher, customer, invoice...' UNION ALL
    SELECT N'VatIn121.Kpi_InputDebit', N'vi-VN', N'VAT đầu vào Nợ' UNION ALL
    SELECT N'VatIn121.Kpi_InputDebit', N'en-US', N'VAT input debit' UNION ALL
    SELECT N'VatIn121.Kpi_InputCredit', N'vi-VN', N'VAT đầu vào Có' UNION ALL
    SELECT N'VatIn121.Kpi_InputCredit', N'en-US', N'VAT input credit' UNION ALL
    SELECT N'VatIn121.Kpi_NetVat', N'vi-VN', N'VAT ròng' UNION ALL
    SELECT N'VatIn121.Kpi_NetVat', N'en-US', N'Net VAT' UNION ALL
    SELECT N'VatIn121.Col_Voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'VatIn121.Col_Voucher', N'en-US', N'Voucher' UNION ALL
    SELECT N'VatIn121.Col_Supplier', N'vi-VN', N'Nhà cung cấp / Đối tượng' UNION ALL
    SELECT N'VatIn121.Col_Supplier', N'en-US', N'Supplier / Party' UNION ALL
    SELECT N'VatIn121.Col_VatDebit', N'vi-VN', N'VAT Nợ' UNION ALL
    SELECT N'VatIn121.Col_VatDebit', N'en-US', N'VAT debit' UNION ALL
    SELECT N'VatIn121.Col_VatCredit', N'vi-VN', N'VAT Có' UNION ALL
    SELECT N'VatIn121.Col_VatCredit', N'en-US', N'VAT credit' UNION ALL
    SELECT N'VatIn121.NoPermission', N'vi-VN', N'Bạn không có quyền xem báo cáo thuế.' UNION ALL
    SELECT N'VatIn121.NoPermission', N'en-US', N'You do not have permission to view tax reports.' UNION ALL
    SELECT N'VatIn121.LoadedCount', N'vi-VN', N'Đã tải {0} dòng VAT đầu vào.' UNION ALL
    SELECT N'VatIn121.LoadedCount', N'en-US', N'Loaded {0} VAT input line(s).' UNION ALL
    SELECT N'VatIn121.NoVatAccount', N'vi-VN', N'Dòng này không có tài khoản VAT để mở sổ cái.' UNION ALL
    SELECT N'VatIn121.NoVatAccount', N'en-US', N'This row has no VAT account to open ledger.' UNION ALL

    -- 10.12.2 VAT Output
    SELECT N'VatOut122.Title', N'vi-VN', N'10.12.2 Thuế GTGT đầu ra (33311)' UNION ALL
    SELECT N'VatOut122.Title', N'en-US', N'10.12.2 VAT Output (33311)' UNION ALL
    SELECT N'VatOut122.Subtitle', N'vi-VN', N'Báo cáo thuế GTGT đầu ra' UNION ALL
    SELECT N'VatOut122.Subtitle', N'en-US', N'Output VAT report' UNION ALL
    SELECT N'VatOut122.VatAccount', N'vi-VN', N'TK VAT' UNION ALL
    SELECT N'VatOut122.VatAccount', N'en-US', N'VAT account' UNION ALL
    SELECT N'VatOut122.VatAccount_33311', N'vi-VN', N'33311 - VAT đầu ra' UNION ALL
    SELECT N'VatOut122.VatAccount_33311', N'en-US', N'33311 - VAT Output' UNION ALL
    SELECT N'VatOut122.VatAccount_33312', N'vi-VN', N'33312 - VAT nhập khẩu' UNION ALL
    SELECT N'VatOut122.VatAccount_33312', N'en-US', N'33312 - VAT Import' UNION ALL
    SELECT N'VatOut122.VatAccount_3331All', N'vi-VN', N'3331* - Tất cả VAT phải nộp' UNION ALL
    SELECT N'VatOut122.VatAccount_3331All', N'en-US', N'3331* - All VAT payable' UNION ALL
    SELECT N'VatOut122.SearchPlaceholder', N'vi-VN', N'Chứng từ, khách hàng, hóa đơn, diễn giải...' UNION ALL
    SELECT N'VatOut122.SearchPlaceholder', N'en-US', N'Voucher, customer, invoice, description...' UNION ALL
    SELECT N'VatOut122.Kpi_OutputCredit', N'vi-VN', N'VAT đầu ra Có' UNION ALL
    SELECT N'VatOut122.Kpi_OutputCredit', N'en-US', N'VAT output credit' UNION ALL
    SELECT N'VatOut122.Kpi_OutputDebit', N'vi-VN', N'VAT đầu ra Nợ' UNION ALL
    SELECT N'VatOut122.Kpi_OutputDebit', N'en-US', N'VAT output debit' UNION ALL
    SELECT N'VatOut122.Kpi_NetPayable', N'vi-VN', N'VAT phải nộp ròng' UNION ALL
    SELECT N'VatOut122.Kpi_NetPayable', N'en-US', N'Net VAT payable' UNION ALL
    SELECT N'VatOut122.Col_Customer', N'vi-VN', N'Khách hàng / Đối tượng' UNION ALL
    SELECT N'VatOut122.Col_Customer', N'en-US', N'Customer / Party' UNION ALL
    SELECT N'VatOut122.Col_NetVat', N'vi-VN', N'VAT phải nộp' UNION ALL
    SELECT N'VatOut122.Col_NetVat', N'en-US', N'VAT payable' UNION ALL
    SELECT N'VatOut122.NoPermission', N'vi-VN', N'Bạn không có quyền xem báo cáo thuế.' UNION ALL
    SELECT N'VatOut122.NoPermission', N'en-US', N'You do not have permission to view tax reports.' UNION ALL
    SELECT N'VatOut122.LoadedCount', N'vi-VN', N'Đã tải {0} dòng VAT đầu ra.' UNION ALL
    SELECT N'VatOut122.LoadedCount', N'en-US', N'Loaded {0} VAT output line(s).' UNION ALL
    SELECT N'VatOut122.NoVatAccount', N'vi-VN', N'Dòng này không có tài khoản VAT để mở sổ cái.' UNION ALL
    SELECT N'VatOut122.NoVatAccount', N'en-US', N'This row has no VAT account to open ledger.' UNION ALL

    -- 10.12.3 VAT Reconciliation
    SELECT N'VatRec123.Title', N'vi-VN', N'10.12.3 Đối chiếu thuế GTGT' UNION ALL
    SELECT N'VatRec123.Title', N'en-US', N'10.12.3 VAT Reconciliation' UNION ALL
    SELECT N'VatRec123.Subtitle', N'vi-VN', N'Đối chiếu thuế GTGT đầu vào / đầu ra · VAT reconciliation' UNION ALL
    SELECT N'VatRec123.Subtitle', N'en-US', N'Input / output VAT reconciliation' UNION ALL
    SELECT N'VatRec123.SearchPlaceholder', N'vi-VN', N'Chứng từ, tài khoản, khách hàng, diễn giải...' UNION ALL
    SELECT N'VatRec123.SearchPlaceholder', N'en-US', N'Voucher, account, customer, description...' UNION ALL
    SELECT N'VatRec123.Kpi_InputDeductible', N'vi-VN', N'VAT đầu vào được khấu trừ' UNION ALL
    SELECT N'VatRec123.Kpi_InputDeductible', N'en-US', N'VAT input deductible' UNION ALL
    SELECT N'VatRec123.Kpi_OutputPayable', N'vi-VN', N'VAT đầu ra phải nộp' UNION ALL
    SELECT N'VatRec123.Kpi_OutputPayable', N'en-US', N'VAT output payable' UNION ALL
    SELECT N'VatRec123.Kpi_NetPosition', N'vi-VN', N'Vị thế VAT ròng' UNION ALL
    SELECT N'VatRec123.Kpi_NetPosition', N'en-US', N'Net VAT position' UNION ALL
    SELECT N'VatRec123.Kpi_ReconStatus', N'vi-VN', N'Trạng thái đối chiếu' UNION ALL
    SELECT N'VatRec123.Kpi_ReconStatus', N'en-US', N'Reconciliation status' UNION ALL
    SELECT N'VatRec123.NetPayable', N'vi-VN', N'Phải nộp' UNION ALL
    SELECT N'VatRec123.NetPayable', N'en-US', N'Payable' UNION ALL
    SELECT N'VatRec123.NetRefundable', N'vi-VN', N'Được hoàn' UNION ALL
    SELECT N'VatRec123.NetRefundable', N'en-US', N'Refundable' UNION ALL
    SELECT N'VatRec123.Status_Balanced', N'vi-VN', N'Cân bằng' UNION ALL
    SELECT N'VatRec123.Status_Balanced', N'en-US', N'Balanced' UNION ALL
    SELECT N'VatRec123.Status_Check', N'vi-VN', N'Kiểm tra' UNION ALL
    SELECT N'VatRec123.Status_Check', N'en-US', N'Check' UNION ALL
    SELECT N'VatRec123.DiffLabel', N'vi-VN', N'Chênh lệch: {0}' UNION ALL
    SELECT N'VatRec123.DiffLabel', N'en-US', N'Diff: {0}' UNION ALL
    SELECT N'VatRec123.SummaryTitle', N'vi-VN', N'Tổng hợp VAT theo tài khoản' UNION ALL
    SELECT N'VatRec123.SummaryTitle', N'en-US', N'VAT summary by account' UNION ALL
    SELECT N'VatRec123.Col_Meaning', N'vi-VN', N'Ý nghĩa' UNION ALL
    SELECT N'VatRec123.Col_Meaning', N'en-US', N'Meaning' UNION ALL
    SELECT N'VatRec123.Col_Net', N'vi-VN', N'Ròng' UNION ALL
    SELECT N'VatRec123.Col_Net', N'en-US', N'Net' UNION ALL
    SELECT N'VatRec123.Col_TaxAccount', N'vi-VN', N'TK Thuế' UNION ALL
    SELECT N'VatRec123.Col_TaxAccount', N'en-US', N'Tax account' UNION ALL
    SELECT N'VatRec123.Col_TaxType', N'vi-VN', N'Loại thuế' UNION ALL
    SELECT N'VatRec123.Col_TaxType', N'en-US', N'Tax type' UNION ALL
    SELECT N'VatRec123.NoPermission', N'vi-VN', N'Bạn không có quyền xem báo cáo thuế.' UNION ALL
    SELECT N'VatRec123.NoPermission', N'en-US', N'You do not have permission to view tax reports.' UNION ALL
    SELECT N'VatRec123.LoadedCount', N'vi-VN', N'Đã tải {0} dòng đối chiếu VAT.' UNION ALL
    SELECT N'VatRec123.LoadedCount', N'en-US', N'Loaded {0} VAT reconciliation line(s).' UNION ALL
    SELECT N'VatRec123.NoTaxAccount', N'vi-VN', N'Dòng này không có tài khoản thuế để mở sổ cái.' UNION ALL
    SELECT N'VatRec123.NoTaxAccount', N'en-US', N'This row has no tax account to open ledger.' UNION ALL

    -- 10.13.1 Executive Dashboard
    SELECT N'Exd131.Title', N'vi-VN', N'10.13.1 Bảng điều khiển quản trị' UNION ALL
    SELECT N'Exd131.Title', N'en-US', N'10.13.1 Executive Dashboard' UNION ALL
    SELECT N'Exd131.Subtitle', N'vi-VN', N'Báo cáo quản trị tổng hợp cho công ty logistics' UNION ALL
    SELECT N'Exd131.Subtitle', N'en-US', N'Management dashboard for logistics company' UNION ALL
    SELECT N'Exd131.LoadDashboard', N'vi-VN', N'Tải dashboard' UNION ALL
    SELECT N'Exd131.LoadDashboard', N'en-US', N'Load dashboard' UNION ALL
    SELECT N'Exd131.Branch', N'vi-VN', N'Chi nhánh' UNION ALL
    SELECT N'Exd131.Branch', N'en-US', N'Branch' UNION ALL
    SELECT N'Exd131.BranchPlaceholder', N'vi-VN', N'SGN/HAN/HPH...' UNION ALL
    SELECT N'Exd131.BranchPlaceholder', N'en-US', N'SGN/HAN/HPH...' UNION ALL
    SELECT N'Exd131.SearchPlaceholder', N'vi-VN', N'Khách hàng, chứng từ, chi nhánh, nguồn...' UNION ALL
    SELECT N'Exd131.SearchPlaceholder', N'en-US', N'Customer, voucher, branch, source...' UNION ALL
    SELECT N'Exd131.Kpi_Revenue', N'vi-VN', N'Doanh thu' UNION ALL
    SELECT N'Exd131.Kpi_Revenue', N'en-US', N'Revenue' UNION ALL
    SELECT N'Exd131.Kpi_GrossProfit', N'vi-VN', N'Lợi nhuận gộp' UNION ALL
    SELECT N'Exd131.Kpi_GrossProfit', N'en-US', N'Gross profit' UNION ALL
    SELECT N'Exd131.Kpi_NetProfit', N'vi-VN', N'Lợi nhuận ròng' UNION ALL
    SELECT N'Exd131.Kpi_NetProfit', N'en-US', N'Net profit' UNION ALL
    SELECT N'Exd131.Kpi_CashBank', N'vi-VN', N'Tiền mặt / Ngân hàng' UNION ALL
    SELECT N'Exd131.Kpi_CashBank', N'en-US', N'Cash / Bank' UNION ALL
    SELECT N'Exd131.Kpi_AR', N'vi-VN', N'Phải thu' UNION ALL
    SELECT N'Exd131.Kpi_AR', N'en-US', N'AR - Receivable' UNION ALL
    SELECT N'Exd131.Kpi_AP', N'vi-VN', N'Phải trả' UNION ALL
    SELECT N'Exd131.Kpi_AP', N'en-US', N'AP - Payable' UNION ALL
    SELECT N'Exd131.Kpi_Shipments', N'vi-VN', N'Lô hàng / Chứng từ' UNION ALL
    SELECT N'Exd131.Kpi_Shipments', N'en-US', N'Shipments / Vouchers' UNION ALL
    SELECT N'Exd131.Kpi_Customers', N'vi-VN', N'Khách hàng hoạt động' UNION ALL
    SELECT N'Exd131.Kpi_Customers', N'en-US', N'Active customers' UNION ALL
    SELECT N'Exd131.NoPermission', N'vi-VN', N'Bạn không có quyền xem Executive Dashboard.' UNION ALL
    SELECT N'Exd131.NoPermission', N'en-US', N'You do not have permission to view Executive Dashboard.' UNION ALL

    -- 10.13.2 Customer Profit
    SELECT N'Cpr132.Title', N'vi-VN', N'10.13.2 Lợi nhuận theo khách hàng' UNION ALL
    SELECT N'Cpr132.Title', N'en-US', N'10.13.2 Customer profit report' UNION ALL
    SELECT N'Cpr132.Subtitle', N'vi-VN', N'Doanh thu, chi phí, lợi nhuận theo khách hàng' UNION ALL
    SELECT N'Cpr132.Subtitle', N'en-US', N'Revenue, cost and profit by customer' UNION ALL
    SELECT N'Cpr132.CustomerFilter', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Cpr132.CustomerFilter', N'en-US', N'Customer' UNION ALL
    SELECT N'Cpr132.CustomerPlaceholder', N'vi-VN', N'Mã / Tên' UNION ALL
    SELECT N'Cpr132.CustomerPlaceholder', N'en-US', N'Code / Name' UNION ALL
    SELECT N'Cpr132.SearchPlaceholder', N'vi-VN', N'Khách hàng, chứng từ, nguồn, tài khoản...' UNION ALL
    SELECT N'Cpr132.SearchPlaceholder', N'en-US', N'Customer, voucher, source, account...' UNION ALL
    SELECT N'Cpr132.Kpi_TotalRevenue', N'vi-VN', N'Tổng doanh thu' UNION ALL
    SELECT N'Cpr132.Kpi_TotalRevenue', N'en-US', N'Total revenue' UNION ALL
    SELECT N'Cpr132.Kpi_TotalCost', N'vi-VN', N'Tổng chi phí' UNION ALL
    SELECT N'Cpr132.Kpi_TotalCost', N'en-US', N'Total cost' UNION ALL
    SELECT N'Cpr132.Kpi_Profit', N'vi-VN', N'Lợi nhuận' UNION ALL
    SELECT N'Cpr132.Kpi_Profit', N'en-US', N'Profit' UNION ALL
    SELECT N'Cpr132.Kpi_Customers', N'vi-VN', N'Khách hàng' UNION ALL
    SELECT N'Cpr132.Kpi_Customers', N'en-US', N'Customers' UNION ALL
    SELECT N'Cpr132.ReportTitle', N'vi-VN', N'BÁO CÁO LỢI NHUẬN THEO KHÁCH HÀNG' UNION ALL
    SELECT N'Cpr132.ReportTitle', N'en-US', N'CUSTOMER PROFIT REPORT' UNION ALL
    SELECT N'Cpr132.Col_CustomerCode', N'vi-VN', N'Mã khách hàng' UNION ALL
    SELECT N'Cpr132.Col_CustomerCode', N'en-US', N'Customer code' UNION ALL
    SELECT N'Cpr132.Col_CustomerName', N'vi-VN', N'Tên khách hàng' UNION ALL
    SELECT N'Cpr132.Col_CustomerName', N'en-US', N'Customer name' UNION ALL
    SELECT N'Cpr132.Col_Revenue', N'vi-VN', N'Doanh thu' UNION ALL
    SELECT N'Cpr132.Col_Revenue', N'en-US', N'Revenue' UNION ALL
    SELECT N'Cpr132.Col_Cost', N'vi-VN', N'Chi phí' UNION ALL
    SELECT N'Cpr132.Col_Cost', N'en-US', N'Cost' UNION ALL
    SELECT N'Cpr132.Col_Profit', N'vi-VN', N'Lợi nhuận' UNION ALL
    SELECT N'Cpr132.Col_Profit', N'en-US', N'Profit' UNION ALL
    SELECT N'Cpr132.Col_GpPercent', N'vi-VN', N'% LN gộp' UNION ALL
    SELECT N'Cpr132.Col_GpPercent', N'en-US', N'GP%' UNION ALL
    SELECT N'Cpr132.Col_Vouchers', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'Cpr132.Col_Vouchers', N'en-US', N'Vouchers' UNION ALL
    SELECT N'Cpr132.Col_Sources', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'Cpr132.Col_Sources', N'en-US', N'Sources' UNION ALL
    SELECT N'Cpr132.NoRecords', N'vi-VN', N'Không có dữ liệu lợi nhuận theo khách hàng.' UNION ALL
    SELECT N'Cpr132.NoRecords', N'en-US', N'No customer profit data.' UNION ALL
    SELECT N'Cpr132.NoPermission', N'vi-VN', N'Bạn không có quyền xem báo cáo lợi nhuận theo khách hàng.' UNION ALL
    SELECT N'Cpr132.NoPermission', N'en-US', N'You do not have permission to view customer profit report.' UNION ALL
    SELECT N'Cpr132.LoadedCount', N'vi-VN', N'Đã tải {0} dòng lợi nhuận khách hàng.' UNION ALL
    SELECT N'Cpr132.LoadedCount', N'en-US', N'Loaded {0} customer profit row(s).' UNION ALL

    -- 10.14.1 Debt Due Notification
    SELECT N'Ddn141.Title', N'vi-VN', N'10.14.1 Thông báo hạn công nợ' UNION ALL
    SELECT N'Ddn141.Title', N'en-US', N'10.14.1 Debt due notification' UNION ALL
    SELECT N'Ddn141.Subtitle', N'vi-VN', N'Theo dõi công nợ đến hạn sau khi ghi sổ' UNION ALL
    SELECT N'Ddn141.Subtitle', N'en-US', N'Track debt due dates after posting' UNION ALL
    SELECT N'Ddn141.FromDueDate', N'vi-VN', N'Từ ngày đến hạn' UNION ALL
    SELECT N'Ddn141.FromDueDate', N'en-US', N'From due date' UNION ALL
    SELECT N'Ddn141.ToDueDate', N'vi-VN', N'Đến ngày đến hạn' UNION ALL
    SELECT N'Ddn141.ToDueDate', N'en-US', N'To due date' UNION ALL
    SELECT N'Ddn141.DebtType', N'vi-VN', N'Loại công nợ' UNION ALL
    SELECT N'Ddn141.DebtType', N'en-US', N'Debt type' UNION ALL
    SELECT N'Ddn141.DebtType_AR', N'vi-VN', N'AR - Phải thu 131' UNION ALL
    SELECT N'Ddn141.DebtType_AR', N'en-US', N'AR - Receivable 131' UNION ALL
    SELECT N'Ddn141.DebtType_AP', N'vi-VN', N'AP - Phải trả 331' UNION ALL
    SELECT N'Ddn141.DebtType_AP', N'en-US', N'AP - Payable 331' UNION ALL
    SELECT N'Ddn141.Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'Ddn141.Status', N'en-US', N'Status' UNION ALL
    SELECT N'Ddn141.Status_Overdue', N'vi-VN', N'Quá hạn' UNION ALL
    SELECT N'Ddn141.Status_Overdue', N'en-US', N'Overdue' UNION ALL
    SELECT N'Ddn141.Status_Today', N'vi-VN', N'Hôm nay' UNION ALL
    SELECT N'Ddn141.Status_Today', N'en-US', N'Today' UNION ALL
    SELECT N'Ddn141.Status_Next7', N'vi-VN', N'7 ngày' UNION ALL
    SELECT N'Ddn141.Status_Next7', N'en-US', N'Next 7 days' UNION ALL
    SELECT N'Ddn141.SearchPlaceholder', N'vi-VN', N'Khách hàng, chứng từ, hóa đơn, HBL...' UNION ALL
    SELECT N'Ddn141.SearchPlaceholder', N'en-US', N'Customer, voucher, invoice, HBL...' UNION ALL
    SELECT N'Ddn141.Kpi_Overdue', N'vi-VN', N'Quá hạn' UNION ALL
    SELECT N'Ddn141.Kpi_Overdue', N'en-US', N'Overdue' UNION ALL
    SELECT N'Ddn141.Kpi_DueToday', N'vi-VN', N'Đến hạn hôm nay' UNION ALL
    SELECT N'Ddn141.Kpi_DueToday', N'en-US', N'Due today' UNION ALL
    SELECT N'Ddn141.Kpi_Next7', N'vi-VN', N'Sắp đến hạn 7 ngày' UNION ALL
    SELECT N'Ddn141.Kpi_Next7', N'en-US', N'Due in 7 days' UNION ALL
    SELECT N'Ddn141.Kpi_TotalDebt', N'vi-VN', N'Tổng công nợ' UNION ALL
    SELECT N'Ddn141.Kpi_TotalDebt', N'en-US', N'Total debt' UNION ALL
    SELECT N'Ddn141.RowCount', N'vi-VN', N'{0} dòng' UNION ALL
    SELECT N'Ddn141.RowCount', N'en-US', N'{0} row(s)' UNION ALL
    SELECT N'Ddn141.ReportTitle', N'vi-VN', N'DANH SÁCH CÔNG NỢ ĐẾN HẠN' UNION ALL
    SELECT N'Ddn141.ReportTitle', N'en-US', N'DEBT DUE LIST' UNION ALL
    SELECT N'Ddn141.Col_Status', N'vi-VN', N'Tình trạng' UNION ALL
    SELECT N'Ddn141.Col_Status', N'en-US', N'Status' UNION ALL
    SELECT N'Ddn141.Col_Days', N'vi-VN', N'Số ngày' UNION ALL
    SELECT N'Ddn141.Col_Days', N'en-US', N'Days' UNION ALL
    SELECT N'Ddn141.Col_DueDate', N'vi-VN', N'Ngày đến hạn' UNION ALL
    SELECT N'Ddn141.Col_DueDate', N'en-US', N'Due date' UNION ALL
    SELECT N'Ddn141.Col_Voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'Ddn141.Col_Voucher', N'en-US', N'Voucher' UNION ALL
    SELECT N'Ddn141.Col_Customer', N'vi-VN', N'Khách hàng / Vendor' UNION ALL
    SELECT N'Ddn141.Col_Customer', N'en-US', N'Customer / Vendor' UNION ALL
    SELECT N'Ddn141.Col_Amount', N'vi-VN', N'Số tiền' UNION ALL
    SELECT N'Ddn141.Col_Amount', N'en-US', N'Amount' UNION ALL
    SELECT N'Ddn141.NoRecords', N'vi-VN', N'Không có công nợ đến hạn theo điều kiện lọc.' UNION ALL
    SELECT N'Ddn141.NoRecords', N'en-US', N'No due debt for current filters.' UNION ALL
    SELECT N'Ddn141.NoPermission', N'vi-VN', N'Bạn không có quyền xem thông báo hạn công nợ.' UNION ALL
    SELECT N'Ddn141.NoPermission', N'en-US', N'You do not have permission to view debt due notification.' UNION ALL
    SELECT N'Ddn141.LoadedCount', N'vi-VN', N'Đã tải {0} dòng công nợ đến hạn.' UNION ALL
    SELECT N'Ddn141.LoadedCount', N'en-US', N'Loaded {0} outstanding debt row(s).' UNION ALL
    SELECT N'Ddn141.ReminderLogged', N'vi-VN', N'Đã ghi nhận nhắc công nợ: {0} - {1:N0} - hạn {2:dd/MM/yyyy}.' UNION ALL
    SELECT N'Ddn141.ReminderLogged', N'en-US', N'Reminder logged: {0} - {1:N0} - due {2:dd/MM/yyyy}.' UNION ALL

    -- 10.14.2 Statement of Account
    SELECT N'Soa142.Title', N'vi-VN', N'10.14.2 SOA - Đối chiếu công nợ' UNION ALL
    SELECT N'Soa142.Title', N'en-US', N'10.14.2 SOA - Statement of Account' UNION ALL
    SELECT N'Soa142.Subtitle', N'vi-VN', N'Đối chiếu công nợ khách hàng / vendor theo sổ cái đã ghi sổ' UNION ALL
    SELECT N'Soa142.Subtitle', N'en-US', N'Customer / vendor reconciliation from posted ledger' UNION ALL
    SELECT N'Soa142.SoaType', N'vi-VN', N'Loại SOA' UNION ALL
    SELECT N'Soa142.SoaType', N'en-US', N'SOA type' UNION ALL
    SELECT N'Soa142.SoaType_AR', N'vi-VN', N'AR - Khách hàng 131' UNION ALL
    SELECT N'Soa142.SoaType_AR', N'en-US', N'AR - Customer 131' UNION ALL
    SELECT N'Soa142.SoaType_AP', N'vi-VN', N'AP - Vendor 331' UNION ALL
    SELECT N'Soa142.SoaType_AP', N'en-US', N'AP - Vendor 331' UNION ALL
    SELECT N'Soa142.SoaType_All', N'vi-VN', N'ALL - Cả 131 + 331' UNION ALL
    SELECT N'Soa142.SoaType_All', N'en-US', N'ALL - Both 131 + 331' UNION ALL
    SELECT N'Soa142.CustomerVendor', N'vi-VN', N'Khách hàng / Vendor' UNION ALL
    SELECT N'Soa142.CustomerVendor', N'en-US', N'Customer / Vendor' UNION ALL
    SELECT N'Soa142.SearchPlaceholder', N'vi-VN', N'Chứng từ, diễn giải, nguồn...' UNION ALL
    SELECT N'Soa142.SearchPlaceholder', N'en-US', N'Voucher, description, source...' UNION ALL
    SELECT N'Soa142.Kpi_OpeningBalance', N'vi-VN', N'Số dư đầu kỳ' UNION ALL
    SELECT N'Soa142.Kpi_OpeningBalance', N'en-US', N'Opening balance' UNION ALL
    SELECT N'Soa142.Kpi_DebitMovement', N'vi-VN', N'Phát sinh Nợ' UNION ALL
    SELECT N'Soa142.Kpi_DebitMovement', N'en-US', N'Debit movement' UNION ALL
    SELECT N'Soa142.Kpi_CreditMovement', N'vi-VN', N'Phát sinh Có' UNION ALL
    SELECT N'Soa142.Kpi_CreditMovement', N'en-US', N'Credit movement' UNION ALL
    SELECT N'Soa142.Kpi_ClosingBalance', N'vi-VN', N'Số dư cuối kỳ' UNION ALL
    SELECT N'Soa142.Kpi_ClosingBalance', N'en-US', N'Closing balance' UNION ALL
    SELECT N'Soa142.NoPermission', N'vi-VN', N'Bạn không có quyền xem SOA.' UNION ALL
    SELECT N'Soa142.NoPermission', N'en-US', N'You do not have permission to view SOA.' UNION ALL
    SELECT N'Soa142.LoadedCount', N'vi-VN', N'Đã tải {0} dòng SOA.' UNION ALL
    SELECT N'Soa142.LoadedCount', N'en-US', N'Loaded {0} SOA line(s).' UNION ALL
    SELECT N'Soa142.OpenLedgerHint', N'vi-VN', N'Đang mở sổ cái TK 131. Với dòng 331, bấm nút Sổ cái trên từng dòng.' UNION ALL
    SELECT N'Soa142.OpenLedgerHint', N'en-US', N'Opening ledger for account 131. For 331 rows, use ledger button on each row.' UNION ALL
    SELECT N'Soa142.NoPrintData', N'vi-VN', N'Không có dữ liệu để in SOA.' UNION ALL
    SELECT N'Soa142.NoPrintData', N'en-US', N'No data to print SOA.' UNION ALL

    -- 10.14.3 Debt Aging
    SELECT N'Dag143.Title', N'vi-VN', N'10.14.3 Aging công nợ' UNION ALL
    SELECT N'Dag143.Title', N'en-US', N'10.14.3 Debt aging' UNION ALL
    SELECT N'Dag143.Subtitle', N'vi-VN', N'Báo cáo tuổi nợ công nợ phải thu / phải trả' UNION ALL
    SELECT N'Dag143.Subtitle', N'en-US', N'Accounts receivable / payable aging report' UNION ALL
    SELECT N'Dag143.AsOfDate', N'vi-VN', N'Ngày chốt công nợ' UNION ALL
    SELECT N'Dag143.AsOfDate', N'en-US', N'As-of date' UNION ALL
    SELECT N'Dag143.Kpi_TotalBalance', N'vi-VN', N'Tổng còn nợ' UNION ALL
    SELECT N'Dag143.Kpi_TotalBalance', N'en-US', N'Total balance' UNION ALL
    SELECT N'Dag143.Kpi_NoDue', N'vi-VN', N'Chưa có hạn' UNION ALL
    SELECT N'Dag143.Kpi_NoDue', N'en-US', N'No due date' UNION ALL
    SELECT N'Dag143.Kpi_0_30', N'vi-VN', N'0 - 30 ngày' UNION ALL
    SELECT N'Dag143.Kpi_0_30', N'en-US', N'0 - 30 days' UNION ALL
    SELECT N'Dag143.Kpi_31_60', N'vi-VN', N'31 - 60 ngày' UNION ALL
    SELECT N'Dag143.Kpi_31_60', N'en-US', N'31 - 60 days' UNION ALL
    SELECT N'Dag143.Kpi_61_90', N'vi-VN', N'61 - 90 ngày' UNION ALL
    SELECT N'Dag143.Kpi_61_90', N'en-US', N'61 - 90 days' UNION ALL
    SELECT N'Dag143.Kpi_90Plus', N'vi-VN', N'Trên 90 ngày' UNION ALL
    SELECT N'Dag143.Kpi_90Plus', N'en-US', N'Over 90 days' UNION ALL
    SELECT N'Dag143.ReportTitle', N'vi-VN', N'AGING REPORT - CÔNG NỢ THEO TUỔI NỢ' UNION ALL
    SELECT N'Dag143.ReportTitle', N'en-US', N'AGING REPORT - DEBT BY AGE' UNION ALL
    SELECT N'Dag143.DetailTitle', N'vi-VN', N'CHI TIẾT TUỔI NỢ' UNION ALL
    SELECT N'Dag143.DetailTitle', N'en-US', N'AGING DETAIL' UNION ALL
    SELECT N'Dag143.Col_Bucket', N'vi-VN', N'Nhóm tuổi nợ' UNION ALL
    SELECT N'Dag143.Col_Bucket', N'en-US', N'Aging bucket' UNION ALL
    SELECT N'Dag143.Col_OriginalVoucher', N'vi-VN', N'Chứng từ gốc' UNION ALL
    SELECT N'Dag143.Col_OriginalVoucher', N'en-US', N'Source voucher' UNION ALL
    SELECT N'Dag143.Col_PaidAmount', N'vi-VN', N'Đã thanh toán' UNION ALL
    SELECT N'Dag143.Col_PaidAmount', N'en-US', N'Paid amount' UNION ALL
    SELECT N'Dag143.Col_Balance', N'vi-VN', N'Còn lại' UNION ALL
    SELECT N'Dag143.Col_Balance', N'en-US', N'Balance' UNION ALL
    SELECT N'Dag143.NoRecords', N'vi-VN', N'Không có dữ liệu aging theo điều kiện lọc.' UNION ALL
    SELECT N'Dag143.NoRecords', N'en-US', N'No aging data for current filters.' UNION ALL
    SELECT N'Dag143.NoPermission', N'vi-VN', N'Bạn không có quyền xem Aging công nợ.' UNION ALL
    SELECT N'Dag143.NoPermission', N'en-US', N'You do not have permission to view debt aging.' UNION ALL
    SELECT N'Dag143.LoadedCount', N'vi-VN', N'Đã tải {0} dòng aging.' UNION ALL
    SELECT N'Dag143.LoadedCount', N'en-US', N'Loaded {0} aging row(s).' UNION ALL
    SELECT N'Dag143.SelectCustomerFirst', N'vi-VN', N'Chọn Customer/Vendor trước khi mở sổ cái tổng hợp.' UNION ALL
    SELECT N'Dag143.SelectCustomerFirst', N'en-US', N'Select customer/vendor before opening general ledger.' UNION ALL

    -- 10.15 Account Flow Tree
    SELECT N'Aft151.Title', N'vi-VN', N'10.15 Cây T chu trình logistics' UNION ALL
    SELECT N'Aft151.Title', N'en-US', N'10.15 Logistics flow T-tree' UNION ALL
    SELECT N'Aft151.Subtitle', N'vi-VN', N'Theo dõi nối tiếp một job logistics: RFQ → Shipment/HBL → Debit/Credit → Voucher → Sổ cái → SOA/Aging/Profit' UNION ALL
    SELECT N'Aft151.Subtitle', N'en-US', N'Track a logistics job flow: RFQ → Shipment/HBL → Debit/Credit → Voucher → Ledger → SOA/Aging/Profit' UNION ALL
    SELECT N'Aft151.OpenBalance', N'vi-VN', N'10.9.1 Số dư' UNION ALL
    SELECT N'Aft151.OpenBalance', N'en-US', N'10.9.1 Balance' UNION ALL
    SELECT N'Aft151.ByDateRange', N'vi-VN', N'Từ ngày - đến ngày' UNION ALL
    SELECT N'Aft151.ByDateRange', N'en-US', N'Date range' UNION ALL
    SELECT N'Aft151.VoucherSearch', N'vi-VN', N'Chứng từ / Tìm kiếm' UNION ALL
    SELECT N'Aft151.VoucherSearch', N'en-US', N'Voucher / Search' UNION ALL
    SELECT N'Aft151.VoucherSearchPlaceholder', N'vi-VN', N'Số CT, mô tả, module...' UNION ALL
    SELECT N'Aft151.VoucherSearchPlaceholder', N'en-US', N'Voucher no, description, module...' UNION ALL
    SELECT N'Aft151.NoPermission', N'vi-VN', N'Bạn không có quyền xem sổ cái.' UNION ALL
    SELECT N'Aft151.NoPermission', N'en-US', N'You do not have permission to view the ledger.' UNION ALL
    SELECT N'Aft151.LoadedCount', N'vi-VN', N'Đã tải {0} dòng sổ cái.' UNION ALL
    SELECT N'Aft151.LoadedCount', N'en-US', N'Loaded {0} ledger row(s).' UNION ALL
    SELECT N'Aft151.SimulatorLoaded', N'vi-VN', N'Đã load simulator cây T nghiệp vụ logistics.' UNION ALL
    SELECT N'Aft151.SimulatorLoaded', N'en-US', N'Loaded logistics T-tree simulator.' UNION ALL
    SELECT N'Aft151.NoBalanceTab', N'vi-VN', N'Không mở được tab 10.9.1 vì chưa truyền OnTabRequest.' UNION ALL
    SELECT N'Aft151.NoBalanceTab', N'en-US', N'Cannot open 10.9.1 tab: OnTabRequest was not provided.' UNION ALL

    -- 10.16 Cash Flow T
    SELECT N'Cft161.Title', N'vi-VN', N'10.16 Theo dõi dòng tiền theo T' UNION ALL
    SELECT N'Cft161.Title', N'en-US', N'10.16 Cash flow T-tracker' UNION ALL
    SELECT N'Cft161.Subtitle', N'vi-VN', N'Theo dõi tiền vào / tiền ra từ voucher → sổ cái → tài khoản tiền 111/112 → số dư cuối kỳ' UNION ALL
    SELECT N'Cft161.Subtitle', N'en-US', N'Track cash in/out from voucher → ledger → cash accounts 111/112 → closing balance' UNION ALL
    SELECT N'Cft161.OpenLedger', N'vi-VN', N'10.6.1 Sổ cái' UNION ALL
    SELECT N'Cft161.OpenLedger', N'en-US', N'10.6.1 Ledger' UNION ALL
    SELECT N'Cft161.OpenBalance', N'vi-VN', N'10.9.1 Số dư' UNION ALL
    SELECT N'Cft161.OpenBalance', N'en-US', N'10.9.1 Balance' UNION ALL
    SELECT N'Cft161.CashAccount', N'vi-VN', N'TK tiền' UNION ALL
    SELECT N'Cft161.CashAccount', N'en-US', N'Cash account' UNION ALL
    SELECT N'Cft161.CashAccountPlaceholder', N'vi-VN', N'111, 112...' UNION ALL
    SELECT N'Cft161.CashAccountPlaceholder', N'en-US', N'111, 112...' UNION ALL
    SELECT N'Cft161.FlowDirection', N'vi-VN', N'Dòng tiền' UNION ALL
    SELECT N'Cft161.FlowDirection', N'en-US', N'Cash flow' UNION ALL
    SELECT N'Cft161.BusinessCategory', N'vi-VN', N'Nhóm nghiệp vụ' UNION ALL
    SELECT N'Cft161.BusinessCategory', N'en-US', N'Business category' UNION ALL
    SELECT N'Cft161.Kpi_OpeningCash', N'vi-VN', N'Số dư đầu kỳ' UNION ALL
    SELECT N'Cft161.Kpi_OpeningCash', N'en-US', N'Opening cash balance' UNION ALL
    SELECT N'Cft161.Kpi_CashIn', N'vi-VN', N'Tiền vào' UNION ALL
    SELECT N'Cft161.Kpi_CashIn', N'en-US', N'Cash in' UNION ALL
    SELECT N'Cft161.Kpi_CashOut', N'vi-VN', N'Tiền ra' UNION ALL
    SELECT N'Cft161.Kpi_CashOut', N'en-US', N'Cash out' UNION ALL
    SELECT N'Cft161.Kpi_ClosingCash', N'vi-VN', N'Số dư cuối kỳ' UNION ALL
    SELECT N'Cft161.Kpi_ClosingCash', N'en-US', N'Closing cash balance' UNION ALL
    SELECT N'Cft161.NoPermission', N'vi-VN', N'Bạn không có quyền xem sổ cái.' UNION ALL
    SELECT N'Cft161.NoPermission', N'en-US', N'You do not have permission to view the ledger.' UNION ALL
    SELECT N'Cft161.LoadedCount', N'vi-VN', N'Đã tải {0} dòng tiền theo T.' UNION ALL
    SELECT N'Cft161.LoadedCount', N'en-US', N'Loaded {0} T-track cash row(s).' UNION ALL
    SELECT N'Cft161.SimulatorLoaded', N'vi-VN', N'Đã load simulator dòng tiền theo chữ T.' UNION ALL
    SELECT N'Cft161.SimulatorLoaded', N'en-US', N'Loaded T-account cash flow simulator.'
)
MERGE dbo.LocalizationResources AS tgt
USING src ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN INSERT (ResourceKey, Culture, Value) VALUES (src.ResourceKey, src.Culture, src.Value);

COMMIT TRANSACTION;
