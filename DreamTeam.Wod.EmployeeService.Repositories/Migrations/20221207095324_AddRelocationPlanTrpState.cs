using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanTrpState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrpState",
                table: "RelocationCaseProgress",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RelocationPlanTrpState",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanTrpState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanStatus",
                columns: new[] { "Id", "CaseStatusId", "ExternalId", "Name" },
                values: new object[,]
                {
                    { 30, null, "trp_docs_preparation", "TRP Docs Preparation" },
                    { 31, null, "trp_application_submission", "TRP Application Submission" },
                    { 32, null, "trp_in_progress", "TRP In Progress" },
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanTrpState",
                column: "Id",
                values: new object[]
                {
                    "DocsPreparation",
                    "ApplicationSubmission",
                    "InProgress",
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanStatusTimeLimit",
                columns: new[] { "StatusId", "TimeLimitDays" },
                values: new object[] { 30, 14 });

            migrationBuilder.InsertData(
                table: "RelocationPlanStatusTimeLimit",
                columns: new[] { "StatusId", "TimeLimitDays" },
                values: new object[] { 31, 42 });

            migrationBuilder.InsertData(
                table: "RelocationPlanStatusTimeLimit",
                columns: new[] { "StatusId", "TimeLimitDays" },
                values: new object[] { 32, 70 });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseProgress_TrpState",
                table: "RelocationCaseProgress",
                column: "TrpState");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationCaseProgress_RelocationPlanTrpState_TrpState",
                table: "RelocationCaseProgress",
                column: "TrpState",
                principalTable: "RelocationPlanTrpState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationCaseProgress_RelocationPlanTrpState_TrpState",
                table: "RelocationCaseProgress");

            migrationBuilder.DropTable(
                name: "RelocationPlanTrpState");

            migrationBuilder.DropIndex(
                name: "IX_RelocationCaseProgress_TrpState",
                table: "RelocationCaseProgress");

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatusTimeLimit",
                keyColumn: "StatusId",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatusTimeLimit",
                keyColumn: "StatusId",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatusTimeLimit",
                keyColumn: "StatusId",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DropColumn(
                name: "TrpState",
                table: "RelocationCaseProgress");
        }
    }
}
