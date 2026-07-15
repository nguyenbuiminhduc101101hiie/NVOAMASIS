/*
  Align dbo.ShipmentChargeContext with M_ShipmentChargeContext:
  - Keep CustomerId (from HBL.CustomerID)
  - Drop obsolete context columns no longer saved by the app
  - Ensure tariffcode_id / Amount exist
*/
IF OBJECT_ID(N'dbo.ShipmentChargeContext', N'U') IS NULL
BEGIN
    RAISERROR(N'ShipmentChargeContext table does not exist.', 16, 1);
    RETURN;
END
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'ChargeTypeId') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN ChargeTypeId;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'ShippingLineId') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN ShippingLineId;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'DepotId') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN DepotId;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'PortId') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN PortId;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'ContainerTypeId') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN ContainerTypeId;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'Direction') IS NOT NULL
    ALTER TABLE dbo.ShipmentChargeContext DROP COLUMN Direction;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'CustomerId') IS NULL
    ALTER TABLE dbo.ShipmentChargeContext ADD CustomerId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_ShipmentChargeContext_CustomerId DEFAULT ('00000000-0000-0000-0000-000000000000');
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'tariffcode_id') IS NULL
    ALTER TABLE dbo.ShipmentChargeContext ADD tariffcode_id UNIQUEIDENTIFIER NULL;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'Amount') IS NULL
    ALTER TABLE dbo.ShipmentChargeContext ADD Amount DECIMAL(18, 0) NULL;
GO

IF COL_LENGTH(N'dbo.ShipmentChargeContext', N'Amount') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        JOIN sys.types t ON c.user_type_id = t.user_type_id
        WHERE c.object_id = OBJECT_ID(N'dbo.ShipmentChargeContext')
          AND c.name = N'Amount'
          AND NOT (t.name = N'decimal' AND c.precision = 18 AND c.scale = 0))
        ALTER TABLE dbo.ShipmentChargeContext ALTER COLUMN Amount DECIMAL(18, 0) NULL;
END
GO

PRINT N'AlterShipmentChargeContext_DropObsoleteColumns: OK.';
GO
