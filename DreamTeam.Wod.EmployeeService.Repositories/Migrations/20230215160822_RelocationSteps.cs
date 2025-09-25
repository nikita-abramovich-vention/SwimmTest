using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable SA1413

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RelocationSteps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_RelocationCaseStatus_RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_PreviousCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropTable(
                name: "RelocationCaseVisaProgress");

            migrationBuilder.DropTable(
                name: "RelocationPlanStatusTimeLimit");

            migrationBuilder.DropTable(
                name: "RelocationCaseProgress");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_PreviousCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "NewCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousCaseStatusId",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStepId",
                table: "RelocationPlan",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Induction");

            migrationBuilder.CreateTable(
                name: "RelocationStep",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationStep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CountryRelocationStep",
                columns: table => new
                {
                    CountryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StepId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryRelocationStep", x => new { x.CountryId, x.StepId });
                    table.ForeignKey(
                        name: "FK_CountryRelocationStep_RelocationStep_StepId",
                        column: x => x.StepId,
                        principalTable: "RelocationStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlanStep",
                columns: table => new
                {
                    RelocationPlanId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationInDays = table.Column<int>(type: "int", nullable: true),
                    ExpectedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanStep", x => new { x.RelocationPlanId, x.StepId });
                    table.ForeignKey(
                        name: "FK_RelocationPlanStep_RelocationPlan_RelocationPlanId",
                        column: x => x.RelocationPlanId,
                        principalTable: "RelocationPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelocationPlanStep_RelocationStep_StepId",
                        column: x => x.StepId,
                        principalTable: "RelocationStep",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RelocationStep",
                column: "Id",
                values: new object[]
                {
                    "Induction",
                    "RelocationConfirmation",
                    "PendingApproval",
                    "ProcessingQueue",
                    "VisaDocsPreparation",
                    "WaitingEmbassyAppointment",
                    "EmbassyAppointment",
                    "VisaInProgress",
                    "TrpDocsPreparation",
                    "TrpApplicationSubmission",
                    "TrpInProgress",
                    "EmploymentConfirmation",
                    "EmploymentInProgress"
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_CurrentStepId",
                table: "RelocationPlan",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryRelocationStep_StepId",
                table: "CountryRelocationStep",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanStep_StepId",
                table: "RelocationPlanStep",
                column: "StepId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_RelocationStep_CurrentStepId",
                table: "RelocationPlan",
                column: "CurrentStepId",
                principalTable: "RelocationStep",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_RelocationStep_CurrentStepId",
                table: "RelocationPlan");

            migrationBuilder.DropTable(
                name: "CountryRelocationStep");

            migrationBuilder.DropTable(
                name: "RelocationPlanStep");

            migrationBuilder.DropTable(
                name: "RelocationStep");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_CurrentStepId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "CurrentStepId",
                table: "RelocationPlan");

            migrationBuilder.AddColumn<int>(
                name: "NewCaseStatusId",
                table: "RelocationPlanChange",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousCaseStatusId",
                table: "RelocationPlanChange",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelocationCaseStatusId",
                table: "RelocationPlan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RelocationCaseProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelocationPlanId = table.Column<int>(type: "int", nullable: false),
                    IsAccommodationBooked = table.Column<bool>(type: "bit", nullable: false),
                    IsTransferBooked = table.Column<bool>(type: "bit", nullable: false),
                    IsVisaGathered = table.Column<bool>(type: "bit", nullable: false),
                    TrpState = table.Column<string>(type: "nvarchar(450)", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_RelocationCaseProgress_RelocationPlanTrpState_TrpState",
                        column: x => x.TrpState,
                        principalTable: "RelocationPlanTrpState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlanStatusTimeLimit",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TimeLimitDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanStatusTimeLimit", x => x.StatusId);
                    table.ForeignKey(
                        name: "FK_RelocationPlanStatusTimeLimit_RelocationPlanStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "RelocationPlanStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelocationCaseVisaProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RelocationCaseProgressId = table.Column<int>(type: "int", nullable: false),
                    AreDocsGathered = table.Column<bool>(type: "bit", nullable: false),
                    AreDocsSentToAgency = table.Column<bool>(type: "bit", nullable: false),
                    IsAttended = table.Column<bool>(type: "bit", nullable: false),
                    IsPassportCollected = table.Column<bool>(type: "bit", nullable: false),
                    IsScheduled = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.InsertData(
                table: "RelocationPlanStatusTimeLimit",
                columns: new[] { "StatusId", "TimeLimitDays" },
                values: new object[,]
                {
                    { 1, 14 },
                    { 2, 30 },
                    { 3, 14 },
                    { 4, 7 },
                    { 5, 14 },
                    { 6, 14 },
                    { 7, 42 },
                    { 8, 42 },
                    { 9, 14 },
                    { 10, 14 },
                    { 30, 14 },
                    { 31, 42 },
                    { 32, 70 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewCaseStatusId",
                table: "RelocationPlanChange",
                column: "NewCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousCaseStatusId",
                table: "RelocationPlanChange",
                column: "PreviousCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_RelocationCaseStatusId",
                table: "RelocationPlan",
                column: "RelocationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseProgress_RelocationPlanId",
                table: "RelocationCaseProgress",
                column: "RelocationPlanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseProgress_TrpState",
                table: "RelocationCaseProgress",
                column: "TrpState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseVisaProgress_RelocationCaseProgressId",
                table: "RelocationCaseVisaProgress",
                column: "RelocationCaseProgressId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_RelocationCaseStatus_RelocationCaseStatusId",
                table: "RelocationPlan",
                column: "RelocationCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_NewCaseStatusId",
                table: "RelocationPlanChange",
                column: "NewCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_RelocationCaseStatus_PreviousCaseStatusId",
                table: "RelocationPlanChange",
                column: "PreviousCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
