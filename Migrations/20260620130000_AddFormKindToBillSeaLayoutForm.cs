using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddFormKindToBillSeaLayoutForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormKind",
                table: "BillSeaLayoutForm",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Sea");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormKind",
                table: "BillSeaLayoutForm");
        }
    }
}
