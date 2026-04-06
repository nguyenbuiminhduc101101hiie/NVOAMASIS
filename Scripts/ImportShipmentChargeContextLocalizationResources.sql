/*
  8.7 ShipmentChargeContext — LocalizationResources (en-US, vi-VN, zh-CN).
  Components: ShipmentChargeContext_Index.razor, ShipmentChargeContext_EditDialog.razor

  Safe to re-run: DELETE ResourceKey LIKE N'Scc%' then INSERT.
*/
SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.LocalizationResources
WHERE ResourceKey LIKE N'Scc%';

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value) VALUES
(N'Scc_Title', N'en-US', N'Shipment charge context'),
(N'Scc_Title', N'vi-VN', N'Ngữ cảnh tính phí lô'),
(N'Scc_Title', N'zh-CN', N'货运计费上下文'),

(N'Scc_NewDialogTitle', N'en-US', N'New shipment charge context'),
(N'Scc_NewDialogTitle', N'vi-VN', N'Thêm ngữ cảnh tính phí'),
(N'Scc_NewDialogTitle', N'zh-CN', N'新建计费上下文'),

(N'Scc_EditDialogTitle', N'en-US', N'Edit shipment charge context'),
(N'Scc_EditDialogTitle', N'vi-VN', N'Sửa ngữ cảnh tính phí'),
(N'Scc_EditDialogTitle', N'zh-CN', N'编辑计费上下文'),

(N'Scc_Warning', N'en-US', N'Warning'),
(N'Scc_Warning', N'vi-VN', N'Cảnh báo'),
(N'Scc_Warning', N'zh-CN', N'警告'),

(N'Scc_DeleteConfirmBody', N'en-US', N'Delete this record? This cannot be undone.'),
(N'Scc_DeleteConfirmBody', N'vi-VN', N'Xóa bản ghi này? Thao tác không hoàn tác.'),
(N'Scc_DeleteConfirmBody', N'zh-CN', N'确定删除此记录？不可撤销。'),

(N'Scc_CannotLoad', N'en-US', N'Cannot load: {0}'),
(N'Scc_CannotLoad', N'vi-VN', N'Không tải được: {0}'),
(N'Scc_CannotLoad', N'zh-CN', N'无法加载：{0}'),

(N'Scc_CannotSave', N'en-US', N'Cannot save: {0}'),
(N'Scc_CannotSave', N'vi-VN', N'Không lưu được: {0}'),
(N'Scc_CannotSave', N'zh-CN', N'无法保存：{0}'),

(N'Scc_CannotDelete', N'en-US', N'Cannot delete: {0}'),
(N'Scc_CannotDelete', N'vi-VN', N'Không xóa được: {0}'),
(N'Scc_CannotDelete', N'zh-CN', N'无法删除：{0}'),

(N'Scc_RecordNotFound', N'en-US', N'Record not found.'),
(N'Scc_RecordNotFound', N'vi-VN', N'Không tìm thấy bản ghi.'),
(N'Scc_RecordNotFound', N'zh-CN', N'未找到记录。'),

(N'Scc_Saved', N'en-US', N'Saved.'),
(N'Scc_Saved', N'vi-VN', N'Đã lưu.'),
(N'Scc_Saved', N'zh-CN', N'已保存。'),

(N'Scc_Deleted', N'en-US', N'Deleted.'),
(N'Scc_Deleted', N'vi-VN', N'Đã xóa.'),
(N'Scc_Deleted', N'zh-CN', N'已删除。'),

(N'Scc_NoAccessView', N'en-US', N'You do not have permission to view this screen.'),
(N'Scc_NoAccessView', N'vi-VN', N'Bạn không có quyền xem'),
(N'Scc_NoAccessView', N'zh-CN', N'您没有查看权限'),

(N'Scc_NoAccessAdd', N'en-US', N'You do not have permission to add.'),
(N'Scc_NoAccessAdd', N'vi-VN', N'Bạn không có quyền thêm'),
(N'Scc_NoAccessAdd', N'zh-CN', N'您没有新增权限'),

(N'Scc_NoAccessEdit', N'en-US', N'You do not have permission to edit.'),
(N'Scc_NoAccessEdit', N'vi-VN', N'Bạn không có quyền sửa'),
(N'Scc_NoAccessEdit', N'zh-CN', N'您没有编辑权限'),

(N'Scc_NoAccessDelete', N'en-US', N'You do not have permission to delete.'),
(N'Scc_NoAccessDelete', N'vi-VN', N'Bạn không có quyền xóa'),
(N'Scc_NoAccessDelete', N'zh-CN', N'您没有删除权限'),

(N'Scc_SearchHint', N'en-US', N'Search HBL, container, charge, customer…'),
(N'Scc_SearchHint', N'vi-VN', N'Tìm HBL, container, loại phí, khách…'),
(N'Scc_SearchHint', N'zh-CN', N'搜索提单、集装箱、费用、客户…'),

(N'Scc_Col_HBL', N'en-US', N'HBL'),
(N'Scc_Col_HBL', N'vi-VN', N'Số HBL'),
(N'Scc_Col_HBL', N'zh-CN', N'分提单号'),

(N'Scc_Col_Container', N'en-US', N'Container'),
(N'Scc_Col_Container', N'vi-VN', N'Số container'),
(N'Scc_Col_Container', N'zh-CN', N'箱号'),

(N'Scc_Col_ChargeType', N'en-US', N'Charge type'),
(N'Scc_Col_ChargeType', N'vi-VN', N'Loại phí'),
(N'Scc_Col_ChargeType', N'zh-CN', N'费用类型'),

(N'Scc_Col_Customer', N'en-US', N'Customer'),
(N'Scc_Col_Customer', N'vi-VN', N'Khách hàng'),
(N'Scc_Col_Customer', N'zh-CN', N'客户'),

(N'Scc_Col_ShippingLine', N'en-US', N'Shipping line'),
(N'Scc_Col_ShippingLine', N'vi-VN', N'Hãng tàu'),
(N'Scc_Col_ShippingLine', N'zh-CN', N'船公司'),

(N'Scc_Col_Depot', N'en-US', N'Depot / port'),
(N'Scc_Col_Depot', N'vi-VN', N'Cảng / depot'),
(N'Scc_Col_Depot', N'zh-CN', N'港口/堆场'),

(N'Scc_Col_Terminal', N'en-US', N'Terminal'),
(N'Scc_Col_Terminal', N'vi-VN', N'Cảng cạn / terminal'),
(N'Scc_Col_Terminal', N'zh-CN', N'码头'),

(N'Scc_Col_ContType', N'en-US', N'Cont. type'),
(N'Scc_Col_ContType', N'vi-VN', N'Loại cont'),
(N'Scc_Col_ContType', N'zh-CN', N'箱型'),

(N'Scc_Col_Direction', N'en-US', N'Direction'),
(N'Scc_Col_Direction', N'vi-VN', N'Hướng'),
(N'Scc_Col_Direction', N'zh-CN', N'方向'),

(N'Scc_Col_FreeDays', N'en-US', N'Free days'),
(N'Scc_Col_FreeDays', N'vi-VN', N'Ngày miễn phí'),
(N'Scc_Col_FreeDays', N'zh-CN', N'免箱期天数'),

(N'Scc_Col_BillableDays', N'en-US', N'Billable days'),
(N'Scc_Col_BillableDays', N'vi-VN', N'Ngày tính phí'),
(N'Scc_Col_BillableDays', N'zh-CN', N'计费天数'),

(N'Scc_Col_Currency', N'en-US', N'Currency'),
(N'Scc_Col_Currency', N'vi-VN', N'Tiền tệ'),
(N'Scc_Col_Currency', N'zh-CN', N'币种'),

(N'Scc_Col_EmptyPickup', N'en-US', N'Empty pickup'),
(N'Scc_Col_EmptyPickup', N'vi-VN', N'Lấy rỗng'),
(N'Scc_Col_EmptyPickup', N'zh-CN', N'提空'),

(N'Scc_Col_Remarks', N'en-US', N'Remarks'),
(N'Scc_Col_Remarks', N'vi-VN', N'Ghi chú'),
(N'Scc_Col_Remarks', N'zh-CN', N'备注'),

(N'Scc_Lbl_HBL', N'en-US', N'HBL (number)'),
(N'Scc_Lbl_HBL', N'vi-VN', N'HBL (số)'),
(N'Scc_Lbl_HBL', N'zh-CN', N'HBL（单号）'),

(N'Scc_Lbl_Container', N'en-US', N'Container (number)'),
(N'Scc_Lbl_Container', N'vi-VN', N'Container (số cont)'),
(N'Scc_Lbl_Container', N'zh-CN', N'集装箱（箱号）'),

(N'Scc_Lbl_ChargeType', N'en-US', N'Charge type (name)'),
(N'Scc_Lbl_ChargeType', N'vi-VN', N'Loại phí (tên)'),
(N'Scc_Lbl_ChargeType', N'zh-CN', N'费用类型（名称）'),

(N'Scc_Lbl_Customer', N'en-US', N'Customer'),
(N'Scc_Lbl_Customer', N'vi-VN', N'Khách hàng'),
(N'Scc_Lbl_Customer', N'zh-CN', N'客户'),

(N'Scc_Lbl_ShippingLine', N'en-US', N'Shipping line'),
(N'Scc_Lbl_ShippingLine', N'vi-VN', N'Hãng tàu'),
(N'Scc_Lbl_ShippingLine', N'zh-CN', N'船公司'),

(N'Scc_Lbl_Depot', N'en-US', N'Depot / port'),
(N'Scc_Lbl_Depot', N'vi-VN', N'Cảng / depot'),
(N'Scc_Lbl_Depot', N'zh-CN', N'港口'),

(N'Scc_Lbl_Terminal', N'en-US', N'Terminal'),
(N'Scc_Lbl_Terminal', N'vi-VN', N'Terminal'),
(N'Scc_Lbl_Terminal', N'zh-CN', N'码头'),

(N'Scc_Lbl_ContainerType', N'en-US', N'Container type'),
(N'Scc_Lbl_ContainerType', N'vi-VN', N'Loại container'),
(N'Scc_Lbl_ContainerType', N'zh-CN', N'箱型'),

(N'Scc_Lbl_Direction', N'en-US', N'Direction'),
(N'Scc_Lbl_Direction', N'vi-VN', N'Hướng'),
(N'Scc_Lbl_Direction', N'zh-CN', N'方向'),

(N'Scc_Lbl_Currency', N'en-US', N'Currency'),
(N'Scc_Lbl_Currency', N'vi-VN', N'Tiền tệ'),
(N'Scc_Lbl_Currency', N'zh-CN', N'币种'),

(N'Scc_Lbl_FreeDays', N'en-US', N'Free days'),
(N'Scc_Lbl_FreeDays', N'vi-VN', N'Ngày miễn phí'),
(N'Scc_Lbl_FreeDays', N'zh-CN', N'免箱天数'),

(N'Scc_Lbl_BillableDays', N'en-US', N'Billable days'),
(N'Scc_Lbl_BillableDays', N'vi-VN', N'Ngày tính phí'),
(N'Scc_Lbl_BillableDays', N'zh-CN', N'计费天数'),

(N'Scc_Lbl_EmptyPickup', N'en-US', N'Empty pickup date'),
(N'Scc_Lbl_EmptyPickup', N'vi-VN', N'Ngày lấy rỗng'),
(N'Scc_Lbl_EmptyPickup', N'zh-CN', N'提空日期'),

(N'Scc_Lbl_FullDischarge', N'en-US', N'Full discharge date'),
(N'Scc_Lbl_FullDischarge', N'vi-VN', N'Ngày dỡ hàng'),
(N'Scc_Lbl_FullDischarge', N'zh-CN', N'卸货日期'),

(N'Scc_Lbl_FullDelivery', N'en-US', N'Full delivery date'),
(N'Scc_Lbl_FullDelivery', N'vi-VN', N'Ngày giao hàng'),
(N'Scc_Lbl_FullDelivery', N'zh-CN', N'交货日期'),

(N'Scc_Lbl_EmptyReturn', N'en-US', N'Empty return date'),
(N'Scc_Lbl_EmptyReturn', N'vi-VN', N'Ngày trả rỗng'),
(N'Scc_Lbl_EmptyReturn', N'zh-CN', N'还空日期'),

(N'Scc_Lbl_StorageIn', N'en-US', N'Storage in'),
(N'Scc_Lbl_StorageIn', N'vi-VN', N'Vào kho'),
(N'Scc_Lbl_StorageIn', N'zh-CN', N'入库'),

(N'Scc_Lbl_StorageOut', N'en-US', N'Storage out'),
(N'Scc_Lbl_StorageOut', N'vi-VN', N'Ra kho'),
(N'Scc_Lbl_StorageOut', N'zh-CN', N'出库'),

(N'Scc_Lbl_Remarks', N'en-US', N'Remarks'),
(N'Scc_Lbl_Remarks', N'vi-VN', N'Ghi chú'),
(N'Scc_Lbl_Remarks', N'zh-CN', N'备注'),

(N'Scc_Msg_SelectLinks', N'en-US', N'Please select HBL, container, charge type, customer, shipping line, depot and terminal.'),
(N'Scc_Msg_SelectLinks', N'vi-VN', N'Vui lòng chọn HBL, container, loại phí, khách, hãng tàu, cảng và terminal.'),
(N'Scc_Msg_SelectLinks', N'zh-CN', N'请选择提单、集装箱、费用类型、客户、船公司、港口和码头。'),

(N'Scc_Msg_ContainerTypeRequired', N'en-US', N'Container type is required.'),
(N'Scc_Msg_ContainerTypeRequired', N'vi-VN', N'Bắt buộc chọn loại container.'),
(N'Scc_Msg_ContainerTypeRequired', N'zh-CN', N'箱型为必填。');

COMMIT TRANSACTION;
PRINT N'ImportShipmentChargeContextLocalizationResources: done.';
