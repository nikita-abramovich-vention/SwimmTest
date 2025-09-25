using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmploymentRequestsUpdateSyncLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExternalEmploymentRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SourceId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(nullable: false),
                    StatusName = table.Column<string>(maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(maxLength: 200, nullable: true),
                    LastName = table.Column<string>(maxLength: 200, nullable: true),
                    UnitId = table.Column<string>(maxLength: 64, nullable: true),
                    LocationId = table.Column<int>(nullable: false),
                    Location = table.Column<string>(maxLength: 200, nullable: true),
                    WorkplaceId = table.Column<int>(nullable: false),
                    EmploymentDate = table.Column<DateTime>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalEmploymentRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SyncType",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentRequest",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(maxLength: 64, nullable: false),
                    SourceId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: true),
                    FirstName = table.Column<string>(maxLength: 200, nullable: true),
                    LastName = table.Column<string>(maxLength: 200, nullable: true),
                    UnitId = table.Column<string>(maxLength: 64, nullable: true),
                    Location = table.Column<string>(maxLength: 200, nullable: true),
                    WorkplaceId = table.Column<string>(maxLength: 64, nullable: true),
                    CountryId = table.Column<string>(maxLength: 64, nullable: true),
                    OrganizationId = table.Column<string>(maxLength: 64, nullable: true),
                    EmploymentDate = table.Column<DateTime>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmploymentRequest_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmploymentRequest_ExternalEmploymentRequest_SourceId",
                        column: x => x.SourceId,
                        principalTable: "ExternalEmploymentRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(nullable: false),
                    SyncStartDate = table.Column<DateTime>(nullable: false),
                    SyncCompletedDate = table.Column<DateTime>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    IsOutdated = table.Column<bool>(nullable: false),
                    AffectedEntitiesCount = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncLog_SyncType_Type",
                        column: x => x.Type,
                        principalTable: "SyncType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "SyncType",
                column: "Id",
                values: new object[]
                {
                    "DownloadExternalWspData",
                    "LinkEmployeeWorkplaces",
                    "DownloadExternalEmploymentRequestData",
                    "LinkEmploymentRequests",
                });

            migrationBuilder.Sql(@"
                INSERT INTO [dbo].[SyncLog]
                           ([Type]
                           ,[SyncStartDate]
                           ,[SyncCompletedDate]
                           ,[IsSuccessful]
                           ,[IsOutdated]
                           ,[AffectedEntitiesCount])
                SELECT CASE sl.[Type]
			                WHEN 'DownloadExternalData' THEN 'DownloadExternalWspData'
			                ELSE sl.[Type]
	                   END,
	                   sl.[SyncStartDate],
	                   sl.[SyncCompletedDate],
	                   sl.[IsSuccessful],
	                   sl.[IsOutdated],
	                   sl.[AffectedWorkplacesCount]
                FROM [dbo].[WspSyncLog] sl
            ");

            migrationBuilder.DropTable(
                name: "WspSyncLog");

            migrationBuilder.DropTable(
                name: "WspSyncType");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentRequest_EmployeeId",
                table: "EmploymentRequest",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentRequest_ExternalId",
                table: "EmploymentRequest",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentRequest_SourceId",
                table: "EmploymentRequest",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLog_Type",
                table: "SyncLog",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "WspSyncLog",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AffectedWorkplacesCount = table.Column<int>(nullable: false),
                    IsOutdated = table.Column<bool>(nullable: false),
                    IsSuccessful = table.Column<bool>(nullable: false),
                    SyncCompletedDate = table.Column<DateTime>(nullable: false),
                    SyncStartDate = table.Column<DateTime>(nullable: false),
                    Type = table.Column<string>(nullable: false),
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

            migrationBuilder.InsertData(
                table: "WspSyncType",
                column: "Id",
                values: new object[]
                {
                    "DownloadExternalData",
                    "LinkEmployeeWorkplaces",
                });

            migrationBuilder.Sql(@"
                INSERT INTO [dbo].[WspSyncLog]
                           ([Type]
                           ,[SyncStartDate]
                           ,[SyncCompletedDate]
                           ,[IsSuccessful]
                           ,[IsOutdated]
                           ,[AffectedWorkplacesCount])
                SELECT CASE sl.[Type]
			                WHEN 'DownloadExternalWspData' THEN 'DownloadExternalData'
			                ELSE sl.[Type]
	                   END,
	                   sl.[SyncStartDate],
	                   sl.[SyncCompletedDate],
	                   sl.[IsSuccessful],
	                   sl.[IsOutdated],
	                   sl.[AffectedEntitiesCount]
                FROM [dbo].[SyncLog] sl
                WHERE sl.[Type] in ('DownloadExternalWspData', 'LinkEmployeeWorkplaces')
            ");

            migrationBuilder.DropTable(
                name: "EmploymentRequest");

            migrationBuilder.DropTable(
                name: "SyncLog");

            migrationBuilder.DropTable(
                name: "ExternalEmploymentRequest");

            migrationBuilder.DropTable(
                name: "SyncType");

            migrationBuilder.CreateIndex(
                name: "IX_WspSyncLog_Type",
                table: "WspSyncLog",
                column: "Type");
        }
    }
}
