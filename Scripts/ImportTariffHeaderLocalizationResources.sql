/*
  Import LocalizationResources keys for TariffHeader UI (en-US, vi-VN, zh-CN).
  Table: dbo.LocalizationResources (ResourceKey, Culture, Value).
  Safe to re-run: DELETE keys starting with TariffHeader then INSERT.

  After import: restart app or clear DbStringLocalizerFactory cache if needed.
*/
SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.LocalizationResources
WHERE ResourceKey LIKE N'TariffHeader%';

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value) VALUES
-- Page / menu
(N'TariffHeader', N'en-US', N'Tariff header'),
(N'TariffHeader', N'vi-VN', N'Biểu phí (header)'),
(N'TariffHeader', N'zh-CN', N'运价表头'),

-- Messages (Index)
(N'TariffHeader_Warning', N'en-US', N'Warning'),
(N'TariffHeader_Warning', N'vi-VN', N'Cảnh báo'),
(N'TariffHeader_Warning', N'zh-CN', N'警告'),

(N'TariffHeader_DeleteConfirmBody', N'en-US', N'Delete this tariff and its tier lines? This cannot be undone.'),
(N'TariffHeader_DeleteConfirmBody', N'vi-VN', N'Xóa biểu phí này và các dòng bậc thang? Thao tác không hoàn tác.'),
(N'TariffHeader_DeleteConfirmBody', N'zh-CN', N'确定删除此运价及其阶梯明细？此操作不可撤销。'),

(N'TariffHeader_CannotLoad', N'en-US', N'Cannot load: {0}'),
(N'TariffHeader_CannotLoad', N'vi-VN', N'Không tải được: {0}'),
(N'TariffHeader_CannotLoad', N'zh-CN', N'无法加载：{0}'),

(N'TariffHeader_CannotSave', N'en-US', N'Cannot save: {0}'),
(N'TariffHeader_CannotSave', N'vi-VN', N'Không lưu được: {0}'),
(N'TariffHeader_CannotSave', N'zh-CN', N'无法保存：{0}'),

(N'TariffHeader_CannotDelete', N'en-US', N'Cannot delete: {0}'),
(N'TariffHeader_CannotDelete', N'vi-VN', N'Không xóa được: {0}'),
(N'TariffHeader_CannotDelete', N'zh-CN', N'无法删除：{0}'),

(N'TariffHeader_RecordNotFound', N'en-US', N'Record not found.'),
(N'TariffHeader_RecordNotFound', N'vi-VN', N'Không tìm thấy bản ghi.'),
(N'TariffHeader_RecordNotFound', N'zh-CN', N'未找到记录。'),

(N'TariffHeader_Saved', N'en-US', N'Saved.'),
(N'TariffHeader_Saved', N'vi-VN', N'Đã lưu.'),
(N'TariffHeader_Saved', N'zh-CN', N'已保存。'),

(N'TariffHeader_Deleted', N'en-US', N'Deleted.'),
(N'TariffHeader_Deleted', N'vi-VN', N'Đã xóa.'),
(N'TariffHeader_Deleted', N'zh-CN', N'已删除。'),

-- Permission snackbars (optional: bind in Razor with Localizer)
(N'TariffHeader_NoAccessView', N'en-US', N'You have not access to view'),
(N'TariffHeader_NoAccessView', N'vi-VN', N'Bạn không có quyền xem'),
(N'TariffHeader_NoAccessView', N'zh-CN', N'您没有查看权限'),

(N'TariffHeader_NoAccessAdd', N'en-US', N'You have not access to ADD'),
(N'TariffHeader_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm'),
(N'TariffHeader_NoAccessAdd', N'zh-CN', N'您没有新增权限'),

(N'TariffHeader_NoAccessEdit', N'en-US', N'You have not access to EDIT'),
(N'TariffHeader_NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa'),
(N'TariffHeader_NoAccessEdit', N'zh-CN', N'您没有编辑权限'),

(N'TariffHeader_NoAccessDelete', N'en-US', N'You have not access to Delete'),
(N'TariffHeader_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa'),
(N'TariffHeader_NoAccessDelete', N'zh-CN', N'您没有删除权限'),

-- Grid column titles
(N'TariffHeader_Col_TariffCode', N'en-US', N'Tariff code'),
(N'TariffHeader_Col_TariffCode', N'vi-VN', N'Mã biểu phí'),
(N'TariffHeader_Col_TariffCode', N'zh-CN', N'运价代码'),

(N'TariffHeader_Col_TariffName', N'en-US', N'Tariff name'),
(N'TariffHeader_Col_TariffName', N'vi-VN', N'Tên biểu phí'),
(N'TariffHeader_Col_TariffName', N'zh-CN', N'运价名称'),

(N'TariffHeader_Col_ChargeType', N'en-US', N'Charge type'),
(N'TariffHeader_Col_ChargeType', N'vi-VN', N'Loại phí'),
(N'TariffHeader_Col_ChargeType', N'zh-CN', N'费用类型'),

(N'TariffHeader_Col_Customer', N'en-US', N'Customer'),
(N'TariffHeader_Col_Customer', N'vi-VN', N'Khách hàng'),
(N'TariffHeader_Col_Customer', N'zh-CN', N'客户'),

(N'TariffHeader_Col_ShippingLine', N'en-US', N'Shipping line'),
(N'TariffHeader_Col_ShippingLine', N'vi-VN', N'Hãng tàu'),
(N'TariffHeader_Col_ShippingLine', N'zh-CN', N'船公司'),

(N'TariffHeader_Col_Depot', N'en-US', N'Depot'),
(N'TariffHeader_Col_Depot', N'vi-VN', N'Cảng / depot'),
(N'TariffHeader_Col_Depot', N'zh-CN', N'堆场/港口'),

(N'TariffHeader_Col_Terminal', N'en-US', N'Terminal'),
(N'TariffHeader_Col_Terminal', N'vi-VN', N'Cảng cạn / terminal'),
(N'TariffHeader_Col_Terminal', N'zh-CN', N'码头'),

(N'TariffHeader_Col_ContType', N'en-US', N'Cont type'),
(N'TariffHeader_Col_ContType', N'vi-VN', N'Loại cont'),
(N'TariffHeader_Col_ContType', N'zh-CN', N'柜型'),

(N'TariffHeader_Col_CargoType', N'en-US', N'Cargo type'),
(N'TariffHeader_Col_CargoType', N'vi-VN', N'Loại hàng'),
(N'TariffHeader_Col_CargoType', N'zh-CN', N'货类'),

(N'TariffHeader_Col_Direction', N'en-US', N'Direction'),
(N'TariffHeader_Col_Direction', N'vi-VN', N'Hướng'),
(N'TariffHeader_Col_Direction', N'zh-CN', N'方向'),

(N'TariffHeader_Col_Currency', N'en-US', N'Curr.'),
(N'TariffHeader_Col_Currency', N'vi-VN', N'Tiền tệ'),
(N'TariffHeader_Col_Currency', N'zh-CN', N'币种'),

(N'TariffHeader_Col_EffectiveFrom', N'en-US', N'From'),
(N'TariffHeader_Col_EffectiveFrom', N'vi-VN', N'Từ ngày'),
(N'TariffHeader_Col_EffectiveFrom', N'zh-CN', N'生效起'),

(N'TariffHeader_Col_EffectiveTo', N'en-US', N'To'),
(N'TariffHeader_Col_EffectiveTo', N'vi-VN', N'Đến ngày'),
(N'TariffHeader_Col_EffectiveTo', N'zh-CN', N'生效止'),

(N'TariffHeader_Col_Status', N'en-US', N'Status'),
(N'TariffHeader_Col_Status', N'vi-VN', N'Trạng thái'),
(N'TariffHeader_Col_Status', N'zh-CN', N'状态'),

-- Edit dialog labels
(N'TariffHeader_Lbl_TariffCode', N'en-US', N'Tariff code'),
(N'TariffHeader_Lbl_TariffCode', N'vi-VN', N'Mã biểu phí'),
(N'TariffHeader_Lbl_TariffCode', N'zh-CN', N'运价代码'),

(N'TariffHeader_Lbl_TariffName', N'en-US', N'Tariff name'),
(N'TariffHeader_Lbl_TariffName', N'vi-VN', N'Tên biểu phí'),
(N'TariffHeader_Lbl_TariffName', N'zh-CN', N'运价名称'),

(N'TariffHeader_Lbl_ChargeType', N'en-US', N'Charge type (code - name)'),
(N'TariffHeader_Lbl_ChargeType', N'vi-VN', N'Loại phí (mã - tên)'),
(N'TariffHeader_Lbl_ChargeType', N'zh-CN', N'费用类型（代码-名称）'),

(N'TariffHeader_Lbl_Customer', N'en-US', N'Customer (English name)'),
(N'TariffHeader_Lbl_Customer', N'vi-VN', N'Khách hàng (tên tiếng Anh)'),
(N'TariffHeader_Lbl_Customer', N'zh-CN', N'客户（英文名）'),

(N'TariffHeader_Lbl_ShippingLine', N'en-US', N'Shipping line (English name)'),
(N'TariffHeader_Lbl_ShippingLine', N'vi-VN', N'Hãng tàu (tên tiếng Anh)'),
(N'TariffHeader_Lbl_ShippingLine', N'zh-CN', N'船公司（英文名）'),

(N'TariffHeader_Lbl_Depot', N'en-US', N'Depot (port)'),
(N'TariffHeader_Lbl_Depot', N'vi-VN', N'Depot (cảng)'),
(N'TariffHeader_Lbl_Depot', N'zh-CN', N'堆场（港口）'),

(N'TariffHeader_Lbl_Terminal', N'en-US', N'Terminal'),
(N'TariffHeader_Lbl_Terminal', N'vi-VN', N'Terminal'),
(N'TariffHeader_Lbl_Terminal', N'zh-CN', N'码头'),

(N'TariffHeader_Lbl_ContainerType', N'en-US', N'Container type'),
(N'TariffHeader_Lbl_ContainerType', N'vi-VN', N'Loại container'),
(N'TariffHeader_Lbl_ContainerType', N'zh-CN', N'柜型'),

(N'TariffHeader_Lbl_CargoType', N'en-US', N'Cargo type'),
(N'TariffHeader_Lbl_CargoType', N'vi-VN', N'Loại hàng'),
(N'TariffHeader_Lbl_CargoType', N'zh-CN', N'货类'),

(N'TariffHeader_Lbl_Direction', N'en-US', N'Direction'),
(N'TariffHeader_Lbl_Direction', N'vi-VN', N'Hướng'),
(N'TariffHeader_Lbl_Direction', N'zh-CN', N'方向'),

(N'TariffHeader_Lbl_Currency', N'en-US', N'Currency'),
(N'TariffHeader_Lbl_Currency', N'vi-VN', N'Tiền tệ'),
(N'TariffHeader_Lbl_Currency', N'zh-CN', N'币种'),

(N'TariffHeader_Lbl_EffectiveFrom', N'en-US', N'Effective from'),
(N'TariffHeader_Lbl_EffectiveFrom', N'vi-VN', N'Hiệu lực từ'),
(N'TariffHeader_Lbl_EffectiveFrom', N'zh-CN', N'生效日期'),

(N'TariffHeader_Lbl_EffectiveTo', N'en-US', N'Effective to'),
(N'TariffHeader_Lbl_EffectiveTo', N'vi-VN', N'Hiệu lực đến'),
(N'TariffHeader_Lbl_EffectiveTo', N'zh-CN', N'失效日期'),

(N'TariffHeader_Lbl_Status', N'en-US', N'Status'),
(N'TariffHeader_Lbl_Status', N'vi-VN', N'Trạng thái'),
(N'TariffHeader_Lbl_Status', N'zh-CN', N'状态'),

(N'TariffHeader_Lbl_Notes', N'en-US', N'Notes'),
(N'TariffHeader_Lbl_Notes', N'vi-VN', N'Ghi chú'),
(N'TariffHeader_Lbl_Notes', N'zh-CN', N'备注'),

-- Edit dialog validation (Snackbar)
(N'TariffHeader_Msg_SelectLinks', N'en-US', N'Please select charge type, customer, shipping line, depot and terminal.'),
(N'TariffHeader_Msg_SelectLinks', N'vi-VN', N'Vui lòng chọn loại phí, khách hàng, hãng tàu, depot và terminal.'),
(N'TariffHeader_Msg_SelectLinks', N'zh-CN', N'请选择费用类型、客户、船公司、堆场和码头。'),

(N'TariffHeader_Msg_ContCargoRequired', N'en-US', N'Container type and cargo type are required.'),
(N'TariffHeader_Msg_ContCargoRequired', N'vi-VN', N'Bắt buộc chọn loại container và loại hàng.'),
(N'TariffHeader_Msg_ContCargoRequired', N'zh-CN', N'柜型和货类为必填。'),

(N'TariffHeader_Msg_EffectiveFromRequired', N'en-US', N'Effective from is required.'),
(N'TariffHeader_Msg_EffectiveFromRequired', N'vi-VN', N'Bắt buộc chọn ngày hiệu lực từ.'),
(N'TariffHeader_Msg_EffectiveFromRequired', N'zh-CN', N'生效起始日期为必填。'),

-- Status options (dialog MudSelect)
(N'TariffHeader_Status_Active', N'en-US', N'Active'),
(N'TariffHeader_Status_Active', N'vi-VN', N'Hiệu lực'),
(N'TariffHeader_Status_Active', N'zh-CN', N'启用'),

(N'TariffHeader_Status_Inactive', N'en-US', N'Inactive'),
(N'TariffHeader_Status_Inactive', N'vi-VN', N'Ngưng'),
(N'TariffHeader_Status_Inactive', N'zh-CN', N'停用'),

(N'TariffHeader_Status_Draft', N'en-US', N'Draft'),
(N'TariffHeader_Status_Draft', N'vi-VN', N'Nháp'),
(N'TariffHeader_Status_Draft', N'zh-CN', N'草稿'),

(N'TariffHeader_Status_Closed', N'en-US', N'Closed'),
(N'TariffHeader_Status_Closed', N'vi-VN', N'Đóng'),
(N'TariffHeader_Status_Closed', N'zh-CN', N'关闭'),

(N'TariffHeader_SearchHint', N'en-US', N'Search code, name, links, dates, status…'),
(N'TariffHeader_SearchHint', N'vi-VN', N'Tìm mã, tên, liên kết, ngày, trạng thái…'),
(N'TariffHeader_SearchHint', N'zh-CN', N'搜索代码、名称、关联、日期、状态…');

COMMIT TRANSACTION;

PRINT N'ImportTariffHeaderLocalizationResources: inserted TariffHeader* keys for en-US, vi-VN, zh-CN.';
