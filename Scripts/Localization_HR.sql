/*
  Module 12 - Quản lý nhân sự (giai đoạn 1 + 2 + 3): chuỗi giao diện (en-US / vi-VN / zh-CN).
  - Khóa hr_*: MERGE (cập nhật nếu đã có), chia nhiều phần để câu lệnh không quá lớn.
  - Khóa dùng chung (new, edit, save, NoPermission_*...): CHỈ thêm nếu chưa có,
    không ghi đè bản dịch hiện tại.
  - Chạy trên từng tenant DB + DB template. Safe to re-run (bản này thay cho các bản trước).
  - DbStringLocalizer cache theo culture trong bộ nhớ → cần recycle AppPool / restart app
    sau khi chạy script để giao diện nhận chuỗi mới.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- Phần 1/5
;WITH src AS (
    SELECT N'hr_account' AS ResourceKey, N'en-US' AS Culture, N'Login account' AS Value UNION ALL
    SELECT N'hr_account', N'vi-VN', N'Tài khoản đăng nhập' UNION ALL
    SELECT N'hr_account', N'zh-CN', N'登录账号' UNION ALL
    SELECT N'hr_account_helper' AS ResourceKey, N'en-US' AS Culture, N'Only accounts not linked to another record are shown' AS Value UNION ALL
    SELECT N'hr_account_helper', N'vi-VN', N'Chỉ hiện tài khoản chưa gắn với hồ sơ khác' UNION ALL
    SELECT N'hr_account_helper', N'zh-CN', N'仅显示未关联其他档案的账号' UNION ALL
    SELECT N'hr_active' AS ResourceKey, N'en-US' AS Culture, N'Active' AS Value UNION ALL
    SELECT N'hr_active', N'vi-VN', N'Đang dùng' UNION ALL
    SELECT N'hr_active', N'zh-CN', N'启用' UNION ALL
    SELECT N'hr_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Afternoon' AS Value UNION ALL
    SELECT N'hr_afternoon', N'vi-VN', N'Buổi chiều' UNION ALL
    SELECT N'hr_afternoon', N'zh-CN', N'下午' UNION ALL
    SELECT N'hr_age' AS ResourceKey, N'en-US' AS Culture, N'Age' AS Value UNION ALL
    SELECT N'hr_age', N'vi-VN', N'Tuổi' UNION ALL
    SELECT N'hr_age', N'zh-CN', N'年龄' UNION ALL
    SELECT N'hr_alert_days' AS ResourceKey, N'en-US' AS Culture, N'Alert window' AS Value UNION ALL
    SELECT N'hr_alert_days', N'vi-VN', N'Cảnh báo trước' UNION ALL
    SELECT N'hr_alert_days', N'zh-CN', N'提前提醒' UNION ALL
    SELECT N'hr_allowance' AS ResourceKey, N'en-US' AS Culture, N'Allowance' AS Value UNION ALL
    SELECT N'hr_allowance', N'vi-VN', N'Phụ cấp' UNION ALL
    SELECT N'hr_allowance', N'zh-CN', N'津贴' UNION ALL
    SELECT N'hr_approve' AS ResourceKey, N'en-US' AS Culture, N'Approve' AS Value UNION ALL
    SELECT N'hr_approve', N'vi-VN', N'Duyệt' UNION ALL
    SELECT N'hr_approve', N'zh-CN', N'批准' UNION ALL
    SELECT N'hr_bal_adjustment' AS ResourceKey, N'en-US' AS Culture, N'Adjustment' AS Value UNION ALL
    SELECT N'hr_bal_adjustment', N'vi-VN', N'Điều chỉnh' UNION ALL
    SELECT N'hr_bal_adjustment', N'zh-CN', N'调整' UNION ALL
    SELECT N'hr_bal_adjustment_hint' AS ResourceKey, N'en-US' AS Culture, N'Can be negative; kept on recalculation' AS Value UNION ALL
    SELECT N'hr_bal_adjustment_hint', N'vi-VN', N'Có thể âm; giữ nguyên khi tính lại' UNION ALL
    SELECT N'hr_bal_adjustment_hint', N'zh-CN', N'可为负数；重新计算时保留' UNION ALL
    SELECT N'hr_bal_available' AS ResourceKey, N'en-US' AS Culture, N'Available' AS Value UNION ALL
    SELECT N'hr_bal_available', N'vi-VN', N'Có thể dùng' UNION ALL
    SELECT N'hr_bal_available', N'zh-CN', N'可用' UNION ALL
    SELECT N'hr_bal_breakdown' AS ResourceKey, N'en-US' AS Culture, N'Standard {0} + seniority {1} + carried over {2} + adjustment {3}' AS Value UNION ALL
    SELECT N'hr_bal_breakdown', N'vi-VN', N'Tiêu chuẩn {0} + thâm niên {1} + tồn năm trước {2} + điều chỉnh {3}' UNION ALL
    SELECT N'hr_bal_breakdown', N'zh-CN', N'标准 {0} + 工龄 {1} + 上年结转 {2} + 调整 {3}' UNION ALL
    SELECT N'hr_bal_carried' AS ResourceKey, N'en-US' AS Culture, N'Carried over' AS Value UNION ALL
    SELECT N'hr_bal_carried', N'vi-VN', N'Tồn năm trước' UNION ALL
    SELECT N'hr_bal_carried', N'zh-CN', N'上年结转' UNION ALL
    SELECT N'hr_bal_edit' AS ResourceKey, N'en-US' AS Culture, N'Edit leave balance' AS Value UNION ALL
    SELECT N'hr_bal_edit', N'vi-VN', N'Sửa quỹ phép' UNION ALL
    SELECT N'hr_bal_edit', N'zh-CN', N'编辑年假额度' UNION ALL
    SELECT N'hr_bal_entitled' AS ResourceKey, N'en-US' AS Culture, N'Standard days' AS Value UNION ALL
    SELECT N'hr_bal_entitled', N'vi-VN', N'Phép tiêu chuẩn' UNION ALL
    SELECT N'hr_bal_entitled', N'zh-CN', N'标准天数' UNION ALL
    SELECT N'hr_bal_estimated' AS ResourceKey, N'en-US' AS Culture, N'Estimated from settings, not saved yet' AS Value UNION ALL
    SELECT N'hr_bal_estimated', N'vi-VN', N'Tạm tính theo cấu hình, chưa lưu' UNION ALL
    SELECT N'hr_bal_estimated', N'zh-CN', N'按设置估算，尚未保存' UNION ALL
    SELECT N'hr_bal_estimated_hint' AS ResourceKey, N'en-US' AS Culture, N'Rows with an hourglass are estimates — click "Create balances" to save them' AS Value UNION ALL
    SELECT N'hr_bal_estimated_hint', N'vi-VN', N'Dòng có biểu tượng đồng hồ cát là số tạm tính — bấm "Tạo quỹ phép" để lưu' UNION ALL
    SELECT N'hr_bal_estimated_hint', N'zh-CN', N'带沙漏图标的行为估算值——点击"生成额度"保存' UNION ALL
    SELECT N'hr_bal_generate' AS ResourceKey, N'en-US' AS Culture, N'Create balances' AS Value UNION ALL
    SELECT N'hr_bal_generate', N'vi-VN', N'Tạo quỹ phép' UNION ALL
    SELECT N'hr_bal_generate', N'zh-CN', N'生成额度' UNION ALL
    SELECT N'hr_bal_generate_confirm' AS ResourceKey, N'en-US' AS Culture, N'Create {0} leave balances for employees who don''t have one?' AS Value UNION ALL
    SELECT N'hr_bal_generate_confirm', N'vi-VN', N'Tạo quỹ phép năm {0} cho các nhân viên chưa có?' UNION ALL
    SELECT N'hr_bal_generate_confirm', N'zh-CN', N'为尚无额度的员工生成 {0} 年年假额度？' UNION ALL
    SELECT N'hr_bal_pending' AS ResourceKey, N'en-US' AS Culture, N'Pending' AS Value UNION ALL
    SELECT N'hr_bal_pending', N'vi-VN', N'Chờ duyệt' UNION ALL
    SELECT N'hr_bal_pending', N'zh-CN', N'待审批' UNION ALL
    SELECT N'hr_bal_preview' AS ResourceKey, N'en-US' AS Culture, N'Total {0} − used {1} = remaining {2}' AS Value UNION ALL
    SELECT N'hr_bal_preview', N'vi-VN', N'Tổng {0} − đã dùng {1} = còn lại {2}' UNION ALL
    SELECT N'hr_bal_preview', N'zh-CN', N'合计 {0} − 已用 {1} = 剩余 {2}' UNION ALL
    SELECT N'hr_bal_recalc' AS ResourceKey, N'en-US' AS Culture, N'Recalculate all' AS Value UNION ALL
    SELECT N'hr_bal_recalc', N'vi-VN', N'Tính lại tất cả' UNION ALL
    SELECT N'hr_bal_recalc', N'zh-CN', N'全部重新计算' UNION ALL
    SELECT N'hr_bal_recalc_confirm' AS ResourceKey, N'en-US' AS Culture, N'Recalculate standard, seniority and carried-over days for {0} for all employees? Manual adjustments are kept.' AS Value UNION ALL
    SELECT N'hr_bal_recalc_confirm', N'vi-VN', N'Tính lại phép tiêu chuẩn, thâm niên, tồn năm trước của năm {0} cho tất cả nhân viên? Phần điều chỉnh tay được giữ nguyên.' UNION ALL
    SELECT N'hr_bal_recalc_confirm', N'zh-CN', N'重新计算所有员工 {0} 年的标准、工龄和结转天数？手动调整保持不变。' UNION ALL
    SELECT N'hr_bal_remaining' AS ResourceKey, N'en-US' AS Culture, N'Remaining' AS Value UNION ALL
    SELECT N'hr_bal_remaining', N'vi-VN', N'Còn lại' UNION ALL
    SELECT N'hr_bal_remaining', N'zh-CN', N'剩余' UNION ALL
    SELECT N'hr_bal_seniority' AS ResourceKey, N'en-US' AS Culture, N'Seniority' AS Value UNION ALL
    SELECT N'hr_bal_seniority', N'vi-VN', N'Thâm niên' UNION ALL
    SELECT N'hr_bal_seniority', N'zh-CN', N'工龄假' UNION ALL
    SELECT N'hr_bal_total' AS ResourceKey, N'en-US' AS Culture, N'Total' AS Value UNION ALL
    SELECT N'hr_bal_total', N'vi-VN', N'Tổng phép' UNION ALL
    SELECT N'hr_bal_total', N'zh-CN', N'合计' UNION ALL
    SELECT N'hr_bal_used' AS ResourceKey, N'en-US' AS Culture, N'Used' AS Value UNION ALL
    SELECT N'hr_bal_used', N'vi-VN', N'Đã dùng' UNION ALL
    SELECT N'hr_bal_used', N'zh-CN', N'已用' UNION ALL
    SELECT N'hr_balance_generated' AS ResourceKey, N'en-US' AS Culture, N'{0} created, {1} recalculated' AS Value UNION ALL
    SELECT N'hr_balance_generated', N'vi-VN', N'Đã tạo {0} và tính lại {1} quỹ phép' UNION ALL
    SELECT N'hr_balance_generated', N'zh-CN', N'已生成 {0} 条，重新计算 {1} 条' UNION ALL
    SELECT N'hr_bank_account' AS ResourceKey, N'en-US' AS Culture, N'Bank account no.' AS Value UNION ALL
    SELECT N'hr_bank_account', N'vi-VN', N'Số tài khoản ngân hàng' UNION ALL
    SELECT N'hr_bank_account', N'zh-CN', N'银行账号' UNION ALL
    SELECT N'hr_bank_branch' AS ResourceKey, N'en-US' AS Culture, N'Bank branch' AS Value UNION ALL
    SELECT N'hr_bank_branch', N'vi-VN', N'Chi nhánh ngân hàng' UNION ALL
    SELECT N'hr_bank_branch', N'zh-CN', N'开户支行' UNION ALL
    SELECT N'hr_bank_name' AS ResourceKey, N'en-US' AS Culture, N'Bank' AS Value UNION ALL
    SELECT N'hr_bank_name', N'vi-VN', N'Ngân hàng' UNION ALL
    SELECT N'hr_bank_name', N'zh-CN', N'银行' UNION ALL
    SELECT N'hr_base_salary' AS ResourceKey, N'en-US' AS Culture, N'Base salary' AS Value UNION ALL
    SELECT N'hr_base_salary', N'vi-VN', N'Lương cơ bản' UNION ALL
    SELECT N'hr_base_salary', N'zh-CN', N'基本工资' UNION ALL
    SELECT N'hr_birthdays_this_month' AS ResourceKey, N'en-US' AS Culture, N'Birthdays this month' AS Value UNION ALL
    SELECT N'hr_birthdays_this_month', N'vi-VN', N'Sinh nhật trong tháng' UNION ALL
    SELECT N'hr_birthdays_this_month', N'zh-CN', N'本月生日' UNION ALL
    SELECT N'hr_branch' AS ResourceKey, N'en-US' AS Culture, N'Branch' AS Value UNION ALL
    SELECT N'hr_branch', N'vi-VN', N'Chi nhánh' UNION ALL
    SELECT N'hr_branch', N'zh-CN', N'分公司' UNION ALL
    SELECT N'hr_by_branch' AS ResourceKey, N'en-US' AS Culture, N'By branch' AS Value UNION ALL
    SELECT N'hr_by_branch', N'vi-VN', N'Theo chi nhánh' UNION ALL
    SELECT N'hr_by_branch', N'zh-CN', N'按分公司' UNION ALL
    SELECT N'hr_by_department' AS ResourceKey, N'en-US' AS Culture, N'By department' AS Value UNION ALL
    SELECT N'hr_by_department', N'vi-VN', N'Theo phòng ban' UNION ALL
    SELECT N'hr_by_department', N'zh-CN', N'按部门' UNION ALL
    SELECT N'hr_by_user' AS ResourceKey, N'en-US' AS Culture, N'By' AS Value UNION ALL
    SELECT N'hr_by_user', N'vi-VN', N'Người thực hiện' UNION ALL
    SELECT N'hr_by_user', N'zh-CN', N'操作人' UNION ALL
    SELECT N'hr_change_account' AS ResourceKey, N'en-US' AS Culture, N'Account change' AS Value UNION ALL
    SELECT N'hr_change_account', N'vi-VN', N'Đổi tài khoản' UNION ALL
    SELECT N'hr_change_account', N'zh-CN', N'账号变更' UNION ALL
    SELECT N'hr_change_branch' AS ResourceKey, N'en-US' AS Culture, N'Branch change' AS Value UNION ALL
    SELECT N'hr_change_branch', N'vi-VN', N'Đổi chi nhánh' UNION ALL
    SELECT N'hr_change_branch', N'zh-CN', N'分公司变更' UNION ALL
    SELECT N'hr_change_contract' AS ResourceKey, N'en-US' AS Culture, N'Contract signed' AS Value UNION ALL
    SELECT N'hr_change_contract', N'vi-VN', N'Ký hợp đồng' UNION ALL
    SELECT N'hr_change_contract', N'zh-CN', N'签订合同' UNION ALL
    SELECT N'hr_change_created' AS ResourceKey, N'en-US' AS Culture, N'Record created' AS Value UNION ALL
    SELECT N'hr_change_created', N'vi-VN', N'Tạo hồ sơ' UNION ALL
    SELECT N'hr_change_created', N'zh-CN', N'创建档案' UNION ALL
    SELECT N'hr_change_department' AS ResourceKey, N'en-US' AS Culture, N'Department change' AS Value UNION ALL
    SELECT N'hr_change_department', N'vi-VN', N'Đổi phòng ban' UNION ALL
    SELECT N'hr_change_department', N'zh-CN', N'部门变更' UNION ALL
    SELECT N'hr_change_manager' AS ResourceKey, N'en-US' AS Culture, N'Manager change' AS Value UNION ALL
    SELECT N'hr_change_manager', N'vi-VN', N'Đổi quản lý' UNION ALL
    SELECT N'hr_change_manager', N'zh-CN', N'上级变更' UNION ALL
    SELECT N'hr_change_position' AS ResourceKey, N'en-US' AS Culture, N'Position change' AS Value UNION ALL
    SELECT N'hr_change_position', N'vi-VN', N'Đổi chức vụ' UNION ALL
    SELECT N'hr_change_position', N'zh-CN', N'职位变更' UNION ALL
    SELECT N'hr_change_status' AS ResourceKey, N'en-US' AS Culture, N'Status change' AS Value UNION ALL
    SELECT N'hr_change_status', N'vi-VN', N'Đổi trạng thái' UNION ALL
    SELECT N'hr_change_status', N'zh-CN', N'状态变更' UNION ALL
    SELECT N'hr_change_type' AS ResourceKey, N'en-US' AS Culture, N'Change' AS Value UNION ALL
    SELECT N'hr_change_type', N'vi-VN', N'Loại thay đổi' UNION ALL
    SELECT N'hr_change_type', N'zh-CN', N'变更类型' UNION ALL
    SELECT N'hr_close' AS ResourceKey, N'en-US' AS Culture, N'Close' AS Value UNION ALL
    SELECT N'hr_close', N'vi-VN', N'Đóng' UNION ALL
    SELECT N'hr_close', N'zh-CN', N'关闭' UNION ALL
    SELECT N'hr_code' AS ResourceKey, N'en-US' AS Culture, N'Code' AS Value UNION ALL
    SELECT N'hr_code', N'vi-VN', N'Mã' UNION ALL
    SELECT N'hr_code', N'zh-CN', N'代码' UNION ALL
    SELECT N'hr_comment' AS ResourceKey, N'en-US' AS Culture, N'Comment' AS Value UNION ALL
    SELECT N'hr_comment', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'hr_comment', N'zh-CN', N'备注' UNION ALL
    SELECT N'hr_company_code' AS ResourceKey, N'en-US' AS Culture, N'Company code' AS Value UNION ALL
    SELECT N'hr_company_code', N'vi-VN', N'Mã công ty' UNION ALL
    SELECT N'hr_company_code', N'zh-CN', N'公司代码' UNION ALL
    SELECT N'hr_confirm' AS ResourceKey, N'en-US' AS Culture, N'Confirm' AS Value UNION ALL
    SELECT N'hr_confirm', N'vi-VN', N'Xác nhận' UNION ALL
    SELECT N'hr_confirm', N'zh-CN', N'确认' UNION ALL
    SELECT N'hr_contract_no' AS ResourceKey, N'en-US' AS Culture, N'Contract no.' AS Value UNION ALL
    SELECT N'hr_contract_no', N'vi-VN', N'Số hợp đồng' UNION ALL
    SELECT N'hr_contract_no', N'zh-CN', N'合同编号' UNION ALL
    SELECT N'hr_contract_status_active' AS ResourceKey, N'en-US' AS Culture, N'Active' AS Value UNION ALL
    SELECT N'hr_contract_status_active', N'vi-VN', N'Hiệu lực' UNION ALL
    SELECT N'hr_contract_status_active', N'zh-CN', N'有效' UNION ALL
    SELECT N'hr_contract_status_expired' AS ResourceKey, N'en-US' AS Culture, N'Expired' AS Value UNION ALL
    SELECT N'hr_contract_status_expired', N'vi-VN', N'Đã hết hạn' UNION ALL
    SELECT N'hr_contract_status_expired', N'zh-CN', N'已到期' UNION ALL
    SELECT N'hr_contract_status_terminated' AS ResourceKey, N'en-US' AS Culture, N'Terminated' AS Value UNION ALL
    SELECT N'hr_contract_status_terminated', N'vi-VN', N'Đã chấm dứt' UNION ALL
    SELECT N'hr_contract_status_terminated', N'zh-CN', N'已终止' UNION ALL
    SELECT N'hr_contract_type' AS ResourceKey, N'en-US' AS Culture, N'Contract type' AS Value UNION ALL
    SELECT N'hr_contract_type', N'vi-VN', N'Loại hợp đồng' UNION ALL
    SELECT N'hr_contract_type', N'zh-CN', N'合同类型' UNION ALL
    SELECT N'hr_contract_type_fixed' AS ResourceKey, N'en-US' AS Culture, N'Fixed-term' AS Value UNION ALL
    SELECT N'hr_contract_type_fixed', N'vi-VN', N'Xác định thời hạn' UNION ALL
    SELECT N'hr_contract_type_fixed', N'zh-CN', N'固定期限' UNION ALL
    SELECT N'hr_contract_type_indefinite' AS ResourceKey, N'en-US' AS Culture, N'Indefinite' AS Value UNION ALL
    SELECT N'hr_contract_type_indefinite', N'vi-VN', N'Không xác định thời hạn' UNION ALL
    SELECT N'hr_contract_type_indefinite', N'zh-CN', N'无固定期限' UNION ALL
    SELECT N'hr_contract_type_probation' AS ResourceKey, N'en-US' AS Culture, N'Probation' AS Value UNION ALL
    SELECT N'hr_contract_type_probation', N'vi-VN', N'Thử việc' UNION ALL
    SELECT N'hr_contract_type_probation', N'zh-CN', N'试用' UNION ALL
    SELECT N'hr_contract_type_service' AS ResourceKey, N'en-US' AS Culture, N'Service / seasonal' AS Value UNION ALL
    SELECT N'hr_contract_type_service', N'vi-VN', N'Dịch vụ / thời vụ' UNION ALL
    SELECT N'hr_contract_type_service', N'zh-CN', N'劳务/季节性' UNION ALL
    SELECT N'hr_contracts' AS ResourceKey, N'en-US' AS Culture, N'Labour contracts' AS Value UNION ALL
    SELECT N'hr_contracts', N'vi-VN', N'Hợp đồng lao động' UNION ALL
    SELECT N'hr_contracts', N'zh-CN', N'劳动合同' UNION ALL
    SELECT N'hr_contracts_expiring' AS ResourceKey, N'en-US' AS Culture, N'Contracts expiring' AS Value UNION ALL
    SELECT N'hr_contracts_expiring', N'vi-VN', N'Hợp đồng sắp hết hạn' UNION ALL
    SELECT N'hr_contracts_expiring', N'zh-CN', N'即将到期合同' UNION ALL
    SELECT N'hr_contracts_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Track contracts, terms and expiry alerts' AS Value UNION ALL
    SELECT N'hr_contracts_subtitle', N'vi-VN', N'Theo dõi hợp đồng, thời hạn và cảnh báo hết hạn' UNION ALL
    SELECT N'hr_contracts_subtitle', N'zh-CN', N'跟踪合同、期限及到期提醒' UNION ALL
    SELECT N'hr_current_address' AS ResourceKey, N'en-US' AS Culture, N'Current address' AS Value UNION ALL
    SELECT N'hr_current_address', N'vi-VN', N'Địa chỉ hiện tại' UNION ALL
    SELECT N'hr_current_address', N'zh-CN', N'现住址' UNION ALL
    SELECT N'hr_dashboard' AS ResourceKey, N'en-US' AS Culture, N'HR overview' AS Value UNION ALL
    SELECT N'hr_dashboard', N'vi-VN', N'Tổng quan nhân sự' UNION ALL
    SELECT N'hr_dashboard', N'zh-CN', N'人事概览' UNION ALL
    SELECT N'hr_dashboard_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Headcount figures and alerts that need action' AS Value UNION ALL
    SELECT N'hr_dashboard_subtitle', N'vi-VN', N'Số liệu nhân sự và các cảnh báo cần xử lý' UNION ALL
    SELECT N'hr_dashboard_subtitle', N'zh-CN', N'人员统计及待处理提醒' UNION ALL
    SELECT N'hr_date' AS ResourceKey, N'en-US' AS Culture, N'Date' AS Value UNION ALL
    SELECT N'hr_date', N'vi-VN', N'Ngày' UNION ALL
    SELECT N'hr_date', N'zh-CN', N'日期' UNION ALL
    SELECT N'hr_days' AS ResourceKey, N'en-US' AS Culture, N'days' AS Value UNION ALL
    SELECT N'hr_days', N'vi-VN', N'ngày' UNION ALL
    SELECT N'hr_days', N'zh-CN', N'天' UNION ALL
    SELECT N'hr_days_left' AS ResourceKey, N'en-US' AS Culture, N'{0} days left' AS Value UNION ALL
    SELECT N'hr_days_left', N'vi-VN', N'còn {0} ngày' UNION ALL
    SELECT N'hr_days_left', N'zh-CN', N'剩余 {0} 天' UNION ALL
    SELECT N'hr_delete_contract_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete contract {0}?' AS Value UNION ALL
    SELECT N'hr_delete_contract_confirm', N'vi-VN', N'Xóa hợp đồng {0}?' UNION ALL
    SELECT N'hr_delete_contract_confirm', N'zh-CN', N'删除合同 {0}？' UNION ALL
    SELECT N'hr_delete_doc_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete document {0}?' AS Value UNION ALL
    SELECT N'hr_delete_doc_confirm', N'vi-VN', N'Xóa giấy tờ {0}?' UNION ALL
    SELECT N'hr_delete_doc_confirm', N'zh-CN', N'删除文件 {0}？' UNION ALL
    SELECT N'hr_delete_employee_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete record {0}? For employees who have left, set the status to "Resigned" instead of deleting.' AS Value UNION ALL
    SELECT N'hr_delete_employee_confirm', N'vi-VN', N'Xóa hồ sơ {0}? Nếu nhân viên đã nghỉ, nên chuyển trạng thái "Đã nghỉ việc" thay vì xóa.' UNION ALL
    SELECT N'hr_delete_employee_confirm', N'zh-CN', N'删除档案 {0}？员工离职时建议将状态改为"已离职"而不是删除。' UNION ALL
    SELECT N'hr_delete_item_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete {0}?' AS Value UNION ALL
    SELECT N'hr_delete_item_confirm', N'vi-VN', N'Xóa {0}?' UNION ALL
    SELECT N'hr_delete_item_confirm', N'zh-CN', N'删除 {0}？' UNION ALL
    SELECT N'hr_deleted' AS ResourceKey, N'en-US' AS Culture, N'Deleted' AS Value UNION ALL
    SELECT N'hr_deleted', N'vi-VN', N'Đã xóa' UNION ALL
    SELECT N'hr_deleted', N'zh-CN', N'已删除' UNION ALL
    SELECT N'hr_department' AS ResourceKey, N'en-US' AS Culture, N'Department' AS Value UNION ALL
    SELECT N'hr_department', N'vi-VN', N'Phòng ban' UNION ALL
    SELECT N'hr_department', N'zh-CN', N'部门' UNION ALL
    SELECT N'hr_departments' AS ResourceKey, N'en-US' AS Culture, N'Departments' AS Value UNION ALL
    SELECT N'hr_departments', N'vi-VN', N'Phòng ban' UNION ALL
    SELECT N'hr_departments', N'zh-CN', N'部门' UNION ALL
    SELECT N'hr_dependent_count' AS ResourceKey, N'en-US' AS Culture, N'Dependants' AS Value UNION ALL
    SELECT N'hr_dependent_count', N'vi-VN', N'Số người phụ thuộc' UNION ALL
    SELECT N'hr_dependent_count', N'zh-CN', N'被抚养人数' UNION ALL
    SELECT N'hr_dept_code_hint' AS ResourceKey, N'en-US' AS Culture, N'The department code is also the "Department" value of user accounts' AS Value UNION ALL
    SELECT N'hr_dept_code_hint', N'vi-VN', N'Mã phòng ban cũng là giá trị "Phòng ban" của tài khoản người dùng' UNION ALL
    SELECT N'hr_dept_code_hint', N'zh-CN', N'部门代码同时也是用户账号的"部门"值' UNION ALL
    SELECT N'hr_dept_code_locked_hint' AS ResourceKey, N'en-US' AS Culture, N'This code is used by user accounts and cannot be changed' AS Value UNION ALL
    SELECT N'hr_dept_code_locked_hint', N'vi-VN', N'Đang có tài khoản dùng mã này nên không thể đổi' UNION ALL
    SELECT N'hr_dept_code_locked_hint', N'zh-CN', N'已有账号使用此代码，无法修改' UNION ALL
    SELECT N'hr_dob' AS ResourceKey, N'en-US' AS Culture, N'Date of birth' AS Value UNION ALL
    SELECT N'hr_dob', N'vi-VN', N'Ngày sinh' UNION ALL
    SELECT N'hr_dob', N'zh-CN', N'出生日期' UNION ALL
    SELECT N'hr_doc_limit' AS ResourceKey, N'en-US' AS Culture, N'Max {0} MB: pdf, images, Word, Excel, zip, rar' AS Value UNION ALL
    SELECT N'hr_doc_limit', N'vi-VN', N'Tối đa {0} MB: pdf, ảnh, Word, Excel, zip, rar' UNION ALL
    SELECT N'hr_doc_limit', N'zh-CN', N'最大 {0} MB：pdf、图片、Word、Excel、zip、rar' UNION ALL
    SELECT N'hr_doc_path_invalid' AS ResourceKey, N'en-US' AS Culture, N'Invalid file path' AS Value UNION ALL
    SELECT N'hr_doc_path_invalid', N'vi-VN', N'Đường dẫn file không hợp lệ' UNION ALL
    SELECT N'hr_doc_path_invalid', N'zh-CN', N'文件路径无效' UNION ALL
    SELECT N'hr_doc_title' AS ResourceKey, N'en-US' AS Culture, N'Title' AS Value UNION ALL
    SELECT N'hr_doc_title', N'vi-VN', N'Tiêu đề' UNION ALL
    SELECT N'hr_doc_title', N'zh-CN', N'标题' UNION ALL
    SELECT N'hr_doc_too_large' AS ResourceKey, N'en-US' AS Culture, N'File exceeds the size limit' AS Value UNION ALL
    SELECT N'hr_doc_too_large', N'vi-VN', N'File vượt quá dung lượng cho phép' UNION ALL
    SELECT N'hr_doc_too_large', N'zh-CN', N'文件超过大小限制' UNION ALL
    SELECT N'hr_doc_type' AS ResourceKey, N'en-US' AS Culture, N'Document type' AS Value UNION ALL
    SELECT N'hr_doc_type', N'vi-VN', N'Loại giấy tờ' UNION ALL
    SELECT N'hr_doc_type', N'zh-CN', N'文件类型' UNION ALL
    SELECT N'hr_doc_type_not_allowed' AS ResourceKey, N'en-US' AS Culture, N'File type not allowed' AS Value UNION ALL
    SELECT N'hr_doc_type_not_allowed', N'vi-VN', N'Định dạng file không được phép' UNION ALL
    SELECT N'hr_doc_type_not_allowed', N'zh-CN', N'不允许的文件格式' UNION ALL
    SELECT N'hr_doc_uploaded' AS ResourceKey, N'en-US' AS Culture, N'Uploaded' AS Value UNION ALL
    SELECT N'hr_doc_uploaded', N'vi-VN', N'Đã tải lên' UNION ALL
    SELECT N'hr_doc_uploaded', N'zh-CN', N'已上传' UNION ALL
    SELECT N'hr_doctype_contract' AS ResourceKey, N'en-US' AS Culture, N'Contract / Annex' AS Value UNION ALL
    SELECT N'hr_doctype_contract', N'vi-VN', N'Hợp đồng / Phụ lục' UNION ALL
    SELECT N'hr_doctype_contract', N'zh-CN', N'合同/附件' UNION ALL
    SELECT N'hr_doctype_cv' AS ResourceKey, N'en-US' AS Culture, N'CV / Resume' AS Value UNION ALL
    SELECT N'hr_doctype_cv', N'vi-VN', N'CV / Sơ yếu lý lịch' UNION ALL
    SELECT N'hr_doctype_cv', N'zh-CN', N'简历' UNION ALL
    SELECT N'hr_doctype_degree' AS ResourceKey, N'en-US' AS Culture, N'Degree / Certificate' AS Value UNION ALL
    SELECT N'hr_doctype_degree', N'vi-VN', N'Bằng cấp / Chứng chỉ' UNION ALL
    SELECT N'hr_doctype_degree', N'zh-CN', N'学历/证书' UNION ALL
    SELECT N'hr_doctype_health' AS ResourceKey, N'en-US' AS Culture, N'Health certificate' AS Value UNION ALL
    SELECT N'hr_doctype_health', N'vi-VN', N'Giấy khám sức khỏe' UNION ALL
    SELECT N'hr_doctype_health', N'zh-CN', N'体检证明' UNION ALL
    SELECT N'hr_doctype_idcard' AS ResourceKey, N'en-US' AS Culture, N'ID card / Passport' AS Value UNION ALL
    SELECT N'hr_doctype_idcard', N'vi-VN', N'CCCD / Hộ chiếu' UNION ALL
    SELECT N'hr_doctype_idcard', N'zh-CN', N'身份证/护照' UNION ALL
    SELECT N'hr_doctype_other' AS ResourceKey, N'en-US' AS Culture, N'Other' AS Value UNION ALL
    SELECT N'hr_doctype_other', N'vi-VN', N'Khác' UNION ALL
    SELECT N'hr_doctype_other', N'zh-CN', N'其他' UNION ALL
    SELECT N'hr_download' AS ResourceKey, N'en-US' AS Culture, N'Download' AS Value UNION ALL
    SELECT N'hr_download', N'vi-VN', N'Tải về' UNION ALL
    SELECT N'hr_download', N'zh-CN', N'下载' UNION ALL
    SELECT N'hr_duration_full' AS ResourceKey, N'en-US' AS Culture, N'Full day' AS Value UNION ALL
    SELECT N'hr_duration_full', N'vi-VN', N'Cả ngày' UNION ALL
    SELECT N'hr_duration_full', N'zh-CN', N'全天' UNION ALL
    SELECT N'hr_duration_half' AS ResourceKey, N'en-US' AS Culture, N'Half day' AS Value UNION ALL
    SELECT N'hr_duration_half', N'vi-VN', N'Nửa ngày' UNION ALL
    SELECT N'hr_duration_half', N'zh-CN', N'半天' UNION ALL
    SELECT N'hr_duration_hours' AS ResourceKey, N'en-US' AS Culture, N'By hours' AS Value UNION ALL
    SELECT N'hr_duration_hours', N'vi-VN', N'Theo giờ' UNION ALL
    SELECT N'hr_duration_hours', N'zh-CN', N'按小时' UNION ALL
    SELECT N'hr_edit_contract' AS ResourceKey, N'en-US' AS Culture, N'Edit contract' AS Value UNION ALL
    SELECT N'hr_edit_contract', N'vi-VN', N'Sửa hợp đồng' UNION ALL
    SELECT N'hr_edit_contract', N'zh-CN', N'编辑合同' UNION ALL
    SELECT N'hr_edit_department' AS ResourceKey, N'en-US' AS Culture, N'Edit department' AS Value UNION ALL
    SELECT N'hr_edit_department', N'vi-VN', N'Sửa phòng ban' UNION ALL
    SELECT N'hr_edit_department', N'zh-CN', N'编辑部门' UNION ALL
    SELECT N'hr_edit_employee' AS ResourceKey, N'en-US' AS Culture, N'Edit employee' AS Value UNION ALL
    SELECT N'hr_edit_employee', N'vi-VN', N'Sửa hồ sơ' UNION ALL
    SELECT N'hr_edit_employee', N'zh-CN', N'编辑员工'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Phần 2/5
;WITH src AS (
    SELECT N'hr_edit_position' AS ResourceKey, N'en-US' AS Culture, N'Edit position' AS Value UNION ALL
    SELECT N'hr_edit_position', N'vi-VN', N'Sửa chức vụ' UNION ALL
    SELECT N'hr_edit_position', N'zh-CN', N'编辑职位' UNION ALL
    SELECT N'hr_effective_date' AS ResourceKey, N'en-US' AS Culture, N'Effective date' AS Value UNION ALL
    SELECT N'hr_effective_date', N'vi-VN', N'Ngày hiệu lực' UNION ALL
    SELECT N'hr_effective_date', N'zh-CN', N'生效日期' UNION ALL
    SELECT N'hr_emergency_name' AS ResourceKey, N'en-US' AS Culture, N'Emergency contact' AS Value UNION ALL
    SELECT N'hr_emergency_name', N'vi-VN', N'Người liên hệ khẩn cấp' UNION ALL
    SELECT N'hr_emergency_name', N'zh-CN', N'紧急联系人' UNION ALL
    SELECT N'hr_emergency_phone' AS ResourceKey, N'en-US' AS Culture, N'Emergency phone' AS Value UNION ALL
    SELECT N'hr_emergency_phone', N'vi-VN', N'SĐT liên hệ khẩn cấp' UNION ALL
    SELECT N'hr_emergency_phone', N'zh-CN', N'紧急联系电话' UNION ALL
    SELECT N'hr_employee' AS ResourceKey, N'en-US' AS Culture, N'Employee' AS Value UNION ALL
    SELECT N'hr_employee', N'vi-VN', N'Nhân viên' UNION ALL
    SELECT N'hr_employee', N'zh-CN', N'员工' UNION ALL
    SELECT N'hr_employee_code' AS ResourceKey, N'en-US' AS Culture, N'Employee code' AS Value UNION ALL
    SELECT N'hr_employee_code', N'vi-VN', N'Mã NV' UNION ALL
    SELECT N'hr_employee_code', N'zh-CN', N'员工编号' UNION ALL
    SELECT N'hr_employee_count' AS ResourceKey, N'en-US' AS Culture, N'Employees' AS Value UNION ALL
    SELECT N'hr_employee_count', N'vi-VN', N'Số nhân viên' UNION ALL
    SELECT N'hr_employee_count', N'zh-CN', N'员工数' UNION ALL
    SELECT N'hr_employees' AS ResourceKey, N'en-US' AS Culture, N'Employee records' AS Value UNION ALL
    SELECT N'hr_employees', N'vi-VN', N'Hồ sơ nhân viên' UNION ALL
    SELECT N'hr_employees', N'zh-CN', N'员工档案' UNION ALL
    SELECT N'hr_employees_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Manage employee details, jobs and documents' AS Value UNION ALL
    SELECT N'hr_employees_subtitle', N'vi-VN', N'Quản lý thông tin, công việc, giấy tờ của nhân viên' UNION ALL
    SELECT N'hr_employees_subtitle', N'zh-CN', N'管理员工信息、岗位和证件' UNION ALL
    SELECT N'hr_end_date' AS ResourceKey, N'en-US' AS Culture, N'End date' AS Value UNION ALL
    SELECT N'hr_end_date', N'vi-VN', N'Ngày kết thúc' UNION ALL
    SELECT N'hr_end_date', N'zh-CN', N'结束日期' UNION ALL
    SELECT N'hr_err_code_exists' AS ResourceKey, N'en-US' AS Culture, N'Code already exists' AS Value UNION ALL
    SELECT N'hr_err_code_exists', N'vi-VN', N'Mã đã tồn tại' UNION ALL
    SELECT N'hr_err_code_exists', N'zh-CN', N'代码已存在' UNION ALL
    SELECT N'hr_err_code_required' AS ResourceKey, N'en-US' AS Culture, N'Code is required' AS Value UNION ALL
    SELECT N'hr_err_code_required', N'vi-VN', N'Vui lòng nhập mã' UNION ALL
    SELECT N'hr_err_code_required', N'zh-CN', N'请输入代码' UNION ALL
    SELECT N'hr_err_comment_required' AS ResourceKey, N'en-US' AS Culture, N'Please enter a comment' AS Value UNION ALL
    SELECT N'hr_err_comment_required', N'vi-VN', N'Vui lòng nhập ghi chú' UNION ALL
    SELECT N'hr_err_comment_required', N'zh-CN', N'请输入备注' UNION ALL
    SELECT N'hr_err_contract_no_exists' AS ResourceKey, N'en-US' AS Culture, N'Contract number already exists' AS Value UNION ALL
    SELECT N'hr_err_contract_no_exists', N'vi-VN', N'Số hợp đồng đã tồn tại' UNION ALL
    SELECT N'hr_err_contract_no_exists', N'zh-CN', N'合同编号已存在' UNION ALL
    SELECT N'hr_err_contract_no_required' AS ResourceKey, N'en-US' AS Culture, N'Contract number is required' AS Value UNION ALL
    SELECT N'hr_err_contract_no_required', N'vi-VN', N'Vui lòng nhập số hợp đồng' UNION ALL
    SELECT N'hr_err_contract_no_required', N'zh-CN', N'请输入合同编号' UNION ALL
    SELECT N'hr_err_contract_type' AS ResourceKey, N'en-US' AS Culture, N'Invalid contract type' AS Value UNION ALL
    SELECT N'hr_err_contract_type', N'vi-VN', N'Loại hợp đồng không hợp lệ' UNION ALL
    SELECT N'hr_err_contract_type', N'zh-CN', N'合同类型无效' UNION ALL
    SELECT N'hr_err_delete' AS ResourceKey, N'en-US' AS Culture, N'Could not delete' AS Value UNION ALL
    SELECT N'hr_err_delete', N'vi-VN', N'Không xóa được' UNION ALL
    SELECT N'hr_err_delete', N'zh-CN', N'删除失败' UNION ALL
    SELECT N'hr_err_dept_code_in_use' AS ResourceKey, N'en-US' AS Culture, N'The department code is used by user accounts' AS Value UNION ALL
    SELECT N'hr_err_dept_code_in_use', N'vi-VN', N'Mã phòng ban đang được tài khoản người dùng sử dụng' UNION ALL
    SELECT N'hr_err_dept_code_in_use', N'zh-CN', N'部门代码正被用户账号使用' UNION ALL
    SELECT N'hr_err_dept_has_employees' AS ResourceKey, N'en-US' AS Culture, N'The department still has employees' AS Value UNION ALL
    SELECT N'hr_err_dept_has_employees', N'vi-VN', N'Phòng ban đang có nhân viên' UNION ALL
    SELECT N'hr_err_dept_has_employees', N'zh-CN', N'该部门仍有员工' UNION ALL
    SELECT N'hr_err_email' AS ResourceKey, N'en-US' AS Culture, N'Invalid email' AS Value UNION ALL
    SELECT N'hr_err_email', N'vi-VN', N'Email không hợp lệ' UNION ALL
    SELECT N'hr_err_email', N'zh-CN', N'邮箱无效' UNION ALL
    SELECT N'hr_err_employee_required' AS ResourceKey, N'en-US' AS Culture, N'Please select an employee' AS Value UNION ALL
    SELECT N'hr_err_employee_required', N'vi-VN', N'Vui lòng chọn nhân viên' UNION ALL
    SELECT N'hr_err_employee_required', N'zh-CN', N'请选择员工' UNION ALL
    SELECT N'hr_err_end_before_start' AS ResourceKey, N'en-US' AS Culture, N'End date must be after start date' AS Value UNION ALL
    SELECT N'hr_err_end_before_start', N'vi-VN', N'Ngày kết thúc phải sau ngày bắt đầu' UNION ALL
    SELECT N'hr_err_end_before_start', N'zh-CN', N'结束日期必须晚于开始日期' UNION ALL
    SELECT N'hr_err_end_date_required' AS ResourceKey, N'en-US' AS Culture, N'This contract type needs an end date' AS Value UNION ALL
    SELECT N'hr_err_end_date_required', N'vi-VN', N'Loại hợp đồng này cần ngày kết thúc' UNION ALL
    SELECT N'hr_err_end_date_required', N'zh-CN', N'此合同类型需要结束日期' UNION ALL
    SELECT N'hr_err_export' AS ResourceKey, N'en-US' AS Culture, N'Export failed' AS Value UNION ALL
    SELECT N'hr_err_export', N'vi-VN', N'Không xuất được file' UNION ALL
    SELECT N'hr_err_export', N'zh-CN', N'导出失败' UNION ALL
    SELECT N'hr_err_form_invalid' AS ResourceKey, N'en-US' AS Culture, N'Please check the required fields' AS Value UNION ALL
    SELECT N'hr_err_form_invalid', N'vi-VN', N'Vui lòng kiểm tra lại các trường bắt buộc' UNION ALL
    SELECT N'hr_err_form_invalid', N'zh-CN', N'请检查必填项' UNION ALL
    SELECT N'hr_err_has_contracts' AS ResourceKey, N'en-US' AS Culture, N'The employee has contracts and cannot be deleted. Set the status to "Resigned" instead.' AS Value UNION ALL
    SELECT N'hr_err_has_contracts', N'vi-VN', N'Nhân viên đã có hợp đồng, không thể xóa. Hãy chuyển trạng thái sang "Đã nghỉ việc".' UNION ALL
    SELECT N'hr_err_has_contracts', N'zh-CN', N'该员工已有合同，无法删除，请将状态改为"已离职"。' UNION ALL
    SELECT N'hr_err_has_subordinates' AS ResourceKey, N'en-US' AS Culture, N'The employee is manager of other employees' AS Value UNION ALL
    SELECT N'hr_err_has_subordinates', N'vi-VN', N'Nhân viên đang là quản lý của người khác' UNION ALL
    SELECT N'hr_err_has_subordinates', N'zh-CN', N'该员工是其他员工的上级' UNION ALL
    SELECT N'hr_err_holiday_exists' AS ResourceKey, N'en-US' AS Culture, N'This date is already a holiday' AS Value UNION ALL
    SELECT N'hr_err_holiday_exists', N'vi-VN', N'Ngày này đã có trong danh sách ngày lễ' UNION ALL
    SELECT N'hr_err_holiday_exists', N'zh-CN', N'该日期已是节假日' UNION ALL
    SELECT N'hr_err_hours_per_day' AS ResourceKey, N'en-US' AS Culture, N'Hours per day must be 1–24' AS Value UNION ALL
    SELECT N'hr_err_hours_per_day', N'vi-VN', N'Số giờ/ngày phải từ 1 đến 24' UNION ALL
    SELECT N'hr_err_hours_per_day', N'zh-CN', N'每日工时须为 1–24' UNION ALL
    SELECT N'hr_err_leave_already_reviewed' AS ResourceKey, N'en-US' AS Culture, N'The request has already been reviewed' AS Value UNION ALL
    SELECT N'hr_err_leave_already_reviewed', N'vi-VN', N'Đơn đã được xử lý' UNION ALL
    SELECT N'hr_err_leave_already_reviewed', N'zh-CN', N'该申请已处理' UNION ALL
    SELECT N'hr_err_leave_duration' AS ResourceKey, N'en-US' AS Culture, N'Invalid duration type' AS Value UNION ALL
    SELECT N'hr_err_leave_duration', N'vi-VN', N'Hình thức nghỉ không hợp lệ' UNION ALL
    SELECT N'hr_err_leave_duration', N'zh-CN', N'时长类型无效' UNION ALL
    SELECT N'hr_err_leave_goout_fullday' AS ResourceKey, N'en-US' AS Culture, N'Going out can only be half day or by hours' AS Value UNION ALL
    SELECT N'hr_err_leave_goout_fullday', N'vi-VN', N'Xin ra ngoài chỉ chọn nửa ngày hoặc theo giờ' UNION ALL
    SELECT N'hr_err_leave_goout_fullday', N'zh-CN', N'外出只能选择半天或按小时' UNION ALL
    SELECT N'hr_err_leave_insufficient' AS ResourceKey, N'en-US' AS Culture, N'Not enough annual leave. Days available' AS Value UNION ALL
    SELECT N'hr_err_leave_insufficient', N'vi-VN', N'Không đủ phép năm. Số ngày còn có thể dùng' UNION ALL
    SELECT N'hr_err_leave_insufficient', N'zh-CN', N'年假不足。可用天数' UNION ALL
    SELECT N'hr_err_leave_no_workday' AS ResourceKey, N'en-US' AS Culture, N'The selected period has no working day' AS Value UNION ALL
    SELECT N'hr_err_leave_no_workday', N'vi-VN', N'Khoảng thời gian chọn không có ngày làm việc' UNION ALL
    SELECT N'hr_err_leave_no_workday', N'zh-CN', N'所选时间段内没有工作日' UNION ALL
    SELECT N'hr_err_leave_not_approved' AS ResourceKey, N'en-US' AS Culture, N'Only approved requests can be cancelled' AS Value UNION ALL
    SELECT N'hr_err_leave_not_approved', N'vi-VN', N'Chỉ hủy được đơn đã duyệt' UNION ALL
    SELECT N'hr_err_leave_not_approved', N'zh-CN', N'只能取消已批准的申请' UNION ALL
    SELECT N'hr_err_leave_not_approver' AS ResourceKey, N'en-US' AS Culture, N'You are not the approver of this request' AS Value UNION ALL
    SELECT N'hr_err_leave_not_approver', N'vi-VN', N'Bạn không phải người duyệt đơn này' UNION ALL
    SELECT N'hr_err_leave_not_approver', N'zh-CN', N'您不是该申请的审批人' UNION ALL
    SELECT N'hr_err_leave_not_owner' AS ResourceKey, N'en-US' AS Culture, N'You can only change your own requests' AS Value UNION ALL
    SELECT N'hr_err_leave_not_owner', N'vi-VN', N'Bạn chỉ sửa/xóa được đơn của mình' UNION ALL
    SELECT N'hr_err_leave_not_owner', N'zh-CN', N'只能修改自己的申请' UNION ALL
    SELECT N'hr_err_leave_not_pending' AS ResourceKey, N'en-US' AS Culture, N'Only pending requests can be changed' AS Value UNION ALL
    SELECT N'hr_err_leave_not_pending', N'vi-VN', N'Chỉ sửa/xóa được đơn đang chờ duyệt' UNION ALL
    SELECT N'hr_err_leave_not_pending', N'zh-CN', N'只能修改待审批的申请' UNION ALL
    SELECT N'hr_err_leave_overlap' AS ResourceKey, N'en-US' AS Culture, N'Overlaps another request on' AS Value UNION ALL
    SELECT N'hr_err_leave_overlap', N'vi-VN', N'Trùng với đơn nghỉ khác ngày' UNION ALL
    SELECT N'hr_err_leave_overlap', N'zh-CN', N'与另一申请重叠，日期' UNION ALL
    SELECT N'hr_err_leave_reason' AS ResourceKey, N'en-US' AS Culture, N'Please enter a reason' AS Value UNION ALL
    SELECT N'hr_err_leave_reason', N'vi-VN', N'Vui lòng nhập lý do' UNION ALL
    SELECT N'hr_err_leave_reason', N'zh-CN', N'请输入原因' UNION ALL
    SELECT N'hr_err_leave_saturday_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Saturday is morning-only; no afternoon leave' AS Value UNION ALL
    SELECT N'hr_err_leave_saturday_afternoon', N'vi-VN', N'Thứ 7 chỉ làm buổi sáng, không xin nghỉ buổi chiều' UNION ALL
    SELECT N'hr_err_leave_saturday_afternoon', N'zh-CN', N'周六仅上午上班，不能请下午假' UNION ALL
    SELECT N'hr_err_leave_self_review' AS ResourceKey, N'en-US' AS Culture, N'You cannot review your own request' AS Value UNION ALL
    SELECT N'hr_err_leave_self_review', N'vi-VN', N'Không thể tự duyệt đơn của mình' UNION ALL
    SELECT N'hr_err_leave_self_review', N'zh-CN', N'不能审批自己的申请' UNION ALL
    SELECT N'hr_err_leave_time_range' AS ResourceKey, N'en-US' AS Culture, N'End time must be after start time' AS Value UNION ALL
    SELECT N'hr_err_leave_time_range', N'vi-VN', N'Giờ kết thúc phải sau giờ bắt đầu' UNION ALL
    SELECT N'hr_err_leave_time_range', N'zh-CN', N'结束时间须晚于开始时间' UNION ALL
    SELECT N'hr_err_leave_too_long' AS ResourceKey, N'en-US' AS Culture, N'A request can cover at most 90 days' AS Value UNION ALL
    SELECT N'hr_err_leave_too_long', N'vi-VN', N'Mỗi đơn tối đa 90 ngày' UNION ALL
    SELECT N'hr_err_leave_too_long', N'zh-CN', N'每份申请最多 90 天' UNION ALL
    SELECT N'hr_err_leave_type' AS ResourceKey, N'en-US' AS Culture, N'Invalid leave type' AS Value UNION ALL
    SELECT N'hr_err_leave_type', N'vi-VN', N'Loại nghỉ không hợp lệ' UNION ALL
    SELECT N'hr_err_leave_type', N'zh-CN', N'假别无效' UNION ALL
    SELECT N'hr_err_load' AS ResourceKey, N'en-US' AS Culture, N'Could not load data' AS Value UNION ALL
    SELECT N'hr_err_load', N'vi-VN', N'Không tải được dữ liệu' UNION ALL
    SELECT N'hr_err_load', N'zh-CN', N'数据加载失败' UNION ALL
    SELECT N'hr_err_manager_cycle' AS ResourceKey, N'en-US' AS Culture, N'This manager would create a reporting loop' AS Value UNION ALL
    SELECT N'hr_err_manager_cycle', N'vi-VN', N'Chọn quản lý này sẽ tạo vòng lặp quản lý' UNION ALL
    SELECT N'hr_err_manager_cycle', N'zh-CN', N'该上级会造成汇报循环' UNION ALL
    SELECT N'hr_err_manager_self' AS ResourceKey, N'en-US' AS Culture, N'An employee cannot be their own manager' AS Value UNION ALL
    SELECT N'hr_err_manager_self', N'vi-VN', N'Nhân viên không thể tự quản lý chính mình' UNION ALL
    SELECT N'hr_err_manager_self', N'zh-CN', N'员工不能是自己的上级' UNION ALL
    SELECT N'hr_err_name_required' AS ResourceKey, N'en-US' AS Culture, N'Name is required' AS Value UNION ALL
    SELECT N'hr_err_name_required', N'vi-VN', N'Vui lòng nhập tên' UNION ALL
    SELECT N'hr_err_name_required', N'zh-CN', N'请输入名称' UNION ALL
    SELECT N'hr_err_negative_value' AS ResourceKey, N'en-US' AS Culture, N'Value cannot be negative' AS Value UNION ALL
    SELECT N'hr_err_negative_value', N'vi-VN', N'Giá trị không được âm' UNION ALL
    SELECT N'hr_err_negative_value', N'zh-CN', N'数值不能为负' UNION ALL
    SELECT N'hr_err_no_user' AS ResourceKey, N'en-US' AS Culture, N'Cannot identify the logged-in account' AS Value UNION ALL
    SELECT N'hr_err_no_user', N'vi-VN', N'Không xác định được tài khoản đăng nhập' UNION ALL
    SELECT N'hr_err_no_user', N'zh-CN', N'无法识别当前登录账号' UNION ALL
    SELECT N'hr_err_not_found' AS ResourceKey, N'en-US' AS Culture, N'Record not found' AS Value UNION ALL
    SELECT N'hr_err_not_found', N'vi-VN', N'Không tìm thấy dữ liệu' UNION ALL
    SELECT N'hr_err_not_found', N'zh-CN', N'未找到记录' UNION ALL
    SELECT N'hr_err_position_in_use' AS ResourceKey, N'en-US' AS Culture, N'The position is assigned to employees' AS Value UNION ALL
    SELECT N'hr_err_position_in_use', N'vi-VN', N'Chức vụ đang được gán cho nhân viên' UNION ALL
    SELECT N'hr_err_position_in_use', N'zh-CN', N'该职位已分配给员工' UNION ALL
    SELECT N'hr_err_resign_date_required' AS ResourceKey, N'en-US' AS Culture, N'Resignation date is required' AS Value UNION ALL
    SELECT N'hr_err_resign_date_required', N'vi-VN', N'Vui lòng nhập ngày nghỉ việc' UNION ALL
    SELECT N'hr_err_resign_date_required', N'zh-CN', N'请输入离职日期' UNION ALL
    SELECT N'hr_err_save' AS ResourceKey, N'en-US' AS Culture, N'Could not save' AS Value UNION ALL
    SELECT N'hr_err_save', N'vi-VN', N'Không lưu được' UNION ALL
    SELECT N'hr_err_save', N'zh-CN', N'保存失败' UNION ALL
    SELECT N'hr_err_terminated_date_required' AS ResourceKey, N'en-US' AS Culture, N'Termination date is required' AS Value UNION ALL
    SELECT N'hr_err_terminated_date_required', N'vi-VN', N'Vui lòng nhập ngày chấm dứt' UNION ALL
    SELECT N'hr_err_terminated_date_required', N'zh-CN', N'请输入终止日期' UNION ALL
    SELECT N'hr_err_user_linked' AS ResourceKey, N'en-US' AS Culture, N'This account is already linked to another record' AS Value UNION ALL
    SELECT N'hr_err_user_linked', N'vi-VN', N'Tài khoản này đã gắn với hồ sơ khác' UNION ALL
    SELECT N'hr_err_user_linked', N'zh-CN', N'该账号已关联其他档案' UNION ALL
    SELECT N'hr_err_workdays_required' AS ResourceKey, N'en-US' AS Culture, N'Select at least one working day' AS Value UNION ALL
    SELECT N'hr_err_workdays_required', N'vi-VN', N'Chọn ít nhất 1 ngày làm việc' UNION ALL
    SELECT N'hr_err_workdays_required', N'zh-CN', N'请至少选择一个工作日' UNION ALL
    SELECT N'hr_expiring_within' AS ResourceKey, N'en-US' AS Culture, N'Expiring within' AS Value UNION ALL
    SELECT N'hr_expiring_within', N'vi-VN', N'Hết hạn trong' UNION ALL
    SELECT N'hr_expiring_within', N'zh-CN', N'到期范围' UNION ALL
    SELECT N'hr_export_excel' AS ResourceKey, N'en-US' AS Culture, N'Export Excel' AS Value UNION ALL
    SELECT N'hr_export_excel', N'vi-VN', N'Xuất Excel' UNION ALL
    SELECT N'hr_export_excel', N'zh-CN', N'导出 Excel' UNION ALL
    SELECT N'hr_file_name' AS ResourceKey, N'en-US' AS Culture, N'File name' AS Value UNION ALL
    SELECT N'hr_file_name', N'vi-VN', N'Tên file' UNION ALL
    SELECT N'hr_file_name', N'zh-CN', N'文件名' UNION ALL
    SELECT N'hr_file_size' AS ResourceKey, N'en-US' AS Culture, N'Size' AS Value UNION ALL
    SELECT N'hr_file_size', N'vi-VN', N'Dung lượng' UNION ALL
    SELECT N'hr_file_size', N'zh-CN', N'大小' UNION ALL
    SELECT N'hr_from_time' AS ResourceKey, N'en-US' AS Culture, N'From' AS Value UNION ALL
    SELECT N'hr_from_time', N'vi-VN', N'Từ giờ' UNION ALL
    SELECT N'hr_from_time', N'zh-CN', N'开始时间' UNION ALL
    SELECT N'hr_full_name' AS ResourceKey, N'en-US' AS Culture, N'Full name' AS Value UNION ALL
    SELECT N'hr_full_name', N'vi-VN', N'Họ và tên' UNION ALL
    SELECT N'hr_full_name', N'zh-CN', N'姓名' UNION ALL
    SELECT N'hr_gender' AS ResourceKey, N'en-US' AS Culture, N'Gender' AS Value UNION ALL
    SELECT N'hr_gender', N'vi-VN', N'Giới tính' UNION ALL
    SELECT N'hr_gender', N'zh-CN', N'性别' UNION ALL
    SELECT N'hr_gender_female' AS ResourceKey, N'en-US' AS Culture, N'Female' AS Value UNION ALL
    SELECT N'hr_gender_female', N'vi-VN', N'Nữ' UNION ALL
    SELECT N'hr_gender_female', N'zh-CN', N'女' UNION ALL
    SELECT N'hr_gender_male' AS ResourceKey, N'en-US' AS Culture, N'Male' AS Value UNION ALL
    SELECT N'hr_gender_male', N'vi-VN', N'Nam' UNION ALL
    SELECT N'hr_gender_male', N'zh-CN', N'男' UNION ALL
    SELECT N'hr_gender_other' AS ResourceKey, N'en-US' AS Culture, N'Other' AS Value UNION ALL
    SELECT N'hr_gender_other', N'vi-VN', N'Khác' UNION ALL
    SELECT N'hr_gender_other', N'zh-CN', N'其他' UNION ALL
    SELECT N'hr_holiday_add_fixed' AS ResourceKey, N'en-US' AS Culture, N'Add fixed holidays' AS Value UNION ALL
    SELECT N'hr_holiday_add_fixed', N'vi-VN', N'Thêm lễ cố định' UNION ALL
    SELECT N'hr_holiday_add_fixed', N'zh-CN', N'添加固定节日' UNION ALL
    SELECT N'hr_holiday_edit' AS ResourceKey, N'en-US' AS Culture, N'Edit holiday' AS Value UNION ALL
    SELECT N'hr_holiday_edit', N'vi-VN', N'Sửa ngày lễ' UNION ALL
    SELECT N'hr_holiday_edit', N'zh-CN', N'编辑节假日' UNION ALL
    SELECT N'hr_holiday_hint' AS ResourceKey, N'en-US' AS Culture, N'"Add fixed holidays" only adds 1/1, 30/4, 1/5 and 2/9. Lunar New Year, Hung Kings'' day and substitute days change every year — enter them from the official announcement.' AS Value UNION ALL
    SELECT N'hr_holiday_hint', N'vi-VN', N'"Thêm lễ cố định" chỉ thêm 1/1, 30/4, 1/5, 2/9. Tết Âm lịch, Giỗ Tổ và các ngày nghỉ bù thay đổi hằng năm — vui lòng nhập theo thông báo chính thức.' UNION ALL
    SELECT N'hr_holiday_hint', N'zh-CN', N'"添加固定节日"仅添加 1/1、30/4、1/5、2/9。春节、雄王祭和补休日每年不同，请按官方公告录入。' UNION ALL
    SELECT N'hr_holiday_name' AS ResourceKey, N'en-US' AS Culture, N'Holiday name' AS Value UNION ALL
    SELECT N'hr_holiday_name', N'vi-VN', N'Tên ngày lễ' UNION ALL
    SELECT N'hr_holiday_name', N'zh-CN', N'节日名称' UNION ALL
    SELECT N'hr_holiday_new' AS ResourceKey, N'en-US' AS Culture, N'New holiday' AS Value UNION ALL
    SELECT N'hr_holiday_new', N'vi-VN', N'Thêm ngày lễ' UNION ALL
    SELECT N'hr_holiday_new', N'zh-CN', N'新增节假日' UNION ALL
    SELECT N'hr_holiday_on_dayoff' AS ResourceKey, N'en-US' AS Culture, N'Falls on a day off — add a substitute day' AS Value UNION ALL
    SELECT N'hr_holiday_on_dayoff', N'vi-VN', N'Rơi vào ngày nghỉ — cần thêm ngày nghỉ bù' UNION ALL
    SELECT N'hr_holiday_on_dayoff', N'zh-CN', N'适逢休息日——需添加补休日' UNION ALL
    SELECT N'hr_holiday_weekday' AS ResourceKey, N'en-US' AS Culture, N'Weekday' AS Value UNION ALL
    SELECT N'hr_holiday_weekday', N'vi-VN', N'Thứ' UNION ALL
    SELECT N'hr_holiday_weekday', N'zh-CN', N'星期' UNION ALL
    SELECT N'hr_holidays_added' AS ResourceKey, N'en-US' AS Culture, N'{0} holidays added' AS Value UNION ALL
    SELECT N'hr_holidays_added', N'vi-VN', N'Đã thêm {0} ngày lễ' UNION ALL
    SELECT N'hr_holidays_added', N'zh-CN', N'已添加 {0} 个节假日' UNION ALL
    SELECT N'hr_id_card_issue_date' AS ResourceKey, N'en-US' AS Culture, N'Issue date' AS Value UNION ALL
    SELECT N'hr_id_card_issue_date', N'vi-VN', N'Ngày cấp' UNION ALL
    SELECT N'hr_id_card_issue_date', N'zh-CN', N'签发日期' UNION ALL
    SELECT N'hr_id_card_issue_place' AS ResourceKey, N'en-US' AS Culture, N'Issued by' AS Value UNION ALL
    SELECT N'hr_id_card_issue_place', N'vi-VN', N'Nơi cấp' UNION ALL
    SELECT N'hr_id_card_issue_place', N'zh-CN', N'签发机关' UNION ALL
    SELECT N'hr_id_card_no' AS ResourceKey, N'en-US' AS Culture, N'ID / Passport no.' AS Value UNION ALL
    SELECT N'hr_id_card_no', N'vi-VN', N'Số CCCD / Hộ chiếu' UNION ALL
    SELECT N'hr_id_card_no', N'zh-CN', N'身份证/护照号' UNION ALL
    SELECT N'hr_import_create' AS ResourceKey, N'en-US' AS Culture, N'Create records' AS Value UNION ALL
    SELECT N'hr_import_create', N'vi-VN', N'Tạo hồ sơ' UNION ALL
    SELECT N'hr_import_create', N'zh-CN', N'创建档案' UNION ALL
    SELECT N'hr_import_done' AS ResourceKey, N'en-US' AS Culture, N'{0} employee records created' AS Value UNION ALL
    SELECT N'hr_import_done', N'vi-VN', N'Đã tạo {0} hồ sơ nhân viên' UNION ALL
    SELECT N'hr_import_done', N'zh-CN', N'已创建 {0} 份员工档案' UNION ALL
    SELECT N'hr_import_from_users' AS ResourceKey, N'en-US' AS Culture, N'Create from accounts' AS Value UNION ALL
    SELECT N'hr_import_from_users', N'vi-VN', N'Tạo từ tài khoản' UNION ALL
    SELECT N'hr_import_from_users', N'zh-CN', N'从账号创建' UNION ALL
    SELECT N'hr_import_hint' AS ResourceKey, N'en-US' AS Culture, N'Pick login accounts that have no record yet. Employee code is generated and name, email, department and branch are copied from the account; status defaults to Active.' AS Value UNION ALL
    SELECT N'hr_import_hint', N'vi-VN', N'Chọn tài khoản đăng nhập chưa có hồ sơ. Hệ thống tự tạo mã NV, lấy họ tên, email, phòng ban, chi nhánh từ tài khoản; trạng thái mặc định Chính thức.' UNION ALL
    SELECT N'hr_import_hint', N'zh-CN', N'选择尚无档案的登录账号。系统自动生成员工编号并从账号复制姓名、邮箱、部门、分公司；默认状态为正式。' UNION ALL
    SELECT N'hr_import_no_users' AS ResourceKey, N'en-US' AS Culture, N'Every account already has a record' AS Value UNION ALL
    SELECT N'hr_import_no_users', N'vi-VN', N'Tất cả tài khoản đều đã có hồ sơ' UNION ALL
    SELECT N'hr_import_no_users', N'zh-CN', N'所有账号均已有档案' UNION ALL
    SELECT N'hr_import_none_selected' AS ResourceKey, N'en-US' AS Culture, N'No account selected' AS Value UNION ALL
    SELECT N'hr_import_none_selected', N'vi-VN', N'Chưa chọn tài khoản nào' UNION ALL
    SELECT N'hr_import_none_selected', N'zh-CN', N'未选择账号' UNION ALL
    SELECT N'hr_include_resigned' AS ResourceKey, N'en-US' AS Culture, N'Include resigned' AS Value UNION ALL
    SELECT N'hr_include_resigned', N'vi-VN', N'Hiện cả người đã nghỉ' UNION ALL
    SELECT N'hr_include_resigned', N'zh-CN', N'包含已离职' UNION ALL
    SELECT N'hr_indefinite' AS ResourceKey, N'en-US' AS Culture, N'Indefinite' AS Value UNION ALL
    SELECT N'hr_indefinite', N'vi-VN', N'Không thời hạn' UNION ALL
    SELECT N'hr_indefinite', N'zh-CN', N'无固定期限' UNION ALL
    SELECT N'hr_insurance_salary' AS ResourceKey, N'en-US' AS Culture, N'Insurance salary' AS Value UNION ALL
    SELECT N'hr_insurance_salary', N'vi-VN', N'Lương đóng BHXH' UNION ALL
    SELECT N'hr_insurance_salary', N'zh-CN', N'社保工资' UNION ALL
    SELECT N'hr_join_date' AS ResourceKey, N'en-US' AS Culture, N'Join date' AS Value UNION ALL
    SELECT N'hr_join_date', N'vi-VN', N'Ngày vào làm' UNION ALL
    SELECT N'hr_join_date', N'zh-CN', N'入职日期' UNION ALL
    SELECT N'hr_joined_this_month' AS ResourceKey, N'en-US' AS Culture, N'Joined this month' AS Value UNION ALL
    SELECT N'hr_joined_this_month', N'vi-VN', N'Vào làm trong tháng' UNION ALL
    SELECT N'hr_joined_this_month', N'zh-CN', N'本月入职' UNION ALL
    SELECT N'hr_leave' AS ResourceKey, N'en-US' AS Culture, N'Leave' AS Value UNION ALL
    SELECT N'hr_leave', N'vi-VN', N'Nghỉ phép' UNION ALL
    SELECT N'hr_leave', N'zh-CN', N'请假' UNION ALL
    SELECT N'hr_leave_amount' AS ResourceKey, N'en-US' AS Culture, N'Amount' AS Value UNION ALL
    SELECT N'hr_leave_amount', N'vi-VN', N'Số lượng' UNION ALL
    SELECT N'hr_leave_amount', N'zh-CN', N'数量' UNION ALL
    SELECT N'hr_leave_approve_confirm' AS ResourceKey, N'en-US' AS Culture, N'Approve the request of {0} ({1})?' AS Value UNION ALL
    SELECT N'hr_leave_approve_confirm', N'vi-VN', N'Duyệt đơn của {0} ({1})?' UNION ALL
    SELECT N'hr_leave_approve_confirm', N'zh-CN', N'批准 {0} 的申请（{1}）？' UNION ALL
    SELECT N'hr_leave_approved' AS ResourceKey, N'en-US' AS Culture, N'Approved' AS Value UNION ALL
    SELECT N'hr_leave_approved', N'vi-VN', N'Đã duyệt' UNION ALL
    SELECT N'hr_leave_approved', N'zh-CN', N'已批准' UNION ALL
    SELECT N'hr_leave_approver' AS ResourceKey, N'en-US' AS Culture, N'Approver' AS Value UNION ALL
    SELECT N'hr_leave_approver', N'vi-VN', N'Người duyệt' UNION ALL
    SELECT N'hr_leave_approver', N'zh-CN', N'审批人' UNION ALL
    SELECT N'hr_leave_available_after' AS ResourceKey, N'en-US' AS Culture, N'{0} annual days left after this request' AS Value UNION ALL
    SELECT N'hr_leave_available_after', N'vi-VN', N'phép năm còn {0} sau đơn này' UNION ALL
    SELECT N'hr_leave_available_after', N'zh-CN', N'本申请后年假剩余 {0} 天' UNION ALL
    SELECT N'hr_leave_balance' AS ResourceKey, N'en-US' AS Culture, N'Annual leave balance' AS Value UNION ALL
    SELECT N'hr_leave_balance', N'vi-VN', N'Quỹ phép năm' UNION ALL
    SELECT N'hr_leave_balance', N'zh-CN', N'年假额度' UNION ALL
    SELECT N'hr_leave_balance_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Standard, seniority, carried-over and used days per employee' AS Value UNION ALL
    SELECT N'hr_leave_balance_subtitle', N'vi-VN', N'Phép tiêu chuẩn, thâm niên, tồn năm trước và số đã dùng của từng nhân viên' UNION ALL
    SELECT N'hr_leave_balance_subtitle', N'zh-CN', N'每位员工的标准、工龄、结转及已用天数' UNION ALL
    SELECT N'hr_leave_calc_days' AS ResourceKey, N'en-US' AS Culture, N'{0} days' AS Value UNION ALL
    SELECT N'hr_leave_calc_days', N'vi-VN', N'{0} ngày' UNION ALL
    SELECT N'hr_leave_calc_days', N'zh-CN', N'{0} 天' UNION ALL
    SELECT N'hr_leave_calc_hours' AS ResourceKey, N'en-US' AS Culture, N'{0} hours' AS Value UNION ALL
    SELECT N'hr_leave_calc_hours', N'vi-VN', N'{0} giờ' UNION ALL
    SELECT N'hr_leave_calc_hours', N'zh-CN', N'{0} 小时'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Phần 3/5
;WITH src AS (
    SELECT N'hr_leave_cancel' AS ResourceKey, N'en-US' AS Culture, N'Cancel request' AS Value UNION ALL
    SELECT N'hr_leave_cancel', N'vi-VN', N'Hủy đơn' UNION ALL
    SELECT N'hr_leave_cancel', N'zh-CN', N'撤销申请' UNION ALL
    SELECT N'hr_leave_cancel_confirm' AS ResourceKey, N'en-US' AS Culture, N'Cancel the approved request of {0} ({1})? The days will be returned.' AS Value UNION ALL
    SELECT N'hr_leave_cancel_confirm', N'vi-VN', N'Hủy đơn đã duyệt của {0} ({1})? Ngày phép sẽ được hoàn lại.' UNION ALL
    SELECT N'hr_leave_cancel_confirm', N'zh-CN', N'撤销 {0} 已批准的申请（{1}）？天数将退回。' UNION ALL
    SELECT N'hr_leave_cancelled' AS ResourceKey, N'en-US' AS Culture, N'Request cancelled' AS Value UNION ALL
    SELECT N'hr_leave_cancelled', N'vi-VN', N'Đã hủy đơn' UNION ALL
    SELECT N'hr_leave_cancelled', N'zh-CN', N'已撤销申请' UNION ALL
    SELECT N'hr_leave_delete_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete this request?' AS Value UNION ALL
    SELECT N'hr_leave_delete_confirm', N'vi-VN', N'Xóa đơn nghỉ này?' UNION ALL
    SELECT N'hr_leave_delete_confirm', N'zh-CN', N'删除该申请？' UNION ALL
    SELECT N'hr_leave_duration' AS ResourceKey, N'en-US' AS Culture, N'Duration' AS Value UNION ALL
    SELECT N'hr_leave_duration', N'vi-VN', N'Hình thức' UNION ALL
    SELECT N'hr_leave_duration', N'zh-CN', N'时长' UNION ALL
    SELECT N'hr_leave_edit' AS ResourceKey, N'en-US' AS Culture, N'Edit request' AS Value UNION ALL
    SELECT N'hr_leave_edit', N'vi-VN', N'Sửa đơn nghỉ' UNION ALL
    SELECT N'hr_leave_edit', N'zh-CN', N'编辑申请' UNION ALL
    SELECT N'hr_leave_hr_approver_hint' AS ResourceKey, N'en-US' AS Culture, N'You can approve all requests (LeaveRequests - Approve permission)' AS Value UNION ALL
    SELECT N'hr_leave_hr_approver_hint', N'vi-VN', N'Bạn có quyền duyệt mọi đơn (quyền LeaveRequests - Approve)' UNION ALL
    SELECT N'hr_leave_hr_approver_hint', N'zh-CN', N'您可审批所有申请（LeaveRequests - Approve 权限）' UNION ALL
    SELECT N'hr_leave_hr_pool' AS ResourceKey, N'en-US' AS Culture, N'HR / admin' AS Value UNION ALL
    SELECT N'hr_leave_hr_pool', N'vi-VN', N'Nhân sự / quản trị' UNION ALL
    SELECT N'hr_leave_hr_pool', N'zh-CN', N'人事/管理员' UNION ALL
    SELECT N'hr_leave_new' AS ResourceKey, N'en-US' AS Culture, N'New request' AS Value UNION ALL
    SELECT N'hr_leave_new', N'vi-VN', N'Tạo đơn nghỉ' UNION ALL
    SELECT N'hr_leave_new', N'zh-CN', N'新建申请' UNION ALL
    SELECT N'hr_leave_no_profile' AS ResourceKey, N'en-US' AS Culture, N'Your account is not linked to an employee record, so there is no leave balance or line manager yet. Requests will go to HR.' AS Value UNION ALL
    SELECT N'hr_leave_no_profile', N'vi-VN', N'Tài khoản của bạn chưa gắn hồ sơ nhân viên nên chưa có quỹ phép và chưa xác định được quản lý duyệt. Đơn sẽ được gửi tới bộ phận nhân sự.' UNION ALL
    SELECT N'hr_leave_no_profile', N'zh-CN', N'您的账号尚未关联员工档案，暂无年假额度和直属上级，申请将发送给人事部门。' UNION ALL
    SELECT N'hr_leave_period' AS ResourceKey, N'en-US' AS Culture, N'Period' AS Value UNION ALL
    SELECT N'hr_leave_period', N'vi-VN', N'Thời gian' UNION ALL
    SELECT N'hr_leave_period', N'zh-CN', N'时间' UNION ALL
    SELECT N'hr_leave_reason' AS ResourceKey, N'en-US' AS Culture, N'Reason' AS Value UNION ALL
    SELECT N'hr_leave_reason', N'vi-VN', N'Lý do' UNION ALL
    SELECT N'hr_leave_reason', N'zh-CN', N'原因' UNION ALL
    SELECT N'hr_leave_reject_confirm' AS ResourceKey, N'en-US' AS Culture, N'Reject the request of {0} ({1})' AS Value UNION ALL
    SELECT N'hr_leave_reject_confirm', N'vi-VN', N'Từ chối đơn của {0} ({1})' UNION ALL
    SELECT N'hr_leave_reject_confirm', N'zh-CN', N'拒绝 {0} 的申请（{1}）' UNION ALL
    SELECT N'hr_leave_rejected' AS ResourceKey, N'en-US' AS Culture, N'Rejected' AS Value UNION ALL
    SELECT N'hr_leave_rejected', N'vi-VN', N'Đã từ chối' UNION ALL
    SELECT N'hr_leave_rejected', N'zh-CN', N'已拒绝' UNION ALL
    SELECT N'hr_leave_send' AS ResourceKey, N'en-US' AS Culture, N'Submit' AS Value UNION ALL
    SELECT N'hr_leave_send', N'vi-VN', N'Gửi đơn' UNION ALL
    SELECT N'hr_leave_send', N'zh-CN', N'提交' UNION ALL
    SELECT N'hr_leave_status_approved' AS ResourceKey, N'en-US' AS Culture, N'Approved' AS Value UNION ALL
    SELECT N'hr_leave_status_approved', N'vi-VN', N'Đã duyệt' UNION ALL
    SELECT N'hr_leave_status_approved', N'zh-CN', N'已批准' UNION ALL
    SELECT N'hr_leave_status_cancelled' AS ResourceKey, N'en-US' AS Culture, N'Cancelled' AS Value UNION ALL
    SELECT N'hr_leave_status_cancelled', N'vi-VN', N'Đã hủy' UNION ALL
    SELECT N'hr_leave_status_cancelled', N'zh-CN', N'已撤销' UNION ALL
    SELECT N'hr_leave_status_pending' AS ResourceKey, N'en-US' AS Culture, N'Pending' AS Value UNION ALL
    SELECT N'hr_leave_status_pending', N'vi-VN', N'Chờ duyệt' UNION ALL
    SELECT N'hr_leave_status_pending', N'zh-CN', N'待审批' UNION ALL
    SELECT N'hr_leave_status_rejected' AS ResourceKey, N'en-US' AS Culture, N'Rejected' AS Value UNION ALL
    SELECT N'hr_leave_status_rejected', N'vi-VN', N'Từ chối' UNION ALL
    SELECT N'hr_leave_status_rejected', N'zh-CN', N'已拒绝' UNION ALL
    SELECT N'hr_leave_submitted' AS ResourceKey, N'en-US' AS Culture, N'Request sent to HR' AS Value UNION ALL
    SELECT N'hr_leave_submitted', N'vi-VN', N'Đã gửi đơn tới bộ phận nhân sự' UNION ALL
    SELECT N'hr_leave_submitted', N'zh-CN', N'申请已发送给人事部门' UNION ALL
    SELECT N'hr_leave_submitted_manager' AS ResourceKey, N'en-US' AS Culture, N'Request sent to your line manager' AS Value UNION ALL
    SELECT N'hr_leave_submitted_manager', N'vi-VN', N'Đã gửi đơn tới quản lý trực tiếp' UNION ALL
    SELECT N'hr_leave_submitted_manager', N'zh-CN', N'申请已发送给直属上级' UNION ALL
    SELECT N'hr_leave_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Request leave, approve your team''s requests and track annual leave' AS Value UNION ALL
    SELECT N'hr_leave_subtitle', N'vi-VN', N'Đăng ký nghỉ, duyệt đơn của cấp dưới và theo dõi phép năm' UNION ALL
    SELECT N'hr_leave_subtitle', N'zh-CN', N'申请请假、审批下属申请并跟踪年假' UNION ALL
    SELECT N'hr_leave_tab_all' AS ResourceKey, N'en-US' AS Culture, N'All requests' AS Value UNION ALL
    SELECT N'hr_leave_tab_all', N'vi-VN', N'Tất cả đơn' UNION ALL
    SELECT N'hr_leave_tab_all', N'zh-CN', N'全部申请' UNION ALL
    SELECT N'hr_leave_tab_approve' AS ResourceKey, N'en-US' AS Culture, N'Awaiting my approval' AS Value UNION ALL
    SELECT N'hr_leave_tab_approve', N'vi-VN', N'Chờ tôi duyệt' UNION ALL
    SELECT N'hr_leave_tab_approve', N'zh-CN', N'待我审批' UNION ALL
    SELECT N'hr_leave_tab_my' AS ResourceKey, N'en-US' AS Culture, N'My requests' AS Value UNION ALL
    SELECT N'hr_leave_tab_my', N'vi-VN', N'Đơn của tôi' UNION ALL
    SELECT N'hr_leave_tab_my', N'zh-CN', N'我的申请' UNION ALL
    SELECT N'hr_leave_type' AS ResourceKey, N'en-US' AS Culture, N'Leave type' AS Value UNION ALL
    SELECT N'hr_leave_type', N'vi-VN', N'Loại nghỉ' UNION ALL
    SELECT N'hr_leave_type', N'zh-CN', N'假别' UNION ALL
    SELECT N'hr_leave_type_annual' AS ResourceKey, N'en-US' AS Culture, N'Annual leave' AS Value UNION ALL
    SELECT N'hr_leave_type_annual', N'vi-VN', N'Nghỉ phép năm' UNION ALL
    SELECT N'hr_leave_type_annual', N'zh-CN', N'年假' UNION ALL
    SELECT N'hr_leave_type_goout' AS ResourceKey, N'en-US' AS Culture, N'Going out' AS Value UNION ALL
    SELECT N'hr_leave_type_goout', N'vi-VN', N'Xin ra ngoài' UNION ALL
    SELECT N'hr_leave_type_goout', N'zh-CN', N'外出' UNION ALL
    SELECT N'hr_leave_type_personal' AS ResourceKey, N'en-US' AS Culture, N'Personal leave' AS Value UNION ALL
    SELECT N'hr_leave_type_personal', N'vi-VN', N'Nghỉ việc riêng' UNION ALL
    SELECT N'hr_leave_type_personal', N'zh-CN', N'事假' UNION ALL
    SELECT N'hr_leave_type_sick' AS ResourceKey, N'en-US' AS Culture, N'Sick leave' AS Value UNION ALL
    SELECT N'hr_leave_type_sick', N'vi-VN', N'Nghỉ ốm' UNION ALL
    SELECT N'hr_leave_type_sick', N'zh-CN', N'病假' UNION ALL
    SELECT N'hr_leave_type_unpaid' AS ResourceKey, N'en-US' AS Culture, N'Unpaid leave' AS Value UNION ALL
    SELECT N'hr_leave_type_unpaid', N'vi-VN', N'Nghỉ không lương' UNION ALL
    SELECT N'hr_leave_type_unpaid', N'zh-CN', N'无薪假' UNION ALL
    SELECT N'hr_manager' AS ResourceKey, N'en-US' AS Culture, N'Line manager' AS Value UNION ALL
    SELECT N'hr_manager', N'vi-VN', N'Quản lý trực tiếp' UNION ALL
    SELECT N'hr_manager', N'zh-CN', N'直属上级' UNION ALL
    SELECT N'hr_month' AS ResourceKey, N'en-US' AS Culture, N'Month' AS Value UNION ALL
    SELECT N'hr_month', N'vi-VN', N'Tháng' UNION ALL
    SELECT N'hr_month', N'zh-CN', N'月' UNION ALL
    SELECT N'hr_morning' AS ResourceKey, N'en-US' AS Culture, N'Morning' AS Value UNION ALL
    SELECT N'hr_morning', N'vi-VN', N'Buổi sáng' UNION ALL
    SELECT N'hr_morning', N'zh-CN', N'上午' UNION ALL
    SELECT N'hr_name' AS ResourceKey, N'en-US' AS Culture, N'Name' AS Value UNION ALL
    SELECT N'hr_name', N'vi-VN', N'Tên' UNION ALL
    SELECT N'hr_name', N'zh-CN', N'名称' UNION ALL
    SELECT N'hr_nav' AS ResourceKey, N'en-US' AS Culture, N'Human resources' AS Value UNION ALL
    SELECT N'hr_nav', N'vi-VN', N'Quản lý nhân sự' UNION ALL
    SELECT N'hr_nav', N'zh-CN', N'人力资源' UNION ALL
    SELECT N'hr_new_contract' AS ResourceKey, N'en-US' AS Culture, N'New contract' AS Value UNION ALL
    SELECT N'hr_new_contract', N'vi-VN', N'Thêm hợp đồng' UNION ALL
    SELECT N'hr_new_contract', N'zh-CN', N'新增合同' UNION ALL
    SELECT N'hr_new_department' AS ResourceKey, N'en-US' AS Culture, N'New department' AS Value UNION ALL
    SELECT N'hr_new_department', N'vi-VN', N'Thêm phòng ban' UNION ALL
    SELECT N'hr_new_department', N'zh-CN', N'新增部门' UNION ALL
    SELECT N'hr_new_employee' AS ResourceKey, N'en-US' AS Culture, N'New employee' AS Value UNION ALL
    SELECT N'hr_new_employee', N'vi-VN', N'Thêm nhân viên' UNION ALL
    SELECT N'hr_new_employee', N'zh-CN', N'新增员工' UNION ALL
    SELECT N'hr_new_position' AS ResourceKey, N'en-US' AS Culture, N'New position' AS Value UNION ALL
    SELECT N'hr_new_position', N'vi-VN', N'Thêm chức vụ' UNION ALL
    SELECT N'hr_new_position', N'zh-CN', N'新增职位' UNION ALL
    SELECT N'hr_new_value' AS ResourceKey, N'en-US' AS Culture, N'New value' AS Value UNION ALL
    SELECT N'hr_new_value', N'vi-VN', N'Giá trị mới' UNION ALL
    SELECT N'hr_new_value', N'zh-CN', N'新值' UNION ALL
    SELECT N'hr_no_items' AS ResourceKey, N'en-US' AS Culture, N'No data' AS Value UNION ALL
    SELECT N'hr_no_items', N'vi-VN', N'Không có dữ liệu' UNION ALL
    SELECT N'hr_no_items', N'zh-CN', N'暂无数据' UNION ALL
    SELECT N'hr_note' AS ResourceKey, N'en-US' AS Culture, N'Note' AS Value UNION ALL
    SELECT N'hr_note', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'hr_note', N'zh-CN', N'备注' UNION ALL
    SELECT N'hr_official_date' AS ResourceKey, N'en-US' AS Culture, N'Official date' AS Value UNION ALL
    SELECT N'hr_official_date', N'vi-VN', N'Ngày chính thức' UNION ALL
    SELECT N'hr_official_date', N'zh-CN', N'转正日期' UNION ALL
    SELECT N'hr_old_value' AS ResourceKey, N'en-US' AS Culture, N'Old value' AS Value UNION ALL
    SELECT N'hr_old_value', N'vi-VN', N'Giá trị cũ' UNION ALL
    SELECT N'hr_old_value', N'zh-CN', N'原值' UNION ALL
    SELECT N'hr_opt_promote_official' AS ResourceKey, N'en-US' AS Culture, N'Set the employee to "Active" from the contract start date' AS Value UNION ALL
    SELECT N'hr_opt_promote_official', N'vi-VN', N'Chuyển nhân viên sang "Chính thức" từ ngày bắt đầu hợp đồng' UNION ALL
    SELECT N'hr_opt_promote_official', N'zh-CN', N'自合同开始日起将员工设为"正式"' UNION ALL
    SELECT N'hr_opt_terminate_previous' AS ResourceKey, N'en-US' AS Culture, N'Terminate the employee''s other active contracts' AS Value UNION ALL
    SELECT N'hr_opt_terminate_previous', N'vi-VN', N'Chấm dứt các hợp đồng đang hiệu lực trước đó của nhân viên' UNION ALL
    SELECT N'hr_opt_terminate_previous', N'zh-CN', N'终止该员工其他有效合同' UNION ALL
    SELECT N'hr_org' AS ResourceKey, N'en-US' AS Culture, N'Departments & positions' AS Value UNION ALL
    SELECT N'hr_org', N'vi-VN', N'Phòng ban & Chức vụ' UNION ALL
    SELECT N'hr_org', N'zh-CN', N'部门与职位' UNION ALL
    SELECT N'hr_org_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Department and position lists used in employee records' AS Value UNION ALL
    SELECT N'hr_org_subtitle', N'vi-VN', N'Danh mục phòng ban và chức vụ dùng cho hồ sơ nhân viên' UNION ALL
    SELECT N'hr_org_subtitle', N'zh-CN', N'员工档案使用的部门和职位目录' UNION ALL
    SELECT N'hr_overdue_days' AS ResourceKey, N'en-US' AS Culture, N'{0} days overdue' AS Value UNION ALL
    SELECT N'hr_overdue_days', N'vi-VN', N'quá hạn {0} ngày' UNION ALL
    SELECT N'hr_overdue_days', N'zh-CN', N'已逾期 {0} 天' UNION ALL
    SELECT N'hr_pay_acc_advance' AS ResourceKey, N'en-US' AS Culture, N'Advance acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_advance', N'vi-VN', N'TK tạm ứng' UNION ALL
    SELECT N'hr_pay_acc_advance', N'zh-CN', N'预支科目' UNION ALL
    SELECT N'hr_pay_acc_expense' AS ResourceKey, N'en-US' AS Culture, N'Expense acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_expense', N'vi-VN', N'TK chi phí' UNION ALL
    SELECT N'hr_pay_acc_expense', N'zh-CN', N'费用科目' UNION ALL
    SELECT N'hr_pay_acc_health' AS ResourceKey, N'en-US' AS Culture, N'HI acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_health', N'vi-VN', N'TK BHYT' UNION ALL
    SELECT N'hr_pay_acc_health', N'zh-CN', N'医保科目' UNION ALL
    SELECT N'hr_pay_acc_other' AS ResourceKey, N'en-US' AS Culture, N'Other deduction acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_other', N'vi-VN', N'TK khấu trừ khác' UNION ALL
    SELECT N'hr_pay_acc_other', N'zh-CN', N'其他扣款科目' UNION ALL
    SELECT N'hr_pay_acc_payable' AS ResourceKey, N'en-US' AS Culture, N'Payable acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_payable', N'vi-VN', N'TK phải trả NLĐ' UNION ALL
    SELECT N'hr_pay_acc_payable', N'zh-CN', N'应付职工科目' UNION ALL
    SELECT N'hr_pay_acc_pit' AS ResourceKey, N'en-US' AS Culture, N'PIT acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_pit', N'vi-VN', N'TK thuế TNCN' UNION ALL
    SELECT N'hr_pay_acc_pit', N'zh-CN', N'个税科目' UNION ALL
    SELECT N'hr_pay_acc_social' AS ResourceKey, N'en-US' AS Culture, N'SI acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_social', N'vi-VN', N'TK BHXH' UNION ALL
    SELECT N'hr_pay_acc_social', N'zh-CN', N'社保科目' UNION ALL
    SELECT N'hr_pay_acc_unemployment' AS ResourceKey, N'en-US' AS Culture, N'UI acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_unemployment', N'vi-VN', N'TK BHTN' UNION ALL
    SELECT N'hr_pay_acc_unemployment', N'zh-CN', N'失业险科目' UNION ALL
    SELECT N'hr_pay_acc_union' AS ResourceKey, N'en-US' AS Culture, N'Union acc.' AS Value UNION ALL
    SELECT N'hr_pay_acc_union', N'vi-VN', N'TK KPCĐ' UNION ALL
    SELECT N'hr_pay_acc_union', N'zh-CN', N'工会科目' UNION ALL
    SELECT N'hr_pay_account' AS ResourceKey, N'en-US' AS Culture, N'Account' AS Value UNION ALL
    SELECT N'hr_pay_account', N'vi-VN', N'Tài khoản' UNION ALL
    SELECT N'hr_pay_account', N'zh-CN', N'科目' UNION ALL
    SELECT N'hr_pay_advance' AS ResourceKey, N'en-US' AS Culture, N'Advance' AS Value UNION ALL
    SELECT N'hr_pay_advance', N'vi-VN', N'Tạm ứng' UNION ALL
    SELECT N'hr_pay_advance', N'zh-CN', N'预支' UNION ALL
    SELECT N'hr_pay_allowance_amount' AS ResourceKey, N'en-US' AS Culture, N'Allowance by days' AS Value UNION ALL
    SELECT N'hr_pay_allowance_amount', N'vi-VN', N'Phụ cấp theo công' UNION ALL
    SELECT N'hr_pay_allowance_amount', N'zh-CN', N'按工日津贴' UNION ALL
    SELECT N'hr_pay_assessable' AS ResourceKey, N'en-US' AS Culture, N'Assessable income' AS Value UNION ALL
    SELECT N'hr_pay_assessable', N'vi-VN', N'Thu nhập tính thuế' UNION ALL
    SELECT N'hr_pay_assessable', N'zh-CN', N'应纳税所得' UNION ALL
    SELECT N'hr_pay_bonus' AS ResourceKey, N'en-US' AS Culture, N'Bonus' AS Value UNION ALL
    SELECT N'hr_pay_bonus', N'vi-VN', N'Thưởng' UNION ALL
    SELECT N'hr_pay_bonus', N'zh-CN', N'奖金' UNION ALL
    SELECT N'hr_pay_calculated' AS ResourceKey, N'en-US' AS Culture, N'Payroll calculated for {0} employees' AS Value UNION ALL
    SELECT N'hr_pay_calculated', N'vi-VN', N'Đã tính lương cho {0} nhân viên' UNION ALL
    SELECT N'hr_pay_calculated', N'zh-CN', N'已为 {0} 名员工计算工资' UNION ALL
    SELECT N'hr_pay_cap_multiplier' AS ResourceKey, N'en-US' AS Culture, N'Cap multiplier' AS Value UNION ALL
    SELECT N'hr_pay_cap_multiplier', N'vi-VN', N'Hệ số trần (lần)' UNION ALL
    SELECT N'hr_pay_cap_multiplier', N'zh-CN', N'上限倍数' UNION ALL
    SELECT N'hr_pay_co_health' AS ResourceKey, N'en-US' AS Culture, N'Health insurance (company)' AS Value UNION ALL
    SELECT N'hr_pay_co_health', N'vi-VN', N'BHYT (DN)' UNION ALL
    SELECT N'hr_pay_co_health', N'zh-CN', N'医保（公司）' UNION ALL
    SELECT N'hr_pay_co_social' AS ResourceKey, N'en-US' AS Culture, N'Social insurance (company)' AS Value UNION ALL
    SELECT N'hr_pay_co_social', N'vi-VN', N'BHXH (DN)' UNION ALL
    SELECT N'hr_pay_co_social', N'zh-CN', N'社保（公司）' UNION ALL
    SELECT N'hr_pay_co_unemployment' AS ResourceKey, N'en-US' AS Culture, N'Unemployment ins. (company)' AS Value UNION ALL
    SELECT N'hr_pay_co_unemployment', N'vi-VN', N'BHTN (DN)' UNION ALL
    SELECT N'hr_pay_co_unemployment', N'zh-CN', N'失业险（公司）' UNION ALL
    SELECT N'hr_pay_co_union' AS ResourceKey, N'en-US' AS Culture, N'Union fee (company)' AS Value UNION ALL
    SELECT N'hr_pay_co_union', N'vi-VN', N'KPCĐ (DN)' UNION ALL
    SELECT N'hr_pay_co_union', N'zh-CN', N'工会经费（公司）' UNION ALL
    SELECT N'hr_pay_company' AS ResourceKey, N'en-US' AS Culture, N'Company' AS Value UNION ALL
    SELECT N'hr_pay_company', N'vi-VN', N'Công ty (đối tượng)' UNION ALL
    SELECT N'hr_pay_company', N'zh-CN', N'公司' UNION ALL
    SELECT N'hr_pay_config_title' AS ResourceKey, N'en-US' AS Culture, N'Payroll settings and posting accounts' AS Value UNION ALL
    SELECT N'hr_pay_config_title', N'vi-VN', N'Cấu hình tính lương và tài khoản hạch toán' UNION ALL
    SELECT N'hr_pay_config_title', N'zh-CN', N'工资设置与过账科目' UNION ALL
    SELECT N'hr_pay_create_calc' AS ResourceKey, N'en-US' AS Culture, N'Create & calculate' AS Value UNION ALL
    SELECT N'hr_pay_create_calc', N'vi-VN', N'Tạo và tính' UNION ALL
    SELECT N'hr_pay_create_calc', N'zh-CN', N'创建并计算' UNION ALL
    SELECT N'hr_pay_credit' AS ResourceKey, N'en-US' AS Culture, N'Credit' AS Value UNION ALL
    SELECT N'hr_pay_credit', N'vi-VN', N'Có' UNION ALL
    SELECT N'hr_pay_credit', N'zh-CN', N'贷方' UNION ALL
    SELECT N'hr_pay_debit' AS ResourceKey, N'en-US' AS Culture, N'Debit' AS Value UNION ALL
    SELECT N'hr_pay_debit', N'vi-VN', N'Nợ' UNION ALL
    SELECT N'hr_pay_debit', N'zh-CN', N'借方' UNION ALL
    SELECT N'hr_pay_dependent_deduction' AS ResourceKey, N'en-US' AS Culture, N'Deduction per dependant' AS Value UNION ALL
    SELECT N'hr_pay_dependent_deduction', N'vi-VN', N'Giảm trừ mỗi người phụ thuộc' UNION ALL
    SELECT N'hr_pay_dependent_deduction', N'zh-CN', N'每名被抚养人扣除' UNION ALL
    SELECT N'hr_pay_dependents_short' AS ResourceKey, N'en-US' AS Culture, N'dep.' AS Value UNION ALL
    SELECT N'hr_pay_dependents_short', N'vi-VN', N'NPT' UNION ALL
    SELECT N'hr_pay_dependents_short', N'zh-CN', N'被抚养人' UNION ALL
    SELECT N'hr_pay_edit_slip' AS ResourceKey, N'en-US' AS Culture, N'Edit payslip' AS Value UNION ALL
    SELECT N'hr_pay_edit_slip', N'vi-VN', N'Sửa phiếu' UNION ALL
    SELECT N'hr_pay_edit_slip', N'zh-CN', N'编辑工资单' UNION ALL
    SELECT N'hr_pay_effective_from' AS ResourceKey, N'en-US' AS Culture, N'Effective from' AS Value UNION ALL
    SELECT N'hr_pay_effective_from', N'vi-VN', N'Hiệu lực từ' UNION ALL
    SELECT N'hr_pay_effective_from', N'zh-CN', N'生效日期' UNION ALL
    SELECT N'hr_pay_emp_health' AS ResourceKey, N'en-US' AS Culture, N'Health insurance (employee)' AS Value UNION ALL
    SELECT N'hr_pay_emp_health', N'vi-VN', N'BHYT (NLĐ)' UNION ALL
    SELECT N'hr_pay_emp_health', N'zh-CN', N'医保（个人）' UNION ALL
    SELECT N'hr_pay_emp_insurance' AS ResourceKey, N'en-US' AS Culture, N'Employee insurance' AS Value UNION ALL
    SELECT N'hr_pay_emp_insurance', N'vi-VN', N'BH trừ lương' UNION ALL
    SELECT N'hr_pay_emp_insurance', N'zh-CN', N'个人保险' UNION ALL
    SELECT N'hr_pay_emp_social' AS ResourceKey, N'en-US' AS Culture, N'Social insurance (employee)' AS Value UNION ALL
    SELECT N'hr_pay_emp_social', N'vi-VN', N'BHXH (NLĐ)' UNION ALL
    SELECT N'hr_pay_emp_social', N'zh-CN', N'社保（个人）' UNION ALL
    SELECT N'hr_pay_emp_unemployment' AS ResourceKey, N'en-US' AS Culture, N'Unemployment ins. (employee)' AS Value UNION ALL
    SELECT N'hr_pay_emp_unemployment', N'vi-VN', N'BHTN (NLĐ)' UNION ALL
    SELECT N'hr_pay_emp_unemployment', N'zh-CN', N'失业险（个人）' UNION ALL
    SELECT N'hr_pay_employer_cost' AS ResourceKey, N'en-US' AS Culture, N'Employer cost' AS Value UNION ALL
    SELECT N'hr_pay_employer_cost', N'vi-VN', N'Tổng chi phí DN' UNION ALL
    SELECT N'hr_pay_employer_cost', N'zh-CN', N'公司总成本' UNION ALL
    SELECT N'hr_pay_employer_part' AS ResourceKey, N'en-US' AS Culture, N'Company part: SI {0}, HI {1}, UI {2}, union {3} · Total cost {4}' AS Value UNION ALL
    SELECT N'hr_pay_employer_part', N'vi-VN', N'Phần DN đóng: BHXH {0}, BHYT {1}, BHTN {2}, KPCĐ {3} · Tổng chi phí {4}' UNION ALL
    SELECT N'hr_pay_employer_part', N'zh-CN', N'公司部分：社保 {0}，医保 {1}，失业险 {2}，工会 {3} · 总成本 {4}' UNION ALL
    SELECT N'hr_pay_err_company_required' AS ResourceKey, N'en-US' AS Culture, N'Please select a company' AS Value UNION ALL
    SELECT N'hr_pay_err_company_required', N'vi-VN', N'Vui lòng chọn công ty' UNION ALL
    SELECT N'hr_pay_err_company_required', N'zh-CN', N'请选择公司' UNION ALL
    SELECT N'hr_pay_err_empty' AS ResourceKey, N'en-US' AS Culture, N'The period has no payslips' AS Value UNION ALL
    SELECT N'hr_pay_err_empty', N'vi-VN', N'Kỳ lương chưa có phiếu lương' UNION ALL
    SELECT N'hr_pay_err_empty', N'zh-CN', N'该期间没有工资单' UNION ALL
    SELECT N'hr_pay_err_missing_accounts' AS ResourceKey, N'en-US' AS Culture, N'Accounts missing from the chart' AS Value UNION ALL
    SELECT N'hr_pay_err_missing_accounts', N'vi-VN', N'Chưa có tài khoản trong danh mục' UNION ALL
    SELECT N'hr_pay_err_missing_accounts', N'zh-CN', N'科目表中缺少科目' UNION ALL
    SELECT N'hr_pay_err_no_param' AS ResourceKey, N'en-US' AS Culture, N'No payroll parameters effective for this month' AS Value UNION ALL
    SELECT N'hr_pay_err_no_param', N'vi-VN', N'Chưa có tham số lương hiệu lực cho tháng này' UNION ALL
    SELECT N'hr_pay_err_no_param', N'zh-CN', N'本月无有效工资参数' UNION ALL
    SELECT N'hr_pay_err_not_draft' AS ResourceKey, N'en-US' AS Culture, N'Only allowed while the period is draft' AS Value UNION ALL
    SELECT N'hr_pay_err_not_draft', N'vi-VN', N'Chỉ thực hiện được khi kỳ lương ở trạng thái nháp' UNION ALL
    SELECT N'hr_pay_err_not_draft', N'zh-CN', N'仅草稿状态可操作' UNION ALL
    SELECT N'hr_pay_err_not_locked' AS ResourceKey, N'en-US' AS Culture, N'The period must be locked' AS Value UNION ALL
    SELECT N'hr_pay_err_not_locked', N'vi-VN', N'Kỳ lương phải ở trạng thái đã chốt' UNION ALL
    SELECT N'hr_pay_err_not_locked', N'zh-CN', N'期间必须为已锁定' UNION ALL
    SELECT N'hr_pay_err_not_posted' AS ResourceKey, N'en-US' AS Culture, N'The period is not posted' AS Value UNION ALL
    SELECT N'hr_pay_err_not_posted', N'vi-VN', N'Kỳ lương chưa hạch toán' UNION ALL
    SELECT N'hr_pay_err_not_posted', N'zh-CN', N'期间未过账' UNION ALL
    SELECT N'hr_pay_err_param_brackets' AS ResourceKey, N'en-US' AS Culture, N'Invalid tax brackets' AS Value UNION ALL
    SELECT N'hr_pay_err_param_brackets', N'vi-VN', N'Biểu thuế không hợp lệ' UNION ALL
    SELECT N'hr_pay_err_param_brackets', N'zh-CN', N'税率表无效' UNION ALL
    SELECT N'hr_pay_err_param_exists' AS ResourceKey, N'en-US' AS Culture, N'Parameters with this effective date already exist' AS Value UNION ALL
    SELECT N'hr_pay_err_param_exists', N'vi-VN', N'Đã có tham số cùng ngày hiệu lực' UNION ALL
    SELECT N'hr_pay_err_param_exists', N'zh-CN', N'已存在相同生效日期的参数' UNION ALL
    SELECT N'hr_pay_err_param_in_use' AS ResourceKey, N'en-US' AS Culture, N'Parameters are used by a pay period' AS Value UNION ALL
    SELECT N'hr_pay_err_param_in_use', N'vi-VN', N'Tham số đang được kỳ lương sử dụng' UNION ALL
    SELECT N'hr_pay_err_param_in_use', N'zh-CN', N'参数正被工资期间使用' UNION ALL
    SELECT N'hr_pay_err_param_wage' AS ResourceKey, N'en-US' AS Culture, N'Enter the reference wage and region I minimum wage' AS Value UNION ALL
    SELECT N'hr_pay_err_param_wage', N'vi-VN', N'Nhập mức tham chiếu và lương tối thiểu vùng I' UNION ALL
    SELECT N'hr_pay_err_param_wage', N'zh-CN', N'请输入参考工资和一类地区最低工资' UNION ALL
    SELECT N'hr_pay_err_period_exists' AS ResourceKey, N'en-US' AS Culture, N'A pay period for this month already exists' AS Value UNION ALL
    SELECT N'hr_pay_err_period_exists', N'vi-VN', N'Kỳ lương tháng này đã tồn tại' UNION ALL
    SELECT N'hr_pay_err_period_exists', N'zh-CN', N'该月工资期间已存在' UNION ALL
    SELECT N'hr_pay_err_region' AS ResourceKey, N'en-US' AS Culture, N'Region must be 1–4' AS Value UNION ALL
    SELECT N'hr_pay_err_region', N'vi-VN', N'Vùng phải từ 1 đến 4' UNION ALL
    SELECT N'hr_pay_err_region', N'zh-CN', N'地区须为 1–4' UNION ALL
    SELECT N'hr_pay_err_transaction_type' AS ResourceKey, N'en-US' AS Culture, N'Please select a transaction type' AS Value UNION ALL
    SELECT N'hr_pay_err_transaction_type', N'vi-VN', N'Vui lòng chọn loại nghiệp vụ' UNION ALL
    SELECT N'hr_pay_err_transaction_type', N'zh-CN', N'请选择业务类型' UNION ALL
    SELECT N'hr_pay_err_unbalanced' AS ResourceKey, N'en-US' AS Culture, N'Entries are not balanced' AS Value UNION ALL
    SELECT N'hr_pay_err_unbalanced', N'vi-VN', N'Bút toán không cân' UNION ALL
    SELECT N'hr_pay_err_unbalanced', N'zh-CN', N'分录不平衡'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Phần 4/5
;WITH src AS (
    SELECT N'hr_pay_err_voucher_posted' AS ResourceKey, N'en-US' AS Culture, N'The voucher is already posted — cancel it in accounting first' AS Value UNION ALL
    SELECT N'hr_pay_err_voucher_posted', N'vi-VN', N'Chứng từ đã được ghi sổ/duyệt — hủy trong kế toán trước' UNION ALL
    SELECT N'hr_pay_err_voucher_posted', N'zh-CN', N'凭证已记账——请先在会计中撤销' UNION ALL
    SELECT N'hr_pay_family_deduction' AS ResourceKey, N'en-US' AS Culture, N'Family deduction' AS Value UNION ALL
    SELECT N'hr_pay_family_deduction', N'vi-VN', N'Giảm trừ gia cảnh' UNION ALL
    SELECT N'hr_pay_family_deduction', N'zh-CN', N'家庭扣除' UNION ALL
    SELECT N'hr_pay_flat_rate' AS ResourceKey, N'en-US' AS Culture, N'Flat withholding rate (%)' AS Value UNION ALL
    SELECT N'hr_pay_flat_rate', N'vi-VN', N'Thuế suất khấu trừ (%)' UNION ALL
    SELECT N'hr_pay_flat_rate', N'zh-CN', N'扣缴税率（%）' UNION ALL
    SELECT N'hr_pay_flat_threshold' AS ResourceKey, N'en-US' AS Culture, N'Withholding threshold' AS Value UNION ALL
    SELECT N'hr_pay_flat_threshold', N'vi-VN', N'Ngưỡng khấu trừ' UNION ALL
    SELECT N'hr_pay_flat_threshold', N'zh-CN', N'扣缴起点' UNION ALL
    SELECT N'hr_pay_gross' AS ResourceKey, N'en-US' AS Culture, N'Gross income' AS Value UNION ALL
    SELECT N'hr_pay_gross', N'vi-VN', N'Tổng thu nhập' UNION ALL
    SELECT N'hr_pay_gross', N'zh-CN', N'总收入' UNION ALL
    SELECT N'hr_pay_insurance_base' AS ResourceKey, N'en-US' AS Culture, N'Insurance base' AS Value UNION ALL
    SELECT N'hr_pay_insurance_base', N'vi-VN', N'Lương đóng BH' UNION ALL
    SELECT N'hr_pay_insurance_base', N'zh-CN', N'缴保基数' UNION ALL
    SELECT N'hr_pay_line_desc' AS ResourceKey, N'en-US' AS Culture, N'Description' AS Value UNION ALL
    SELECT N'hr_pay_line_desc', N'vi-VN', N'Diễn giải' UNION ALL
    SELECT N'hr_pay_line_desc', N'zh-CN', N'摘要' UNION ALL
    SELECT N'hr_pay_lock' AS ResourceKey, N'en-US' AS Culture, N'Lock' AS Value UNION ALL
    SELECT N'hr_pay_lock', N'vi-VN', N'Chốt lương' UNION ALL
    SELECT N'hr_pay_lock', N'zh-CN', N'锁定' UNION ALL
    SELECT N'hr_pay_lock_confirm' AS ResourceKey, N'en-US' AS Culture, N'Lock the payroll? Payslips can no longer be edited and employees can see their own payslip.' AS Value UNION ALL
    SELECT N'hr_pay_lock_confirm', N'vi-VN', N'Chốt bảng lương? Sau khi chốt không sửa được phiếu, nhân viên xem được phiếu lương của mình.' UNION ALL
    SELECT N'hr_pay_lock_confirm', N'zh-CN', N'锁定工资表？锁定后不能再修改，员工可查看自己的工资单。' UNION ALL
    SELECT N'hr_pay_locked' AS ResourceKey, N'en-US' AS Culture, N'Payroll locked' AS Value UNION ALL
    SELECT N'hr_pay_locked', N'vi-VN', N'Đã chốt bảng lương' UNION ALL
    SELECT N'hr_pay_locked', N'zh-CN', N'工资表已锁定' UNION ALL
    SELECT N'hr_pay_min_wage_1' AS ResourceKey, N'en-US' AS Culture, N'Min wage region I' AS Value UNION ALL
    SELECT N'hr_pay_min_wage_1', N'vi-VN', N'Lương tối thiểu vùng I' UNION ALL
    SELECT N'hr_pay_min_wage_1', N'zh-CN', N'一类地区最低工资' UNION ALL
    SELECT N'hr_pay_min_wage_2' AS ResourceKey, N'en-US' AS Culture, N'Min wage region II' AS Value UNION ALL
    SELECT N'hr_pay_min_wage_2', N'vi-VN', N'Lương tối thiểu vùng II' UNION ALL
    SELECT N'hr_pay_min_wage_2', N'zh-CN', N'二类地区最低工资' UNION ALL
    SELECT N'hr_pay_min_wage_3' AS ResourceKey, N'en-US' AS Culture, N'Min wage region III' AS Value UNION ALL
    SELECT N'hr_pay_min_wage_3', N'vi-VN', N'Lương tối thiểu vùng III' UNION ALL
    SELECT N'hr_pay_min_wage_3', N'zh-CN', N'三类地区最低工资' UNION ALL
    SELECT N'hr_pay_min_wage_4' AS ResourceKey, N'en-US' AS Culture, N'Min wage region IV' AS Value UNION ALL
    SELECT N'hr_pay_min_wage_4', N'vi-VN', N'Lương tối thiểu vùng IV' UNION ALL
    SELECT N'hr_pay_min_wage_4', N'zh-CN', N'四类地区最低工资' UNION ALL
    SELECT N'hr_pay_missing_accounts_hint' AS ResourceKey, N'en-US' AS Culture, N'Accounts not in the chart of accounts: {0}. Add them in 1.12 or change the codes in Parameters & accounts.' AS Value UNION ALL
    SELECT N'hr_pay_missing_accounts_hint', N'vi-VN', N'Chưa có tài khoản trong danh mục: {0}. Thêm tại 1.12 Danh mục tài khoản hoặc đổi mã ở tab Tham số & tài khoản.' UNION ALL
    SELECT N'hr_pay_missing_accounts_hint', N'zh-CN', N'科目表中缺少科目：{0}。请在 1.12 添加或在参数与科目中修改。' UNION ALL
    SELECT N'hr_pay_net' AS ResourceKey, N'en-US' AS Culture, N'Net pay' AS Value UNION ALL
    SELECT N'hr_pay_net', N'vi-VN', N'Thực lĩnh' UNION ALL
    SELECT N'hr_pay_net', N'zh-CN', N'实发工资' UNION ALL
    SELECT N'hr_pay_new_period' AS ResourceKey, N'en-US' AS Culture, N'New pay period' AS Value UNION ALL
    SELECT N'hr_pay_new_period', N'vi-VN', N'Tạo kỳ lương' UNION ALL
    SELECT N'hr_pay_new_period', N'zh-CN', N'新建工资期间' UNION ALL
    SELECT N'hr_pay_new_period_hint' AS ResourceKey, N'en-US' AS Culture, N'Active contracts, the monthly timesheet and legal parameters are used to calculate immediately.' AS Value UNION ALL
    SELECT N'hr_pay_new_period_hint', N'vi-VN', N'Hệ thống sẽ lấy hợp đồng đang hiệu lực, bảng công tháng và tham số pháp lý để tính ngay.' UNION ALL
    SELECT N'hr_pay_new_period_hint', N'zh-CN', N'系统将使用有效合同、月度考勤和法定参数立即计算。' UNION ALL
    SELECT N'hr_pay_no_insurance_days' AS ResourceKey, N'en-US' AS Culture, N'Unpaid days to skip insurance' AS Value UNION ALL
    SELECT N'hr_pay_no_insurance_days', N'vi-VN', N'Số ngày không hưởng lương để không đóng BH' UNION ALL
    SELECT N'hr_pay_no_insurance_days', N'zh-CN', N'免缴保险的无薪天数' UNION ALL
    SELECT N'hr_pay_no_insurance_days_hint' AS ResourceKey, N'en-US' AS Culture, N'Social Insurance Law: 14 working days or more' AS Value UNION ALL
    SELECT N'hr_pay_no_insurance_days_hint', N'vi-VN', N'Luật BHXH: từ 14 ngày làm việc trở lên' UNION ALL
    SELECT N'hr_pay_no_insurance_days_hint', N'zh-CN', N'社保法：14 个工作日及以上' UNION ALL
    SELECT N'hr_pay_no_my_slips' AS ResourceKey, N'en-US' AS Culture, N'No locked payslips yet' AS Value UNION ALL
    SELECT N'hr_pay_no_my_slips', N'vi-VN', N'Chưa có phiếu lương đã chốt' UNION ALL
    SELECT N'hr_pay_no_my_slips', N'zh-CN', N'暂无已锁定的工资单' UNION ALL
    SELECT N'hr_pay_no_period' AS ResourceKey, N'en-US' AS Culture, N'No pay period this year — click "New pay period"' AS Value UNION ALL
    SELECT N'hr_pay_no_period', N'vi-VN', N'Chưa có kỳ lương trong năm này — bấm "Tạo kỳ lương"' UNION ALL
    SELECT N'hr_pay_no_period', N'zh-CN', N'本年尚无工资期间——点击"新建工资期间"' UNION ALL
    SELECT N'hr_pay_non_taxable' AS ResourceKey, N'en-US' AS Culture, N'Non-taxable income' AS Value UNION ALL
    SELECT N'hr_pay_non_taxable', N'vi-VN', N'Thu nhập không chịu thuế' UNION ALL
    SELECT N'hr_pay_non_taxable', N'zh-CN', N'免税收入' UNION ALL
    SELECT N'hr_pay_non_taxable_hint' AS ResourceKey, N'en-US' AS Culture, N'E.g. exempt meal allowance or exempt overtime premium' AS Value UNION ALL
    SELECT N'hr_pay_non_taxable_hint', N'vi-VN', N'Vd: phần phụ cấp ăn ca, phần lương làm thêm được miễn thuế' UNION ALL
    SELECT N'hr_pay_non_taxable_hint', N'zh-CN', N'如：免税餐补、免税加班部分' UNION ALL
    SELECT N'hr_pay_other_deduction' AS ResourceKey, N'en-US' AS Culture, N'Other deductions' AS Value UNION ALL
    SELECT N'hr_pay_other_deduction', N'vi-VN', N'Khấu trừ khác' UNION ALL
    SELECT N'hr_pay_other_deduction', N'zh-CN', N'其他扣款' UNION ALL
    SELECT N'hr_pay_other_income' AS ResourceKey, N'en-US' AS Culture, N'Other income' AS Value UNION ALL
    SELECT N'hr_pay_other_income', N'vi-VN', N'Thu nhập khác' UNION ALL
    SELECT N'hr_pay_other_income', N'zh-CN', N'其他收入' UNION ALL
    SELECT N'hr_pay_overtime' AS ResourceKey, N'en-US' AS Culture, N'Overtime' AS Value UNION ALL
    SELECT N'hr_pay_overtime', N'vi-VN', N'Làm thêm giờ' UNION ALL
    SELECT N'hr_pay_overtime', N'zh-CN', N'加班费' UNION ALL
    SELECT N'hr_pay_paid_days' AS ResourceKey, N'en-US' AS Culture, N'Paid days' AS Value UNION ALL
    SELECT N'hr_pay_paid_days', N'vi-VN', N'Công hưởng lương' UNION ALL
    SELECT N'hr_pay_paid_days', N'zh-CN', N'带薪工日' UNION ALL
    SELECT N'hr_pay_paid_days_hint' AS ResourceKey, N'en-US' AS Culture, N'Empty = from timesheet ({0} / {1})' AS Value UNION ALL
    SELECT N'hr_pay_paid_days_hint', N'vi-VN', N'Để trống = theo bảng công ({0} / {1})' UNION ALL
    SELECT N'hr_pay_paid_days_hint', N'zh-CN', N'留空 = 按考勤（{0} / {1}）' UNION ALL
    SELECT N'hr_pay_paid_days_override' AS ResourceKey, N'en-US' AS Culture, N'Paid days (override)' AS Value UNION ALL
    SELECT N'hr_pay_paid_days_override', N'vi-VN', N'Công hưởng lương (nhập tay)' UNION ALL
    SELECT N'hr_pay_paid_days_override', N'zh-CN', N'带薪工日（手动）' UNION ALL
    SELECT N'hr_pay_param_edit' AS ResourceKey, N'en-US' AS Culture, N'Edit parameters' AS Value UNION ALL
    SELECT N'hr_pay_param_edit', N'vi-VN', N'Sửa tham số' UNION ALL
    SELECT N'hr_pay_param_edit', N'zh-CN', N'编辑参数' UNION ALL
    SELECT N'hr_pay_param_new' AS ResourceKey, N'en-US' AS Culture, N'New parameters' AS Value UNION ALL
    SELECT N'hr_pay_param_new', N'vi-VN', N'Thêm tham số' UNION ALL
    SELECT N'hr_pay_param_new', N'zh-CN', N'新增参数' UNION ALL
    SELECT N'hr_pay_params_hint' AS ResourceKey, N'en-US' AS Culture, N'A period uses the latest row effective on or before the 1st of the month. When the state changes the base wage, minimum wage, family deduction or tax brackets, add a new row instead of editing old ones.' AS Value UNION ALL
    SELECT N'hr_pay_params_hint', N'vi-VN', N'Kỳ lương dùng dòng có ngày hiệu lực gần nhất không sau ngày 1 của tháng. Khi Nhà nước thay đổi mức lương cơ sở, lương tối thiểu vùng, giảm trừ gia cảnh hoặc biểu thuế, thêm dòng mới thay vì sửa dòng cũ.' UNION ALL
    SELECT N'hr_pay_params_hint', N'zh-CN', N'期间使用不晚于当月 1 日的最新参数。国家调整时请新增一行，而不要修改旧行。' UNION ALL
    SELECT N'hr_pay_params_title' AS ResourceKey, N'en-US' AS Culture, N'Legal parameters by effective date' AS Value UNION ALL
    SELECT N'hr_pay_params_title', N'vi-VN', N'Tham số pháp lý theo thời điểm hiệu lực' UNION ALL
    SELECT N'hr_pay_params_title', N'zh-CN', N'按生效日期的法定参数' UNION ALL
    SELECT N'hr_pay_payslip' AS ResourceKey, N'en-US' AS Culture, N'Payslip' AS Value UNION ALL
    SELECT N'hr_pay_payslip', N'vi-VN', N'Phiếu lương' UNION ALL
    SELECT N'hr_pay_payslip', N'zh-CN', N'工资单' UNION ALL
    SELECT N'hr_pay_period' AS ResourceKey, N'en-US' AS Culture, N'Pay period' AS Value UNION ALL
    SELECT N'hr_pay_period', N'vi-VN', N'Kỳ lương' UNION ALL
    SELECT N'hr_pay_period', N'zh-CN', N'工资期间' UNION ALL
    SELECT N'hr_pay_period_created' AS ResourceKey, N'en-US' AS Culture, N'Pay period created and calculated' AS Value UNION ALL
    SELECT N'hr_pay_period_created', N'vi-VN', N'Đã tạo và tính kỳ lương' UNION ALL
    SELECT N'hr_pay_period_created', N'zh-CN', N'已创建并计算工资期间' UNION ALL
    SELECT N'hr_pay_pit' AS ResourceKey, N'en-US' AS Culture, N'PIT' AS Value UNION ALL
    SELECT N'hr_pay_pit', N'vi-VN', N'Thuế TNCN' UNION ALL
    SELECT N'hr_pay_pit', N'zh-CN', N'个人所得税' UNION ALL
    SELECT N'hr_pay_pit_params' AS ResourceKey, N'en-US' AS Culture, N'Personal income tax' AS Value UNION ALL
    SELECT N'hr_pay_pit_params', N'vi-VN', N'Thuế thu nhập cá nhân' UNION ALL
    SELECT N'hr_pay_pit_params', N'zh-CN', N'个人所得税' UNION ALL
    SELECT N'hr_pay_post' AS ResourceKey, N'en-US' AS Culture, N'Post to accounting' AS Value UNION ALL
    SELECT N'hr_pay_post', N'vi-VN', N'Hạch toán' UNION ALL
    SELECT N'hr_pay_post', N'zh-CN', N'过账' UNION ALL
    SELECT N'hr_pay_post_hint' AS ResourceKey, N'en-US' AS Culture, N'A DRAFT accounting voucher is created. The accountant reviews and posts it in the voucher screen.' AS Value UNION ALL
    SELECT N'hr_pay_post_hint', N'vi-VN', N'Hệ thống tạo 1 chứng từ kế toán ở trạng thái NHÁP. Kế toán kiểm tra và ghi sổ tại màn hình chứng từ kế toán.' UNION ALL
    SELECT N'hr_pay_post_hint', N'zh-CN', N'系统将生成一张草稿会计凭证，由会计在凭证界面审核并记账。' UNION ALL
    SELECT N'hr_pay_posted' AS ResourceKey, N'en-US' AS Culture, N'Draft voucher {0} created' AS Value UNION ALL
    SELECT N'hr_pay_posted', N'vi-VN', N'Đã tạo chứng từ nháp {0}' UNION ALL
    SELECT N'hr_pay_posted', N'zh-CN', N'已生成草稿凭证 {0}' UNION ALL
    SELECT N'hr_pay_rates_employee' AS ResourceKey, N'en-US' AS Culture, N'Employee rates' AS Value UNION ALL
    SELECT N'hr_pay_rates_employee', N'vi-VN', N'Tỷ lệ người lao động đóng' UNION ALL
    SELECT N'hr_pay_rates_employee', N'zh-CN', N'个人缴费比例' UNION ALL
    SELECT N'hr_pay_rates_employer' AS ResourceKey, N'en-US' AS Culture, N'Employer rates' AS Value UNION ALL
    SELECT N'hr_pay_rates_employer', N'vi-VN', N'Tỷ lệ doanh nghiệp đóng' UNION ALL
    SELECT N'hr_pay_rates_employer', N'zh-CN', N'公司缴费比例' UNION ALL
    SELECT N'hr_pay_recalc' AS ResourceKey, N'en-US' AS Culture, N'Recalculate' AS Value UNION ALL
    SELECT N'hr_pay_recalc', N'vi-VN', N'Tính lại' UNION ALL
    SELECT N'hr_pay_recalc', N'zh-CN', N'重新计算' UNION ALL
    SELECT N'hr_pay_recalc_confirm' AS ResourceKey, N'en-US' AS Culture, N'Recalculate the whole period from current contracts, timesheet and parameters? Manual inputs (overtime, bonus, advances, deductions, overridden days, notes) are kept.' AS Value UNION ALL
    SELECT N'hr_pay_recalc_confirm', N'vi-VN', N'Tính lại toàn bộ kỳ theo hợp đồng, bảng công và tham số hiện tại? Các khoản nhập tay (làm thêm, thưởng, tạm ứng, khấu trừ, công nhập tay, ghi chú) được giữ nguyên.' UNION ALL
    SELECT N'hr_pay_recalc_confirm', N'zh-CN', N'按当前合同、考勤和参数重新计算整个期间？手动录入项（加班、奖金、预支、扣款、手动工日、备注）保持不变。' UNION ALL
    SELECT N'hr_pay_reference_wage' AS ResourceKey, N'en-US' AS Culture, N'Reference / base wage' AS Value UNION ALL
    SELECT N'hr_pay_reference_wage', N'vi-VN', N'Mức tham chiếu / lương cơ sở' UNION ALL
    SELECT N'hr_pay_reference_wage', N'zh-CN', N'参考工资/基本工资' UNION ALL
    SELECT N'hr_pay_region' AS ResourceKey, N'en-US' AS Culture, N'Minimum wage region' AS Value UNION ALL
    SELECT N'hr_pay_region', N'vi-VN', N'Vùng lương tối thiểu' UNION ALL
    SELECT N'hr_pay_region', N'zh-CN', N'最低工资地区' UNION ALL
    SELECT N'hr_pay_region_n' AS ResourceKey, N'en-US' AS Culture, N'Region {0}' AS Value UNION ALL
    SELECT N'hr_pay_region_n', N'vi-VN', N'Vùng {0}' UNION ALL
    SELECT N'hr_pay_region_n', N'zh-CN', N'{0} 类地区' UNION ALL
    SELECT N'hr_pay_salary_by_days' AS ResourceKey, N'en-US' AS Culture, N'Salary by days' AS Value UNION ALL
    SELECT N'hr_pay_salary_by_days', N'vi-VN', N'Lương theo công' UNION ALL
    SELECT N'hr_pay_salary_by_days', N'zh-CN', N'按工日工资' UNION ALL
    SELECT N'hr_pay_save_recalc' AS ResourceKey, N'en-US' AS Culture, N'Save & recalculate' AS Value UNION ALL
    SELECT N'hr_pay_save_recalc', N'vi-VN', N'Lưu và tính lại' UNION ALL
    SELECT N'hr_pay_save_recalc', N'zh-CN', N'保存并重新计算' UNION ALL
    SELECT N'hr_pay_sec_days' AS ResourceKey, N'en-US' AS Culture, N'Working days' AS Value UNION ALL
    SELECT N'hr_pay_sec_days', N'vi-VN', N'Ngày công' UNION ALL
    SELECT N'hr_pay_sec_days', N'zh-CN', N'工日' UNION ALL
    SELECT N'hr_pay_sec_deduction' AS ResourceKey, N'en-US' AS Culture, N'Deductions' AS Value UNION ALL
    SELECT N'hr_pay_sec_deduction', N'vi-VN', N'Các khoản khấu trừ' UNION ALL
    SELECT N'hr_pay_sec_deduction', N'zh-CN', N'扣除项' UNION ALL
    SELECT N'hr_pay_sec_income' AS ResourceKey, N'en-US' AS Culture, N'Income' AS Value UNION ALL
    SELECT N'hr_pay_sec_income', N'vi-VN', N'Thu nhập' UNION ALL
    SELECT N'hr_pay_sec_income', N'zh-CN', N'收入' UNION ALL
    SELECT N'hr_pay_self_deduction' AS ResourceKey, N'en-US' AS Culture, N'Self deduction' AS Value UNION ALL
    SELECT N'hr_pay_self_deduction', N'vi-VN', N'Giảm trừ bản thân' UNION ALL
    SELECT N'hr_pay_self_deduction', N'zh-CN', N'本人扣除' UNION ALL
    SELECT N'hr_pay_status_draft' AS ResourceKey, N'en-US' AS Culture, N'Draft' AS Value UNION ALL
    SELECT N'hr_pay_status_draft', N'vi-VN', N'Nháp' UNION ALL
    SELECT N'hr_pay_status_draft', N'zh-CN', N'草稿' UNION ALL
    SELECT N'hr_pay_status_locked' AS ResourceKey, N'en-US' AS Culture, N'Locked' AS Value UNION ALL
    SELECT N'hr_pay_status_locked', N'vi-VN', N'Đã chốt' UNION ALL
    SELECT N'hr_pay_status_locked', N'zh-CN', N'已锁定' UNION ALL
    SELECT N'hr_pay_status_posted' AS ResourceKey, N'en-US' AS Culture, N'Posted' AS Value UNION ALL
    SELECT N'hr_pay_status_posted', N'vi-VN', N'Đã hạch toán' UNION ALL
    SELECT N'hr_pay_status_posted', N'zh-CN', N'已过账' UNION ALL
    SELECT N'hr_pay_tab_my' AS ResourceKey, N'en-US' AS Culture, N'My payslips' AS Value UNION ALL
    SELECT N'hr_pay_tab_my', N'vi-VN', N'Phiếu lương của tôi' UNION ALL
    SELECT N'hr_pay_tab_my', N'zh-CN', N'我的工资单' UNION ALL
    SELECT N'hr_pay_tab_params' AS ResourceKey, N'en-US' AS Culture, N'Parameters & accounts' AS Value UNION ALL
    SELECT N'hr_pay_tab_params', N'vi-VN', N'Tham số & tài khoản' UNION ALL
    SELECT N'hr_pay_tab_params', N'zh-CN', N'参数与科目' UNION ALL
    SELECT N'hr_pay_tab_periods' AS ResourceKey, N'en-US' AS Culture, N'Pay periods' AS Value UNION ALL
    SELECT N'hr_pay_tab_periods', N'vi-VN', N'Kỳ lương' UNION ALL
    SELECT N'hr_pay_tab_periods', N'zh-CN', N'工资期间' UNION ALL
    SELECT N'hr_pay_tax_brackets' AS ResourceKey, N'en-US' AS Culture, N'Progressive brackets (monthly)' AS Value UNION ALL
    SELECT N'hr_pay_tax_brackets', N'vi-VN', N'Biểu thuế lũy tiến (tháng)' UNION ALL
    SELECT N'hr_pay_tax_brackets', N'zh-CN', N'累进税率表（月）' UNION ALL
    SELECT N'hr_pay_tax_brackets_hint' AS ResourceKey, N'en-US' AS Culture, N'Format threshold:rate; threshold 0 = unlimited. E.g. 10000000:5;30000000:10;60000000:20;100000000:30;0:35' AS Value UNION ALL
    SELECT N'hr_pay_tax_brackets_hint', N'vi-VN', N'Dạng ngưỡng:thuế suất; ngưỡng 0 = không giới hạn. Vd 10000000:5;30000000:10;60000000:20;100000000:30;0:35' UNION ALL
    SELECT N'hr_pay_tax_brackets_hint', N'zh-CN', N'格式 上限:税率；上限 0 = 无上限。例 10000000:5;30000000:10;60000000:20;100000000:30;0:35' UNION ALL
    SELECT N'hr_pay_tax_mode' AS ResourceKey, N'en-US' AS Culture, N'Tax method' AS Value UNION ALL
    SELECT N'hr_pay_tax_mode', N'vi-VN', N'Cách tính thuế' UNION ALL
    SELECT N'hr_pay_tax_mode', N'zh-CN', N'计税方式' UNION ALL
    SELECT N'hr_pay_total' AS ResourceKey, N'en-US' AS Culture, N'Total' AS Value UNION ALL
    SELECT N'hr_pay_total', N'vi-VN', N'Tổng cộng' UNION ALL
    SELECT N'hr_pay_total', N'zh-CN', N'合计' UNION ALL
    SELECT N'hr_pay_total_deduction' AS ResourceKey, N'en-US' AS Culture, N'Total deductions' AS Value UNION ALL
    SELECT N'hr_pay_total_deduction', N'vi-VN', N'Tổng khấu trừ' UNION ALL
    SELECT N'hr_pay_total_deduction', N'zh-CN', N'扣除合计' UNION ALL
    SELECT N'hr_pay_transaction_type' AS ResourceKey, N'en-US' AS Culture, N'Transaction type' AS Value UNION ALL
    SELECT N'hr_pay_transaction_type', N'vi-VN', N'Loại nghiệp vụ' UNION ALL
    SELECT N'hr_pay_transaction_type', N'zh-CN', N'业务类型' UNION ALL
    SELECT N'hr_pay_union_fee_enabled' AS ResourceKey, N'en-US' AS Culture, N'Accrue 2% union fee' AS Value UNION ALL
    SELECT N'hr_pay_union_fee_enabled', N'vi-VN', N'Trích kinh phí công đoàn 2%' UNION ALL
    SELECT N'hr_pay_union_fee_enabled', N'zh-CN', N'计提 2% 工会经费' UNION ALL
    SELECT N'hr_pay_unlock' AS ResourceKey, N'en-US' AS Culture, N'Unlock' AS Value UNION ALL
    SELECT N'hr_pay_unlock', N'vi-VN', N'Mở chốt' UNION ALL
    SELECT N'hr_pay_unlock', N'zh-CN', N'解锁' UNION ALL
    SELECT N'hr_pay_unlock_confirm' AS ResourceKey, N'en-US' AS Culture, N'Unlock the payroll for changes?' AS Value UNION ALL
    SELECT N'hr_pay_unlock_confirm', N'vi-VN', N'Mở chốt để sửa lại bảng lương?' UNION ALL
    SELECT N'hr_pay_unlock_confirm', N'zh-CN', N'解锁以修改工资表？' UNION ALL
    SELECT N'hr_pay_unlocked' AS ResourceKey, N'en-US' AS Culture, N'Payroll unlocked' AS Value UNION ALL
    SELECT N'hr_pay_unlocked', N'vi-VN', N'Đã mở chốt' UNION ALL
    SELECT N'hr_pay_unlocked', N'zh-CN', N'已解锁' UNION ALL
    SELECT N'hr_pay_unpost' AS ResourceKey, N'en-US' AS Culture, N'Unpost' AS Value UNION ALL
    SELECT N'hr_pay_unpost', N'vi-VN', N'Hủy hạch toán' UNION ALL
    SELECT N'hr_pay_unpost', N'zh-CN', N'撤销过账' UNION ALL
    SELECT N'hr_pay_unpost_confirm' AS ResourceKey, N'en-US' AS Culture, N'Delete draft voucher {0} and return the period to locked?' AS Value UNION ALL
    SELECT N'hr_pay_unpost_confirm', N'vi-VN', N'Xóa chứng từ nháp {0} và đưa kỳ lương về trạng thái đã chốt?' UNION ALL
    SELECT N'hr_pay_unpost_confirm', N'zh-CN', N'删除草稿凭证 {0} 并将期间恢复为已锁定？' UNION ALL
    SELECT N'hr_pay_unposted' AS ResourceKey, N'en-US' AS Culture, N'Posting cancelled' AS Value UNION ALL
    SELECT N'hr_pay_unposted', N'vi-VN', N'Đã hủy hạch toán' UNION ALL
    SELECT N'hr_pay_unposted', N'zh-CN', N'已撤销过账' UNION ALL
    SELECT N'hr_pay_view_slip' AS ResourceKey, N'en-US' AS Culture, N'View payslip' AS Value UNION ALL
    SELECT N'hr_pay_view_slip', N'vi-VN', N'Xem phiếu' UNION ALL
    SELECT N'hr_pay_view_slip', N'zh-CN', N'查看工资单' UNION ALL
    SELECT N'hr_pay_voucher' AS ResourceKey, N'en-US' AS Culture, N'Voucher' AS Value UNION ALL
    SELECT N'hr_pay_voucher', N'vi-VN', N'Chứng từ' UNION ALL
    SELECT N'hr_pay_voucher', N'zh-CN', N'凭证' UNION ALL
    SELECT N'hr_pay_voucher_date' AS ResourceKey, N'en-US' AS Culture, N'Voucher date' AS Value UNION ALL
    SELECT N'hr_pay_voucher_date', N'vi-VN', N'Ngày chứng từ' UNION ALL
    SELECT N'hr_pay_voucher_date', N'zh-CN', N'凭证日期' UNION ALL
    SELECT N'hr_pay_warn_below_min_wage' AS ResourceKey, N'en-US' AS Culture, N'Insurance salary below regional minimum wage' AS Value UNION ALL
    SELECT N'hr_pay_warn_below_min_wage', N'vi-VN', N'Lương đóng BH thấp hơn lương tối thiểu vùng' UNION ALL
    SELECT N'hr_pay_warn_below_min_wage', N'zh-CN', N'缴保工资低于地区最低工资' UNION ALL
    SELECT N'hr_pay_warn_month_not_finished' AS ResourceKey, N'en-US' AS Culture, N'Month not finished — future days not counted' AS Value UNION ALL
    SELECT N'hr_pay_warn_month_not_finished', N'vi-VN', N'Tháng chưa kết thúc — ngày chưa tới chưa tính công' UNION ALL
    SELECT N'hr_pay_warn_month_not_finished', N'zh-CN', N'月份未结束——未来日期未计工' UNION ALL
    SELECT N'hr_pay_warn_negative_net' AS ResourceKey, N'en-US' AS Culture, N'Negative net pay' AS Value UNION ALL
    SELECT N'hr_pay_warn_negative_net', N'vi-VN', N'Thực lĩnh âm' UNION ALL
    SELECT N'hr_pay_warn_negative_net', N'zh-CN', N'实发为负' UNION ALL
    SELECT N'hr_pay_warn_no_contract' AS ResourceKey, N'en-US' AS Culture, N'No active contract in the month' AS Value UNION ALL
    SELECT N'hr_pay_warn_no_contract', N'vi-VN', N'Không có hợp đồng hiệu lực trong tháng' UNION ALL
    SELECT N'hr_pay_warn_no_contract', N'zh-CN', N'本月无有效合同' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_days' AS ResourceKey, N'en-US' AS Culture, N'14+ unpaid days — no insurance this month' AS Value UNION ALL
    SELECT N'hr_pay_warn_no_insurance_days', N'vi-VN', N'Nghỉ không hưởng lương từ 14 ngày — không đóng BH tháng này' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_days', N'zh-CN', N'无薪天数 ≥14——本月不缴保险' UNION ALL
    SELECT N'hr_pay_warn_no_timesheet' AS ResourceKey, N'en-US' AS Culture, N'No attendance account — full days assumed' AS Value UNION ALL
    SELECT N'hr_pay_warn_no_timesheet', N'vi-VN', N'Chưa có tài khoản chấm công — tạm tính đủ công' UNION ALL
    SELECT N'hr_pay_warn_no_timesheet', N'zh-CN', N'无考勤账号——按满勤计算' UNION ALL
    SELECT N'hr_pay_warning_count' AS ResourceKey, N'en-US' AS Culture, N'{0} payslips with warnings' AS Value UNION ALL
    SELECT N'hr_pay_warning_count', N'vi-VN', N'{0} phiếu có cảnh báo' UNION ALL
    SELECT N'hr_pay_warning_count', N'zh-CN', N'{0} 张工资单有警告' UNION ALL
    SELECT N'hr_payroll' AS ResourceKey, N'en-US' AS Culture, N'Payroll' AS Value UNION ALL
    SELECT N'hr_payroll', N'vi-VN', N'Bảng lương' UNION ALL
    SELECT N'hr_payroll', N'zh-CN', N'工资表' UNION ALL
    SELECT N'hr_payroll_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Payroll from timesheet and contracts, insurance, PIT, lock and post to accounting' AS Value UNION ALL
    SELECT N'hr_payroll_subtitle', N'vi-VN', N'Tính lương theo bảng công và hợp đồng, BHXH, thuế TNCN, chốt và hạch toán' UNION ALL
    SELECT N'hr_payroll_subtitle', N'zh-CN', N'根据考勤和合同计算工资、社保、个税，锁定并过账' UNION ALL
    SELECT N'hr_permanent_address' AS ResourceKey, N'en-US' AS Culture, N'Permanent address' AS Value UNION ALL
    SELECT N'hr_permanent_address', N'vi-VN', N'Địa chỉ thường trú' UNION ALL
    SELECT N'hr_permanent_address', N'zh-CN', N'户籍地址' UNION ALL
    SELECT N'hr_personal_email' AS ResourceKey, N'en-US' AS Culture, N'Personal email' AS Value UNION ALL
    SELECT N'hr_personal_email', N'vi-VN', N'Email cá nhân' UNION ALL
    SELECT N'hr_personal_email', N'zh-CN', N'个人邮箱' UNION ALL
    SELECT N'hr_phone' AS ResourceKey, N'en-US' AS Culture, N'Phone' AS Value UNION ALL
    SELECT N'hr_phone', N'vi-VN', N'Điện thoại' UNION ALL
    SELECT N'hr_phone', N'zh-CN', N'电话' UNION ALL
    SELECT N'hr_place_of_birth' AS ResourceKey, N'en-US' AS Culture, N'Place of birth' AS Value UNION ALL
    SELECT N'hr_place_of_birth', N'vi-VN', N'Nơi sinh' UNION ALL
    SELECT N'hr_place_of_birth', N'zh-CN', N'出生地' UNION ALL
    SELECT N'hr_position' AS ResourceKey, N'en-US' AS Culture, N'Position' AS Value UNION ALL
    SELECT N'hr_position', N'vi-VN', N'Chức vụ' UNION ALL
    SELECT N'hr_position', N'zh-CN', N'职位' UNION ALL
    SELECT N'hr_positions' AS ResourceKey, N'en-US' AS Culture, N'Positions' AS Value UNION ALL
    SELECT N'hr_positions', N'vi-VN', N'Chức vụ' UNION ALL
    SELECT N'hr_positions', N'zh-CN', N'职位' UNION ALL
    SELECT N'hr_probation_end' AS ResourceKey, N'en-US' AS Culture, N'Probation end' AS Value UNION ALL
    SELECT N'hr_probation_end', N'vi-VN', N'Hết thử việc' UNION ALL
    SELECT N'hr_probation_end', N'zh-CN', N'试用期结束' UNION ALL
    SELECT N'hr_probation_ending' AS ResourceKey, N'en-US' AS Culture, N'Probation ending' AS Value UNION ALL
    SELECT N'hr_probation_ending', N'vi-VN', N'Sắp hết thử việc' UNION ALL
    SELECT N'hr_probation_ending', N'zh-CN', N'试用期即将结束' UNION ALL
    SELECT N'hr_probation_sync_hint' AS ResourceKey, N'en-US' AS Culture, N'The employee''s probation end date will follow this contract''s end date' AS Value UNION ALL
    SELECT N'hr_probation_sync_hint', N'vi-VN', N'Ngày hết thử việc của nhân viên sẽ được cập nhật theo ngày kết thúc hợp đồng này' UNION ALL
    SELECT N'hr_probation_sync_hint', N'zh-CN', N'员工试用期结束日期将按此合同结束日期更新' UNION ALL
    SELECT N'hr_profile' AS ResourceKey, N'en-US' AS Culture, N'Profile' AS Value UNION ALL
    SELECT N'hr_profile', N'vi-VN', N'Hồ sơ' UNION ALL
    SELECT N'hr_profile', N'zh-CN', N'档案' UNION ALL
    SELECT N'hr_recorded_at' AS ResourceKey, N'en-US' AS Culture, N'Recorded at' AS Value UNION ALL
    SELECT N'hr_recorded_at', N'vi-VN', N'Thời điểm ghi' UNION ALL
    SELECT N'hr_recorded_at', N'zh-CN', N'记录时间' UNION ALL
    SELECT N'hr_reject' AS ResourceKey, N'en-US' AS Culture, N'Reject' AS Value UNION ALL
    SELECT N'hr_reject', N'vi-VN', N'Từ chối' UNION ALL
    SELECT N'hr_reject', N'zh-CN', N'拒绝' UNION ALL
    SELECT N'hr_resign_date' AS ResourceKey, N'en-US' AS Culture, N'Resignation date' AS Value UNION ALL
    SELECT N'hr_resign_date', N'vi-VN', N'Ngày nghỉ việc' UNION ALL
    SELECT N'hr_resign_date', N'zh-CN', N'离职日期' UNION ALL
    SELECT N'hr_resign_reason' AS ResourceKey, N'en-US' AS Culture, N'Resignation reason' AS Value UNION ALL
    SELECT N'hr_resign_reason', N'vi-VN', N'Lý do nghỉ việc' UNION ALL
    SELECT N'hr_resign_reason', N'zh-CN', N'离职原因'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Phần 5/5
;WITH src AS (
    SELECT N'hr_resigned_this_month' AS ResourceKey, N'en-US' AS Culture, N'Resigned this month' AS Value UNION ALL
    SELECT N'hr_resigned_this_month', N'vi-VN', N'Nghỉ việc trong tháng' UNION ALL
    SELECT N'hr_resigned_this_month', N'zh-CN', N'本月离职' UNION ALL
    SELECT N'hr_salary_hidden_hint' AS ResourceKey, N'en-US' AS Culture, N'You do not have permission to view salary (HR_Salary)' AS Value UNION ALL
    SELECT N'hr_salary_hidden_hint', N'vi-VN', N'Bạn không có quyền xem thông tin lương (HR_Salary)' UNION ALL
    SELECT N'hr_salary_hidden_hint', N'zh-CN', N'您无权查看工资信息（HR_Salary）' UNION ALL
    SELECT N'hr_salary_readonly_hint' AS ResourceKey, N'en-US' AS Culture, N'You can view but not edit salary' AS Value UNION ALL
    SELECT N'hr_salary_readonly_hint', N'vi-VN', N'Bạn chỉ có quyền xem lương, không được sửa' UNION ALL
    SELECT N'hr_salary_readonly_hint', N'zh-CN', N'您只能查看工资，不能修改' UNION ALL
    SELECT N'hr_saved' AS ResourceKey, N'en-US' AS Culture, N'Saved' AS Value UNION ALL
    SELECT N'hr_saved', N'vi-VN', N'Đã lưu' UNION ALL
    SELECT N'hr_saved', N'zh-CN', N'已保存' UNION ALL
    SELECT N'hr_search' AS ResourceKey, N'en-US' AS Culture, N'Search' AS Value UNION ALL
    SELECT N'hr_search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'hr_search', N'zh-CN', N'搜索' UNION ALL
    SELECT N'hr_search_contract' AS ResourceKey, N'en-US' AS Culture, N'Search contract no., code, name (Enter)' AS Value UNION ALL
    SELECT N'hr_search_contract', N'vi-VN', N'Tìm số HĐ, mã NV, tên (Enter)' UNION ALL
    SELECT N'hr_search_contract', N'zh-CN', N'搜索合同号、编号、姓名（回车）' UNION ALL
    SELECT N'hr_search_employee' AS ResourceKey, N'en-US' AS Culture, N'Search code, name, phone, email, ID (Enter)' AS Value UNION ALL
    SELECT N'hr_search_employee', N'vi-VN', N'Tìm mã, tên, SĐT, email, CCCD (Enter)' UNION ALL
    SELECT N'hr_search_employee', N'zh-CN', N'搜索编号、姓名、电话、邮箱、证件（回车）' UNION ALL
    SELECT N'hr_selected_count' AS ResourceKey, N'en-US' AS Culture, N'{0} selected' AS Value UNION ALL
    SELECT N'hr_selected_count', N'vi-VN', N'Đã chọn {0}' UNION ALL
    SELECT N'hr_selected_count', N'zh-CN', N'已选 {0}' UNION ALL
    SELECT N'hr_seniority' AS ResourceKey, N'en-US' AS Culture, N'Seniority' AS Value UNION ALL
    SELECT N'hr_seniority', N'vi-VN', N'Thâm niên' UNION ALL
    SELECT N'hr_seniority', N'zh-CN', N'工龄' UNION ALL
    SELECT N'hr_seniority_format' AS ResourceKey, N'en-US' AS Culture, N'{0} years {1} months' AS Value UNION ALL
    SELECT N'hr_seniority_format', N'vi-VN', N'{0} năm {1} tháng' UNION ALL
    SELECT N'hr_seniority_format', N'zh-CN', N'{0} 年 {1} 个月' UNION ALL
    SELECT N'hr_set_allow_negative' AS ResourceKey, N'en-US' AS Culture, N'Allow annual leave beyond the balance' AS Value UNION ALL
    SELECT N'hr_set_allow_negative', N'vi-VN', N'Cho phép nghỉ phép năm vượt quỹ (âm phép)' UNION ALL
    SELECT N'hr_set_allow_negative', N'zh-CN', N'允许超额使用年假' UNION ALL
    SELECT N'hr_set_attendance' AS ResourceKey, N'en-US' AS Culture, N'Attendance' AS Value UNION ALL
    SELECT N'hr_set_attendance', N'vi-VN', N'Chấm công' UNION ALL
    SELECT N'hr_set_attendance', N'zh-CN', N'考勤' UNION ALL
    SELECT N'hr_set_base_days' AS ResourceKey, N'en-US' AS Culture, N'Standard annual leave days' AS Value UNION ALL
    SELECT N'hr_set_base_days', N'vi-VN', N'Số ngày phép năm tiêu chuẩn' UNION ALL
    SELECT N'hr_set_base_days', N'zh-CN', N'标准年假天数' UNION ALL
    SELECT N'hr_set_base_days_hint' AS ResourceKey, N'en-US' AS Culture, N'Labour Code: 12 days in normal conditions; mid-year joiners are prorated by month' AS Value UNION ALL
    SELECT N'hr_set_base_days_hint', N'vi-VN', N'Bộ luật Lao động: 12 ngày với điều kiện bình thường; nhân viên vào làm giữa năm được tính theo tỷ lệ tháng' UNION ALL
    SELECT N'hr_set_base_days_hint', N'zh-CN', N'劳动法：正常条件下 12 天；年中入职按月折算' UNION ALL
    SELECT N'hr_set_calendar' AS ResourceKey, N'en-US' AS Culture, N'Working calendar' AS Value UNION ALL
    SELECT N'hr_set_calendar', N'vi-VN', N'Lịch làm việc' UNION ALL
    SELECT N'hr_set_calendar', N'zh-CN', N'工作日历' UNION ALL
    SELECT N'hr_set_hours_per_day' AS ResourceKey, N'en-US' AS Culture, N'Working hours per day' AS Value UNION ALL
    SELECT N'hr_set_hours_per_day', N'vi-VN', N'Số giờ làm việc/ngày' UNION ALL
    SELECT N'hr_set_hours_per_day', N'zh-CN', N'每日工作小时' UNION ALL
    SELECT N'hr_set_ip_client' AS ResourceKey, N'en-US' AS Culture, N'Browser detects IP (current behaviour)' AS Value UNION ALL
    SELECT N'hr_set_ip_client', N'vi-VN', N'Trình duyệt tự lấy IP (như cũ)' UNION ALL
    SELECT N'hr_set_ip_client', N'zh-CN', N'浏览器获取 IP（现有方式）' UNION ALL
    SELECT N'hr_set_ip_hint' AS ResourceKey, N'en-US' AS Culture, N'"Server" is harder to spoof but needs IIS/proxy to pass the client IP (X-Forwarded-For). Test a check-in after switching; if office check-ins show as remote, switch back.' AS Value UNION ALL
    SELECT N'hr_set_ip_hint', N'vi-VN', N'"Server" khó giả mạo hơn nhưng cần IIS/proxy chuyển đúng IP người dùng (X-Forwarded-For). Hãy thử chấm công sau khi đổi; nếu tại văn phòng mà bị báo "ngoài văn phòng" thì chuyển lại.' UNION ALL
    SELECT N'hr_set_ip_hint', N'zh-CN', N'"服务器"更难伪造，但需要 IIS/代理正确传递客户端 IP（X-Forwarded-For）。切换后请测试打卡；若在办公室被判为远程，请切换回来。' UNION ALL
    SELECT N'hr_set_ip_server' AS ResourceKey, N'en-US' AS Culture, N'Server reads IP from the request (harder to spoof)' AS Value UNION ALL
    SELECT N'hr_set_ip_server', N'vi-VN', N'Server lấy IP từ request (khó giả mạo hơn)' UNION ALL
    SELECT N'hr_set_ip_server', N'zh-CN', N'服务器从请求读取 IP（更难伪造）' UNION ALL
    SELECT N'hr_set_ip_source' AS ResourceKey, N'en-US' AS Culture, N'IP source for check-in' AS Value UNION ALL
    SELECT N'hr_set_ip_source', N'vi-VN', N'Nguồn địa chỉ IP khi chấm công' UNION ALL
    SELECT N'hr_set_ip_source', N'zh-CN', N'打卡 IP 来源' UNION ALL
    SELECT N'hr_set_leave_rules' AS ResourceKey, N'en-US' AS Culture, N'Annual leave rules' AS Value UNION ALL
    SELECT N'hr_set_leave_rules', N'vi-VN', N'Quy tắc phép năm' UNION ALL
    SELECT N'hr_set_leave_rules', N'zh-CN', N'年假规则' UNION ALL
    SELECT N'hr_set_max_carry' AS ResourceKey, N'en-US' AS Culture, N'Max days carried over to next year' AS Value UNION ALL
    SELECT N'hr_set_max_carry', N'vi-VN', N'Số ngày phép tồn tối đa được chuyển sang năm sau' UNION ALL
    SELECT N'hr_set_max_carry', N'zh-CN', N'可结转至次年的最多天数' UNION ALL
    SELECT N'hr_set_max_carry_hint' AS ResourceKey, N'en-US' AS Culture, N'0 = no carry-over' AS Value UNION ALL
    SELECT N'hr_set_max_carry_hint', N'vi-VN', N'0 = không chuyển phép tồn' UNION ALL
    SELECT N'hr_set_max_carry_hint', N'zh-CN', N'0 = 不结转' UNION ALL
    SELECT N'hr_set_saturday_half' AS ResourceKey, N'en-US' AS Culture, N'Saturday is morning only (0.5 day)' AS Value UNION ALL
    SELECT N'hr_set_saturday_half', N'vi-VN', N'Thứ 7 chỉ làm buổi sáng (0,5 công)' UNION ALL
    SELECT N'hr_set_saturday_half', N'zh-CN', N'周六仅上午（0.5 天）' UNION ALL
    SELECT N'hr_set_seniority_step' AS ResourceKey, N'en-US' AS Culture, N'+1 leave day every N years of service' AS Value UNION ALL
    SELECT N'hr_set_seniority_step', N'vi-VN', N'Cứ mỗi N năm thâm niên được thêm 1 ngày phép' UNION ALL
    SELECT N'hr_set_seniority_step', N'zh-CN', N'每满 N 年工龄增加 1 天年假' UNION ALL
    SELECT N'hr_set_seniority_step_hint' AS ResourceKey, N'en-US' AS Culture, N'Labour Code: 5 years. 0 = disabled' AS Value UNION ALL
    SELECT N'hr_set_seniority_step_hint', N'vi-VN', N'Bộ luật Lao động: 5 năm. 0 = không tính thâm niên' UNION ALL
    SELECT N'hr_set_seniority_step_hint', N'zh-CN', N'劳动法：5 年。0 = 不计算' UNION ALL
    SELECT N'hr_set_workdays' AS ResourceKey, N'en-US' AS Culture, N'Working days' AS Value UNION ALL
    SELECT N'hr_set_workdays', N'vi-VN', N'Ngày làm việc trong tuần' UNION ALL
    SELECT N'hr_set_workdays', N'zh-CN', N'每周工作日' UNION ALL
    SELECT N'hr_settings' AS ResourceKey, N'en-US' AS Culture, N'HR settings' AS Value UNION ALL
    SELECT N'hr_settings', N'vi-VN', N'Cài đặt nhân sự' UNION ALL
    SELECT N'hr_settings', N'zh-CN', N'人事设置' UNION ALL
    SELECT N'hr_settings_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Working calendar, holidays, leave and attendance rules' AS Value UNION ALL
    SELECT N'hr_settings_subtitle', N'vi-VN', N'Lịch làm việc, ngày lễ, quy tắc phép năm và chấm công' UNION ALL
    SELECT N'hr_settings_subtitle', N'zh-CN', N'工作日历、节假日、年假及考勤规则' UNION ALL
    SELECT N'hr_settings_tab_holidays' AS ResourceKey, N'en-US' AS Culture, N'Holidays' AS Value UNION ALL
    SELECT N'hr_settings_tab_holidays', N'vi-VN', N'Ngày lễ' UNION ALL
    SELECT N'hr_settings_tab_holidays', N'zh-CN', N'节假日' UNION ALL
    SELECT N'hr_settings_tab_rules' AS ResourceKey, N'en-US' AS Culture, N'Rules' AS Value UNION ALL
    SELECT N'hr_settings_tab_rules', N'vi-VN', N'Quy tắc' UNION ALL
    SELECT N'hr_settings_tab_rules', N'zh-CN', N'规则' UNION ALL
    SELECT N'hr_sign_date' AS ResourceKey, N'en-US' AS Culture, N'Signing date' AS Value UNION ALL
    SELECT N'hr_sign_date', N'vi-VN', N'Ngày ký' UNION ALL
    SELECT N'hr_sign_date', N'zh-CN', N'签订日期' UNION ALL
    SELECT N'hr_social_insurance_no' AS ResourceKey, N'en-US' AS Culture, N'Social insurance no.' AS Value UNION ALL
    SELECT N'hr_social_insurance_no', N'vi-VN', N'Số sổ BHXH' UNION ALL
    SELECT N'hr_social_insurance_no', N'zh-CN', N'社保号' UNION ALL
    SELECT N'hr_sort_order' AS ResourceKey, N'en-US' AS Culture, N'Order' AS Value UNION ALL
    SELECT N'hr_sort_order', N'vi-VN', N'Thứ tự' UNION ALL
    SELECT N'hr_sort_order', N'zh-CN', N'排序' UNION ALL
    SELECT N'hr_sort_order_hint' AS ResourceKey, N'en-US' AS Culture, N'Lower number = higher rank' AS Value UNION ALL
    SELECT N'hr_sort_order_hint', N'vi-VN', N'Số nhỏ = cấp cao hơn' UNION ALL
    SELECT N'hr_sort_order_hint', N'zh-CN', N'数字越小级别越高' UNION ALL
    SELECT N'hr_start_date' AS ResourceKey, N'en-US' AS Culture, N'Start date' AS Value UNION ALL
    SELECT N'hr_start_date', N'vi-VN', N'Ngày bắt đầu' UNION ALL
    SELECT N'hr_start_date', N'zh-CN', N'开始日期' UNION ALL
    SELECT N'hr_status' AS ResourceKey, N'en-US' AS Culture, N'Status' AS Value UNION ALL
    SELECT N'hr_status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'hr_status', N'zh-CN', N'状态' UNION ALL
    SELECT N'hr_status_active' AS ResourceKey, N'en-US' AS Culture, N'Active' AS Value UNION ALL
    SELECT N'hr_status_active', N'vi-VN', N'Chính thức' UNION ALL
    SELECT N'hr_status_active', N'zh-CN', N'正式' UNION ALL
    SELECT N'hr_status_probation' AS ResourceKey, N'en-US' AS Culture, N'Probation' AS Value UNION ALL
    SELECT N'hr_status_probation', N'vi-VN', N'Thử việc' UNION ALL
    SELECT N'hr_status_probation', N'zh-CN', N'试用' UNION ALL
    SELECT N'hr_status_resigned' AS ResourceKey, N'en-US' AS Culture, N'Resigned' AS Value UNION ALL
    SELECT N'hr_status_resigned', N'vi-VN', N'Đã nghỉ việc' UNION ALL
    SELECT N'hr_status_resigned', N'zh-CN', N'已离职' UNION ALL
    SELECT N'hr_status_suspended' AS ResourceKey, N'en-US' AS Culture, N'On leave' AS Value UNION ALL
    SELECT N'hr_status_suspended', N'vi-VN', N'Tạm nghỉ' UNION ALL
    SELECT N'hr_status_suspended', N'zh-CN', N'停薪留职' UNION ALL
    SELECT N'hr_tab_contracts' AS ResourceKey, N'en-US' AS Culture, N'Contracts' AS Value UNION ALL
    SELECT N'hr_tab_contracts', N'vi-VN', N'Hợp đồng' UNION ALL
    SELECT N'hr_tab_contracts', N'zh-CN', N'合同' UNION ALL
    SELECT N'hr_tab_documents' AS ResourceKey, N'en-US' AS Culture, N'Documents' AS Value UNION ALL
    SELECT N'hr_tab_documents', N'vi-VN', N'Giấy tờ' UNION ALL
    SELECT N'hr_tab_documents', N'zh-CN', N'证件文件' UNION ALL
    SELECT N'hr_tab_general' AS ResourceKey, N'en-US' AS Culture, N'General' AS Value UNION ALL
    SELECT N'hr_tab_general', N'vi-VN', N'Thông tin chung' UNION ALL
    SELECT N'hr_tab_general', N'zh-CN', N'基本信息' UNION ALL
    SELECT N'hr_tab_history' AS ResourceKey, N'en-US' AS Culture, N'History' AS Value UNION ALL
    SELECT N'hr_tab_history', N'vi-VN', N'Lịch sử' UNION ALL
    SELECT N'hr_tab_history', N'zh-CN', N'历史' UNION ALL
    SELECT N'hr_tab_legal' AS ResourceKey, N'en-US' AS Culture, N'ID & bank' AS Value UNION ALL
    SELECT N'hr_tab_legal', N'vi-VN', N'Giấy tờ & Ngân hàng' UNION ALL
    SELECT N'hr_tab_legal', N'zh-CN', N'证件与银行' UNION ALL
    SELECT N'hr_tab_overview' AS ResourceKey, N'en-US' AS Culture, N'Overview' AS Value UNION ALL
    SELECT N'hr_tab_overview', N'vi-VN', N'Tổng quan' UNION ALL
    SELECT N'hr_tab_overview', N'zh-CN', N'概览' UNION ALL
    SELECT N'hr_tab_work' AS ResourceKey, N'en-US' AS Culture, N'Job' AS Value UNION ALL
    SELECT N'hr_tab_work', N'vi-VN', N'Công việc' UNION ALL
    SELECT N'hr_tab_work', N'zh-CN', N'工作' UNION ALL
    SELECT N'hr_tax_code' AS ResourceKey, N'en-US' AS Culture, N'Personal tax code' AS Value UNION ALL
    SELECT N'hr_tax_code', N'vi-VN', N'Mã số thuế cá nhân' UNION ALL
    SELECT N'hr_tax_code', N'zh-CN', N'个人税号' UNION ALL
    SELECT N'hr_tax_flat' AS ResourceKey, N'en-US' AS Culture, N'Flat 10%' AS Value UNION ALL
    SELECT N'hr_tax_flat', N'vi-VN', N'Khấu trừ 10%' UNION ALL
    SELECT N'hr_tax_flat', N'zh-CN', N'10% 扣缴' UNION ALL
    SELECT N'hr_tax_none' AS ResourceKey, N'en-US' AS Culture, N'No withholding' AS Value UNION ALL
    SELECT N'hr_tax_none', N'vi-VN', N'Không khấu trừ' UNION ALL
    SELECT N'hr_tax_none', N'zh-CN', N'不扣缴' UNION ALL
    SELECT N'hr_tax_progressive' AS ResourceKey, N'en-US' AS Culture, N'Progressive' AS Value UNION ALL
    SELECT N'hr_tax_progressive', N'vi-VN', N'Lũy tiến' UNION ALL
    SELECT N'hr_tax_progressive', N'zh-CN', N'累进' UNION ALL
    SELECT N'hr_terminated_date' AS ResourceKey, N'en-US' AS Culture, N'Termination date' AS Value UNION ALL
    SELECT N'hr_terminated_date', N'vi-VN', N'Ngày chấm dứt' UNION ALL
    SELECT N'hr_terminated_date', N'zh-CN', N'终止日期' UNION ALL
    SELECT N'hr_timesheet' AS ResourceKey, N'en-US' AS Culture, N'Monthly timesheet' AS Value UNION ALL
    SELECT N'hr_timesheet', N'vi-VN', N'Bảng công tháng' UNION ALL
    SELECT N'hr_timesheet', N'zh-CN', N'月度考勤表' UNION ALL
    SELECT N'hr_timesheet_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Monthly summary of attendance, leave and holidays' AS Value UNION ALL
    SELECT N'hr_timesheet_subtitle', N'vi-VN', N'Tổng hợp chấm công, nghỉ phép và ngày lễ theo tháng' UNION ALL
    SELECT N'hr_timesheet_subtitle', N'zh-CN', N'按月汇总考勤、请假和节假日' UNION ALL
    SELECT N'hr_to_time' AS ResourceKey, N'en-US' AS Culture, N'To' AS Value UNION ALL
    SELECT N'hr_to_time', N'vi-VN', N'Đến giờ' UNION ALL
    SELECT N'hr_to_time', N'zh-CN', N'结束时间' UNION ALL
    SELECT N'hr_today' AS ResourceKey, N'en-US' AS Culture, N'Today' AS Value UNION ALL
    SELECT N'hr_today', N'vi-VN', N'Hôm nay' UNION ALL
    SELECT N'hr_today', N'zh-CN', N'今天' UNION ALL
    SELECT N'hr_total_rows' AS ResourceKey, N'en-US' AS Culture, N'Total: {0}' AS Value UNION ALL
    SELECT N'hr_total_rows', N'vi-VN', N'Tổng: {0}' UNION ALL
    SELECT N'hr_total_rows', N'zh-CN', N'合计：{0}' UNION ALL
    SELECT N'hr_total_working' AS ResourceKey, N'en-US' AS Culture, N'Currently employed' AS Value UNION ALL
    SELECT N'hr_total_working', N'vi-VN', N'Đang làm việc' UNION ALL
    SELECT N'hr_total_working', N'zh-CN', N'在职人数' UNION ALL
    SELECT N'hr_ts_absent' AS ResourceKey, N'en-US' AS Culture, N'Absent' AS Value UNION ALL
    SELECT N'hr_ts_absent', N'vi-VN', N'Vắng' UNION ALL
    SELECT N'hr_ts_absent', N'zh-CN', N'缺勤' UNION ALL
    SELECT N'hr_ts_annual' AS ResourceKey, N'en-US' AS Culture, N'Annual' AS Value UNION ALL
    SELECT N'hr_ts_annual', N'vi-VN', N'Phép năm' UNION ALL
    SELECT N'hr_ts_annual', N'zh-CN', N'年假' UNION ALL
    SELECT N'hr_ts_build' AS ResourceKey, N'en-US' AS Culture, N'Build' AS Value UNION ALL
    SELECT N'hr_ts_build', N'vi-VN', N'Tổng hợp' UNION ALL
    SELECT N'hr_ts_build', N'zh-CN', N'生成' UNION ALL
    SELECT N'hr_ts_code_dash' AS ResourceKey, N'en-US' AS Culture, N'Not yet joined / already left' AS Value UNION ALL
    SELECT N'hr_ts_code_dash', N'vi-VN', N'Chưa vào làm / đã nghỉ việc' UNION ALL
    SELECT N'hr_ts_code_dash', N'zh-CN', N'未入职/已离职' UNION ALL
    SELECT N'hr_ts_code_l' AS ResourceKey, N'en-US' AS Culture, N'Public holiday (paid)' AS Value UNION ALL
    SELECT N'hr_ts_code_l', N'vi-VN', N'Nghỉ lễ (hưởng lương)' UNION ALL
    SELECT N'hr_ts_code_l', N'zh-CN', N'法定节假日（带薪）' UNION ALL
    SELECT N'hr_ts_code_q' AS ResourceKey, N'en-US' AS Culture, N'No login account — no attendance data' AS Value UNION ALL
    SELECT N'hr_ts_code_q', N'vi-VN', N'Chưa có tài khoản đăng nhập — không có dữ liệu chấm công' UNION ALL
    SELECT N'hr_ts_code_q', N'zh-CN', N'无登录账号——无考勤数据' UNION ALL
    SELECT N'hr_ts_code_split' AS ResourceKey, N'en-US' AS Culture, N'Morning / afternoon' AS Value UNION ALL
    SELECT N'hr_ts_code_split', N'vi-VN', N'Buổi sáng / buổi chiều' UNION ALL
    SELECT N'hr_ts_code_split', N'zh-CN', N'上午/下午' UNION ALL
    SELECT N'hr_ts_code_tx' AS ResourceKey, N'en-US' AS Culture, N'Remote work (checked in outside the office)' AS Value UNION ALL
    SELECT N'hr_ts_code_tx', N'vi-VN', N'Làm việc từ xa (chấm công ngoài văn phòng)' UNION ALL
    SELECT N'hr_ts_code_tx', N'zh-CN', N'远程办公（办公室外打卡）' UNION ALL
    SELECT N'hr_ts_code_v' AS ResourceKey, N'en-US' AS Culture, N'Absent without leave (no check-in, no request)' AS Value UNION ALL
    SELECT N'hr_ts_code_v', N'vi-VN', N'Vắng không phép (không chấm công, không có đơn)' UNION ALL
    SELECT N'hr_ts_code_v', N'zh-CN', N'无故缺勤（未打卡且无申请）' UNION ALL
    SELECT N'hr_ts_code_x' AS ResourceKey, N'en-US' AS Culture, N'Worked at the office' AS Value UNION ALL
    SELECT N'hr_ts_code_x', N'vi-VN', N'Làm việc tại văn phòng' UNION ALL
    SELECT N'hr_ts_code_x', N'zh-CN', N'在办公室工作' UNION ALL
    SELECT N'hr_ts_goout' AS ResourceKey, N'en-US' AS Culture, N'Going out (times)' AS Value UNION ALL
    SELECT N'hr_ts_goout', N'vi-VN', N'Ra ngoài (lần)' UNION ALL
    SELECT N'hr_ts_goout', N'zh-CN', N'外出（次）' UNION ALL
    SELECT N'hr_ts_holiday' AS ResourceKey, N'en-US' AS Culture, N'Holiday' AS Value UNION ALL
    SELECT N'hr_ts_holiday', N'vi-VN', N'Lễ' UNION ALL
    SELECT N'hr_ts_holiday', N'zh-CN', N'节假日' UNION ALL
    SELECT N'hr_ts_legend' AS ResourceKey, N'en-US' AS Culture, N'Legend' AS Value UNION ALL
    SELECT N'hr_ts_legend', N'vi-VN', N'Chú thích' UNION ALL
    SELECT N'hr_ts_legend', N'zh-CN', N'图例' UNION ALL
    SELECT N'hr_ts_legend_inline' AS ResourceKey, N'en-US' AS Culture, N'X: office · TX: remote · P: annual · Ô: sick · R: personal · KL: unpaid · L: holiday · V: absent · ?: no account · -: not employed · a/b: morning/afternoon' AS Value UNION ALL
    SELECT N'hr_ts_legend_inline', N'vi-VN', N'X: tại VP · TX: từ xa · P: phép năm · Ô: ốm · R: việc riêng · KL: không lương · L: lễ · V: vắng · ?: chưa có tài khoản · -: chưa vào làm/đã nghỉ · a/b: sáng/chiều' UNION ALL
    SELECT N'hr_ts_legend_inline', N'zh-CN', N'X：办公室 · TX：远程 · P：年假 · Ô：病假 · R：事假 · KL：无薪 · L：节假日 · V：缺勤 · ?：无账号 · -：未在职 · a/b：上午/下午' UNION ALL
    SELECT N'hr_ts_note' AS ResourceKey, N'en-US' AS Culture, N'Work is counted per session (0.5 day each) from the 8–9h and 13–14h check-ins. A session with no check-in and no approved leave counts as absent. "Paid days" = worked + annual + personal + holidays; sick leave is paid by social insurance and is not included.' AS Value UNION ALL
    SELECT N'hr_ts_note', N'vi-VN', N'Công được tính theo buổi (0,5 công/buổi) từ lượt chấm công 8–9h và 13–14h. Buổi không chấm công và không có đơn nghỉ đã duyệt được tính là vắng. "Công hưởng lương" = làm việc + phép năm + việc riêng + lễ; nghỉ ốm do BHXH chi trả nên không cộng vào.' UNION ALL
    SELECT N'hr_ts_note', N'zh-CN', N'按时段计工（每时段 0.5 天），依据 8–9 点和 13–14 点的打卡。无打卡且无已批准请假的时段计为缺勤。"带薪工日" = 出勤 + 年假 + 事假 + 节假日；病假由社保支付，不计入。' UNION ALL
    SELECT N'hr_ts_paid' AS ResourceKey, N'en-US' AS Culture, N'Paid days' AS Value UNION ALL
    SELECT N'hr_ts_paid', N'vi-VN', N'Công hưởng lương' UNION ALL
    SELECT N'hr_ts_paid', N'zh-CN', N'带薪工日' UNION ALL
    SELECT N'hr_ts_personal' AS ResourceKey, N'en-US' AS Culture, N'Personal' AS Value UNION ALL
    SELECT N'hr_ts_personal', N'vi-VN', N'Việc riêng' UNION ALL
    SELECT N'hr_ts_personal', N'zh-CN', N'事假' UNION ALL
    SELECT N'hr_ts_remote' AS ResourceKey, N'en-US' AS Culture, N'Remote' AS Value UNION ALL
    SELECT N'hr_ts_remote', N'vi-VN', N'Từ xa' UNION ALL
    SELECT N'hr_ts_remote', N'zh-CN', N'远程' UNION ALL
    SELECT N'hr_ts_sick' AS ResourceKey, N'en-US' AS Culture, N'Sick' AS Value UNION ALL
    SELECT N'hr_ts_sick', N'vi-VN', N'Ốm' UNION ALL
    SELECT N'hr_ts_sick', N'zh-CN', N'病假' UNION ALL
    SELECT N'hr_ts_standard' AS ResourceKey, N'en-US' AS Culture, N'Standard' AS Value UNION ALL
    SELECT N'hr_ts_standard', N'vi-VN', N'Công chuẩn' UNION ALL
    SELECT N'hr_ts_standard', N'zh-CN', N'标准工日' UNION ALL
    SELECT N'hr_ts_standard_days' AS ResourceKey, N'en-US' AS Culture, N'Standard working days' AS Value UNION ALL
    SELECT N'hr_ts_standard_days', N'vi-VN', N'Số công chuẩn của tháng' UNION ALL
    SELECT N'hr_ts_standard_days', N'zh-CN', N'本月标准工日' UNION ALL
    SELECT N'hr_ts_summary' AS ResourceKey, N'en-US' AS Culture, N'{0} employees · {1} standard days' AS Value UNION ALL
    SELECT N'hr_ts_summary', N'vi-VN', N'{0} nhân viên · công chuẩn {1}' UNION ALL
    SELECT N'hr_ts_summary', N'zh-CN', N'{0} 名员工 · 标准工日 {1}' UNION ALL
    SELECT N'hr_ts_unpaid' AS ResourceKey, N'en-US' AS Culture, N'Unpaid' AS Value UNION ALL
    SELECT N'hr_ts_unpaid', N'vi-VN', N'Không lương' UNION ALL
    SELECT N'hr_ts_unpaid', N'zh-CN', N'无薪' UNION ALL
    SELECT N'hr_ts_worked' AS ResourceKey, N'en-US' AS Culture, N'Worked' AS Value UNION ALL
    SELECT N'hr_ts_worked', N'vi-VN', N'Làm việc' UNION ALL
    SELECT N'hr_ts_worked', N'zh-CN', N'出勤' UNION ALL
    SELECT N'hr_unassigned' AS ResourceKey, N'en-US' AS Culture, N'(Unassigned)' AS Value UNION ALL
    SELECT N'hr_unassigned', N'vi-VN', N'(Chưa gán)' UNION ALL
    SELECT N'hr_unassigned', N'zh-CN', N'（未分配）' UNION ALL
    SELECT N'hr_unknown' AS ResourceKey, N'en-US' AS Culture, N'Unknown' AS Value UNION ALL
    SELECT N'hr_unknown', N'vi-VN', N'Chưa rõ' UNION ALL
    SELECT N'hr_unknown', N'zh-CN', N'未知' UNION ALL
    SELECT N'hr_upload' AS ResourceKey, N'en-US' AS Culture, N'Upload' AS Value UNION ALL
    SELECT N'hr_upload', N'vi-VN', N'Tải lên' UNION ALL
    SELECT N'hr_upload', N'zh-CN', N'上传' UNION ALL
    SELECT N'hr_uploaded_at' AS ResourceKey, N'en-US' AS Culture, N'Uploaded at' AS Value UNION ALL
    SELECT N'hr_uploaded_at', N'vi-VN', N'Ngày tải lên' UNION ALL
    SELECT N'hr_uploaded_at', N'zh-CN', N'上传时间' UNION ALL
    SELECT N'hr_uploading' AS ResourceKey, N'en-US' AS Culture, N'Uploading...' AS Value UNION ALL
    SELECT N'hr_uploading', N'vi-VN', N'Đang tải lên...' UNION ALL
    SELECT N'hr_uploading', N'zh-CN', N'上传中...' UNION ALL
    SELECT N'hr_user_count' AS ResourceKey, N'en-US' AS Culture, N'Accounts' AS Value UNION ALL
    SELECT N'hr_user_count', N'vi-VN', N'Số tài khoản' UNION ALL
    SELECT N'hr_user_count', N'zh-CN', N'账号数' UNION ALL
    SELECT N'hr_view' AS ResourceKey, N'en-US' AS Culture, N'View' AS Value UNION ALL
    SELECT N'hr_view', N'vi-VN', N'Xem' UNION ALL
    SELECT N'hr_view', N'zh-CN', N'查看' UNION ALL
    SELECT N'hr_without_account' AS ResourceKey, N'en-US' AS Culture, N'Without account' AS Value UNION ALL
    SELECT N'hr_without_account', N'vi-VN', N'Chưa có tài khoản' UNION ALL
    SELECT N'hr_without_account', N'zh-CN', N'无账号' UNION ALL
    SELECT N'hr_without_contract' AS ResourceKey, N'en-US' AS Culture, N'Without active contract' AS Value UNION ALL
    SELECT N'hr_without_contract', N'vi-VN', N'Chưa có HĐ hiệu lực' UNION ALL
    SELECT N'hr_without_contract', N'zh-CN', N'无有效合同' UNION ALL
    SELECT N'hr_work_email' AS ResourceKey, N'en-US' AS Culture, N'Work email' AS Value UNION ALL
    SELECT N'hr_work_email', N'vi-VN', N'Email công việc' UNION ALL
    SELECT N'hr_work_email', N'zh-CN', N'工作邮箱' UNION ALL
    SELECT N'hr_year' AS ResourceKey, N'en-US' AS Culture, N'Year' AS Value UNION ALL
    SELECT N'hr_year', N'vi-VN', N'Năm' UNION ALL
    SELECT N'hr_year', N'zh-CN', N'年'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Khóa dùng chung: chỉ thêm nếu thiếu
;WITH src AS (
    SELECT N'NoPermission_Add' AS ResourceKey, N'en-US' AS Culture, N'You do not have add permission' AS Value UNION ALL
    SELECT N'NoPermission_Add', N'vi-VN', N'Bạn không có quyền thêm' UNION ALL
    SELECT N'NoPermission_Add', N'zh-CN', N'您没有新增权限' UNION ALL
    SELECT N'NoPermission_Delete' AS ResourceKey, N'en-US' AS Culture, N'You do not have delete permission' AS Value UNION ALL
    SELECT N'NoPermission_Delete', N'vi-VN', N'Bạn không có quyền xóa' UNION ALL
    SELECT N'NoPermission_Delete', N'zh-CN', N'您没有删除权限' UNION ALL
    SELECT N'NoPermission_Edit' AS ResourceKey, N'en-US' AS Culture, N'You do not have edit permission' AS Value UNION ALL
    SELECT N'NoPermission_Edit', N'vi-VN', N'Bạn không có quyền sửa' UNION ALL
    SELECT N'NoPermission_Edit', N'zh-CN', N'您没有编辑权限' UNION ALL
    SELECT N'NoPermission_View' AS ResourceKey, N'en-US' AS Culture, N'You do not have view permission' AS Value UNION ALL
    SELECT N'NoPermission_View', N'vi-VN', N'Bạn không có quyền xem' UNION ALL
    SELECT N'NoPermission_View', N'zh-CN', N'您没有查看权限' UNION ALL
    SELECT N'cancel' AS ResourceKey, N'en-US' AS Culture, N'Cancel' AS Value UNION ALL
    SELECT N'cancel', N'vi-VN', N'Hủy' UNION ALL
    SELECT N'cancel', N'zh-CN', N'取消' UNION ALL
    SELECT N'delete' AS ResourceKey, N'en-US' AS Culture, N'Delete' AS Value UNION ALL
    SELECT N'delete', N'vi-VN', N'Xóa' UNION ALL
    SELECT N'delete', N'zh-CN', N'删除' UNION ALL
    SELECT N'edit' AS ResourceKey, N'en-US' AS Culture, N'Edit' AS Value UNION ALL
    SELECT N'edit', N'vi-VN', N'Sửa' UNION ALL
    SELECT N'edit', N'zh-CN', N'编辑' UNION ALL
    SELECT N'new' AS ResourceKey, N'en-US' AS Culture, N'New' AS Value UNION ALL
    SELECT N'new', N'vi-VN', N'Thêm mới' UNION ALL
    SELECT N'new', N'zh-CN', N'新增' UNION ALL
    SELECT N'reload' AS ResourceKey, N'en-US' AS Culture, N'Reload' AS Value UNION ALL
    SELECT N'reload', N'vi-VN', N'Tải lại' UNION ALL
    SELECT N'reload', N'zh-CN', N'刷新' UNION ALL
    SELECT N'save' AS ResourceKey, N'en-US' AS Culture, N'Save' AS Value UNION ALL
    SELECT N'save', N'vi-VN', N'Lưu' UNION ALL
    SELECT N'save', N'zh-CN', N'保存'
)
INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value)
SELECT src.ResourceKey, src.Culture, src.Value
FROM src
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.LocalizationResources t
    WHERE t.ResourceKey = src.ResourceKey AND t.Culture = src.Culture
);

-- Bổ sung: sáng T7 tính 1 công, công chuẩn cố định
;WITH src AS (
    SELECT N'hr_set_saturday_full_credit' AS ResourceKey, N'en-US' AS Culture, N'Count Saturday morning as 1 full day' AS Value UNION ALL
    SELECT N'hr_set_saturday_full_credit', N'vi-VN', N'Tính buổi sáng T7 là 1 công' UNION ALL
    SELECT N'hr_set_saturday_full_credit', N'zh-CN', N'周六上午按 1 天计' UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint' AS ResourceKey, N'en-US' AS Culture, N'For Mon – Sat morning schedules paid on ~26 days/month. Saturday morning worked = 1 day, leave on Saturday morning = 1 day.' AS Value UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint', N'vi-VN', N'Dùng khi làm T2 – sáng T7, công chuẩn ~26 công/tháng. Đi làm sáng T7 = 1 công, nghỉ phép sáng T7 = trừ 1 ngày phép.' UNION ALL
    SELECT N'hr_set_saturday_full_credit_hint', N'zh-CN', N'适用于周一至周六上午、每月约 26 个工日。周六上午出勤 = 1 天，周六上午请假 = 1 天。' UNION ALL
    SELECT N'hr_set_fixed_standard' AS ResourceKey, N'en-US' AS Culture, N'Fixed standard days for payroll' AS Value UNION ALL
    SELECT N'hr_set_fixed_standard', N'vi-VN', N'Công chuẩn cố định khi tính lương' UNION ALL
    SELECT N'hr_set_fixed_standard', N'zh-CN', N'计薪固定标准工日' UNION ALL
    SELECT N'hr_set_fixed_standard_hint' AS ResourceKey, N'en-US' AS Culture, N'0 = actual working days of each month. E.g. 24: full attendance = full salary, each missing day deducts Salary / 24.' AS Value UNION ALL
    SELECT N'hr_set_fixed_standard_hint', N'vi-VN', N'0 = theo lịch thực tế từng tháng. Vd 24: đi làm đủ = đủ lương, mỗi ngày nghỉ không lương trừ Lương / 24.' UNION ALL
    SELECT N'hr_set_fixed_standard_hint', N'zh-CN', N'0 = 按每月实际工作日。例如 24：全勤 = 全薪，每缺勤一天扣 工资 / 24。' UNION ALL
    SELECT N'hr_err_fixed_standard_days' AS ResourceKey, N'en-US' AS Culture, N'Fixed standard days must be between 0 and 31' AS Value UNION ALL
    SELECT N'hr_err_fixed_standard_days', N'vi-VN', N'Công chuẩn cố định phải từ 0 đến 31' UNION ALL
    SELECT N'hr_err_fixed_standard_days', N'zh-CN', N'固定标准工日须在 0 到 31 之间'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Bổ sung: chấm công hằng ngày (12.10)
;WITH src AS (
    SELECT N'hr_attendance' AS ResourceKey, N'en-US' AS Culture, N'Daily attendance' AS Value UNION ALL
    SELECT N'hr_attendance', N'vi-VN', N'Chấm công hằng ngày' UNION ALL
    SELECT N'hr_attendance', N'zh-CN', N'每日考勤' UNION ALL
    SELECT N'hr_attendance_subtitle' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out by day, late arrivals, HR corrections' AS Value UNION ALL
    SELECT N'hr_attendance_subtitle', N'vi-VN', N'Giờ vào / ra từng ngày, đi muộn, HR chấm bù' UNION ALL
    SELECT N'hr_attendance_subtitle', N'zh-CN', N'每日上下班打卡、迟到、人事补卡' UNION ALL
    SELECT N'hr_att_tab_daily' AS ResourceKey, N'en-US' AS Culture, N'By day' AS Value UNION ALL
    SELECT N'hr_att_tab_daily', N'vi-VN', N'Theo ngày' UNION ALL
    SELECT N'hr_att_tab_daily', N'zh-CN', N'按日' UNION ALL
    SELECT N'hr_att_tab_mine' AS ResourceKey, N'en-US' AS Culture, N'My attendance' AS Value UNION ALL
    SELECT N'hr_att_tab_mine', N'vi-VN', N'Công của tôi' UNION ALL
    SELECT N'hr_att_tab_mine', N'zh-CN', N'我的考勤' UNION ALL
    SELECT N'hr_att_today' AS ResourceKey, N'en-US' AS Culture, N'Today' AS Value UNION ALL
    SELECT N'hr_att_today', N'vi-VN', N'Hôm nay' UNION ALL
    SELECT N'hr_att_today', N'zh-CN', N'今天' UNION ALL
    SELECT N'hr_att_code' AS ResourceKey, N'en-US' AS Culture, N'Timesheet' AS Value UNION ALL
    SELECT N'hr_att_code', N'vi-VN', N'Công' UNION ALL
    SELECT N'hr_att_code', N'zh-CN', N'考勤' UNION ALL
    SELECT N'hr_att_first_in' AS ResourceKey, N'en-US' AS Culture, N'In' AS Value UNION ALL
    SELECT N'hr_att_first_in', N'vi-VN', N'Giờ vào' UNION ALL
    SELECT N'hr_att_first_in', N'zh-CN', N'上班' UNION ALL
    SELECT N'hr_att_last_out' AS ResourceKey, N'en-US' AS Culture, N'Out' AS Value UNION ALL
    SELECT N'hr_att_last_out', N'vi-VN', N'Giờ ra' UNION ALL
    SELECT N'hr_att_last_out', N'zh-CN', N'下班' UNION ALL
    SELECT N'hr_att_late' AS ResourceKey, N'en-US' AS Culture, N'Late (min)' AS Value UNION ALL
    SELECT N'hr_att_late', N'vi-VN', N'Muộn (phút)' UNION ALL
    SELECT N'hr_att_late', N'zh-CN', N'迟到(分)' UNION ALL
    SELECT N'hr_att_early' AS ResourceKey, N'en-US' AS Culture, N'Early leave (min)' AS Value UNION ALL
    SELECT N'hr_att_early', N'vi-VN', N'Về sớm (phút)' UNION ALL
    SELECT N'hr_att_early', N'zh-CN', N'早退(分)' UNION ALL
    SELECT N'hr_att_status' AS ResourceKey, N'en-US' AS Culture, N'Status' AS Value UNION ALL
    SELECT N'hr_att_status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'hr_att_status', N'zh-CN', N'状态' UNION ALL
    SELECT N'hr_att_source' AS ResourceKey, N'en-US' AS Culture, N'Source' AS Value UNION ALL
    SELECT N'hr_att_source', N'vi-VN', N'Nguồn' UNION ALL
    SELECT N'hr_att_source', N'zh-CN', N'来源' UNION ALL
    SELECT N'hr_att_time' AS ResourceKey, N'en-US' AS Culture, N'Time' AS Value UNION ALL
    SELECT N'hr_att_time', N'vi-VN', N'Giờ' UNION ALL
    SELECT N'hr_att_time', N'zh-CN', N'时间' UNION ALL
    SELECT N'hr_att_type' AS ResourceKey, N'en-US' AS Culture, N'Type' AS Value UNION ALL
    SELECT N'hr_att_type', N'vi-VN', N'Loại' UNION ALL
    SELECT N'hr_att_type', N'zh-CN', N'类型' UNION ALL
    SELECT N'hr_att_session' AS ResourceKey, N'en-US' AS Culture, N'Session' AS Value UNION ALL
    SELECT N'hr_att_session', N'vi-VN', N'Buổi' UNION ALL
    SELECT N'hr_att_session', N'zh-CN', N'时段' UNION ALL
    SELECT N'hr_att_place' AS ResourceKey, N'en-US' AS Culture, N'Location' AS Value UNION ALL
    SELECT N'hr_att_place', N'vi-VN', N'Vị trí' UNION ALL
    SELECT N'hr_att_place', N'zh-CN', N'位置' UNION ALL
    SELECT N'hr_att_in' AS ResourceKey, N'en-US' AS Culture, N'Check-in' AS Value UNION ALL
    SELECT N'hr_att_in', N'vi-VN', N'Vào' UNION ALL
    SELECT N'hr_att_in', N'zh-CN', N'上班' UNION ALL
    SELECT N'hr_att_out' AS ResourceKey, N'en-US' AS Culture, N'Check-out' AS Value UNION ALL
    SELECT N'hr_att_out', N'vi-VN', N'Ra' UNION ALL
    SELECT N'hr_att_out', N'zh-CN', N'下班' UNION ALL
    SELECT N'hr_att_by_session' AS ResourceKey, N'en-US' AS Culture, N'By session' AS Value UNION ALL
    SELECT N'hr_att_by_session', N'vi-VN', N'Theo buổi' UNION ALL
    SELECT N'hr_att_by_session', N'zh-CN', N'按时段' UNION ALL
    SELECT N'hr_att_morning' AS ResourceKey, N'en-US' AS Culture, N'Morning' AS Value UNION ALL
    SELECT N'hr_att_morning', N'vi-VN', N'Sáng' UNION ALL
    SELECT N'hr_att_morning', N'zh-CN', N'上午' UNION ALL
    SELECT N'hr_att_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Afternoon' AS Value UNION ALL
    SELECT N'hr_att_afternoon', N'vi-VN', N'Chiều' UNION ALL
    SELECT N'hr_att_afternoon', N'zh-CN', N'下午' UNION ALL
    SELECT N'hr_att_onsite' AS ResourceKey, N'en-US' AS Culture, N'At office' AS Value UNION ALL
    SELECT N'hr_att_onsite', N'vi-VN', N'Tại văn phòng' UNION ALL
    SELECT N'hr_att_onsite', N'zh-CN', N'在办公室' UNION ALL
    SELECT N'hr_att_remote' AS ResourceKey, N'en-US' AS Culture, N'Remote' AS Value UNION ALL
    SELECT N'hr_att_remote', N'vi-VN', N'Từ xa' UNION ALL
    SELECT N'hr_att_remote', N'zh-CN', N'远程' UNION ALL
    SELECT N'hr_att_src_web' AS ResourceKey, N'en-US' AS Culture, N'Web' AS Value UNION ALL
    SELECT N'hr_att_src_web', N'vi-VN', N'Web' UNION ALL
    SELECT N'hr_att_src_web', N'zh-CN', N'网页' UNION ALL
    SELECT N'hr_att_src_mobile' AS ResourceKey, N'en-US' AS Culture, N'Mobile' AS Value UNION ALL
    SELECT N'hr_att_src_mobile', N'vi-VN', N'Điện thoại' UNION ALL
    SELECT N'hr_att_src_mobile', N'zh-CN', N'手机' UNION ALL
    SELECT N'hr_att_src_manual' AS ResourceKey, N'en-US' AS Culture, N'HR correction' AS Value UNION ALL
    SELECT N'hr_att_src_manual', N'vi-VN', N'HR chấm bù' UNION ALL
    SELECT N'hr_att_src_manual', N'zh-CN', N'人事补卡' UNION ALL
    SELECT N'hr_att_punches' AS ResourceKey, N'en-US' AS Culture, N'Attendance records' AS Value UNION ALL
    SELECT N'hr_att_punches', N'vi-VN', N'Các lần chấm công' UNION ALL
    SELECT N'hr_att_punches', N'zh-CN', N'打卡记录' UNION ALL
    SELECT N'hr_att_no_punch' AS ResourceKey, N'en-US' AS Culture, N'No records' AS Value UNION ALL
    SELECT N'hr_att_no_punch', N'vi-VN', N'Chưa có lần chấm nào' UNION ALL
    SELECT N'hr_att_no_punch', N'zh-CN', N'无打卡记录' UNION ALL
    SELECT N'hr_att_manual' AS ResourceKey, N'en-US' AS Culture, N'HR correction' AS Value UNION ALL
    SELECT N'hr_att_manual', N'vi-VN', N'HR chấm bù / sửa công' UNION ALL
    SELECT N'hr_att_manual', N'zh-CN', N'人事补卡 / 修改' UNION ALL
    SELECT N'hr_att_manual_reason' AS ResourceKey, N'en-US' AS Culture, N'Reason (required)' AS Value UNION ALL
    SELECT N'hr_att_manual_reason', N'vi-VN', N'Lý do (bắt buộc)' UNION ALL
    SELECT N'hr_att_manual_reason', N'zh-CN', N'原因（必填）' UNION ALL
    SELECT N'hr_att_manual_reason_hint' AS ResourceKey, N'en-US' AS Culture, N'E.g. forgot to check in, business trip, device error' AS Value UNION ALL
    SELECT N'hr_att_manual_reason_hint', N'vi-VN', N'Vd: quên chấm, đi công tác, lỗi mạng' UNION ALL
    SELECT N'hr_att_manual_reason_hint', N'zh-CN', N'例如：忘记打卡、出差、设备故障' UNION ALL
    SELECT N'hr_att_preset_full' AS ResourceKey, N'en-US' AS Culture, N'Full day' AS Value UNION ALL
    SELECT N'hr_att_preset_full', N'vi-VN', N'Bù cả ngày' UNION ALL
    SELECT N'hr_att_preset_full', N'zh-CN', N'补全天' UNION ALL
    SELECT N'hr_att_preset_morning' AS ResourceKey, N'en-US' AS Culture, N'Morning' AS Value UNION ALL
    SELECT N'hr_att_preset_morning', N'vi-VN', N'Bù buổi sáng' UNION ALL
    SELECT N'hr_att_preset_morning', N'zh-CN', N'补上午' UNION ALL
    SELECT N'hr_att_preset_afternoon' AS ResourceKey, N'en-US' AS Culture, N'Afternoon' AS Value UNION ALL
    SELECT N'hr_att_preset_afternoon', N'vi-VN', N'Bù buổi chiều' UNION ALL
    SELECT N'hr_att_preset_afternoon', N'zh-CN', N'补下午' UNION ALL
    SELECT N'hr_att_add_punch' AS ResourceKey, N'en-US' AS Culture, N'Add' AS Value UNION ALL
    SELECT N'hr_att_add_punch', N'vi-VN', N'Thêm lần chấm' UNION ALL
    SELECT N'hr_att_add_punch', N'zh-CN', N'添加' UNION ALL
    SELECT N'hr_att_manual_hint' AS ResourceKey, N'en-US' AS Culture, N'Corrections are saved with your name and reason. Only corrections can be deleted; employees'' own records are kept. Months with a locked payroll cannot be changed.' AS Value UNION ALL
    SELECT N'hr_att_manual_hint', N'vi-VN', N'Lần chấm bù được lưu kèm tên người sửa và lý do. Chỉ xóa được lần chấm bù; lần chấm thật của nhân viên được giữ nguyên. Tháng đã chốt lương không sửa được.' UNION ALL
    SELECT N'hr_att_manual_hint', N'zh-CN', N'补卡记录会保存修改人和原因。只能删除补卡记录；员工本人的打卡记录保留。已锁定工资的月份不能修改。' UNION ALL
    SELECT N'hr_att_click_hint' AS ResourceKey, N'en-US' AS Culture, N'Click a row to see each check-in / check-out.' AS Value UNION ALL
    SELECT N'hr_att_click_hint', N'vi-VN', N'Bấm vào 1 dòng để xem các lần chấm công.' UNION ALL
    SELECT N'hr_att_click_hint', N'zh-CN', N'点击一行查看打卡记录。' UNION ALL
    SELECT N'hr_att_click_hint_edit' AS ResourceKey, N'en-US' AS Culture, N'Click a row to see records and add corrections.' AS Value UNION ALL
    SELECT N'hr_att_click_hint_edit', N'vi-VN', N'Bấm vào 1 dòng để xem các lần chấm và chấm bù.' UNION ALL
    SELECT N'hr_att_click_hint_edit', N'zh-CN', N'点击一行查看记录并补卡。' UNION ALL
    SELECT N'hr_att_mine_summary' AS ResourceKey, N'en-US' AS Culture, N'Late {0} times ({1} min) · Early leave {2} times · Absent / missing check-out {3} days' AS Value UNION ALL
    SELECT N'hr_att_mine_summary', N'vi-VN', N'Đi muộn {0} lần ({1} phút) · Về sớm {2} lần · Vắng / quên chấm ra {3} ngày' UNION ALL
    SELECT N'hr_att_mine_summary', N'zh-CN', N'迟到 {0} 次（{1} 分钟）· 早退 {2} 次 · 缺勤 / 漏打下班卡 {3} 天' UNION ALL
    SELECT N'hr_att_mine_hint' AS ResourceKey, N'en-US' AS Culture, N'Wrong or missing record? Contact HR for a correction.' AS Value UNION ALL
    SELECT N'hr_att_mine_hint', N'vi-VN', N'Thiếu hoặc sai công? Liên hệ HR để chấm bù.' UNION ALL
    SELECT N'hr_att_mine_hint', N'zh-CN', N'记录缺失或有误？请联系人事补卡。' UNION ALL
    SELECT N'hr_att_no_profile' AS ResourceKey, N'en-US' AS Culture, N'Your account is not linked to an employee profile.' AS Value UNION ALL
    SELECT N'hr_att_no_profile', N'vi-VN', N'Tài khoản của bạn chưa gắn với hồ sơ nhân viên.' UNION ALL
    SELECT N'hr_att_no_profile', N'zh-CN', N'您的账号尚未关联员工档案。' UNION ALL
    SELECT N'hr_att_st_present' AS ResourceKey, N'en-US' AS Culture, N'Present' AS Value UNION ALL
    SELECT N'hr_att_st_present', N'vi-VN', N'Có mặt' UNION ALL
    SELECT N'hr_att_st_present', N'zh-CN', N'出勤' UNION ALL
    SELECT N'hr_att_st_late' AS ResourceKey, N'en-US' AS Culture, N'Late' AS Value UNION ALL
    SELECT N'hr_att_st_late', N'vi-VN', N'Đi muộn' UNION ALL
    SELECT N'hr_att_st_late', N'zh-CN', N'迟到' UNION ALL
    SELECT N'hr_att_st_missing_out' AS ResourceKey, N'en-US' AS Culture, N'No check-out' AS Value UNION ALL
    SELECT N'hr_att_st_missing_out', N'vi-VN', N'Quên chấm ra' UNION ALL
    SELECT N'hr_att_st_missing_out', N'zh-CN', N'漏打下班卡' UNION ALL
    SELECT N'hr_att_st_not_yet' AS ResourceKey, N'en-US' AS Culture, N'Not yet' AS Value UNION ALL
    SELECT N'hr_att_st_not_yet', N'vi-VN', N'Chưa chấm' UNION ALL
    SELECT N'hr_att_st_not_yet', N'zh-CN', N'未打卡' UNION ALL
    SELECT N'hr_att_st_leave' AS ResourceKey, N'en-US' AS Culture, N'On leave' AS Value UNION ALL
    SELECT N'hr_att_st_leave', N'vi-VN', N'Nghỉ phép' UNION ALL
    SELECT N'hr_att_st_leave', N'zh-CN', N'请假' UNION ALL
    SELECT N'hr_att_st_absent' AS ResourceKey, N'en-US' AS Culture, N'Absent' AS Value UNION ALL
    SELECT N'hr_att_st_absent', N'vi-VN', N'Vắng' UNION ALL
    SELECT N'hr_att_st_absent', N'zh-CN', N'缺勤' UNION ALL
    SELECT N'hr_att_st_off' AS ResourceKey, N'en-US' AS Culture, N'Day off' AS Value UNION ALL
    SELECT N'hr_att_st_off', N'vi-VN', N'Ngày nghỉ' UNION ALL
    SELECT N'hr_att_st_off', N'zh-CN', N'休息日' UNION ALL
    SELECT N'hr_att_st_holiday' AS ResourceKey, N'en-US' AS Culture, N'Holiday' AS Value UNION ALL
    SELECT N'hr_att_st_holiday', N'vi-VN', N'Nghỉ lễ' UNION ALL
    SELECT N'hr_att_st_holiday', N'zh-CN', N'节假日' UNION ALL
    SELECT N'hr_att_st_not_employed' AS ResourceKey, N'en-US' AS Culture, N'Not employed' AS Value UNION ALL
    SELECT N'hr_att_st_not_employed', N'vi-VN', N'Chưa vào làm / đã nghỉ' UNION ALL
    SELECT N'hr_att_st_not_employed', N'zh-CN', N'未在职' UNION ALL
    SELECT N'hr_att_st_no_account' AS ResourceKey, N'en-US' AS Culture, N'No login account' AS Value UNION ALL
    SELECT N'hr_att_st_no_account', N'vi-VN', N'Chưa có tài khoản' UNION ALL
    SELECT N'hr_att_st_no_account', N'zh-CN', N'无账号' UNION ALL
    SELECT N'hr_att_sum_total' AS ResourceKey, N'en-US' AS Culture, N'Employees' AS Value UNION ALL
    SELECT N'hr_att_sum_total', N'vi-VN', N'Nhân viên' UNION ALL
    SELECT N'hr_att_sum_total', N'zh-CN', N'员工' UNION ALL
    SELECT N'hr_att_sum_present' AS ResourceKey, N'en-US' AS Culture, N'Present' AS Value UNION ALL
    SELECT N'hr_att_sum_present', N'vi-VN', N'Có mặt' UNION ALL
    SELECT N'hr_att_sum_present', N'zh-CN', N'出勤' UNION ALL
    SELECT N'hr_att_sum_late' AS ResourceKey, N'en-US' AS Culture, N'Late' AS Value UNION ALL
    SELECT N'hr_att_sum_late', N'vi-VN', N'Đi muộn' UNION ALL
    SELECT N'hr_att_sum_late', N'zh-CN', N'迟到' UNION ALL
    SELECT N'hr_att_sum_not_yet' AS ResourceKey, N'en-US' AS Culture, N'Not yet' AS Value UNION ALL
    SELECT N'hr_att_sum_not_yet', N'vi-VN', N'Chưa chấm' UNION ALL
    SELECT N'hr_att_sum_not_yet', N'zh-CN', N'未打卡' UNION ALL
    SELECT N'hr_att_sum_leave' AS ResourceKey, N'en-US' AS Culture, N'Leave' AS Value UNION ALL
    SELECT N'hr_att_sum_leave', N'vi-VN', N'Nghỉ phép' UNION ALL
    SELECT N'hr_att_sum_leave', N'zh-CN', N'请假' UNION ALL
    SELECT N'hr_att_sum_absent' AS ResourceKey, N'en-US' AS Culture, N'Absent' AS Value UNION ALL
    SELECT N'hr_att_sum_absent', N'vi-VN', N'Vắng' UNION ALL
    SELECT N'hr_att_sum_absent', N'zh-CN', N'缺勤' UNION ALL
    SELECT N'hr_att_sum_missing_out' AS ResourceKey, N'en-US' AS Culture, N'No check-out' AS Value UNION ALL
    SELECT N'hr_att_sum_missing_out', N'vi-VN', N'Quên chấm ra' UNION ALL
    SELECT N'hr_att_sum_missing_out', N'zh-CN', N'漏打下班卡' UNION ALL
    SELECT N'hr_att_sum_remote' AS ResourceKey, N'en-US' AS Culture, N'Remote' AS Value UNION ALL
    SELECT N'hr_att_sum_remote', N'vi-VN', N'Từ xa' UNION ALL
    SELECT N'hr_att_sum_remote', N'zh-CN', N'远程' UNION ALL
    SELECT N'hr_att_err_note_required' AS ResourceKey, N'en-US' AS Culture, N'Please enter a reason' AS Value UNION ALL
    SELECT N'hr_att_err_note_required', N'vi-VN', N'Vui lòng nhập lý do chấm bù' UNION ALL
    SELECT N'hr_att_err_note_required', N'zh-CN', N'请输入补卡原因' UNION ALL
    SELECT N'hr_att_err_time_required' AS ResourceKey, N'en-US' AS Culture, N'Please choose the date and time' AS Value UNION ALL
    SELECT N'hr_att_err_time_required', N'vi-VN', N'Vui lòng chọn ngày và giờ' UNION ALL
    SELECT N'hr_att_err_time_required', N'zh-CN', N'请选择日期和时间' UNION ALL
    SELECT N'hr_att_err_future' AS ResourceKey, N'en-US' AS Culture, N'Cannot add a record in the future' AS Value UNION ALL
    SELECT N'hr_att_err_future', N'vi-VN', N'Không chấm bù cho thời điểm trong tương lai' UNION ALL
    SELECT N'hr_att_err_future', N'zh-CN', N'不能补未来的打卡' UNION ALL
    SELECT N'hr_att_err_only_manual' AS ResourceKey, N'en-US' AS Culture, N'Only HR corrections can be deleted' AS Value UNION ALL
    SELECT N'hr_att_err_only_manual', N'vi-VN', N'Chỉ xóa được lần chấm bù của HR' UNION ALL
    SELECT N'hr_att_err_only_manual', N'zh-CN', N'只能删除人事补卡记录' UNION ALL
    SELECT N'hr_att_err_period_locked' AS ResourceKey, N'en-US' AS Culture, N'Payroll for this month is locked. Unlock it before changing attendance.' AS Value UNION ALL
    SELECT N'hr_att_err_period_locked', N'vi-VN', N'Tháng này đã chốt lương. Mở chốt bảng lương trước khi sửa công.' UNION ALL
    SELECT N'hr_att_err_period_locked', N'zh-CN', N'本月工资已锁定，请先解锁再修改考勤。' UNION ALL
    SELECT N'hr_set_att_mode' AS ResourceKey, N'en-US' AS Culture, N'Attendance mode' AS Value UNION ALL
    SELECT N'hr_set_att_mode', N'vi-VN', N'Chế độ chấm công' UNION ALL
    SELECT N'hr_set_att_mode', N'zh-CN', N'考勤模式' UNION ALL
    SELECT N'hr_set_att_mode_session' AS ResourceKey, N'en-US' AS Culture, N'By session (morning 1h window, afternoon 1h window)' AS Value UNION ALL
    SELECT N'hr_set_att_mode_session', N'vi-VN', N'Theo buổi (khung 1 giờ đầu buổi sáng / chiều)' UNION ALL
    SELECT N'hr_set_att_mode_session', N'zh-CN', N'按时段（上午/下午开始后1小时内）' UNION ALL
    SELECT N'hr_set_att_mode_inout' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out at any time' AS Value UNION ALL
    SELECT N'hr_set_att_mode_inout', N'vi-VN', N'Giờ vào – giờ ra (chấm bất kỳ lúc nào)' UNION ALL
    SELECT N'hr_set_att_mode_inout', N'zh-CN', N'上下班打卡（任意时间）' UNION ALL
    SELECT N'hr_set_att_mode_hint' AS ResourceKey, N'en-US' AS Culture, N'Check-in / check-out: 8:00 in + 17:00 out counts both sessions; late / early minutes are tracked.' AS Value UNION ALL
    SELECT N'hr_set_att_mode_hint', N'vi-VN', N'Giờ vào – giờ ra: vào 8:00 + ra 17:00 = đủ 2 buổi; tính số phút đi muộn / về sớm.' UNION ALL
    SELECT N'hr_set_att_mode_hint', N'zh-CN', N'上下班打卡：8:00上班 + 17:00下班 = 两个时段；统计迟到/早退分钟。' UNION ALL
    SELECT N'hr_set_work_start' AS ResourceKey, N'en-US' AS Culture, N'Start' AS Value UNION ALL
    SELECT N'hr_set_work_start', N'vi-VN', N'Giờ vào làm' UNION ALL
    SELECT N'hr_set_work_start', N'zh-CN', N'上班时间' UNION ALL
    SELECT N'hr_set_lunch_start' AS ResourceKey, N'en-US' AS Culture, N'Lunch start' AS Value UNION ALL
    SELECT N'hr_set_lunch_start', N'vi-VN', N'Nghỉ trưa từ' UNION ALL
    SELECT N'hr_set_lunch_start', N'zh-CN', N'午休开始' UNION ALL
    SELECT N'hr_set_lunch_end' AS ResourceKey, N'en-US' AS Culture, N'Lunch end' AS Value UNION ALL
    SELECT N'hr_set_lunch_end', N'vi-VN', N'Làm chiều từ' UNION ALL
    SELECT N'hr_set_lunch_end', N'zh-CN', N'午休结束' UNION ALL
    SELECT N'hr_set_work_end' AS ResourceKey, N'en-US' AS Culture, N'End' AS Value UNION ALL
    SELECT N'hr_set_work_end', N'vi-VN', N'Giờ tan làm' UNION ALL
    SELECT N'hr_set_work_end', N'zh-CN', N'下班时间' UNION ALL
    SELECT N'hr_set_saturday_end' AS ResourceKey, N'en-US' AS Culture, N'Saturday end' AS Value UNION ALL
    SELECT N'hr_set_saturday_end', N'vi-VN', N'Tan làm T7' UNION ALL
    SELECT N'hr_set_saturday_end', N'zh-CN', N'周六下班' UNION ALL
    SELECT N'hr_set_late_grace' AS ResourceKey, N'en-US' AS Culture, N'Grace (min)' AS Value UNION ALL
    SELECT N'hr_set_late_grace', N'vi-VN', N'Cho phép muộn (phút)' UNION ALL
    SELECT N'hr_set_late_grace', N'zh-CN', N'宽限(分)' UNION ALL
    SELECT N'hr_set_mobile' AS ResourceKey, N'en-US' AS Culture, N'Mobile attendance' AS Value UNION ALL
    SELECT N'hr_set_mobile', N'vi-VN', N'Chấm công trên điện thoại' UNION ALL
    SELECT N'hr_set_mobile', N'zh-CN', N'手机打卡' UNION ALL
    SELECT N'hr_set_office_lat' AS ResourceKey, N'en-US' AS Culture, N'Office latitude' AS Value UNION ALL
    SELECT N'hr_set_office_lat', N'vi-VN', N'Vĩ độ văn phòng' UNION ALL
    SELECT N'hr_set_office_lat', N'zh-CN', N'办公室纬度' UNION ALL
    SELECT N'hr_set_office_lng' AS ResourceKey, N'en-US' AS Culture, N'Office longitude' AS Value UNION ALL
    SELECT N'hr_set_office_lng', N'vi-VN', N'Kinh độ văn phòng' UNION ALL
    SELECT N'hr_set_office_lng', N'zh-CN', N'办公室经度' UNION ALL
    SELECT N'hr_set_office_radius' AS ResourceKey, N'en-US' AS Culture, N'Radius (m)' AS Value UNION ALL
    SELECT N'hr_set_office_radius', N'vi-VN', N'Bán kính (m)' UNION ALL
    SELECT N'hr_set_office_radius', N'zh-CN', N'半径(米)' UNION ALL
    SELECT N'hr_set_office_hint' AS ResourceKey, N'en-US' AS Culture, N'Get coordinates from Google Maps (right-click the office). Phones on the office WiFi are also counted as at the office (IP list in company info). Leave empty to disable GPS.' AS Value UNION ALL
    SELECT N'hr_set_office_hint', N'vi-VN', N'Lấy tọa độ từ Google Maps (bấm chuột phải vào văn phòng). Điện thoại dùng WiFi văn phòng cũng được tính là tại văn phòng (theo danh sách IP trong thông tin công ty). Để trống = không dùng GPS.' UNION ALL
    SELECT N'hr_set_office_hint', N'zh-CN', N'在谷歌地图右键办公室获取坐标。连接办公室WiFi的手机也算在办公室（公司信息中的IP列表）。留空 = 不使用GPS。' UNION ALL
    SELECT N'hr_set_mobile_require_onsite' AS ResourceKey, N'en-US' AS Culture, N'Phones can only check in at the office' AS Value UNION ALL
    SELECT N'hr_set_mobile_require_onsite', N'vi-VN', N'Điện thoại chỉ chấm được khi ở văn phòng' UNION ALL
    SELECT N'hr_set_mobile_require_onsite', N'zh-CN', N'手机只能在办公室打卡' UNION ALL
    SELECT N'hr_err_work_hours' AS ResourceKey, N'en-US' AS Culture, N'Working hours are invalid (start < lunch start ≤ lunch end < end)' AS Value UNION ALL
    SELECT N'hr_err_work_hours', N'vi-VN', N'Giờ làm không hợp lệ (vào < nghỉ trưa ≤ làm chiều < tan làm)' UNION ALL
    SELECT N'hr_err_work_hours', N'zh-CN', N'工作时间无效' UNION ALL
    SELECT N'hr_err_office_location' AS ResourceKey, N'en-US' AS Culture, N'Office coordinates are invalid (enter both latitude and longitude)' AS Value UNION ALL
    SELECT N'hr_err_office_location', N'vi-VN', N'Tọa độ văn phòng không hợp lệ (nhập đủ vĩ độ và kinh độ)' UNION ALL
    SELECT N'hr_err_office_location', N'zh-CN', N'办公室坐标无效（请同时填写纬度和经度）'
)
MERGE dbo.LocalizationResources AS tgt
USING src
ON tgt.ResourceKey = src.ResourceKey AND tgt.Culture = src.Culture
WHEN MATCHED THEN
    UPDATE SET Value = src.Value
WHEN NOT MATCHED BY TARGET THEN
    INSERT (ResourceKey, Culture, Value)
    VALUES (src.ResourceKey, src.Culture, src.Value);

-- Bổ sung: lương NET / GROSS, tháng có cả thử việc và chính thức (12.3, 12.9)
;WITH src AS (
    SELECT N'hr_net_salary' AS ResourceKey, N'en-US' AS Culture, N'Net salary (amounts are take-home)' AS Value UNION ALL
    SELECT N'hr_net_salary', N'vi-VN', N'Lương NET (lương cơ bản, phụ cấp là số thực nhận)' UNION ALL
    SELECT N'hr_net_salary', N'zh-CN', N'净工资（基本工资、津贴为实得金额）' UNION ALL
    SELECT N'hr_net_salary_hint' AS ResourceKey, N'en-US' AS Culture, N'Company pays employee insurance and PIT on top; payroll converts net to gross automatically.' AS Value UNION ALL
    SELECT N'hr_net_salary_hint', N'vi-VN', N'Công ty chịu BH phần người lao động và thuế TNCN; bảng lương tự quy đổi NET → GROSS.' UNION ALL
    SELECT N'hr_net_salary_hint', N'zh-CN', N'公司承担员工保险和个税；工资表自动由净额换算为税前。' UNION ALL
    SELECT N'hr_gross_salary_hint' AS ResourceKey, N'en-US' AS Culture, N'Gross salary: insurance and PIT are deducted from the amounts above.' AS Value UNION ALL
    SELECT N'hr_gross_salary_hint', N'vi-VN', N'Lương GROSS: BH và thuế TNCN trừ vào số tiền trên.' UNION ALL
    SELECT N'hr_gross_salary_hint', N'zh-CN', N'税前工资：保险和个税从上述金额中扣除。' UNION ALL
    SELECT N'hr_pay_salary_kind' AS ResourceKey, N'en-US' AS Culture, N'Net/Gross' AS Value UNION ALL
    SELECT N'hr_pay_salary_kind', N'vi-VN', N'NET/GROSS' UNION ALL
    SELECT N'hr_pay_salary_kind', N'zh-CN', N'净/税前' UNION ALL
    SELECT N'hr_pay_pro_short' AS ResourceKey, N'en-US' AS Culture, N'Probation / prev. contract' AS Value UNION ALL
    SELECT N'hr_pay_pro_short', N'vi-VN', N'Thử việc / HĐ trước' UNION ALL
    SELECT N'hr_pay_pro_short', N'zh-CN', N'试用/前合同' UNION ALL
    SELECT N'hr_pay_main_short' AS ResourceKey, N'en-US' AS Culture, N'Main contract' AS Value UNION ALL
    SELECT N'hr_pay_main_short', N'vi-VN', N'HĐ chính thức' UNION ALL
    SELECT N'hr_pay_main_short', N'zh-CN', N'正式合同' UNION ALL
    SELECT N'hr_pay_pro_contract' AS ResourceKey, N'en-US' AS Culture, N'Prev. contract (probation)' AS Value UNION ALL
    SELECT N'hr_pay_pro_contract', N'vi-VN', N'HĐ trước trong tháng (thử việc)' UNION ALL
    SELECT N'hr_pay_pro_contract', N'zh-CN', N'本月前一合同（试用）' UNION ALL
    SELECT N'hr_pay_pro_base_salary' AS ResourceKey, N'en-US' AS Culture, N'Probation salary' AS Value UNION ALL
    SELECT N'hr_pay_pro_base_salary', N'vi-VN', N'Lương thử việc' UNION ALL
    SELECT N'hr_pay_pro_base_salary', N'zh-CN', N'试用工资' UNION ALL
    SELECT N'hr_pay_pro_paid_days' AS ResourceKey, N'en-US' AS Culture, N'Probation paid days' AS Value UNION ALL
    SELECT N'hr_pay_pro_paid_days', N'vi-VN', N'Công thử việc' UNION ALL
    SELECT N'hr_pay_pro_paid_days', N'zh-CN', N'试用计薪天数' UNION ALL
    SELECT N'hr_pay_pro_salary_by_days' AS ResourceKey, N'en-US' AS Culture, N'Probation salary + allowance by days' AS Value UNION ALL
    SELECT N'hr_pay_pro_salary_by_days', N'vi-VN', N'Lương + phụ cấp thử việc theo công' UNION ALL
    SELECT N'hr_pay_pro_salary_by_days', N'zh-CN', N'试用期按天工资+津贴' UNION ALL
    SELECT N'hr_pay_pro_tax' AS ResourceKey, N'en-US' AS Culture, N'PIT on probation part' AS Value UNION ALL
    SELECT N'hr_pay_pro_tax', N'vi-VN', N'Thuế TNCN phần thử việc' UNION ALL
    SELECT N'hr_pay_pro_tax', N'zh-CN', N'试用部分个税' UNION ALL
    SELECT N'hr_pay_gross_up' AS ResourceKey, N'en-US' AS Culture, N'Gross-up (paid by company)' AS Value UNION ALL
    SELECT N'hr_pay_gross_up', N'vi-VN', N'Gross-up (công ty chịu thay BH, thuế)' UNION ALL
    SELECT N'hr_pay_gross_up', N'zh-CN', N'税费补贴（公司承担）' UNION ALL
    SELECT N'hr_pay_net_note' AS ResourceKey, N'en-US' AS Culture, N'NET salary: the employee receives the agreed amount; insurance and PIT shown are borne by the company (gross-up).' AS Value UNION ALL
    SELECT N'hr_pay_net_note', N'vi-VN', N'Lương NET: người lao động nhận đúng số thỏa thuận; BH và thuế ở trên do công ty chịu (dòng Gross-up).' UNION ALL
    SELECT N'hr_pay_net_note', N'zh-CN', N'净工资：员工按约定金额领取；上述保险和个税由公司承担（税费补贴）。' UNION ALL
    SELECT N'hr_pay_net_input_hint' AS ResourceKey, N'en-US' AS Culture, N'NET contract: overtime, bonus and other income entered here are take-home amounts; tax on them is also grossed up.' AS Value UNION ALL
    SELECT N'hr_pay_net_input_hint', N'vi-VN', N'HĐ lương NET: làm thêm, thưởng, thu nhập khác nhập ở đây là số thực nhận — thuế của các khoản này cũng được quy đổi (công ty chịu).' UNION ALL
    SELECT N'hr_pay_net_input_hint', N'zh-CN', N'净工资合同：此处加班、奖金、其他收入为实得金额，其税费也由公司承担。' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation' AS ResourceKey, N'en-US' AS Culture, N'Probation/unpaid days in the month reach the threshold — insurance starts next month' AS Value UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation', N'vi-VN', N'Số ngày thử việc + nghỉ không lương trong tháng từ 14 ngày — chưa đóng BH tháng này (đóng từ tháng sau)' UNION ALL
    SELECT N'hr_pay_warn_no_insurance_probation', N'zh-CN', N'试用+无薪天数达14天——本月不缴保险，下月开始' UNION ALL
    SELECT N'hr_pay_warn_many_contracts' AS ResourceKey, N'en-US' AS Culture, N'More than 2 contracts in the month — only the latest 2 are used' AS Value UNION ALL
    SELECT N'hr_pay_warn_many_contracts', N'vi-VN', N'Trong tháng có hơn 2 hợp đồng — chỉ tính 2 hợp đồng gần nhất' UNION ALL
    SELECT N'hr_pay_warn_many_contracts', N'zh-CN', N'本月合同超过2份——仅计算最近2份' UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary' AS ResourceKey, N'en-US' AS Culture, N'NET contract without insurance salary — insurance is calculated on the net salary' AS Value UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary', N'vi-VN', N'HĐ lương NET chưa nhập lương đóng BH — BH đang tính trên lương NET (nên nhập lương đóng BH)' UNION ALL
    SELECT N'hr_pay_warn_net_no_insurance_salary', N'zh-CN', N'净工资合同未填写社保工资——保险按净工资计算'
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

PRINT N'HR localization imported.';
