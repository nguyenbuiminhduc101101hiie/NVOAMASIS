-- Menu 10: Manage company cash flow / Accounting (10.1–10.16)
-- Depends on: Group C, Groups 2–9
-- Reuses: NoPermission_*, PleaseSelectData, DataAlreadyApproved, SomethingWentWrong,
--   Error_Message, SelectOneRow (Group 2)
-- Also run: ImportAccountingBatch2_LocalizationResources.sql, ImportGeneralLedgerEntriesLocalizationResources.sql
BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey IN (
    'Manage_company_cash_flow_nav',
    'Phieuthu','Phieuchi','BaoCaoQuyTienMat','BcQuy_Subtitle',
    'Accounting','PhieuKeToan','Import_Voucher_Excel','Modify_Voucher_Lines',
    'Fixed_Assets_TT99','Fixed_Asset_Depreciation',
    'TransactionTypes_Details',
    'General_Ledger','General_Ledger_Entries','General_Ledger_Summary',
    'Account_Ledger','T_Account_Ledger','Import_Excel_General_Ledger',
    'Financial_Report_Mapping','Accounting_Period',
    'Account_Balance','Account_Balance_Snapshot','Import_Excel_Debt',
    'Financial_Report_Lines','Financial_Reports_TT99',
    'Tax_Reports','VAT_Input_1331','VAT_Output_33311','VAT_Reconciliation',
    'Management_Reports','Executive_Dashboard','Customer_Profit_Report',
    'Debt_AR_AP','Debt_Due_Notification','SOA_Debt','Debt_Aging',
    'Account_Flow_Tree','Cash_Flow_T',
    'Pt_Subtitle','Pt_SelectBeforeVoucher','Pt_NotApprovedForVoucher','Pt_MissingDebitCreditAccount',
    'Pt_AmountZero','Pt_NoPermissionCreateVoucher','Pt_CompanyIdNotFound','Pt_VoucherCreatedSuccess',
    'Pt_SqlError','Pt_SelectForApproveRequest','Pt_ApproveRequestFailed','Pt_SelectForUnapprove','Pt_UnapproveFailed',
    'Pc_Subtitle','Pc_SelectBeforeVoucher','Pc_NotApprovedForVoucher','Pc_MissingDebitCreditAccount',
    'Pc_AmountZero','Pc_NoPermissionCreateVoucher','Pc_VoucherCreatedSuccess',
    'Pc_SqlError','Pc_SelectForApproveRequest','Pc_ApproveRequestFailed','Pc_SelectForUnapprove','Pc_UnapproveFailed'
);

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
-- 10.0 Group title
('Manage_company_cash_flow_nav','en-US',N'Manage company cash flow'),('Manage_company_cash_flow_nav','vi-VN',N'Quản lý quỹ tiền công ty'),('Manage_company_cash_flow_nav','zh-CN',N'公司现金流管理'),
-- 10.1–10.3
('Phieuthu','en-US',N'Receipt voucher'),('Phieuthu','vi-VN',N'Phiếu thu'),('Phieuthu','zh-CN',N'收款单'),
('Phieuchi','en-US',N'Payment voucher'),('Phieuchi','vi-VN',N'Phiếu chi'),('Phieuchi','zh-CN',N'付款单'),
('BaoCaoQuyTienMat','en-US',N'Cash book report'),('BaoCaoQuyTienMat','vi-VN',N'Báo cáo quỹ tiền mặt'),('BaoCaoQuyTienMat','zh-CN',N'现金账簿报表'),
('BcQuy_Subtitle','en-US',N'Cash book · receipts and payments'),('BcQuy_Subtitle','vi-VN',N'Sổ quỹ · thu chi'),('BcQuy_Subtitle','zh-CN',N'现金账簿 · 收支'),
-- 10.4 Accounting
('Accounting','en-US',N'Accounting'),('Accounting','vi-VN',N'Kế toán'),('Accounting','zh-CN',N'会计'),
('PhieuKeToan','en-US',N'Accounting vouchers'),('PhieuKeToan','vi-VN',N'Phiếu kế toán'),('PhieuKeToan','zh-CN',N'会计凭证'),
('Import_Voucher_Excel','en-US',N'Import voucher Excel'),('Import_Voucher_Excel','vi-VN',N'Import phiếu Excel'),('Import_Voucher_Excel','zh-CN',N'导入凭证 Excel'),
('Modify_Voucher_Lines','en-US',N'Modify voucher lines'),('Modify_Voucher_Lines','vi-VN',N'Sửa dòng phiếu'),('Modify_Voucher_Lines','zh-CN',N'修改凭证行'),
('Fixed_Assets_TT99','en-US',N'Fixed assets (TT99)'),('Fixed_Assets_TT99','vi-VN',N'Tài sản cố định (TT99)'),('Fixed_Assets_TT99','zh-CN',N'固定资产 (TT99)'),
('Fixed_Asset_Depreciation','en-US',N'Fixed asset depreciation'),('Fixed_Asset_Depreciation','vi-VN',N'Khấu hao TSCĐ'),('Fixed_Asset_Depreciation','zh-CN',N'固定资产折旧'),
-- 10.5
('TransactionTypes_Details','en-US',N'Transaction types'),('TransactionTypes_Details','vi-VN',N'Loại giao dịch'),('TransactionTypes_Details','zh-CN',N'交易类型'),
-- 10.6 General ledger
('General_Ledger','en-US',N'General ledger'),('General_Ledger','vi-VN',N'Sổ cái'),('General_Ledger','zh-CN',N'总账'),
('General_Ledger_Entries','en-US',N'General ledger entries'),('General_Ledger_Entries','vi-VN',N'Bút toán sổ cái'),('General_Ledger_Entries','zh-CN',N'总账分录'),
('General_Ledger_Summary','en-US',N'General ledger summary'),('General_Ledger_Summary','vi-VN',N'Tổng hợp sổ cái'),('General_Ledger_Summary','zh-CN',N'总账汇总'),
('Account_Ledger','en-US',N'Account ledger'),('Account_Ledger','vi-VN',N'Sổ chi tiết tài khoản'),('Account_Ledger','zh-CN',N'明细账'),
('T_Account_Ledger','en-US',N'T-account ledger'),('T_Account_Ledger','vi-VN',N'Sổ T'),('T_Account_Ledger','zh-CN',N'T 型账'),
('Import_Excel_General_Ledger','en-US',N'Import general ledger Excel'),('Import_Excel_General_Ledger','vi-VN',N'Import sổ cái Excel'),('Import_Excel_General_Ledger','zh-CN',N'导入总账 Excel'),
-- 10.7–10.11
('Financial_Report_Mapping','en-US',N'Financial report mapping'),('Financial_Report_Mapping','vi-VN',N'Ánh xạ báo cáo tài chính'),('Financial_Report_Mapping','zh-CN',N'财务报表映射'),
('Accounting_Period','en-US',N'Accounting period'),('Accounting_Period','vi-VN',N'Kỳ kế toán'),('Accounting_Period','zh-CN',N'会计期间'),
('Account_Balance','en-US',N'Account balance'),('Account_Balance','vi-VN',N'Số dư tài khoản'),('Account_Balance','zh-CN',N'科目余额'),
('Account_Balance_Snapshot','en-US',N'Account balance snapshot'),('Account_Balance_Snapshot','vi-VN',N'Ảnh chụp số dư'),('Account_Balance_Snapshot','zh-CN',N'余额快照'),
('Import_Excel_Debt','en-US',N'Import debt Excel'),('Import_Excel_Debt','vi-VN',N'Import công nợ Excel'),('Import_Excel_Debt','zh-CN',N'导入欠款 Excel'),
('Financial_Report_Lines','en-US',N'Financial report lines'),('Financial_Report_Lines','vi-VN',N'Dòng báo cáo tài chính'),('Financial_Report_Lines','zh-CN',N'财务报表行'),
('Financial_Reports_TT99','en-US',N'Financial reports (TT99)'),('Financial_Reports_TT99','vi-VN',N'Báo cáo tài chính (TT99)'),('Financial_Reports_TT99','zh-CN',N'财务报表 (TT99)'),
-- 10.12 Tax
('Tax_Reports','en-US',N'Tax reports'),('Tax_Reports','vi-VN',N'Báo cáo thuế'),('Tax_Reports','zh-CN',N'税务报表'),
('VAT_Input_1331','en-US',N'VAT input (1331)'),('VAT_Input_1331','vi-VN',N'VAT đầu vào (1331)'),('VAT_Input_1331','zh-CN',N'进项 VAT (1331)'),
('VAT_Output_33311','en-US',N'VAT output (33311)'),('VAT_Output_33311','vi-VN',N'VAT đầu ra (33311)'),('VAT_Output_33311','zh-CN',N'销项 VAT (33311)'),
('VAT_Reconciliation','en-US',N'VAT reconciliation'),('VAT_Reconciliation','vi-VN',N'Đối chiếu VAT'),('VAT_Reconciliation','zh-CN',N'VAT 对账'),
-- 10.13 Management
('Management_Reports','en-US',N'Management reports'),('Management_Reports','vi-VN',N'Báo cáo quản trị'),('Management_Reports','zh-CN',N'管理报表'),
('Executive_Dashboard','en-US',N'Executive dashboard'),('Executive_Dashboard','vi-VN',N'Dashboard điều hành'),('Executive_Dashboard','zh-CN',N'高管仪表盘'),
('Customer_Profit_Report','en-US',N'Customer profit report'),('Customer_Profit_Report','vi-VN',N'Báo cáo lợi nhuận khách hàng'),('Customer_Profit_Report','zh-CN',N'客户利润报表'),
-- 10.14 Debt
('Debt_AR_AP','en-US',N'Debt (AR/AP)'),('Debt_AR_AP','vi-VN',N'Công nợ (AR/AP)'),('Debt_AR_AP','zh-CN',N'往来账款 (AR/AP)'),
('Debt_Due_Notification','en-US',N'Debt due notification'),('Debt_Due_Notification','vi-VN',N'Thông báo công nợ đến hạn'),('Debt_Due_Notification','zh-CN',N'到期欠款通知'),
('SOA_Debt','en-US',N'Statement of account (debt)'),('SOA_Debt','vi-VN',N'SAO công nợ'),('SOA_Debt','zh-CN',N'对账单（欠款）'),
('Debt_Aging','en-US',N'Debt aging'),('Debt_Aging','vi-VN',N'Phân tích tuổi nợ'),('Debt_Aging','zh-CN',N'账龄分析'),
-- 10.15–10.16
('Account_Flow_Tree','en-US',N'Account flow tree'),('Account_Flow_Tree','vi-VN',N'Cây luồng tài khoản'),('Account_Flow_Tree','zh-CN',N'科目流向树'),
('Cash_Flow_T','en-US',N'Cash flow (T-account)'),('Cash_Flow_T','vi-VN',N'Lưu chuyển tiền (sổ T)'),('Cash_Flow_T','zh-CN',N'现金流量 (T 型账)'),
-- 10.1 PhieuThu page
('Pt_Subtitle','en-US',N'Receipt vouchers · create voucher · print'),('Pt_Subtitle','vi-VN',N'Quản lý phiếu thu · tạo voucher · xuất phiếu'),('Pt_Subtitle','zh-CN',N'收款单 · 生成凭证 · 打印'),
('Pt_SelectBeforeVoucher','en-US',N'Please select one receipt voucher before creating an accounting voucher.'),('Pt_SelectBeforeVoucher','vi-VN',N'Vui lòng chọn 1 phiếu thu trước khi tạo voucher.'),('Pt_SelectBeforeVoucher','zh-CN',N'请先选择一张收款单再生成会计凭证。'),
('Pt_NotApprovedForVoucher','en-US',N'Receipt voucher is not approved; cannot create accounting voucher.'),('Pt_NotApprovedForVoucher','vi-VN',N'Phiếu thu chưa Approve, không thể tạo voucher.'),('Pt_NotApprovedForVoucher','zh-CN',N'收款单未审批，无法生成凭证。'),
('Pt_MissingDebitCreditAccount','en-US',N'Receipt voucher is missing debit or credit account.'),('Pt_MissingDebitCreditAccount','vi-VN',N'Phiếu thu thiếu TK Nợ hoặc TK Có.'),('Pt_MissingDebitCreditAccount','zh-CN',N'收款单缺少借方或贷方科目。'),
('Pt_AmountZero','en-US',N'Receipt voucher amount is zero.'),('Pt_AmountZero','vi-VN',N'Số tiền phiếu thu bằng 0.'),('Pt_AmountZero','zh-CN',N'收款单金额为零。'),
('Pt_NoPermissionCreateVoucher','en-US',N'You do not have permission to create accounting vouchers.'),('Pt_NoPermissionCreateVoucher','vi-VN',N'Bạn không có quyền tạo voucher kế toán.'),('Pt_NoPermissionCreateVoucher','zh-CN',N'您没有创建会计凭证的权限。'),
('Pt_CompanyIdNotFound','en-US',N'CompanyId not found in CompanyInfomation.'),('Pt_CompanyIdNotFound','vi-VN',N'Không tìm thấy CompanyId trong CompanyInfomation.'),('Pt_CompanyIdNotFound','zh-CN',N'在 CompanyInfomation 中未找到 CompanyId。'),
('Pt_VoucherCreatedSuccess','en-US',N'Accounting voucher created from receipt voucher successfully.'),('Pt_VoucherCreatedSuccess','vi-VN',N'Tạo voucher từ phiếu thu thành công.'),('Pt_VoucherCreatedSuccess','zh-CN',N'已从收款单成功生成会计凭证。'),
('Pt_SqlError','en-US',N'SQL error: {0}'),('Pt_SqlError','vi-VN',N'Lỗi SQL: {0}'),('Pt_SqlError','zh-CN',N'SQL 错误：{0}'),
('Pt_SelectForApproveRequest','en-US',N'Please select receipt voucher(s) to send for approval.'),('Pt_SelectForApproveRequest','vi-VN',N'Vui lòng chọn phiếu thu cần gửi yêu cầu duyệt.'),('Pt_SelectForApproveRequest','zh-CN',N'请选择要提交审批的收款单。'),
('Pt_ApproveRequestFailed','en-US',N'Failed to send approval request: {0}'),('Pt_ApproveRequestFailed','vi-VN',N'Lỗi gửi yêu cầu duyệt: {0}'),('Pt_ApproveRequestFailed','zh-CN',N'发送审批请求失败：{0}'),
('Pt_SelectForUnapprove','en-US',N'Please select receipt voucher(s) to unapprove.'),('Pt_SelectForUnapprove','vi-VN',N'Vui lòng chọn phiếu thu cần bỏ Approve.'),('Pt_SelectForUnapprove','zh-CN',N'请选择要取消审批的收款单。'),
('Pt_UnapproveFailed','en-US',N'Failed to unapprove: {0}'),('Pt_UnapproveFailed','vi-VN',N'Lỗi bỏ Approve: {0}'),('Pt_UnapproveFailed','zh-CN',N'取消审批失败：{0}'),
-- 10.2 PhieuChi page
('Pc_Subtitle','en-US',N'Payment vouchers · payment management · create accounting voucher'),('Pc_Subtitle','vi-VN',N'Phiếu chi · quản lý thanh toán · tạo voucher kế toán'),('Pc_Subtitle','zh-CN',N'付款单 · 付款管理 · 生成会计凭证'),
('Pc_SelectBeforeVoucher','en-US',N'Please select one payment voucher before creating an accounting voucher.'),('Pc_SelectBeforeVoucher','vi-VN',N'Vui lòng chọn 1 phiếu chi trước khi tạo voucher.'),('Pc_SelectBeforeVoucher','zh-CN',N'请先选择一张付款单再生成会计凭证。'),
('Pc_NotApprovedForVoucher','en-US',N'Payment voucher is not approved; cannot create accounting voucher.'),('Pc_NotApprovedForVoucher','vi-VN',N'Phiếu chi chưa Approve, không thể tạo voucher.'),('Pc_NotApprovedForVoucher','zh-CN',N'付款单未审批，无法生成凭证。'),
('Pc_MissingDebitCreditAccount','en-US',N'Payment voucher is missing debit or credit account.'),('Pc_MissingDebitCreditAccount','vi-VN',N'Phiếu chi thiếu TK Nợ hoặc TK Có.'),('Pc_MissingDebitCreditAccount','zh-CN',N'付款单缺少借方或贷方科目。'),
('Pc_AmountZero','en-US',N'Payment voucher amount is zero.'),('Pc_AmountZero','vi-VN',N'Số tiền phiếu chi bằng 0.'),('Pc_AmountZero','zh-CN',N'付款单金额为零。'),
('Pc_NoPermissionCreateVoucher','en-US',N'You do not have permission to create accounting vouchers.'),('Pc_NoPermissionCreateVoucher','vi-VN',N'Bạn không có quyền tạo voucher kế toán.'),('Pc_NoPermissionCreateVoucher','zh-CN',N'您没有创建会计凭证的权限。'),
('Pc_VoucherCreatedSuccess','en-US',N'Accounting voucher created from payment voucher successfully.'),('Pc_VoucherCreatedSuccess','vi-VN',N'Tạo voucher từ phiếu chi thành công.'),('Pc_VoucherCreatedSuccess','zh-CN',N'已从付款单成功生成会计凭证。'),
('Pc_SqlError','en-US',N'SQL error: {0}'),('Pc_SqlError','vi-VN',N'Lỗi SQL: {0}'),('Pc_SqlError','zh-CN',N'SQL 错误：{0}'),
('Pc_SelectForApproveRequest','en-US',N'Please select payment voucher(s) to send for approval.'),('Pc_SelectForApproveRequest','vi-VN',N'Vui lòng chọn phiếu chi cần gửi yêu cầu duyệt.'),('Pc_SelectForApproveRequest','zh-CN',N'请选择要提交审批的付款单。'),
('Pc_ApproveRequestFailed','en-US',N'Failed to send approval request: {0}'),('Pc_ApproveRequestFailed','vi-VN',N'Lỗi gửi yêu cầu duyệt: {0}'),('Pc_ApproveRequestFailed','zh-CN',N'发送审批请求失败：{0}'),
('Pc_SelectForUnapprove','en-US',N'Please select payment voucher(s) to unapprove.'),('Pc_SelectForUnapprove','vi-VN',N'Vui lòng chọn phiếu chi cần bỏ Approve.'),('Pc_SelectForUnapprove','zh-CN',N'请选择要取消审批的付款单。'),
('Pc_UnapproveFailed','en-US',N'Failed to unapprove: {0}'),('Pc_UnapproveFailed','vi-VN',N'Lỗi bỏ Approve: {0}'),('Pc_UnapproveFailed','zh-CN',N'取消审批失败：{0}');

COMMIT TRANSACTION;
-- After run: DbStringLocalizerFactory.ClearCache() or restart app.
