using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddIsDomainNameVerified : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDomainNameVerified",
                table: "Internship",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Internship SET IsDomainNameVerified = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDomainNameVerified",
                table: "Internship");
        }
    }
}
