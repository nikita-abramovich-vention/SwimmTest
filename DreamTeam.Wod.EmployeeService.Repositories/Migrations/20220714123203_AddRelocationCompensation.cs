using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddRelocationCompensation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompensationInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RelocationPlanId = table.Column<int>(nullable: false),
                    Amount = table.Column<int>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompensationInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompensationInfo_RelocationPlan_RelocationPlanId",
                        column: x => x.RelocationPlanId,
                        principalTable: "RelocationPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompensationInfo_RelocationPlanId",
                table: "CompensationInfo",
                column: "RelocationPlanId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompensationInfo");
        }
    }
}
