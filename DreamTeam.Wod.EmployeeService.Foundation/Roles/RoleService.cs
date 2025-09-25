using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.EqualityComparison;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Roles
{
    [UsedImplicitly]
    public sealed class RoleService : IRoleService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IEmployeeService _employeeService;
        private readonly IUnitProvider _unitProvider;


        public event AsyncObserver<EmployeeChangedEventArgs> EmployeeUpdated;


        public RoleService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService,
            IEmployeeService employeeService,
            IUnitProvider unitProvider)
        {
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;
            _employeeService = employeeService;
            _unitProvider = unitProvider;

            _employeeService.EmployeeCreating += OnEmployeeCreatingOrUpdating;
            _employeeService.EmployeeUpdating += OnEmployeeCreatingOrUpdating;
        }


        public void Initialize(IWodObservable wodObservable)
        {
            wodObservable.UnitChanged += OnUnitChangedAsync;
        }

        public async Task<IReadOnlyCollection<Role>> GetRolesAsync(IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var roleRepository = currentUow.GetRepository<Role>();
            var roles = await roleRepository.GetAllAsync();

            return roles;
        }

        public async Task<Role> GetRoleByIdAsync(string id)
        {
            var roleRepository = _uowProvider.CurrentUow.GetRepository<Role>();
            var role = await roleRepository.GetSingleOrDefaultAsync(r => r.ExternalId == id);

            return role;
        }

        public async Task<EntityManagementResult<Role, RoleManagementError>> CreateRoleAsync(Role role)
        {
            var uow = _uowProvider.CurrentUow;
            var roleRepository = uow.GetRepository<Role>();
            var roleWithTheSameNameOrId = await roleRepository.GetSingleOrDefaultAsync(r => r.Name == role.Name) ??
                                          await roleRepository.GetSingleOrDefaultAsync(r => r.ExternalId == role.ExternalId);
            if (roleWithTheSameNameOrId != null)
            {
                return EntityManagementResult<Role, RoleManagementError>.CreateUnsuccessful(new[] { RoleManagementError.RoleAlreadyExists });
            }

            roleRepository.Add(role);
            await uow.SaveChangesAsync();

            return EntityManagementResult<Role, RoleManagementError>.CreateSuccessful(role);
        }

        public async Task<EntityManagementResult<Role, RoleManagementError>> DeleteRoleAsync(Role role)
        {
            if (role.IsBuiltIn)
            {
                return RoleManagementError.BuiltInRoleDeletionError;
            }

            var uow = _uowProvider.CurrentUow;
            var roleRepository = uow.GetRepository<Role>();

            roleRepository.Delete(role);
            await uow.SaveChangesAsync();

            return role;
        }

        public async Task<EntityManagementResult<RoleConfiguration, RoleManagementError>> CreateRoleWithConfigurationAsync(RoleConfiguration roleConfiguration)
        {
            if (roleConfiguration.Role.IsBuiltIn)
            {
                return RoleManagementError.BuiltInRoleCreationError;
            }

            var uow = _uowProvider.CurrentUow;
            var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();

            var roleWithTheSameName = await roleConfigurationRepository.GetSingleOrDefaultAsync(rc => rc.Role.Name == roleConfiguration.Role.Name);

            if (roleWithTheSameName != null)
            {
                return RoleManagementError.RoleAlreadyExists;
            }

            roleConfiguration.Role.CreationDate = _environmentInfoService.CurrentUtcDateTime;

            roleConfigurationRepository.Add(roleConfiguration);

            var employeesToUpdate = await _employeeService.GetEmployeesByRoleParametersAsync(roleConfiguration.TitleRoles.ToList(), roleConfiguration.Units.ToList(), roleConfiguration.Employees.ToList());
            var clonedEmployees = UpdateEmployeeRoles(employeesToUpdate, roleConfiguration);

            await uow.SaveChangesAsync();

            await RaiseAndUpdateEmployeesChangesAsync(employeesToUpdate, clonedEmployees);

            return EntityManagementResult<RoleConfiguration, RoleManagementError>.CreateSuccessful(roleConfiguration);
        }

        public async Task<EntityManagementResult<Role, RoleManagementError>> UpdateRoleAsync(Role role)
        {
            var uow = _uowProvider.CurrentUow;
            var roleRepository = uow.GetRepository<Role>();
            var roleWithTheSameName = await roleRepository.GetSingleOrDefaultAsync(r => r.Name == role.Name);
            if (roleWithTheSameName != null && roleWithTheSameName.Id != role.Id)
            {
                return EntityManagementResult<Role, RoleManagementError>.CreateUnsuccessful(new[] { RoleManagementError.RoleAlreadyExists });
            }

            await uow.SaveChangesAsync();

            return EntityManagementResult<Role, RoleManagementError>.CreateSuccessful(role);
        }

        public async Task<IReadOnlyCollection<RoleConfiguration>> GetRoleConfigurationsAsync(RoleType? type = null, IEmployeeServiceUnitOfWork uow = null)
        {
            var unitOfWork = uow ?? _uowProvider.CurrentUow;
            var roleConfigurationRepository = unitOfWork.GetRepository<RoleConfiguration>();
            var loadStrategy = GetRoleConfigurationLoadStrategy();

            switch (type)
            {
                case RoleType.BuiltIn:
                {
                    var builtInRoleConfigurations = await roleConfigurationRepository.GetWhereAsync(r => r.Role.IsBuiltIn, loadStrategy);

                    return builtInRoleConfigurations;
                }
                case RoleType.Custom:
                {
                    var customRoleConfigurations = await roleConfigurationRepository.GetWhereAsync(r => !r.Role.IsBuiltIn, loadStrategy);

                    return customRoleConfigurations;
                }
                default:
                {
                    var roleConfigurations = await roleConfigurationRepository.GetAllAsync(loadStrategy);

                    return roleConfigurations;
                }
            }
        }

        public async Task<RoleConfiguration> GetRoleConfigurationByIdAsync(string id)
        {
            var roleConfigurationRepository = _uowProvider.CurrentUow.GetRepository<RoleConfiguration>();
            var loadStrategy = GetRoleConfigurationLoadStrategy();
            var roleConfiguration = await roleConfigurationRepository.GetSingleOrDefaultAsync(c => c.Role.ExternalId == id, loadStrategy);

            return roleConfiguration;
        }

        public async Task<EntityManagementResult<RoleConfiguration, RoleManagementError>> UpdateRoleConfigurationAsync(RoleConfiguration configuration, RoleConfiguration fromConfiguration)
        {
            if (configuration.Role.IsBuiltIn && CheckIfBuiltInRoleIsModifiedConstantFields(configuration.Role, fromConfiguration.Role))
            {
                return RoleManagementError.BuiltInRoleUpdatingError;
            }

            var uow = _uowProvider.CurrentUow;
            configuration.Role.Name = fromConfiguration.Role.Name;
            configuration.Role.Description = fromConfiguration.Role.Description;
            configuration.Role.RoleManagerId = fromConfiguration.Role.RoleManagerId;
            configuration.Role.UpdatedBy = fromConfiguration.UpdatedBy;
            configuration.Role.UpdateDate = _environmentInfoService.CurrentUtcDateTime;

            var newAndOldTitleRoles = configuration.TitleRoles
                .Concat(fromConfiguration.TitleRoles)
                .DistinctBy(e => e.TitleRoleId)
                .ToList();
            configuration.TitleRoles.Reconcile(fromConfiguration.TitleRoles, r => r.TitleRole.Id);

            var newAndOldUnits = configuration.Units
                .Concat(fromConfiguration.Units)
                .DistinctBy(e => e.UnitId)
                .ToList();
            configuration.Units.Reconcile(fromConfiguration.Units, u => u.UnitId);

            var (addedEmployees, removedEmployees) = configuration.Employees.Diff(fromConfiguration.Employees, r => r.Employee.Id);
            var addedAndRemovedEmployees = addedEmployees.Concat(removedEmployees).ToList();
            configuration.Employees.Reconcile(fromConfiguration.Employees, e => e.Employee.Id);
            configuration.UpdatedBy = fromConfiguration.UpdatedBy;
            configuration.UpdateDate = _environmentInfoService.CurrentUtcDateTime;

            var employeesToUpdate = await _employeeService.GetEmployeesByRoleParametersAsync(newAndOldTitleRoles, newAndOldUnits, addedAndRemovedEmployees);
            var clonedEmployees = UpdateEmployeeRoles(employeesToUpdate, configuration);

            await uow.SaveChangesAsync();

            await RaiseAndUpdateEmployeesChangesAsync(employeesToUpdate, clonedEmployees);

            return configuration;
        }

        public IReadOnlyCollection<EmployeeRole> GetEmployeeRoles(Employee employee, IReadOnlyCollection<RoleConfiguration> roleConfigurations)
        {
            var unitMap = _unitProvider.GetUnitMap();
            var employeeRoles = roleConfigurations
                .Where(c => CheckIfEmployeeHasRole(employee, c, unitMap))
                .Select(c => new EmployeeRole { RoleId = c.Role.Id, Role = c.Role })
                .ToList();

            return employeeRoles;
        }


        private async Task OnUnitChangedAsync(ServiceEntityChanged<UnitDataContract> unitChanged)
        {
            if (unitChanged.ChangeType != ServiceEntityChangeType.Create || String.IsNullOrEmpty(unitChanged.NewValue.ParentUnitId))
            {
                return;
            }

            var parentUnitId = unitChanged.NewValue.ParentUnitId;
            var newUnitId = unitChanged.NewValue.Id;
            var siblingUnitIds = _unitProvider.GetSubUnits(parentUnitId, recursive: true)
                .Select(u => u.Id)
                .Where(id => id != newUnitId)
                .ToList();

            var uow = _uowProvider.CurrentUow;
            var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();
            var loadStrategy = GetRoleConfigurationWithUnitsLoadStrategy();
            var roleConfigurations = await roleConfigurationRepository.GetWhereAsync(r => r.Units.Any(u => u.UnitId == parentUnitId), loadStrategy);

            foreach (var roleConfiguration in roleConfigurations)
            {
                var roleConfigurationUnitIds = roleConfiguration.Units.Select(u => u.UnitId).ToHashSet();
                if (siblingUnitIds.All(roleConfigurationUnitIds.Contains))
                {
                    roleConfiguration.Units.Add(new RoleConfigurationUnit { UnitId = newUnitId });
                    LoggerContext.Current.Log("Added new unit {unitId} to role configuration {roleName} as a child of parent unit {parentUnitId}.", newUnitId, roleConfiguration.Role.Name, parentUnitId);
                }
            }

            await uow.SaveChangesAsync();
        }

        private async Task OnEmployeeCreatingOrUpdating(EmployeeChangingEventArgs e)
        {
            if (!EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(e.Employee))
            {
                return;
            }

            var uow = e.UnitOfWork;
            var employee = e.Employee;
            var previousEmployee = e.PreviousEmployee;
            var isRoleUpdateRequired = previousEmployee == null || CheckIfRolesUpdateIsRequired(employee, previousEmployee);

            if (isRoleUpdateRequired)
            {
                var roleConfigurations = await GetRoleConfigurationsAsync(uow: uow);
                var employeeRoles = GetEmployeeRoles(employee, roleConfigurations);
                employee.Roles.Reconcile(employeeRoles.ToList(), r => r.Role.Id);
            }
        }


        private async Task RaiseAndUpdateEmployeesChangesAsync(IReadOnlyCollection<Employee> employeesToUpdate, IReadOnlyDictionary<int, Employee> clonedEmployees)
        {
            var updatedEmployees = employeesToUpdate
                .Intersect(clonedEmployees.Values, EqualityComparerFactory<Employee>.FromKey(e => e.Id))
                .ToList();

            foreach (var employee in updatedEmployees)
            {
                var cloneEmployee = clonedEmployees[employee.Id];
                await EmployeeUpdated.RaiseAsync(new EmployeeChangedEventArgs(cloneEmployee, employee));
            }

            Task.Run(async () =>
            {
                try
                {
                    foreach (var employee in updatedEmployees)
                    {
                        await _employeeService.AddOrUpdateEmployeeProfileAsync(employee);
                    }
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogError("Failed to add or update employee profiles due to service error.", ex);
                }
            }).Forget();
        }

        private IReadOnlyDictionary<int, Employee> UpdateEmployeeRoles(
            IReadOnlyCollection<Employee> employeesToUpdate,
            RoleConfiguration updatedConfiguration)
        {
            var unitByIdMap = _unitProvider.GetUnitMap();
            var clonedEmployees = new Dictionary<int, Employee>();
            foreach (var employee in employeesToUpdate)
            {
                var needToAddRole = false;
                var needToRemoveRole = false;
                var shouldEmployeeHaveRole = CheckIfEmployeeHasRole(employee, updatedConfiguration, unitByIdMap);
                var doesEmployeeHaveRole = employee.Roles.Any(r => r.RoleId == updatedConfiguration.Role.Id);
                if (shouldEmployeeHaveRole && !doesEmployeeHaveRole)
                {
                    needToAddRole = true;
                }
                else if (!shouldEmployeeHaveRole && doesEmployeeHaveRole)
                {
                    needToRemoveRole = true;
                }

                if (!needToAddRole && !needToRemoveRole)
                {
                    continue;
                }

                var cloneEmployee = employee.Clone();
                clonedEmployees.Add(cloneEmployee.Id, cloneEmployee);

                if (needToAddRole)
                {
                    var employeeRole = new EmployeeRole { RoleId = updatedConfiguration.Role.Id, Role = updatedConfiguration.Role };
                    employee.Roles.Add(employeeRole);
                }

                if (needToRemoveRole)
                {
                    var employeeRole = employee.Roles.Single(r => r.RoleId == updatedConfiguration.Role.Id);
                    employee.Roles.Remove(employeeRole);
                }
            }

            return clonedEmployees;
        }

        public bool CheckIfEmployeeHasRole(Employee employee, RoleConfiguration roleConfiguration, IReadOnlyDictionary<string, UnitDataContract> unitMap)
        {
            if (roleConfiguration.Employees.Any(e => e.Employee.Id == employee.Id))
            {
                return true;
            }

            var managesRequiredUnit = roleConfiguration.Units.Any(u => CheckIfUnitIsManagedBy(employee, unitMap[u.UnitId]));
            if (managesRequiredUnit)
            {
                return true;
            }

            var hasRequiredTitleRole = roleConfiguration.TitleRoles.Any(ctr => employee.TitleRole != null && employee.TitleRole.Id == ctr.TitleRole.Id);
            var belongsToRequiredUnit = roleConfiguration.Units.Any(u => u.UnitId == employee.UnitId);

            if (hasRequiredTitleRole)
            {
                return roleConfiguration.IsAllUnits || belongsToRequiredUnit;
            }

            return belongsToRequiredUnit && roleConfiguration.IsAllTitleRoles;
        }


        private static bool CheckIfBuiltInRoleIsModifiedConstantFields(Role role, Role fromRole)
        {
            return role.Name != fromRole.Name || role.Description != fromRole.Description;
        }

        private static bool CheckIfUnitIsManagedBy(Employee employee, UnitDataContract unit)
        {
            return unit.ManagerId == employee.ExternalId || unit.DeputyIds.Contains(employee.ExternalId);
        }

        private static bool CheckIfRolesUpdateIsRequired(Employee employee, Employee previousEmployee)
        {
            return employee.UnitId != previousEmployee.UnitId ||
                   employee.TitleRole != previousEmployee.TitleRole;
        }

        private static IEntityLoadStrategy<RoleConfiguration> GetRoleConfigurationLoadStrategy()
        {
            return new EntityLoadStrategy<RoleConfiguration>(
                c => c.Role,
                c => c.TitleRoles.Select(r => r.TitleRole),
                c => c.Units,
                c => c.Employees.Select(e => e.Employee));
        }

        private static IEntityLoadStrategy<RoleConfiguration> GetRoleConfigurationWithUnitsLoadStrategy()
        {
            return new EntityLoadStrategy<RoleConfiguration>(c => c.Units, c => c.Role);
        }
    }
}
