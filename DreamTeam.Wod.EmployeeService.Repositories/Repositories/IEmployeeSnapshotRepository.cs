using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IEmployeeSnapshotRepository : IRepository<EmployeeSnapshot>
    {
        Task<IReadOnlyCollection<EmployeeSnapshot>> GetLastEmployeeSnapshotsAsync();
    }
}
