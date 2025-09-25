using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmployeeHistoryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RelocationPlan",
                table: "RelocationPlan");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelocationPlan",
                table: "RelocationPlan",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmployeeCurrentLocationChange",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(nullable: false),
                    PreviousLocationId = table.Column<int>(nullable: true),
                    NewLocationId = table.Column<int>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCurrentLocationChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeCurrentLocationChange_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeCurrentLocationChange_CurrentLocation_NewLocationId",
                        column: x => x.NewLocationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCurrentLocationChange_CurrentLocation_PreviousLocationId",
                        column: x => x.PreviousLocationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeOrganizationChange",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EmployeeId = table.Column<int>(nullable: false),
                    PreviousOrganizationId = table.Column<string>(maxLength: 64, nullable: true),
                    NewOrganizationId = table.Column<string>(maxLength: 64, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeOrganizationChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeOrganizationChange_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlanChangeType",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanChangeType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlanState",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelocationPlanChange",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RelocationPlanId = table.Column<int>(nullable: false),
                    EmployeeId = table.Column<int>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    PreviousWithEmployment = table.Column<bool>(nullable: true),
                    NewWithEmployment = table.Column<bool>(nullable: true),
                    PreviousDestinationId = table.Column<int>(nullable: true),
                    NewDestinationId = table.Column<int>(nullable: true),
                    PreviousVisaState = table.Column<string>(nullable: true),
                    NewVisaState = table.Column<string>(nullable: true),
                    PreviousGmVisaState = table.Column<string>(nullable: true),
                    NewGmVisaState = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 64, nullable: true),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationPlanChange", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_CurrentLocation_NewDestinationId",
                        column: x => x.NewDestinationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_GmVisaState_NewGmVisaState",
                        column: x => x.NewGmVisaState,
                        principalTable: "GmVisaState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_VisaState_NewVisaState",
                        column: x => x.NewVisaState,
                        principalTable: "VisaState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_CurrentLocation_PreviousDestinationId",
                        column: x => x.PreviousDestinationId,
                        principalTable: "CurrentLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_GmVisaState_PreviousGmVisaState",
                        column: x => x.PreviousGmVisaState,
                        principalTable: "GmVisaState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_VisaState_PreviousVisaState",
                        column: x => x.PreviousVisaState,
                        principalTable: "VisaState",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_RelocationPlan_RelocationPlanId",
                        column: x => x.RelocationPlanId,
                        principalTable: "RelocationPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelocationPlanChange_RelocationPlanChangeType_Type",
                        column: x => x.Type,
                        principalTable: "RelocationPlanChangeType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                values: new object[]
                {
                    "WithEmployment",
                    "Destination",
                    "VisaState",
                    "GmVisaState",
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanState",
                column: "Id",
                values: new object[]
                {
                    "Active",
                    "Completed",
                    "Cancelled",
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_EmployeeId",
                table: "RelocationPlan",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_State",
                table: "RelocationPlan",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCurrentLocationChange_EmployeeId",
                table: "EmployeeCurrentLocationChange",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCurrentLocationChange_NewLocationId",
                table: "EmployeeCurrentLocationChange",
                column: "NewLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCurrentLocationChange_PreviousLocationId",
                table: "EmployeeCurrentLocationChange",
                column: "PreviousLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeOrganizationChange_EmployeeId",
                table: "EmployeeOrganizationChange",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_EmployeeId",
                table: "RelocationPlanChange",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewDestinationId",
                table: "RelocationPlanChange",
                column: "NewDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewGmVisaState",
                table: "RelocationPlanChange",
                column: "NewGmVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewVisaState",
                table: "RelocationPlanChange",
                column: "NewVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousDestinationId",
                table: "RelocationPlanChange",
                column: "PreviousDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousGmVisaState",
                table: "RelocationPlanChange",
                column: "PreviousGmVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousVisaState",
                table: "RelocationPlanChange",
                column: "PreviousVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_RelocationPlanId",
                table: "RelocationPlanChange",
                column: "RelocationPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_Type",
                table: "RelocationPlanChange",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_RelocationPlanState_State",
                table: "RelocationPlan",
                column: "State",
                principalTable: "RelocationPlanState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_RelocationPlanState_State",
                table: "RelocationPlan");

            migrationBuilder.DropTable(
                name: "EmployeeCurrentLocationChange");

            migrationBuilder.DropTable(
                name: "EmployeeOrganizationChange");

            migrationBuilder.DropTable(
                name: "RelocationPlanChange");

            migrationBuilder.DropTable(
                name: "RelocationPlanState");

            migrationBuilder.DropTable(
                name: "RelocationPlanChangeType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RelocationPlan",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_EmployeeId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_State",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "State",
                table: "RelocationPlan");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelocationPlan",
                table: "RelocationPlan",
                column: "EmployeeId");
        }
    }
}
