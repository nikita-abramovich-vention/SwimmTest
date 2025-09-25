using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmplyeeCurrentLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentLocationId",
                table: "Employee",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurrentLocation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    IsCustom = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCurrentLocation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    ChangedBy = table.Column<string>(maxLength: 64, nullable: false),
                    ChangeDate = table.Column<DateTime>(nullable: false),
                    SinceDate = table.Column<DateTime>(nullable: false),
                    UntilDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCurrentLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeCurrentLocation_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeCurrentLocation_CurrentLocation_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CurrentLocation",
                columns: new[] { "Id", "CreatedBy", "CreationDate", "ExternalId", "IsCustom", "Name" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "minsk", false, "Minsk" },
                    { 20, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "vilnius", false, "Vilnius" },
                    { 19, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "tashkent", false, "Tashkent" },
                    { 18, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "warsaw", false, "Warsaw" },
                    { 17, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "wroclaw", false, "Wroclaw" },
                    { 16, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "lviv", false, "Lviv" },
                    { 15, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "kiev", false, "Kiev" },
                    { 14, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "vienna", false, "Vienna" },
                    { 13, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "lodz", false, "Lodz" },
                    { 12, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "london", false, "London" },
                    { 11, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "iselin", false, "Iselin" },
                    { 10, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "marietta", false, "Marietta" },
                    { 9, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "san francisco", false, "San Francisco" },
                    { 8, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "new york", false, "New York" },
                    { 7, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "polotsk", false, "Polotsk" },
                    { 6, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "grodno", false, "Grodno" },
                    { 5, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "gomel", false, "Gomel" },
                    { 4, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "brest", false, "Brest" },
                    { 3, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "vitebsk", false, "Vitebsk" },
                    { 2, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "mogilev", false, "Mogilev" },
                    { 21, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "krakow", false, "Krakow" },
                    { 22, null, new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "gdansk", false, "Gdansk" },
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_CurrentLocationId",
                table: "Employee",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentLocation_ExternalId",
                table: "CurrentLocation",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentLocation_IsCustom",
                table: "CurrentLocation",
                column: "IsCustom");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentLocation_Name",
                table: "CurrentLocation",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCurrentLocation_EmployeeId",
                table: "EmployeeCurrentLocation",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCurrentLocation_LocationId",
                table: "EmployeeCurrentLocation",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmployeeCurrentLocation_CurrentLocationId",
                table: "Employee",
                column: "CurrentLocationId",
                principalTable: "EmployeeCurrentLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmployeeCurrentLocation_CurrentLocationId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "EmployeeCurrentLocation");

            migrationBuilder.DropTable(
                name: "CurrentLocation");

            migrationBuilder.DropIndex(
                name: "IX_Employee_CurrentLocationId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CurrentLocationId",
                table: "Employee");
        }
    }
}
