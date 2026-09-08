-- Incremental: add Lệnh Cấp Cont Rỗng help text for layout editor (1.17)
-- Run then clear cache: DbStringLocalizerFactory.ClearCache() or restart app

BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey = 'LayoutEditor_Help_LenhCapContRong';

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('LayoutEditor_Help_LenhCapContRong','en-US',N'<b>Lệnh Cấp Cont Rỗng</b> from <b>LenhCapContRong.mrt</b> (no Sea/Air split). <b>Image1</b> uses <b>CompanyInfo.Logo</b>; connection string follows the active DB when exporting.'),
('LayoutEditor_Help_LenhCapContRong','vi-VN',N'<b>Lệnh Cấp Cont Rỗng</b> từ <b>LenhCapContRong.mrt</b> (không tách Sea/Air). <b>Image1</b> lấy logo từ <b>CompanyInfo.Logo</b>; connection string theo DB đang kết nối khi xuất.'),
('LayoutEditor_Help_LenhCapContRong','zh-CN',N'<b>Lệnh Cấp Cont Rỗng</b> 来自 <b>LenhCapContRong.mrt</b>（不分 Sea/Air）。<b>Image1</b> 使用 <b>CompanyInfo.Logo</b>；导出时连接字符串跟随当前数据库。');

COMMIT;
