using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Specification;
using DreamTeam.DomainModel;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IInternshipRepository : IRepository<Internship>
    {
        Task<Internship> GetLastInternshipByDomainNameAsync(string domainName);

        Task<Internship> GetLastInternshipByPersonIdAsync(string personId);

        Task<IReadOnlyCollection<UnitInternshipsCount>> GetUnitInternshipCountsAsync();

        Task<PaginatedItems<Internship>> GetInternshipsPaginatedAsync(
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            Specification<Internship> specification,
            IEntityLoadStrategy<Internship> loadStrategy = null);
    }
}