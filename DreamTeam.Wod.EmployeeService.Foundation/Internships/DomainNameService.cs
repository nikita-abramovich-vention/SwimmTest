using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Internships
{
    public class DomainNameService : IDomainNameService
    {
        private readonly IActiveDirectoryService _activeDirectoryService;
        private readonly IEmployeeServiceConfiguration _employeeServiceConfiguration;
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _unitOfWorkProvider;


        public DomainNameService(
            IActiveDirectoryService activeDirectoryService,
            IEmployeeServiceConfiguration employeeServiceConfiguration,
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> unitOfWorkProvider)
        {
            _activeDirectoryService = activeDirectoryService;
            _employeeServiceConfiguration = employeeServiceConfiguration;
            _unitOfWorkProvider = unitOfWorkProvider;
        }


        public async Task<OperationResult<bool>> VerifyDomainNameAsync(string domainName)
        {
            var getUserResult = await _activeDirectoryService.GetUserAsync(domainName);
            if (!getUserResult.IsSuccessful)
            {
                return OperationResult<bool>.CreateUnsuccessful();
            }

            var user = getUserResult.Result;

            return user != null;
        }


        public async Task<Func<Internship, (string DomainName, bool IsVerified)>> CreateDomainNameGeneratorAsync(IEmployeeServiceUnitOfWork uow = null)
        {
            var getAdUsersResult = await _activeDirectoryService.GetUsersAsync();
            var adUsers = getAdUsersResult.IsSuccessful
                ? getAdUsersResult.Result
                : Array.Empty<ActiveDirectoryUser>();

            var currentUow = uow ?? _unitOfWorkProvider.CurrentUow;
            var takenDomainNames = await GetTakenDomainNamesAsync(currentUow);
            var takenDomainNamesSet = takenDomainNames.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            Func<Internship, (string DomainName, bool IsVerified)> generator = internship =>
            {
                var domainName = $"{internship.FirstName}.{internship.LastName}";

                for (var i = 1; ; i++)
                {
                    var isDomainNameExist = takenDomainNamesSet.Contains(domainName);
                    if (!isDomainNameExist)
                    {
                        takenDomainNamesSet.Add(domainName);
                        var isDomainNameVerified = adUsers.Any(u => string.Equals(u.DomainName, domainName, StringComparison.InvariantCultureIgnoreCase));

                        return (domainName, isDomainNameVerified);
                    }

                    domainName = $"{internship.FirstName}.{internship.LastName}_duplicate_{i}";
                }
            };

            return generator;
        }

        public string GenerateEmail(string domainName)
        {
            return String.Format(_employeeServiceConfiguration.InternEmailTemplate, domainName);
        }


        private static async Task<IReadOnlyCollection<string>> GetTakenDomainNamesAsync(IEmployeeServiceUnitOfWork uow)
        {
            var internshipRepository = uow.GetRepository<Internship>();
            var employeeRepository = uow.GetRepository<Employee>();
            var activeInternships = await internshipRepository.GetWhereAsync(i => i.IsActive);
            var internshipDomainNames = activeInternships.Select(i => i.DomainName);
            var employeeDomainNames = await employeeRepository.SelectAllAsync(e => e.DomainName);

            var takenDomainNames = internshipDomainNames.Union(employeeDomainNames).ToList();

            return takenDomainNames;
        }
    }
}
