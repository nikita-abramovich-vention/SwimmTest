using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddCaseStatusChangeHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewCaseStatusId",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousCaseStatusId",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "CaseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewCaseStatusId",
                table: "RelocationPlanChange",
                column: "NewCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousCaseStatusId",
                table: "RelocationPlanChange",
                column: "PreviousCaseStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_NewCaseStatusId",
                table: "RelocationPlanChange",
                column: "NewCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_PreviousCaseStatusId",
                table: "RelocationPlanChange",
                column: "PreviousCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_PreviousCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_PreviousCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "CaseStatus");

            migrationBuilder.DropColumn(
                name: "NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousCaseStatusId",
                table: "RelocationPlanChange");
        }
    }
}
