using System.Threading.Tasks;

namespace DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync
{
    public interface IStudentLabSyncService
    {
        Task ActivateRegularSyncAsync();

        Task SyncAsync();
    }
}