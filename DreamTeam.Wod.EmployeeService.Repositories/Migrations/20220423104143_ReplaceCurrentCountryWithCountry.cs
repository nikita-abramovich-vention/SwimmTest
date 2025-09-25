using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class ReplaceCurrentCountryWithCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentLocation_CurrentCountry_CountryId",
                table: "CurrentLocation");

            migrationBuilder.DropIndex(
                name: "IX_CurrentLocation_CountryId",
                table: "CurrentLocation");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "CurrentLocation",
                newName: "CountryIdObsolete");

            migrationBuilder.AddColumn<string>(
                name: "CountryId",
                table: "CurrentLocation",
                maxLength: 64,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "CurrentLocation");

            migrationBuilder.RenameColumn(
                name: "CountryIdObsolete",
                table: "CurrentLocation",
                newName: "CountryId");

            migrationBuilder.AlterColumn<int>(
                name: "CountryId",
                table: "CurrentLocation",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentLocation_CountryId",
                table: "CurrentLocation",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentLocation_CurrentCountry_CountryId",
                table: "CurrentLocation",
                column: "CountryId",
                principalTable: "CurrentCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
