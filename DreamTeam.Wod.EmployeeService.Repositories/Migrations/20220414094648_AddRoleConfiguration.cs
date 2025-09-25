using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRoleConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleConfiguration_Role_Id",
                        column: x => x.Id,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleConfigurationEmployee",
                columns: table => new
                {
                    RoleConfigurationId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleConfigurationEmployee", x => new { x.RoleConfigurationId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_RoleConfigurationEmployee_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleConfigurationEmployee_RoleConfiguration_RoleConfigurationId",
                        column: x => x.RoleConfigurationId,
                        principalTable: "RoleConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleConfigurationTitleRole",
                columns: table => new
                {
                    RoleConfigurationId = table.Column<int>(nullable: false),
                    TitleRoleId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleConfigurationTitleRole", x => new { x.RoleConfigurationId, x.TitleRoleId });
                    table.ForeignKey(
                        name: "FK_RoleConfigurationTitleRole_RoleConfiguration_RoleConfigurationId",
                        column: x => x.RoleConfigurationId,
                        principalTable: "RoleConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleConfigurationTitleRole_TitleRole_TitleRoleId",
                        column: x => x.TitleRoleId,
                        principalTable: "TitleRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleConfigurationUnit",
                columns: table => new
                {
                    RoleConfigurationId = table.Column<int>(nullable: false),
                    UnitId = table.Column<string>(maxLength: 64, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleConfigurationUnit", x => new { x.RoleConfigurationId, x.UnitId });
                    table.ForeignKey(
                        name: "FK_RoleConfigurationUnit_RoleConfiguration_RoleConfigurationId",
                        column: x => x.RoleConfigurationId,
                        principalTable: "RoleConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleConfigurationEmployee_EmployeeId",
                table: "RoleConfigurationEmployee",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleConfigurationTitleRole_TitleRoleId",
                table: "RoleConfigurationTitleRole",
                column: "TitleRoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleConfigurationEmployee");

            migrationBuilder.DropTable(
                name: "RoleConfigurationTitleRole");

            migrationBuilder.DropTable(
                name: "RoleConfigurationUnit");

            migrationBuilder.DropTable(
                name: "RoleConfiguration");
        }
    }
}
