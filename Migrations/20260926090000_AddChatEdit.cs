using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NVOAMASIS.Data;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>Chỉnh sửa tin nhắn — tương đương Scripts/AlterChatTables_AddEdit.sql</summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260926090000_AddChatEdit")]
    public partial class AddChatEdit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'EditedAt') IS NULL
                    ALTER TABLE [dbo].[ChatMessages] ADD [EditedAt] datetime2 NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.ChatMessages', N'EditedAt') IS NOT NULL
                    ALTER TABLE [dbo].[ChatMessages] DROP COLUMN [EditedAt];
                """);
        }
    }
}
