using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Internships
{
    public interface IDomainNameService
    {
        Task<OperationResult<bool>> VerifyDomainNameAsync(string domainName);

        Task<Func<Internship, (string DomainName, bool IsVerified)>> CreateDomainNameGeneratorAsync(IEmployeeServiceUnitOfWork uow = null);

        string GenerateEmail(string domainName);
    }
}