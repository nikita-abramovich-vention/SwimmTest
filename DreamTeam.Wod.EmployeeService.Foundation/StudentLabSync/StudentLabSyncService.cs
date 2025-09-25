using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Common.Threading;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.Internships;
using DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync.Models;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.StudentLabSync
{
    [UsedImplicitly]
    public sealed class StudentLabSyncService : IStudentLabSyncService
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _uowFactory;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IStudentLabService _studentLabService;
        private readonly IInternshipService _internshipService;
        private readonly IStudentLabSyncServiceConfiguration _configuration;

        private readonly Timer _syncTimer;
        private readonly IAsyncResourceLocker _syncLocker;


        public StudentLabSyncService(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> uowFactory,
            IEnvironmentInfoService environmentInfoService,
            IStudentLabService studentLabService,
            IInternshipService internshipService,
            IStudentLabSyncServiceConfiguration configuration)
        {
            _uowFactory = uowFactory;
            _environmentInfoService = environmentInfoService;
            _studentLabService = studentLabService;
            _internshipService = internshipService;
            _configuration = configuration;

            _syncTimer = new Timer(Math.Min(TimeSpan.FromMinutes(5).TotalMilliseconds, _configuration.SyncInterval.TotalMilliseconds / 5));
            _syncTimer.Elapsed += SyncTimerOnElapsed;
            _syncLocker = new AsyncResourceLocker();
        }


        public Task ActivateRegularSyncAsync()
        {
            _syncTimer.Start();
            Task.Run(() => SyncAsync()).Forget();

            return Task.CompletedTask;
        }

        public async Task SyncAsync()
        {
            if (!_configuration.Enable)
            {
                return;
            }

            var isSyncRequired = await CheckIfSyncRequiredAsync();
            if (!isSyncRequired)
            {
                return;
            }

            using (_syncLocker.TryGetAccess(out var isAccessed))
            {
                if (!isAccessed)
                {
                    LoggerContext.Current.LogWarning("Failed to perform lock for sync.");
                    return;
                }

                await SyncStudentLabInternshipsAsync();
            }
        }


        private async Task SyncStudentLabInternshipsAsync()
        {
            using (var uow = _uowFactory.Create())
            {
                var syncLogRepository = uow.StudentLabSyncLogs;
                var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync();
                var previousSyncStartDate = lastSyncLog != null && !lastSyncLog.IsOutdated
                    ? lastSyncLog.SyncStartDate
                    : (DateTime?)null;

                var syncStartDate = _environmentInfoService.CurrentUtcDateTime;
                var syncLog = new StudentLabSyncLog
                {
                    SyncStartDate = syncStartDate,
                };

                LoggerContext.Current.Log("Starting Student Lab internships sync...");
                try
                {
                    syncLog.AffectedInternshipsCount = await DownloadAndLinkStudentLabInternshipsAsync(previousSyncStartDate, uow);
                    syncLog.IsSuccessful = true;
                    LoggerContext.Current.Log("Student Lab sync completed successfully. Affected internships - {affectedInternshipsCount}.", syncLog.AffectedInternshipsCount);
                }
                catch (Exception ex) when (ex.IsCatchableExceptionType())
                {
                    LoggerContext.Current.LogError("Failed to sync Student Lab internships.", ex);
                }
                finally
                {
                    syncLog.SyncCompletedDate = _environmentInfoService.CurrentUtcDateTime;
                    syncLogRepository.Add(syncLog);
                    await uow.SaveChangesAsync();
                }
            }
        }

        private async Task<int> DownloadAndLinkStudentLabInternshipsAsync(DateTime? previousSyncStartDate, IEmployeeServiceUnitOfWork uow)
        {
            var internships = await _studentLabService.GetInternshipsAsync(previousSyncStartDate);
            if (internships == null || internships.Count == 0)
            {
                LoggerContext.Current.Log("No Student Lab internships to sync.");
                return 0;
            }

            var internalInternships = await _internshipService.GetInternshipsAsync(true, uow);
            var internalInternshipsMap = internalInternships.ToDictionary(i => i.ExternalId);
            var affectedInternshipsCount = 0;

            foreach (var internship in internships)
            {
                if (internalInternshipsMap.TryGetValue(internship.WodInternshipId, out var internalInternship))
                {
                    var updatedInternship = UpdateFrom(internship, internalInternship);
                    var updateResult = await _internshipService.UpdateInternshipAsync(internalInternship, updatedInternship, uow);
                    if (!updateResult.IsSuccessful)
                    {
                        var errors = updateResult.Errors.Select(e => e.ToString()).JoinStrings();
                        LoggerContext.Current.LogWarning("Failed to update wod internship because {errors}", errors);
                        continue;
                    }

                    affectedInternshipsCount++;
                }
                else
                {
                    LoggerContext.Current.LogWarning("Failed to get corresponding wod internship.");
                }
            }

            return affectedInternshipsCount;
        }

        private async void SyncTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            await SyncAsync();
        }

        private async Task<bool> CheckIfSyncRequiredAsync()
        {
            using (var uow = _uowFactory.Create())
            {
                var syncLogRepository = uow.StudentLabSyncLogs;
                var lastSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync();

                return lastSyncLog == null || lastSyncLog.IsOutdated || _environmentInfoService.CurrentUtcDateTime - lastSyncLog.SyncStartDate > _configuration.SyncInterval;
            }
        }

        private Internship UpdateFrom(StudentLabInternship studentLabInternship, Internship internship)
        {
            var updatedInternship = internship.Clone();
            if (studentLabInternship.HasStLabActivities)
            {
                updatedInternship.StudentLabId = studentLabInternship.Id;
                updatedInternship.StudentLabProfileUrl = studentLabInternship.ProfileUrl;
            }
            else
            {
                updatedInternship.StudentLabId = null;
                updatedInternship.StudentLabProfileUrl = null;
            }

            return updatedInternship;
        }
    }
}
