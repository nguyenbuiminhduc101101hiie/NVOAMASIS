-- Menu 9: Inventory (9.0)
-- Depends on: Group C, Groups 2–8
-- Reuses: NoPermission_View (Group 2), PKGs/GrossWT/Print (Group 2), CBM (Group 6),
--   ContainerDetail_StatusCont/ExportError (Group 7), Container/Refresh/Search/Ci_Status*/Ci_SectionInDepot/EqGate_GateOut (Group 8)
BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey IN (
    'Inventory',
    'Inv_Subtitle','Inv_Excel','Inv_Csv','Inv_Pdf',
    'Inv_OwnerName','Inv_ContainerCount','Inv_NetWt','Inv_Decommision',
    'Inv_InPort','Inv_GateOutCol',
    'Inv_LoadFailed','Inv_ExportSuccess','Inv_PdfPrintFailed',
    'Inv_ExportDepotTitle','Inv_ExportPortTitle','Inv_ExportGateOutTitle'
);

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('Inventory','en-US',N'Container Inventory'),('Inventory','vi-VN',N'Tồn kho container'),('Inventory','zh-CN',N'集装箱库存'),
('Inv_Subtitle','en-US',N'Depot · port arrival · utilization'),('Inv_Subtitle','vi-VN',N'Depot · đến cảng · tỷ lệ sử dụng'),('Inv_Subtitle','zh-CN',N'堆场 · 到港 · 利用率'),
('Inv_Excel','en-US',N'Excel'),('Inv_Excel','vi-VN',N'Excel'),('Inv_Excel','zh-CN',N'Excel'),
('Inv_Csv','en-US',N'CSV'),('Inv_Csv','vi-VN',N'CSV'),('Inv_Csv','zh-CN',N'CSV'),
('Inv_Pdf','en-US',N'PDF'),('Inv_Pdf','vi-VN',N'PDF'),('Inv_Pdf','zh-CN',N'PDF'),
('Inv_OwnerName','en-US',N'Owner Name'),('Inv_OwnerName','vi-VN',N'Chủ sở hữu'),('Inv_OwnerName','zh-CN',N'箱主'),
('Inv_ContainerCount','en-US',N'Container Count'),('Inv_ContainerCount','vi-VN',N'Số lượng container'),('Inv_ContainerCount','zh-CN',N'集装箱数量'),
('Inv_NetWt','en-US',N'Net WT'),('Inv_NetWt','vi-VN',N'Net WT'),('Inv_NetWt','zh-CN',N'净重'),
('Inv_Decommision','en-US',N'Decommission'),('Inv_Decommision','vi-VN',N'Ngưng sử dụng'),('Inv_Decommision','zh-CN',N'停用'),
('Inv_InPort','en-US',N'IN PORT'),('Inv_InPort','vi-VN',N'TẠI CẢNG'),('Inv_InPort','zh-CN',N'在港'),
('Inv_GateOutCol','en-US',N'GATE OUT'),('Inv_GateOutCol','vi-VN',N'GATE OUT'),('Inv_GateOutCol','zh-CN',N'出闸'),
('Inv_LoadFailed','en-US',N'Cannot load container inventory: {0}'),('Inv_LoadFailed','vi-VN',N'Không tải được tồn kho container: {0}'),('Inv_LoadFailed','zh-CN',N'无法加载集装箱库存：{0}'),
('Inv_ExportSuccess','en-US',N'Excel exported.'),('Inv_ExportSuccess','vi-VN',N'Đã xuất Excel.'),('Inv_ExportSuccess','zh-CN',N'Excel 已导出。'),
('Inv_PdfPrintFailed','en-US',N'PDF print failed: {0}'),('Inv_PdfPrintFailed','vi-VN',N'In PDF thất bại: {0}'),('Inv_PdfPrintFailed','zh-CN',N'PDF 打印失败：{0}'),
('Inv_ExportDepotTitle','en-US',N'IN DEPOT Containers'),('Inv_ExportDepotTitle','vi-VN',N'Container IN DEPOT'),('Inv_ExportDepotTitle','zh-CN',N'在堆场集装箱'),
('Inv_ExportPortTitle','en-US',N'IN PORT Containers'),('Inv_ExportPortTitle','vi-VN',N'Container IN PORT'),('Inv_ExportPortTitle','zh-CN',N'在港集装箱'),
('Inv_ExportGateOutTitle','en-US',N'GATE OUT Containers'),('Inv_ExportGateOutTitle','vi-VN',N'Container GATE OUT'),('Inv_ExportGateOutTitle','zh-CN',N'出闸集装箱');

COMMIT TRANSACTION;
-- After run: DbStringLocalizerFactory.ClearCache() or restart app.
