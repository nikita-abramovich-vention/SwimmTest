using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddExternalIdToEmployeeUnitHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "EmployeeUnitHistory",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE EmployeeUnitHistory SET ExternalId = REPLACE(LOWER(NEWID()), '-', '')
            ");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUnitHistory_ExternalId",
                table: "EmployeeUnitHistory",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeUnitHistory_ExternalId",
                table: "EmployeeUnitHistory");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "EmployeeUnitHistory");
        }
    }
}
