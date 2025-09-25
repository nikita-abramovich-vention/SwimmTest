using System;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement;
using DreamTeam.Wod.EmployeeService.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class FixDuplicateInternships : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IInternshipService _internshipService;
        private readonly IProfileService _profileService;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public string Name => "FixDuplicateInternships";

        public DateTime CreationDate => new DateTime(2022, 05, 16, 12, 11, 11);


        public FixDuplicateInternships(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IInternshipService internshipService,
            IProfileService profileService,
            IEnvironmentInfoService environmentInfoService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _internshipService = internshipService;
            _profileService = profileService;
            _environmentInfoService = environmentInfoService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var internshipRepository = uow.Internships;
                var internships = await _internshipService.GetInternshipsAsync(false, uow);
                var peopleIds = internships.Select(i => i.PersonId).ToList();
                var people = await _profileService.GetPeopleByIdsAsync(peopleIds);

                var duplicateInternshipsByDomainName = internships
                    .GroupBy(i => i.DomainName, StringComparer.InvariantCultureIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => new
                    {
                        DomainName = g.Key,
                        Internships = g.Select(i => new
                        {
                            Internship = i,
                            Person = people.SingleOrDefault(p => p.Id == i.PersonId),
                        }),
                    });

                var internshipsToDelete = duplicateInternshipsByDomainName
                    .SelectMany(i => i.Internships
                        .OrderByDescending(i => i.Person?.UpdateDate)
                        .ThenByDescending(i => i.Internship.EndDate)
                        .Skip(1)
                        .Select(i => i.Internship));

                foreach (var internship in internshipsToDelete)
                {
                    await _profileService.DeleteProfileAsync(internship.PersonId, ProfileTypes.Intern);
                    await _profileService.DeletePersonAsync(internship.PersonId);

                    internshipRepository.Delete(internship);
                }

                await uow.SaveChangesAsync();
            }
        }
    }
}