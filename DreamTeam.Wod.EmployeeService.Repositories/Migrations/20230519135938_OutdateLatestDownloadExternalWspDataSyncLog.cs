using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    public partial class OutdateLatestDownloadExternalWspDataSyncLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE SyncLog SET IsOutdated = 1 WHERE Id =
                (
                    SELECT TOP 1 Id FROM SyncLog WHERE Type = 'DownloadExternalWspData' AND IsSuccessful = 1 ORDER BY SyncCompletedDate DESC
                )
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
