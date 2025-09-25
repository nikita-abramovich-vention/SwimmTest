using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddCorporateDevelopmentManagerRoleConfiguration : IMigration
    {
        private readonly IRoleService _roleService;
        private readonly IEmployeeService _employeeService;
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;


        public string Name => "AddCorporateDevelopmentManagerRoleConfiguration";

        public DateTime CreationDate => new DateTime(2022, 06, 13, 10, 05, 33);


        public AddCorporateDevelopmentManagerRoleConfiguration(
            IRoleService roleService,
            IEmployeeService employeeService,
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory)
        {
            _roleService = roleService;
            _employeeService = employeeService;
            _unitOfWorkFactory = unitOfWorkFactory;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();
                var roleRepository = uow.GetRepository<Role>();
                var titleRoleRepository = uow.GetRepository<TitleRole>();

                var existingRoleConfiguration = await roleConfigurationRepository.GetSingleOrDefaultAsync(c => c.Role.ExternalId == Role.BuiltIn.CorporateDevelopmentManager);
                if (existingRoleConfiguration != null)
                {
                    return;
                }

                var role = await roleRepository.GetSingleAsync(r => r.ExternalId == Role.BuiltIn.CorporateDevelopmentManager);
                var titleRole = await titleRoleRepository.GetSingleAsync(tr => tr.ExternalId == TitleRole.BuiltIn.CorporateDevelopmentManager);
                var roleConfiguration = new RoleConfiguration
                {
                    Role = role,
                    TitleRoles = new[] { new RoleConfigurationTitleRole { TitleRole = titleRole } },
                    Units = new List<RoleConfigurationUnit>(0),
                    Employees = new List<RoleConfigurationEmployee>(0),
                };
                roleConfigurationRepository.Add(roleConfiguration);

                var employeeRepository = uow.GetRepository<Employee>();
                var employeeLoadStrategy = new EntityLoadStrategy<Employee>(
                    e => e.TitleRole,
                    e => e.Roles.Select(er => er.Role),
                    e => e.Seniority,
                    e => e.Workplaces.Select(ew => ew.Workplace));
                var specification = EmployeeSpecification.EmployeeNotInternship;
                var employees = await employeeRepository.GetWhereAsync(specification, employeeLoadStrategy);

                var emptyUnitsByIdMap = new Dictionary<string, UnitDataContract>(0);
                foreach (var employee in employees)
                {
                    if (_roleService.CheckIfEmployeeHasRole(employee, roleConfiguration, emptyUnitsByIdMap))
                    {
                        var employeeRole = new EmployeeRole { RoleId = roleConfiguration.Role.Id, Role = roleConfiguration.Role };
                        if (!employee.Roles.Any(er => er.RoleId == employeeRole.RoleId))
                        {
                            employee.Roles.Add(employeeRole);
                            await _employeeService.AddOrUpdateEmployeeProfileAsync(employee);
                        }
                    }
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}