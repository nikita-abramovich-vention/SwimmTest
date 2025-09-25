using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationEmploymentConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmploymentConfirmedByEmployee",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "EmploymentConfirmedByEmployee");

            migrationBuilder.Sql(@"
                INSERT INTO RelocationPlanStatus (ExternalId, Name, CaseStatusId)
                VALUES ('employment_confirmation_by_employee', 'Employment confirmation', NULL)
            ");

            migrationBuilder.Sql(@"
                DECLARE @CaseStatusId INT = (SELECT TOP 1 CaseStatusId FROM RelocationPlanStatus WHERE ExternalId = 'ready_for_employment')
                UPDATE RelocationPlanStatus
                SET
                    CaseStatusId = NULL
                WHERE ExternalId = 'ready_for_employment'

                UPDATE RelocationPlanStatus
                SET
                    CaseStatusId = @CaseStatusId
                WHERE ExternalId = 'employment_confirmation_by_employee'
            ");

            migrationBuilder.Sql(@"     
                INSERT INTO RelocationPlanStatusTimeLimit (StatusId, TimeLimitDays)
                VALUES ((SELECT Id FROM RelocationPlanStatus WHERE ExternalId = 'employment_confirmation_by_employee'), NULL)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "EmploymentConfirmedByEmployee");

            migrationBuilder.DropColumn(
                name: "IsEmploymentConfirmedByEmployee",
                table: "RelocationPlan");
        }
    }
}
