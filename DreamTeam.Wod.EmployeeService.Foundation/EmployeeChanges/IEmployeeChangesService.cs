using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeChanges
{
    public interface IEmployeeChangesService
    {
        Task HandleEmployeeUpdateAsync(Employee previousEmployee, Employee employee, IEmployeeServiceUnitOfWork uow);
    }
}
