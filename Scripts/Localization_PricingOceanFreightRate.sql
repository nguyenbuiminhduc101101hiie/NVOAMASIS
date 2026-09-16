/*
  Localization strings for the 4.7 Pricing Ocean Freight Rate feature.
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'PricingOceanFreight_Title' AS ResourceKey, N'en-US' AS Culture, N'Pricing Ocean Freight Rate' AS Value UNION ALL
    SELECT N'PricingOceanFreight_Title', N'vi-VN', N'Bảng giá cước biển' UNION ALL
    SELECT N'PricingOceanFreight_Title', N'zh-CN', N'海运费价格表' UNION ALL

    SELECT N'PricingOceanFreight_ImportExcel', N'en-US', N'Import Excel' UNION ALL
    SELECT N'PricingOceanFreight_ImportExcel', N'vi-VN', N'Nhập Excel' UNION ALL
    SELECT N'PricingOceanFreight_ImportExcel', N'zh-CN', N'导入Excel' UNION ALL

    SELECT N'PricingOceanFreight_Tab_Intrasia', N'en-US', N'Intrasia' UNION ALL
    SELECT N'PricingOceanFreight_Tab_Intrasia', N'vi-VN', N'Intrasia' UNION ALL
    SELECT N'PricingOceanFreight_Tab_Intrasia', N'zh-CN', N'Intrasia' UNION ALL

    SELECT N'PricingOceanFreight_Tab_IntrasiaRF', N'en-US', N'Intrasia (Reefer)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_IntrasiaRF', N'vi-VN', N'Intrasia (Lạnh)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_IntrasiaRF', N'zh-CN', N'Intrasia (冷藏)' UNION ALL

    SELECT N'PricingOceanFreight_Tab_MiddleEast', N'en-US', N'Middle East / Red Sea / India' UNION ALL
    SELECT N'PricingOceanFreight_Tab_MiddleEast', N'vi-VN', N'Trung Đông / Hồng Hải / Ấn Độ' UNION ALL
    SELECT N'PricingOceanFreight_Tab_MiddleEast', N'zh-CN', N'中东/红海/印度' UNION ALL

    SELECT N'PricingOceanFreight_Tab_LatinWestEastAus', N'en-US', N'Latin West/East Coast, Australia' UNION ALL
    SELECT N'PricingOceanFreight_Tab_LatinWestEastAus', N'vi-VN', N'Nam Mỹ Tây/Đông, Úc' UNION ALL
    SELECT N'PricingOceanFreight_Tab_LatinWestEastAus', N'zh-CN', N'拉美东西海岸/澳大利亚' UNION ALL

    SELECT N'PricingOceanFreight_Tab_MiddleEastRF', N'en-US', N'Middle East / Red Sea / India (Reefer)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_MiddleEastRF', N'vi-VN', N'Trung Đông / Hồng Hải / Ấn Độ (Lạnh)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_MiddleEastRF', N'zh-CN', N'中东/红海/印度 (冷藏)' UNION ALL

    SELECT N'PricingOceanFreight_Tab_EuUsaAfrica', N'en-US', N'EU / USA-Canada / Africa / South America' UNION ALL
    SELECT N'PricingOceanFreight_Tab_EuUsaAfrica', N'vi-VN', N'EU / USA-Canada / Châu Phi / Nam Mỹ' UNION ALL
    SELECT N'PricingOceanFreight_Tab_EuUsaAfrica', N'zh-CN', N'欧盟/美加/非洲/南美' UNION ALL

    SELECT N'PricingOceanFreight_Tab_EuUsaAfricaRF', N'en-US', N'EU / USA-Canada / Africa / South America (Reefer)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_EuUsaAfricaRF', N'vi-VN', N'EU / USA-Canada / Châu Phi / Nam Mỹ (Lạnh)' UNION ALL
    SELECT N'PricingOceanFreight_Tab_EuUsaAfricaRF', N'zh-CN', N'欧盟/美加/非洲/南美 (冷藏)' UNION ALL

    SELECT N'PricingOceanFreight_Col_Carrier', N'en-US', N'Carrier' UNION ALL
    SELECT N'PricingOceanFreight_Col_Carrier', N'vi-VN', N'Hãng tàu' UNION ALL
    SELECT N'PricingOceanFreight_Col_Carrier', N'zh-CN', N'承运人' UNION ALL

    SELECT N'PricingOceanFreight_Col_Country', N'en-US', N'Country' UNION ALL
    SELECT N'PricingOceanFreight_Col_Country', N'vi-VN', N'Quốc gia' UNION ALL
    SELECT N'PricingOceanFreight_Col_Country', N'zh-CN', N'国家' UNION ALL

    SELECT N'PricingOceanFreight_Col_FreeTime', N'en-US', N'Free Time at POD' UNION ALL
    SELECT N'PricingOceanFreight_Col_FreeTime', N'vi-VN', N'Free Time tại POD' UNION ALL
    SELECT N'PricingOceanFreight_Col_FreeTime', N'zh-CN', N'目的港免箱期' UNION ALL

    SELECT N'PricingOceanFreight_Col_Remark', N'en-US', N'Remark' UNION ALL
    SELECT N'PricingOceanFreight_Col_Remark', N'vi-VN', N'Ghi chú' UNION ALL
    SELECT N'PricingOceanFreight_Col_Remark', N'zh-CN', N'备注' UNION ALL

    SELECT N'PricingOceanFreight_NoAccessView', N'en-US', N'You do not have permission to view this menu.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessView', N'vi-VN', N'Bạn không có quyền xem menu này.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessView', N'zh-CN', N'您无权查看此菜单。' UNION ALL

    SELECT N'PricingOceanFreight_NoAccessAdd', N'en-US', N'You do not have permission to add.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm mới.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessAdd', N'zh-CN', N'您无权添加。' UNION ALL

    SELECT N'PricingOceanFreight_NoAccessEdit', N'en-US', N'You do not have permission to edit.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessEdit', N'vi-VN', N'Bạn không có quyền chỉnh sửa.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessEdit', N'zh-CN', N'您无权编辑。' UNION ALL

    SELECT N'PricingOceanFreight_NoAccessDelete', N'en-US', N'You do not have permission to delete.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xoá.' UNION ALL
    SELECT N'PricingOceanFreight_NoAccessDelete', N'zh-CN', N'您无权删除。' UNION ALL

    SELECT N'PricingOceanFreight_LoadError', N'en-US', N'Failed to load data: {0}' UNION ALL
    SELECT N'PricingOceanFreight_LoadError', N'vi-VN', N'Tải dữ liệu thất bại: {0}' UNION ALL
    SELECT N'PricingOceanFreight_LoadError', N'zh-CN', N'加载数据失败：{0}' UNION ALL

    SELECT N'PricingOceanFreight_ImportError', N'en-US', N'Import failed: {0}' UNION ALL
    SELECT N'PricingOceanFreight_ImportError', N'vi-VN', N'Nhập dữ liệu thất bại: {0}' UNION ALL
    SELECT N'PricingOceanFreight_ImportError', N'zh-CN', N'导入失败：{0}' UNION ALL

    SELECT N'PricingOceanFreight_ImportSuccess', N'en-US', N'Imported {0} rows successfully.' UNION ALL
    SELECT N'PricingOceanFreight_ImportSuccess', N'vi-VN', N'Nhập thành công {0} dòng.' UNION ALL
    SELECT N'PricingOceanFreight_ImportSuccess', N'zh-CN', N'成功导入 {0} 行。' UNION ALL

    SELECT N'PricingOceanFreight_SelectOneRowToEdit', N'en-US', N'Please select a row to edit.' UNION ALL
    SELECT N'PricingOceanFreight_SelectOneRowToEdit', N'vi-VN', N'Vui lòng chọn một dòng để sửa.' UNION ALL
    SELECT N'PricingOceanFreight_SelectOneRowToEdit', N'zh-CN', N'请选择要编辑的行。' UNION ALL

    SELECT N'PricingOceanFreight_Warning', N'en-US', N'Warning' UNION ALL
    SELECT N'PricingOceanFreight_Warning', N'vi-VN', N'Cảnh báo' UNION ALL
    SELECT N'PricingOceanFreight_Warning', N'zh-CN', N'警告' UNION ALL

    SELECT N'PricingOceanFreight_DeleteConfirm', N'en-US', N'Delete the selected row?' UNION ALL
    SELECT N'PricingOceanFreight_DeleteConfirm', N'vi-VN', N'Xoá dòng đã chọn?' UNION ALL
    SELECT N'PricingOceanFreight_DeleteConfirm', N'zh-CN', N'删除所选行？' UNION ALL

    SELECT N'PricingOceanFreight_NoRowSelected', N'en-US', N'No row selected.' UNION ALL
    SELECT N'PricingOceanFreight_NoRowSelected', N'vi-VN', N'Chưa chọn dòng nào.' UNION ALL
    SELECT N'PricingOceanFreight_NoRowSelected', N'zh-CN', N'未选择任何行。' UNION ALL

    SELECT N'PricingOceanFreight_DeleteFailed', N'en-US', N'Delete failed.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteFailed', N'vi-VN', N'Xoá thất bại.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteFailed', N'zh-CN', N'删除失败。' UNION ALL

    SELECT N'PricingOceanFreight_Deleted', N'en-US', N'Deleted.' UNION ALL
    SELECT N'PricingOceanFreight_Deleted', N'vi-VN', N'Đã xoá.' UNION ALL
    SELECT N'PricingOceanFreight_Deleted', N'zh-CN', N'已删除。' UNION ALL

    SELECT N'PricingOceanFreight_EditDialogTitle', N'en-US', N'Rate details' UNION ALL
    SELECT N'PricingOceanFreight_EditDialogTitle', N'vi-VN', N'Chi tiết giá cước' UNION ALL
    SELECT N'PricingOceanFreight_EditDialogTitle', N'zh-CN', N'费率详情' UNION ALL

    SELECT N'PricingOceanFreight_SaveFailed', N'en-US', N'Save failed.' UNION ALL
    SELECT N'PricingOceanFreight_SaveFailed', N'vi-VN', N'Lưu thất bại.' UNION ALL
    SELECT N'PricingOceanFreight_SaveFailed', N'zh-CN', N'保存失败。' UNION ALL

    SELECT N'PricingOceanFreight_Saved', N'en-US', N'Saved.' UNION ALL
    SELECT N'PricingOceanFreight_Saved', N'vi-VN', N'Đã lưu.' UNION ALL
    SELECT N'PricingOceanFreight_Saved', N'zh-CN', N'已保存。' UNION ALL

    SELECT N'PricingOceanFreight_ImportEmptyFile', N'en-US', N'No data rows were found in the file.' UNION ALL
    SELECT N'PricingOceanFreight_ImportEmptyFile', N'vi-VN', N'Không đọc được dòng dữ liệu nào từ file.' UNION ALL
    SELECT N'PricingOceanFreight_ImportEmptyFile', N'zh-CN', N'文件中未找到数据行。' UNION ALL

    SELECT N'PricingOceanFreight_PreviewTitle', N'en-US', N'Preview data before import' UNION ALL
    SELECT N'PricingOceanFreight_PreviewTitle', N'vi-VN', N'Xem trước dữ liệu trước khi nhập' UNION ALL
    SELECT N'PricingOceanFreight_PreviewTitle', N'zh-CN', N'导入前预览数据' UNION ALL

    SELECT N'PricingOceanFreight_PreviewWarning', N'en-US', N'File "{0}": read {1} rows. Confirming will ADD these as new data (existing data from other files is not touched).' UNION ALL
    SELECT N'PricingOceanFreight_PreviewWarning', N'vi-VN', N'File "{0}": đã đọc được {1} dòng. Xác nhận sẽ THÊM MỚI dữ liệu này (không đụng đến dữ liệu của các file khác đã import trước đó).' UNION ALL
    SELECT N'PricingOceanFreight_PreviewWarning', N'zh-CN', N'文件 "{0}"：已读取 {1} 行。确认后将新增这些数据（不影响其他文件已导入的数据）。' UNION ALL

    SELECT N'PricingOceanFreight_ConfirmImport', N'en-US', N'Confirm import' UNION ALL
    SELECT N'PricingOceanFreight_ConfirmImport', N'vi-VN', N'Xác nhận nhập dữ liệu' UNION ALL
    SELECT N'PricingOceanFreight_ConfirmImport', N'zh-CN', N'确认导入' UNION ALL

    SELECT N'PricingOceanFreight_Col_FileName', N'en-US', N'Source File' UNION ALL
    SELECT N'PricingOceanFreight_Col_FileName', N'vi-VN', N'Tên File' UNION ALL
    SELECT N'PricingOceanFreight_Col_FileName', N'zh-CN', N'来源文件' UNION ALL

    SELECT N'PricingOceanFreight_Col_Lane', N'en-US', N'Trade Lane' UNION ALL
    SELECT N'PricingOceanFreight_Col_Lane', N'vi-VN', N'Tuyến' UNION ALL
    SELECT N'PricingOceanFreight_Col_Lane', N'zh-CN', N'航线' UNION ALL

    SELECT N'PricingOceanFreight_DeleteByLane', N'en-US', N'Delete one lane of a file' UNION ALL
    SELECT N'PricingOceanFreight_DeleteByLane', N'vi-VN', N'Xoá 1 sheet của 1 file' UNION ALL
    SELECT N'PricingOceanFreight_DeleteByLane', N'zh-CN', N'删除某文件的一个航线' UNION ALL

    SELECT N'PricingOceanFreight_DeleteByLaneHint', N'en-US', N'Choose the imported file and the trade lane (sheet) whose data should be deleted. Other lanes of the same file are not affected.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteByLaneHint', N'vi-VN', N'Chọn file đã import và tuyến (sheet) muốn xoá dữ liệu. Các tuyến khác trong cùng file không bị ảnh hưởng.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteByLaneHint', N'zh-CN', N'选择已导入的文件和要删除数据的航线（工作表）。同一文件的其他航线不受影响。' UNION ALL

    SELECT N'PricingOceanFreight_NoImportedFiles', N'en-US', N'No imported file yet.' UNION ALL
    SELECT N'PricingOceanFreight_NoImportedFiles', N'vi-VN', N'Chưa có file nào được import.' UNION ALL
    SELECT N'PricingOceanFreight_NoImportedFiles', N'zh-CN', N'尚未导入任何文件。' UNION ALL

    SELECT N'PricingOceanFreight_FileAlreadyImported', N'en-US', N'File "{0}" was already imported. Delete its old data first (see the imported-files list) before importing it again.' UNION ALL
    SELECT N'PricingOceanFreight_FileAlreadyImported', N'vi-VN', N'File "{0}" đã được import trước đó. Hãy xoá dữ liệu cũ của file này (trong danh sách file đã import) trước khi nhập lại.' UNION ALL
    SELECT N'PricingOceanFreight_FileAlreadyImported', N'zh-CN', N'文件 "{0}" 已经导入过。请先删除该文件的旧数据，然后再重新导入。' UNION ALL

    SELECT N'PricingOceanFreight_StageReading', N'en-US', N'Reading Excel file...' UNION ALL
    SELECT N'PricingOceanFreight_StageReading', N'vi-VN', N'Đang đọc file Excel...' UNION ALL
    SELECT N'PricingOceanFreight_StageReading', N'zh-CN', N'正在读取Excel文件...' UNION ALL

    SELECT N'PricingOceanFreight_StageSaving', N'en-US', N'Saving data...' UNION ALL
    SELECT N'PricingOceanFreight_StageSaving', N'vi-VN', N'Đang lưu dữ liệu...' UNION ALL
    SELECT N'PricingOceanFreight_StageSaving', N'zh-CN', N'正在保存数据...' UNION ALL

    SELECT N'PricingOceanFreight_DeleteManyConfirm', N'en-US', N'Delete data for the selected files?\n{0}\nThis cannot be undone.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteManyConfirm', N'vi-VN', N'Xoá dữ liệu của các file đã chọn?\n{0}\nThao tác không thể hoàn tác.' UNION ALL
    SELECT N'PricingOceanFreight_DeleteManyConfirm', N'zh-CN', N'删除所选文件的数据？\n{0}\n此操作无法撤销。' UNION ALL

    SELECT N'PricingOceanFreight_DeletedRows', N'en-US', N'Deleted {0} rows.' UNION ALL
    SELECT N'PricingOceanFreight_DeletedRows', N'vi-VN', N'Đã xoá {0} dòng.' UNION ALL
    SELECT N'PricingOceanFreight_DeletedRows', N'zh-CN', N'已删除 {0} 行。' UNION ALL

    SELECT N'PricingOceanFreight_SearchPlaceholder', N'en-US', N'Enter search keyword...' UNION ALL
    SELECT N'PricingOceanFreight_SearchPlaceholder', N'vi-VN', N'Nhập từ khoá tìm kiếm...' UNION ALL
    SELECT N'PricingOceanFreight_SearchPlaceholder', N'zh-CN', N'输入搜索关键字...' UNION ALL

    SELECT N'PricingOceanFreight_SearchField_All', N'en-US', N'All attributes' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_All', N'vi-VN', N'Tất cả thuộc tính' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_All', N'zh-CN', N'所有属性' UNION ALL

    SELECT N'PricingOceanFreight_SearchField_Pol', N'en-US', N'POL' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_Pol', N'vi-VN', N'POL' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_Pol', N'zh-CN', N'POL' UNION ALL

    SELECT N'PricingOceanFreight_SearchField_Pod', N'en-US', N'POD' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_Pod', N'vi-VN', N'POD' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_Pod', N'zh-CN', N'POD' UNION ALL

    SELECT N'PricingOceanFreight_SearchField_ImportedBy', N'en-US', N'Imported By' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_ImportedBy', N'vi-VN', N'Người import' UNION ALL
    SELECT N'PricingOceanFreight_SearchField_ImportedBy', N'zh-CN', N'导入人'
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

PRINT N'PricingOceanFreightRate localization imported.';
