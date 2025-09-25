using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddBulgarianSteps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompletionDateHidden",
                table: "RelocationPlanStep",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "RelocationPlanStatus",
                columns: new[] { "Id", "CaseStatusId", "ExternalId", "Name" },
                values: new object[,]
                {
                    { 40, null, "trp_docs_translation_and_legalization", "TRP docs translation and legalization" },
                    { 41, null, "trp_docs_submission_to_migration_directorate", "TRP docs submission to the migration directorate" },
                    { 42, null, "trp_id_card_docs_in_progress", "ID card docs in progress" },
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanTrpState",
                column: "Id",
                values: new object[]
                {
                    "DocsTranslationAndLegalization",
                    "SubmissionToMigrationDirectorate",
                    "IdCardDocsInProgress",
                });

            migrationBuilder.InsertData(
                table: "RelocationStep",
                column: "Id",
                values: new object[]
                {
                    "TrpDocsTranslationAndLegalization",
                    "TrpDocsSubmissionToMigrationDirectorate",
                    "TrpIdCardDocsInProgress",
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "RelocationPlanTrpState",
                keyColumn: "Id",
                keyValue: "DocsTranslationAndLegalization");

            migrationBuilder.DeleteData(
                table: "RelocationPlanTrpState",
                keyColumn: "Id",
                keyValue: "SubmissionToMigrationDirectorate");

            migrationBuilder.DeleteData(
                table: "RelocationPlanTrpState",
                keyColumn: "Id",
                keyValue: "IdCardDocsInProgress");

            migrationBuilder.DeleteData(
                table: "RelocationStep",
                keyColumn: "Id",
                keyValue: "TrpDocsTranslationAndLegalization");

            migrationBuilder.DeleteData(
                table: "RelocationStep",
                keyColumn: "Id",
                keyValue: "TrpDocsSubmissionToMigrationDirectorate");

            migrationBuilder.DeleteData(
                table: "RelocationStep",
                keyColumn: "Id",
                keyValue: "TrpIdCardDocsInProgress");

            migrationBuilder.DropColumn(
                name: "IsCompletionDateHidden",
                table: "RelocationPlanStep");
        }
    }
}
