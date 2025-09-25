using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddInternshipEmploymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInternship",
                table: "EmploymentPeriod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "EmployeeSnapshot",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Office");

            migrationBuilder.InsertData(
                table: "EmploymentType",
                column: "Id",
                value: "Internship");

            migrationBuilder.Sql("""
                                 UPDATE s
                                 SET s.[EmploymentType] = e.[EmploymentType]
                                 FROM [WodEmployees].[dbo].[EmployeeSnapshot] s
                                 JOIN [WodEmployees].[dbo].[Employee] e ON s.[EmployeeId] = e.[Id]
                                 """);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshot_EmploymentType",
                table: "EmployeeSnapshot",
                column: "EmploymentType");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSnapshot_EmploymentType_EmploymentType",
                table: "EmployeeSnapshot",
                column: "EmploymentType",
                principalTable: "EmploymentType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSnapshot_EmploymentType_EmploymentType",
                table: "EmployeeSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshot_EmploymentType",
                table: "EmployeeSnapshot");

            migrationBuilder.DeleteData(
                table: "EmploymentType",
                keyColumn: "Id",
                keyValue: "Internship");

            migrationBuilder.DropColumn(
                name: "IsInternship",
                table: "EmploymentPeriod");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "EmployeeSnapshot");
        }
    }
}
