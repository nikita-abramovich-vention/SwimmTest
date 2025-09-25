using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Repositories.Repositories
{
    public interface IRelocationPlanRepository : IRepository<RelocationPlan>
    {
        Task<IReadOnlyCollection<CurrentLocation>> GetActiveRelocationLocationsAsync();

        Task<RelocationPlan> GetLatestSameCountryApprovedRelocationPlanAsync(int employeeId, string countryId);
    }
}