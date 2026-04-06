using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingTemperatureAndVentOpenToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SettingTemperature",
                table: "CONTAINEROUTBOUNDNOTIFY_sale",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VentOpen",
                table: "CONTAINEROUTBOUNDNOTIFY_sale",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettingTemperature",
                table: "CONTAINEROUTBOUNDNOTIFY_sale");

            migrationBuilder.DropColumn(
                name: "VentOpen",
                table: "CONTAINEROUTBOUNDNOTIFY_sale");
        }
    }
}
