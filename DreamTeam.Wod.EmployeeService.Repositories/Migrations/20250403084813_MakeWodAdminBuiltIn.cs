using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MakeWodAdminBuiltIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ROLE
                SET IsBuiltIn = 1, ExternalId = 'wod_admin', Description = 'System Admin'
                WHERE Name = 'WoD Admin'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
