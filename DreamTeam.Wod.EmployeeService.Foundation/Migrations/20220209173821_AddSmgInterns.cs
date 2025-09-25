using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Microservices.Interfaces;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class AddSmgInterns : IMigration
    {
        private const int InternshipEndDateMonthThreshold = 4;


        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IInternshipService _internshipService;
        private readonly IProfileService _profileService;
        private readonly IDepartmentService _departmentService;
        private readonly IDomainNameService _domainNameService;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IEmployeeService _employeeService;
        private readonly ISmgProfileMapper _smgProfileMapper;


        public string Name => "AddSmgInterns";

        public DateTime CreationDate => new DateTime(2022, 02, 09, 17, 38, 21);


        public AddSmgInterns(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IInternshipService internshipService,
            IProfileService profileService,
            IDepartmentService departmentService,
            IDomainNameService domainNameService,
            IEnvironmentInfoService environmentInfoService,
            IEmployeeService employeeService,
            ISmgProfileMapper smgProfileMapper)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _internshipService = internshipService;
            _profileService = profileService;
            _departmentService = departmentService;
            _domainNameService = domainNameService;
            _environmentInfoService = environmentInfoService;
            _employeeService = employeeService;
            _smgProfileMapper = smgProfileMapper;

        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var units = await _departmentService.GetUnitsAsync();
                var peopleWithSmgInternProfiles = await _profileService.GetPeopleWithProfilesAsync<SmgInternProfileDataContract>(ProfileTypes.SmgIntern);
                var peopleWithActiveSmgInternProfiles = peopleWithSmgInternProfiles.Where(p => p.Profile.IsActive).ToList();
                var internshipMap = new Dictionary<string, Internship>();
                var updatedInternshipMap = new Dictionary<string, Internship>();
                var internships = await _internshipService.GetInternshipsAsync(false, uow);
                var employees = await _employeeService.GetEmployeesAsync(false, uow, true);
                var domainNameGenerator = await _domainNameService.CreateDomainNameGeneratorAsync(uow);

                var internshipRepository = uow.GetRepository<Internship>();
                var employeeRepository = uow.GetRepository<Employee>();

                foreach (var personWithSmgInternProfile in peopleWithActiveSmgInternProfiles)
                {
                    var person = personWithSmgInternProfile.Person;
                    var profile = personWithSmgInternProfile.Profile;

                    var employee = employees.SingleOrDefault(e => e.PersonId == person.Id);
                    if (employee != null)
                    {
                        continue;
                    }

                    var internship = internships.SingleOrDefault(i => i.PersonId == person.Id);
                    if (internship != null)
                    {
                        continue;
                    }

                    internship = _smgProfileMapper.CreateInternshipFrom(personWithSmgInternProfile.Person, profile);

                    var (domainName, isVerified) = domainNameGenerator(internship);
                    internship.DomainName = domainName;
                    internship.IsDomainNameVerified = isVerified;
                    internship.Email = _domainNameService.GenerateEmail(internship.DomainName);

                    var now = _environmentInfoService.CurrentUtcDateTime;
                    var unit = units.Single(u => u.ImportId == profile.UnitId);
                    internship.UnitId = unit.Id;
                    internship.EndDate = internship.StartDate.AddMonths(InternshipEndDateMonthThreshold);
                    internship.IsActive = internship.EndDate >= now.ToDateOnly();
                    internship.CreationDate = now;
                    internshipRepository.Add(internship);

                    internshipMap.Add(profile.SmgId, internship);
                }

                var addedOrUpdatedInternships = internshipMap.Values.ToList();
                foreach (var internship in addedOrUpdatedInternships)
                {
                    await _internshipService.AddOrUpdateInternProfileAsync(internship);
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}