/*
  Feature: 4.7 Pricing Ocean Freight Rate (Bieu gia cuoc bien - EX HO CHI MINH)
  Source file: "PRICING- EX HO CHI MINH-GOOGLE SHEET.xlsx" (7 sheets / trade lanes)

  REVISION 2: consolidated from 7 separate per-sheet tables into a single table
  with a TradeLane discriminator column (same column layout, all sheets share it).
  If you already ran revision 1 of this script, this drops those 7 tables first.

  Column layout mirrors the sheet's fixed positional columns (A..AG):
    A: extra/stray note column (messy source data, kept for traceability),
    EFF, VALID, CARRIERS, COUNTRY, POL, POD,
    O/F RATE + COM (20/40/40HC), Commission/HDL (20/40/40HC), REMARK, FREE TIME AT POD,
    Basic O/F NET (20/40/40HC),
    LSS/OBS/NBF/BAF/ETS/FAF/PCS (20/40/40HC),
    ISO/transport/Premium cargo/WRP/PIS/AIS/PSS (20/40/40HC),
    ISPS/ETS/ECA/PTS/CRS/Water/GSF/WIN (20/40/40HC),
    TOTAL O/F (20/40/40HC), VAT COM (20/40/40HC)
  For the two "RF" lanes the first three blocks represent 20RF/40RF/40RH instead of
  20DC/40DC/40HC; column names are kept identical for every lane since the sheets
  themselves label the surcharge/total/vat blocks as 20DC/40DC/40HC even for reefer lanes.

  Text columns coming from the spreadsheet (ExtraNote/Carrier/Country/Pol/Pod/Remark/
  FreeTimeAtPod) are NVARCHAR(MAX): the source sheet has rows with unpredictably long
  text (messy copy-paste), so a fixed size caused "String or binary data would be
  truncated" errors during import. NVARCHAR(MAX) columns cannot be index keys, so only
  TradeLane and SourceFileName are indexed.

  Safe to re-run (drops and recreates).
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Drop the old per-sheet tables from revision 1, if present.
IF OBJECT_ID(N'dbo.PricingRate_Intrasia', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_Intrasia;
IF OBJECT_ID(N'dbo.PricingRate_IntrasiaRF', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_IntrasiaRF;
IF OBJECT_ID(N'dbo.PricingRate_MiddleEastRedSeaIndia', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_MiddleEastRedSeaIndia;
IF OBJECT_ID(N'dbo.PricingRate_LatinWestEastCoastAus', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_LatinWestEastCoastAus;
IF OBJECT_ID(N'dbo.PricingRate_MiddleEastRedSeaIndiaRF', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_MiddleEastRedSeaIndiaRF;
IF OBJECT_ID(N'dbo.PricingRate_EuUsaAfricaNamMy', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_EuUsaAfricaNamMy;
IF OBJECT_ID(N'dbo.PricingRate_EuUsaAfricaNamMyRF', N'U') IS NOT NULL DROP TABLE dbo.PricingRate_EuUsaAfricaNamMyRF;
GO

IF OBJECT_ID(N'dbo.PricingRate', N'U') IS NOT NULL DROP TABLE dbo.PricingRate;
GO

CREATE TABLE dbo.PricingRate (
    [Id]                        UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    -- One of: INTRASIA, INTRASIA_RF, MIDEAST_REDSEA_INDIA, LATIN_WEST_EAST_AUS,
    --         MIDEAST_REDSEA_INDIA_RF, EU_USA_AFRICA_NAMMY, EU_USA_AFRICA_NAMMY_RF
    [TradeLane]                 NVARCHAR(50)    NOT NULL,
    -- Name of the .xlsx file this row came from (NULL for rows added manually via the CRUD form).
    [SourceFileName]            NVARCHAR(255)   NULL,
    [ExtraNote]                 NVARCHAR(MAX)   NULL,
    [EffDate]                   DATETIME2       NULL,
    [ValidDate]                 DATETIME2       NULL,
    [Carrier]                   NVARCHAR(MAX)   NULL,
    [Country]                   NVARCHAR(MAX)   NULL,
    [Pol]                       NVARCHAR(MAX)   NULL,
    [Pod]                       NVARCHAR(MAX)   NULL,
    [OfRateCom20]               DECIMAL(18,4)   NULL,
    [OfRateCom40]               DECIMAL(18,4)   NULL,
    [OfRateCom40Hc]             DECIMAL(18,4)   NULL,
    [CommissionHdl20]           DECIMAL(18,4)   NULL,
    [CommissionHdl40]           DECIMAL(18,4)   NULL,
    [CommissionHdl40Hc]         DECIMAL(18,4)   NULL,
    [Remark]                    NVARCHAR(MAX)   NULL,
    [FreeTimeAtPod]             NVARCHAR(MAX)   NULL,
    [BasicOfNet20]              DECIMAL(18,4)   NULL,
    [BasicOfNet40]              DECIMAL(18,4)   NULL,
    [BasicOfNet40Hc]            DECIMAL(18,4)   NULL,
    [SurchargeFuelEnv20]        DECIMAL(18,4)   NULL,
    [SurchargeFuelEnv40]        DECIMAL(18,4)   NULL,
    [SurchargeFuelEnv40Hc]      DECIMAL(18,4)   NULL,
    [SurchargeMisc20]           DECIMAL(18,4)   NULL,
    [SurchargeMisc40]           DECIMAL(18,4)   NULL,
    [SurchargeMisc40Hc]         DECIMAL(18,4)   NULL,
    [SurchargeSecurityEnv20]    DECIMAL(18,4)   NULL,
    [SurchargeSecurityEnv40]    DECIMAL(18,4)   NULL,
    [SurchargeSecurityEnv40Hc]  DECIMAL(18,4)   NULL,
    [TotalOf20]                 DECIMAL(18,4)   NULL,
    [TotalOf40]                 DECIMAL(18,4)   NULL,
    [TotalOf40Hc]               DECIMAL(18,4)   NULL,
    [VatCom20]                  DECIMAL(18,4)   NULL,
    [VatCom40]                  DECIMAL(18,4)   NULL,
    [VatCom40Hc]                DECIMAL(18,4)   NULL,
    [DateImport]                DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME(),
    [UserImport]                NVARCHAR(100)   NULL,
    [CreatedAt]                 DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Carrier/Pol/Pod are NVARCHAR(MAX) (unbounded, to avoid truncation on messy source
-- data), which SQL Server cannot use as a regular index key; only TradeLane and
-- SourceFileName are indexed (used to filter tabs and to manage/delete by imported file).
CREATE INDEX IX_PricingRate_TradeLane ON dbo.PricingRate(TradeLane);
CREATE INDEX IX_PricingRate_SourceFileName ON dbo.PricingRate(SourceFileName);
GO

PRINT N'dbo.PricingRate table created.';
