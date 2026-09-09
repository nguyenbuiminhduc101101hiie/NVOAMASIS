using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    public partial class AddEInvoiceCompanyInfoAndExportLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email_E_invoice",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password_E_invoice",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantID",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber_E_Invoice",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Serial_E_Invoice",
                table: "CompanyInfomation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EInvoiceExportLog",
                columns: table => new
                {
                    EInvoiceExportLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EInvoiceGuid = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LookupCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ViewUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SoHoaDonNoiBo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HblId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HblCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExportType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    LineCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Continued = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PdfFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PdfFileContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    XmlFileContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EInvoiceExportLog", x => x.EInvoiceExportLogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceExportLog_CreatedAt",
                table: "EInvoiceExportLog",
                columns: new[] { "CreatedAt" },
                descending: new[] { true });

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceExportLog_HblId",
                table: "EInvoiceExportLog",
                column: "HblId");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceExportLog_InvoiceId",
                table: "EInvoiceExportLog",
                column: "InvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EInvoiceExportLog");

            migrationBuilder.DropColumn(name: "Email_E_invoice", table: "CompanyInfomation");
            migrationBuilder.DropColumn(name: "Password_E_invoice", table: "CompanyInfomation");
            migrationBuilder.DropColumn(name: "TenantID", table: "CompanyInfomation");
            migrationBuilder.DropColumn(name: "TaxNumber_E_Invoice", table: "CompanyInfomation");
            migrationBuilder.DropColumn(name: "Serial_E_Invoice", table: "CompanyInfomation");
        }
    }
}
