using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.WodEvents;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    [UsedImplicitly]
    public sealed class EmployeeSnapshotService : IEmployeeSnapshotService
    {
        private const int EmployeeSnapshotsChangedDelayMilliseconds = 5000;
        private static readonly TimeSpan SnapshotTimerInterval = TimeSpan.FromHours(1);

        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;

        private readonly Timer _snapshotTimer;
        private readonly IAsyncResourceLocker _snapshotLocker;
        private readonly Func<Task> _raiseEmployeeSnapshotsChangedDebounced;


        public event AsyncObserver EmployeeSnapshotsChanged;


        public EmployeeSnapshotService(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService)
        {
            _uowFactory = uowFactory;
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;

            _snapshotTimer = new Timer(SnapshotTimerInterval.TotalMilliseconds);
            _snapshotTimer.Elapsed += SnapshotTimerOnElapsed;
            _snapshotLocker = new AsyncResourceLocker();

            _raiseEmployeeSnapshotsChangedDebounced = ((Func<Task>)(() => EmployeeSnapshotsChanged.RaiseAsync())).Debounce(EmployeeSnapshotsChangedDelayMilliseconds);
        }


        public Task InitializeAsync(IWodObservable wodObservable)
        {
            _snapshotTimer.Start();
            Task.Run(CreateSnapshotsAsync).Forget();

            return Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<EmployeeSnapshot>> GetEmployeeSnapshotsPerDayAsync(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> unitIds = null, bool shouldIncludeInactive = false)
        {
            var uow = _uowProvider.CurrentUow;
            var snapshotRepository = uow.EmployeeSnapshots;
            var loadStrategy = GetLoadStrategy();

            var specification = EmployeeSnapshotSpecification.ByFromDate(fromDate) &
                                EmployeeSnapshotSpecification.ByToDate(toDate);
            if (unitIds != null)
            {
                specification &= EmployeeSnapshotSpecification.ByUnitIds(unitIds);
            }

            if (!shouldIncludeInactive)
            {
                specification &= EmployeeSnapshotSpecification.Active;
            }

            var snapshots = await snapshotRepository.GetWhereAsync(specification, loadStrategy);

            return snapshots;
        }

        public async Task<IReadOnlyCollection<EmployeeSnapshot>> GetAllEmployeeSnapshotsAsync()
        {
            var uow = _uowProvider.CurrentUow;
            var snapshotRepository = uow.EmployeeSnapshots;
            var loadStrategy = GetLoadStrategy();

            var snapshots = await snapshotRepository.GetAllAsync(loadStrategy);

            return snapshots;
        }

        public async Task CreateSnapshotForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow)
        {
            using (await _snapshotLocker.GetAccessAsync())
            {
                var currentDate = _environmentInfoService.CurrentUtcDate;

                CreateEmployeeSnapshot(employee, currentDate, uow);
            }
        }

        public async Task UpdateLastSnapshotForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow)
        {
            using (await _snapshotLocker.GetAccessAsync())
            {
                var currentDate = _environmentInfoService.CurrentUtcDate;
                var dayBefore = currentDate.AddDays(-1);

                var snapshotRepository = uow.EmployeeSnapshots;
                var loadStrategy = GetLoadStrategy();
                var specification = EmployeeSnapshotSpecification.ByEmployeeId(employee.ExternalId) &
                                    EmployeeSnapshotSpecification.ByFromDate(currentDate) &
                                    EmployeeSnapshotSpecification.ByToDate(currentDate);

                var snapshotToRecalculate = await snapshotRepository.GetSingleOrDefaultAsync(specification, loadStrategy);
                if (snapshotToRecalculate != null)
                {
                    var isSnapshotUpToDate = CheckIfSnapshotIsUpToDate(snapshotToRecalculate, employee);
                    if (!isSnapshotUpToDate)
                    {
                        if (snapshotToRecalculate.FromDate <= dayBefore)
                        {
                            snapshotToRecalculate.ToDate = dayBefore;
                        }
                        else
                        {
                            snapshotRepository.Delete(snapshotToRecalculate);
                        }

                        CreateEmployeeSnapshot(employee, currentDate, uow);
                    }
                }
            }
        }

        public async Task RewriteLastSnapshotEmploymentTypeForEmployeeAsync(Employee employee, IEmployeeServiceUnitOfWork uow)
        {
            using (await _snapshotLocker.GetAccessAsync())
            {
                var currentDate = _environmentInfoService.CurrentUtcDate;

                var snapshotRepository = uow.EmployeeSnapshots;
                var specification = EmployeeSnapshotSpecification.ByEmployeeId(employee.ExternalId) &
                                    EmployeeSnapshotSpecification.ByToDate(currentDate);

                var snapshots = await snapshotRepository.GetWhereAsync(specification);
                var snapshotToRecalculate = snapshots.MaxBy(s => s.ToDate);
                if (snapshotToRecalculate != null)
                {
                    if (snapshotToRecalculate.EmploymentType != employee.EmploymentType)
                    {
                        LoggerContext.Current.LogWarning(
                            "Rewriting snapshot {snapshotId} employment type from {oldEmploymentType} to {newEmploymentType}.",
                            snapshotToRecalculate.Id,
                            snapshotToRecalculate.EmploymentType,
                            employee.EmploymentType);

                        snapshotToRecalculate.EmploymentType = employee.EmploymentType;
                        uow.OnChangesSavedAndCommitted(_raiseEmployeeSnapshotsChangedDebounced);
                    }

                    if (snapshots.Count > 1)
                    {
                        LoggerContext.Current.LogWarning(
                            "Employee has more snapshots to adjust employment type. Employee {employeeId} has {snapshotsCount} snapshots to verify in total.",
                            employee.ExternalId,
                            snapshots.Count);
                    }
                }
            }
        }

        public async Task<RecalculateEmployeeSnapshotsStatus> RecalculateEmployeeSnapshotsAsync(IReadOnlyCollection<string> employeeIds, DateOnly fromDate)
        {
            var isAllEmployeesHaveSnapshots = await CheckIfAllEmployeesHaveSnapshotsAsync(employeeIds, fromDate);
            if (!isAllEmployeesHaveSnapshots)
            {
                return RecalculateEmployeeSnapshotsStatus.InvalidArguments;
            }

            using (await _snapshotLocker.GetAccessAsync())
            {
                await RecalculateEmployeeSnapshotsForEmployeesAsync(employeeIds, fromDate);

                return RecalculateEmployeeSnapshotsStatus.Success;
            }
        }


        private async void SnapshotTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await CreateSnapshotsAsync();
        }

        private async Task CreateSnapshotsAsync()
        {
            using (_snapshotLocker.TryGetAccess(out var isAccessed))
            {
                if (!isAccessed)
                {
                    return;
                }

                await CreateSnapshotsForAllEmployeesAsync();
            }
        }

        private async Task CreateSnapshotsForAllEmployeesAsync()
        {
            var now = _environmentInfoService.CurrentUtcDateTime;
            var currentDate = now.ToDateOnly();
            var isAlreadyExists = await CheckIfSnapshotAlreadyExistsAsync(currentDate);
            if (isAlreadyExists)
            {
                return;
            }

            var isCreatedSuccessfully = false;
            try
            {
                using (var uow = _uowFactory.Create())
                {
                    var employeeRepository = uow.GetRepository<Employee>();
                    var employees = await employeeRepository.GetAllAsync();

                    var snapshotRepository = uow.EmployeeSnapshots;
                    var failedSnapshots = await snapshotRepository.GetWhereAsync(s => s.ToDate == currentDate);
                    foreach (var failedSnapshot in failedSnapshots)
                    {
                        failedSnapshot.ToDate = currentDate.AddDays(-1);
                    }
                    var failedSnapshotsToDelete = failedSnapshots.Where(s => s.FromDate == currentDate).ToList();
                    snapshotRepository.DeleteAll(failedSnapshotsToDelete);
                    await uow.SaveChangesAsync();

                    var lastSnapshots = await snapshotRepository.GetLastEmployeeSnapshotsAsync();
                    var lastSnapshotsMap = lastSnapshots.ToDictionary(es => es.EmployeeId);

                    var newSnapshots = new List<EmployeeSnapshot>();
                    foreach (var employee in employees)
                    {
                        DateOnly newSnapshotFromDate;

                        if (lastSnapshotsMap.TryGetValue(employee.Id, out var lastSnapshot))
                        {
                            if (CheckIfSnapshotIsUpToDate(lastSnapshot, employee))
                            {
                                lastSnapshot.ToDate = currentDate;

                                continue;
                            }

                            newSnapshotFromDate = lastSnapshot.ToDate.AddDays(1);
                        }
                        else
                        {
                            newSnapshotFromDate = currentDate;
                        }

                        var newSnapshot = CreateFrom(employee, newSnapshotFromDate, currentDate);
                        newSnapshots.Add(newSnapshot);
                    }

                    snapshotRepository.AddRange(newSnapshots);
                    await uow.SaveChangesAsync();

                    isCreatedSuccessfully = true;
                    await _raiseEmployeeSnapshotsChangedDebounced();
                }
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError("Failed to save employees snapshots.", ex);
            }
            finally
            {
                await CreateSnapshotLogAsync(isCreatedSuccessfully, now);
            }
        }

        private async Task RecalculateEmployeeSnapshotsForEmployeesAsync(IReadOnlyCollection<string> employeeIds, DateOnly fromDate)
        {
            var currentDate = _environmentInfoService.CurrentUtcDate;
            using (var uow = _uowFactory.Create())
            {
                var snapshotRepository = uow.EmployeeSnapshots;
                var loadStrategy = GetLoadStrategy();
                var specification = EmployeeSnapshotSpecification.ByEmployeeIds(employeeIds) &
                                    EmployeeSnapshotSpecification.ByFromDate(fromDate);

                var snapshots = await snapshotRepository.GetWhereAsync(specification, loadStrategy);
                var snapshotGroups = snapshots.GroupBy(s => s.Employee);

                var dayBeforeFromDate = fromDate.AddDays(-1);
                var newSnapshots = new List<EmployeeSnapshot>();
                var snapshotsToDelete = new List<EmployeeSnapshot>();
                foreach (var snapshotGroup in snapshotGroups)
                {
                    var snapshotsToRecalculate = snapshotGroup.OrderBy(s => s.ToDate).ToList();
                    var oldestSnapshotToRecalculate = snapshotsToRecalculate.First();
                    var snapshotsExceptOldest = snapshotsToRecalculate.Skip(1).ToList();
                    snapshotsToDelete.AddRange(snapshotsExceptOldest);

                    var employee = snapshotGroup.Key;
                    var isOldestSnapshotUpToDate = CheckIfSnapshotIsUpToDate(oldestSnapshotToRecalculate, employee);
                    if (isOldestSnapshotUpToDate)
                    {
                        oldestSnapshotToRecalculate.ToDate = currentDate;
                    }
                    else
                    {
                        if (oldestSnapshotToRecalculate.FromDate <= dayBeforeFromDate)
                        {
                            oldestSnapshotToRecalculate.ToDate = dayBeforeFromDate;
                        }
                        else
                        {
                            snapshotsToDelete.Add(oldestSnapshotToRecalculate);
                        }

                        var newSnapshot = CreateFrom(employee, fromDate, currentDate);
                        newSnapshots.Add(newSnapshot);
                    }
                }

                snapshotRepository.AddRange(newSnapshots);
                snapshotRepository.DeleteAll(snapshotsToDelete);
                await uow.SaveChangesAsync();

                await _raiseEmployeeSnapshotsChangedDebounced();
            }
        }

        private async Task<bool> CheckIfSnapshotAlreadyExistsAsync(DateOnly currentDate)
        {
            using (var uow = _uowFactory.Create())
            {
                var snapshotLogRepository = uow.EmployeeSnapshotLogs;
                var lastSuccessfulEmployeeSnapshotLog = await snapshotLogRepository.GetLastSuccessfulEmployeeSnapshotLogAsync();
                var isAlreadyExists = lastSuccessfulEmployeeSnapshotLog != null && lastSuccessfulEmployeeSnapshotLog.Date.ToDateOnly() >= currentDate;

                return isAlreadyExists;
            }
        }

        private async Task CreateSnapshotLogAsync(bool isSuccessful, DateTime currentDateTime)
        {
            using (var uow = _uowFactory.Create())
            {
                var snapshotLogRepository = uow.EmployeeSnapshotLogs;
                var employeeSnapshotLog = new EmployeeSnapshotLog
                {
                    Date = currentDateTime,
                    IsSuccessful = isSuccessful,
                };
                snapshotLogRepository.Add(employeeSnapshotLog);
                await uow.SaveChangesAsync();
            }
        }

        private async Task<bool> CheckIfAllEmployeesHaveSnapshotsAsync(IReadOnlyCollection<string> employeeIds, DateOnly date)
        {
            var uow = _uowProvider.CurrentUow;
            var snapshotRepository = uow.EmployeeSnapshots;

            var specification = EmployeeSnapshotSpecification.ByEmployeeIds(employeeIds) &
                                EmployeeSnapshotSpecification.ByFromDate(date) &
                                EmployeeSnapshotSpecification.ByToDate(date);

            var snapshotsCount = await snapshotRepository.CountAsync(specification);

            var isAllEmployeesHaveSnapshots = snapshotsCount == employeeIds.Count;

            return isAllEmployeesHaveSnapshots;
        }

        private void CreateEmployeeSnapshot(Employee employee, DateOnly currentDate, IEmployeeServiceUnitOfWork uow)
        {
            var snapshotRepository = uow.EmployeeSnapshots;
            var newSnapshot = CreateFrom(employee, currentDate, currentDate);
            snapshotRepository.Add(newSnapshot);
            uow.OnChangesSavedAndCommitted(_raiseEmployeeSnapshotsChangedDebounced);
        }

        private static EmployeeSnapshot CreateFrom(Employee employee, DateOnly fromDate, DateOnly toDate)
        {
            return new EmployeeSnapshot
            {
                EmployeeId = employee.Id,
                Employee = employee,
                FromDate = fromDate,
                ToDate = toDate,
                IsActive = employee.IsActive,
                SeniorityId = employee.SeniorityId,
                TitleRoleId = employee.TitleRoleId,
                CountryId = employee.CountryId,
                OrganizationId = employee.OrganizationId,
                UnitId = employee.UnitId,
                EmploymentType = employee.EmploymentType,
            };
        }

        private static bool CheckIfSnapshotIsUpToDate(EmployeeSnapshot snapshot, Employee employee)
        {
            return snapshot.IsActive == employee.IsActive &&
                snapshot.SeniorityId == employee.SeniorityId &&
                snapshot.TitleRoleId == employee.TitleRoleId &&
                snapshot.CountryId == employee.CountryId &&
                snapshot.OrganizationId == employee.OrganizationId &&
                snapshot.UnitId == employee.UnitId &&
                snapshot.EmploymentType == employee.EmploymentType;
        }

        private static IEntityLoadStrategy<EmployeeSnapshot> GetLoadStrategy()
        {
            return new EntityLoadStrategy<EmployeeSnapshot>(s => s.Employee, s => s.TitleRole, s => s.Seniority);
        }
    }
}