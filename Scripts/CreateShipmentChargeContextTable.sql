/*
  Bảng ngữ cảnh tính phí lô (8.7 ShipmentChargeContext).
  CustomerId: lấy từ HBL.CustomerID khi chọn HBL.
  Chi tiết tariff (ChargeType/ShippingLine/Depot/…) lấy qua tariffcode_id → TariffHeader.
*/
IF OBJECT_ID(N'dbo.ShipmentChargeContext', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ShipmentChargeContext (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ShipmentChargeContext PRIMARY KEY,
        ShipmentId UNIQUEIDENTIFIER NOT NULL,
        ContainerId UNIQUEIDENTIFIER NOT NULL,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        EmptyPickupDate DATETIME2 NULL,
        FullDischargeDate DATETIME2 NULL,
        FullDeliveryDate DATETIME2 NULL,
        EmptyReturnDate DATETIME2 NULL,
        StorageInDate DATETIME2 NULL,
        StorageOutDate DATETIME2 NULL,
        FreeDays INT NOT NULL CONSTRAINT DF_ShipmentChargeContext_FreeDays DEFAULT (0),
        BillableDays INT NULL,
        CurrencyCode NVARCHAR(16) NULL,
        Remarks NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NULL,
        tariffcode_id UNIQUEIDENTIFIER NULL,
        Amount DECIMAL(18, 0) NULL
    );
END
GO

PRINT N'CreateShipmentChargeContextTable: OK.';
