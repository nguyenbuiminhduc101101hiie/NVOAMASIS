/*
  Localization strings for the 4.1.1 LCC POL VIETNAM feature.
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'LccPol_Title' AS ResourceKey, N'en-US' AS Culture, N'LCC POL VIETNAM' AS Value UNION ALL
    SELECT N'LccPol_Title', N'vi-VN', N'LCC POL VIETNAM' UNION ALL
    SELECT N'LccPol_Title', N'zh-CN', N'越南起运港本地费 (LCC POL)' UNION ALL

    SELECT N'LccPol_ImportExcel', N'en-US', N'Import Excel' UNION ALL
    SELECT N'LccPol_ImportExcel', N'vi-VN', N'Nhập Excel' UNION ALL
    SELECT N'LccPol_ImportExcel', N'zh-CN', N'导入Excel' UNION ALL

    SELECT N'LccPol_ManageImportedFiles', N'en-US', N'Manage imported files' UNION ALL
    SELECT N'LccPol_ManageImportedFiles', N'vi-VN', N'Quản lý file đã import' UNION ALL
    SELECT N'LccPol_ManageImportedFiles', N'zh-CN', N'管理已导入文件' UNION ALL

    SELECT N'LccPol_SearchPlaceholder', N'en-US', N'Search carrier / charge type...' UNION ALL
    SELECT N'LccPol_SearchPlaceholder', N'vi-VN', N'Tìm hãng tàu / loại phí...' UNION ALL
    SELECT N'LccPol_SearchPlaceholder', N'zh-CN', N'搜索船公司 / 费用类型...' UNION ALL

    SELECT N'LccPol_Col_No', N'en-US', N'No.' UNION ALL
    SELECT N'LccPol_Col_No', N'vi-VN', N'STT' UNION ALL
    SELECT N'LccPol_Col_No', N'zh-CN', N'序号' UNION ALL

    SELECT N'LccPol_Col_Carrier', N'en-US', N'Carrier' UNION ALL
    SELECT N'LccPol_Col_Carrier', N'vi-VN', N'Hãng tàu' UNION ALL
    SELECT N'LccPol_Col_Carrier', N'zh-CN', N'船公司' UNION ALL

    SELECT N'LccPol_Col_ChargeType', N'en-US', N'Charge type' UNION ALL
    SELECT N'LccPol_Col_ChargeType', N'vi-VN', N'Loại phí' UNION ALL
    SELECT N'LccPol_Col_ChargeType', N'zh-CN', N'费用类型' UNION ALL

    SELECT N'LccPol_Col_Remarks', N'en-US', N'Remarks' UNION ALL
    SELECT N'LccPol_Col_Remarks', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'LccPol_Col_Remarks', N'zh-CN', N'备注' UNION ALL

    SELECT N'LccPol_Col_UpdatedBy', N'en-US', N'Updated by' UNION ALL
    SELECT N'LccPol_Col_UpdatedBy', N'vi-VN', N'Người cập nhật' UNION ALL
    SELECT N'LccPol_Col_UpdatedBy', N'zh-CN', N'更新人' UNION ALL

    SELECT N'LccPol_Col_UpdatedDate', N'en-US', N'Updated date' UNION ALL
    SELECT N'LccPol_Col_UpdatedDate', N'vi-VN', N'Ngày cập nhật' UNION ALL
    SELECT N'LccPol_Col_UpdatedDate', N'zh-CN', N'更新日期' UNION ALL

    SELECT N'LccPol_Col_FileName', N'en-US', N'Imported file' UNION ALL
    SELECT N'LccPol_Col_FileName', N'vi-VN', N'File đã import' UNION ALL
    SELECT N'LccPol_Col_FileName', N'zh-CN', N'导入文件' UNION ALL

    SELECT N'LccPol_Col_RowCount', N'en-US', N'Rows' UNION ALL
    SELECT N'LccPol_Col_RowCount', N'vi-VN', N'Số dòng' UNION ALL
    SELECT N'LccPol_Col_RowCount', N'zh-CN', N'行数' UNION ALL

    SELECT N'LccPol_Col_LastImport', N'en-US', N'Last imported' UNION ALL
    SELECT N'LccPol_Col_LastImport', N'vi-VN', N'Import lần cuối' UNION ALL
    SELECT N'LccPol_Col_LastImport', N'zh-CN', N'最后导入时间' UNION ALL

    SELECT N'LccPol_NoAccessView', N'en-US', N'You do not have permission to view this data.' UNION ALL
    SELECT N'LccPol_NoAccessView', N'vi-VN', N'Bạn không có quyền xem dữ liệu này.' UNION ALL
    SELECT N'LccPol_NoAccessView', N'zh-CN', N'您没有查看此数据的权限。' UNION ALL

    SELECT N'LccPol_NoAccessAdd', N'en-US', N'You do not have permission to add.' UNION ALL
    SELECT N'LccPol_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm mới.' UNION ALL
    SELECT N'LccPol_NoAccessAdd', N'zh-CN', N'您没有新增权限。' UNION ALL

    SELECT N'LccPol_NoAccessEdit', N'en-US', N'You do not have permission to edit.' UNION ALL
    SELECT N'LccPol_NoAccessEdit', N'vi-VN', N'Bạn không có quyền chỉnh sửa.' UNION ALL
    SELECT N'LccPol_NoAccessEdit', N'zh-CN', N'您没有编辑权限。' UNION ALL

    SELECT N'LccPol_NoAccessDelete', N'en-US', N'You do not have permission to delete.' UNION ALL
    SELECT N'LccPol_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xoá.' UNION ALL
    SELECT N'LccPol_NoAccessDelete', N'zh-CN', N'您没有删除权限。' UNION ALL

    SELECT N'LccPol_NoAccessImport', N'en-US', N'You do not have permission to import data.' UNION ALL
    SELECT N'LccPol_NoAccessImport', N'vi-VN', N'Bạn không có quyền import dữ liệu.' UNION ALL
    SELECT N'LccPol_NoAccessImport', N'zh-CN', N'您没有导入数据的权限。' UNION ALL

    SELECT N'LccPol_SelectRowToEdit', N'en-US', N'Please select a row to edit.' UNION ALL
    SELECT N'LccPol_SelectRowToEdit', N'vi-VN', N'Vui lòng chọn một dòng để sửa.' UNION ALL
    SELECT N'LccPol_SelectRowToEdit', N'zh-CN', N'请选择要编辑的行。' UNION ALL

    SELECT N'LccPol_SelectRowToDelete', N'en-US', N'Please select a row to delete.' UNION ALL
    SELECT N'LccPol_SelectRowToDelete', N'vi-VN', N'Vui lòng chọn một dòng để xoá.' UNION ALL
    SELECT N'LccPol_SelectRowToDelete', N'zh-CN', N'请选择要删除的行。' UNION ALL

    SELECT N'LccPol_Confirm', N'en-US', N'Confirm' UNION ALL
    SELECT N'LccPol_Confirm', N'vi-VN', N'Xác nhận' UNION ALL
    SELECT N'LccPol_Confirm', N'zh-CN', N'确认' UNION ALL

    SELECT N'LccPol_DeleteConfirm', N'en-US', N'Delete the row {0} – {1}?' UNION ALL
    SELECT N'LccPol_DeleteConfirm', N'vi-VN', N'Xoá dòng {0} – {1}?' UNION ALL
    SELECT N'LccPol_DeleteConfirm', N'zh-CN', N'删除 {0} – {1} 这一行？' UNION ALL

    SELECT N'LccPol_Created', N'en-US', N'Created successfully.' UNION ALL
    SELECT N'LccPol_Created', N'vi-VN', N'Tạo mới thành công.' UNION ALL
    SELECT N'LccPol_Created', N'zh-CN', N'新增成功。' UNION ALL

    SELECT N'LccPol_Updated', N'en-US', N'Updated successfully.' UNION ALL
    SELECT N'LccPol_Updated', N'vi-VN', N'Cập nhật thành công.' UNION ALL
    SELECT N'LccPol_Updated', N'zh-CN', N'更新成功。' UNION ALL

    SELECT N'LccPol_Deleted', N'en-US', N'Deleted successfully.' UNION ALL
    SELECT N'LccPol_Deleted', N'vi-VN', N'Xoá thành công.' UNION ALL
    SELECT N'LccPol_Deleted', N'zh-CN', N'删除成功。' UNION ALL

    SELECT N'LccPol_AddDialogTitle', N'en-US', N'Add LCC POL charge' UNION ALL
    SELECT N'LccPol_AddDialogTitle', N'vi-VN', N'Thêm phí LCC POL' UNION ALL
    SELECT N'LccPol_AddDialogTitle', N'zh-CN', N'新增 LCC POL 费用' UNION ALL

    SELECT N'LccPol_EditDialogTitle', N'en-US', N'Edit LCC POL charge' UNION ALL
    SELECT N'LccPol_EditDialogTitle', N'vi-VN', N'Sửa phí LCC POL' UNION ALL
    SELECT N'LccPol_EditDialogTitle', N'zh-CN', N'编辑 LCC POL 费用' UNION ALL

    SELECT N'LccPol_Section_Charge', N'en-US', N'Carrier & charge type' UNION ALL
    SELECT N'LccPol_Section_Charge', N'vi-VN', N'Hãng tàu & loại phí' UNION ALL
    SELECT N'LccPol_Section_Charge', N'zh-CN', N'船公司与费用类型' UNION ALL

    SELECT N'LccPol_Section_Containers', N'en-US', N'Charges by container' UNION ALL
    SELECT N'LccPol_Section_Containers', N'vi-VN', N'Phí theo container' UNION ALL
    SELECT N'LccPol_Section_Containers', N'zh-CN', N'按箱型收费' UNION ALL

    SELECT N'LccPol_Section_Notes', N'en-US', N'Notes' UNION ALL
    SELECT N'LccPol_Section_Notes', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'LccPol_Section_Notes', N'zh-CN', N'备注' UNION ALL

    SELECT N'LccPol_Required', N'en-US', N'Required' UNION ALL
    SELECT N'LccPol_Required', N'vi-VN', N'Bắt buộc nhập' UNION ALL
    SELECT N'LccPol_Required', N'zh-CN', N'必填' UNION ALL

    SELECT N'LccPol_DuplicateWarning', N'en-US', N'A row for {0} – {1} already exists. Excel import matches rows by carrier + charge type, so duplicates may be updated unexpectedly.' UNION ALL
    SELECT N'LccPol_DuplicateWarning', N'vi-VN', N'Đã có dòng {0} – {1}. Import Excel khớp dữ liệu theo hãng tàu + loại phí nên dòng trùng có thể bị cập nhật ngoài ý muốn.' UNION ALL
    SELECT N'LccPol_DuplicateWarning', N'zh-CN', N'已存在 {0} – {1} 的记录。Excel 导入按船公司 + 费用类型匹配，重复记录可能被意外更新。' UNION ALL

    SELECT N'LccPol_Meta_Manual', N'en-US', N'Entered manually' UNION ALL
    SELECT N'LccPol_Meta_Manual', N'vi-VN', N'Nhập tay' UNION ALL
    SELECT N'LccPol_Meta_Manual', N'zh-CN', N'手动录入' UNION ALL

    SELECT N'LccPol_Meta_UpdatedBy', N'en-US', N'updated by {0}' UNION ALL
    SELECT N'LccPol_Meta_UpdatedBy', N'vi-VN', N'cập nhật bởi {0}' UNION ALL
    SELECT N'LccPol_Meta_UpdatedBy', N'zh-CN', N'由 {0} 更新' UNION ALL

    SELECT N'LccPol_ImportDialogTitle', N'en-US', N'Import Excel - LCC POL VIETNAM' UNION ALL
    SELECT N'LccPol_ImportDialogTitle', N'vi-VN', N'Import Excel - LCC POL VIETNAM' UNION ALL
    SELECT N'LccPol_ImportDialogTitle', N'zh-CN', N'导入Excel - LCC POL VIETNAM' UNION ALL

    SELECT N'LccPol_ImportedFilesDialogTitle', N'en-US', N'Imported files - LCC POL VIETNAM' UNION ALL
    SELECT N'LccPol_ImportedFilesDialogTitle', N'vi-VN', N'File đã import - LCC POL VIETNAM' UNION ALL
    SELECT N'LccPol_ImportedFilesDialogTitle', N'zh-CN', N'已导入文件 - LCC POL VIETNAM' UNION ALL

    SELECT N'LccPol_ImportHint', N'en-US', N'Choose the original Excel file (only the first sheet "POL LOCAL" is read). After choosing a file, the data to be imported is shown so you can review it before it is saved.' UNION ALL
    SELECT N'LccPol_ImportHint', N'vi-VN', N'Chọn file Excel gốc (chỉ đọc sheet đầu tiên "POL LOCAL"). Sau khi chọn file, hệ thống sẽ hiển thị trước dữ liệu sẽ được import để bạn kiểm tra trước khi ghi vào hệ thống.' UNION ALL
    SELECT N'LccPol_ImportHint', N'zh-CN', N'请选择原始 Excel 文件（仅读取第一个工作表 "POL LOCAL"）。选择文件后，系统会先显示将要导入的数据，供您在保存前检查。' UNION ALL

    SELECT N'LccPol_File', N'en-US', N'File' UNION ALL
    SELECT N'LccPol_File', N'vi-VN', N'File' UNION ALL
    SELECT N'LccPol_File', N'zh-CN', N'文件' UNION ALL

    SELECT N'LccPol_PreviewSummary', N'en-US', N'{0} new rows, {1} changed rows, {2} unchanged rows.' UNION ALL
    SELECT N'LccPol_PreviewSummary', N'vi-VN', N'{0} dòng mới, {1} dòng thay đổi, {2} dòng không đổi.' UNION ALL
    SELECT N'LccPol_PreviewSummary', N'zh-CN', N'新增 {0} 行，变更 {1} 行，未变 {2} 行。' UNION ALL

    SELECT N'LccPol_PreviewNewValue', N'en-US', N'new' UNION ALL
    SELECT N'LccPol_PreviewNewValue', N'vi-VN', N'mới' UNION ALL
    SELECT N'LccPol_PreviewNewValue', N'zh-CN', N'新' UNION ALL

    SELECT N'LccPol_PreviewCurrentValue', N'en-US', N'current' UNION ALL
    SELECT N'LccPol_PreviewCurrentValue', N'vi-VN', N'hiện có' UNION ALL
    SELECT N'LccPol_PreviewCurrentValue', N'zh-CN', N'现有' UNION ALL

    SELECT N'LccPol_Status_New', N'en-US', N'New' UNION ALL
    SELECT N'LccPol_Status_New', N'vi-VN', N'Mới' UNION ALL
    SELECT N'LccPol_Status_New', N'zh-CN', N'新增' UNION ALL

    SELECT N'LccPol_Status_Changed', N'en-US', N'Changed' UNION ALL
    SELECT N'LccPol_Status_Changed', N'vi-VN', N'Thay đổi' UNION ALL
    SELECT N'LccPol_Status_Changed', N'zh-CN', N'变更' UNION ALL

    SELECT N'LccPol_Status_Unchanged', N'en-US', N'Unchanged' UNION ALL
    SELECT N'LccPol_Status_Unchanged', N'vi-VN', N'Không đổi' UNION ALL
    SELECT N'LccPol_Status_Unchanged', N'zh-CN', N'未变' UNION ALL

    SELECT N'LccPol_ChooseAnotherFile', N'en-US', N'Choose another file' UNION ALL
    SELECT N'LccPol_ChooseAnotherFile', N'vi-VN', N'Chọn file khác' UNION ALL
    SELECT N'LccPol_ChooseAnotherFile', N'zh-CN', N'选择其他文件' UNION ALL

    SELECT N'LccPol_ConfirmImport', N'en-US', N'Confirm import ({0} new, {1} changed)' UNION ALL
    SELECT N'LccPol_ConfirmImport', N'vi-VN', N'Xác nhận Import ({0} mới, {1} thay đổi)' UNION ALL
    SELECT N'LccPol_ConfirmImport', N'zh-CN', N'确认导入（新增 {0}，变更 {1}）' UNION ALL

    SELECT N'LccPol_FileTooLarge', N'en-US', N'File exceeds {0}MB' UNION ALL
    SELECT N'LccPol_FileTooLarge', N'vi-VN', N'File vượt quá {0}MB' UNION ALL
    SELECT N'LccPol_FileTooLarge', N'zh-CN', N'文件超过 {0}MB' UNION ALL

    SELECT N'LccPol_ImportSuccess', N'en-US', N'Import successful: {0} new rows, {1} updated rows.' UNION ALL
    SELECT N'LccPol_ImportSuccess', N'vi-VN', N'Import thành công: {0} dòng mới, {1} dòng cập nhật.' UNION ALL
    SELECT N'LccPol_ImportSuccess', N'zh-CN', N'导入成功：新增 {0} 行，更新 {1} 行。' UNION ALL

    SELECT N'LccPol_ImportedFilesHint', N'en-US', N'Excel files that have been imported. Deleting a file deletes every row (carrier + charge type) loaded from that file.' UNION ALL
    SELECT N'LccPol_ImportedFilesHint', N'vi-VN', N'Danh sách các file Excel đã import. Xoá 1 file sẽ xoá toàn bộ các dòng dữ liệu (hãng tàu + loại phí) được nạp từ file đó.' UNION ALL
    SELECT N'LccPol_ImportedFilesHint', N'zh-CN', N'已导入的 Excel 文件列表。删除一个文件将删除从该文件导入的所有数据行（船公司 + 费用类型）。' UNION ALL

    SELECT N'LccPol_NoImportedFiles', N'en-US', N'No files have been imported yet.' UNION ALL
    SELECT N'LccPol_NoImportedFiles', N'vi-VN', N'Chưa có file nào được import.' UNION ALL
    SELECT N'LccPol_NoImportedFiles', N'zh-CN', N'尚未导入任何文件。' UNION ALL

    SELECT N'LccPol_DeleteFileConfirm', N'en-US', N'Delete all data imported from file ''{0}''?' UNION ALL
    SELECT N'LccPol_DeleteFileConfirm', N'vi-VN', N'Xoá toàn bộ dữ liệu đã import từ file ''{0}''?' UNION ALL
    SELECT N'LccPol_DeleteFileConfirm', N'zh-CN', N'删除文件 ''{0}'' 导入的全部数据？' UNION ALL

    SELECT N'LccPol_DeletedFileRows', N'en-US', N'Deleted {0} rows from file ''{1}''.' UNION ALL
    SELECT N'LccPol_DeletedFileRows', N'vi-VN', N'Đã xoá {0} dòng thuộc file ''{1}''.' UNION ALL
    SELECT N'LccPol_DeletedFileRows', N'zh-CN', N'已删除文件 ''{1}'' 的 {0} 行数据。' UNION ALL

    SELECT N'LccPol_Close', N'en-US', N'Close' UNION ALL
    SELECT N'LccPol_Close', N'vi-VN', N'Đóng' UNION ALL
    SELECT N'LccPol_Close', N'zh-CN', N'关闭'
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

PRINT N'LccPolVietnam localization imported.';
