using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Kết quả BKAV cho Tax Invoice — tương đương Scripts/AlterTaxInvoice_AddBkav.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260927090000_AddBkavToTaxInvoice")]
    public partial class AddBkavToTaxInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceID') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPartnerInvoiceID] bigint NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceStringID') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPartnerInvoiceStringID] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceNo') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceNo] int NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceForm') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceForm] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceSerial') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceSerial] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceLink') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavInvoiceLink] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPdfPath') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavPdfPath] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavXmlPath') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavXmlPath] nvarchar(max) NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavStatusID') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavStatusID] int NULL;
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavLastMessage') IS NULL
                    ALTER TABLE [dbo].[TaxInvoice] ADD [BkavLastMessage] nvarchar(max) NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceID') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavPartnerInvoiceID];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPartnerInvoiceStringID') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavPartnerInvoiceStringID];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceNo') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavInvoiceNo];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceForm') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavInvoiceForm];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceSerial') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavInvoiceSerial];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavInvoiceLink') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavInvoiceLink];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavPdfPath') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavPdfPath];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavXmlPath') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavXmlPath];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavStatusID') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavStatusID];
                IF COL_LENGTH(N'dbo.TaxInvoice', N'BkavLastMessage') IS NOT NULL
                    ALTER TABLE [dbo].[TaxInvoice] DROP COLUMN [BkavLastMessage];
                """);
        }
    }
}
