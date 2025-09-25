using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Specification;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IDismissalRequestRepository : IRepository<DismissalRequest>
    {
        Task<IReadOnlyCollection<DismissalRequest>> GetUniqueAsync(
            ISpecification<DismissalRequest> specification,
            IEntityLoadStrategy<DismissalRequest> loadStrategy = null);
    }
}