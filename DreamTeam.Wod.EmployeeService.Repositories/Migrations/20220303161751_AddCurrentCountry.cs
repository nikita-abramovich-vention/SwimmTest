using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddCurrentCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "CurrentLocation",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurrentCountry",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentCountry", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CurrentCountry",
                columns: new[] { "Id", "ExternalId", "Name" },
                values: new object[,]
                {
                    { 1, "uk", "UK" },
                    { 2, "us", "US" },
                    { 3, "austria", "Austria" },
                    { 4, "belarus", "Belarus" },
                    { 5, "georgia", "Georgia" },
                    { 6, "kazakhstan", "Kazakhstan" },
                    { 7, "lithuania", "Lithuania" },
                    { 8, "poland", "Poland" },
                    { 9, "ukraine", "Ukraine" },
                    { 10, "uzbekistan", "Uzbekistan" },
                });

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 1,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 2,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 3,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 4,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 5,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 6,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 7,
                column: "CountryId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 8,
                column: "CountryId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 9,
                column: "CountryId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 10,
                column: "CountryId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 11,
                column: "CountryId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 12,
                column: "CountryId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 13,
                column: "CountryId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 14,
                column: "CountryId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 15,
                column: "CountryId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 16,
                column: "CountryId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 17,
                column: "CountryId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 18,
                column: "CountryId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 19,
                column: "CountryId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 20,
                column: "CountryId",
                value: 7);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 21,
                column: "CountryId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 22,
                column: "CountryId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 23,
                column: "CountryId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 24,
                column: "CountryId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "CurrentLocation",
                keyColumn: "Id",
                keyValue: 25,
                column: "CountryId",
                value: 6);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentLocation_CountryId",
                table: "CurrentLocation",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentCountry_ExternalId",
                table: "CurrentCountry",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentCountry_Name",
                table: "CurrentCountry",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentLocation_CurrentCountry_CountryId",
                table: "CurrentLocation",
                column: "CountryId",
                principalTable: "CurrentCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentLocation_CurrentCountry_CountryId",
                table: "CurrentLocation");

            migrationBuilder.DropTable(
                name: "CurrentCountry");

            migrationBuilder.DropIndex(
                name: "IX_CurrentLocation_CountryId",
                table: "CurrentLocation");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "CurrentLocation");
        }
    }
}
