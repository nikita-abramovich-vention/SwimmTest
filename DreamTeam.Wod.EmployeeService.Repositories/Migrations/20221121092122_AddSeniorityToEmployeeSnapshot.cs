using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddSeniorityToEmployeeSnapshot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeniorityId",
                table: "EmployeeSnapshot",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_SeniorityId",
                table: "EmployeeSnapshot",
                column: "SeniorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSnapshot_Seniority_SeniorityId",
                table: "EmployeeSnapshot",
                column: "SeniorityId",
                principalTable: "Seniority",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
                UPDATE s
                SET s.SeniorityId = e.SeniorityId
                FROM EmployeeSnapshot s
                INNER JOIN Employee e ON e.Id = s.EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSnapshot_Seniority_SeniorityId",
                table: "EmployeeSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_SeniorityId",
                table: "EmployeeSnapshot");

            migrationBuilder.DropColumn(
                name: "SeniorityId",
                table: "EmployeeSnapshot");
        }
    }
}
