using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RemoveDepartmentAndGroupId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Employee");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Internship",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Internship",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentId",
                table: "Employee",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "Employee",
                maxLength: 64,
                nullable: true);
        }
    }
}