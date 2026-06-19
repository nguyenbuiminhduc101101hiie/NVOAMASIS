IF OBJECT_ID(N'dbo.BillSeaLayoutForm', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BillSeaLayoutForm
    (
        BillSeaLayoutFormId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_BillSeaLayoutForm PRIMARY KEY,
        FormName            NVARCHAR(200)    NOT NULL,
        MrtContent          VARBINARY(MAX)   NOT NULL,
        AttachMrtContent    VARBINARY(MAX)   NULL,
        SourceTemplate      NVARCHAR(260)    NOT NULL CONSTRAINT DF_BillSeaLayoutForm_SourceTemplate DEFAULT (N'BillSea_NVOCC.mrt'),
        CreatedAt           DATETIME2        NOT NULL CONSTRAINT DF_BillSeaLayoutForm_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt           DATETIME2        NOT NULL CONSTRAINT DF_BillSeaLayoutForm_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy           NVARCHAR(100)    NULL,
        IsActive            BIT              NOT NULL CONSTRAINT DF_BillSeaLayoutForm_IsActive DEFAULT (1)
    );

    CREATE INDEX IX_BillSeaLayoutForm_FormName ON dbo.BillSeaLayoutForm (FormName);
END
GO
