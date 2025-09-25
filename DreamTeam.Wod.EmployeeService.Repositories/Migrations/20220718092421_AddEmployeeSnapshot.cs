using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeSnapshot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    TitleRoleId = table.Column<int>(nullable: true),
                    CountryId = table.Column<string>(maxLength: 64, nullable: true),
                    OrganizationId = table.Column<string>(maxLength: 64, nullable: true),
                    UnitId = table.Column<string>(maxLength: 64, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSnapshot_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSnapshot_TitleRole_TitleRoleId",
                        column: x => x.TitleRoleId,
                        principalTable: "TitleRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_Date",
                table: "EmployeeSnapshot",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmployeeId",
                table: "EmployeeSnapshot",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_TitleRoleId",
                table: "EmployeeSnapshot",
                column: "TitleRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSnapshot");
        }
    }
}
