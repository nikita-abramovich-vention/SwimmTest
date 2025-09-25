using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddSeniorityToEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSeniority",
                table: "Role",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SeniorityId",
                table: "Employee",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Seniority",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    IsHidden = table.Column<bool>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seniority", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Seniority",
                columns: new[] { "Id", "ExternalId", "IsHidden", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "junior", false, "Junior", 1 },
                    { 2, "middle", true, "Middle", 2 },
                    { 3, "senior", false, "Senior", 3 },
                    { 4, "lead", false, "Lead", 4 },
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employee_SeniorityId",
                table: "Employee",
                column: "SeniorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Seniority_ExternalId",
                table: "Seniority",
                column: "ExternalId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Seniority_SeniorityId",
                table: "Employee",
                column: "SeniorityId",
                principalTable: "Seniority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Seniority_SeniorityId",
                table: "Employee");

            migrationBuilder.DropTable(
                name: "Seniority");

            migrationBuilder.DropIndex(
                name: "IX_Employee_SeniorityId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "HasSeniority",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "SeniorityId",
                table: "Employee");
        }
    }
}