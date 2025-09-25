using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.DomainModel;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public interface IEmploymentPeriodService
    {
        Task<IReadOnlyCollection<EmploymentPeriod>> GetAllReadOnlyAsync();

        Task<IReadOnlyCollection<EmploymentPeriod>> GetByDateAsync(DateOnly date);

        Task<IReadOnlyCollection<EmploymentPeriod>> GetByEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds);

        Task<IReadOnlyCollection<EmploymentPeriod>> GetForPeriodAsync(DateOnly startDate, DateOnly endDate, [CanBeNull]IReadOnlyCollection<string> employeeIds);

        Task<PaginatedItems<EmploymentPeriod>> GetByEmployeeIdPaginatedAsync(string employeeId, DateOnly fromDate, DateOnly toDate, PaginationDirection direction);
    }
}