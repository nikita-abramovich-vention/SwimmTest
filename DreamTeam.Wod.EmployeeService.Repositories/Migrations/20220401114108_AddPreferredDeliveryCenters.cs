using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddPreferredDeliveryCenters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreferredDeliveryCenter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExternalId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredDeliveryCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePreferredDeliveryCenter",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(nullable: false),
                    PreferredDeliveryCenterId = table.Column<int>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePreferredDeliveryCenter", x => new { x.PreferredDeliveryCenterId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_EmployeePreferredDeliveryCenter_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePreferredDeliveryCenter_PreferredDeliveryCenter_PreferredDeliveryCenterId",
                        column: x => x.PreferredDeliveryCenterId,
                        principalTable: "PreferredDeliveryCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PreferredDeliveryCenter",
                columns: new[] { "Id", "ExternalId", "Name" },
                values: new object[,]
                {
                    { 1, "central_asia_kazakhstan_uzbekistan_kyrgyzstan", "Central Asia (Kazakhstan, Uzbekistan, Kyrgyzstan)" },
                    { 2, "georgia", "Georgia" },
                    { 3, "poland", "Poland" },
                    { 4, "lithuania", "Lithuania" },
                    { 5, "czech_republic_slovakia_hungary", "Czech Republic, Slovakia, Hungary" },
                    { 6, "bulgaria_serbia", "Bulgaria, Serbia" },
                    { 7, "latam", "LATAM" },
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePreferredDeliveryCenter_EmployeeId",
                table: "EmployeePreferredDeliveryCenter",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferredDeliveryCenter_ExternalId",
                table: "PreferredDeliveryCenter",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreferredDeliveryCenter_Name",
                table: "PreferredDeliveryCenter",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePreferredDeliveryCenter");

            migrationBuilder.DropTable(
                name: "PreferredDeliveryCenter");
        }
    }
}
