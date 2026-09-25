using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260924120000_AddBankPaymentToPhieuThu")]
    public partial class AddBankPaymentToPhieuThu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Phieuthu', N'dathanhtoan') IS NULL
                    ALTER TABLE [dbo].[Phieuthu]
                    ADD [dathanhtoan] bit NULL
                        CONSTRAINT [DF_Phieuthu_dathanhtoan] DEFAULT CAST(0 AS bit);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Phieuthu', N'NgayThanhToan') IS NULL
                    ALTER TABLE [dbo].[Phieuthu] ADD [NgayThanhToan] datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.BankTransaction', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[BankTransaction]
                    (
                        [Id]              uniqueidentifier NOT NULL CONSTRAINT [PK_BankTransaction] PRIMARY KEY,
                        [Provider]        nvarchar(20)     NOT NULL,
                        [ProviderTxnId]   nvarchar(50)     NOT NULL,
                        [Gateway]         nvarchar(100)    NULL,
                        [AccountNumber]   nvarchar(50)     NULL,
                        [SubAccount]      nvarchar(50)     NULL,
                        [TransactionDate] datetime2        NULL,
                        [TransferType]    nvarchar(10)     NULL,
                        [Amount]          decimal(18,2)    NOT NULL,
                        [Content]         nvarchar(1000)   NULL,
                        [ReferenceCode]   nvarchar(100)    NULL,
                        [RawJson]         nvarchar(max)    NULL,
                        [PhieuthuID]      uniqueidentifier NULL,
                        [MatchStatus]     nvarchar(20)     NOT NULL,
                        [ReceivedAt]      datetime2        NOT NULL
                    );

                    CREATE UNIQUE INDEX [UX_BankTransaction_Provider_TxnId]
                        ON [dbo].[BankTransaction] ([Provider], [ProviderTxnId]);
                    CREATE INDEX [IX_BankTransaction_PhieuthuID]
                        ON [dbo].[BankTransaction] ([PhieuthuID]);
                END;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.BankTransaction', N'U') IS NOT NULL
                    DROP TABLE [dbo].[BankTransaction];

                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE [name] = N'DF_Phieuthu_dathanhtoan')
                    ALTER TABLE [dbo].[Phieuthu] DROP CONSTRAINT [DF_Phieuthu_dathanhtoan];

                IF COL_LENGTH(N'dbo.Phieuthu', N'dathanhtoan') IS NOT NULL
                    ALTER TABLE [dbo].[Phieuthu] DROP COLUMN [dathanhtoan];

                IF COL_LENGTH(N'dbo.Phieuthu', N'NgayThanhToan') IS NOT NULL
                    ALTER TABLE [dbo].[Phieuthu] DROP COLUMN [NgayThanhToan];
                """);
        }
    }
}
