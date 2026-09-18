using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260918140000_AddEorImportFieldsToCredit")]
    public partial class AddEorImportFieldsToCredit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'EstNo') IS NULL
                    ALTER TABLE [dbo].[Credit] ADD [EstNo] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'Grade') IS NULL
                    ALTER TABLE [dbo].[Credit] ADD [Grade] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'DamageDetail') IS NULL
                    ALTER TABLE [dbo].[Credit] ADD [DamageDetail] nvarchar(max) NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'Remark') IS NULL
                    ALTER TABLE [dbo].[Credit] ADD [Remark] nvarchar(max) NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'EstNo') IS NOT NULL
                    ALTER TABLE [dbo].[Credit] DROP COLUMN [EstNo];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'Grade') IS NOT NULL
                    ALTER TABLE [dbo].[Credit] DROP COLUMN [Grade];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'DamageDetail') IS NOT NULL
                    ALTER TABLE [dbo].[Credit] DROP COLUMN [DamageDetail];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Credit', N'Remark') IS NOT NULL
                    ALTER TABLE [dbo].[Credit] DROP COLUMN [Remark];
                """);
        }
    }
}
