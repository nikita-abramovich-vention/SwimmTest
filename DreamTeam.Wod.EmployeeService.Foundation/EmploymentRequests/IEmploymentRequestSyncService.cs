using System.Threading.Tasks;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests
{
    public interface IEmploymentRequestSyncService
    {
        Task ActivateRegularSyncAsync();
    }
}
