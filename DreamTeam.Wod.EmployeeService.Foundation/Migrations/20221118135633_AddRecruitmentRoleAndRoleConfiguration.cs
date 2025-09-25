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
    public sealed class AddRecruitmentRoleAndRoleConfiguration : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;


        public string Name => "AddRecruitmentRoleAndRoleConfiguration";

        public DateTime CreationDate => new DateTime(2022, 11, 18, 13, 56, 33);


        public AddRecruitmentRoleAndRoleConfiguration(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var roleRepository = uow.GetRepository<Role>();
                var roleConfigurationRepository = uow.GetRepository<RoleConfiguration>();

                var existingRoleConfiguration = await roleConfigurationRepository.GetSingleOrDefaultAsync(c => c.Role.ExternalId == Role.BuiltIn.Recruiter);
                if (existingRoleConfiguration != null)
                {
                    return;
                }

                var role = await roleRepository.GetSingleAsync(r => r.ExternalId == Role.BuiltIn.Recruiter);

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