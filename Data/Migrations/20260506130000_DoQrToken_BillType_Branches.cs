using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Data.Migrations
{
    /// <summary>
    /// DoQrToken: thêm BillType, Branches; index unique mới.
    /// </summary>
    public partial class DoQrTokenBillTypeBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.DoQrToken', N'BillType') IS NULL
    ALTER TABLE [dbo].[DoQrToken] ADD [BillType] [nvarchar](50) NULL;");

            migrationBuilder.Sql(@"
UPDATE [dbo].[DoQrToken] SET [BillType] = N'PASL' WHERE [BillType] IS NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.DoQrToken') AND c.name = N'BillType' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[DoQrToken] ALTER COLUMN [BillType] [nvarchar](50) NOT NULL;");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.DoQrToken', N'Branches') IS NULL
    ALTER TABLE [dbo].[DoQrToken] ADD [Branches] [nvarchar](500) NULL;");

            migrationBuilder.Sql(@"
UPDATE [dbo].[DoQrToken] SET [Branches] = N'' WHERE [Branches] IS NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'dbo.DoQrToken') AND c.name = N'Branches' AND c.is_nullable = 1)
    ALTER TABLE [dbo].[DoQrToken] ALTER COLUMN [Branches] [nvarchar](500) NOT NULL;");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    DROP INDEX [IX_DoQrToken_HblId_Type] ON [dbo].[DoQrToken];");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_DoQrToken_HblId_Type_BillType_Branches]
        ON [dbo].[DoQrToken] ([HblId] ASC, [Type] ASC, [BillType] ASC, [Branches] ASC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type_BillType_Branches' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    DROP INDEX [IX_DoQrToken_HblId_Type_BillType_Branches] ON [dbo].[DoQrToken];");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.DoQrToken', N'Branches') IS NOT NULL
    ALTER TABLE [dbo].[DoQrToken] DROP COLUMN [Branches];");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.DoQrToken', N'BillType') IS NOT NULL
    ALTER TABLE [dbo].[DoQrToken] DROP COLUMN [BillType];");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_DoQrToken_HblId_Type' AND object_id = OBJECT_ID(N'[dbo].[DoQrToken]'))
    CREATE UNIQUE NONCLUSTERED INDEX [IX_DoQrToken_HblId_Type]
        ON [dbo].[DoQrToken] ([HblId] ASC, [Type] ASC);");
        }
    }
}
