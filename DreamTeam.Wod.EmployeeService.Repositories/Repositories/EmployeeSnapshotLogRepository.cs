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
    public sealed class EmployeeSnapshotLogRepository : Repository<EmployeeSnapshotLog>, IEmployeeSnapshotLogRepository
    {
        public EmployeeSnapshotLogRepository(IDbContext dbContext)
            : base(dbContext)
        {

        }


        public async Task<EmployeeSnapshotLog> GetLastSuccessfulEmployeeSnapshotLogAsync()
        {
            return await GetQuery().Where(l => l.IsSuccessful).OrderByDescending(l => l.Date).FirstOrDefaultAsync();
        }
    }
}