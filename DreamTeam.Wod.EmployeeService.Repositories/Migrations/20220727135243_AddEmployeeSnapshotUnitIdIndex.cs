using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeSnapshotUnitIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot",
                column: "UnitId")
                .Annotation("SqlServer:Include", new[] { "Date", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot");
        }
    }
}
