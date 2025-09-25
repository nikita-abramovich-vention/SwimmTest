using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.TitleRoles
{
    public interface ITitleRoleService
    {
        Task<IReadOnlyCollection<TitleRole>> GetTitleRolesAsync(IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<TitleRole>> GetTitleRolesByIdsAsync(IReadOnlyCollection<string> ids);

        Task<TitleRole> GetTitleRoleByIdAsync(string externalId);
    }
}