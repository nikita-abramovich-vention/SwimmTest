using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Roles
{
    public interface IRoleService
    {
        event AsyncObserver<EmployeeChangedEventArgs> EmployeeUpdated;


        void Initialize(IWodObservable wodObservable);

        Task<IReadOnlyCollection<Role>> GetRolesAsync(IEmployeeServiceUnitOfWork uow = null);

        Task<Role> GetRoleByIdAsync(string id);

        Task<EntityManagementResult<Role, RoleManagementError>> CreateRoleAsync(Role role);

        Task<EntityManagementResult<Role, RoleManagementError>> DeleteRoleAsync(Role role);

        Task<EntityManagementResult<RoleConfiguration, RoleManagementError>> CreateRoleWithConfigurationAsync(RoleConfiguration roleConfiguration);

        Task<EntityManagementResult<Role, RoleManagementError>> UpdateRoleAsync(Role role);

        Task<IReadOnlyCollection<RoleConfiguration>> GetRoleConfigurationsAsync(RoleType? type = null, IEmployeeServiceUnitOfWork uow = null);

        Task<RoleConfiguration> GetRoleConfigurationByIdAsync(string id);

        Task<EntityManagementResult<RoleConfiguration, RoleManagementError>> UpdateRoleConfigurationAsync(RoleConfiguration configuration, RoleConfiguration fromConfiguration);

        bool CheckIfEmployeeHasRole(Employee employee, RoleConfiguration roleConfiguration, IReadOnlyDictionary<string, UnitDataContract> unitMap);

        IReadOnlyCollection<EmployeeRole> GetEmployeeRoles(Employee employee, IReadOnlyCollection<RoleConfiguration> roleConfigurations);
    }
}