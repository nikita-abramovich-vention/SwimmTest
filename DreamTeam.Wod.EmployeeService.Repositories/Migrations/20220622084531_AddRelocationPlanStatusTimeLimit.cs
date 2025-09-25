using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanStatusTimeLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StatusDueDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusStartDate",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "RelocationPlanStatusTimeLimit",
                columns: table => new
                {
                    StatusId = table.Column<int>(nullable: false),
                    TimeLimitDays = table.Column<int>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanStatusTimeLimit", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_RelocationPlanStatusTimeLimit_RelocationPlanStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "RelocationPlanStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanStatusTimeLimit",
                columns: new[] { "StatusId", "TimeLimitDays" },
                values: new object[,]
                {
                    { 1, 14 },
                    { 2, null },
                    { 3, 14 },
                    { 4, 7 },
                    { 5, 14 },
                    { 6, 14 },
                    { 7, 42 },
                    { 8, 14 },
                    { 9, 14 },
                    { 10, 14 },
                });

            migrationBuilder.Sql(@"
                INSERT RelocationCaseStatus
                SELECT *
                FROM (VALUES 
	                ('ready_for_employment', 'Ready for employment', 'Ready for employment', GETDATE())) c (ExternalId, SourceId, Name, CreationDate)
                WHERE NOT EXISTS (SELECT 1 FROM RelocationCaseStatus cs WHERE cs.SourceId = c.SourceId)");

            migrationBuilder.Sql(@"
               INSERT RelocationPlanStatus
               SELECT *
               FROM (VALUES 
	               ('ready_for_employment', 'Ready for employment', (SELECT Id FROM RelocationCaseStatus WHERE SourceId = 'Ready for employment'))) p (ExternalId, [Name], CaseStatusId)
               WHERE NOT EXISTS (SELECT 1 FROM RelocationPlanStatus ps WHERE ps.ExternalId = p.ExternalId)");

            migrationBuilder.Sql(@"
                INSERT RelocationPlanStatusTimeLimit
                SELECT *
                FROM (VALUES ((SELECT Id FROM RelocationPlanStatus WHERE ExternalId = 'ready_for_employment'), 14)) l (StatusId, TimeLimitDays)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelocationPlanStatusTimeLimit");

            migrationBuilder.DropColumn(
                name: "StatusDueDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "StatusStartDate",
                table: "RelocationPlan");
        }
    }
}
