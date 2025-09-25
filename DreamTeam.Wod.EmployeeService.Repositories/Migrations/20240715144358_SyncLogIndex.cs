using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreamTeam.Wod.EmployeeService.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SyncLogIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncLog_Type",
                table: "SyncLog");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLog_Type_IsSuccessful_SyncCompletedDate",
                table: "SyncLog",
                columns: new[] { "Type", "IsSuccessful", "SyncCompletedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentLabSyncLog_IsSuccessful_SyncCompletedDate",
                table: "StudentLabSyncLog",
                columns: new[] { "IsSuccessful", "SyncCompletedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSnapshotLog_IsSuccessful_Date",
                table: "EmployeeSnapshotLog",
                columns: new[] { "IsSuccessful", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SyncLog_Type_IsSuccessful_SyncCompletedDate",
                table: "SyncLog");

            migrationBuilder.DropIndex(
                name: "IX_StudentLabSyncLog_IsSuccessful_SyncCompletedDate",
                table: "StudentLabSyncLog");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSnapshotLog_IsSuccessful_Date",
                table: "EmployeeSnapshotLog");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLog_Type",
                table: "SyncLog",
                column: "Type");
        }
    }
}
