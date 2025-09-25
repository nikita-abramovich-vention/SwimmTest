using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.SeniorityManagement
{
    [UsedImplicitly]
    public sealed class SeniorityService : ISeniorityService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;


        public SeniorityService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider)
        {
            _uowProvider = uowProvider;
        }


        public Task<IReadOnlyCollection<Seniority>> GetAllSeniorityAsync()
        {
            return GetAllSeniorityAsync(_uowProvider.CurrentUow);
        }

        public async Task<IReadOnlyCollection<Seniority>> GetAllSeniorityAsync(IEmployeeServiceUnitOfWork uow)
        {
            var seniorityRepository = uow.GetRepository<Seniority>();
            var allSeniority = await seniorityRepository.GetAllAsync();

            return allSeniority;
        }

        public async Task<Seniority> GetSeniorityByIdAsync(string id)
        {
            var seniorityRepository = _uowProvider.CurrentUow.GetRepository<Seniority>();
            var seniority = await seniorityRepository.GetSingleAsync(s => s.ExternalId == id);

            return seniority;
        }
    }
}