using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests
{
    public interface IEmploymentRequestService
    {
        Task<IReadOnlyCollection<EmploymentRequest>> GetEmploymentRequestsAsync(bool includeWithEmployees, bool includeInternshipEmploymentRequests);

        Task<IReadOnlyCollection<EmploymentRequest>> GetByIdsAsync(IReadOnlyCollection<string> ids, bool includeInternshipEmploymentRequests);

        Task<IReadOnlyCollection<EmploymentRequest>> GetByPeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate);
    }
}
