-- B09-DN (TT99/2025) reporting/disclosure tables.
-- Read-only on GeneralLedgerEntries. Safe to run on DEV/UAT before Generate.
-- If EF __EFMigrationsHistory is used, also insert the migration row after this script.

IF OBJECT_ID(N'dbo.FinancialStatementNoteTemplates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteTemplates
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatementNoteTemplates PRIMARY KEY,
        Code NVARCHAR(30) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        AccountingRegime NVARCHAR(30) NOT NULL,
        Version NVARCHAR(20) NOT NULL,
        EffectiveFrom DATETIME2 NOT NULL,
        EffectiveTo DATETIME2 NULL,
        IsActive BIT NOT NULL
    );
    CREATE UNIQUE INDEX IX_FinancialStatementNoteTemplates_Code_Version
        ON dbo.FinancialStatementNoteTemplates (Code, Version);
END
GO

IF OBJECT_ID(N'dbo.FinancialStatementNoteSections', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteSections
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatementNoteSections PRIMARY KEY,
        TemplateId INT NOT NULL,
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(500) NOT NULL,
        DisplayOrder INT NOT NULL,
        CONSTRAINT FK_FinancialStatementNoteSections_FinancialStatementNoteTemplates_TemplateId
            FOREIGN KEY (TemplateId) REFERENCES dbo.FinancialStatementNoteTemplates (Id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX IX_FinancialStatementNoteSections_TemplateId_Code
        ON dbo.FinancialStatementNoteSections (TemplateId, Code);
END
GO

IF OBJECT_ID(N'dbo.FinancialStatementNoteLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteLines
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatementNoteLines PRIMARY KEY,
        SectionId INT NOT NULL,
        ParentLineId INT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(1000) NOT NULL,
        ValueType INT NOT NULL,
        SourceType INT NOT NULL,
        DisplayOrder INT NOT NULL,
        IsRequiredNarrative BIT NOT NULL,
        IsVisible BIT NOT NULL,
        CONSTRAINT FK_FinancialStatementNoteLines_FinancialStatementNoteSections_SectionId
            FOREIGN KEY (SectionId) REFERENCES dbo.FinancialStatementNoteSections (Id) ON DELETE CASCADE,
        CONSTRAINT FK_FinancialStatementNoteLines_FinancialStatementNoteLines_ParentLineId
            FOREIGN KEY (ParentLineId) REFERENCES dbo.FinancialStatementNoteLines (Id)
    );
    CREATE UNIQUE INDEX IX_FinancialStatementNoteLines_SectionId_Code
        ON dbo.FinancialStatementNoteLines (SectionId, Code);
    CREATE INDEX IX_FinancialStatementNoteLines_ParentLineId
        ON dbo.FinancialStatementNoteLines (ParentLineId);
END
GO

IF OBJECT_ID(N'dbo.FinancialStatementNoteMappings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteMappings
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatementNoteMappings PRIMARY KEY,
        NoteLineId INT NOT NULL,
        Mode INT NOT NULL,
        AccountPrefixesCsv NVARCHAR(1000) NULL,
        SignMultiplier DECIMAL(18,6) NOT NULL,
        DetailThresholdPercent DECIMAL(9,4) NULL,
        CustomSql NVARCHAR(MAX) NULL,
        IsEnabled BIT NOT NULL,
        CONSTRAINT FK_FinancialStatementNoteMappings_FinancialStatementNoteLines_NoteLineId
            FOREIGN KEY (NoteLineId) REFERENCES dbo.FinancialStatementNoteLines (Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_FinancialStatementNoteMappings_NoteLineId
        ON dbo.FinancialStatementNoteMappings (NoteLineId);
END
GO

IF OBJECT_ID(N'dbo.FinancialStatementNoteReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteReports
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FinancialStatementNoteReports PRIMARY KEY,
        CompanyId UNIQUEIDENTIFIER NOT NULL,
        TemplateId INT NOT NULL,
        FiscalYear INT NOT NULL,
        Status INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        CreatedBy NVARCHAR(200) NULL,
        GeneratedAt DATETIME2 NULL,
        ValidatedAt DATETIME2 NULL,
        LockedAt DATETIME2 NULL,
        LockedBy NVARCHAR(200) NULL,
        RowVersion ROWVERSION NULL,
        CONSTRAINT FK_FinancialStatementNoteReports_FinancialStatementNoteTemplates_TemplateId
            FOREIGN KEY (TemplateId) REFERENCES dbo.FinancialStatementNoteTemplates (Id)
    );
    CREATE UNIQUE INDEX IX_FinancialStatementNoteReports_CompanyId_TemplateId_FiscalYear
        ON dbo.FinancialStatementNoteReports (CompanyId, TemplateId, FiscalYear);
    CREATE INDEX IX_FinancialStatementNoteReports_TemplateId
        ON dbo.FinancialStatementNoteReports (TemplateId);
END
GO

IF OBJECT_ID(N'dbo.FinancialStatementNoteValues', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.FinancialStatementNoteValues
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_FinancialStatementNoteValues PRIMARY KEY,
        ReportId UNIQUEIDENTIFIER NOT NULL,
        NoteLineId INT NOT NULL,
        CurrentSystemValue DECIMAL(28,4) NULL,
        CurrentAdjustment DECIMAL(28,4) NOT NULL,
        PreviousSystemValue DECIMAL(28,4) NULL,
        PreviousAdjustment DECIMAL(28,4) NOT NULL,
        Narrative NVARCHAR(MAX) NULL,
        AdjustmentReason NVARCHAR(MAX) NULL,
        UpdatedBy NVARCHAR(200) NULL,
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_FinancialStatementNoteValues_FinancialStatementNoteReports_ReportId
            FOREIGN KEY (ReportId) REFERENCES dbo.FinancialStatementNoteReports (Id) ON DELETE CASCADE,
        CONSTRAINT FK_FinancialStatementNoteValues_FinancialStatementNoteLines_NoteLineId
            FOREIGN KEY (NoteLineId) REFERENCES dbo.FinancialStatementNoteLines (Id)
    );
    CREATE UNIQUE INDEX IX_FinancialStatementNoteValues_ReportId_NoteLineId
        ON dbo.FinancialStatementNoteValues (ReportId, NoteLineId);
    CREATE INDEX IX_FinancialStatementNoteValues_NoteLineId
        ON dbo.FinancialStatementNoteValues (NoteLineId);
END
GO

IF OBJECT_ID(N'dbo.RelatedPartyProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RelatedPartyProfiles
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RelatedPartyProfiles PRIMARY KEY,
        CompanyId UNIQUEIDENTIFIER NOT NULL,
        ClientId INT NULL,
        RelatedPartyType NVARCHAR(100) NOT NULL,
        RelationshipDescription NVARCHAR(1000) NOT NULL,
        EffectiveFrom DATETIME2 NOT NULL,
        EffectiveTo DATETIME2 NULL,
        IsActive BIT NOT NULL
    );
    CREATE INDEX IX_RelatedPartyProfiles_CompanyId_ClientId_EffectiveFrom
        ON dbo.RelatedPartyProfiles (CompanyId, ClientId, EffectiveFrom);
END
GO

IF OBJECT_ID(N'dbo.CustodyAssets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CustodyAssets
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustodyAssets PRIMARY KEY,
        CompanyId UNIQUEIDENTIFIER NOT NULL,
        ClientId INT NULL,
        ShipmentId BIGINT NULL,
        ContainerId BIGINT NULL,
        WarehouseId INT NULL,
        CustodyType NVARCHAR(50) NOT NULL,
        CommodityGroup NVARCHAR(300) NULL,
        Description NVARCHAR(1000) NULL,
        Specification NVARCHAR(1000) NULL,
        Quantity DECIMAL(28,4) NULL,
        Unit NVARCHAR(50) NULL,
        Condition NVARCHAR(200) NULL,
        EstimatedValue DECIMAL(28,4) NULL,
        Currency NVARCHAR(10) NULL,
        ReceivedDate DATETIME2 NULL,
        ReleasedDate DATETIME2 NULL,
        RightsAndObligations NVARCHAR(MAX) NULL,
        StorageResponsibility NVARCHAR(MAX) NULL,
        SignificantRisk NVARCHAR(MAX) NULL,
        DisclosureNote NVARCHAR(MAX) NULL
    );
    CREATE INDEX IX_CustodyAssets_CompanyId_ClientId
        ON dbo.CustodyAssets (CompanyId, ClientId);
    CREATE INDEX IX_CustodyAssets_CompanyId_ContainerId
        ON dbo.CustodyAssets (CompanyId, ContainerId);
END
GO

IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260819120000_AddB09FinancialStatementNotes')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260819120000_AddB09FinancialStatementNotes', N'8.0.0');
END
GO
