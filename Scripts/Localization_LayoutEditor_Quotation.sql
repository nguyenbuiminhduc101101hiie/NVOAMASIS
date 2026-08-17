-- Incremental: add Quotation help text for layout editor (1.17)
-- Run then clear cache: DbStringLocalizerFactory.ClearCache() or restart app

BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey = 'LayoutEditor_Help_Quotation';

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('LayoutEditor_Help_Quotation','en-US',N'<b>Quotation</b> from <b>Quotation.mrt</b> (no Sea/Air split). <b>Image1</b> uses <b>CompanyInfo.Logo</b>; connection string follows the active DB when exporting.'),
('LayoutEditor_Help_Quotation','vi-VN',N'<b>Quotation</b> từ <b>Quotation.mrt</b> (không tách Sea/Air). <b>Image1</b> lấy logo từ <b>CompanyInfo.Logo</b>; connection string theo DB đang kết nối khi xuất.'),
('LayoutEditor_Help_Quotation','zh-CN',N'<b>Quotation</b> 来自 <b>Quotation.mrt</b>（不分 Sea/Air）。<b>Image1</b> 使用 <b>CompanyInfo.Logo</b>；导出时连接字符串跟随当前数据库。');

COMMIT;
