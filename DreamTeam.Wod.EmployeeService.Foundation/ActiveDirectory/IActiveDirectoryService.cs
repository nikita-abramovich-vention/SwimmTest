using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;

namespace DreamTeam.Wod.EmployeeService.Foundation.ActiveDirectory
{
    public interface IActiveDirectoryService
    {
        Task<OperationResult<ActiveDirectoryUser>> GetUserAsync(string domainName);

        Task<OperationResult<IReadOnlyCollection<ActiveDirectoryUser>>> GetUsersAsync();
    }
}