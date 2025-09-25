using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Observable;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.DomainModel;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Internships
{
    public interface IInternshipService
    {
        event AsyncObserver<InternshipChangedEventArgs> InternshipCreated;
        event AsyncObserver<InternshipChangedEventArgs> InternshipUpdated;


        Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(bool shouldIncludeInactive);

        Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(bool shouldIncludeInactive, IEmployeeServiceUnitOfWork uow);

        Task<IReadOnlyCollection<UnitInternshipsCount>> GetUnitInternshipCountsAsync();

        Task<IReadOnlyCollection<Internship>> GetInternshipsAsync(IReadOnlyCollection<string> ids, bool shouldIncludeInactive);

        Task<PaginatedItems<Internship>> GetInternshipsPaginatedAsync(
            IReadOnlyCollection<string> ids,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<Internship>> GetInternshipsByPersonIdAsync(string personId, bool shouldIncludeInactive);

        Task<IReadOnlyCollection<Internship>> GetInternshipsByUnitIdsAsync(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive);

        Task<IReadOnlyCollection<Internship>> GetInternshipsByPeopleIdsAsync(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive);

        Task<PaginatedItems<Internship>> GetInternshipsByPeopleIdsPaginatedAsync(
            IReadOnlyCollection<string> peopleIds,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false);

        Task<Internship> GetInternshipByIdAsync(string id);

        Task<bool> CheckIfDomainNameIsTakenAsync(string domainName);

        Task<Internship> GetLastInternshipByDomainNameAsync(string domainName);

        Task<Internship> GetLastInternshipByPersonIdAsync(string personId);

        Task<EntityManagementResult<Internship, InternshipManagementError>> CreateInternshipAsync(Internship internship);

        Task<OperationResult<Internship>> CreateInternshipFromSmgInternProfileAsync(PersonDataContract person, SmgInternProfileDataContract smgInternProfile);

        Task<EntityManagementResult<Internship, InternshipManagementError>> UpdateInternshipAsync(Internship internship, Internship fromInternship, [CanBeNull] IEmployeeServiceUnitOfWork uow = null);

        Task<Internship> DeleteInternshipAsync(string id);

        void ScheduleCloseActiveInternship(Internship internship);

        Task CloseInternshipAsync(Internship internship, InternshipCloseReason closeReason);

        Task AddOrUpdateInternProfileAsync(Internship internship);
    }
}