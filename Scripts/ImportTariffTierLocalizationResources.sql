/*
  Import LocalizationResources for TariffTier UI — mục 8.6
  Components: TariffTier_Index.razor, TariffTier_EditDialog.razor

  Table: dbo.LocalizationResources (ResourceKey, Culture, Value)
  Cultures: en-US, vi-VN, zh-CN

  Safe to re-run: DELETE keys LIKE N'TariffTier%' then INSERT.

  After import: restart app or clear DbStringLocalizerFactory cache if needed.
*/
SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.LocalizationResources
WHERE ResourceKey LIKE N'TariffTier%';

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value) VALUES
-- Page / menu title
(N'TariffTier', N'en-US', N'Tariff tier'),
(N'TariffTier', N'vi-VN', N'Bậc biểu phí'),
(N'TariffTier', N'zh-CN', N'运价阶梯'),

-- Dialog titles
(N'TariffTier_NewDialogTitle', N'en-US', N'New tariff tier'),
(N'TariffTier_NewDialogTitle', N'vi-VN', N'Thêm bậc biểu phí'),
(N'TariffTier_NewDialogTitle', N'zh-CN', N'新建运价阶梯'),

(N'TariffTier_EditDialogTitle', N'en-US', N'Edit tariff tier'),
(N'TariffTier_EditDialogTitle', N'vi-VN', N'Sửa bậc biểu phí'),
(N'TariffTier_EditDialogTitle', N'zh-CN', N'编辑运价阶梯'),

-- Confirm delete (ShowMessageBox)
(N'TariffTier_Warning', N'en-US', N'Warning'),
(N'TariffTier_Warning', N'vi-VN', N'Cảnh báo'),
(N'TariffTier_Warning', N'zh-CN', N'警告'),

(N'TariffTier_DeleteConfirmBody', N'en-US', N'Delete this tariff tier? This cannot be undone.'),
(N'TariffTier_DeleteConfirmBody', N'vi-VN', N'Xóa bậc biểu phí này? Thao tác không hoàn tác.'),
(N'TariffTier_DeleteConfirmBody', N'zh-CN', N'确定删除此运价阶梯？此操作不可撤销。'),

-- Index / save / delete messages ({0} = exception message)
(N'TariffTier_CannotLoad', N'en-US', N'Cannot load: {0}'),
(N'TariffTier_CannotLoad', N'vi-VN', N'Không tải được: {0}'),
(N'TariffTier_CannotLoad', N'zh-CN', N'无法加载：{0}'),

(N'TariffTier_CannotSave', N'en-US', N'Cannot save: {0}'),
(N'TariffTier_CannotSave', N'vi-VN', N'Không lưu được: {0}'),
(N'TariffTier_CannotSave', N'zh-CN', N'无法保存：{0}'),

(N'TariffTier_CannotDelete', N'en-US', N'Cannot delete: {0}'),
(N'TariffTier_CannotDelete', N'vi-VN', N'Không xóa được: {0}'),
(N'TariffTier_CannotDelete', N'zh-CN', N'无法删除：{0}'),

(N'TariffTier_RecordNotFound', N'en-US', N'Record not found.'),
(N'TariffTier_RecordNotFound', N'vi-VN', N'Không tìm thấy bản ghi.'),
(N'TariffTier_RecordNotFound', N'zh-CN', N'未找到记录。'),

(N'TariffTier_Saved', N'en-US', N'Saved.'),
(N'TariffTier_Saved', N'vi-VN', N'Đã lưu.'),
(N'TariffTier_Saved', N'zh-CN', N'已保存。'),

(N'TariffTier_Deleted', N'en-US', N'Deleted.'),
(N'TariffTier_Deleted', N'vi-VN', N'Đã xóa.'),
(N'TariffTier_Deleted', N'zh-CN', N'已删除。'),

-- Permission snackbars
(N'TariffTier_NoAccessView', N'en-US', N'You do not have permission to view this screen.'),
(N'TariffTier_NoAccessView', N'vi-VN', N'Bạn không có quyền xem'),
(N'TariffTier_NoAccessView', N'zh-CN', N'您没有查看权限'),

(N'TariffTier_NoAccessAdd', N'en-US', N'You do not have permission to add.'),
(N'TariffTier_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm'),
(N'TariffTier_NoAccessAdd', N'zh-CN', N'您没有新增权限'),

(N'TariffTier_NoAccessEdit', N'en-US', N'You do not have permission to edit.'),
(N'TariffTier_NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa'),
(N'TariffTier_NoAccessEdit', N'zh-CN', N'您没有编辑权限'),

(N'TariffTier_NoAccessDelete', N'en-US', N'You do not have permission to delete.'),
(N'TariffTier_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa'),
(N'TariffTier_NoAccessDelete', N'zh-CN', N'您没有删除权限'),

-- Grid column titles (TariffTier_Index)
(N'TariffTier_Col_TariffHeader', N'en-US', N'Tariff'),
(N'TariffTier_Col_TariffHeader', N'vi-VN', N'Biểu phí'),
(N'TariffTier_Col_TariffHeader', N'zh-CN', N'运价'),

(N'TariffTier_Col_FromDay', N'en-US', N'From day'),
(N'TariffTier_Col_FromDay', N'vi-VN', N'Từ ngày thứ'),
(N'TariffTier_Col_FromDay', N'zh-CN', N'起始天'),

(N'TariffTier_Col_ToDay', N'en-US', N'To day'),
(N'TariffTier_Col_ToDay', N'vi-VN', N'Đến ngày thứ'),
(N'TariffTier_Col_ToDay', N'zh-CN', N'结束天'),

(N'TariffTier_Col_SequenceNo', N'en-US', N'Sequence'),
(N'TariffTier_Col_SequenceNo', N'vi-VN', N'Thứ tự'),
(N'TariffTier_Col_SequenceNo', N'zh-CN', N'序号'),

(N'TariffTier_Col_Rate', N'en-US', N'Rate'),
(N'TariffTier_Col_Rate', N'vi-VN', N'Đơn giá'),
(N'TariffTier_Col_Rate', N'zh-CN', N'费率'),

(N'TariffTier_Col_RateBasis', N'en-US', N'Rate basis'),
(N'TariffTier_Col_RateBasis', N'vi-VN', N'Cơ sở tính'),
(N'TariffTier_Col_RateBasis', N'zh-CN', N'计费基础'),

(N'TariffTier_Col_Unit', N'en-US', N'Unit'),
(N'TariffTier_Col_Unit', N'vi-VN', N'Đơn vị'),
(N'TariffTier_Col_Unit', N'zh-CN', N'单位'),

(N'TariffTier_Col_MinCharge', N'en-US', N'Min. charge'),
(N'TariffTier_Col_MinCharge', N'vi-VN', N'Phí tối thiểu'),
(N'TariffTier_Col_MinCharge', N'zh-CN', N'最低费用'),

(N'TariffTier_Col_Notes', N'en-US', N'Notes'),
(N'TariffTier_Col_Notes', N'vi-VN', N'Ghi chú'),
(N'TariffTier_Col_Notes', N'zh-CN', N'备注'),

-- Form labels (TariffTier_EditDialog)
(N'TariffTier_Lbl_TariffHeader', N'en-US', N'Tariff (name or code)'),
(N'TariffTier_Lbl_TariffHeader', N'vi-VN', N'Biểu phí (tên hoặc mã)'),
(N'TariffTier_Lbl_TariffHeader', N'zh-CN', N'运价（名称或代码）'),

(N'TariffTier_Lbl_FromDay', N'en-US', N'From day'),
(N'TariffTier_Lbl_FromDay', N'vi-VN', N'Từ ngày thứ'),
(N'TariffTier_Lbl_FromDay', N'zh-CN', N'起始天'),

(N'TariffTier_Lbl_ToDay', N'en-US', N'To day'),
(N'TariffTier_Lbl_ToDay', N'vi-VN', N'Đến ngày thứ'),
(N'TariffTier_Lbl_ToDay', N'zh-CN', N'结束天'),

(N'TariffTier_Lbl_SequenceNo', N'en-US', N'Sequence'),
(N'TariffTier_Lbl_SequenceNo', N'vi-VN', N'Thứ tự'),
(N'TariffTier_Lbl_SequenceNo', N'zh-CN', N'序号'),

(N'TariffTier_Lbl_Rate', N'en-US', N'Rate'),
(N'TariffTier_Lbl_Rate', N'vi-VN', N'Đơn giá'),
(N'TariffTier_Lbl_Rate', N'zh-CN', N'费率'),

(N'TariffTier_Lbl_RateBasis', N'en-US', N'Rate basis'),
(N'TariffTier_Lbl_RateBasis', N'vi-VN', N'Cơ sở tính'),
(N'TariffTier_Lbl_RateBasis', N'zh-CN', N'计费基础'),

(N'TariffTier_Lbl_Unit', N'en-US', N'Unit'),
(N'TariffTier_Lbl_Unit', N'vi-VN', N'Đơn vị'),
(N'TariffTier_Lbl_Unit', N'zh-CN', N'单位'),

(N'TariffTier_Lbl_MinCharge', N'en-US', N'Min. charge'),
(N'TariffTier_Lbl_MinCharge', N'vi-VN', N'Phí tối thiểu'),
(N'TariffTier_Lbl_MinCharge', N'zh-CN', N'最低费用'),

(N'TariffTier_Lbl_Notes', N'en-US', N'Notes'),
(N'TariffTier_Lbl_Notes', N'vi-VN', N'Ghi chú'),
(N'TariffTier_Lbl_Notes', N'zh-CN', N'备注'),

-- Validation (dialog)
(N'TariffTier_Msg_SelectHeader', N'en-US', N'Please select a tariff header.'),
(N'TariffTier_Msg_SelectHeader', N'vi-VN', N'Vui lòng chọn biểu phí.'),
(N'TariffTier_Msg_SelectHeader', N'zh-CN', N'请选择运价表头。'),

(N'TariffTier_SearchHint', N'en-US', N'Search tariff, days, rate, basis, notes…'),
(N'TariffTier_SearchHint', N'vi-VN', N'Tìm biểu phí, ngày, đơn giá, cơ sở tính, ghi chú…'),
(N'TariffTier_SearchHint', N'zh-CN', N'搜索运价、天数、费率、计费基础、备注…');

COMMIT TRANSACTION;
PRINT N'ImportTariffTierLocalizationResources (8.6 TariffTier): done.';
