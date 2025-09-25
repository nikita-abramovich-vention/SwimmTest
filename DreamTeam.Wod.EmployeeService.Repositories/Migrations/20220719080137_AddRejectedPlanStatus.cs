using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRejectedPlanStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @RelocationCaseStatusExternalId NVARCHAR(36) = LOWER(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''))
                INSERT INTO RelocationCaseStatus (ExternalId, SourceId, Name, CreationDate)
                VALUES (@RelocationCaseStatusExternalId, 'Rejected', 'Rejected', GETUTCDATE())

                DECLARE @CaseStatusId INT = (SELECT TOP 1 Id FROM RelocationCaseStatus WHERE SourceId = 'Rejected')
                INSERT INTO RelocationPlanStatus (ExternalId, Name, CaseStatusId)
                VALUES ('rejected', 'Rejected', @CaseStatusId)

                DECLARE @PlanStatusId INT = (SELECT Id FROM RelocationPlanStatus WHERE ExternalId = 'rejected')
                UPDATE RelocationPlan
                SET StatusId = @PlanStatusId
                WHERE State = 'Rejected'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
