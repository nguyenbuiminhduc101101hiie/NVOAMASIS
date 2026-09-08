-- Optional menu labels for 8.3.9 Depot Hai Phong
BEGIN TRANSACTION;

DELETE FROM LocalizationResources WHERE ResourceKey IN (
    'DepotHaiPhong','DepotHaiPhong_8391','DepotHaiPhong_8392','DepotHaiPhong_8393','DepotHaiPhong_8394',
    'DepotHaiPhong_8395','DepotHaiPhong_8396','DepotHaiPhong_8397','DepotHaiPhong_8398','DepotHaiPhong_8399',
    'DepotHaiPhong_83910',
    'CiHp_PageTitle','CiHp_Title','CiHp_Subtitle','CiHp_ImportMenu'
);

INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
('DepotHaiPhong','en-US',N'Depot Hai Phong'),('DepotHaiPhong','vi-VN',N'Depot Hai Phong'),('DepotHaiPhong','zh-CN',N'海防堆场'),
('DepotHaiPhong_8391','en-US',N'Nam Dinh Vu Stock'),('DepotHaiPhong_8391','vi-VN',N'Nam Dinh Vu Stock'),('DepotHaiPhong_8391','zh-CN',N'南定 Vu 库存'),
('DepotHaiPhong_8392','en-US',N'Nam Dinh Vu Report'),('DepotHaiPhong_8392','vi-VN',N'Nam Dinh Vu Report'),('DepotHaiPhong_8392','zh-CN',N'南定 Vu 日报'),
('DepotHaiPhong_8393','en-US',N'ICD Hai Phong (TCHP)'),('DepotHaiPhong_8393','vi-VN',N'ICD Hai Phong (TCHP)'),('DepotHaiPhong_8393','zh-CN',N'海防 ICD TCHP'),
('DepotHaiPhong_8394','en-US',N'ICD Nam Hai Stock'),('DepotHaiPhong_8394','vi-VN',N'ICD Nam Hai Stock'),('DepotHaiPhong_8394','zh-CN',N'南海 ICD 库存'),
('DepotHaiPhong_8395','en-US',N'Sao A Depot'),('DepotHaiPhong_8395','vi-VN',N'Sao A Depot'),('DepotHaiPhong_8395','zh-CN',N'Sao A 堆场'),
('DepotHaiPhong_8396','en-US',N'Hai An Movement Report'),('DepotHaiPhong_8396','vi-VN',N'Hai An Movement Report'),('DepotHaiPhong_8396','zh-CN',N'海安动态'),
('DepotHaiPhong_8397','en-US',N'Hai An Stock Report'),('DepotHaiPhong_8397','vi-VN',N'Hai An Stock Report'),('DepotHaiPhong_8397','zh-CN',N'海安库存'),
('DepotHaiPhong_8398','en-US',N'ICD Nam Hai Movement'),('DepotHaiPhong_8398','vi-VN',N'ICD Nam Hai Movement'),('DepotHaiPhong_8398','zh-CN',N'南海 ICD 动态'),
('DepotHaiPhong_8399','en-US',N'GFT Depot'),('DepotHaiPhong_8399','vi-VN',N'GFT Depot'),('DepotHaiPhong_8399','zh-CN',N'GFT 堆场'),
('DepotHaiPhong_83910','en-US',N'HICT'),('DepotHaiPhong_83910','vi-VN',N'HICT'),('DepotHaiPhong_83910','zh-CN',N'HICT'),
('CiHp_PageTitle','en-US',N'8.3.9.11 Container Inventory Hai Phong'),('CiHp_PageTitle','vi-VN',N'8.3.9.11 Tồn kho container Hải Phòng'),('CiHp_PageTitle','zh-CN',N'8.3.9.11 海防集装箱库存'),
('CiHp_Title','en-US',N'8.3.9.11 Container Inventory Hai Phong'),('CiHp_Title','vi-VN',N'8.3.9.11 Tồn kho container Hải Phòng'),('CiHp_Title','zh-CN',N'8.3.9.11 海防集装箱库存'),
('CiHp_Subtitle','en-US',N'Statistics from the 10 Hai Phong depot import tables (8.3.9.1–10): Nam Dinh Vu, ICD Hai Phong TCHP, ICD Nam Hai, Sao A, Hai An, GFT and HICT.'),('CiHp_Subtitle','vi-VN',N'Thống kê từ 10 bảng import depot Hải Phòng (8.3.9.1–10): Nam Dinh Vu, ICD Hai Phong TCHP, ICD Nam Hai, Sao A, Hai An, GFT và HICT.'),('CiHp_Subtitle','zh-CN',N'汇总海防 10 张导入表（8.3.9.1–10）：南定 Vu、海防 ICD TCHP、南海 ICD、Sao A、海安、GFT 与 HICT。'),
('CiHp_ImportMenu','en-US',N'Import 8.3.9'),('CiHp_ImportMenu','vi-VN',N'Import 8.3.9'),('CiHp_ImportMenu','zh-CN',N'导入 8.3.9');

COMMIT TRANSACTION;
