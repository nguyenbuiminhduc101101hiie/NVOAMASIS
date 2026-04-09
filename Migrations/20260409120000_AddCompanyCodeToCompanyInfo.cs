using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddCompanyCodeToCompanyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "CompanyInfomation");
        }
    }
}
