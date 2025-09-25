using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    [UsedImplicitly]
    public sealed class RelocationPlanRepository : Repository<RelocationPlan>, IRelocationPlanRepository
    {
        public RelocationPlanRepository(IDbContext dbContext)
            : base(dbContext)
        {

        }


        public async Task<IReadOnlyCollection<CurrentLocation>> GetActiveRelocationLocationsAsync()
        {
            return await GetQuery()
                .Where(p => p.State == RelocationPlanState.Active)
                .Select(p => p.Location)
                .Distinct()
                .ToListAsync();
        }

        public async Task<RelocationPlan> GetLatestSameCountryApprovedRelocationPlanAsync(int employeeId, string countryId)
        {
            return await GetQuery()
                .Where(p =>
                    p.EmployeeId == employeeId
                    && p.Location.CountryId == countryId
                    && p.IsApproved
                    && p.State != RelocationPlanState.Rejected)
                .OrderByDescending(p => p.ApprovalDate)
                .FirstOrDefaultAsync();
        }
    }
}