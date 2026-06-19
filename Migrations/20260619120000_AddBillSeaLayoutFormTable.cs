using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddBillSeaLayoutFormTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillSeaLayoutForm",
                columns: table => new
                {
                    BillSeaLayoutFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MrtContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SourceTemplate = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false, defaultValue: "BillSea_NVOCC.mrt"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillSeaLayoutForm", x => x.BillSeaLayoutFormId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillSeaLayoutForm_FormName",
                table: "BillSeaLayoutForm",
                column: "FormName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BillSeaLayoutForm");
        }
    }
}
