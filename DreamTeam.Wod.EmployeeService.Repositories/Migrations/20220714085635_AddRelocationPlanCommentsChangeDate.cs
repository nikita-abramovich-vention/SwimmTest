using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanCommentsChangeDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApproverCommentChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmployeeCommentChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GmCommentChangeDate",
                table: "RelocationPlan",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverCommentChangeDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "EmployeeCommentChangeDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmCommentChangeDate",
                table: "RelocationPlan");
        }
    }
}
