using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddIsDismissed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_DismissalReason_DismissalReason",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "DismissalReason");

            migrationBuilder.RenameColumn(
                name: "DismissalReason",
                table: "Employee",
                newName: "DeactivationReason");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DismissalReason",
                table: "Employee",
                newName: "IX_Employee_DeactivationReason");

            migrationBuilder.AddColumn<bool>(
                name: "IsDismissed",
                table: "Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeactivationReason",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeactivationReason", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeactivationReason",
                column: "Id",
                value: "Dismissed");

            migrationBuilder.InsertData(
                table: "DeactivationReason",
                column: "Id",
                value: "MaternityLeave");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_DeactivationReason_DeactivationReason",
                table: "Employee",
                column: "DeactivationReason",
                principalTable: "DeactivationReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_DeactivationReason_DeactivationReason",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "DeactivationReason");

            migrationBuilder.DropColumn(
                name: "IsDismissed",
                table: "Employee");

            migrationBuilder.RenameColumn(
                name: "DeactivationReason",
                table: "Employee",
                newName: "DismissalReason");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DeactivationReason",
                table: "Employee",
                newName: "IX_Employee_DismissalReason");

            migrationBuilder.CreateTable(
                name: "DismissalReason",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissalReason", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DismissalReason",
                column: "Id",
                value: "Dismissed");

            migrationBuilder.InsertData(
                table: "DismissalReason",
                column: "Id",
                value: "MaternityLeave");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_DismissalReason_DismissalReason",
                table: "Employee",
                column: "DismissalReason",
                principalTable: "DismissalReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
