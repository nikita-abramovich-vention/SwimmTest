using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DreamTeam.BackgroundJobs;
using DreamTeam.Common;
using DreamTeam.Common.Dates;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.DomainModel;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Microservices;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Internships
{
    [UsedImplicitly]
    public sealed class InternshipService : IInternshipService
    {
        private const int InternshipEndDateMonthThreshold = 3;


        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IProfileService _profileService;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmployeeServiceConfiguration _employeeServiceConfiguration;
        private readonly IUnitProvider _unitProvider;
        private readonly ISmgProfileMapper _smgProfileMapper;
        private readonly IDomainNameService _domainNameService;


        public event AsyncObserver<InternshipChangedEventArgs> InternshipCreated;

        public event AsyncObserver<InternshipChangedEventArgs> InternshipUpdated;


        public InternshipService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IProfileService profileService,
            IEnvironmentInfoService environmentInfoService,
            IJobScheduler jobScheduler,
            IEmployeeServiceConfiguration employeeServiceConfiguration,
            IUnitProvider unitProvider,
            ISmgProfileMapper smgProfileMapper,
            IDomainNameService domainNameService)
        {
            _uowProvider = uowProvider;
            _profileService = profileService;
            _environmentInfoService = environmentInfoService;
            _jobScheduler = jobScheduler;
            _employeeServiceConfiguration = employeeServiceConfiguration;
            _unitProvider = unitProvider;
            _smgProfileMapper = smgProfileMapper;
            _domainNameService = domainNameService;
        }


        public async Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var internships = await GetInternshipsAsync(shouldIncludeInactive, uow);

            return internships;
        }

        public async Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(bool shouldIncludeInactive, IEmployeeServiceUnitOfWork uow)
        {
            var internshipRepository = uow.GetRepository<Internship>();
            var internships = await internshipRepository.GetWhereAsync(i => i.IsActive || shouldIncludeInactive);

            return internships;
        }

        public async Task<IReadOnlyCollection<UnitInternshipsCount>> GetUnitInternshipCountsAsync()
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.Internships;
            var counts = await internshipRepository.GetUnitInternshipCountsAsync();

            return counts;
        }

        public async Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(IReadOnlyCollection<string> ids, bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.Internships;

            Specification<Internship> specification = new InternshipByIdsSpecification(ids);
            if (!shouldIncludeInactive)
            {
                specification &= new ActiveInternshipSpecification();
            }
            var internships = await internshipRepository.GetWhereAsync(specification);

            return internships;
        }

        public async Task<PaginatedItems<Internship>> GetInternshipsPaginatedAsync(
            IReadOnlyCollection<string> ids,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.Internships;

            Specification<Internship> specification = new InternshipByIdsSpecification(ids);
            if (!shouldIncludeInactive)
            {
                specification &= new ActiveInternshipSpecification();
            }
            var paginatedInternships = await internshipRepository.GetInternshipsPaginatedAsync(fromDate, toDate, direction, specification);

            return paginatedInternships;
        }

        public async Task<IReadOnlyCollection<Internship>> GetInternshipsByPersonIdAsync(string personId, bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var internships = await internshipRepository.GetWhereAsync(i => i.PersonId == personId && (i.IsActive || shouldIncludeInactive));

            return internships;
        }

        public async Task<IReadOnlyCollection<Internship>> GetInternshipsByUnitIdsAsync(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();

            var filter = unitIds.Count > 1
                ? (Specification<Internship>)new InternshipByUnitIdsSpecification(unitIds)
                : new InternshipByUnitIdSpecification(unitIds.Single());
            if (!shouldIncludeInactive)
            {
                filter &= new ActiveInternshipSpecification();
            }

            var internships = await internshipRepository.GetWhereAsync(filter);

            return internships;
        }

        public async Task<IReadOnlyCollection<Internship>> GetInternshipsByPeopleIdsAsync(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();

            Specification<Internship> filter = new InternshipByPeopleIdsSpecification(peopleIds);
            if (!shouldIncludeInactive)
            {
                filter &= new ActiveInternshipSpecification();
            }

            var internships = await internshipRepository.GetWhereAsync(filter);

            return internships;
        }

        public async Task<PaginatedItems<Internship>> GetInternshipsByPeopleIdsPaginatedAsync(
            IReadOnlyCollection<string> peopleIds,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.Internships;

            Specification<Internship> specification = new InternshipByPeopleIdsSpecification(peopleIds);
            if (!shouldIncludeInactive)
            {
                specification &= new ActiveInternshipSpecification();
            }
            var paginatedInternships = await internshipRepository.GetInternshipsPaginatedAsync(fromDate, toDate, direction, specification);

            return paginatedInternships;
        }

        public async Task<Internship> GetInternshipByIdAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var internship = await internshipRepository.GetSingleOrDefaultAsync(i => i.ExternalId == id);

            return internship;
        }

        public async Task<bool> CheckIfDomainNameIsTakenAsync(string domainName)
        {
            var isDomainNameTaken = await CheckIfDomainNameIsTakenAsync(domainName, null);

            return isDomainNameTaken;
        }

        public async Task<Internship> GetLastInternshipByDomainNameAsync(string domainName)
        {
            var uow = _uowProvider.CurrentUow;
            var internship = await uow.Internships.GetLastInternshipByDomainNameAsync(domainName);

            return internship;
        }

        public async Task<Internship> GetLastInternshipByPersonIdAsync(string personId)
        {
            var uow = _uowProvider.CurrentUow;
            var internship = await uow.Internships.GetLastInternshipByPersonIdAsync(personId);

            return internship;
        }

        public async Task<EntityManagementResult<Internship, InternshipManagementError>> CreateInternshipAsync(Internship internship)
        {
            if (String.IsNullOrEmpty(internship.FirstNameLocal) || String.IsNullOrEmpty(internship.LastNameLocal))
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.LocalNameRequired });
            }

            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var isInternshipAlreadyExists = await internshipRepository.AnyAsync(i => i.PersonId == internship.PersonId && i.IsActive);
            if (isInternshipAlreadyExists)
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.InternshipAlreadyExists });
            }

            var isDomainNameTaken = await CheckIfDomainNameIsTakenAsync(internship.DomainName);
            if (isDomainNameTaken)
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.DomainNameIsTaken });
            }
            if (!ValidateDomainName(internship))
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.InvalidDomainName });
            }

            internship.ExternalId = Guid.NewGuid().ToString("N");
            internship.IsActive = true;
            internship.CreationDate = _environmentInfoService.CurrentUtcDateTime;
            internshipRepository.Add(internship);

            await uow.SaveChangesAsync();

            ScheduleCloseActiveInternship(internship);

            await AddOrUpdateInternProfileAsync(internship);

            return EntityManagementResult<Internship, InternshipManagementError>.CreateSuccessful(internship);
        }

        public async Task<OperationResult<Internship>> CreateInternshipFromSmgInternProfileAsync(PersonDataContract person, SmgInternProfileDataContract smgInternProfile)
        {
            LoggerContext.Current.Log($"Creating internship for person {{personId}} ({person.FirstName} {person.LastName})...", person.Id);
            var internship = _smgProfileMapper.CreateInternshipFrom(person, smgInternProfile);

            internship.UnitId = _unitProvider.GetUnitIdByImportId(smgInternProfile.UnitId);
            var domainNameGenerator = await _domainNameService.CreateDomainNameGeneratorAsync();
            var (domainName, isVerified) = domainNameGenerator(internship);
            internship.DomainName = domainName;
            internship.IsDomainNameVerified = isVerified;
            internship.Email = _domainNameService.GenerateEmail(internship.DomainName);
            internship.EndDate = internship.StartDate.AddMonths(InternshipEndDateMonthThreshold);
            internship.IsActive = internship.EndDate >= _environmentInfoService.CurrentUtcDate;

            var updatePersonResult = await _profileService.UpdatePersonEmailAsync(person.Id, internship.Email);
            if (!updatePersonResult.IsSuccessful)
            {
                LoggerContext.Current.LogError($"Failed to update internship email {{internshipId}} due to service error {updatePersonResult.ErrorCodes.JoinStrings()}.", internship.ExternalId);
                throw new ArgumentException($"Failed to update internship email {internship.ExternalId} due to service error {updatePersonResult.ErrorCodes.JoinStrings()}.");
            }

            var createResult = await CreateInternshipAsync(internship);
            if (!createResult.IsSuccessful)
            {
                LoggerContext.Current.LogError($"Failed creating internship for person {{personId}} ({person.FirstName} {person.LastName}) because of {createResult.Errors}.", person.Id);

                return OperationResult<Internship>.CreateUnsuccessful();
            }

            LoggerContext.Current.Log("Internship {internshipId} created successfully.", internship.ExternalId);
            if (internship.IsActive)
            {
                await InternshipCreated.RaiseAsync(new InternshipChangedEventArgs(internship));
            }

            return internship;
        }

        public async Task<EntityManagementResult<Internship, InternshipManagementError>> UpdateInternshipAsync(Internship internship, Internship fromInternship, IEmployeeServiceUnitOfWork uow)
        {
            var effectiveUow = uow ?? _uowProvider.CurrentUow;

            if (internship.DomainName != fromInternship.DomainName)
            {
                if (!ValidateDomainName(fromInternship))
                {
                    return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.InvalidDomainName });
                }

                var isDomainNameTaken = await CheckIfDomainNameIsTakenAsync(fromInternship.DomainName, internship.Id);
                if (isDomainNameTaken)
                {
                    return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.DomainNameIsTaken });
                }
            }

            if (!internship.IsActive && fromInternship.IsActive)
            {
                var isInternshipCanBeReopened = await CheckIfInternshipCanBeReopenedAsync(internship);
                if (!isInternshipCanBeReopened)
                {
                    return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.DomainNameIsTaken });
                }
            }

            if (String.IsNullOrEmpty(fromInternship.FirstNameLocal) && !String.IsNullOrEmpty(fromInternship.LastNameLocal) ||
                !String.IsNullOrEmpty(fromInternship.FirstNameLocal) && String.IsNullOrEmpty(fromInternship.LastNameLocal))
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.LocalNameRequired });
            }

            if (internship.IsActive == fromInternship.IsActive && (String.IsNullOrEmpty(fromInternship.FirstNameLocal) || String.IsNullOrEmpty(fromInternship.LastNameLocal)))
            {
                return EntityManagementResult<Internship, InternshipManagementError>.CreateUnsuccessful(new[] { InternshipManagementError.LocalNameRequired });
            }

            if (internship.IsActive && !fromInternship.IsActive)
            {
                internship.CloseReason = InternshipCloseReason.Manually;
            }

            internship.IsActive = fromInternship.IsActive;
            internship.FirstName = fromInternship.FirstName;
            internship.LastName = fromInternship.LastName;
            internship.FirstNameLocal = fromInternship.FirstNameLocal;
            internship.LastNameLocal = fromInternship.LastNameLocal;
            internship.PhotoId = fromInternship.PhotoId;
            internship.Skype = fromInternship.Skype;
            internship.Telegram = fromInternship.Telegram;
            internship.Phone = fromInternship.Phone;
            internship.Email = fromInternship.Email;
            internship.Location = fromInternship.Location;
            internship.DomainName = fromInternship.DomainName;
            internship.IsDomainNameVerified = true;
            internship.UnitId = fromInternship.UnitId;
            internship.StartDate = fromInternship.StartDate;
            internship.EndDate = fromInternship.EndDate;
            internship.UpdatedBy = fromInternship.UpdatedBy;
            internship.UpdateDate = _environmentInfoService.CurrentUtcDateTime;

            if (uow is null)
            {
                await effectiveUow.SaveChangesAsync();
            }

            ScheduleCloseActiveInternship(internship);

            await AddOrUpdateInternProfileAsync(internship);

            return EntityManagementResult<Internship, InternshipManagementError>.CreateSuccessful(internship);
        }

        public async Task<Internship> DeleteInternshipAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var internship = await internshipRepository.GetSingleOrDefaultAsync(i => i.ExternalId == id);
            if (internship == null)
            {
                return null;
            }

            internshipRepository.Delete(internship);
            await uow.SaveChangesAsync();

            await _profileService.DeleteProfileAsync(internship.PersonId, ProfileTypes.Intern);

            return internship;
        }

        public async Task AddOrUpdateInternProfileAsync(Internship internship)
        {
            var internProfile = new InternProfileDataContract
            {
                Id = internship.PersonId,
                Type = ProfileTypes.Intern,
                IsActive = internship.IsActive,
                DomainName = internship.DomainName,
                Email = internship.Email,
                UnitId = internship.UnitId,
                InternshipId = internship.ExternalId,
                Location = internship.Location,
            };
            var result = await _profileService.AddOrUpdateProfileAsync(internship.PersonId, internProfile);
            if (!result.IsSuccessful)
            {
                LoggerContext.Current.LogError($"Failed to add or update internship profile {{internshipId}} due to service error {result.ErrorCodes.JoinStrings()}.", internship.ExternalId);
                throw new ArgumentException($"Failed to add or update internship profile {internship.ExternalId} due to service error {result.ErrorCodes.JoinStrings()}.");
            }
        }

        [UsedImplicitly]
        public async Task CloseActiveInternshipAsync(string internshipExternalId)
        {
            var internship = await GetInternshipByIdAsync(internshipExternalId);
            if (internship == null || !internship.IsActive)
            {
                return;
            }

            var closeAfterDate = GetCloseAfterDate(internship);
            if (closeAfterDate <= _environmentInfoService.CurrentUtcDate)
            {
                await CloseInternshipAsync(internship, InternshipCloseReason.AutomaticallyDueInactivity);
            }
        }

        public void ScheduleCloseActiveInternship(Internship internship)
        {
            if (!internship.IsActive)
            {
                return;
            }

            var closeAfterDate = GetCloseAfterDate(internship);
            _jobScheduler.ScheduleJob<InternshipService>(
                s => s.CloseActiveInternshipAsync(internship.ExternalId),
                closeAfterDate.ToDateTime());
        }

        public async Task CloseInternshipAsync(Internship internship, InternshipCloseReason closeReason)
        {
            var currentInternship = internship.Clone();
            internship.IsActive = false;
            internship.UpdateDate = _environmentInfoService.CurrentUtcDateTime;
            internship.UpdatedBy = null;
            internship.CloseReason = closeReason;

            var uow = _uowProvider.CurrentUow;
            await uow.SaveChangesAsync();

            await AddOrUpdateInternProfileAsync(internship);

            await InternshipUpdated.RaiseAsync(new InternshipChangedEventArgs(internship, currentInternship));
        }


        private async Task<bool> CheckIfDomainNameIsTakenAsync(string domainName, int? allowInternshipId)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var employeeRepository = uow.GetRepository<Employee>();
            var isDomainNameTaken = await internshipRepository.AnyAsync(i => i.DomainName == domainName && i.IsActive && (!allowInternshipId.HasValue || i.Id != allowInternshipId.Value))
                                    || await employeeRepository.AnyAsync(e => e.DomainName == domainName);

            return isDomainNameTaken;
        }

        private async Task<bool> CheckIfInternshipCanBeReopenedAsync(Internship internship)
        {
            var uow = _uowProvider.CurrentUow;
            var internshipRepository = uow.GetRepository<Internship>();
            var employeeRepository = uow.GetRepository<Employee>();

            var isInternshipCanBeReopened =
                !await internshipRepository.AnyAsync(i => i.DomainName == internship.DomainName && i.IsActive) &&
                !await employeeRepository.AnyAsync(e => e.DomainName == internship.DomainName && e.PersonId != internship.PersonId);

            return isInternshipCanBeReopened;
        }

        private DateOnly GetCloseAfterDate(Internship internship)
        {
            var lastChangeDate = internship.UpdateDate ?? internship.CreationDate;
            var maxInternshipDate = DateOnlyMath.GetMaxDate(internship.EndDate, lastChangeDate.ToDateOnly());

            return maxInternshipDate.AddDays(_employeeServiceConfiguration.AutomaticallyCloseInternshipInDays);
        }

        private static bool ValidateDomainName(Internship internship)
        {
            return Regex.IsMatch(internship.DomainName, Internship.DomainNameRegex);
        }



        private sealed class InternshipByIdsSpecification : Specification<Internship>
        {
            public InternshipByIdsSpecification(IReadOnlyCollection<string> ids)
                : base(i => ids.Contains(i.ExternalId))
            {
            }
        }

        private sealed class InternshipByUnitIdSpecification : Specification<Internship>
        {
            public InternshipByUnitIdSpecification(string unitId)
                : base(i => i.UnitId == unitId)
            {
            }
        }

        private sealed class InternshipByPeopleIdsSpecification : Specification<Internship>
        {
            public InternshipByPeopleIdsSpecification(IReadOnlyCollection<string> peopleIds)
                : base(i => peopleIds.Contains(i.PersonId))
            {
            }
        }

        private sealed class InternshipByUnitIdsSpecification : Specification<Internship>
        {
            public InternshipByUnitIdsSpecification(IReadOnlyCollection<string> unitIds)
                : base(i => unitIds.Contains(i.UnitId))
            {
            }
        }

        private sealed class ActiveInternshipSpecification : Specification<Internship>
        {
            public ActiveInternshipSpecification()
                : base(i => i.IsActive)
            {
            }
        }
    }
}