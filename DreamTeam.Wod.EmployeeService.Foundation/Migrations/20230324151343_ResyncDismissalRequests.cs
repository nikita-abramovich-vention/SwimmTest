using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Logging;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests;
using DreamTeam.Wod.EmployeeService.Repositories;
using DreamTeam.Wod.SmgService.DataContracts;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class ResyncDismissalRequests : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IDismissalRequestSyncService _dismissalRequestSyncService;


        public string Name => "ResyncDismissalRequests";

        public DateTime CreationDate => new(2023, 03, 24, 15, 13, 43);


        public ResyncDismissalRequests(
            IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory,
            IDismissalRequestSyncService dismissalRequestSyncService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _dismissalRequestSyncService = dismissalRequestSyncService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var syncLogRepository = uow.SyncLogs;

                var lastDownloadDismissalRequestsSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalDismissalRequestData);
                if (lastDownloadDismissalRequestsSyncLog != null)
                {
                    lastDownloadDismissalRequestsSyncLog.IsOutdated = true;
                }

                var lastLinkDismissalRequestsSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkDismissalRequests);
                if (lastLinkDismissalRequestsSyncLog != null)
                {
                    lastLinkDismissalRequestsSyncLog.IsOutdated = true;
                }

                await uow.SaveChangesAsync();
            }

            var isSyncedSyccessfully = await _dismissalRequestSyncService.SyncAsync();
            if (!isSyncedSyccessfully)
            {
                LoggerContext.Current.LogError("Failed to sync Dismissal Requests");

                return;
            }

            using (var uow = _unitOfWorkFactory.Create())
            {
                var dismissalRequestRepository = uow.DismissalRequests;

                var dismissalRequestsToRemove = await dismissalRequestRepository.GetWhereAsync(
                    r => r.Type == DismissalRequestType.Ordinary && r.SourceDismissalRequest.DismissalSpecificId != SmgDismissalRequestTypes.Ordinary);

                dismissalRequestRepository.DeleteAll(dismissalRequestsToRemove);

                await uow.SaveChangesAsync();
            }
        }
    }
}
