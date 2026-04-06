using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>
    /// Inserts SharedResource key SI (Shipment Instruction) for en-US, vi-VN, zh-CN.
    /// </summary>
    public partial class AddSILocalizationResources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM LocalizationResources
                WHERE ResourceKey = N'SI';

                INSERT INTO LocalizationResources (ResourceKey, Culture, Value) VALUES
                (N'SI', N'en-US', N'Shipment Instruction'),
                (N'SI', N'vi-VN', N'Chỉ thị giao hàng'),
                (N'SI', N'zh-CN', N'装运指示');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM LocalizationResources
                WHERE ResourceKey = N'SI';
                """);
        }
    }
}
