using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories;

[UsedImplicitly]
public sealed class WageRatePeriodRepository : Repository<WageRatePeriod>, IWageRatePeriodRepository
{
    public WageRatePeriodRepository(IDbContext dbContext)
        : base(dbContext)
    {

    }


    public async Task<IReadOnlyCollection<double>> GetAllWageRatesAsync()
    {
        var wageRates = await GetQuery()
            .Select(p => p.Rate)
            .Distinct()
            .ToListAsync();

        return wageRates;
    }
}