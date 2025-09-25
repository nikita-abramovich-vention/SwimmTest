using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class MakeSourceDismissalRequestNotRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DismissalRequest_SourceDismissalRequestId",
                table: "DismissalRequest");

            migrationBuilder.AlterColumn<int>(
                name: "SourceDismissalRequestId",
                table: "DismissalRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_SourceDismissalRequestId",
                table: "DismissalRequest",
                column: "SourceDismissalRequestId",
                unique: true,
                filter: "[SourceDismissalRequestId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DismissalRequest_SourceDismissalRequestId",
                table: "DismissalRequest");

            migrationBuilder.AlterColumn<int>(
                name: "SourceDismissalRequestId",
                table: "DismissalRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_SourceDismissalRequestId",
                table: "DismissalRequest",
                column: "SourceDismissalRequestId",
                unique: true);
        }
    }
}
