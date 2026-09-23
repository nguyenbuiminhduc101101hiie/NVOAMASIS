SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LocalizationResources', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.LocalizationResources does not exist.', 1;
END;
GO

-- Payment Report (5.7) columns were using the shared keys "hoadondaura" /
-- "congnohoadondaura" ("Output Invoice"), but the values they display come
-- from HoaDonDauVao (input invoice), not HoaDonDauRa (output invoice).
-- Those two shared keys are also used correctly (for real output-invoice
-- data) on CongNoToiHanCus/Profit_HBL.razor and CongNoToiHanCus/Index_5_4.razor,
-- so they must NOT be overwritten. New keys are added instead, and
-- Payment_Report/Index.razor is switched to reference them.

DECLARE @Resources TABLE
(
    ResourceKey nvarchar(500) NOT NULL,
    Culture nvarchar(10) NOT NULL,
    Value nvarchar(max) NOT NULL,
    PRIMARY KEY (ResourceKey, Culture)
);

INSERT INTO @Resources (ResourceKey, Culture, Value)
VALUES
(N'hoadondauvao', N'en-US', N'Input Invoice'),
(N'hoadondauvao', N'vi-VN', N'Hóa Đơn Đầu Vào'),
(N'hoadondauvao', N'zh-CN', N'进项发票'),

(N'congnohoadondauvao', N'en-US', N'Input Invoice Debt'),
(N'congnohoadondauvao', N'vi-VN', N'Công Nợ Hóa Đơn Đầu Vào'),
(N'congnohoadondauvao', N'zh-CN', N'进项发票欠款');

UPDATE target
SET target.Value = source.Value
FROM dbo.LocalizationResources AS target
INNER JOIN @Resources AS source
    ON source.ResourceKey = target.ResourceKey
   AND source.Culture = target.Culture;

INSERT INTO dbo.LocalizationResources (ResourceKey, Culture, Value)
SELECT source.ResourceKey, source.Culture, source.Value
FROM @Resources AS source
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.LocalizationResources AS target
    WHERE target.ResourceKey = source.ResourceKey
      AND target.Culture = source.Culture
);

SELECT ResourceKey, Culture, Value
FROM dbo.LocalizationResources
WHERE ResourceKey IN (N'hoadondauvao', N'congnohoadondauvao')
ORDER BY ResourceKey, Culture;
GO
