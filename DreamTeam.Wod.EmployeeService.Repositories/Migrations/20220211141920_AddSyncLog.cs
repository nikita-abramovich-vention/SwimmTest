using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddSyncLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StudentLabId",
                table: "Internship",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentLabProfileUrl",
                table: "Internship",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SyncLog",
                columns: table => new
                {
                    SyncLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SyncStartDate = table.Column<DateTime>(nullable: false),
                    SyncCompletedDate = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    IsOutdated = table.Column<bool>(nullable: false),
                    AffectedInternshipsCount = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLog", x => x.SyncLogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncLog");

            migrationBuilder.DropColumn(
                name: "StudentLabId",
                table: "Internship");

            migrationBuilder.DropColumn(
                name: "StudentLabProfileUrl",
                table: "Internship");
        }
    }
}
