using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddUniqueInternshipDomainNameConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Internship_DomainName",
                table: "Internship");

            migrationBuilder.CreateIndex(
                name: "IX_Internship_DomainName",
                table: "Internship",
                column: "DomainName",
                unique: true,
                filter: "IsActive = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Internship_DomainName",
                table: "Internship");

            migrationBuilder.CreateIndex(
                name: "IX_Internship_DomainName",
                table: "Internship",
                column: "DomainName");
        }
    }
}
