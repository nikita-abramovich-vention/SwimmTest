using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories;

public interface IExternalEmployeeUnitHistoryRepository : IRepository<ExternalEmployeeUnitHistory>
{
    Task<IReadOnlyCollection<string>> GetSourceIdsOfEmployeesWithMultipleActiveHistoryItemsAsync();
}