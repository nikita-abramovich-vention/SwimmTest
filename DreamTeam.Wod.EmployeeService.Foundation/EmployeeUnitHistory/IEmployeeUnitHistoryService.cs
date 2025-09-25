using System.Collections.Generic;
using System.Threading.Tasks;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;

public interface IEmployeeUnitHistoryService
{
    Task<IReadOnlyCollection<DomainModel.EmployeeUnitHistory>> GetAllAsync();

    Task<IReadOnlyCollection<DomainModel.EmployeeUnitHistory>> GetByEmployeeIdAsync(string employeeId);
}