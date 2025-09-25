using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddNonActivePreferredDeliveryCenters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PreferredDeliveryCenter",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "PreferredDeliveryCenter",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsActive",
                value: true);

            migrationBuilder.InsertData(
                table: "PreferredDeliveryCenter",
                columns: new[] { "Id", "ExternalId", "IsActive", "Name", "OrganizationId" },
                values: new object[,]
                {
                    { 8, "belarus", false, "Belarus", null },
                    { 9, "ukraine", false, "Ukraine", null },
                    { 10, "uk", false, "UK", null },
                    { 11, "us", false, "US", null },
                    { 12, "austria", false, "Austria", null },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PreferredDeliveryCenter",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PreferredDeliveryCenter");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PreferredDeliveryCenter");
        }
    }
}
