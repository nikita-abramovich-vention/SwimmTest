using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddIdToRelocationApprover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RelocationApprover",
                table: "RelocationApprover");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RelocationApprover",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelocationApprover",
                table: "RelocationApprover",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationApprover_EmployeeId_CountryId",
                table: "RelocationApprover",
                columns: new[] { "EmployeeId", "CountryId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RelocationApprover",
                table: "RelocationApprover");

            migrationBuilder.DropIndex(
                name: "IX_RelocationApprover_EmployeeId_CountryId",
                table: "RelocationApprover");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RelocationApprover");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelocationApprover",
                table: "RelocationApprover",
                columns: new[] { "EmployeeId", "CountryId" });
        }
    }
}
