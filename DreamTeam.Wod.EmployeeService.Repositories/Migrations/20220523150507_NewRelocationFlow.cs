using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class NewRelocationFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_GmVisaState_GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_VisaState_VisaState",
                table: "RelocationPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_GmVisaState_NewGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_VisaState_NewVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_GmVisaState_PreviousGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlanChange_VisaState_PreviousVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropTable(
                name: "CurrentCountry");

            migrationBuilder.DropTable(
                name: "GmVisaState");

            migrationBuilder.DropTable(
                name: "VisaState");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_NewGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_NewVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_PreviousGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlanChange_PreviousVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_EmployeeId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_VisaState",
                table: "RelocationPlan");

            migrationBuilder.Sql(@"        
            DELETE RelocationPlanChange
            WHERE [Type] = 'GmVisaState'");

            migrationBuilder.Sql(@"        
            DELETE RelocationPlanChange
            WHERE [Type] = 'VisaState'");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "GmVisaState");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "VisaState");

            migrationBuilder.DropColumn(
                name: "NewGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "NewVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousGmVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousVisaState",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "GmVisaState",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "VisaState",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "CountryIdObsolete",
                table: "CurrentLocation");

            migrationBuilder.DropColumn(
                name: "GmVisaStateChangedBy",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "GmVisaStateChangeDate",
                table: "RelocationPlan");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "RelocationPlan",
                newName: "EmployeeDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "InductionStatusChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InductionStatusChangedBy",
                maxLength: 64,
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "RelocationPlan",
                newName: "EmployeeComment");

            migrationBuilder.AddColumn<string>(
                name: "GmComment",
                table: "RelocationPlan",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NewIsApproved",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousIsApproved",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "RelocationPlan",
                nullable: false,
                oldClrType: typeof(string),
                oldDefaultValue: "Active");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "RelocationPlan",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApproverComment",
                table: "RelocationPlan",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApproverDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApproverId",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloseComment",
                table: "RelocationPlan",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedBy",
                table: "RelocationPlan",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RelocationCaseStatusId",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Salary",
                table: "RelocationPlan",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCompanyOffice",
                table: "CurrentLocation",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RelocationCaseStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    SourceId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelocationCaseStatus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "Approved");

            migrationBuilder.InsertData(
                table: "RelocationPlanState",
                column: "Id",
                value: "Rejected");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_ApproverId",
                table: "RelocationPlan",
                column: "ApproverId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_RelocationCaseStatusId",
                table: "RelocationPlan",
                column: "RelocationCaseStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_EmployeeId_State",
                table: "RelocationPlan",
                columns: new[] { "EmployeeId", "State" },
                unique: true,
                filter: "[State] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationCaseStatus_ExternalId",
                table: "RelocationCaseStatus",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_Employee_ApproverId",
                table: "RelocationPlan",
                column: "ApproverId",
                principalTable: "Employee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_RelocationCaseStatus_RelocationCaseStatusId",
                table: "RelocationPlan",
                column: "RelocationCaseStatusId",
                principalTable: "RelocationCaseStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_Employee_ApproverId",
                table: "RelocationPlan");

            migrationBuilder.DropForeignKey(
                name: "FK_RelocationPlan_RelocationCaseStatus_RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.DropTable(
                name: "RelocationCaseStatus");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_ApproverId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.DropIndex(
                name: "IX_RelocationPlan_EmployeeId_State",
                table: "RelocationPlan");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "Approved");

            migrationBuilder.DeleteData(
                table: "RelocationPlanState",
                keyColumn: "Id",
                keyValue: "Rejected");

            migrationBuilder.DropColumn(
                name: "NewIsApproved",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "PreviousIsApproved",
                table: "RelocationPlanChange");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ApproverComment",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ApproverDate",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ApproverId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "CloseComment",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "ClosedBy",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "RelocationCaseStatusId",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "RelocationPlan");

            migrationBuilder.DropColumn(
                name: "HasCompanyOffice",
                table: "CurrentLocation");

            migrationBuilder.DropColumn(
                name: "InductionStatusChangedBy",
                table: "RelocationPlan");

            migrationBuilder.AddColumn<string>(
                name: "GmVisaStateChangedBy",
                maxLength: 64,
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "InductionStatusChangeDate",
                table: "RelocationPlan");

            migrationBuilder.AddColumn<DateTime>(
                name: "GmVisaStateChangeDate",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "EmployeeComment",
                table: "RelocationPlan",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "EmployeeDate",
                table: "RelocationPlan",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "NewGmVisaState",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewVisaState",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousGmVisaState",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousVisaState",
                table: "RelocationPlanChange",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: "Active",
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "GmVisaState",
                table: "RelocationPlan",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisaState",
                table: "RelocationPlan",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CountryIdObsolete",
                table: "CurrentLocation",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CurrentCountry",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    SupportsRelocation = table.Column<bool>(nullable: false, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentCountry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GmVisaState",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GmVisaState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisaState",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisaState", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "GmVisaState",
                column: "Id",
                values: new object[]
                {
                    "RequireVerification",
                    "Pending",
                    "Agency",
                    "EmbassyAppointment",
                    "VisaInProgress",
                    "VisaDone",
                    "VisaNotValid",
                });

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                values: new object[]
                {
                    "VisaState",
                    "GmVisaState",
                });

            migrationBuilder.InsertData(
                table: "VisaState",
                column: "Id",
                values: new object[]
                {
                    "Acquired",
                    "InProgress",
                    "NotRequired",
                    "RequireAssistance",
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewGmVisaState",
                table: "RelocationPlanChange",
                column: "NewGmVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_NewVisaState",
                table: "RelocationPlanChange",
                column: "NewVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousGmVisaState",
                table: "RelocationPlanChange",
                column: "PreviousGmVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlanChange_PreviousVisaState",
                table: "RelocationPlanChange",
                column: "PreviousVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_EmployeeId",
                table: "RelocationPlan",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_GmVisaState",
                table: "RelocationPlan",
                column: "GmVisaState");

            migrationBuilder.CreateIndex(
                name: "IX_RelocationPlan_VisaState",
                table: "RelocationPlan",
                column: "VisaState");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentCountry_ExternalId",
                table: "CurrentCountry",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentCountry_Name",
                table: "CurrentCountry",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_GmVisaState_GmVisaState",
                table: "RelocationPlan",
                column: "GmVisaState",
                principalTable: "GmVisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlan_VisaState_VisaState",
                table: "RelocationPlan",
                column: "VisaState",
                principalTable: "VisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_GmVisaState_NewGmVisaState",
                table: "RelocationPlanChange",
                column: "NewGmVisaState",
                principalTable: "GmVisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_VisaState_NewVisaState",
                table: "RelocationPlanChange",
                column: "NewVisaState",
                principalTable: "VisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_GmVisaState_PreviousGmVisaState",
                table: "RelocationPlanChange",
                column: "PreviousGmVisaState",
                principalTable: "GmVisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RelocationPlanChange_VisaState_PreviousVisaState",
                table: "RelocationPlanChange",
                column: "PreviousVisaState",
                principalTable: "VisaState",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
