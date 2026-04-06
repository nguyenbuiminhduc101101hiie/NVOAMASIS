using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddContTypeQuantityToLenhCapContRong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContType",
                table: "LenhCapContRong",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "LenhCapContRong",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Saycont",
                table: "LenhCapContRong",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContType",
                table: "LenhCapContRong");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "LenhCapContRong");

            migrationBuilder.DropColumn(
                name: "Saycont",
                table: "LenhCapContRong");
        }
    }
}

