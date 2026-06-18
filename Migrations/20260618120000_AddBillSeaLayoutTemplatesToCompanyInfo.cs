using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddBillSeaLayoutTemplatesToCompanyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "BillSeaLayoutMrt",
                table: "CompanyInfomation",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "BillSeaLayoutAttMrt",
                table: "CompanyInfomation",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillSeaLayoutMrt",
                table: "CompanyInfomation");

            migrationBuilder.DropColumn(
                name: "BillSeaLayoutAttMrt",
                table: "CompanyInfomation");
        }
    }
}
