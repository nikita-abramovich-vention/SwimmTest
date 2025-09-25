using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMentor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MentorId",
                table: "Employee",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_MentorId",
                table: "Employee",
                column: "MentorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Employee_MentorId",
                table: "Employee",
                column: "MentorId",
                principalTable: "Employee",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Employee_MentorId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Employee_MentorId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "MentorId",
                table: "Employee");
        }
    }
}
