using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RenameDeliveryOrganizationApprovers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DeliveryOrganizationApprover",
                newName: "RelocationApprover");

            migrationBuilder.RenameIndex(
                name: "PK_DeliveryOrganizationApprover",
                newName: "PK_RelocationApprover",
                table: "RelocationApprover");

            migrationBuilder.RenameIndex(
                name: "IX_DeliveryOrganizationApprover_CountryId",
                newName: "IX_RelocationApprover_CountryId",
                table: "RelocationApprover");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RelocationApprover",
                newName: "DeliveryOrganizationApprover");

            migrationBuilder.RenameIndex(
                name: "PK_RelocationApprover",
                newName: "PK_DeliveryOrganizationApprover",
                table: "DeliveryOrganizationApprover");

            migrationBuilder.RenameIndex(
                name: "IX_RelocationApprover_CountryId",
                newName: "IX_DeliveryOrganizationApprover_CountryId",
                table: "DeliveryOrganizationApprover");
        }
    }
}
