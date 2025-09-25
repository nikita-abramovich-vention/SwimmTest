using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Foundation;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests
{
    public interface IDismissalRequestSyncService
    {
        event AsyncObserver<EntityChangedEventArgs<DismissalRequest>> DismissalRequestCreated;

        event AsyncObserver<EntityChangedEventArgs<DismissalRequest>> DismissalRequestUpdated;


        Task ActivateRegularSyncAsync();

        Task<bool> SyncAsync();
    }
}
