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
    public sealed class SyncLogRepository : Repository<SyncLog>, ISyncLogRepository
    {
        public SyncLogRepository(IDbContext dbContext)
            : base(dbContext)
        {

        }


        public async Task<SyncLog> GetLastSuccessfulSyncLogAsync(SyncType type)
        {
            return await GetQuery().Where(l => l.Type == type && l.IsSuccessful).OrderByDescending(l => l.SyncCompletedDate).FirstOrDefaultAsync();
        }
    }
}