using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Core.ProfileService.Providers;
using DreamTeam.DomainModel;
using DreamTeam.Foundation;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.SmgService;
using DreamTeam.Wod.SmgService.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests
{
    [UsedImplicitly]
    public sealed class DismissalRequestSyncService : IDismissalRequestSyncService
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IDismissalRequestSyncServiceConfiguration _configuration;
        private readonly ISmgService _smgService;
        private readonly IEmployeeService _employeeService;
        private readonly ISmgIdProvider _smgIdProvider;

        private readonly Timer _syncTimer;
        private readonly IAsyncResourceLocker _syncLocker;


        public event AsyncObserver<EntityChangedEventArgs<DismissalRequest>> DismissalRequestCreated;

        public event AsyncObserver<EntityChangedEventArgs<DismissalRequest>> DismissalRequestUpdated;


        public DismissalRequestSyncService(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
            IEnvironmentInfoService environmentInfoService,
            IDismissalRequestSyncServiceConfiguration configuration,
            ISmgService smgService,
            IEmployeeService employeeService,
            ISmgIdProvider smgIdProvider)
        {
            _uowFactory = uowFactory;
            _environmentInfoService = environmentInfoService;
            _configuration = configuration;
            _smgService = smgService;
            _employeeService = employeeService;
            _smgIdProvider = smgIdProvider;

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

                var isDownloadedSuccessfully = await DownloadAndSaveExternalDismissalRequestsAsync();
                if (!isDownloadedSuccessfully)
                {
                    return false;
                }

                var isLinkedSuccessfully = await LinkExternalDismissalRequestsAsync();

                return isLinkedSuccessfully;
            }
        }


        private async Task<bool> DownloadAndSaveExternalDismissalRequestsAsync()
        {
            using var uow = _uowFactory.Create();
            var syncLogRepository = uow.SyncLogs;
            var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalDismissalRequestData);
            var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
            if (!isSyncRequired)
            {
                return true;
            }

            var previousSyncStartDate = lastSyncLog?.IsOutdated == true ? null : lastSyncLog?.SyncStartDate;
            var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
            var syncLog = new SyncLog
            {
                SyncStartDate = syncStartDate,
                Type = SyncType.DownloadExternalDismissalRequestData,
            };

            LoggerContext.Current.Log("Starting dismissal requests download...");
            var isCompletedSuccessfully = false;
            try
            {
                var downloadAndSaveDismissalRequestDataResult = await DownloadAndSaveDismissalRequestsDataAsync(syncStartDate, previousSyncStartDate);
                isCompletedSuccessfully = downloadAndSaveDismissalRequestDataResult.IsSuccessful;
                syncLog.AffectedEntitiesCount = downloadAndSaveDismissalRequestDataResult.AffectedEntitiesCount;

                return isCompletedSuccessfully;
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError("Failed to download dismissal requests.", ex);

                return false;
            }
            finally
            {
                if (isCompletedSuccessfully)
                {
                    LoggerContext.Current.Log("Dismissal requests download completed successfully. Affected dismissal requests - {affectedDismissalRequestsCount}.", syncLog.AffectedEntitiesCount);
                }
                else
                {
                    LoggerContext.Current.LogWarning("Dismissal requests download failed.");
                }

                syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                syncLog.IsSuccessful = isCompletedSuccessfully;
                syncLogRepository.Add(syncLog);
                await uow.SaveChangesAsync();
            }
        }

        private async Task<DownloadDataResult> DownloadAndSaveDismissalRequestsDataAsync(DateTime creationDate, DateTime? previousSyncStartDate = null)
        {
            using var uow = _uowFactory.Create();

            List<string> employeeSmgIdsToSync = null;
            if (_configuration.EmployeeIdsToSync != null)
            {
                var employeeIdSmgIdMap = await _smgIdProvider.GetEmployeeIdToSmgIdMapAsync();
                employeeSmgIdsToSync = _configuration.EmployeeIdsToSync
                    .Where(employeeIdSmgIdMap.ContainsKey)
                    .Select(i => employeeIdSmgIdMap[i])
                    .ToList();
            }

            var dismissalRequestRepository = uow.GetRepository<ExternalDismissalRequest>();
            var existingDismissalRequests = await dismissalRequestRepository.GetAllAsync();
            var existingDismissalRequestsMap = existingDismissalRequests.ToDictionary(r => r.SourceId);

            var affectedDismissalRequestsCount = 0;
            var now = _environmentInfoService.CurrentUtcDateTime;
            const int take = 1000;
            var hasNext = true;
            var skip = 0;
            while (hasNext)
            {
                var downloadDismissalRequestsResult = await _smgService.GetDismissalRequestsAsync(skip, take, previousSyncStartDate);
                if (!downloadDismissalRequestsResult.IsSuccessful)
                {
                    LoggerContext.Current.LogWarning("Failed to download dismissal requests");
                    return DownloadDataResult.CreateUnsuccessful();
                }

                var smgDismissalRequestsItems = downloadDismissalRequestsResult.Result;

                var dismissalRequests = smgDismissalRequestsItems.Items;
                var filteredDismissalRequests = employeeSmgIdsToSync != null
                    ? dismissalRequests.Where(e => employeeSmgIdsToSync.Contains(e.EmployeeId)).ToList()
                    : dismissalRequests;

                foreach (var dismissalRequest in filteredDismissalRequests)
                {
                    if (existingDismissalRequestsMap.TryGetValue(dismissalRequest.Id, out var internalDismissalRequest))
                    {
                        var isUpdated = UpdateFrom(internalDismissalRequest, dismissalRequest);
                        if (isUpdated)
                        {
                            internalDismissalRequest.UpdateDate = now;
                            affectedDismissalRequestsCount++;
                        }
                    }
                    else
                    {
                        var newInternalDismissalRequest = new ExternalDismissalRequest
                        {
                            SourceId = dismissalRequest.Id,
                            CreationDate = creationDate,
                        };

                        UpdateFrom(newInternalDismissalRequest, dismissalRequest);
                        dismissalRequestRepository.Add(newInternalDismissalRequest);
                        existingDismissalRequestsMap.Add(newInternalDismissalRequest.SourceId, newInternalDismissalRequest);
                        affectedDismissalRequestsCount++;
                    }
                }

                hasNext = smgDismissalRequestsItems.HasNext;
                skip += take;
            }

            await uow.SaveChangesAsync();

            return DownloadDataResult.CreateSuccessful(affectedDismissalRequestsCount);
        }

        private async Task<bool> LinkExternalDismissalRequestsAsync()
        {
            using var uow = _uowFactory.Create();
            var syncLogRepository = uow.SyncLogs;
            var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkDismissalRequests);
            var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
            if (!isSyncRequired)
            {
                return true;
            }

            var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
            var syncLog = new SyncLog
            {
                SyncStartDate = syncStartDate,
                Type = SyncType.LinkDismissalRequests,
            };

            LoggerContext.Current.Log("Starting dismissal requests linking...");
            try
            {
                syncLog.AffectedEntitiesCount = await LinkDismissalRequestsAsync(syncStartDate);
                syncLog.IsSuccessful = true;

                LoggerContext.Current.Log("Dismissal requests linked successfully. Affected dismissal requests - {affectedDismissalRequestsCount}.", syncLog.AffectedEntitiesCount);

                return true;
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError("Failed to link dismissal requests.", ex);

                return false;
            }
            finally
            {
                syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                syncLogRepository.Add(syncLog);
                await uow.SaveChangesAsync();
            }
        }

        private async Task<int> LinkDismissalRequestsAsync(DateTime now)
        {
            var affectedDismissalRequestsCount = 0;
            using var uow = _uowFactory.Create();

            var externalDismissalRequestRepository = uow.GetRepository<ExternalDismissalRequest>();
            var externalDismissalRequests = await externalDismissalRequestRepository.GetWhereAsync(r => r.DismissalSpecificId == SmgDismissalRequestTypes.Ordinary);

            var dismissalRequestRepository = uow.GetRepository<DismissalRequest>();
            var loadStrategy = GetDismissalRequestLoadStrategy();
            var dismissalRequests = await dismissalRequestRepository.GetWhereAsync(DismissalRequestSpecification.IsLinked, loadStrategy);
            var dismissalRequestsMap = dismissalRequests.ToDictionary(r => r.SourceDismissalRequestId);

            var smgToEmployeesMap = await _employeeService.GetSmgToEmployeesMapAsync(uow);

            var createdDismissalRequests = new List<DismissalRequest>();
            var changedDismissalRequests = new List<EntityChangedEventArgs<DismissalRequest>>();

            foreach (var externalDismissalRequest in externalDismissalRequests)
            {
                var employee = smgToEmployeesMap.GetValueOrDefault(externalDismissalRequest.SourceEmployeeId);
                if (employee == null)
                {
                    LoggerContext.Current.LogWarning("Failed to match employee with {sourceEmployeeId}", externalDismissalRequest.SourceEmployeeId);

                    continue;
                }

                if (dismissalRequestsMap.TryGetValue(externalDismissalRequest.Id, out var dismissalRequest))
                {
                    var previousDismissalRequest = dismissalRequest.Clone();

                    var isUpdated = UpdateFrom(dismissalRequest, externalDismissalRequest, employee);
                    if (isUpdated)
                    {
                        dismissalRequest.UpdateDate = now;

                        affectedDismissalRequestsCount++;

                        changedDismissalRequests.Add(new EntityChangedEventArgs<DismissalRequest>(previousDismissalRequest, dismissalRequest));
                    }

                    continue;
                }

                var newDismissalRequest = new DismissalRequest
                {
                    ExternalId = ExternalId.Generate(),
                    SourceDismissalRequest = externalDismissalRequest,
                    CreationDate = now,
                };

                UpdateFrom(newDismissalRequest, externalDismissalRequest, employee);
                dismissalRequestRepository.Add(newDismissalRequest);

                affectedDismissalRequestsCount++;

                createdDismissalRequests.Add(newDismissalRequest);
            }

            await uow.SaveChangesAsync();

            foreach (var request in createdDismissalRequests)
            {
                await DismissalRequestCreated.RaiseAsync(new EntityChangedEventArgs<DismissalRequest>(request));
            }

            foreach (var request in changedDismissalRequests)
            {
                await DismissalRequestUpdated.RaiseAsync(request);
            }

            return affectedDismissalRequestsCount;
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

        private static bool UpdateFrom(
            DismissalRequest dismissalRequest,
            ExternalDismissalRequest externalDismissalRequest,
            Employee employee)
        {
            var dismissalType = GetDismissalRequestType(externalDismissalRequest.DismissalSpecificId);

            var isUpdated = Reflector.SetProperty(() => dismissalRequest.IsActive, externalDismissalRequest.IsActive) |
                            Reflector.SetProperty(() => dismissalRequest.DismissalDate, externalDismissalRequest.DismissalDate) |
                            Reflector.SetProperty(() => dismissalRequest.CloseDate, externalDismissalRequest.CloseDate) |
                            Reflector.SetProperty(() => dismissalRequest.Type, dismissalType) |
                            Reflector.SetProperty(() => dismissalRequest.EmployeeId, employee.Id);

            return isUpdated;
        }

        private static bool UpdateFrom(ExternalDismissalRequest dismissalRequest, SmgDismissalRequestDataContract smgDismissalRequest)
        {
            var isUpdated = Reflector.SetProperty(() => dismissalRequest.SourceId, smgDismissalRequest.Id) |
                            Reflector.SetProperty(() => dismissalRequest.SourceEmployeeId, smgDismissalRequest.EmployeeId) |
                            Reflector.SetProperty(() => dismissalRequest.IsActive, smgDismissalRequest.IsActive) |
                            Reflector.SetProperty(() => dismissalRequest.SourceCreationDate, smgDismissalRequest.CreationDate) |
                            Reflector.SetProperty(() => dismissalRequest.DismissalDate, smgDismissalRequest.DismissalDate) |
                            Reflector.SetProperty(() => dismissalRequest.CloseDate, smgDismissalRequest.CloseDate) |
                            Reflector.SetProperty(() => dismissalRequest.DismissalSpecificId, smgDismissalRequest.DismissalSpecificId);

            return isUpdated;
        }

        private static DismissalRequestType GetDismissalRequestType(string dismissalSpecificId)
        {
            return dismissalSpecificId switch
            {
                SmgDismissalRequestTypes.Ordinary => DismissalRequestType.Ordinary,
                SmgDismissalRequestTypes.ChangeOfContractType => DismissalRequestType.ContractChange,
                SmgDismissalRequestTypes.MaternityLeave => DismissalRequestType.MaternityLeave,
                SmgDismissalRequestTypes.Relocation => DismissalRequestType.Relocation,
                _ => throw new ArgumentOutOfRangeException(nameof(dismissalSpecificId), dismissalSpecificId, "Type is not supported."),
            };
        }

        private static IEntityLoadStrategy<DismissalRequest> GetDismissalRequestLoadStrategy()
        {
            return new EntityLoadStrategy<DismissalRequest>(r => r.Employee);
        }



        private sealed class PeopleWithProfilesData
        {
            public required IReadOnlyCollection<PersonWithProfileDataContract<SmgProfileDataContract>> PeopleWithSmgProfiles { get; init; }

            public required IReadOnlyCollection<PersonWithProfileDataContract<EmployeeProfileDataContract>> PeopleWithEmployeeProfiles { get; init; }
        }
    }
}
