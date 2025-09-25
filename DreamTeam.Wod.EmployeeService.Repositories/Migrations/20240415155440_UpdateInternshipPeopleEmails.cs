using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class UpdateInternshipPeopleEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE p
                                   SET p.Email = i.Email
                                   FROM WodProfiles.dbo.Person p
                                   JOIN WodEmployees.dbo.Internship i ON p.Externalid = i.PersonId
                                   WHERE p.Email IS NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
