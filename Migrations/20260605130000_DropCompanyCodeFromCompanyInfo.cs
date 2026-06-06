using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class DropCompanyCodeFromCompanyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "CompanyInfomation");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
