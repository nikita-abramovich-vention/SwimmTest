using System.Collections.Generic;
using System.Threading.Tasks;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public interface IWageRatePeriodService
{
    Task<IReadOnlyCollection<double>> GetAllWageRatesAsync();
}