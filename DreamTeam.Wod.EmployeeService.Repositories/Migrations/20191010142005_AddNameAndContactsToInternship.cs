using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddNameAndContactsToInternship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Internship",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Internship",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Internship",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Internship",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoId",
                table: "Internship",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skype",
                table: "Internship",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "Skype",
                table: "Internship");
        }
    }
}