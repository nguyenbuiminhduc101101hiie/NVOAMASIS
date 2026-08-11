/*
  Top nav extras: View Document, Brochure, Help group
  Safe to re-run.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

;WITH src AS (
    SELECT N'ViewDocument' AS ResourceKey, N'en-US' AS Culture, N'View Document' AS Value UNION ALL
    SELECT N'ViewDocument', N'vi-VN', N'Xem tài liệu' UNION ALL
    SELECT N'ViewDocument', N'zh-CN', N'查看文档' UNION ALL

    SELECT N'Brochure', N'en-US', N'Brochure' UNION ALL
    SELECT N'Brochure', N'vi-VN', N'Brochure' UNION ALL
    SELECT N'Brochure', N'zh-CN', N'宣传册' UNION ALL

    SELECT N'Help_nav', N'en-US', N'Help' UNION ALL
    SELECT N'Help_nav', N'vi-VN', N'Trợ giúp' UNION ALL
    SELECT N'Help_nav', N'zh-CN', N'帮助' UNION ALL

    SELECT N'Guidelines', N'en-US', N'Guidelines' UNION ALL
    SELECT N'Guidelines', N'vi-VN', N'Hướng dẫn' UNION ALL
    SELECT N'Guidelines', N'zh-CN', N'指南' UNION ALL

    SELECT N'UserManual', N'en-US', N'User Manual' UNION ALL
    SELECT N'UserManual', N'vi-VN', N'Hướng dẫn sử dụng' UNION ALL
    SELECT N'UserManual', N'zh-CN', N'用户手册' UNION ALL

    SELECT N'Help', N'en-US', N'Help' UNION ALL
    SELECT N'Help', N'vi-VN', N'Trợ giúp' UNION ALL
    SELECT N'Help', N'zh-CN', N'帮助'
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

PRINT N'ViewDocument / Brochure / Help nav localization imported.';
