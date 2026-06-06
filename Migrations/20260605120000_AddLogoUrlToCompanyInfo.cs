using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddLogoUrlToCompanyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Logo",
                table: "CompanyInfomation",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                table: "CompanyInfomation");
        }
    }
}
