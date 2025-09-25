using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Foundation;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;

public interface IEmployeeUnitHistorySyncService
{
    event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryCreated;

    event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryUpdated;

    event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryDeleted;


    Task ActivateRegularSyncAsync();

    Task<bool> SyncAsync();
}