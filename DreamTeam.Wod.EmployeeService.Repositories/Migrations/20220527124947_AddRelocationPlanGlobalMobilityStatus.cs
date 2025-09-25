using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationPlanGlobalMobilityStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "RelocationCaseStatus",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "RelocationCaseStatus",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "RelocationCaseProgress",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RelocationPlanId = table.Column<int>(nullable: false),
                    IsTransferBooked = table.Column<bool>(nullable: false),
                    IsAccommodationBooked = table.Column<bool>(nullable: false),
                    IsVisaGathered = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationCaseProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelocationCaseProgress_RelocationPlan_RelocationPlanId",
                        column: x => x.RelocationPlanId,
                        principalTable: "RelocationPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelocationCaseVisaProgress",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RelocationCaseProgressId = table.Column<int>(nullable: false),
                    IsScheduled = table.Column<bool>(nullable: false),
                    AreDocsGathered = table.Column<bool>(nullable: false),
                    IsAttended = table.Column<bool>(nullable: false),
                    IsPassportCollected = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationCaseVisaProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelocationCaseVisaProgress_RelocationCaseProgress_RelocationCaseProgressId",
                        column: x => x.RelocationCaseProgressId,
                        principalTable: "RelocationCaseProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseStatus_SourceId",
                table: "RelocationCaseStatus",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseProgress_RelocationPlanId",
                table: "RelocationCaseProgress",
                column: "RelocationPlanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseVisaProgress_RelocationCaseProgressId",
                table: "RelocationCaseVisaProgress",
                column: "RelocationCaseProgressId",
                unique: true);

            migrationBuilder.Sql(@"
INSERT RelocationCaseProgress
SELECT rp.Id, 0, 0, 0
FROM RelocationPlan rp
WHERE NOT EXISTS(SELECT 1 FROM RelocationCaseProgress cp WHERE cp.RelocationPlanId = rp.Id)

INSERT RelocationCaseVisaProgress
SELECT cp.Id, 0, 0, 0, 0
FROM RelocationCaseProgress cp
WHERE NOT EXISTS(SELECT 1 FROM RelocationCaseVisaProgress vp WHERE vp.RelocationCaseProgressId = cp.Id)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelocationCaseVisaProgress");

            migrationBuilder.DropTable(
                name: "RelocationCaseProgress");

            migrationBuilder.DropIndex(
                name: "IX_RelocationCaseStatus_SourceId",
                table: "RelocationCaseStatus");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "RelocationCaseStatus");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "RelocationCaseStatus",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
