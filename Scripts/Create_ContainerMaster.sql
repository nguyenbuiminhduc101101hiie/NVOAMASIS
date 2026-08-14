/*
  Bảng ContainerMaster: quản lý thông tin container và chủ container.
  - ID là khóa chính nội bộ (GUID), tự sinh bằng NEWID().
  - ContainerNo chọn từ dbo.Container(CONTAINER_NO) hoặc tự nhập tay, không cho trùng.
  - OwnerID lưu theo dbo.Customer(Customer_ID).
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.ContainerMaster', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContainerMaster (
        ID                  UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_ContainerMaster PRIMARY KEY CLUSTERED
            CONSTRAINT DF_ContainerMaster_ID DEFAULT (NEWID()),
        ContainerNo         NVARCHAR(20)     NOT NULL       -- TGHU1234567
            CONSTRAINT UQ_ContainerMaster_ContainerNo UNIQUE,
        ContainerType       NVARCHAR(20)     NULL,          -- 20GP, 40GP, 40HC, Reefer...
        OwnershipType       NVARCHAR(20)     NULL,          -- SOC / COC / Leased
        OwnerID             UNIQUEIDENTIFIER NULL,          -- dbo.Customer.Customer_ID
        Status              NVARCHAR(20)     NULL,          -- Available / Booked / Laden / Empty / Repair
        CurrentLocation     NVARCHAR(200)    NULL,
        CurrentDepot        NVARCHAR(200)    NULL,
        ManufactureDate     DATETIME2(7)     NULL,
        LastInspectionDate  DATETIME2(7)     NULL,
        Condition           NVARCHAR(20)     NULL           -- Good / Damaged / Repair
    );
END
GO

PRINT N'Create_ContainerMaster: OK.';
GO
