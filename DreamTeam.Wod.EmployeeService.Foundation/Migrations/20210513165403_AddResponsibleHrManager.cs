using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Logging;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddResponsibleHrManager : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IEmployeeService _employeeService;
        private readonly IProfileService _profileService;


        public string Name => "AddResponsibleHrManager";

        public DateTime CreationDate => new DateTime(2021, 05, 13, 16, 54, 03);


        public AddResponsibleHrManager(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IEmployeeService employeeService,
            IProfileService profileService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _employeeService = employeeService;
            _profileService = profileService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var employees = await _employeeService.GetEmployeesAsync(true, uow);
                var peopleWithSmgProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgProfileDataContract>(ProfileTypes.Smg);
                foreach (var employee in employees)
                {
                    var personWithSmgProfile = peopleWithSmgProfiles.FirstOrDefault(p => p.Person.Id == employee.PersonId);
                    if (personWithSmgProfile != null && personWithSmgProfile.Profile.ResponsibleHrManagerId != null)
                    {
                        var responsibleHrManagerPersonWithSmgProfile = peopleWithSmgProfiles.SingleOrDefault(p => p.Profile.SmgId == personWithSmgProfile.Profile.ResponsibleHrManagerId);
                        if (responsibleHrManagerPersonWithSmgProfile == null)
                        {
                            LoggerContext.Current.LogWarning(
                                "Failed to get responsible hr manager person with smg profile by smg id {smgId} for employee {employeeId}.",
                                personWithSmgProfile.Profile.ResponsibleHrManagerId,
                                employee.ExternalId);

                            continue;
                        }

                        var responsibleHrManager = employees.SingleOrDefault(e => e.ExternalId == responsibleHrManagerPersonWithSmgProfile.Profile.Id);
                        if (responsibleHrManager == null)
                        {
                            LoggerContext.Current.LogWarning(
                                "Failed to get responsible hr manager employee by profile id {profileId} for employee {employeeId}.",
                                responsibleHrManagerPersonWithSmgProfile.Profile.Id,
                                employee.ExternalId);

                            continue;
                        }

                        employee.ResponsibleHrManager = responsibleHrManager;
                    }
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}
