using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddDeliveryOrganizationApprovers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryOrganizationApprover",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(nullable: false),
                    CountryId = table.Column<string>(nullable: false),
                    IsPrimary = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrganizationApprover", x => new { x.EmployeeId, x.CountryId });
                    table.ForeignKey(
                        name: "FK_DeliveryOrganizationApprover_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrganizationApprover_CountryId",
                table: "DeliveryOrganizationApprover",
                column: "CountryId",
                unique: true,
                filter: "[IsPrimary] = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryOrganizationApprover");
        }
    }
}
