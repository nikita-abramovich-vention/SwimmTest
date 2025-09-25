using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class EmbassyAppointmentIncreaseLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE RelocationPlanStatusTimeLimit
                SET TimeLimitDays = 42
                FROM [WodEmployees].[dbo].[RelocationPlanStatusTimeLimit] RelocationPlanStatusTimeLimit
                JOIN [WodEmployees].[dbo].[RelocationPlanStatus] RelocationPlanStatus
                    ON RelocationPlanStatusTimeLimit.StatusId = RelocationPlanStatus.Id
                WHERE RelocationPlanStatus.ExternalId = 'embassy_appointment'
            ");

            migrationBuilder.Sql(@"
                UPDATE RelocationPlan
                SET StatusDueDate = DATEADD(day, 42, StatusStartDate)
                FROM [WodEmployees].[dbo].[RelocationPlan] RelocationPlan
                JOIN [WodEmployees].[dbo].[RelocationPlanStatus] RelocationPlanStatus
                    ON RelocationPlan.StatusId = RelocationPlanStatus.Id
                WHERE RelocationPlanStatus.ExternalId = 'embassy_appointment'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}