using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddCloseReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloseReason",
                table: "Internship",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InternshipCloseReason",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternshipCloseReason", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InternshipCloseReason",
                column: "Id",
                value: "Manually");

            migrationBuilder.InsertData(
                table: "InternshipCloseReason",
                column: "Id",
                value: "AutomaticallyDueInactivity");

            migrationBuilder.InsertData(
                table: "InternshipCloseReason",
                column: "Id",
                value: "AutomaticallyDueEmployment");

            migrationBuilder.CreateIndex(
                name: "IX_Internship_CloseReason",
                table: "Internship",
                column: "CloseReason");

            migrationBuilder.AddForeignKey(
                name: "FK_Internship_InternshipCloseReason_CloseReason",
                table: "Internship",
                column: "CloseReason",
                principalTable: "InternshipCloseReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"UPDATE Internship SET CloseReason = 'Manually' WHERE IsActive = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Internship_InternshipCloseReason_CloseReason",
                table: "Internship");

            migrationBuilder.DropTable(
                name: "InternshipCloseReason");

            migrationBuilder.DropIndex(
                name: "IX_Internship_CloseReason",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "CloseReason",
                table: "Internship");
        }
    }
}
