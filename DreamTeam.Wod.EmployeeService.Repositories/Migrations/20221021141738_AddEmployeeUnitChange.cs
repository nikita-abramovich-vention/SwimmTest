using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeUnitChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeUnitChange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PreviousUnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    NewUnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeUnitChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeUnitChange_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUnitChange_EmployeeId",
                table: "EmployeeUnitChange",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeUnitChange");
        }
    }
}
