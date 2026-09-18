using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260918120000_AddDaThanhToanToCredit")]
    public partial class AddDaThanhToanToCredit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'dathanhtoan') IS NULL
                    ALTER TABLE [dbo].[Credit]
                    ADD [dathanhtoan] bit NULL
                        CONSTRAINT [DF_Credit_dathanhtoan] DEFAULT CAST(0 AS bit);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS
                (
                    SELECT 1 FROM sys.default_constraints
                    WHERE [name] = N'DF_Credit_dathanhtoan'
                )
                    ALTER TABLE [dbo].[Credit] DROP CONSTRAINT [DF_Credit_dathanhtoan];

                IF COL_LENGTH(N'dbo.Credit', N'dathanhtoan') IS NOT NULL
                    ALTER TABLE [dbo].[Credit] DROP COLUMN [dathanhtoan];
                """);
        }
    }
}
