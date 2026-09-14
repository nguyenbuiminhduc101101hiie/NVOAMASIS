using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddCocSocFieldsToLocalChargesPt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContainerType",
                table: "localcharges_pt",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DEM",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DET",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Deposit",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FreeTimeDemDet",
                table: "localcharges_pt",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RepairFee",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SurveyFee",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FumigationFee",
                table: "localcharges_pt",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SocHandlingFee",
                table: "localcharges_pt",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ContainerType", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "DEM", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "DET", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "Deposit", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "FreeTimeDemDet", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "RepairFee", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "SurveyFee", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "FumigationFee", table: "localcharges_pt");
            migrationBuilder.DropColumn(name: "SocHandlingFee", table: "localcharges_pt");
        }
    }
}
