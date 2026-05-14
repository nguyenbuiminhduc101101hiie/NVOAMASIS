/*
  Bảng dbo.FinancialReportLines / dbo.FinancialReportAccountMappings ĐÃ TỒN TẠI — chỉ ALTER, không CREATE.

  Đổi CompanyId (ví dụ BIGINT) -> UNIQUEIDENTIFIER: dùng cột tạm CompanyId_New, UPDATE, DROP cột cũ, sp_rename.

  Sửa phần UPDATE (TODO) nếu có dữ liệu: map BIGINT cũ -> Guid (vd. bảng map nội bộ).
  Bảng rỗng: không cần UPDATE, script chạy xuyên suốt.

  Một batch: lỗi sẽ dừng cả script (XACT_ABORT).
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @fkMappingsToLines NVARCHAR(256);
DECLARE @dcLines NVARCHAR(256);
DECLARE @dcMaps NVARCHAR(256);

/* ---- 1) Gỡ FK Mappings -> Lines (theo ReportLineId) ---- */
SELECT @fkMappingsToLines = fk.name
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc
    ON fk.object_id = fkc.constraint_object_id
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U')
  AND fk.referenced_object_id = OBJECT_ID(N'dbo.FinancialReportLines', N'U')
  AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'ReportLineId';

IF @fkMappingsToLines IS NOT NULL
    EXEC(N'ALTER TABLE dbo.FinancialReportAccountMappings DROP CONSTRAINT ' + QUOTENAME(@fkMappingsToLines) + N';');

/* ---- 2) FinancialReportLines: CompanyId -> UNIQUEIDENTIFIER ---- */
IF OBJECT_ID(N'dbo.FinancialReportLines', N'U') IS NOT NULL
   AND EXISTS (
        SELECT 1
        FROM sys.columns AS c
        INNER JOIN sys.types AS t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.FinancialReportLines', N'U')
          AND c.name = N'CompanyId'
          AND t.name <> N'uniqueidentifier'
    )
BEGIN
    IF COL_LENGTH(N'dbo.FinancialReportLines', N'CompanyId_New') IS NULL
        ALTER TABLE dbo.FinancialReportLines ADD CompanyId_New UNIQUEIDENTIFIER NULL;

    /*
      TODO — có dòng dữ liệu thì bỏ comment và chỉnh map, ví dụ:
      UPDATE L
      SET L.CompanyId_New = M.CompanyGuid
      FROM dbo.FinancialReportLines AS L
      INNER JOIN dbo.YourLegacyCompanyMap AS M ON M.LegacyCompanyId = L.CompanyId;

      Dev / một công ty duy nhất (cẩn trọng):
      UPDATE L
      SET L.CompanyId_New = (SELECT TOP (1) CompanyID FROM dbo.CompanyInfomation ORDER BY CompanyID)
      FROM dbo.FinancialReportLines AS L
      WHERE L.CompanyId_New IS NULL;
    */
    IF EXISTS (SELECT 1 FROM dbo.FinancialReportLines AS L WHERE L.CompanyId_New IS NULL)
        THROW 50001, N'FinancialReportLines: điền CompanyId_New (UPDATE map từ CompanyId cũ). Xem TODO trong script.', 1;

    SELECT @dcLines = dc.name
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.FinancialReportLines', N'U')
      AND c.name = N'CompanyId';

    IF @dcLines IS NOT NULL
        EXEC(N'ALTER TABLE dbo.FinancialReportLines DROP CONSTRAINT ' + QUOTENAME(@dcLines) + N';');

    ALTER TABLE dbo.FinancialReportLines DROP COLUMN CompanyId;

    EXEC sys.sp_rename N'dbo.FinancialReportLines.CompanyId_New', N'CompanyId', N'COLUMN';

    ALTER TABLE dbo.FinancialReportLines ALTER COLUMN CompanyId UNIQUEIDENTIFIER NOT NULL;
END;

/* ---- 3) FinancialReportAccountMappings: CompanyId -> UNIQUEIDENTIFIER ---- */
IF OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U') IS NOT NULL
   AND EXISTS (
        SELECT 1
        FROM sys.columns AS c
        INNER JOIN sys.types AS t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U')
          AND c.name = N'CompanyId'
          AND t.name <> N'uniqueidentifier'
    )
BEGIN
    IF COL_LENGTH(N'dbo.FinancialReportAccountMappings', N'CompanyId_New') IS NULL
        ALTER TABLE dbo.FinancialReportAccountMappings ADD CompanyId_New UNIQUEIDENTIFIER NULL;

    /*
      TODO — map từ dòng báo cáo (sau bước 2, Lines.CompanyId đã là Guid):
      UPDATE M
      SET M.CompanyId_New = L.CompanyId
      FROM dbo.FinancialReportAccountMappings AS M
      INNER JOIN dbo.FinancialReportLines AS L ON L.Id = M.ReportLineId
      WHERE M.CompanyId_New IS NULL;
    */
    IF EXISTS (SELECT 1 FROM dbo.FinancialReportAccountMappings AS M WHERE M.CompanyId_New IS NULL)
        THROW 50002, N'FinancialReportAccountMappings: điền CompanyId_New (UPDATE — xem TODO trong script).', 1;

    SELECT @dcMaps = dc.name
    FROM sys.default_constraints AS dc
    INNER JOIN sys.columns AS c
        ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U')
      AND c.name = N'CompanyId';

    IF @dcMaps IS NOT NULL
        EXEC(N'ALTER TABLE dbo.FinancialReportAccountMappings DROP CONSTRAINT ' + QUOTENAME(@dcMaps) + N';');

    ALTER TABLE dbo.FinancialReportAccountMappings DROP COLUMN CompanyId;

    EXEC sys.sp_rename N'dbo.FinancialReportAccountMappings.CompanyId_New', N'CompanyId', N'COLUMN';

    ALTER TABLE dbo.FinancialReportAccountMappings ALTER COLUMN CompanyId UNIQUEIDENTIFIER NOT NULL;
END;

/* ---- 4) Tạo lại FK ReportLineId ---- */
IF OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.FinancialReportLines', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys AS fk
        INNER JOIN sys.foreign_key_columns AS fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID(N'dbo.FinancialReportAccountMappings', N'U')
          AND fk.referenced_object_id = OBJECT_ID(N'dbo.FinancialReportLines', N'U')
          AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'ReportLineId'
    )
BEGIN
    ALTER TABLE dbo.FinancialReportAccountMappings
        ADD CONSTRAINT FK_FinancialReportAccountMappings_ReportLine
        FOREIGN KEY (ReportLineId) REFERENCES dbo.FinancialReportLines (Id);
END;
