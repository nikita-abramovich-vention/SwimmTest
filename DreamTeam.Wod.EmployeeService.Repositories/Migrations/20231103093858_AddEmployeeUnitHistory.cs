using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeUnitHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeUnitChange");

            migrationBuilder.CreateTable(
                name: "EmployeeUnitHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeUnitHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeUnitHistory_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExternalEmployeeUnitHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceEmployeeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceUnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmployeeUnitHistoryId = table.Column<int>(type: "int", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmployeeUnitHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalEmployeeUnitHistory_EmployeeUnitHistory_EmployeeUnitHistoryId",
                        column: x => x.EmployeeUnitHistoryId,
                        principalTable: "EmployeeUnitHistory",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SyncType",
                column: "Id",
                value: "DownloadExternalEmployeeUnitHistory");

            migrationBuilder.InsertData(
                table: "SyncType",
                column: "Id",
                value: "LinkEmployeeUnitHistory");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUnitHistory_EmployeeId_UnitId_EndDate",
                table: "EmployeeUnitHistory",
                columns: new[] { "EmployeeId", "UnitId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUnitHistory_EmployeeId_UnitId_StartDate",
                table: "EmployeeUnitHistory",
                columns: new[] { "EmployeeId", "UnitId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployeeUnitHistory_EmployeeUnitHistoryId",
                table: "ExternalEmployeeUnitHistory",
                column: "EmployeeUnitHistoryId",
                unique: true,
                filter: "[EmployeeUnitHistoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployeeUnitHistory_SourceEmployeeId_SourceUnitId_EndDate",
                table: "ExternalEmployeeUnitHistory",
                columns: new[] { "SourceEmployeeId", "SourceUnitId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployeeUnitHistory_SourceEmployeeId_SourceUnitId_StartDate",
                table: "ExternalEmployeeUnitHistory",
                columns: new[] { "SourceEmployeeId", "SourceUnitId", "StartDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalEmployeeUnitHistory");

            migrationBuilder.DropTable(
                name: "EmployeeUnitHistory");

            migrationBuilder.DeleteData(
                table: "SyncType",
                keyColumn: "Id",
                keyValue: "DownloadExternalEmployeeUnitHistory");

            migrationBuilder.DeleteData(
                table: "SyncType",
                keyColumn: "Id",
                keyValue: "LinkEmployeeUnitHistory");

            migrationBuilder.CreateTable(
                name: "EmployeeUnitChange",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    NewUnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PreviousUnitId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeUnitChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeUnitChange_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeUnitChange_EmployeeId",
                table: "EmployeeUnitChange",
                column: "EmployeeId");
        }
    }
}
