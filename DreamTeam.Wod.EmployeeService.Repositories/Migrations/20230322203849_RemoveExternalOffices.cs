using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RemoveExternalOffices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ExternalEmployeeWorkplace");
            migrationBuilder.Sql(@"DELETE FROM EmployeeWorkplace");
            migrationBuilder.Sql(@"DELETE FROM Workplace");
            migrationBuilder.Sql(@"DELETE FROM ExternalWorkplace");

            migrationBuilder.Sql(@"UPDATE SyncLog SET IsOutdated = 1
                                   WHERE Id=
                                   (SELECT TOP 1 Id FROM SyncLog
                                   WHERE Type = 'DownloadExternalWspData'
                                   ORDER BY SyncCompletedDate DESC)");

            migrationBuilder.Sql(@"UPDATE SyncLog SET IsOutdated = 1
                                   WHERE Id=
                                   (SELECT TOP 1 Id FROM SyncLog
                                   WHERE Type = 'LinkEmployeeWorkplaces'
                                   ORDER BY SyncCompletedDate DESC)");

            migrationBuilder.DropForeignKey(
                name: "FK_Workplace_Office_OfficeId",
                table: "Workplace");

            migrationBuilder.DropTable(
                name: "Office");

            migrationBuilder.DropTable(
                name: "ExternalOffice");

            migrationBuilder.DropIndex(
                name: "IX_Workplace_OfficeId",
                table: "Workplace");

            migrationBuilder.AlterColumn<string>(
                name: "OfficeId",
                table: "Workplace",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OfficeId",
                table: "Workplace",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateTable(
                name: "ExternalOffice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CitySourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitySourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalOffice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Office",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalOfficeId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Office", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Office_ExternalOffice_ExternalOfficeId",
                        column: x => x.ExternalOfficeId,
                        principalTable: "ExternalOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_OfficeId",
                table: "Workplace",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Office_ExternalOfficeId",
                table: "Office",
                column: "ExternalOfficeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Workplace_Office_OfficeId",
                table: "Workplace",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
