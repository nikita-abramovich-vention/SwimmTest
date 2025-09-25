using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "Status");

            migrationBuilder.UpdateData("RelocationPlanChange", "Type", "CaseStatus", "Type", "Status");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "CaseStatus");

            migrationBuilder.CreateTable(
                name: "RelocationPlanStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    CaseStatusId = table.Column<int>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelocationPlanStatus_RelocationCaseStatus_CaseStatusId",
                        column: x => x.CaseStatusId,
                        principalTable: "RelocationCaseStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
INSERT RelocationCaseStatus
SELECT *
FROM (VALUES 
	('new', 'New', 'New', GETDATE()),
	('in_progress', 'In Progress', 'In Progress', GETDATE())
) c (ExternalId, SourceId, Name, CreationDate)
WHERE NOT EXISTS (SELECT 1 FROM RelocationCaseStatus cs WHERE cs.SourceId = c.SourceId)");

            migrationBuilder.InsertData(
                table: "RelocationPlanStatus",
                columns: new[] { "Id", "CaseStatusId", "ExternalId", "Name" },
                values: new object[,]
                {
                    { 1, null, "induction", "Induction" },
                    { 2, null, "employee_confirmation", "Employee Confirmation" },
                    { 3, null, "pending_approval", "Pending Approval" },
                    { 4, null, "relocation_approved", "Relocation Approved" },
                    { 5, null, "in_progress", "In Progress" },
                    { 6, null, "visa_docs_preparation", "Visa Docs Preparation" },
                    { 7, null, "waiting_for_embassy_appointment", "Waiting for embassy appointment" },
                    { 8, null, "embassy_appointment", "Embassy Appointment" },
                    { 9, null, "visa_in_progress", "Visa in Progress" },
                    { 10, null, "visa_done", "Visa Done" },
                });

            migrationBuilder.Sql(@"
UPDATE RelocationPlanStatus
SET CaseStatusId = (SELECT Id FROM RelocationCaseStatus WHERE SourceId = 'New')
WHERE ExternalId = 'relocation_approved'

UPDATE RelocationPlanStatus
SET CaseStatusId = (SELECT Id FROM RelocationCaseStatus WHERE SourceId = 'In Progress')
WHERE ExternalId = 'in_progress'");

            migrationBuilder.Sql(@"
INSERT RelocationPlanStatus
SELECT LOWER(REPLACE(cs.SourceId, ' ', '_')), cs.Name, cs.Id
FROM RelocationCaseStatus cs
WHERE Id NOT IN (SELECT CaseStatusId FROM RelocationPlanStatus WHERE CaseStatusId IS NOT NULL)");

            migrationBuilder.AddColumn<int>(
                name: "NewStatusId",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatusId",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "AreDocsSentToAgency",
                table: "RelocationCaseVisaProgress",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewStatusId",
                table: "RelocationPlanChange",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousStatusId",
                table: "RelocationPlanChange",
                column: "PreviousStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_StatusId",
                table: "RelocationPlan",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanStatus_CaseStatusId",
                table: "RelocationPlanStatus",
                column: "CaseStatusId",
                unique: true,
                filter: "[CaseStatusId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanStatus_ExternalId",
                table: "RelocationPlanStatus",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_RelocationPlanStatus_StatusId",
                table: "RelocationPlan",
                column: "StatusId",
                principalTable: "RelocationPlanStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationPlanStatus_NewStatusId",
                table: "RelocationPlanChange",
                column: "NewStatusId",
                principalTable: "RelocationPlanStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationPlanStatus_PreviousStatusId",
                table: "RelocationPlanChange",
                column: "PreviousStatusId",
                principalTable: "RelocationPlanStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_RelocationPlanStatus_StatusId",
                table: "RelocationPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationPlanStatus_NewStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationPlanStatus_PreviousStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropTable(
                name: "RelocationPlanStatus");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_NewStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_PreviousStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_StatusId",
                table: "RelocationPlan");

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "CaseStatus");

            migrationBuilder.UpdateData("RelocationPlanChange", "Type", "Status", "Type", "CaseStatus");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "Status");

            migrationBuilder.DropColumn(
                name: "NewStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "AreDocsSentToAgency",
                table: "RelocationCaseVisaProgress");
        }
    }
}
