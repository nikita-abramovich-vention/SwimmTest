using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common.Observable;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public interface IEmployeeSnapshotService
    {
        event AsyncObserver EmployeeSnapshotsChanged;


        Task InitializeAsync(IWodObservable wodObservable);

        Task<IReadOnlyCollection<EmployeeSnapshot>> GetEmployeeSnapshotsPerDayAsync(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> unitIds = null, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<EmployeeSnapshot>> GetAllEmployeeSnapshotsAsync();

        Task CreateSnapshotForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow);

        Task UpdateLastSnapshotForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow);

        Task<RecalculateEmployeeSnapshotsStatus> RecalculateEmployeeSnapshotsAsync(IReadOnlyCollection<string> employeeIds, DateOnly fromDate);

        Task RewriteLastSnapshotEmploymentTypeForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow);
    }
}