/*
  Import localization resources for 5.8.8 Accounting Period
  Component: AccountingPeriod_Index.razor
  Cultures: vi-VN, en-US, zh-CN
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'AccountingPeriod.Title' AS ResourceKey, N'vi-VN' AS Culture, N'Kỳ kế toán' AS Value UNION ALL
    SELECT N'AccountingPeriod.Title', N'en-US', N'Accounting Period' UNION ALL
    SELECT N'AccountingPeriod.Title', N'zh-CN', N'会计期间' UNION ALL

    SELECT N'AccountingPeriod.Search', N'vi-VN', N'Tìm kiếm' UNION ALL
    SELECT N'AccountingPeriod.Search', N'en-US', N'Search' UNION ALL
    SELECT N'AccountingPeriod.Search', N'zh-CN', N'搜索' UNION ALL

    SELECT N'AccountingPeriod.FiscalYear', N'vi-VN', N'Năm tài chính' UNION ALL
    SELECT N'AccountingPeriod.FiscalYear', N'en-US', N'Fiscal Year' UNION ALL
    SELECT N'AccountingPeriod.FiscalYear', N'zh-CN', N'会计年度' UNION ALL

    SELECT N'AccountingPeriod.PeriodMonth', N'vi-VN', N'Tháng kỳ' UNION ALL
    SELECT N'AccountingPeriod.PeriodMonth', N'en-US', N'Period Month' UNION ALL
    SELECT N'AccountingPeriod.PeriodMonth', N'zh-CN', N'期间月份' UNION ALL

    SELECT N'AccountingPeriod.StartDate', N'vi-VN', N'Ngày bắt đầu' UNION ALL
    SELECT N'AccountingPeriod.StartDate', N'en-US', N'Start Date' UNION ALL
    SELECT N'AccountingPeriod.StartDate', N'zh-CN', N'开始日期' UNION ALL

    SELECT N'AccountingPeriod.EndDate', N'vi-VN', N'Ngày kết thúc' UNION ALL
    SELECT N'AccountingPeriod.EndDate', N'en-US', N'End Date' UNION ALL
    SELECT N'AccountingPeriod.EndDate', N'zh-CN', N'结束日期' UNION ALL

    SELECT N'AccountingPeriod.Status', N'vi-VN', N'Trạng thái' UNION ALL
    SELECT N'AccountingPeriod.Status', N'en-US', N'Status' UNION ALL
    SELECT N'AccountingPeriod.Status', N'zh-CN', N'状态' UNION ALL

    SELECT N'AccountingPeriod.IsClosed', N'vi-VN', N'Đã khóa' UNION ALL
    SELECT N'AccountingPeriod.IsClosed', N'en-US', N'Is Closed' UNION ALL
    SELECT N'AccountingPeriod.IsClosed', N'zh-CN', N'已关闭' UNION ALL

    SELECT N'AccountingPeriod.UpdatedAt', N'vi-VN', N'Cập nhật lúc' UNION ALL
    SELECT N'AccountingPeriod.UpdatedAt', N'en-US', N'Updated At' UNION ALL
    SELECT N'AccountingPeriod.UpdatedAt', N'zh-CN', N'更新时间' UNION ALL

    SELECT N'AccountingPeriod.NoAccessView', N'vi-VN', N'Bạn không có quyền xem' UNION ALL
    SELECT N'AccountingPeriod.NoAccessView', N'en-US', N'You do not have permission to view' UNION ALL
    SELECT N'AccountingPeriod.NoAccessView', N'zh-CN', N'您没有查看权限' UNION ALL

    SELECT N'AccountingPeriod.NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm' UNION ALL
    SELECT N'AccountingPeriod.NoAccessAdd', N'en-US', N'You do not have permission to add' UNION ALL
    SELECT N'AccountingPeriod.NoAccessAdd', N'zh-CN', N'您没有新增权限' UNION ALL

    SELECT N'AccountingPeriod.NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa' UNION ALL
    SELECT N'AccountingPeriod.NoAccessEdit', N'en-US', N'You do not have permission to edit' UNION ALL
    SELECT N'AccountingPeriod.NoAccessEdit', N'zh-CN', N'您没有编辑权限' UNION ALL

    SELECT N'AccountingPeriod.NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa' UNION ALL
    SELECT N'AccountingPeriod.NoAccessDelete', N'en-US', N'You do not have permission to delete' UNION ALL
    SELECT N'AccountingPeriod.NoAccessDelete', N'zh-CN', N'您没有删除权限' UNION ALL

    SELECT N'AccountingPeriod.AddTitle', N'vi-VN', N'Thêm kỳ kế toán' UNION ALL
    SELECT N'AccountingPeriod.AddTitle', N'en-US', N'Add Accounting Period' UNION ALL
    SELECT N'AccountingPeriod.AddTitle', N'zh-CN', N'新增会计期间' UNION ALL

    SELECT N'AccountingPeriod.EditTitle', N'vi-VN', N'Sửa kỳ kế toán' UNION ALL
    SELECT N'AccountingPeriod.EditTitle', N'en-US', N'Edit Accounting Period' UNION ALL
    SELECT N'AccountingPeriod.EditTitle', N'zh-CN', N'编辑会计期间' UNION ALL

    SELECT N'AccountingPeriod.StartDateGreaterThanEndDate', N'vi-VN', N'Ngày bắt đầu không được lớn hơn ngày kết thúc.' UNION ALL
    SELECT N'AccountingPeriod.StartDateGreaterThanEndDate', N'en-US', N'Start date cannot be greater than end date.' UNION ALL
    SELECT N'AccountingPeriod.StartDateGreaterThanEndDate', N'zh-CN', N'开始日期不能晚于结束日期。' UNION ALL

    SELECT N'AccountingPeriod.FiscalYearMonthExists', N'vi-VN', N'Năm tài chính và tháng kỳ đã tồn tại.' UNION ALL
    SELECT N'AccountingPeriod.FiscalYearMonthExists', N'en-US', N'Fiscal year + month already exists.' UNION ALL
    SELECT N'AccountingPeriod.FiscalYearMonthExists', N'zh-CN', N'会计年度与期间月份已存在。' UNION ALL

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
PRINT N'ImportAccountingPeriod_588_LocalizationResources: done.';
