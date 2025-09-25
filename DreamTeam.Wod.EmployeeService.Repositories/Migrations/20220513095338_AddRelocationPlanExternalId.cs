using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanExternalId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE RelocationPlan SET ExternalId = LOWER(REPLACE(CAST(NewId() AS VARCHAR(36)), '-', ''))");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_ExternalId",
                table: "RelocationPlan",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_ExternalId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "RelocationPlan");
        }
    }
}