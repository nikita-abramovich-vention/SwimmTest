using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddDismissalReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DismissalReason",
                table: "Employee",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DismissalReason",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissalReason", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DismissalReason",
                column: "Id",
                values: new object[]
                {
                    "Dismissed",
                    "MaternityLeave",
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DismissalReason",
                table: "Employee",
                column: "DismissalReason");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_DismissalReason_DismissalReason",
                table: "Employee",
                column: "DismissalReason",
                principalTable: "DismissalReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE [dbo].[Employee]
                SET [DismissalReason] = 'Dismissed'
                WHERE IsActive = 0
                      AND DismissalReason is Null
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_DismissalReason_DismissalReason",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "DismissalReason");

            migrationBuilder.DropIndex(
                name: "IX_Employee_DismissalReason",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "DismissalReason",
                table: "Employee");
        }
    }
}
