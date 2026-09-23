using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Trả lời, chuyển tiếp, ghim tin nhắn — tương đương Scripts/AlterChatTables_AddForwardPin.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260925090000_AddChatForwardPin")]
    public partial class AddChatForwardPin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'IsForwarded') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [IsForwarded] bit NOT NULL
                        CONSTRAINT [DF_ChatMessages_IsForwarded] DEFAULT (0);
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedAt') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [PinnedAt] datetime2 NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedByUserId') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [PinnedByUserId] uniqueidentifier NULL;
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'ReplyToMessageId') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [ReplyToMessageId] uniqueidentifier NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChatMessages_Pinned' AND object_id = OBJECT_ID(N'dbo.ChatMessages'))
                    EXEC(N'CREATE INDEX [IX_ChatMessages_Pinned] ON [dbo].[ChatMessages]([ConversationId], [PinnedAt]) WHERE [PinnedAt] IS NOT NULL');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChatMessages_Pinned' AND object_id = OBJECT_ID(N'dbo.ChatMessages'))
                    DROP INDEX [IX_ChatMessages_Pinned] ON [dbo].[ChatMessages];
                IF COL_LENGTH(N'dbo.ChatMessages', N'ReplyToMessageId') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [ReplyToMessageId];
                IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedByUserId') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [PinnedByUserId];
                IF COL_LENGTH(N'dbo.ChatMessages', N'PinnedAt') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [PinnedAt];
                IF OBJECT_ID(N'dbo.DF_ChatMessages_IsForwarded', N'D') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP CONSTRAINT [DF_ChatMessages_IsForwarded];
                IF COL_LENGTH(N'dbo.ChatMessages', N'IsForwarded') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [IsForwarded];
                """);
        }
    }
}
