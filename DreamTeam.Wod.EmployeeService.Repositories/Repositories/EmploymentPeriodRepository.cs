using System;
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

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories;

[UsedImplicitly]
public sealed class EmploymentPeriodRepository : Repository<EmploymentPeriod>, IEmploymentPeriodRepository
{
    public EmploymentPeriodRepository(IDbContext dbContext)
        : base(dbContext)
    {
    }


    public async Task<PaginatedItems<EmploymentPeriod>> GetPaginatedAsync(
        DateOnly fromDate,
        DateOnly toDate,
        PaginationDirection direction,
        Specification<EmploymentPeriod> specification = null,
        IEntityLoadStrategy<EmploymentPeriod> loadStrategy = null)
    {
        // Duplicates existing ForPeriodSpecification
        var resultSpecification = Specification<EmploymentPeriod>.FromExpression(p => p.StartDate <= toDate && (p.EndDate == null || p.EndDate >= fromDate));
        if (specification != null)
        {
            resultSpecification &= specification;
        }

        var items = await GetQuery(loadStrategy)
            .Where(resultSpecification.Predicate)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();

        var nextDirectionPredicate = direction == PaginationDirection.Ascending
            ? (Expression<Func<EmploymentPeriod, bool>>)(p => p.StartDate > toDate)
            : p => p.StartDate < fromDate;
        var hasNext = await GetQuery().Where(specification.Predicate).Where(nextDirectionPredicate).AnyAsync();

        return new PaginatedItems<EmploymentPeriod>(items, hasNext);
    }
}