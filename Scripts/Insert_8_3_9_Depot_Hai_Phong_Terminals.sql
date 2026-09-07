-- 8.3.9 Depot Hai Phong — insert missing Terminal rows
-- Lookup import dung TermiNalName (khong phan biet hoa thuong, dung chuoi).
-- Chay tren DB: nvoamasis / nvopasl / nvovssa / NVOCC (script idempotent).
--
-- 10 menu / 7 ten depot:
--   8.3.9.1 + 8.3.9.2  NAM DINH VU          (da co)
--   8.3.9.3            ICD HAI PHONG TCHP   (thieu)
--   8.3.9.4 + 8.3.9.8  ICD NAM HAI          (thieu)
--   8.3.9.5            SAO A DEPOT          (thieu; da co "SAO A" khac ten)
--   8.3.9.6 + 8.3.9.7  HAI AN               (thieu)
--   8.3.9.9            GFT DEPOT            (thieu)
--   8.3.9.10           HICT                 (thieu; da co HITC / Lach Huyen)
--
-- Ten gan giong da co (KHONG doi, KHONG map tu dong):
--   ICD TAN CANG HP          Code 03CES10
--   NAM HAI ICD              Code 03TGS11
--   SAO A                    Code SAOA
--   CANG HAI AN (VNHIA)      Code 03CES01
--   LACH HUYEN HAI PHONG (HITC)  Code VNCLH - 03EES06
--   CANG NAM DINH VU         Code 03CES11  (khac "NAM DINH VU")

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Now DATETIME2 = SYSDATETIME();
DECLARE @User NVARCHAR(100) = N'system-8.3.9';

-- 8.3.9.3 ICD HAI PHONG TCHP
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'ICD HAI PHONG TCHP')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('D778C31C-368C-471D-9418-2E8792DB94E1', N'HP-TCHP', N'ICD HAI PHONG TCHP', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: ICD HAI PHONG TCHP';
END
ELSE
    PRINT 'Exists: ICD HAI PHONG TCHP';

-- 8.3.9.4 / 8.3.9.8 ICD NAM HAI
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'ICD NAM HAI')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('3D214219-9BC8-4698-9269-91FA663B9E32', N'HP-ICDNH', N'ICD NAM HAI', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: ICD NAM HAI';
END
ELSE
    PRINT 'Exists: ICD NAM HAI';

-- 8.3.9.5 SAO A DEPOT
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'SAO A DEPOT')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('C9BBD45A-9A36-44AD-8978-25ED881330EF', N'HP-SAOAD', N'SAO A DEPOT', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: SAO A DEPOT';
END
ELSE
    PRINT 'Exists: SAO A DEPOT';

-- 8.3.9.6 / 8.3.9.7 HAI AN
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'HAI AN')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('81C4161C-4E84-4D53-84B9-4FBF63B8D9DE', N'HP-HAIAN', N'HAI AN', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: HAI AN';
END
ELSE
    PRINT 'Exists: HAI AN';

-- 8.3.9.9 GFT DEPOT
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'GFT DEPOT')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('38B5C8F3-6957-47EB-A68C-F2DF142268A2', N'HP-GFT', N'GFT DEPOT', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: GFT DEPOT';
END
ELSE
    PRINT 'Exists: GFT DEPOT';

-- 8.3.9.10 HICT
IF NOT EXISTS (
    SELECT 1 FROM dbo.Terminal
    WHERE LOWER(LTRIM(RTRIM(TermiNalName))) = LOWER(N'HICT')
)
BEGIN
    INSERT INTO dbo.Terminal
        (TerminalID, Code, TermiNalName, Address, tel, Capacity, FreeStorage, ValidOrder, OrderReport,
         Continued, Editable, Approve, UserID, UpdateTime, code_nvocc, Codeha)
    VALUES
        ('26DF2CD4-429E-4F19-BFE2-A4D56BCCF836', N'HP-HICT', N'HICT', NULL, NULL, NULL, NULL, NULL, NULL,
         1, 1, 0, @User, @Now, NULL, NULL);
    PRINT 'Inserted: HICT';
END
ELSE
    PRINT 'Exists: HICT';

-- Kiem tra 7 ten depot sau khi chay
SELECT
    v.NeededName,
    t.TerminalID,
    t.Code,
    t.TermiNalName,
    CASE WHEN t.TerminalID IS NULL THEN N'MISSING' ELSE N'OK' END AS Status
FROM (VALUES
    (N'NAM DINH VU'),
    (N'ICD HAI PHONG TCHP'),
    (N'ICD NAM HAI'),
    (N'SAO A DEPOT'),
    (N'HAI AN'),
    (N'GFT DEPOT'),
    (N'HICT')
) v(NeededName)
LEFT JOIN dbo.Terminal t
    ON LOWER(LTRIM(RTRIM(t.TermiNalName))) = LOWER(v.NeededName)
ORDER BY v.NeededName;
