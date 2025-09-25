using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddEmploymentTypeAndEmploymentOfficeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContractor",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "IsRemoteContract",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Employee");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Employee",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Office");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentOfficeId",
                table: "Employee",
                type: "nvarchar(64)",
                nullable: true,
                maxLength: 64);

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EmploymentType",
                column: "Id",
                value: "Contractor");

            migrationBuilder.InsertData(
                table: "EmploymentType",
                column: "Id",
                value: "Office");

            migrationBuilder.InsertData(
                table: "EmploymentType",
                column: "Id",
                value: "Remote");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmploymentType",
                table: "Employee",
                column: "EmploymentType");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_EmploymentType_EmploymentType",
                table: "Employee",
                column: "EmploymentType",
                principalTable: "EmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_EmploymentType_EmploymentType",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "EmploymentType");

            migrationBuilder.DropIndex(
                name: "IX_Employee_EmploymentType",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "EmploymentOfficeId",
                table: "Employee");

            migrationBuilder.AddColumn<bool>(
                name: "IsContractor",
                table: "Employee",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoteContract",
                table: "Employee",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityId",
                table: "Employee",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}
