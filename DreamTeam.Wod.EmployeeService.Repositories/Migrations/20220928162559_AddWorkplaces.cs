using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddWorkplaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SyncLog",
                newName: "StudentLabSyncLog");

            migrationBuilder.RenameIndex(
                name: "PK_SyncLog",
                newName: "PK_StudentLabSyncLog",
                table: "StudentLabSyncLog");

            migrationBuilder.RenameColumn(
                name: "SyncLogId",
                newName: "Id",
                table: "StudentLabSyncLog");

            migrationBuilder.CreateTable(
                name: "Workplace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    SourceId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    SchemeUrl = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workplace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WspSyncType",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WspSyncType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeWorkplace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(nullable: false),
                    WorkplaceId = table.Column<int>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkplace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkplace_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkplace_Workplace_WorkplaceId",
                        column: x => x.WorkplaceId,
                        principalTable: "Workplace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WspSyncLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(nullable: false),
                    SyncStartDate = table.Column<DateTime>(nullable: false),
                    SyncCompletedDate = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    IsOutdated = table.Column<bool>(nullable: false),
                    AffectedWorkplacesCount = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WspSyncLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WspSyncLog_WspSyncType_Type",
                        column: x => x.Type,
                        principalTable: "WspSyncType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalEmployeeWorkplace",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SourceEmployeeId = table.Column<string>(nullable: false),
                    SourceWorkplaceId = table.Column<string>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    EmployeeWorkplaceId = table.Column<int>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmployeeWorkplace", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalEmployeeWorkplace_EmployeeWorkplace_EmployeeWorkplaceId",
                        column: x => x.EmployeeWorkplaceId,
                        principalTable: "EmployeeWorkplace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "WspSyncType",
                column: "Id",
                value: "DownloadExternalData");

            migrationBuilder.InsertData(
                table: "WspSyncType",
                column: "Id",
                value: "LinkEmployeeWorkplaces");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkplace_WorkplaceId",
                table: "EmployeeWorkplace",
                column: "WorkplaceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkplace_EmployeeId_WorkplaceId",
                table: "EmployeeWorkplace",
                columns: new[] { "EmployeeId", "WorkplaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployeeWorkplace_EmployeeWorkplaceId",
                table: "ExternalEmployeeWorkplace",
                column: "EmployeeWorkplaceId",
                unique: true,
                filter: "[EmployeeWorkplaceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalEmployeeWorkplace_SourceEmployeeId_SourceWorkplaceId",
                table: "ExternalEmployeeWorkplace",
                columns: new[] { "SourceEmployeeId", "SourceWorkplaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workplace_ExternalId",
                table: "Workplace",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WspSyncLog_Type",
                table: "WspSyncLog",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalEmployeeWorkplace");

            migrationBuilder.DropTable(
                name: "StudentLabSyncLog");

            migrationBuilder.DropTable(
                name: "WspSyncLog");

            migrationBuilder.DropTable(
                name: "EmployeeWorkplace");

            migrationBuilder.DropTable(
                name: "WspSyncType");

            migrationBuilder.DropTable(
                name: "Workplace");

            migrationBuilder.RenameTable(
                name: "StudentLabSyncLog",
                newName: "SyncLog");

            migrationBuilder.RenameIndex(
                name: "PK_StudentLabSyncLog",
                newName: "PK_SyncLog",
                table: "SyncLog");

            migrationBuilder.RenameColumn(
                name: "Id",
                newName: "SyncLogId",
                table: "SyncLog");
        }
    }
}