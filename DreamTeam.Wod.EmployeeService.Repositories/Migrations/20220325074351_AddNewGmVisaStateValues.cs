using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddNewGmVisaStateValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "GmVisaState",
                column: "Id",
                value: "RequireVerification");

            migrationBuilder.InsertData(
                table: "GmVisaState",
                column: "Id",
                value: "VisaNotValid");

            migrationBuilder.Sql(@"UPDATE RelocationPlan SET GmVisaState = 'RequireVerification' WHERE VisaState = 'Acquired'");
            migrationBuilder.Sql(@"UPDATE RelocationPlan SET GmVisaState = 'Pending' WHERE VisaState = 'RequireAssistance' OR VisaState = 'InProgress'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GmVisaState",
                keyColumn: "Id",
                keyValue: "RequireVerification");

            migrationBuilder.DeleteData(
                table: "GmVisaState",
                keyColumn: "Id",
                keyValue: "VisaNotValid");
        }
    }
}
