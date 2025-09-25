using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddIsEmmploymentConfirmedToHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NewIsEmploymentConfirmedByEmployee",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousIsEmploymentConfirmedByEmployee",
                table: "RelocationPlanChange",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewIsEmploymentConfirmedByEmployee",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousIsEmploymentConfirmedByEmployee",
                table: "RelocationPlanChange");
        }
    }
}
