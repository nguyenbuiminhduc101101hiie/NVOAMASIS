using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260925120000_AddInvoiceLinkToBankTransaction")]
    public partial class AddInvoiceLinkToBankTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.BankTransaction', N'HoaDonNoibo') IS NULL
                    ALTER TABLE [dbo].[BankTransaction] ADD [HoaDonNoibo] nvarchar(100) NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.BankTransaction', N'HoaDonCustomerId') IS NULL
                    ALTER TABLE [dbo].[BankTransaction] ADD [HoaDonCustomerId] uniqueidentifier NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankTransaction_HoaDon' AND object_id = OBJECT_ID(N'dbo.BankTransaction'))
                    CREATE INDEX [IX_BankTransaction_HoaDon]
                        ON [dbo].[BankTransaction] ([HoaDonNoibo], [HoaDonCustomerId]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankTransaction_HoaDon' AND object_id = OBJECT_ID(N'dbo.BankTransaction'))
                    DROP INDEX [IX_BankTransaction_HoaDon] ON [dbo].[BankTransaction];

                IF COL_LENGTH(N'dbo.BankTransaction', N'HoaDonCustomerId') IS NOT NULL
                    ALTER TABLE [dbo].[BankTransaction] DROP COLUMN [HoaDonCustomerId];

                IF COL_LENGTH(N'dbo.BankTransaction', N'HoaDonNoibo') IS NOT NULL
                    ALTER TABLE [dbo].[BankTransaction] DROP COLUMN [HoaDonNoibo];
                """);
        }
    }
}
