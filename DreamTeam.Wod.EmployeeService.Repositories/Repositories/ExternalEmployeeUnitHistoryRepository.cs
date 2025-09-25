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
public sealed class ExternalEmployeeUnitHistoryRepository : Repository<ExternalEmployeeUnitHistory>, IExternalEmployeeUnitHistoryRepository
{
    public ExternalEmployeeUnitHistoryRepository(IDbContext dbContext)
        : base(dbContext)
    {
    }


    public async Task<IReadOnlyCollection<string>> GetSourceIdsOfEmployeesWithMultipleActiveHistoryItemsAsync()
    {
        var source = await GetQuery()
            .GroupBy(h => h.SourceEmployeeId)
            .Where(g => g.Count(h => h.EndDate == null) > 1)
            .Select(g => g.Key)
            .ToListAsync();

        return source;
    }
}