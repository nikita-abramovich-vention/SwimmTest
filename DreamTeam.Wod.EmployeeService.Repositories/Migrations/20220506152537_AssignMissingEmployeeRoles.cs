using Microsoft.EntityFrameworkCore.Migrations;

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class AssignMissingEmployeeRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO EmployeeRole (EmployeeId, RoleId)
                SELECT rce.EmployeeId, rce.RoleConfigurationId
                FROM RoleConfigurationEmployee rce
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM EmployeeRole er
                    WHERE er.EmployeeId = rce.EmployeeId AND er.RoleId = rce.RoleConfigurationId
                )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
