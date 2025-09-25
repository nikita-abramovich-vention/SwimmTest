using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class FixCountryForMarietta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 10,
                column: "CountryId",
                value: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 10,
                column: "CountryId",
                value: 5);
        }
    }
}
