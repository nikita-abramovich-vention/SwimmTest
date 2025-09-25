using System;
using System.Threading.Tasks;
using DreamTeam.Common.Specification;
using DreamTeam.DomainModel;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories;

public interface IEmploymentPeriodRepository : IRepository<EmploymentPeriod>
{
    Task<PaginatedItems<EmploymentPeriod>> GetPaginatedAsync(
        DateOnly fromDate,
        DateOnly toDate,
        PaginationDirection direction,
        Specification<EmploymentPeriod> specification = null,
        IEntityLoadStrategy<EmploymentPeriod> loadStrategy = null);
}