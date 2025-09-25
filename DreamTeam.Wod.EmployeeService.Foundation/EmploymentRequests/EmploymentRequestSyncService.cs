using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Diacritical;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.ProfileService;
using DreamTeam.Core.ProfileService.DataContracts;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.SmgService;
using DreamTeam.Wod.SmgService.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests
{
    [UsedImplicitly]
    public sealed class EmploymentRequestSyncService : IEmploymentRequestSyncService
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IEmploymentRequestSyncServiceConfiguration _configuration;
        private readonly ISmgService _smgService;
        private readonly IDepartmentService _departmentService;
        private readonly IProfileService _profileService;
        private readonly IEmployeeService _employeeService;

        private readonly Timer _syncTimer;
        private readonly IAsyncResourceLocker _syncLocker;


        public EmploymentRequestSyncService(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
            IEnvironmentInfoService environmentInfoService,
            IEmploymentRequestSyncServiceConfiguration configuration,
            ISmgService smgService,
            IDepartmentService departmentService,
            IProfileService profileService,
            IEmployeeService employeeService)
        {
            _uowFactory = uowFactory;
            _environmentInfoService = environmentInfoService;
            _configuration = configuration;
            _smgService = smgService;
            _departmentService = departmentService;
            _profileService = profileService;
            _employeeService = employeeService;

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


        private async Task SyncAsync()
        {
            if (!_configuration.Enable)
            {
                return;
            }

            using (_syncLocker.TryGetAccess(out var isAccessed))
            {
                if (!isAccessed)
                {
                    return;
                }

                var isDownloadedSuccessfully = await DownloadAndSaveExternalEmploymentRequestsAsync();
                if (!isDownloadedSuccessfully)
                {
                    return;
                }

                await LinkExternalEmploymentRequestsAsync();
            }
        }


        private async Task<bool> DownloadAndSaveExternalEmploymentRequestsAsync()
        {
            using var uow = _uowFactory.Create();
            var syncLogRepository = uow.SyncLogs;
            var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalEmploymentRequestData);
            var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
            if (!isSyncRequired)
            {
                return true;
            }

            var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
            var syncLog = new SyncLog
            {
                SyncStartDate = syncStartDate,
                Type = SyncType.DownloadExternalEmploymentRequestData,
            };

            LoggerContext.Current.Log("Starting employment requests download...");
            var isCompletedSuccessfully = false;
            try
            {
                var downloadAndSaveEmploymentRequestDataResult = await DownloadAndSaveEmploymentRequestsDataAsync(syncStartDate);
                isCompletedSuccessfully = downloadAndSaveEmploymentRequestDataResult.IsSuccessful;
                syncLog.AffectedEntitiesCount = downloadAndSaveEmploymentRequestDataResult.AffectedEntitiesCount;

                return isCompletedSuccessfully;
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError("Failed to download employment requests.", ex);

                return false;
            }
            finally
            {
                if (isCompletedSuccessfully)
                {
                    LoggerContext.Current.Log("Employment requests download completed successfully. Affected employment requests - {affectedEmploymentRequestsCount}.", syncLog.AffectedEntitiesCount);
                }
                else
                {
                    LoggerContext.Current.LogWarning("Employment requests download failed.");
                }

                syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                syncLog.IsSuccessful = isCompletedSuccessfully;
                syncLogRepository.Add(syncLog);
                await uow.SaveChangesAsync();
            }
        }

        private async Task<DownloadDataResult> DownloadAndSaveEmploymentRequestsDataAsync(DateTime now)
        {
            var downloadEmploymentRequestsResult = await _smgService.GetEmploymentRequestsAsync();
            if (!downloadEmploymentRequestsResult.IsSuccessful)
            {
                LoggerContext.Current.LogWarning("Failed to download employment requests");
                return DownloadDataResult.CreateUnsuccessful();
            }

            using var uow = _uowFactory.Create();

            var employmentRequestRepository = uow.GetRepository<ExternalEmploymentRequest>();
            var internalEmploymentRequests = await employmentRequestRepository.GetAllAsync();
            var internalEmploymentRequestsMap = internalEmploymentRequests.ToDictionary(r => r.SourceId);

            var affectedEmploymentRequestsCount = 0;
            var employmentRequests = downloadEmploymentRequestsResult.Result;

            foreach (var employmentRequest in employmentRequests)
            {
                if (internalEmploymentRequestsMap.TryGetValue(employmentRequest.Id, out var internalEmploymentRequest))
                {
                    var isUpdated = UpdateFrom(internalEmploymentRequest, employmentRequest);
                    if (isUpdated)
                    {
                        internalEmploymentRequest.UpdateDate = _environmentInfoService.CurrentUtcDateTime;
                        affectedEmploymentRequestsCount++;
                    }
                }
                else
                {
                    var newInternalEmploymentRequest = new ExternalEmploymentRequest
                    {
                        SourceId = employmentRequest.Id,
                        CreationDate = now,
                    };

                    UpdateFrom(newInternalEmploymentRequest, employmentRequest);
                    employmentRequestRepository.Add(newInternalEmploymentRequest);
                    affectedEmploymentRequestsCount++;
                }
            }

            var downloadedEmploymentRequestIds = employmentRequests.Select(r => r.Id).ToList();
            var closedEmploymentRequests = internalEmploymentRequests.Where(r => !downloadedEmploymentRequestIds.Contains(r.SourceId) && !r.CloseDate.HasValue).ToList();
            foreach (var employmentRequest in closedEmploymentRequests)
            {
                employmentRequest.CloseDate = now;
                employmentRequest.UpdateDate = now;
            }

            await uow.SaveChangesAsync();

            return DownloadDataResult.CreateSuccessful(affectedEmploymentRequestsCount);
        }

        private async Task LinkExternalEmploymentRequestsAsync()
        {
            using var uow = _uowFactory.Create();
            var syncLogRepository = uow.SyncLogs;
            var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkEmploymentRequests);
            var isSyncRequired = CheckIfSyncRequired(lastSyncLog);
            if (!isSyncRequired)
            {
                return;
            }

            var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
            var syncLog = new SyncLog
            {
                SyncStartDate = syncStartDate,
                Type = SyncType.LinkEmploymentRequests,
            };

            LoggerContext.Current.Log("Starting Employment requests linking...");
            try
            {
                syncLog.AffectedEntitiesCount = await LinkEmploymentRequestsAsync(syncStartDate);
                syncLog.IsSuccessful = true;

                LoggerContext.Current.Log("Employment requests linked successfully. Affected employment requests - {affectedEmploymentRequestsCount}.", syncLog.AffectedEntitiesCount);
            }
            catch (Exception ex) when (ex.IsCatchableExceptionType())
            {
                LoggerContext.Current.LogError("Failed to link Employment requests.", ex);
            }
            finally
            {
                syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                syncLogRepository.Add(syncLog);
                await uow.SaveChangesAsync();
            }
        }

        private async Task<int> LinkEmploymentRequestsAsync(DateTime now)
        {
            var organizations = await _departmentService.GetOrganizationsAsync();
            var organizationByImportIdMap = organizations.ToDictionary(o => o.ImportId);

            var units = await _departmentService.GetUnitsAsync();
            var unitImportIdMap = units.ToDictionary(u => u.ImportId, u => u.Id);

            var affectedEmploymentRequestsCount = 0;
            using var uow = _uowFactory.Create();

            var externalEmploymentRequestRepository = uow.GetRepository<ExternalEmploymentRequest>();
            var externalEmploymentRequests = await externalEmploymentRequestRepository.GetAllAsync();

            var employmentRequestRepository = uow.GetRepository<EmploymentRequest>();
            var loadStrategy = GetEmploymentRequestLoadStrategy();
            var employmentRequests = await employmentRequestRepository.GetAllAsync(loadStrategy);
            var employmentRequestsMap = employmentRequests.ToDictionary(r => r.SourceId);
            var employmentRequestsWithEmployees = employmentRequests.Where(r => r.Employee != null).Select(r => r.SourceId).ToImmutableHashSet();

            var fullNames = externalEmploymentRequests
                .Where(r => !employmentRequestsWithEmployees.Contains(r.Id))
                .Select(r => $"{r.LastName} {r.FirstName}".RemoveDiacritics())
                .ToList();
            var people = await _profileService.GetPersonNameMappingsAsync(fullNames);
            var peopleIds = people.Select(p => p.PersonId).ToList();
            var employees = peopleIds.Any()
                ? await _employeeService.GetEmployeesByPeopleIdsAsync(peopleIds, true, uow)
                : Array.Empty<Employee>();
            var employeesByPersonMap = employees.ToDictionary(e => e.PersonId);
            foreach (var externalEmploymentRequest in externalEmploymentRequests)
            {
                if (employmentRequestsMap.TryGetValue(externalEmploymentRequest.Id, out var employmentRequest))
                {
                    var isUpdated = UpdateEmploymentRequestFrom(employmentRequest, externalEmploymentRequest, people, employeesByPersonMap, organizationByImportIdMap, unitImportIdMap);
                    employmentRequest.UpdateDate = isUpdated ? now : employmentRequest.UpdateDate;

                    continue;
                }

                var newEmploymentRequest = new EmploymentRequest
                {
                    ExternalId = Guid.NewGuid().ToString("N"),
                    SourceEmploymentRequest = externalEmploymentRequest,
                    CreationDate = now,
                };

                UpdateEmploymentRequestFrom(newEmploymentRequest, externalEmploymentRequest, people, employeesByPersonMap, organizationByImportIdMap, unitImportIdMap);
                employmentRequestRepository.Add(newEmploymentRequest);

                affectedEmploymentRequestsCount++;
            }

            await uow.SaveChangesAsync();

            return affectedEmploymentRequestsCount;
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

        private static bool UpdateEmploymentRequestFrom(
            EmploymentRequest employmentRequest,
            ExternalEmploymentRequest externalEmploymentRequest,
            IReadOnlyCollection<PersonNameMappingDataContract> people,
            IReadOnlyDictionary<string, Employee> employeesByPersonMap,
            IReadOnlyDictionary<string, OrganizationDataContract> organizationByImportIdMap,
            IReadOnlyDictionary<string, string> unitsMap)
        {
            var employeeOrganization = organizationByImportIdMap[externalEmploymentRequest.OrganizationId];
            var countryId = employeeOrganization.CountryId;
            var employee = employmentRequest.Employee ?? GetEmploymentRequestEmployee(employmentRequest, people, employeesByPersonMap);
            var unitId = unitsMap[externalEmploymentRequest.UnitId];

            var isUpdated = Reflector.SetProperty(() => employmentRequest.FirstName, externalEmploymentRequest.FirstName) |
                            Reflector.SetProperty(() => employmentRequest.LastName, externalEmploymentRequest.LastName) |
                            Reflector.SetProperty(() => employmentRequest.UnitId, unitId) |
                            Reflector.SetProperty(() => employmentRequest.Location, externalEmploymentRequest.Location) |
                            Reflector.SetProperty(() => employmentRequest.CountryId, countryId) |
                            Reflector.SetProperty(() => employmentRequest.OrganizationId, employeeOrganization.Id) |
                            Reflector.SetProperty(() => employmentRequest.EmploymentDate, externalEmploymentRequest.EmploymentDate) |
                            Reflector.SetProperty(() => employmentRequest.Employee, employee);

            return isUpdated;
        }

        private static Employee GetEmploymentRequestEmployee(
            EmploymentRequest employmentRequest,
            IReadOnlyCollection<PersonNameMappingDataContract> people,
            IReadOnlyDictionary<string, Employee> employeesByPersonMap)
        {
            var fullName = $"{employmentRequest.LastName} {employmentRequest.FirstName}";
            var normalizedFullName = fullName.RemoveDiacritics();
            var personIds = people.Where(p => String.Equals(p.PersonFullName, normalizedFullName, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.PersonId)
                .ToList();
            foreach (var personId in personIds)
            {
                if (employeesByPersonMap.TryGetValue(personId, out var employee) && employee.UnitId == employmentRequest.UnitId)
                {
                    return employee;
                }
            }

            return null;
        }

        private static bool UpdateFrom(ExternalEmploymentRequest employmentRequest, SmgEmploymentRequestDataContract smgEmploymentRequest)
        {
            var isUpdated = Reflector.SetProperty(() => employmentRequest.SourceId, smgEmploymentRequest.Id) |
                            Reflector.SetProperty(() => employmentRequest.Type, smgEmploymentRequest.Type) |
                            Reflector.SetProperty(() => employmentRequest.StatusId, smgEmploymentRequest.StatusId) |
                            Reflector.SetProperty(() => employmentRequest.StatusName, smgEmploymentRequest.StatusName) |
                            Reflector.SetProperty(() => employmentRequest.FirstName, smgEmploymentRequest.EmployeeItRequestFieldset.FirstName) |
                            Reflector.SetProperty(() => employmentRequest.LastName, smgEmploymentRequest.EmployeeItRequestFieldset.LastName) |
                            Reflector.SetProperty(() => employmentRequest.UnitId, smgEmploymentRequest.EmployeeItRequestFieldset.UnitId) |
                            Reflector.SetProperty(() => employmentRequest.LocationId, smgEmploymentRequest.EmployeeItRequestFieldset.LocationId) |
                            Reflector.SetProperty(() => employmentRequest.Location, smgEmploymentRequest.EmployeeItRequestFieldset.Location) |
                            Reflector.SetProperty(() => employmentRequest.OrganizationId, smgEmploymentRequest.EmployeeItRequestFieldset.OrganizationId) |
                            Reflector.SetProperty(() => employmentRequest.EmploymentDate, smgEmploymentRequest.EmployeeItRequestFieldset.EmploymentDate) |
                            Reflector.SetProperty(() => employmentRequest.CloseDate, null);

            return isUpdated;
        }


        private static IEntityLoadStrategy<EmploymentRequest> GetEmploymentRequestLoadStrategy()
        {
            return new EntityLoadStrategy<EmploymentRequest>(r => r.Employee);
        }
    }
}
