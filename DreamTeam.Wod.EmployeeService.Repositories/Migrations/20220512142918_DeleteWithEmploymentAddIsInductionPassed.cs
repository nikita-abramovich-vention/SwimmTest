using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class DeleteWithEmploymentAddIsInductionPassed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"        
            DELETE RelocationPlanChange
            WHERE [Type] = 'WithEmployment'");

            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "WithEmployment");

            migrationBuilder.RenameColumn(
                name: "PreviousWithEmployment",
                table: "RelocationPlanChange",
                newName: "PreviousIsInductionPassed");

            migrationBuilder.RenameColumn(
                name: "NewWithEmployment",
                table: "RelocationPlanChange",
                newName: "NewIsInductionPassed");

            migrationBuilder.RenameColumn(
                name: "WithEmployment",
                table: "RelocationPlan",
                newName: "IsInductionPassed");

            migrationBuilder.Sql(@"
            UPDATE RelocationPlan
            SET IsInductionPassed = 1
            WHERE GmVisaState IN ('Agency', 'EmbassyAppointment', 'VisaInProgress', 'VisaDone')");

            migrationBuilder.Sql(@"
            UPDATE RelocationPlan
            SET IsInductionPassed = 0
            WHERE GmVisaState NOT IN ('Agency', 'EmbassyAppointment', 'VisaInProgress', 'VisaDone')");

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "InductionPassed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RelocationPlanChangeType",
                keyColumn: "Id",
                keyValue: "InductionPassed");

            migrationBuilder.RenameColumn(
                name: "PreviousIsInductionPassed",
                table: "RelocationPlanChange",
                newName: "PreviousWithEmployment");

            migrationBuilder.RenameColumn(
                name: "NewIsInductionPassed",
                table: "RelocationPlanChange",
                newName: "NewWithEmployment");

            migrationBuilder.RenameColumn(
                name: "IsInductionPassed",
                table: "RelocationPlan",
                newName: "WithEmployment");

            migrationBuilder.InsertData(
                table: "RelocationPlanChangeType",
                column: "Id",
                value: "WithEmployment");
        }
    }
}
