using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260926120000_AddBankPaymentToPhieuChi")]
    public partial class AddBankPaymentToPhieuChi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Phieuchi', N'dathanhtoan') IS NULL
                    ALTER TABLE [dbo].[Phieuchi]
                    ADD [dathanhtoan] bit NULL
                        CONSTRAINT [DF_Phieuchi_dathanhtoan] DEFAULT CAST(0 AS bit);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Phieuchi', N'NgayThanhToan') IS NULL
                    ALTER TABLE [dbo].[Phieuchi] ADD [NgayThanhToan] datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.BankTransaction', N'PhieuchiID') IS NULL
                    ALTER TABLE [dbo].[BankTransaction] ADD [PhieuchiID] uniqueidentifier NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankTransaction_PhieuchiID' AND object_id = OBJECT_ID(N'dbo.BankTransaction'))
                    CREATE INDEX [IX_BankTransaction_PhieuchiID]
                        ON [dbo].[BankTransaction] ([PhieuchiID]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankTransaction_PhieuchiID' AND object_id = OBJECT_ID(N'dbo.BankTransaction'))
                    DROP INDEX [IX_BankTransaction_PhieuchiID] ON [dbo].[BankTransaction];

                IF COL_LENGTH(N'dbo.BankTransaction', N'PhieuchiID') IS NOT NULL
                    ALTER TABLE [dbo].[BankTransaction] DROP COLUMN [PhieuchiID];

                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE [name] = N'DF_Phieuchi_dathanhtoan')
                    ALTER TABLE [dbo].[Phieuchi] DROP CONSTRAINT [DF_Phieuchi_dathanhtoan];

                IF COL_LENGTH(N'dbo.Phieuchi', N'dathanhtoan') IS NOT NULL
                    ALTER TABLE [dbo].[Phieuchi] DROP COLUMN [dathanhtoan];

                IF COL_LENGTH(N'dbo.Phieuchi', N'NgayThanhToan') IS NOT NULL
                    ALTER TABLE [dbo].[Phieuchi] DROP COLUMN [NgayThanhToan];
                """);
        }
    }
}
