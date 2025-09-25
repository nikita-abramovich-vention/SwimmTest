using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeConfirmationTimeLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RelocationPlanStatusTimeLimit",
                keyColumn: "StatusId",
                keyValue: 2,
                column: "TimeLimitDays",
                value: 30);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RelocationPlanStatusTimeLimit",
                keyColumn: "StatusId",
                keyValue: 2,
                column: "TimeLimitDays",
                value: null);
        }
    }
}
