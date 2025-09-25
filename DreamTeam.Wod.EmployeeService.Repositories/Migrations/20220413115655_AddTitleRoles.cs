using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddTitleRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TitleRoleId",
                table: "Employee",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TitleRole",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    HasSeniority = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleRole", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_TitleRoleId",
                table: "Employee",
                column: "TitleRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleRole_ExternalId",
                table: "TitleRole",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TitleRole_Name",
                table: "TitleRole",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO TitleRole (ExternalId, Name, HasSeniority)
                SELECT ExternalId, Name, HasSeniority
                FROM Role
            ");
            migrationBuilder.Sql(@"
                UPDATE Employee
                SET TitleRoleId = (
                    SELECT TitleRole.Id
                    FROM EmployeeRole
                    JOIN Role on Role.Id = EmployeeRole.RoleId
                    JOIN TitleRole on TitleRole.ExternalId = Role.ExternalId
                    WHERE EmployeeId = Employee.Id
                )
            ");

            migrationBuilder.Sql(@"DELETE FROM EmployeeRole");
            migrationBuilder.Sql(@"DELETE FROM Role");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "HasSeniority",
                table: "Role");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Role",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 200);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_TitleRole_TitleRoleId",
                table: "Employee",
                column: "TitleRoleId",
                principalTable: "TitleRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_TitleRole_TitleRoleId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "TitleRole");

            migrationBuilder.DropIndex(
                name: "IX_Employee_TitleRoleId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "TitleRoleId",
                table: "Employee");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "Role",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Role",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasSeniority",
                table: "Role",
                nullable: false,
                defaultValue: false);
        }
    }
}
