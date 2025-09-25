using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Repositories.EntityFramework.Interfaces;
using DreamTeam.Wod.EmployeeService.DomainModel;
using Microsoft.EntityFrameworkCore;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeSnapshotRepository : Repository<EmployeeSnapshot>, IEmployeeSnapshotRepository
    {
        private readonly IDbContext _dbContext;


        public EmployeeSnapshotRepository(IDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<IReadOnlyCollection<EmployeeSnapshot>> GetLastEmployeeSnapshotsAsync()
        {
            // TODO It's possible to replace raw SQL with LINQ GroupBy when EF Core 6.0 is used
            // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/whatsnew#improved-groupby-support
            var employeeSnapshots = await _dbContext.Set<EmployeeSnapshot>()
                .FromSqlRaw(@"
                    WITH EmployeeSnapshotGroup AS (
                        SELECT *, ROW_NUMBER() OVER (PARTITION BY EmployeeId ORDER BY ToDate DESC) AS RowNumber
                        FROM EmployeeSnapshot
                    )
                    SELECT *
                    FROM EmployeeSnapshotGroup
                    WHERE RowNumber = 1
                ")
                .ToListAsync();

            return employeeSnapshots;
        }
    }
}
