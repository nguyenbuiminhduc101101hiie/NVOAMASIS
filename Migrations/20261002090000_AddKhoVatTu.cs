using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>10.18 Kho vật tư, hàng hóa TK 151–156 — tương đương phần tạo bảng của Scripts/CreateKhoTables.sql
    /// (mã quyền KHO_* chạy bằng script).</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261002090000_AddKhoVatTu")]
    public partial class AddKhoVatTu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.KhoVatTu', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KhoVatTu] (
                        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_KhoVatTu] PRIMARY KEY,
                        [Code]             nvarchar(50)     NOT NULL,
                        [Name]             nvarchar(250)    NOT NULL,
                        [Unit]             nvarchar(30)     NOT NULL CONSTRAINT [DF_KhoVatTu_Unit] DEFAULT N'',
                        [InventoryAccount] nvarchar(20)     NOT NULL,
                        [Category]         nvarchar(100)    NULL,
                        [MinQty]           decimal(18,4)    NULL,
                        [Note]             nvarchar(500)    NULL,
                        [IsActive]         bit              NOT NULL CONSTRAINT [DF_KhoVatTu_IsActive] DEFAULT 1,
                        [CreatedAt]        datetime2        NOT NULL CONSTRAINT [DF_KhoVatTu_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy]        nvarchar(100)    NULL,
                        [UpdatedAt]        datetime2        NULL,
                        [UpdatedBy]        nvarchar(100)    NULL
                    );
                    CREATE UNIQUE INDEX [IX_KhoVatTu_Code] ON [dbo].[KhoVatTu]([Code]);
                END;

                IF OBJECT_ID(N'dbo.KhoHang', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[KhoHang] (
                        [Id]        uniqueidentifier NOT NULL CONSTRAINT [PK_KhoHang] PRIMARY KEY,
                        [Code]      nvarchar(50)     NOT NULL,
                        [Name]      nvarchar(250)    NOT NULL,
                        [Address]   nvarchar(500)    NULL,
                        [Keeper]    nvarchar(150)    NULL,
                        [IsActive]  bit              NOT NULL CONSTRAINT [DF_KhoHang_IsActive] DEFAULT 1,
                        [CreatedAt] datetime2        NOT NULL CONSTRAINT [DF_KhoHang_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy] nvarchar(100)    NULL,
                        [UpdatedAt] datetime2        NULL,
                        [UpdatedBy] nvarchar(100)    NULL
                    );
                    CREATE UNIQUE INDEX [IX_KhoHang_Code] ON [dbo].[KhoHang]([Code]);
                END;

                IF OBJECT_ID(N'dbo.PhieuKho', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[PhieuKho] (
                        [Id]            uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKho] PRIMARY KEY,
                        [DocType]       nvarchar(10)     NOT NULL,
                        [DocNo]         nvarchar(30)     NOT NULL,
                        [DocDate]       datetime2        NOT NULL,
                        [Reason]        nvarchar(30)     NULL,
                        [WarehouseId]   uniqueidentifier NOT NULL,
                        [ToWarehouseId] uniqueidentifier NULL,
                        [PartnerId]     uniqueidentifier NULL,
                        [PartnerName]   nvarchar(250)    NULL,
                        [ContactName]   nvarchar(150)    NULL,
                        [ContraAccount] nvarchar(20)     NULL,
                        [VatAccount]    nvarchar(20)     NULL,
                        [InvoiceNo]     nvarchar(50)     NULL,
                        [InvoiceDate]   datetime2        NULL,
                        [Description]   nvarchar(500)    NULL,
                        [Status]        int              NOT NULL CONSTRAINT [DF_PhieuKho_Status] DEFAULT 0,
                        [VoucherId]     uniqueidentifier NULL,
                        [CreatedAt]     datetime2        NOT NULL CONSTRAINT [DF_PhieuKho_CreatedAt] DEFAULT SYSDATETIME(),
                        [CreatedBy]     nvarchar(100)    NULL,
                        [UpdatedAt]     datetime2        NULL,
                        [UpdatedBy]     nvarchar(100)    NULL,
                        [PostedAt]      datetime2        NULL,
                        [PostedBy]      nvarchar(100)    NULL
                    );
                    CREATE UNIQUE INDEX [IX_PhieuKho_DocNo] ON [dbo].[PhieuKho]([DocNo]);
                    CREATE INDEX [IX_PhieuKho_DocDate_DocType] ON [dbo].[PhieuKho]([DocDate], [DocType]);
                END;

                IF OBJECT_ID(N'dbo.PhieuKhoChiTiet', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[PhieuKhoChiTiet] (
                        [Id]               uniqueidentifier NOT NULL CONSTRAINT [PK_PhieuKhoChiTiet] PRIMARY KEY,
                        [PhieuKhoId]       uniqueidentifier NOT NULL,
                        [LineNo]           int              NOT NULL,
                        [VatTuId]          uniqueidentifier NOT NULL,
                        [InventoryAccount] nvarchar(20)     NOT NULL,
                        [ContraAccount]    nvarchar(20)     NULL,
                        [Quantity]         decimal(18,4)    NOT NULL,
                        [UnitCost]         decimal(18,4)    NOT NULL,
                        [Amount]           decimal(18,2)    NOT NULL,
                        [VatRate]          decimal(5,2)     NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatRate] DEFAULT 0,
                        [VatAmount]        decimal(18,2)    NOT NULL CONSTRAINT [DF_PhieuKhoChiTiet_VatAmount] DEFAULT 0,
                        [Note]             nvarchar(500)    NULL
                    );
                    CREATE INDEX [IX_PhieuKhoChiTiet_PhieuKhoId] ON [dbo].[PhieuKhoChiTiet]([PhieuKhoId]);
                    CREATE INDEX [IX_PhieuKhoChiTiet_VatTuId] ON [dbo].[PhieuKhoChiTiet]([VatTuId]);
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS [dbo].[PhieuKhoChiTiet];
                DROP TABLE IF EXISTS [dbo].[PhieuKho];
                DROP TABLE IF EXISTS [dbo].[KhoHang];
                DROP TABLE IF EXISTS [dbo].[KhoVatTu];
                """);
        }
    }
}
