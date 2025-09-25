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
    public sealed class AddProjectIterationMaintainerRoleConfiguration : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;


        public string Name => "AddProjectIterationMaintainerRoleConfiguration";

        public DateTime CreationDate => new DateTime(2022, 06, 30, 10, 05, 33);


        public AddProjectIterationMaintainerRoleConfiguration(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();
                var roleRepository = uow.GetRepository<Role>();

                var existingRoleConfiguration = await roleConfigurationRepository.GetSingleOrDefaultAsync(c => c.Role.ExternalId == Role.BuiltIn.ProjectIterationMaintainer);
                if (existingRoleConfiguration != null)
                {
                    return;
                }

                var role = await roleRepository.GetSingleAsync(r => r.ExternalId == Role.BuiltIn.ProjectIterationMaintainer);
                var roleConfiguration = new RoleConfiguration
                {
                    Role = role,
                    TitleRoles = new List<RoleConfigurationTitleRole>(0),
                    Units = new List<RoleConfigurationUnit>(0),
                    Employees = new List<RoleConfigurationEmployee>(0),
                };
                roleConfigurationRepository.Add(roleConfiguration);

                await uow.SaveChangesAsync();
            }
        }
    }
}