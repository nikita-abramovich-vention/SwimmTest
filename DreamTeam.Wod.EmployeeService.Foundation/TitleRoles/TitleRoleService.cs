using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.TitleRoles
{
    [UsedImplicitly]
    public sealed class TitleRoleService : ITitleRoleService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;


        public TitleRoleService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider)
        {
            _uowProvider = uowProvider;
        }


        public async Task<IReadOnlyCollection<TitleRole>> GetTitleRolesAsync(IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var titleRoleRepository = currentUow.GetRepository<TitleRole>();
            var titleRoles = await titleRoleRepository.GetAllAsync();

            return titleRoles;
        }

        public async Task<IReadOnlyCollection<TitleRole>> GetTitleRolesByIdsAsync(IReadOnlyCollection<string> ids)
        {
            var currentUow = _uowProvider.CurrentUow;
            var titleRoleRepository = currentUow.GetRepository<TitleRole>();
            var titleRoles = await titleRoleRepository.GetWhereAsync(r => ids.Contains(r.ExternalId));

            return titleRoles;
        }

        public async Task<TitleRole> GetTitleRoleByIdAsync(string externalId)
        {
            var titleRoleRepository = _uowProvider.CurrentUow.GetRepository<TitleRole>();
            var titleRole = await titleRoleRepository.GetSingleOrDefaultAsync(r => r.ExternalId == externalId);

            return titleRole;
        }
    }
}