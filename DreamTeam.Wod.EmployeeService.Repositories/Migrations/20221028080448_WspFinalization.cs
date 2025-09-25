using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class WspFinalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ExternalEmployeeWorkplace");
            migrationBuilder.Sql(@"DELETE FROM EmployeeWorkplace");
            migrationBuilder.Sql(@"DELETE FROM Workplace");

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

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "Workplace");

            migrationBuilder.AlterColumn<string>(
                name: "SchemeUrl",
                table: "Workplace",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalWorkplaceId",
                table: "Workplace",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDate",
                table: "Workplace",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Workplace",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CityId",
                table: "Employee",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoteContract",
                table: "Employee",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalOffice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CitySourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitySourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalOffice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalWorkplace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SchemeUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfficeSourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalWorkplace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Office",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CityId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalOfficeId = table.Column<int>(type: "int", nullable: false),
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
                name: "IX_Workplace_ExternalWorkplaceId",
                table: "Workplace",
                column: "ExternalWorkplaceId",
                unique: true);

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
                name: "FK_Workplace_ExternalWorkplace_ExternalWorkplaceId",
                table: "Workplace",
                column: "ExternalWorkplaceId",
                principalTable: "ExternalWorkplace",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workplace_Office_OfficeId",
                table: "Workplace",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workplace_ExternalWorkplace_ExternalWorkplaceId",
                table: "Workplace");

            migrationBuilder.DropForeignKey(
                name: "FK_Workplace_Office_OfficeId",
                table: "Workplace");

            migrationBuilder.DropTable(
                name: "ExternalWorkplace");

            migrationBuilder.DropTable(
                name: "Office");

            migrationBuilder.DropTable(
                name: "ExternalOffice");

            migrationBuilder.DropIndex(
                name: "IX_Workplace_ExternalWorkplaceId",
                table: "Workplace");

            migrationBuilder.DropIndex(
                name: "IX_Workplace_OfficeId",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "ExternalWorkplaceId",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "LastSyncDate",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Workplace");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "IsRemoteContract",
                table: "Employee");

            migrationBuilder.AlterColumn<string>(
                name: "SchemeUrl",
                table: "Workplace",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "Workplace",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}