using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AddDismissalRequestType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DismissalSpecificId",
                table: "ExternalDismissalRequest",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DismissalRequestType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissalRequestType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DismissalRequestType",
                column: "Id",
                values: new object[]
                {
                    "Ordinary",
                    "Relocation",
                    "ContractChange",
                    "MaternityLeave",
                });

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "DismissalRequest",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "Ordinary");

            migrationBuilder.CreateIndex(
                name: "IX_DismissalRequest_Type",
                table: "DismissalRequest",
                column: "Type");

            migrationBuilder.AddForeignKey(
                name: "FK_DismissalRequest_DismissalRequestType_Type",
                table: "DismissalRequest",
                column: "Type",
                principalTable: "DismissalRequestType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE [WodEmployees].[dbo].[ExternalDismissalRequest]
                SET DismissalSpecificId = 'Ordinary dismiss'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DismissalRequest_DismissalRequestType_Type",
                table: "DismissalRequest");

            migrationBuilder.DropTable(
                name: "DismissalRequestType");

            migrationBuilder.DropIndex(
                name: "IX_DismissalRequest_Type",
                table: "DismissalRequest");

            migrationBuilder.DropColumn(
                name: "DismissalSpecificId",
                table: "ExternalDismissalRequest");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "DismissalRequest");
        }
    }
}
