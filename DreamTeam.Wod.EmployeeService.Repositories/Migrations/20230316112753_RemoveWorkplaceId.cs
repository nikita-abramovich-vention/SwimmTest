using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RemoveWorkplaceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "ExternalEmploymentRequest");

            migrationBuilder.DropColumn(
                name: "WorkplaceId",
                table: "EmploymentRequest");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "ExternalEmploymentRequest",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.Sql(@"DELETE FROM ExternalEmploymentRequest WHERE Id NOT IN (SELECT SourceId FROM EmploymentRequest)");

            migrationBuilder.Sql(@"UPDATE ExternalEmploymentRequest SET ExternalEmploymentRequest.OrganizationId = (SELECT TOP 1 OrganizationId FROM EmploymentRequest WHERE ExternalEmploymentRequest.Id = EmploymentRequest.SourceId)");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "EmploymentRequest",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ExternalEmploymentRequest");

            migrationBuilder.AddColumn<int>(
                name: "WorkplaceId",
                table: "ExternalEmploymentRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "EmploymentRequest",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "WorkplaceId",
                table: "EmploymentRequest",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
