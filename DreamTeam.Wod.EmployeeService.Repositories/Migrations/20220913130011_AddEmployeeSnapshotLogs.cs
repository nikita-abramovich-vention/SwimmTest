using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeSnapshotLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_EmployeeId",
                table: "EmployeeSnapshot");

            migrationBuilder.CreateTable(
                name: "EmployeeSnapshotLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSnapshotLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmployeeId_FromDate",
                table: "EmployeeSnapshot",
                columns: new[] { "EmployeeId", "FromDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshotLog_Date",
                table: "EmployeeSnapshotLog",
                column: "Date");

            migrationBuilder.Sql(@"
                INSERT INTO EmployeeSnapshotLog (Date, IsSuccessful)
                SELECT GETUTCDATE(), 1
                WHERE (SELECT COUNT(*) FROM EmployeeSnapshot WHERE ToDate >= CAST(GETUTCDATE() AS Date)) > 0
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSnapshotLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_EmployeeId_FromDate",
                table: "EmployeeSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmployeeId",
                table: "EmployeeSnapshot",
                column: "EmployeeId");
        }
    }
}
