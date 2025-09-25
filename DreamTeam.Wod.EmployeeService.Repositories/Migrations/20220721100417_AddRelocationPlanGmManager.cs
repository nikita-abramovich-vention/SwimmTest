using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanGmManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GmManagerId",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_GmManagerId",
                table: "RelocationPlan",
                column: "GmManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_Employee_GmManagerId",
                table: "RelocationPlan",
                column: "GmManagerId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_Employee_GmManagerId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_GmManagerId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmManagerId",
                table: "RelocationPlan");
        }
    }
}
