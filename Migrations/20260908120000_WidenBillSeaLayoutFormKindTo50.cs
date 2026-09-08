using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class WidenBillSeaLayoutFormKindTo50 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FormKind",
                table: "BillSeaLayoutForm",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Sea",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Sea");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FormKind",
                table: "BillSeaLayoutForm",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Sea",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Sea");
        }
    }
}
