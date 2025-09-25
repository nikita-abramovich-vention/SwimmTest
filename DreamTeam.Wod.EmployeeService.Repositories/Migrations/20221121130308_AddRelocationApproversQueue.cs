using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationApproversQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApproverOrderId",
                table: "RelocationApprover",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RelocationApproverAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelocationPlanId = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationApproverAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelocationApproverAssignment_Employee_ApproverId",
                        column: x => x.ApproverId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationApproverAssignment_RelocationPlan_RelocationPlanId",
                        column: x => x.RelocationPlanId,
                        principalTable: "RelocationPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelocationApproverOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsNext = table.Column<bool>(type: "bit", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationApproverOrder", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationApprover_ApproverOrderId",
                table: "RelocationApprover",
                column: "ApproverOrderId",
                unique: true,
                filter: "[ApproverOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationApproverAssignment_ApproverId",
                table: "RelocationApproverAssignment",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationApproverAssignment_RelocationPlanId",
                table: "RelocationApproverAssignment",
                column: "RelocationPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationApprover_RelocationApproverOrder_ApproverOrderId",
                table: "RelocationApprover",
                column: "ApproverOrderId",
                principalTable: "RelocationApproverOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationApprover_RelocationApproverOrder_ApproverOrderId",
                table: "RelocationApprover");

            migrationBuilder.DropTable(
                name: "RelocationApproverAssignment");

            migrationBuilder.DropTable(
                name: "RelocationApproverOrder");

            migrationBuilder.DropIndex(
                name: "IX_RelocationApprover_ApproverOrderId",
                table: "RelocationApprover");

            migrationBuilder.DropColumn(
                name: "ApproverOrderId",
                table: "RelocationApprover");
        }
    }
}
