using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddSupportsRelocationToCurrentCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SupportsRelocation",
                table: "CurrentCountry",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 4,
                column: "SupportsRelocation",
                value: true);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 5,
                column: "SupportsRelocation",
                value: true);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 7,
                column: "SupportsRelocation",
                value: true);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 8,
                column: "SupportsRelocation",
                value: true);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 9,
                column: "SupportsRelocation",
                value: true);

            migrationBuilder.UpdateData(
                table: "CurrentCountry",
                keyColumn: "Id",
                keyValue: 10,
                column: "SupportsRelocation",
                value: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportsRelocation",
                table: "CurrentCountry");
        }
    }
}
