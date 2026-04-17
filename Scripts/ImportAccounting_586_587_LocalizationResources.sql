/*
  Import localization resources for:
  - 5.8.6 General Ledger Entries
  - 5.8.7 Account Mapping
  Cultures: vi-VN, en-US, zh-CN
*/
SET NOCOUNT ON;

BEGIN TRANSACTION;

;WITH src AS (
    -- 5.8.6 General Ledger Entries
    SELECT N'GeneralLedgerEntries.Title' AS ResourceKey, N'vi-VN' AS Culture, N'Bút toán sổ cái' AS Value UNION ALL
    SELECT N'GeneralLedgerEntries.Title', N'en-US', N'General Ledger Entries' UNION ALL
    SELECT N'GeneralLedgerEntries.Title', N'zh-CN', N'总账分录' UNION ALL
    SELECT N'GeneralLedgerEntries.Description', N'vi-VN', N'Nội dung' UNION ALL
    SELECT N'GeneralLedgerEntries.Description', N'en-US', N'Description' UNION ALL
    SELECT N'GeneralLedgerEntries.Description', N'zh-CN', N'摘要' UNION ALL

    -- 5.8.7 Account Mapping
    SELECT N'AccountMapping.Title', N'vi-VN', N'Ánh xạ tài khoản' UNION ALL
    SELECT N'AccountMapping.Title', N'en-US', N'Account Mapping' UNION ALL
    SELECT N'AccountMapping.Title', N'zh-CN', N'科目映射' UNION ALL
    SELECT N'AccountMapping.Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'AccountMapping.Search', N'en-US', N'Search' UNION ALL
    SELECT N'AccountMapping.Search', N'zh-CN', N'搜索' UNION ALL
    SELECT N'AccountMapping.AccountCode', N'vi-VN', N'Mã tài khoản' UNION ALL
    SELECT N'AccountMapping.AccountCode', N'en-US', N'Account Code' UNION ALL
    SELECT N'AccountMapping.AccountCode', N'zh-CN', N'科目编码' UNION ALL
    SELECT N'AccountMapping.AccountName', N'vi-VN', N'Tên tài khoản' UNION ALL
    SELECT N'AccountMapping.AccountName', N'en-US', N'Account Name' UNION ALL
    SELECT N'AccountMapping.AccountName', N'zh-CN', N'科目名称' UNION ALL
    SELECT N'AccountMapping.StatementType', N'vi-VN', N'Loại báo cáo' UNION ALL
    SELECT N'AccountMapping.StatementType', N'en-US', N'Statement Type' UNION ALL
    SELECT N'AccountMapping.StatementType', N'zh-CN', N'报表类型' UNION ALL
    SELECT N'AccountMapping.BsSection', N'vi-VN', N'Nhóm BS' UNION ALL
    SELECT N'AccountMapping.BsSection', N'en-US', N'BS Section' UNION ALL
    SELECT N'AccountMapping.BsSection', N'zh-CN', N'资产负债表分区' UNION ALL
    SELECT N'AccountMapping.BsGroup', N'vi-VN', N'Nhóm chi tiết BS' UNION ALL
    SELECT N'AccountMapping.BsGroup', N'en-US', N'BS Group' UNION ALL
    SELECT N'AccountMapping.BsGroup', N'zh-CN', N'资产负债表分组' UNION ALL
    SELECT N'AccountMapping.BsLineItem', N'vi-VN', N'Chỉ tiêu BS' UNION ALL
    SELECT N'AccountMapping.BsLineItem', N'en-US', N'BS Line Item' UNION ALL
    SELECT N'AccountMapping.BsLineItem', N'zh-CN', N'资产负债表项目' UNION ALL
    SELECT N'AccountMapping.NormalBalance', N'vi-VN', N'Tính chất số dư' UNION ALL
    SELECT N'AccountMapping.NormalBalance', N'en-US', N'Normal Balance' UNION ALL
    SELECT N'AccountMapping.NormalBalance', N'zh-CN', N'正常余额方向' UNION ALL
    SELECT N'AccountMapping.DisplayOrder', N'vi-VN', N'Thứ tự hiển thị' UNION ALL
    SELECT N'AccountMapping.DisplayOrder', N'en-US', N'Display Order' UNION ALL
    SELECT N'AccountMapping.DisplayOrder', N'zh-CN', N'显示顺序' UNION ALL
    SELECT N'AccountMapping.IsContraAccount', N'vi-VN', N'Tài khoản đối ứng' UNION ALL
    SELECT N'AccountMapping.IsContraAccount', N'en-US', N'Contra Account' UNION ALL
    SELECT N'AccountMapping.IsContraAccount', N'zh-CN', N'备抵科目' UNION ALL
    SELECT N'AccountMapping.IsActive', N'vi-VN', N'Hoạt động' UNION ALL
    SELECT N'AccountMapping.IsActive', N'en-US', N'Active' UNION ALL
    SELECT N'AccountMapping.IsActive', N'zh-CN', N'启用' UNION ALL
    SELECT N'AccountMapping.NoAccessView', N'vi-VN', N'Bạn không có quyền xem' UNION ALL
    SELECT N'AccountMapping.NoAccessView', N'en-US', N'You do not have permission to view' UNION ALL
    SELECT N'AccountMapping.NoAccessView', N'zh-CN', N'您没有查看权限' UNION ALL
    SELECT N'AccountMapping.NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm' UNION ALL
    SELECT N'AccountMapping.NoAccessAdd', N'en-US', N'You do not have permission to add' UNION ALL
    SELECT N'AccountMapping.NoAccessAdd', N'zh-CN', N'您没有新增权限' UNION ALL
    SELECT N'AccountMapping.NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa' UNION ALL
    SELECT N'AccountMapping.NoAccessEdit', N'en-US', N'You do not have permission to edit' UNION ALL
    SELECT N'AccountMapping.NoAccessEdit', N'zh-CN', N'您没有编辑权限' UNION ALL
    SELECT N'AccountMapping.NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa' UNION ALL
    SELECT N'AccountMapping.NoAccessDelete', N'en-US', N'You do not have permission to delete' UNION ALL
    SELECT N'AccountMapping.NoAccessDelete', N'zh-CN', N'您没有删除权限' UNION ALL
    SELECT N'AccountMapping.AddTitle', N'vi-VN', N'Thêm ánh xạ tài khoản' UNION ALL
    SELECT N'AccountMapping.AddTitle', N'en-US', N'Add Account Mapping' UNION ALL
    SELECT N'AccountMapping.AddTitle', N'zh-CN', N'新增科目映射' UNION ALL
    SELECT N'AccountMapping.EditTitle', N'vi-VN', N'Sửa ánh xạ tài khoản' UNION ALL
    SELECT N'AccountMapping.EditTitle', N'en-US', N'Edit Account Mapping' UNION ALL
    SELECT N'AccountMapping.EditTitle', N'zh-CN', N'编辑科目映射' UNION ALL
    SELECT N'AccountMapping.AccountCodeExists', N'vi-VN', N'Mã tài khoản đã tồn tại.' UNION ALL
    SELECT N'AccountMapping.AccountCodeExists', N'en-US', N'Account code already exists.' UNION ALL
    SELECT N'AccountMapping.AccountCodeExists', N'zh-CN', N'科目编码已存在。' UNION ALL

    -- Common keys used by both screens
    SELECT N'Common.RowNotFound', N'vi-VN', N'Không tìm thấy bản ghi.' UNION ALL
    SELECT N'Common.RowNotFound', N'en-US', N'Row not found.' UNION ALL
    SELECT N'Common.RowNotFound', N'zh-CN', N'未找到记录。' UNION ALL
    SELECT N'Common.SavedSuccessfully', N'vi-VN', N'Lưu thành công.' UNION ALL
    SELECT N'Common.SavedSuccessfully', N'en-US', N'Saved successfully.' UNION ALL
    SELECT N'Common.SavedSuccessfully', N'zh-CN', N'保存成功。' UNION ALL
    SELECT N'Common.DeletedSuccessfully', N'vi-VN', N'Đã xóa thành công.' UNION ALL
    SELECT N'Common.DeletedSuccessfully', N'en-US', N'Deleted successfully.' UNION ALL
    SELECT N'Common.DeletedSuccessfully', N'zh-CN', N'删除成功。'
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
PRINT N'ImportAccounting_586_587_LocalizationResources: done.';
