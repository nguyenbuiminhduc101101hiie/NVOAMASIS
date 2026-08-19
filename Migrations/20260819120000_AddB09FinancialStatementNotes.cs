using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVOAMASIS.Migrations
{
    /// <summary>
    /// B09-DN (TT99/2025) reporting tables. Does not change GeneralLedgerEntries.
    /// </summary>
    public partial class AddB09FinancialStatementNotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountingRegime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelatedPartyProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    RelatedPartyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelationshipDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatedPartyProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustodyAssets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    ShipmentId = table.Column<long>(type: "bigint", nullable: true),
                    ContainerId = table.Column<long>(type: "bigint", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: true),
                    CustodyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommodityGroup = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Specification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RightsAndObligations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StorageResponsibility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignificantRisk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisclosureNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodyAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteSections_FinancialStatementNoteTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FinancialStatementNoteTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteReports_FinancialStatementNoteTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "FinancialStatementNoteTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    ParentLineId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequiredNarrative = table.Column<bool>(type: "bit", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteLines_FinancialStatementNoteLines_ParentLineId",
                        column: x => x.ParentLineId,
                        principalTable: "FinancialStatementNoteLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteLines_FinancialStatementNoteSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "FinancialStatementNoteSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteLineId = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    AccountPrefixesCsv = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SignMultiplier = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    DetailThresholdPercent = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    CustomSql = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteMappings_FinancialStatementNoteLines_NoteLineId",
                        column: x => x.NoteLineId,
                        principalTable: "FinancialStatementNoteLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialStatementNoteValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteLineId = table.Column<int>(type: "int", nullable: false),
                    CurrentSystemValue = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: true),
                    CurrentAdjustment = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: false),
                    PreviousSystemValue = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: true),
                    PreviousAdjustment = table.Column<decimal>(type: "decimal(28,4)", precision: 28, scale: 4, nullable: false),
                    Narrative = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustmentReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialStatementNoteValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteValues_FinancialStatementNoteLines_NoteLineId",
                        column: x => x.NoteLineId,
                        principalTable: "FinancialStatementNoteLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialStatementNoteValues_FinancialStatementNoteReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "FinancialStatementNoteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteTemplates_Code_Version",
                table: "FinancialStatementNoteTemplates",
                columns: new[] { "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteSections_TemplateId_Code",
                table: "FinancialStatementNoteSections",
                columns: new[] { "TemplateId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteLines_ParentLineId",
                table: "FinancialStatementNoteLines",
                column: "ParentLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteLines_SectionId_Code",
                table: "FinancialStatementNoteLines",
                columns: new[] { "SectionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteMappings_NoteLineId",
                table: "FinancialStatementNoteMappings",
                column: "NoteLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteReports_CompanyId_TemplateId_FiscalYear",
                table: "FinancialStatementNoteReports",
                columns: new[] { "CompanyId", "TemplateId", "FiscalYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteReports_TemplateId",
                table: "FinancialStatementNoteReports",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteValues_NoteLineId",
                table: "FinancialStatementNoteValues",
                column: "NoteLineId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialStatementNoteValues_ReportId_NoteLineId",
                table: "FinancialStatementNoteValues",
                columns: new[] { "ReportId", "NoteLineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelatedPartyProfiles_CompanyId_ClientId_EffectiveFrom",
                table: "RelatedPartyProfiles",
                columns: new[] { "CompanyId", "ClientId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyAssets_CompanyId_ClientId",
                table: "CustodyAssets",
                columns: new[] { "CompanyId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustodyAssets_CompanyId_ContainerId",
                table: "CustodyAssets",
                columns: new[] { "CompanyId", "ContainerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "FinancialStatementNoteValues");
            migrationBuilder.DropTable(name: "FinancialStatementNoteMappings");
            migrationBuilder.DropTable(name: "CustodyAssets");
            migrationBuilder.DropTable(name: "RelatedPartyProfiles");
            migrationBuilder.DropTable(name: "FinancialStatementNoteLines");
            migrationBuilder.DropTable(name: "FinancialStatementNoteReports");
            migrationBuilder.DropTable(name: "FinancialStatementNoteSections");
            migrationBuilder.DropTable(name: "FinancialStatementNoteTemplates");
        }
    }
}
