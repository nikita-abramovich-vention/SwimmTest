using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;

namespace DreamTeam.Wod.EmployeeService.Foundation.WspSync
{
    public interface IWspSyncService
    {
        event AsyncObserver<EmployeeChangedEventArgs> EmployeeWorkplacesChanged;


        Task ActivateRegularSyncAsync();

        Task<bool> SyncAsync();
    }
}