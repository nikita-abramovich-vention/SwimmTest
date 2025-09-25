using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Specification;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IEmploymentRequestRepository : IRepository<EmploymentRequest>
    {
        Task<IReadOnlyCollection<EmploymentRequest>> GetUniqueAsync(
            ISpecification<EmploymentRequest> specification,
            IEntityLoadStrategy<EmploymentRequest> loadStrategy = null);
    }
}