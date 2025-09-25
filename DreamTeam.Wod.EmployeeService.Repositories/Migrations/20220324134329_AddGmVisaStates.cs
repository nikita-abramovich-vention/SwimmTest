using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddGmVisaStates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GmVisaState",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GmVisaStateChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GmVisaStateChangedBy",
                table: "RelocationPlan",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GmVisaState",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GmVisaState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GmVisaState",
                column: "Id",
                values: new object[]
                {
                    "Pending",
                    "Agency",
                    "EmbassyAppointment",
                    "VisaInProgress",
                    "VisaDone",
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_GmVisaState",
                table: "RelocationPlan",
                column: "GmVisaState");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_GmVisaState_GmVisaState",
                table: "RelocationPlan",
                column: "GmVisaState",
                principalTable: "GmVisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"UPDATE RelocationPlan SET GmVisaState = 'Pending' WHERE VisaState != 'NotRequired'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_GmVisaState_GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropTable(
                name: "GmVisaState");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmVisaStateChangeDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmVisaStateChangedBy",
                table: "RelocationPlan");
        }
    }
}
