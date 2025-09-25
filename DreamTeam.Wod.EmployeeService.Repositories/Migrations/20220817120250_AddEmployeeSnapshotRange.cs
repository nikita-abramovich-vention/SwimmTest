using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeSnapshotRange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_Date",
                table: "EmployeeSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "EmployeeSnapshot",
                newName: "FromDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ToDate",
                table: "EmployeeSnapshot",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot",
                column: "UnitId")
                .Annotation("SqlServer:Include", new[] { "FromDate", "ToDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_FromDate_ToDate",
                table: "EmployeeSnapshot",
                columns: new[] { "FromDate", "ToDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_FromDate_ToDate",
                table: "EmployeeSnapshot");

            migrationBuilder.DropColumn(
                name: "ToDate",
                table: "EmployeeSnapshot");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "EmployeeSnapshot",
                newName: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_Date",
                table: "EmployeeSnapshot",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_UnitId",
                table: "EmployeeSnapshot",
                column: "UnitId")
                .Annotation("SqlServer:Include", new[] { "Date", "IsActive" });
        }
    }
}
