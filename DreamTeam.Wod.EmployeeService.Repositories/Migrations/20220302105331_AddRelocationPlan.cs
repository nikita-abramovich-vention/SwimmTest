using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisaState",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlan",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(nullable: false),
                    LocationId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    VisaState = table.Column<string>(nullable: false),
                    Comment = table.Column<string>(maxLength: 5000, nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlan", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_RelocationPlan_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelocationPlan_CurrentLocation_LocationId",
                        column: x => x.LocationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelocationPlan_VisaState_VisaState",
                        column: x => x.VisaState,
                        principalTable: "VisaState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "VisaState",
                column: "Id",
                value: "Acquired");

            migrationBuilder.InsertData(
                table: "VisaState",
                column: "Id",
                value: "InProgress");

            migrationBuilder.InsertData(
                table: "VisaState",
                column: "Id",
                value: "NotRequired");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_LocationId",
                table: "RelocationPlan",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_VisaState",
                table: "RelocationPlan",
                column: "VisaState");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelocationPlan");

            migrationBuilder.DropTable(
                name: "VisaState");
        }
    }
}
