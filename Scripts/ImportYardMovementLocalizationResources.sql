/*
  5.8.9 Yard Movement — LocalizationResources (en-US, vi-VN, zh-CN)
  Component: ExcelImportCrud_Index.razor

  Safe to re-run: DELETE ResourceKey LIKE N'Ym_%' then INSERT.
*/
SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.LocalizationResources
WHERE ResourceKey LIKE N'Ym_%';

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value) VALUES
(N'Ym_Title', N'en-US', N'Yard Movement'),
(N'Ym_Title', N'vi-VN', N'Yard Movement'),
(N'Ym_Title', N'zh-CN', N'堆场动态'),

(N'Ym_Tab_HDS', N'en-US', N'HDS'),
(N'Ym_Tab_HDS', N'vi-VN', N'HDS'),
(N'Ym_Tab_HDS', N'zh-CN', N'HDS'),

(N'Ym_Tab_AG', N'en-US', N'AG'),
(N'Ym_Tab_AG', N'vi-VN', N'AG'),
(N'Ym_Tab_AG', N'zh-CN', N'AG'),

(N'Ym_Tab_VSS', N'en-US', N'VSS'),
(N'Ym_Tab_VSS', N'vi-VN', N'VSS'),
(N'Ym_Tab_VSS', N'zh-CN', N'VSS'),

(N'Ym_Tab_AMS', N'en-US', N'AMS'),
(N'Ym_Tab_AMS', N'vi-VN', N'AMS'),
(N'Ym_Tab_AMS', N'zh-CN', N'AMS'),

(N'Ym_Col_SoCont', N'en-US', N'Container No.'),
(N'Ym_Col_SoCont', N'vi-VN', N'Số cont'),
(N'Ym_Col_SoCont', N'zh-CN', N'箱号'),

(N'Ym_Col_TenTau', N'en-US', N'Vessel'),
(N'Ym_Col_TenTau', N'vi-VN', N'Tên tàu'),
(N'Ym_Col_TenTau', N'zh-CN', N'船名'),

(N'Ym_Col_NgayCapBen', N'en-US', N'ETA'),
(N'Ym_Col_NgayCapBen', N'vi-VN', N'Ngày cập bến'),
(N'Ym_Col_NgayCapBen', N'zh-CN', N'靠泊时间'),

(N'Ym_Col_SoBooking', N'en-US', N'Booking No.'),
(N'Ym_Col_SoBooking', N'vi-VN', N'Số booking'),
(N'Ym_Col_SoBooking', N'zh-CN', N'订舱号'),

(N'Ym_Col_SoBL', N'en-US', N'B/L No.'),
(N'Ym_Col_SoBL', N'vi-VN', N'Số B/L'),
(N'Ym_Col_SoBL', N'zh-CN', N'提单号'),

(N'Ym_Col_PhuongAn', N'en-US', N'Plan'),
(N'Ym_Col_PhuongAn', N'vi-VN', N'Phương án'),
(N'Ym_Col_PhuongAn', N'zh-CN', N'作业方式'),

(N'Ym_Col_TTHaiQuan', N'en-US', N'Customs Status'),
(N'Ym_Col_TTHaiQuan', N'vi-VN', N'TT hải quan'),
(N'Ym_Col_TTHaiQuan', N'zh-CN', N'海关状态'),

(N'Ym_Col_SourceSheet', N'en-US', N'Source Sheet'),
(N'Ym_Col_SourceSheet', N'vi-VN', N'Sheet nguồn'),
(N'Ym_Col_SourceSheet', N'zh-CN', N'来源工作表'),

(N'Ym_Col_CNTRNO', N'en-US', N'Container No.'),
(N'Ym_Col_CNTRNO', N'vi-VN', N'Số cont'),
(N'Ym_Col_CNTRNO', N'zh-CN', N'箱号'),

(N'Ym_Col_SZ', N'en-US', N'Size'),
(N'Ym_Col_SZ', N'vi-VN', N'Kích cỡ'),
(N'Ym_Col_SZ', N'zh-CN', N'尺寸'),

(N'Ym_Col_TP', N'en-US', N'Type'),
(N'Ym_Col_TP', N'vi-VN', N'Loại'),
(N'Ym_Col_TP', N'zh-CN', N'类型'),

(N'Ym_Col_ST', N'en-US', N'Status'),
(N'Ym_Col_ST', N'vi-VN', N'Trạng thái'),
(N'Ym_Col_ST', N'zh-CN', N'状态'),

(N'Ym_Col_Location', N'en-US', N'Location'),
(N'Ym_Col_Location', N'vi-VN', N'Vị trí'),
(N'Ym_Col_Location', N'zh-CN', N'位置'),

(N'Ym_Col_Customer', N'en-US', N'Customer'),
(N'Ym_Col_Customer', N'vi-VN', N'Khách hàng'),
(N'Ym_Col_Customer', N'zh-CN', N'客户'),

(N'Ym_Col_SOCONT', N'en-US', N'Container No.'),
(N'Ym_Col_SOCONT', N'vi-VN', N'Số cont'),
(N'Ym_Col_SOCONT', N'zh-CN', N'箱号'),

(N'Ym_Col_KICHCO', N'en-US', N'Size'),
(N'Ym_Col_KICHCO', N'vi-VN', N'Kích cỡ'),
(N'Ym_Col_KICHCO', N'zh-CN', N'尺寸'),

(N'Ym_Col_TRANGTHAI', N'en-US', N'Status'),
(N'Ym_Col_TRANGTHAI', N'vi-VN', N'Trạng thái'),
(N'Ym_Col_TRANGTHAI', N'zh-CN', N'状态'),

(N'Ym_Col_EXEC_TS', N'en-US', N'Execution Time'),
(N'Ym_Col_EXEC_TS', N'vi-VN', N'Thời gian thực hiện'),
(N'Ym_Col_EXEC_TS', N'zh-CN', N'执行时间'),

(N'Ym_Col_BOOK_NO', N'en-US', N'Booking No.'),
(N'Ym_Col_BOOK_NO', N'vi-VN', N'Số booking'),
(N'Ym_Col_BOOK_NO', N'zh-CN', N'订舱号'),

(N'Ym_Col_GIAO', N'en-US', N'Deliver/Receive'),
(N'Ym_Col_GIAO', N'vi-VN', N'Giao/Nhận'),
(N'Ym_Col_GIAO', N'zh-CN', N'交付/接收'),

(N'Ym_ImportExcel', N'en-US', N'Import Excel'),
(N'Ym_ImportExcel', N'vi-VN', N'Import Excel'),
(N'Ym_ImportExcel', N'zh-CN', N'导入 Excel'),

(N'Ym_NoAccessView', N'en-US', N'You do not have permission to view Yard Movement.'),
(N'Ym_NoAccessView', N'vi-VN', N'Bạn không có quyền xem Yard Movement.'),
(N'Ym_NoAccessView', N'zh-CN', N'您没有查看 Yard Movement 的权限。'),

(N'Ym_NoAccessAdd', N'en-US', N'You do not have permission to add/import data.'),
(N'Ym_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm/import dữ liệu.'),
(N'Ym_NoAccessAdd', N'zh-CN', N'您没有新增/导入数据权限。'),

(N'Ym_NoAccessEdit', N'en-US', N'You do not have permission to edit data.'),
(N'Ym_NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa dữ liệu.'),
(N'Ym_NoAccessEdit', N'zh-CN', N'您没有编辑数据权限。'),

(N'Ym_NoAccessDelete', N'en-US', N'You do not have permission to delete data.'),
(N'Ym_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa dữ liệu.'),
(N'Ym_NoAccessDelete', N'zh-CN', N'您没有删除数据权限。'),

(N'Ym_LoadError', N'en-US', N'Load error: {0}'),
(N'Ym_LoadError', N'vi-VN', N'Lỗi tải dữ liệu: {0}'),
(N'Ym_LoadError', N'zh-CN', N'加载错误：{0}'),

(N'Ym_ImportError', N'en-US', N'Import error: {0}'),
(N'Ym_ImportError', N'vi-VN', N'Import lỗi: {0}'),
(N'Ym_ImportError', N'zh-CN', N'导入错误：{0}'),

(N'Ym_ImportSuccess', N'en-US', N'Import successful: {0} row(s).'),
(N'Ym_ImportSuccess', N'vi-VN', N'Import thành công: {0} dòng.'),
(N'Ym_ImportSuccess', N'zh-CN', N'导入成功：{0} 行。'),

(N'Ym_SelectOneRowToEdit', N'en-US', N'Please select one row to edit.'),
(N'Ym_SelectOneRowToEdit', N'vi-VN', N'Hãy chọn một dòng để sửa.'),
(N'Ym_SelectOneRowToEdit', N'zh-CN', N'请选择一行进行编辑。'),

(N'Ym_Warning', N'en-US', N'Warning'),
(N'Ym_Warning', N'vi-VN', N'Cảnh báo'),
(N'Ym_Warning', N'zh-CN', N'警告'),

(N'Ym_DeleteConfirm', N'en-US', N'Are you sure you want to delete the selected row?'),
(N'Ym_DeleteConfirm', N'vi-VN', N'Bạn có chắc chắn muốn xóa dòng đã chọn?'),
(N'Ym_DeleteConfirm', N'zh-CN', N'确定要删除选中的行吗？'),

(N'Ym_NoRowSelected', N'en-US', N'No row selected.'),
(N'Ym_NoRowSelected', N'vi-VN', N'Không có dòng được chọn.'),
(N'Ym_NoRowSelected', N'zh-CN', N'未选择任何行。'),

(N'Ym_DeleteFailed', N'en-US', N'Delete failed.'),
(N'Ym_DeleteFailed', N'vi-VN', N'Xóa thất bại.'),
(N'Ym_DeleteFailed', N'zh-CN', N'删除失败。'),

(N'Ym_Deleted', N'en-US', N'Deleted successfully.'),
(N'Ym_Deleted', N'vi-VN', N'Đã xóa.'),
(N'Ym_Deleted', N'zh-CN', N'删除成功。'),

(N'Ym_EditDialogTitle', N'en-US', N'Update data'),
(N'Ym_EditDialogTitle', N'vi-VN', N'Cập nhật dữ liệu'),
(N'Ym_EditDialogTitle', N'zh-CN', N'更新数据'),

(N'Ym_SaveFailed', N'en-US', N'Save failed.'),
(N'Ym_SaveFailed', N'vi-VN', N'Lưu thất bại.'),
(N'Ym_SaveFailed', N'zh-CN', N'保存失败。'),

(N'Ym_Saved', N'en-US', N'Saved successfully.'),
(N'Ym_Saved', N'vi-VN', N'Lưu thành công.'),
(N'Ym_Saved', N'zh-CN', N'保存成功。');

COMMIT TRANSACTION;
PRINT N'ImportYardMovementLocalizationResources: done.';
