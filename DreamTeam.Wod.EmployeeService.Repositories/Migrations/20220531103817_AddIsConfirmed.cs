using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddIsConfirmed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NewIsConfirmed",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousIsConfirmed",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE RelocationPlan
                SET IsConfirmed = 1,
                    ConfirmationDate = ApprovalDate
                WHERE IsApproved = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewIsConfirmed",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousIsConfirmed",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "ConfirmationDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "RelocationPlan");
        }
    }
}
