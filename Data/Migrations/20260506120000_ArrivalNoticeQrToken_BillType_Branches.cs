using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Data.Migrations
{
    /// <summary>
    /// Bảng ArrivalNoticeQrToken: thêm BillType, Branches; đổi unique index (HblId, Type, BillType, Branches).
    /// Mỗi migrationBuilder.Sql = một batch — tránh lỗi SQL Server: không thể UPDATE cột vừa ADD trong cùng batch.
    /// </summary>
    public partial class ArrivalNoticeQrTokenBillTypeBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'BillType') IS NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ADD [BillType] [nvarchar](50) NULL;");

            migrationBuilder.Sql(@"
UPDATE [dbo].[ArrivalNoticeQrToken] SET [BillType] = N'PASL' WHERE [BillType] IS NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.ArrivalNoticeQrToken') AND c.name = N'BillType' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ALTER COLUMN [BillType] [nvarchar](50) NOT NULL;");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'Branches') IS NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ADD [Branches] [nvarchar](500) NULL;");

            migrationBuilder.Sql(@"
UPDATE [dbo].[ArrivalNoticeQrToken] SET [Branches] = N'' WHERE [Branches] IS NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.ArrivalNoticeQrToken') AND c.name = N'Branches' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] ALTER COLUMN [Branches] [nvarchar](500) NOT NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    DROP INDEX [IX_ArrivalNoticeQrToken_HblId_Type] ON [dbo].[ArrivalNoticeQrToken];");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[ArrivalNoticeQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    DROP INDEX [IX_ArrivalNoticeQrToken_HblId_Type_BillType_Branches] ON [dbo].[ArrivalNoticeQrToken];");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'Branches') IS NOT NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] DROP COLUMN [Branches];");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ArrivalNoticeQrToken', N'BillType') IS NOT NULL
    ALTER TABLE [dbo].[ArrivalNoticeQrToken] DROP COLUMN [BillType];");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_ArrivalNoticeQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[ArrivalNoticeQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ArrivalNoticeQrToken_HblId_Type]
        ON [dbo].[ArrivalNoticeQrToken] ([HblId] ASC, [Type] ASC);");
        }
    }
}
