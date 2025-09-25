using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Timers;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.DomainModel;
using DreamTeam.Foundation;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.SmgService;
using DreamTeam.Wod.SmgService.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;

[UsedImplicitly]
public sealed class EmployeeUnitHistorySyncService : IEmployeeUnitHistorySyncService
{
    private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
    private readonly IEnvironmentInfoService _environmentInfoService;
    private readonly ISmgService _smgService;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly IEmployeeUnitHistorySyncServiceConfiguration _configuration;

    private readonly Timer _syncTimer;
    private readonly IAsyncResourceLocker _syncLocker;


    public event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryCreated;

    public event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryUpdated;

    public event AsyncObserver<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>> EmployeeUnitHistoryDeleted;


    public EmployeeUnitHistorySyncService(
        IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
        IEnvironmentInfoService environmentInfoService,
        ISmgService smgService,
        IEmployeeService employeeService,
        IDepartmentService departmentService,
        IEmployeeUnitHistorySyncServiceConfiguration configuration)
    {
        _uowFactory = uowFactory;
        _environmentInfoService = environmentInfoService;
        _smgService = smgService;
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
        using var uow = _uowFactory.Create();
        var syncLogRepository = uow.SyncLogs;
        var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalEmployeeUnitHistory);
        var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
        if (!isSyncRequired)
        {
            return true;
        }

        var previousSyncStartDate = lastSyncLog is { IsOutdated: false }
            ? lastSyncLog.SyncStartDate
            : (DateTime?)null;

        var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
        var syncLog = new SyncLog
        {
            SyncStartDate = syncStartDate,
            Type = SyncType.DownloadExternalEmployeeUnitHistory,
        };

        LoggerContext.Current.Log("Starting employee unit history download...");
        var isCompletedSuccessfully = false;
        try
        {
            var downloadAndSaveEmployeeUnitHistoryResult = await DownloadAndSaveEmployeeUnitHistoryAsync(syncStartDate, previousSyncStartDate);
            isCompletedSuccessfully = downloadAndSaveEmployeeUnitHistoryResult.IsSuccessful;
            syncLog.AffectedEntitiesCount = downloadAndSaveEmployeeUnitHistoryResult.AffectedEntitiesCount;

            return isCompletedSuccessfully;
        }
        catch (Exception ex) when (ex.IsCatchableExceptionType())
        {
            LoggerContext.Current.LogError("Failed to download employee unit history.", ex);

            return false;
        }
        finally
        {
            if (isCompletedSuccessfully)
            {
                LoggerContext.Current.Log("Employee unit history download completed successfully. Affected items - {affectedEmployeeUnitHistoryCount}.", syncLog.AffectedEntitiesCount);
            }
            else
            {
                LoggerContext.Current.LogWarning("Employee unit history download failed.");
            }

            syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
            syncLog.IsSuccessful = isCompletedSuccessfully;
            syncLogRepository.Add(syncLog);
            await uow.SaveChangesAsync();
        }
    }

    private async Task<bool> LinkExternalDataAsync()
    {
        using var uow = _uowFactory.Create();
        var syncLogRepository = uow.SyncLogs;
        var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkEmployeeUnitHistory);
        var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
        if (!isSyncRequired)
        {
            return true;
        }

        var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
        var syncLog = new SyncLog
        {
            SyncStartDate = syncStartDate,
            Type = SyncType.LinkEmployeeUnitHistory,
        };

        LoggerContext.Current.Log("Starting employee unit history linking...");
        var isCompletedSuccessfully = false;
        try
        {
            var linkEmployeeUnitHistoryResult = await LinkEmployeeUnitHistoryAsync(syncStartDate, lastSyncLog?.SyncStartDate);
            isCompletedSuccessfully = linkEmployeeUnitHistoryResult.IsSuccessful;
            syncLog.AffectedEntitiesCount = linkEmployeeUnitHistoryResult.AffectedEntitiesCount;

            return isCompletedSuccessfully;
        }
        catch (Exception ex) when (ex.IsCatchableExceptionType())
        {
            LoggerContext.Current.LogError("Failed to link employee unit history.", ex);

            return false;
        }
        finally
        {
            if (isCompletedSuccessfully)
            {
                LoggerContext.Current.Log("Employee unit history linked successfully. Affected entities - {affectedEntitiesCount}.", syncLog.AffectedEntitiesCount);
            }
            else
            {
                LoggerContext.Current.LogWarning("Employee unit history link failed.");
            }

            syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
            syncLog.IsSuccessful = isCompletedSuccessfully;
            syncLogRepository.Add(syncLog);
            await uow.SaveChangesAsync();
        }
    }

    private async Task<SyncDataResult> DownloadAndSaveEmployeeUnitHistoryAsync(DateTime now, DateTime? previousSyncStartDate)
    {
        var getSmgEmployeeUnitHistoryResult = await _smgService.GetEmployeeUnitHistoryAsync(previousSyncStartDate);
        if (!getSmgEmployeeUnitHistoryResult.IsSuccessful)
        {
            LoggerContext.Current.LogWarning("Failed to get SMG employee unit history. Error codes: {errorCodes}.", getSmgEmployeeUnitHistoryResult.ErrorCodes.JoinStrings());

            return SyncDataResult.CreateUnsuccessful();
        }

        var smgEmployeeUnitHistory = getSmgEmployeeUnitHistoryResult.Result;
        if (!smgEmployeeUnitHistory.Any())
        {
            return SyncDataResult.CreateSuccessful(affectedEntitiesCount: 0);
        }

        using var uow = _uowFactory.Create();

        var externalEmployeeUnitHistoryRepository = uow.GetRepository<ExternalEmployeeUnitHistory>();

        var smgEmployeeUnitHistoryEmployeeIds = smgEmployeeUnitHistory.Select(h => h.EmployeeId).ToHashSet();
        var dbExternalEmployeeUnitHistory = previousSyncStartDate.HasValue
            ? await externalEmployeeUnitHistoryRepository.GetWhereAsync(ExternalEmployeeUnitHistorySpecification.BySourceEmployeeIds(smgEmployeeUnitHistoryEmployeeIds))
            : await externalEmployeeUnitHistoryRepository.GetAllAsync();
        var serviceExternalEmployeeUnitHistory = smgEmployeeUnitHistory.SelectMany(CreateFromSmgEmployeeUnitHistory).ToList();

        var externalEmployeeUnitHistoryEqualityComparer = new ExternalEmployeeUnitHistoryEqualityComparer();
        var externalEmployeeUnitHistoryToDelete = dbExternalEmployeeUnitHistory
            .Except(serviceExternalEmployeeUnitHistory, externalEmployeeUnitHistoryEqualityComparer)
            .ToList();
        var externalEmployeeUnitHistoryToAdd = serviceExternalEmployeeUnitHistory
            .Except(dbExternalEmployeeUnitHistory, externalEmployeeUnitHistoryEqualityComparer)
            .ToList();
        externalEmployeeUnitHistoryToAdd.ForEach(h => h.CreationDate = now);
        var externalEmployeeUnitHistoryToUpdate = dbExternalEmployeeUnitHistory
            .Intersect(serviceExternalEmployeeUnitHistory, externalEmployeeUnitHistoryEqualityComparer)
            .ToList();
        var serviceExternalEmployeeUnitHistoryMap = serviceExternalEmployeeUnitHistory.ToDictionary(GetExternalEmployeeUnitHistoryKey);
        var affectedEntitiesCount = 0;
        foreach (var externalEmployeeUnitHistoryToUpdateItem in externalEmployeeUnitHistoryToUpdate)
        {
            var key = GetExternalEmployeeUnitHistoryKey(externalEmployeeUnitHistoryToUpdateItem);
            var serviceExternalEmployeeUnitHistoryItem = serviceExternalEmployeeUnitHistoryMap[key];
            if (externalEmployeeUnitHistoryToUpdateItem.EndDate != serviceExternalEmployeeUnitHistoryItem.EndDate)
            {
                externalEmployeeUnitHistoryToUpdateItem.EndDate = serviceExternalEmployeeUnitHistoryItem.EndDate;
                externalEmployeeUnitHistoryToUpdateItem.UpdateDate = now;
                affectedEntitiesCount++;
            }
        }

        externalEmployeeUnitHistoryRepository.AddRange(externalEmployeeUnitHistoryToAdd);
        externalEmployeeUnitHistoryRepository.DeleteAll(externalEmployeeUnitHistoryToDelete);

        await uow.SaveChangesAsync();

        affectedEntitiesCount += externalEmployeeUnitHistoryToAdd.Count + externalEmployeeUnitHistoryToDelete.Count;

        return SyncDataResult.CreateSuccessful(affectedEntitiesCount);
    }

    private async Task<SyncDataResult> LinkEmployeeUnitHistoryAsync(DateTime now, DateTime? previousSyncStartDate)
    {
        var affectedEntitiesCount = 0;
        using var uow = _uowFactory.Create();

        var employeeBySmgIdMap = await _employeeService.GetSmgToEmployeesMapAsync(uow);

        var units = await _departmentService.GetUnitsAsync();
        var unitByImportIdMap = units.ToDictionary(u => u.ImportId);

        var externalEmployeeUnitHistoryRepository = uow.ExternalEmployeeUnitHistory;
        var sourceIdsOfEmployeesWithMultipleActiveHistoryItems = await externalEmployeeUnitHistoryRepository.GetSourceIdsOfEmployeesWithMultipleActiveHistoryItemsAsync();
        if (sourceIdsOfEmployeesWithMultipleActiveHistoryItems.Any())
        {
            var sourceIdsString = sourceIdsOfEmployeesWithMultipleActiveHistoryItems.JoinStrings();
            LoggerContext.Current.LogWarning("Employee unit history for employees [{sourceIds}] contains multiple active items which is not permitted.", sourceIdsString);
            return SyncDataResult.CreateUnsuccessful();
        }
        var loadStrategy = GetExternalEmployeeUnitHistoryLoadStrategy();
        var externalEmployeeUnitHistoryToLink = await externalEmployeeUnitHistoryRepository
            .GetWhereAsync(ExternalEmployeeUnitHistorySpecification.NotLinked | ExternalEmployeeUnitHistorySpecification.InternalEmployeeUnitHistoryIsOutdated, loadStrategy);

        var createdEmployeeUnitHistoryEventArgs = new List<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>>();
        var updatedEmployeeUnitHistoryEventArgs = new List<EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>>();
        foreach (var externalEmployeeUnitHistoryToLinkItem in externalEmployeeUnitHistoryToLink)
        {
            if (externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory == null)
            {
                if (TryLinkEmployeeUnitHistory(externalEmployeeUnitHistoryToLinkItem, employeeBySmgIdMap, unitByImportIdMap, out var linkedEmployeeUnitHistory))
                {
                    linkedEmployeeUnitHistory.CreationDate = now;
                    externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory = linkedEmployeeUnitHistory;
                    createdEmployeeUnitHistoryEventArgs.Add(new EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>(linkedEmployeeUnitHistory));
                    affectedEntitiesCount++;
                }
                else
                {
                    if (previousSyncStartDate == null || (externalEmployeeUnitHistoryToLinkItem.UpdateDate ??
                                                          externalEmployeeUnitHistoryToLinkItem.CreationDate) > previousSyncStartDate)
                    {
                        LoggerContext.Current.LogWarning(
                            "Failed to link employee unit history for employee {employeeSourceId} in unit {unitSourceId}.",
                            externalEmployeeUnitHistoryToLinkItem.SourceEmployeeId,
                            externalEmployeeUnitHistoryToLinkItem.SourceUnitId);
                    }
                }
            }
            else
            {
                var previousEmployeeUnitHistory = externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory.Clone();
                var isUpdated = UpdateFrom(externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory, externalEmployeeUnitHistoryToLinkItem);
                if (isUpdated)
                {
                    externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory.UpdateDate = now;
                    updatedEmployeeUnitHistoryEventArgs.Add(new EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>(previousEmployeeUnitHistory, externalEmployeeUnitHistoryToLinkItem.EmployeeUnitHistory));
                    affectedEntitiesCount++;
                }
            }
        }

        var employeeUnitHistoryRepository = uow.GetRepository<DomainModel.EmployeeUnitHistory>();
        var notLinkedEmployeeUnitHistory = await employeeUnitHistoryRepository.GetWhereAsync(EmployeeUnitHistorySpecification.NotLinked);
        employeeUnitHistoryRepository.DeleteAll(notLinkedEmployeeUnitHistory);
        var deletedEmployeeUnitHistoryEventArgs = notLinkedEmployeeUnitHistory.Select(h => new EntityChangedEventArgs<DomainModel.EmployeeUnitHistory>(h, null)).ToList();

        affectedEntitiesCount += notLinkedEmployeeUnitHistory.Count;

        await uow.SaveChangesAsync();

        foreach (var createdEmployeeUnitHistoryEventArg in createdEmployeeUnitHistoryEventArgs)
        {
            await EmployeeUnitHistoryCreated.RaiseAsync(createdEmployeeUnitHistoryEventArg);
        }

        foreach (var updatedEmployeeUnitHistoryEventArg in updatedEmployeeUnitHistoryEventArgs)
        {
            await EmployeeUnitHistoryUpdated.RaiseAsync(updatedEmployeeUnitHistoryEventArg);
        }

        foreach (var deletedEmployeeUnitHistoryEventArg in deletedEmployeeUnitHistoryEventArgs)
        {
            await EmployeeUnitHistoryDeleted.RaiseAsync(deletedEmployeeUnitHistoryEventArg);
        }

        return SyncDataResult.CreateSuccessful(affectedEntitiesCount);
    }

    private static bool TryLinkEmployeeUnitHistory(
        ExternalEmployeeUnitHistory externalEmployeeUnitHistory,
        IReadOnlyDictionary<string, Employee> employeeBySmgIdMap,
        IReadOnlyDictionary<string, UnitDataContract> unitByImportIdMap,
        out DomainModel.EmployeeUnitHistory employeeUnitHistory)
    {
        employeeUnitHistory = null;

        if (!employeeBySmgIdMap.TryGetValue(externalEmployeeUnitHistory.SourceEmployeeId, out var employee))
        {
            return false;
        }

        if (!unitByImportIdMap.TryGetValue(externalEmployeeUnitHistory.SourceUnitId, out var unit))
        {
            return false;
        }

        employeeUnitHistory = new DomainModel.EmployeeUnitHistory
        {
            ExternalId = ExternalId.Generate(),
            EmployeeId = employee.Id,
            Employee = employee,
            UnitId = unit.Id,
            StartDate = externalEmployeeUnitHistory.StartDate,
            EndDate = externalEmployeeUnitHistory.EndDate,
        };

        return true;
    }

    private async void SyncTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            await SyncAsync();
        }
        catch (Exception ex) when (ex.IsCatchableExceptionType())
        {
            LoggerContext.Current.LogError("Failed to sync employee unit history.", ex);
        }
    }

    private bool CheckIfSyncRequired(SyncLog lastSyncLog)
    {
        var isSyncRequired = lastSyncLog == null || lastSyncLog.IsOutdated || _environmentInfoService.CurrentUtcDateTime - lastSyncLog.SyncStartDate > _configuration.SyncInterval;

        return isSyncRequired;
    }

    private static IReadOnlyCollection<ExternalEmployeeUnitHistory> CreateFromSmgEmployeeUnitHistory(SmgEmployeeUnitHistoryDataContract smgEmployeeUnitHistory)
    {
        var flatEmployeeUnitHistory = smgEmployeeUnitHistory.History
            .Select(h => new ExternalEmployeeUnitHistory
            {
                SourceEmployeeId = smgEmployeeUnitHistory.EmployeeId,
                SourceUnitId = h.UnitId,
                StartDate = h.StartDate,
                EndDate = h.EndDate,
            })
            .ToList();

        return flatEmployeeUnitHistory;
    }

    private object GetExternalEmployeeUnitHistoryKey(ExternalEmployeeUnitHistory history)
    {
        return new { history.SourceEmployeeId, history.SourceUnitId, history.StartDate };
    }

    private static bool UpdateFrom(DomainModel.EmployeeUnitHistory employeeUnitHistory, ExternalEmployeeUnitHistory externalEmployeeUnitHistory)
    {
        var isUpdated = Reflector.SetProperty(() => employeeUnitHistory.StartDate, externalEmployeeUnitHistory.StartDate) |
                        Reflector.SetProperty(() => employeeUnitHistory.EndDate, externalEmployeeUnitHistory.EndDate);

        return isUpdated;
    }

    private static IEntityLoadStrategy<ExternalEmployeeUnitHistory> GetExternalEmployeeUnitHistoryLoadStrategy()
    {
        return new EntityLoadStrategy<ExternalEmployeeUnitHistory>(h => h.EmployeeUnitHistory);
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

    private sealed class ExternalEmployeeUnitHistoryEqualityComparer : IEqualityComparer<ExternalEmployeeUnitHistory>
    {
        public bool Equals([CanBeNull]ExternalEmployeeUnitHistory x, [CanBeNull]ExternalEmployeeUnitHistory y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.SourceEmployeeId == y.SourceEmployeeId &&
                   x.SourceUnitId == y.SourceUnitId &&
                   x.StartDate.Equals(y.StartDate);
        }

        public int GetHashCode(ExternalEmployeeUnitHistory obj)
        {
            return HashCode.Combine(obj.SourceEmployeeId, obj.SourceUnitId, obj.StartDate);
        }
    }

    private sealed class ExternalEmployeeUnitHistorySpecification : Specification<ExternalEmployeeUnitHistory>
    {
        public static ExternalEmployeeUnitHistorySpecification NotLinked
            => new(h => h.EmployeeUnitHistoryId == null);

        public static ExternalEmployeeUnitHistorySpecification InternalEmployeeUnitHistoryIsOutdated
            => new(h =>
                h.UpdateDate != null &&
                h.EmployeeUnitHistory != null &&
                h.UpdateDate > (h.EmployeeUnitHistory.UpdateDate ?? h.EmployeeUnitHistory.CreationDate));


        private ExternalEmployeeUnitHistorySpecification(Expression<Func<ExternalEmployeeUnitHistory, bool>> predicate)
            : base(predicate)
        {
        }


        public static ExternalEmployeeUnitHistorySpecification BySourceEmployeeIds(IReadOnlyCollection<string> employeeIds)
            => new(h => employeeIds.Contains(h.SourceEmployeeId));
    }
}