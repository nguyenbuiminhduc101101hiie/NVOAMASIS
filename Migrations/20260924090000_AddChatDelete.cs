using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Thu hồi tin nhắn / xoá hội thoại phía mình — tương đương Scripts/AlterChatTables_AddDelete.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260924090000_AddChatDelete")]
    public partial class AddChatDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'DeletedAt') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [DeletedAt] datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatParticipants', N'ClearedAt') IS NULL
                    ALTER TABLE [dbo].[ChatParticipants] ADD [ClearedAt] datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatParticipants', N'IsHidden') IS NULL
                    ALTER TABLE [dbo].[ChatParticipants] ADD [IsHidden] bit NOT NULL
                        CONSTRAINT [DF_ChatParticipants_IsHidden] DEFAULT (0);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'DeletedAt') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [DeletedAt];
                IF COL_LENGTH(N'dbo.ChatParticipants', N'ClearedAt') IS NOT NULL
                    ALTER TABLE [dbo].[ChatParticipants] DROP COLUMN [ClearedAt];
                IF OBJECT_ID(N'dbo.DF_ChatParticipants_IsHidden', N'D') IS NOT NULL
                    ALTER TABLE [dbo].[ChatParticipants] DROP CONSTRAINT [DF_ChatParticipants_IsHidden];
                IF COL_LENGTH(N'dbo.ChatParticipants', N'IsHidden') IS NOT NULL
                    ALTER TABLE [dbo].[ChatParticipants] DROP COLUMN [IsHidden];
                """);
        }
    }
}
