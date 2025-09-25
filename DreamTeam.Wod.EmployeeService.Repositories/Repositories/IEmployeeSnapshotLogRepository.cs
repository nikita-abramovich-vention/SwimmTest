using System.Threading.Tasks;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IEmployeeSnapshotLogRepository : IRepository<EmployeeSnapshotLog>
    {
        Task<EmployeeSnapshotLog> GetLastSuccessfulEmployeeSnapshotLogAsync();
    }
}