using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Specification;
using DreamTeam.DomainModel;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    [UsedImplicitly]
    public sealed class InternshipRepository : Repository<Internship>, IInternshipRepository
    {
        public InternshipRepository(IDbContext dbContext)
            : base(dbContext)
        {

        }


        public async Task<Internship> GetLastInternshipByDomainNameAsync(string domainName)
        {
            var lastInternship = await GetQuery().Where(i => i.DomainName == domainName).OrderBy(i => i.StartDate).LastOrDefaultAsync();

            return lastInternship;
        }

        public async Task<Internship> GetLastInternshipByPersonIdAsync(string personId)
        {
            var lastInternship = await GetQuery().Where(i => i.PersonId == personId).OrderBy(i => i.StartDate).LastOrDefaultAsync();

            return lastInternship;
        }

        public async Task<IReadOnlyCollection<UnitInternshipsCount>> GetUnitInternshipCountsAsync()
        {
            var counts = await GetQuery().Where(i => i.IsActive).GroupBy(i => i.UnitId).Select(g =>
                new UnitInternshipsCount
                {
                    UnitId = g.Key,
                    InternshipsCount = g.Count(),
                }).ToListAsync();

            return counts;
        }

        public async Task<PaginatedItems<Internship>> GetInternshipsPaginatedAsync(
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            Specification<Internship> specification,
            IEntityLoadStrategy<Internship> loadStrategy = null)
        {
            var items = await GetQuery(loadStrategy)
                .Where(specification.Predicate)
                .Where(m => (m.StartDate >= fromDate && m.StartDate <= toDate) || (!m.IsActive && m.EndDate >= fromDate && m.EndDate <= toDate))
                .ToListAsync();

            Expression<Func<Internship, bool>> nextItemsPredicate = direction == PaginationDirection.Ascending
                ? m => (m.StartDate > toDate) || (!m.IsActive && m.EndDate > toDate)
                : m => (m.StartDate < fromDate) || (!m.IsActive && m.EndDate < fromDate);

            var hasNext = await GetQuery()
                .Where(specification.Predicate)
                .Where(nextItemsPredicate)
                .AnyAsync();

            var paginatedMentorships = new PaginatedItems<Internship>(items, hasNext);

            return paginatedMentorships;
        }
    }
}