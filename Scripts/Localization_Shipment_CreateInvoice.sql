/*
  Add localization resources for Shipment Create Invoice dialog
  Components:
    - Components/Shipment/Pages/CreateInvoice.razor
      (Invoice No. column, Hide/Show invoiced toggle)
    - Components/Shipment/Pages/DebitTab.razor, Payment/DebitTab_5_0.razor
      (Copy to Credit button)
  Cultures: vi-VN, en-US, zh-CN
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'Invoice No.' AS ResourceKey, N'vi-VN' AS Culture, N'Số hóa đơn' AS Value UNION ALL
    SELECT N'Invoice No.', N'en-US', N'Invoice No.' UNION ALL
    SELECT N'Invoice No.', N'zh-CN', N'发票号码' UNION ALL

    SELECT N'Hide invoiced', N'vi-VN', N'Ẩn phí đã xuất hóa đơn' UNION ALL
    SELECT N'Hide invoiced', N'en-US', N'Hide invoiced' UNION ALL
    SELECT N'Hide invoiced', N'zh-CN', N'隐藏已开票' UNION ALL

    SELECT N'Show invoiced', N'vi-VN', N'Hiện phí đã xuất hóa đơn' UNION ALL
    SELECT N'Show invoiced', N'en-US', N'Show invoiced' UNION ALL
    SELECT N'Show invoiced', N'zh-CN', N'显示已开票' UNION ALL

    SELECT N'Copy to Credit', N'vi-VN', N'Copy sang Credit' UNION ALL
    SELECT N'Copy to Credit', N'en-US', N'Copy to Credit' UNION ALL
    SELECT N'Copy to Credit', N'zh-CN', N'复制到应付'
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
PRINT N'Localization_Shipment_CreateInvoice: done.';
