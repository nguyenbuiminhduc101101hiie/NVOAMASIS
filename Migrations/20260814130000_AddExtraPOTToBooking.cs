using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddExtraPOTToBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hano_POT_Ext",
                table: "CONTAINEROUTBOUNDNOTIFY_sale",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hano_POT_Ext",
                table: "CONTAINEROUTBOUNDNOTIFY_sale");
        }
    }
}
