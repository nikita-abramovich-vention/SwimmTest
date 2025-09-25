using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Core.TimeTrackingService.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public interface IEmployeeService : IEmployeeObservable
    {
        event AsyncObserver<EmployeeChangingEventArgs> EmployeeCreating;

        event AsyncObserver<EmployeeChangingEventArgs> EmployeeUpdating;

        event AsyncObserver<EmployeeChangedEventArgs> EmployeeCreated;

        event AsyncObserver<EmployeeChangedEventArgs> EmployeeUpdated;


        Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(bool shouldIncludeInactive, bool shouldIncludeInterns = false);

        Task<IReadOnlyCollection<Employee>> GetEmployeesAsync(bool shouldIncludeInactive, IEmployeeServiceUnitOfWork uow, bool shouldIncludeInterns = false);

        Task<IReadOnlyCollection<Employee>> GetEmployeesWithRoleAsync(string roleId);

        Task<IReadOnlyCollection<Employee>> GetEmployeesByIdsAsync(
            IReadOnlyCollection<string> ids,
            bool shouldIncludeInactive = false,
            IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<Employee>> GetEmployeesByPeopleIdsAsync(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false, IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<Employee>> GetEmployeesByUnitIdsAsync(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive);

        Task<IReadOnlyCollection<Employee>> GetEmployeesByTitleRoleIdsAsync(IReadOnlyCollection<string> titleRoleIds, bool shouldIncludeInactive = false, IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<Employee>> GetEmployeesByRoleParametersAsync(
            IReadOnlyCollection<RoleConfigurationTitleRole> titleRoles,
            IReadOnlyCollection<RoleConfigurationUnit> units,
            IReadOnlyCollection<RoleConfigurationEmployee> employees);

        Task<IReadOnlyDictionary<string, Employee>> GetSmgToEmployeesMapAsync(IEmployeeServiceUnitOfWork uow);

        void CreateEmployees(IReadOnlyCollection<Employee> employees, IEmployeeServiceUnitOfWork uow);

        Task<Employee> GetEmployeeByPersonIdAsync(string personId);

        Task<Employee> GetEmployeeByIdAsync(string id);

        Task UpdateEmployeeRolesAsync(Employee employee, IReadOnlyCollection<EmployeeRole> fromEmployeeRoles);

        Task UpdateEmployeePartiallyAsync(Employee employee, Employee fromEmployee);

        Task AddOrUpdateEmployeeProfileAsync(Employee employee);

        Task UpdateEmployeeMaternityLeaveStateAsync(
            Employee employee,
            MaternityLeaveDataContract maternityLeave,
            MaternityLeaveDataContract previousMaternityLeave,
            IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<EmployeeOrganizationChange>> GetEmployeeOrganizationChanges(string employeeId);

        Task<IReadOnlyCollection<EmployeeCurrentLocationChange>> GetEmployeeCurrentLocationChanges(string employeeId);

        Task CreateOrUpdateEmployeeFromSmgProfileAsync(PersonDataContract person, SmgProfileDataContract smgProfile, bool shouldCreateEmployee = false);
    }
}