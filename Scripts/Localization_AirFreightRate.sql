/*
  Localization strings for the 4.8 Air Freight Quotation (AirFreightRate) feature.
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'AirFreight_Title' AS ResourceKey, N'en-US' AS Culture, N'Air Freight Quotation' AS Value UNION ALL
    SELECT N'AirFreight_Title', N'vi-VN', N'Báo giá hàng Air' UNION ALL
    SELECT N'AirFreight_Title', N'zh-CN', N'空运报价' UNION ALL

    SELECT N'AirFreight_ImportExcel', N'en-US', N'Import Excel' UNION ALL
    SELECT N'AirFreight_ImportExcel', N'vi-VN', N'Nhập Excel' UNION ALL
    SELECT N'AirFreight_ImportExcel', N'zh-CN', N'导入Excel' UNION ALL

    SELECT N'AirFreight_SearchPlaceholder', N'en-US', N'Search airline, destination, note...' UNION ALL
    SELECT N'AirFreight_SearchPlaceholder', N'vi-VN', N'Tìm hãng bay, điểm đến, ghi chú...' UNION ALL
    SELECT N'AirFreight_SearchPlaceholder', N'zh-CN', N'搜索航空公司、目的地、备注...' UNION ALL

    SELECT N'AirFreight_Col_Airlines', N'en-US', N'Airlines' UNION ALL
    SELECT N'AirFreight_Col_Airlines', N'vi-VN', N'Hãng bay' UNION ALL
    SELECT N'AirFreight_Col_Airlines', N'zh-CN', N'航空公司' UNION ALL

    SELECT N'AirFreight_Col_Destination', N'en-US', N'Destination' UNION ALL
    SELECT N'AirFreight_Col_Destination', N'vi-VN', N'Điểm đến' UNION ALL
    SELECT N'AirFreight_Col_Destination', N'zh-CN', N'目的地' UNION ALL

    SELECT N'AirFreight_Col_Route', N'en-US', N'Route' UNION ALL
    SELECT N'AirFreight_Col_Route', N'vi-VN', N'Hành trình' UNION ALL
    SELECT N'AirFreight_Col_Route', N'zh-CN', N'航线' UNION ALL

    SELECT N'AirFreight_Col_Frequency', N'en-US', N'Frequency' UNION ALL
    SELECT N'AirFreight_Col_Frequency', N'vi-VN', N'Tần suất' UNION ALL
    SELECT N'AirFreight_Col_Frequency', N'zh-CN', N'班次' UNION ALL

    SELECT N'AirFreight_Col_TransitTime', N'en-US', N'Transit time' UNION ALL
    SELECT N'AirFreight_Col_TransitTime', N'vi-VN', N'Thời gian vận chuyển' UNION ALL
    SELECT N'AirFreight_Col_TransitTime', N'zh-CN', N'运输时间' UNION ALL

    SELECT N'AirFreight_Col_Surcharges', N'en-US', N'Surcharges' UNION ALL
    SELECT N'AirFreight_Col_Surcharges', N'vi-VN', N'Phụ phí' UNION ALL
    SELECT N'AirFreight_Col_Surcharges', N'zh-CN', N'附加费' UNION ALL

    SELECT N'AirFreight_Col_Note', N'en-US', N'Note' UNION ALL
    SELECT N'AirFreight_Col_Note', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'AirFreight_Col_Note', N'zh-CN', N'备注' UNION ALL

    SELECT N'AirFreight_Col_FileName', N'en-US', N'Imported file' UNION ALL
    SELECT N'AirFreight_Col_FileName', N'vi-VN', N'File nhập' UNION ALL
    SELECT N'AirFreight_Col_FileName', N'zh-CN', N'导入文件' UNION ALL

    SELECT N'AirFreight_NoAccessView', N'en-US', N'You do not have permission to view this data.' UNION ALL
    SELECT N'AirFreight_NoAccessView', N'vi-VN', N'Bạn không có quyền xem dữ liệu này.' UNION ALL
    SELECT N'AirFreight_NoAccessView', N'zh-CN', N'您没有查看此数据的权限。' UNION ALL

    SELECT N'AirFreight_NoAccessAdd', N'en-US', N'You do not have permission to add this data.' UNION ALL
    SELECT N'AirFreight_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm dữ liệu này.' UNION ALL
    SELECT N'AirFreight_NoAccessAdd', N'zh-CN', N'您没有添加此数据的权限。' UNION ALL

    SELECT N'AirFreight_NoAccessEdit', N'en-US', N'You do not have permission to edit this data.' UNION ALL
    SELECT N'AirFreight_NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa dữ liệu này.' UNION ALL
    SELECT N'AirFreight_NoAccessEdit', N'zh-CN', N'您没有编辑此数据的权限。' UNION ALL

    SELECT N'AirFreight_NoAccessDelete', N'en-US', N'You do not have permission to delete this data.' UNION ALL
    SELECT N'AirFreight_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xoá dữ liệu này.' UNION ALL
    SELECT N'AirFreight_NoAccessDelete', N'zh-CN', N'您没有删除此数据的权限。' UNION ALL

    SELECT N'AirFreight_LoadError', N'en-US', N'Could not load data: {0}' UNION ALL
    SELECT N'AirFreight_LoadError', N'vi-VN', N'Không tải được dữ liệu: {0}' UNION ALL
    SELECT N'AirFreight_LoadError', N'zh-CN', N'无法加载数据：{0}' UNION ALL

    SELECT N'AirFreight_Warning', N'en-US', N'Warning' UNION ALL
    SELECT N'AirFreight_Warning', N'vi-VN', N'Cảnh báo' UNION ALL
    SELECT N'AirFreight_Warning', N'zh-CN', N'警告' UNION ALL

    SELECT N'AirFreight_DeleteManyConfirm', N'en-US', N'Delete all data imported from: {0}?' UNION ALL
    SELECT N'AirFreight_DeleteManyConfirm', N'vi-VN', N'Xoá toàn bộ dữ liệu đã nhập từ: {0}?' UNION ALL
    SELECT N'AirFreight_DeleteManyConfirm', N'zh-CN', N'删除以下文件导入的全部数据：{0}？' UNION ALL

    SELECT N'AirFreight_DeletedRows', N'en-US', N'Deleted {0} rows.' UNION ALL
    SELECT N'AirFreight_DeletedRows', N'vi-VN', N'Đã xoá {0} dòng.' UNION ALL
    SELECT N'AirFreight_DeletedRows', N'zh-CN', N'已删除 {0} 行。' UNION ALL

    SELECT N'AirFreight_ReadFileError', N'en-US', N'Error reading file: {0}' UNION ALL
    SELECT N'AirFreight_ReadFileError', N'vi-VN', N'Lỗi đọc file: {0}' UNION ALL
    SELECT N'AirFreight_ReadFileError', N'zh-CN', N'读取文件出错：{0}' UNION ALL

    SELECT N'AirFreight_FileAlreadyImported', N'en-US', N'File ''{0}'' has already been imported.' UNION ALL
    SELECT N'AirFreight_FileAlreadyImported', N'vi-VN', N'File ''{0}'' đã được import trước đó.' UNION ALL
    SELECT N'AirFreight_FileAlreadyImported', N'zh-CN', N'文件 ''{0}'' 已导入过。' UNION ALL

    SELECT N'AirFreight_StageReading', N'en-US', N'Reading file...' UNION ALL
    SELECT N'AirFreight_StageReading', N'vi-VN', N'Đang đọc file...' UNION ALL
    SELECT N'AirFreight_StageReading', N'zh-CN', N'正在读取文件...' UNION ALL

    SELECT N'AirFreight_StageSaving', N'en-US', N'Saving data...' UNION ALL
    SELECT N'AirFreight_StageSaving', N'vi-VN', N'Đang lưu dữ liệu...' UNION ALL
    SELECT N'AirFreight_StageSaving', N'zh-CN', N'正在保存数据...' UNION ALL

    SELECT N'AirFreight_ImportError', N'en-US', N'Import error: {0}' UNION ALL
    SELECT N'AirFreight_ImportError', N'vi-VN', N'Lỗi import: {0}' UNION ALL
    SELECT N'AirFreight_ImportError', N'zh-CN', N'导入出错：{0}' UNION ALL

    SELECT N'AirFreight_ImportEmptyFile', N'en-US', N'The file has no data to import.' UNION ALL
    SELECT N'AirFreight_ImportEmptyFile', N'vi-VN', N'File không có dữ liệu để import.' UNION ALL
    SELECT N'AirFreight_ImportEmptyFile', N'zh-CN', N'文件中没有可导入的数据。' UNION ALL

    SELECT N'AirFreight_ImportSuccess', N'en-US', N'Imported {0} rows successfully.' UNION ALL
    SELECT N'AirFreight_ImportSuccess', N'vi-VN', N'Đã import thành công {0} dòng.' UNION ALL
    SELECT N'AirFreight_ImportSuccess', N'zh-CN', N'成功导入 {0} 行。' UNION ALL

    SELECT N'AirFreight_PreviewTitle', N'en-US', N'Import preview' UNION ALL
    SELECT N'AirFreight_PreviewTitle', N'vi-VN', N'Xem trước dữ liệu import' UNION ALL
    SELECT N'AirFreight_PreviewTitle', N'zh-CN', N'导入预览' UNION ALL

    SELECT N'AirFreight_PreviewInfo', N'en-US', N'File "{0}" has {1} rows. Please review the data below before confirming the import.' UNION ALL
    SELECT N'AirFreight_PreviewInfo', N'vi-VN', N'File "{0}" có {1} dòng. Kiểm tra lại dữ liệu bên dưới trước khi xác nhận import.' UNION ALL
    SELECT N'AirFreight_PreviewInfo', N'zh-CN', N'文件 "{0}" 共 {1} 行。确认导入前请检查以下数据。' UNION ALL

    SELECT N'AirFreight_ConfirmImport', N'en-US', N'Confirm import' UNION ALL
    SELECT N'AirFreight_ConfirmImport', N'vi-VN', N'Xác nhận import' UNION ALL
    SELECT N'AirFreight_ConfirmImport', N'zh-CN', N'确认导入' UNION ALL

    SELECT N'AirFreight_SelectRowToEdit', N'en-US', N'Please select a row to edit.' UNION ALL
    SELECT N'AirFreight_SelectRowToEdit', N'vi-VN', N'Vui lòng chọn một dòng để sửa.' UNION ALL
    SELECT N'AirFreight_SelectRowToEdit', N'zh-CN', N'请选择要编辑的行。' UNION ALL

    SELECT N'AirFreight_SelectRowToDelete', N'en-US', N'Please select a row to delete.' UNION ALL
    SELECT N'AirFreight_SelectRowToDelete', N'vi-VN', N'Vui lòng chọn một dòng để xoá.' UNION ALL
    SELECT N'AirFreight_SelectRowToDelete', N'zh-CN', N'请选择要删除的行。' UNION ALL

    SELECT N'AirFreight_DeleteConfirm', N'en-US', N'Are you sure you want to delete this row?' UNION ALL
    SELECT N'AirFreight_DeleteConfirm', N'vi-VN', N'Bạn có chắc muốn xoá dòng này?' UNION ALL
    SELECT N'AirFreight_DeleteConfirm', N'zh-CN', N'确定要删除此行吗？' UNION ALL

    SELECT N'AirFreight_DeleteFailed', N'en-US', N'Delete failed.' UNION ALL
    SELECT N'AirFreight_DeleteFailed', N'vi-VN', N'Xoá thất bại.' UNION ALL
    SELECT N'AirFreight_DeleteFailed', N'zh-CN', N'删除失败。' UNION ALL

    SELECT N'AirFreight_Deleted', N'en-US', N'Deleted.' UNION ALL
    SELECT N'AirFreight_Deleted', N'vi-VN', N'Đã xoá.' UNION ALL
    SELECT N'AirFreight_Deleted', N'zh-CN', N'已删除。' UNION ALL

    SELECT N'AirFreight_SaveFailed', N'en-US', N'Save failed.' UNION ALL
    SELECT N'AirFreight_SaveFailed', N'vi-VN', N'Lưu thất bại.' UNION ALL
    SELECT N'AirFreight_SaveFailed', N'zh-CN', N'保存失败。' UNION ALL

    SELECT N'AirFreight_Saved', N'en-US', N'Saved.' UNION ALL
    SELECT N'AirFreight_Saved', N'vi-VN', N'Đã lưu.' UNION ALL
    SELECT N'AirFreight_Saved', N'zh-CN', N'已保存。' UNION ALL

    SELECT N'AirFreight_AddDialogTitle', N'en-US', N'Add air freight rate' UNION ALL
    SELECT N'AirFreight_AddDialogTitle', N'vi-VN', N'Thêm báo giá Air' UNION ALL
    SELECT N'AirFreight_AddDialogTitle', N'zh-CN', N'新增空运报价' UNION ALL

    SELECT N'AirFreight_EditDialogTitle', N'en-US', N'Edit air freight rate' UNION ALL
    SELECT N'AirFreight_EditDialogTitle', N'vi-VN', N'Sửa báo giá Air' UNION ALL
    SELECT N'AirFreight_EditDialogTitle', N'zh-CN', N'编辑空运报价' UNION ALL

    SELECT N'AirFreight_Section_Route', N'en-US', N'Airline & route' UNION ALL
    SELECT N'AirFreight_Section_Route', N'vi-VN', N'Hãng bay & tuyến' UNION ALL
    SELECT N'AirFreight_Section_Route', N'zh-CN', N'航空公司与航线' UNION ALL

    SELECT N'AirFreight_Section_Rates', N'en-US', N'Rates by weight (USD/kg)' UNION ALL
    SELECT N'AirFreight_Section_Rates', N'vi-VN', N'Bảng giá theo trọng lượng (USD/kg)' UNION ALL
    SELECT N'AirFreight_Section_Rates', N'zh-CN', N'按重量报价 (USD/kg)' UNION ALL

    SELECT N'AirFreight_Section_Notes', N'en-US', N'Surcharges & notes' UNION ALL
    SELECT N'AirFreight_Section_Notes', N'vi-VN', N'Phụ phí & ghi chú' UNION ALL
    SELECT N'AirFreight_Section_Notes', N'zh-CN', N'附加费与备注' UNION ALL

    SELECT N'AirFreight_MinUnit', N'en-US', N'USD/shipment' UNION ALL
    SELECT N'AirFreight_MinUnit', N'vi-VN', N'USD/lô' UNION ALL
    SELECT N'AirFreight_MinUnit', N'zh-CN', N'USD/票' UNION ALL

    SELECT N'AirFreight_TierHigherWarning', N'en-US', N'Higher than previous tier' UNION ALL
    SELECT N'AirFreight_TierHigherWarning', N'vi-VN', N'Cao hơn bậc trước' UNION ALL
    SELECT N'AirFreight_TierHigherWarning', N'zh-CN', N'高于上一档' UNION ALL

    SELECT N'AirFreight_Meta_Manual', N'en-US', N'Entered manually' UNION ALL
    SELECT N'AirFreight_Meta_Manual', N'vi-VN', N'Nhập tay' UNION ALL
    SELECT N'AirFreight_Meta_Manual', N'zh-CN', N'手动录入' UNION ALL

    SELECT N'AirFreight_Meta_ImportedBy', N'en-US', N'by {0}' UNION ALL
    SELECT N'AirFreight_Meta_ImportedBy', N'vi-VN', N'bởi {0}' UNION ALL
    SELECT N'AirFreight_Meta_ImportedBy', N'zh-CN', N'由 {0}'
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

PRINT N'AirFreightRate localization imported.';
