using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddDismissalRequestsUpdateSyncLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalDismissalRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceEmployeeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DismissalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceCreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalDismissalRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DismissalRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SourceDismissalRequestId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DismissalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissalRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DismissalRequest_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissalRequest_ExternalDismissalRequest_SourceDismissalRequestId",
                        column: x => x.SourceDismissalRequestId,
                        principalTable: "ExternalDismissalRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SyncType",
                column: "Id",
                value: "DownloadExternalDismissalRequestData");

            migrationBuilder.InsertData(
                table: "SyncType",
                column: "Id",
                value: "LinkDismissalRequests");

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_EmployeeId",
                table: "DismissalRequest",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_ExternalId",
                table: "DismissalRequest",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_SourceDismissalRequestId",
                table: "DismissalRequest",
                column: "SourceDismissalRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalDismissalRequest_SourceId",
                table: "ExternalDismissalRequest",
                column: "SourceId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DismissalRequest");

            migrationBuilder.DropTable(
                name: "ExternalDismissalRequest");

            migrationBuilder.DeleteData(
                table: "SyncType",
                keyColumn: "Id",
                keyValue: "DownloadExternalDismissalRequestData");

            migrationBuilder.DeleteData(
                table: "SyncType",
                keyColumn: "Id",
                keyValue: "LinkDismissalRequests");
        }
    }
}
