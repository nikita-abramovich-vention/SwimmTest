using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddCommentToEmployeePreferredDeliveryCenters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePreferredDeliveryCenter_Employee_EmployeeId",
                table: "EmployeePreferredDeliveryCenter");

            migrationBuilder.CreateTable(
                name: "EmployeePreferredDeliveryCenters",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(nullable: false),
                    Comment = table.Column<string>(maxLength: 5000, nullable: true),
                    ChangedBy = table.Column<string>(maxLength: 64, nullable: true),
                    ChangeDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePreferredDeliveryCenters", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_EmployeePreferredDeliveryCenters_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
INSERT INTO EmployeePreferredDeliveryCenters(EmployeeId, Comment, ChangedBy, ChangeDate)
SELECT EmployeeId, '', Employee.PersonId, GETUTCDATE()
FROM EmployeePreferredDeliveryCenter c
JOIN Employee ON Employee.Id = c.EmployeeId
GROUP BY EmployeeId, Employee.PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePreferredDeliveryCenter_EmployeePreferredDeliveryCenters_EmployeeId",
                table: "EmployeePreferredDeliveryCenter",
                column: "EmployeeId",
                principalTable: "EmployeePreferredDeliveryCenters",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePreferredDeliveryCenter_EmployeePreferredDeliveryCenters_EmployeeId",
                table: "EmployeePreferredDeliveryCenter");

            migrationBuilder.DropTable(
                name: "EmployeePreferredDeliveryCenters");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePreferredDeliveryCenter_Employee_EmployeeId",
                table: "EmployeePreferredDeliveryCenter",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
