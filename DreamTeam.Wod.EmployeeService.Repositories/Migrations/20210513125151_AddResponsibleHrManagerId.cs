using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddResponsibleHrManagerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsibleHrManagerId",
                table: "Employee",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ResponsibleHrManagerId",
                table: "Employee",
                column: "ResponsibleHrManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Employee_ResponsibleHrManagerId",
                table: "Employee",
                column: "ResponsibleHrManagerId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Employee_ResponsibleHrManagerId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_ResponsibleHrManagerId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "ResponsibleHrManagerId",
                table: "Employee");
        }
    }
}
