using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanHrManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HrManagerComment",
                table: "RelocationPlan",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HrManagerCommentChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HrManagerId",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_HrManagerId",
                table: "RelocationPlan",
                column: "HrManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_Employee_HrManagerId",
                table: "RelocationPlan",
                column: "HrManagerId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_Employee_HrManagerId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_HrManagerId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "HrManagerComment",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "HrManagerCommentChangeDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "HrManagerId",
                table: "RelocationPlan");
        }
    }
}
