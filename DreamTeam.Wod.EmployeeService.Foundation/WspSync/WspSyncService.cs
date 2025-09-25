using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DreamTeam.Common;
using DreamTeam.Common.EqualityComparison;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.WspService;
using DreamTeam.Wod.WspService.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.WspSync
{
    [UsedImplicitly]
    public sealed class WspSyncService : IWspSyncService
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IWspService _wspService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IWspSyncServiceConfiguration _configuration;

        private readonly Timer _syncTimer;
        private readonly IAsyncResourceLocker _syncLocker;


        public event AsyncObserver<EmployeeChangedEventArgs> EmployeeWorkplacesChanged;


        public WspSyncService(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
            IEnvironmentInfoService environmentInfoService,
            IWspService wspService,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IWspSyncServiceConfiguration configuration)
        {
            _uowFactory = uowFactory;
            _environmentInfoService = environmentInfoService;
            _wspService = wspService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _configuration = configuration;

            _syncTimer = new Timer(Math.Min(TimeSpan.FromMinutes(5).TotalMilliseconds, _configuration.SyncInterval.TotalMilliseconds / 5));
            _syncTimer.Elapsed += SyncTimerOnElapsed;
            _syncLocker = new AsyncResourceLocker();
        }


        public Task ActivateRegularSyncAsync()
        {
            _syncTimer.Start();
            Task.Run(SyncAsync).Forget();

            return Task.CompletedTask;
        }

        public async Task<bool> SyncAsync()
        {
            if (!_configuration.Enable)
            {
                return true;
            }

            using (_syncLocker.TryGetAccess(out var isAccessed))
            {
                if (!isAccessed)
                {
                    return false;
                }

                var isDownloadedSuccessfully = await DownloadExternalDataAsync();
                if (!isDownloadedSuccessfully)
                {
                    return false;
                }

                var isLinkedSuccessfully = await LinkExternalDataAsync();

                return isLinkedSuccessfully;
            }
        }


        private async Task<bool> DownloadExternalDataAsync()
        {
            using (var uow = _uowFactory.Create())
            {
                var syncLogRepository = uow.SyncLogs;
                var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalWspData);
                var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
                if (!isSyncRequired)
                {
                    return true;
                }

                var previousSyncStartDate = lastSyncLog != null && !lastSyncLog.IsOutdated
                    ? lastSyncLog.SyncStartDate
                    : (DateTime?)null;

                var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
                var syncLog = new SyncLog
                {
                    SyncStartDate = syncStartDate,
                    Type = SyncType.DownloadExternalWspData,
                };

                LoggerContext.Current.Log("Starting WSP workplaces download...");
                var isCompletedSuccessfully = false;
                try
                {
                    var downloadAndSaveWspDataResult = await DownloadAndSaveWspDataAsync(syncStartDate, previousSyncStartDate);
                    isCompletedSuccessfully = downloadAndSaveWspDataResult.IsSuccessful;
                    syncLog.AffectedEntitiesCount = downloadAndSaveWspDataResult.AffectedEntitiesCount;

                    return isCompletedSuccessfully;
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogError("Failed to download WSP workplaces.", ex);

                    return false;
                }
                finally
                {
                    if (isCompletedSuccessfully)
                    {
                        LoggerContext.Current.Log("WSP workplaces download completed successfully. Affected workplaces - {affectedWorkplacesCount}.", syncLog.AffectedEntitiesCount);
                    }
                    else
                    {
                        LoggerContext.Current.LogWarning("WSP workplaces download failed.");
                    }

                    syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                    syncLog.IsSuccessful = isCompletedSuccessfully;
                    syncLogRepository.Add(syncLog);
                    await uow.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> LinkExternalDataAsync()
        {
            using (var uow = _uowFactory.Create())
            {
                var syncLogRepository = uow.SyncLogs;
                var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkEmployeeWorkplaces);
                var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
                if (!isSyncRequired)
                {
                    return true;
                }

                var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
                var syncLog = new SyncLog
                {
                    SyncStartDate = syncStartDate,
                    Type = SyncType.LinkEmployeeWorkplaces,
                };

                LoggerContext.Current.Log("Starting WSP external data linking...");
                var isCompletedSuccessfully = false;
                try
                {
                    var linkWspDataResult = await LinkWspDataAsync(syncStartDate);
                    isCompletedSuccessfully = linkWspDataResult.IsSuccessful;
                    syncLog.AffectedEntitiesCount = linkWspDataResult.AffectedEntitiesCount;

                    return isCompletedSuccessfully;
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogError("Failed to link WSP external data.", ex);

                    return false;
                }
                finally
                {
                    if (isCompletedSuccessfully)
                    {
                        LoggerContext.Current.Log("WSP external data linked successfully. Affected entities - {affectedEntitiesCount}.", syncLog.AffectedEntitiesCount);
                    }
                    else
                    {
                        LoggerContext.Current.LogWarning("WSP external data link failed.");
                    }

                    syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                    syncLog.IsSuccessful = isCompletedSuccessfully;
                    syncLogRepository.Add(syncLog);
                    await uow.SaveChangesAsync();
                }
            }
        }

        private async Task<SyncDataResult> DownloadAndSaveWspDataAsync(DateTime now, DateTime? previousSyncStartDate)
        {
            var downloadAndSaveWspWorkplacesResult = await DownloadAndSaveWspWorkplacesAsync(now, previousSyncStartDate);
            if (!downloadAndSaveWspWorkplacesResult.IsSuccessful)
            {
                LoggerContext.Current.LogWarning("Failed to download and save workplaces from WSP.");

                return SyncDataResult.CreateUnsuccessful();
            }

            var downloadAndSaveWspEmployeeWorkplacesResult = await DownloadAndSaveWspEmployeeWorkplacesAsync(now, previousSyncStartDate);
            if (!downloadAndSaveWspEmployeeWorkplacesResult.IsSuccessful)
            {
                LoggerContext.Current.LogWarning("Failed to download and save employee workplaces from WSP.");

                return SyncDataResult.CreateUnsuccessful();
            }

            return SyncDataResult.CreateSuccessful(downloadAndSaveWspWorkplacesResult.AffectedEntitiesCount + downloadAndSaveWspEmployeeWorkplacesResult.AffectedEntitiesCount);
        }

        private async Task<SyncDataResult> DownloadAndSaveWspWorkplacesAsync(DateTime now, DateTime? previousSyncStartDate)
        {
            var getWorkplacesResult = await _wspService.GetWorkplacesAsync(previousSyncStartDate);
            if (!getWorkplacesResult.IsSuccessful)
            {
                return SyncDataResult.CreateUnsuccessful();
            }

            var workplaces = getWorkplacesResult.Result;
            if (workplaces.Count == 0)
            {
                return SyncDataResult.CreateSuccessful(0);
            }

            var affectedEntitiesCount = 0;
            using (var uow = _uowFactory.Create())
            {
                var externalWorkplaceRepository = uow.GetRepository<ExternalWorkplace>();
                var dbWorkplaces = await externalWorkplaceRepository.GetAllAsync();
                var dbWorkplacesMap = dbWorkplaces.ToDictionary(w => w.SourceId);

                var newExternalWorkplaces = new List<ExternalWorkplace>();
                foreach (var workplace in workplaces)
                {
                    if (dbWorkplacesMap.TryGetValue(workplace.Id, out var dbWorkplace))
                    {
                        var isUpdated = UpdateFrom(dbWorkplace, workplace);
                        if (isUpdated)
                        {
                            dbWorkplace.UpdateDate = now;

                            affectedEntitiesCount++;
                        }
                    }
                    else
                    {
                        var newExternalWorkplace = new ExternalWorkplace
                        {
                            SourceId = workplace.Id,
                            CreationDate = now,
                        };
                        UpdateFrom(newExternalWorkplace, workplace);
                        newExternalWorkplaces.Add(newExternalWorkplace);

                        affectedEntitiesCount++;
                    }
                }

                externalWorkplaceRepository.AddRange(newExternalWorkplaces);

                await uow.SaveChangesAsync();

                return SyncDataResult.CreateSuccessful(affectedEntitiesCount);
            }
        }

        private async Task<SyncDataResult> DownloadAndSaveWspEmployeeWorkplacesAsync(DateTime now, DateTime? previousSyncStartDate)
        {
            var getEmployeeWorkplacesResult = await _wspService.GetEmployeeWorkplacesAsync(previousSyncStartDate);
            if (!getEmployeeWorkplacesResult.IsSuccessful)
            {
                return SyncDataResult.CreateUnsuccessful();
            }

            var employeeWorkplaces = getEmployeeWorkplacesResult.Result;
            if (employeeWorkplaces.Count == 0)
            {
                return SyncDataResult.CreateSuccessful(0);
            }

            using (var uow = _uowFactory.Create())
            {
                var externalEmployeeWorkplaceRepository = uow.GetRepository<ExternalEmployeeWorkplace>();

                var externalEmployeeWorkplacesToAdd = new List<ExternalEmployeeWorkplace>();
                var externalEmployeeWorkplacesToRemove = new List<ExternalEmployeeWorkplace>();

                var sourceEmployeeIds = employeeWorkplaces.Select(ew => ew.EmployeeId).ToList();
                var dbExternalEmployeeWorkplaces = await externalEmployeeWorkplaceRepository.GetWhereAsync(ew => sourceEmployeeIds.Contains(ew.SourceEmployeeId));
                var dbExternalEmployeeWorkplacesMap = dbExternalEmployeeWorkplaces.ToGroupedDictionary(ew => ew.SourceEmployeeId);
                foreach (var employeeWorkplace in employeeWorkplaces)
                {
                    var convertedWspEmployeeWorkplaces = CreateFrom(employeeWorkplace);
                    var currentDbExternalEmployeeWorkplaces = dbExternalEmployeeWorkplacesMap.GetValueOrDefault(employeeWorkplace.EmployeeId, new List<ExternalEmployeeWorkplace>());
                    var employeeWorkplacesToAdd = convertedWspEmployeeWorkplaces
                        .Where(ew => currentDbExternalEmployeeWorkplaces.All(w => w.SourceWorkplaceId != ew.WorkplaceId))
                        .Select(ew => CreateFrom(ew, now))
                        .ToList();
                    var employeeWorkplacesToRemove = currentDbExternalEmployeeWorkplaces
                        .Where(ew => convertedWspEmployeeWorkplaces.All(w => w.WorkplaceId != ew.SourceWorkplaceId))
                        .ToList();

                    externalEmployeeWorkplacesToAdd.AddRange(employeeWorkplacesToAdd);
                    externalEmployeeWorkplacesToRemove.AddRange(employeeWorkplacesToRemove);
                }

                externalEmployeeWorkplaceRepository.AddRange(externalEmployeeWorkplacesToAdd);
                externalEmployeeWorkplaceRepository.DeleteAll(externalEmployeeWorkplacesToRemove);

                await uow.SaveChangesAsync();

                return SyncDataResult.CreateSuccessful(externalEmployeeWorkplacesToAdd.Count + externalEmployeeWorkplacesToRemove.Count);
            }
        }

        private async Task<SyncDataResult> LinkWspDataAsync(DateTime now)
        {
            Dictionary<string, Employee> employeesBeforeChangeMap;
            using (var uow = _uowFactory.Create())
            {
                var employeesBeforeChange = await _employeeService.GetEmployeesAsync(true, uow, true);
                employeesBeforeChangeMap = employeesBeforeChange.ToDictionary(e => e.ExternalId);
            }

            using (var uow = _uowFactory.Create())
            {
                var linkWspWorkplacesResult = await LinkWspWorkplacesAsync(uow, now);
                if (!linkWspWorkplacesResult.IsSuccessful)
                {
                    LoggerContext.Current.LogWarning("Failed to link workplaces from WSP.");

                    return SyncDataResult.CreateUnsuccessful();
                }

                var affectedEmployeeWorkplacesCount = await LinkWspEmployeeWorkplacesAsync(employeesBeforeChangeMap, linkWspWorkplacesResult.AffectedEntities, uow, now);

                await uow.SaveChangesAsync();

                return SyncDataResult.CreateSuccessful(linkWspWorkplacesResult.AffectedEntitiesCount + affectedEmployeeWorkplacesCount);
            }
        }

        private async Task<LinkEntitiesResult<Workplace>> LinkWspWorkplacesAsync(IEmployeeServiceUnitOfWork uow, DateTime now)
        {
            var affectedWorkplacesCount = 0;
            var loadStrategy = new EntityLoadStrategy<ExternalWorkplace>(ew => ew.Workplace);
            var externalWorkplaceRepository = uow.GetRepository<ExternalWorkplace>();
            var externalWorkplacesToLink = await externalWorkplaceRepository.GetWhereAsync(w => w.Workplace == null || w.Workplace.LastSyncDate < w.UpdateDate, loadStrategy);
            if (externalWorkplacesToLink.Count == 0)
            {
                return LinkEntitiesResult<Workplace>.CreateSuccessful(0, new List<Workplace>());
            }

            var offices = await _departmentService.GetOfficesAsync();
            var officeByImportIdMap = offices.ToDictionary(o => o.ImportId);
            var affectedWorkplaces = new List<Workplace>();
            foreach (var externalWorkplaceToLink in externalWorkplacesToLink)
            {
                var externalWorkplaceChangeDate = externalWorkplaceToLink.UpdateDate ?? externalWorkplaceToLink.CreationDate;
                if (externalWorkplaceToLink.Workplace == null)
                {
                    var newWorkplace = new Workplace
                    {
                        ExternalId = externalWorkplaceToLink.SourceId,
                        ExternalWorkplaceId = externalWorkplaceToLink.Id,
                        ExternalWorkplace = externalWorkplaceToLink,
                        CreationDate = now,
                        LastSyncDate = externalWorkplaceChangeDate,
                    };
                    UpdateFrom(newWorkplace, externalWorkplaceToLink, officeByImportIdMap);
                    externalWorkplaceToLink.Workplace = newWorkplace;
                    affectedWorkplaces.Add(newWorkplace);

                    affectedWorkplacesCount++;
                }
                else
                {
                    UpdateFrom(externalWorkplaceToLink.Workplace, externalWorkplaceToLink, officeByImportIdMap);
                    externalWorkplaceToLink.Workplace.UpdateDate = now;
                    externalWorkplaceToLink.Workplace.LastSyncDate = externalWorkplaceChangeDate;
                    affectedWorkplaces.Add(externalWorkplaceToLink.Workplace);

                    affectedWorkplacesCount++;
                }
            }

            return LinkEntitiesResult<Workplace>.CreateSuccessful(affectedWorkplacesCount, affectedWorkplaces);
        }

        private async Task<int> LinkWspEmployeeWorkplacesAsync(
            IReadOnlyDictionary<string, Employee> employeesBeforeChangeMap,
            IReadOnlyCollection<Workplace> updatedWorkplaces,
            IEmployeeServiceUnitOfWork uow,
            DateTime now)
        {
            var affectedWorkplacesCount = 0;
            var smgToEmployeesMap = await _employeeService.GetSmgToEmployeesMapAsync(uow);

            var workplaceLoadStrategy = new EntityLoadStrategy<Workplace>(w => w.ExternalWorkplace);
            var workplaceRepository = uow.GetRepository<Workplace>();
            var workplaces = await workplaceRepository.GetAllAsync(workplaceLoadStrategy);
            var updatedWorkplaceIds = updatedWorkplaces.Select(w => w.ExternalId).ToHashSet();
            var notUpdatedWorkplaces = workplaces.Where(w => !updatedWorkplaceIds.Contains(w.ExternalId)).ToList();
            var allWorkplaces = notUpdatedWorkplaces.Concat(updatedWorkplaces).ToList();
            var workplaceSourceIdMap = allWorkplaces.ToDictionary(w => w.ExternalWorkplace.SourceId);

            var employeeWithAddedWorkplaces = new List<Employee>();
            var externalWspEmployeeWorkplaceRepository = uow.GetRepository<ExternalEmployeeWorkplace>();
            var externalWspEmployeeWorkplaces = await externalWspEmployeeWorkplaceRepository.GetWhereAsync(ew => ew.EmployeeWorkplaceId == null);
            foreach (var externalWspEmployeeWorkplace in externalWspEmployeeWorkplaces)
            {
                if (TryCreateFrom(externalWspEmployeeWorkplace, smgToEmployeesMap, workplaceSourceIdMap, out var employeeWorkplace))
                {
                    employeeWorkplace.CreationDate = now;
                    externalWspEmployeeWorkplace.EmployeeWorkplace = employeeWorkplace;
                    employeeWorkplace.Employee.Workplaces.Add(employeeWorkplace);
                    employeeWithAddedWorkplaces.Add(employeeWorkplace.Employee);

                    affectedWorkplacesCount++;
                }
                else
                {
                    LoggerContext.Current.LogWarning("Failed to link employee workplace for {workplaceSourceId} for employee {employeeSourceId}.", externalWspEmployeeWorkplace.SourceWorkplaceId, externalWspEmployeeWorkplace.SourceEmployeeId);
                }
            }

            var employeeWorkplaceRepository = uow.GetRepository<EmployeeWorkplace>();
            var employeeWorkplaceLoadStrategy = new EntityLoadStrategy<EmployeeWorkplace>(
                ew => ew.Workplace,
                ew => ew.Employee.TitleRole,
                ew => ew.Employee.Roles.Select(er => er.Role),
                ew => ew.Employee.Seniority,
                ew => ew.Employee.CurrentLocation.Location);
            var employeeWorkplacesToRemove = await employeeWorkplaceRepository.GetWhereAsync(ew => ew.ExternalEmployeeWorkplace == null, employeeWorkplaceLoadStrategy);
            employeeWorkplaceRepository.DeleteAll(employeeWorkplacesToRemove);

            affectedWorkplacesCount += employeeWorkplacesToRemove.Count;

            var employeeWithRemovedWorkplaces = employeeWorkplacesToRemove.Select(ew => ew.Employee).ToList();
            var updatedEmployees = employeeWithAddedWorkplaces.Union(employeeWithRemovedWorkplaces, EqualityComparerFactory<Employee>.FromKey(e => e.Id)).ToList();

            foreach (var updatedEmployee in updatedEmployees)
            {
                var employeeBeforeChange = employeesBeforeChangeMap[updatedEmployee.ExternalId];
                await _employeeService.AddOrUpdateEmployeeProfileAsync(updatedEmployee);

                var employeeChangedArgs = new EmployeeChangedEventArgs(employeeBeforeChange, updatedEmployee);
                await EmployeeWorkplacesChanged.RaiseAsync(employeeChangedArgs);
            }

            return affectedWorkplacesCount;
        }

        private async void SyncTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await SyncAsync();
        }

        private bool CheckIfSyncRequired(SyncLog lastSyncLog)
        {
            var isSyncRequired = lastSyncLog == null || lastSyncLog.IsOutdated || _environmentInfoService.CurrentUtcDateTime - lastSyncLog.SyncStartDate > _configuration.SyncInterval;

            return isSyncRequired;
        }

        private void UpdateFrom(
            Workplace workplace,
            ExternalWorkplace externalWorkplace,
            IReadOnlyDictionary<string, Core.DepartmentService.DataContracts.OfficeDataContract> officeByImportIdMap)
        {
            var office = officeByImportIdMap[externalWorkplace.OfficeSourceId];

            workplace.Name = externalWorkplace.Name;
            workplace.FullName = externalWorkplace.FullName;
            workplace.SchemeUrl = externalWorkplace.SchemeUrl;
            workplace.OfficeId = office.Id;
        }

        private static bool TryCreateFrom(
            ExternalEmployeeWorkplace externalWspEmployeeWorkplace,
            IReadOnlyDictionary<string, Employee> smgToEmployeeMap,
            IReadOnlyDictionary<string, Workplace> workplaceSourceIdMap,
            out EmployeeWorkplace employeeWorkplace)
        {
            if (!smgToEmployeeMap.TryGetValue(externalWspEmployeeWorkplace.SourceEmployeeId, out var employee))
            {
                employeeWorkplace = null;

                return false;
            }

            if (!workplaceSourceIdMap.TryGetValue(externalWspEmployeeWorkplace.SourceWorkplaceId, out var workplace))
            {
                employeeWorkplace = null;

                return false;
            }

            employeeWorkplace = new EmployeeWorkplace
            {
                EmployeeId = employee.Id,
                Employee = employee,
                WorkplaceId = workplace.Id,
                Workplace = workplace,
            };

            return true;
        }

        private static ExternalEmployeeWorkplace CreateFrom(ConvertedWspEmployeeWorkplace wspEmployeeWorkplace, DateTime now)
        {
            return new ExternalEmployeeWorkplace
            {
                SourceEmployeeId = wspEmployeeWorkplace.EmployeeId,
                SourceWorkplaceId = wspEmployeeWorkplace.WorkplaceId,
                CreationDate = now,
            };
        }

        private static IReadOnlyCollection<ConvertedWspEmployeeWorkplace> CreateFrom(EmployeeWorkplaceDataContract wspEmployeeWorkplace)
        {
            var convertedWspEmployeeWorkplaces = wspEmployeeWorkplace.WorkplaceIds
                .Select(id => new ConvertedWspEmployeeWorkplace
                {
                    EmployeeId = wspEmployeeWorkplace.EmployeeId,
                    WorkplaceId = id,
                })
                .ToList();

            return convertedWspEmployeeWorkplaces;
        }

        private static bool UpdateFrom(ExternalWorkplace externalWorkplace, WorkplaceDataContract wspWorkplace)
        {
            var isUpdated = Reflector.SetProperty(() => externalWorkplace.Name, wspWorkplace.Name) |
                            Reflector.SetProperty(() => externalWorkplace.FullName, wspWorkplace.FullName) |
                            Reflector.SetProperty(() => externalWorkplace.SchemeUrl, wspWorkplace.Uri) |
                            Reflector.SetProperty(() => externalWorkplace.OfficeSourceId, wspWorkplace.OfficeId);

            return isUpdated;
        }



        private sealed class SyncDataResult
        {
            public bool IsSuccessful { get; }

            public int AffectedEntitiesCount { get; }


            private SyncDataResult(bool isSuccessful, int affectedEntitiesCount)
            {
                IsSuccessful = isSuccessful;
                AffectedEntitiesCount = affectedEntitiesCount;
            }


            public static SyncDataResult CreateSuccessful(int affectedEntitiesCount)
            {
                return new SyncDataResult(true, affectedEntitiesCount);
            }

            public static SyncDataResult CreateUnsuccessful()
            {
                return new SyncDataResult(false, 0);
            }
        }

        private sealed class LinkEntitiesResult<TEntity>
        {
            public bool IsSuccessful { get; }

            public int AffectedEntitiesCount { get; }

            public IReadOnlyCollection<TEntity> AffectedEntities { get; }


            private LinkEntitiesResult(bool isSuccessful, int affectedEntitiesCount, IReadOnlyCollection<TEntity> affectedEntities)
            {
                IsSuccessful = isSuccessful;
                AffectedEntitiesCount = affectedEntitiesCount;
                AffectedEntities = affectedEntities;
            }


            public static LinkEntitiesResult<TEntity> CreateSuccessful(int affectedEntitiesCount, IReadOnlyCollection<TEntity> affectedEntities)
            {
                return new LinkEntitiesResult<TEntity>(true, affectedEntitiesCount, affectedEntities);
            }

            public static LinkEntitiesResult<TEntity> CreateUnsuccessful()
            {
                return new LinkEntitiesResult<TEntity>(false, 0, null);
            }
        }

        private sealed class ConvertedWspEmployeeWorkplace
        {
            public string EmployeeId { get; set; }

            public string WorkplaceId { get; set; }
        }
    }
}