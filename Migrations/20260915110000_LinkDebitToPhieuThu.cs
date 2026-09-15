using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260915110000_LinkDebitToPhieuThu")]
    public partial class LinkDebitToPhieuThu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Debit', N'dathanhtoan') IS NULL
                    ALTER TABLE [dbo].[Debit]
                    ADD [dathanhtoan] bit NULL
                        CONSTRAINT [DF_Debit_dathanhtoan] DEFAULT CAST(0 AS bit);

                IF COL_LENGTH(N'dbo.Debit', N'PhieuthuID') IS NULL
                    ALTER TABLE [dbo].[Debit]
                    ADD [PhieuthuID] uniqueidentifier NULL;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Debit_PhieuthuID'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Debit]')
                )
                    CREATE INDEX [IX_Debit_PhieuthuID]
                    ON [dbo].[Debit] ([PhieuthuID]);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS
                (
                    SELECT 1
                    FROM sys.indexes
                    WHERE [name] = N'IX_Debit_PhieuthuID'
                      AND [object_id] = OBJECT_ID(N'[dbo].[Debit]')
                )
                    DROP INDEX [IX_Debit_PhieuthuID] ON [dbo].[Debit];

                IF COL_LENGTH(N'dbo.Debit', N'PhieuthuID') IS NOT NULL
                    ALTER TABLE [dbo].[Debit] DROP COLUMN [PhieuthuID];
                """);
        }
    }
}
