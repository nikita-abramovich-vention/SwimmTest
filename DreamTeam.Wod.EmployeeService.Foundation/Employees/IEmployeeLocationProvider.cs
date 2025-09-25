using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public interface IEmployeeLocationProvider
    {
        string GetLocation(Employee employee);
    }
}