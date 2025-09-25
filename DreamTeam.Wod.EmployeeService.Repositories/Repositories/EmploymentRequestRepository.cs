using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Specification;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    [UsedImplicitly]
    public sealed class EmploymentRequestRepository : Repository<EmploymentRequest>, IEmploymentRequestRepository
    {
        public EmploymentRequestRepository(IDbContext dbContext)
            : base(dbContext)
        {

        }


        public async Task<IReadOnlyCollection<EmploymentRequest>> GetUniqueAsync(
            ISpecification<EmploymentRequest> specification,
            IEntityLoadStrategy<EmploymentRequest> loadStrategy = null)
        {
            return await GetQuery(loadStrategy)
                .Where(specification.Predicate)
                .GroupBy(r => new { r.FirstName, r.LastName, r.UnitId })
                .Select(gr => gr.OrderBy(r => r.EmploymentDate).Last())
                .ToListAsync();
        }
    }
}