using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class FixCompletedAndCancelledPlanStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM RelocationCaseStatus WHERE SourceId = 'Canceled')
                BEGIN
                    DECLARE @RelocationCaseStatusExternalId
                    NVARCHAR(36) = LOWER(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''))
                    INSERT INTO RelocationCaseStatus(ExternalId, SourceId, Name, CreationDate)
                    VALUES (@RelocationCaseStatusExternalId, 'Canceled', 'Canceled', GETUTCDATE())
                END

                IF NOT EXISTS(SELECT * FROM RelocationPlanStatus WHERE ExternalId = 'canceled')
                BEGIN
                    DECLARE @CaseStatusId INT = (SELECT TOP 1 Id FROM RelocationCaseStatus WHERE SourceId = 'Canceled')
                    INSERT INTO RelocationPlanStatus (ExternalId, Name, CaseStatusId)
                    VALUES ('canceled', 'Canceled', @CaseStatusId)
                END

                DECLARE @PlanStatusId INT = (SELECT Id FROM RelocationPlanStatus WHERE ExternalId = 'canceled')
                UPDATE RelocationPlan
                SET StatusId = @PlanStatusId
                WHERE State = 'Cancelled'
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS(SELECT * FROM RelocationCaseStatus WHERE SourceId = 'Completed')
                BEGIN
                    DECLARE @RelocationCaseStatusExternalId
                    NVARCHAR(36) = LOWER(REPLACE(CONVERT(NVARCHAR(36), NEWID()), '-', ''))
                    INSERT INTO RelocationCaseStatus(ExternalId, SourceId, Name, CreationDate)
                    VALUES (@RelocationCaseStatusExternalId, 'Completed', 'Completed', GETUTCDATE())
                END

                IF NOT EXISTS(SELECT * FROM RelocationPlanStatus WHERE ExternalId = 'completed')
                BEGIN
                    DECLARE @CaseStatusId INT = (SELECT TOP 1 Id FROM RelocationCaseStatus WHERE SourceId = 'Completed')
                    INSERT INTO RelocationPlanStatus (ExternalId, Name, CaseStatusId)
                    VALUES ('completed', 'Completed', @CaseStatusId)
                END

                DECLARE @PlanStatusId INT = (SELECT Id FROM RelocationPlanStatus WHERE ExternalId = 'completed')
                UPDATE RelocationPlan
                SET StatusId = @PlanStatusId
                WHERE State = 'Cancelled'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
