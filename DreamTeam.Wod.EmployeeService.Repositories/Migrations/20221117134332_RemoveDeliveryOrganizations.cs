using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class RemoveDeliveryOrganizations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePreferredDeliveryCenter");

            migrationBuilder.DropTable(
                name: "EmployeePreferredDeliveryCenters");

            migrationBuilder.DropTable(
                name: "PreferredDeliveryCenter");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeePreferredDeliveryCenters",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePreferredDeliveryCenters", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_EmployeePreferredDeliveryCenters_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferredDeliveryCenter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferredDeliveryCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePreferredDeliveryCenter",
                columns: table => new
                {
                    PreferredDeliveryCenterId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePreferredDeliveryCenter", x => new { x.PreferredDeliveryCenterId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_EmployeePreferredDeliveryCenter_EmployeePreferredDeliveryCenters_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeePreferredDeliveryCenters",
                        principalColumn: "EmployeeId",
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
                columns: new[] { "Id", "ExternalId", "IsActive", "Name", "OrganizationId" },
                values: new object[,]
                {
                    { 1, "central_asia_kazakhstan_uzbekistan_kyrgyzstan", true, "Central Asia (Kazakhstan, Uzbekistan, Kyrgyzstan)", null },
                    { 2, "georgia", true, "Georgia", null },
                    { 3, "poland", true, "Poland", null },
                    { 4, "lithuania", true, "Lithuania", null },
                    { 5, "czech_republic_slovakia_hungary", true, "Czech Republic, Slovakia, Hungary", null },
                    { 6, "bulgaria_serbia", true, "Bulgaria, Serbia", null },
                    { 7, "latam", true, "LATAM", null },
                    { 8, "belarus", false, "Belarus", null },
                    { 9, "ukraine", false, "Ukraine", null },
                    { 10, "uk", false, "UK", null },
                    { 11, "us", false, "US", null },
                    { 12, "austria", false, "Austria", null },
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
    }
}