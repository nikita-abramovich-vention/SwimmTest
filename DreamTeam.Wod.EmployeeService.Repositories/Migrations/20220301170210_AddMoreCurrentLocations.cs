using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddMoreCurrentLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CurrentLocation",
                columns: new[] { "Id", "CreatedBy", "CreationDate", "ExternalId", "IsCustom", "Name" },
                values: new object[] { 23, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "tbilisi", false, "Tbilisi" });

            migrationBuilder.InsertData(
                table: "CurrentLocation",
                columns: new[] { "Id", "CreatedBy", "CreationDate", "ExternalId", "IsCustom", "Name" },
                values: new object[] { 24, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "batumi", false, "Batumi" });

            migrationBuilder.InsertData(
                table: "CurrentLocation",
                columns: new[] { "Id", "CreatedBy", "CreationDate", "ExternalId", "IsCustom", "Name" },
                values: new object[] { 25, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "nur-sultan", false, "Nur-Sultan" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 25);
        }
    }
}
