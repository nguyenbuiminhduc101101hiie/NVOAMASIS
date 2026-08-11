using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddTK20ToBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TK20",
                table: "CONTAINEROUTBOUNDNOTIFY_sale",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TK20",
                table: "CONTAINEROUTBOUNDNOTIFY_sale");
        }
    }
}
