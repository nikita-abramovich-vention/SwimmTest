using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AdjustRelocationPlanCompensation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "CompensationInfo",
                newName: "Total");

            migrationBuilder.AlterColumn<float>(
                name: "Total",
                table: "CompensationInfo",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<float>(
                name: "PreviousCompensation_Amount",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "PreviousCompensation_Currency",
                table: "CompensationInfo",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Details_Child_Amount",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "Details_Child_Enabled",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Details_Child_NumberOfPeople",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Details_Employee_Amount",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<bool>(
                name: "Details_Employee_Enabled",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Details_Employee_NumberOfPeople",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Details_Spouse_Amount",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Details_Spouse_Enabled",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Details_Spouse_NumberOfPeople",
                table: "CompensationInfo",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousCompensation_Amount",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "PreviousCompensation_Currency",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Child_Amount",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Child_Enabled",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Child_NumberOfPeople",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Employee_Amount",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Employee_Enabled",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Employee_NumberOfPeople",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Spouse_Amount",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Spouse_Enabled",
                table: "CompensationInfo");

            migrationBuilder.DropColumn(
                name: "Details_Spouse_NumberOfPeople",
                table: "CompensationInfo");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "CompensationInfo",
                newName: "Amount");
        }
    }
}
