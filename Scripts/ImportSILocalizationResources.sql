/*
  Import localization for SI (Shipment Instruction) into LocalizationResources.
  Target: SQL Server, table LocalizationResources (ResourceKey, Culture, Value).
  Safe to re-run: removes ResourceKey N'SI' first, then inserts.

  Run in SSMS / Azure Data Studio / sqlcmd against your application database.
  After import, clear localization cache or restart the app (DbStringLocalizerFactory cache).
*/

SET NOCOUNT ON;

BEGIN TRANSACTION;

DELETE FROM dbo.LocalizationResources
WHERE ResourceKey = N'SI';

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value) VALUES
(N'SI', N'en-US', N'Shipment Instruction'),
(N'SI', N'vi-VN', N'Chỉ thị giao hàng'),
(N'SI', N'zh-CN', N'装运指示');

COMMIT TRANSACTION;
