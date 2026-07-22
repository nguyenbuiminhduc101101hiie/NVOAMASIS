using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class WidenBillSeaLayoutFormKind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FormKind",
                table: "BillSeaLayoutForm",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Sea",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "Sea");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FormKind",
                table: "BillSeaLayoutForm",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Sea",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Sea");
        }
    }
}
