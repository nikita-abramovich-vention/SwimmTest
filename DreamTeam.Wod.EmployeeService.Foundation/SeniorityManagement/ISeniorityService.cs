using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement
{
    public interface ISeniorityService
    {
        Task<IReadOnlyCollection<Seniority>> GetAllSeniorityAsync();

        Task<IReadOnlyCollection<Seniority>> GetAllSeniorityAsync(IEmployeeServiceUnitOfWork uow);

        Task<Seniority> GetSeniorityByIdAsync(string id);
    }
}