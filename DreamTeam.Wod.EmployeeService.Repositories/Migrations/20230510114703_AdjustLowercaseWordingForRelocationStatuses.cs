using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AdjustLowercaseWordingForRelocationStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Employee confirmation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Pending approval");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Relocation approved");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "In progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Visa docs preparation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Embassy appointment");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Visa in progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Visa done");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 30,
                column: "Name",
                value: "TRP docs preparation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 31,
                column: "Name",
                value: "TRP application submission");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 32,
                column: "Name",
                value: "TRP in progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "ExternalId",
                keyValue: "on_hold",
                column: "Name",
                value: "On hold");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Employee Confirmation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Pending Approval");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Relocation Approved");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "In Progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Visa Docs Preparation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Embassy Appointment");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 9,
                column: "Name",
                value: "Visa in Progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 10,
                column: "Name",
                value: "Visa Done");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 30,
                column: "Name",
                value: "TRP Docs Preparation");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 31,
                column: "Name",
                value: "TRP Application Submission");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "Id",
                keyValue: 32,
                column: "Name",
                value: "TRP In Progress");

            migrationBuilder.UpdateData(
                table: "RelocationPlanStatus",
                keyColumn: "ExternalId",
                keyValue: "on_hold",
                column: "Name",
                value: "On Hold");
        }
    }
}
