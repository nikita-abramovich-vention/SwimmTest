using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests
{
    public interface IDismissalRequestService
    {
        Task<IReadOnlyCollection<DismissalRequest>> GetAllAsync(bool activeOnly = false);

        Task<DismissalRequest> GetByIdAsync(string id);

        Task<IReadOnlyCollection<DismissalRequest>> GetByPeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate);

        Task<IReadOnlyCollection<DismissalRequest>> GetByEmployeeIdAsync(string employeeId);

        Task<IReadOnlyCollection<DismissalRequest>> GetByPeriodAndEmployeeIdsAsync(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> employeeIds);

        Task<IReadOnlyCollection<DismissalRequest>> GetByEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds, IReadOnlyCollection<DismissalRequestType> types = null);

        Task<DismissalRequest> CreateAsync(DismissalRequest dismissalRequest);

        Task<DismissalRequest> UpdateAsync(DismissalRequest dismissalRequest, DismissalRequest fromDismissalRequest);
    }
}