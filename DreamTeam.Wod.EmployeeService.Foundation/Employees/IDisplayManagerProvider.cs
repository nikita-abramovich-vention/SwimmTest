using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public interface IDisplayManagerProvider
{
    Task InitializeAsync();

    string GetDisplayManagerId(Employee employee);
}
