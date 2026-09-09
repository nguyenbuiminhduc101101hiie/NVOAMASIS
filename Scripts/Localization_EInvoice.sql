SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.LocalizationResources', N'U') IS NULL
BEGIN
    THROW 50001, 'Table dbo.LocalizationResources does not exist.', 1;
END;
GO

DECLARE @Resources TABLE
(
    ResourceKey nvarchar(500) NOT NULL,
    Culture nvarchar(10) NOT NULL,
    Value nvarchar(max) NOT NULL,
    PRIMARY KEY (ResourceKey, Culture)
);

INSERT INTO @Resources (ResourceKey, Culture, Value)
VALUES
(N'E Invoice', N'en-US', N'E-Invoice'),
(N'E Invoice', N'vi-VN', N'Hóa đơn điện tử'),
(N'E Invoice', N'zh-CN', N'电子发票');

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
GO
