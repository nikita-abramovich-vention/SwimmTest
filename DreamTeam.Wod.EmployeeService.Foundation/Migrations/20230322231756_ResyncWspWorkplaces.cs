using System;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Migrations;
using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.WspSync;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Migrations
{
    [UsedImplicitly]
    public sealed class ResyncWspWorkplaces : IMigration
    {
        private readonly IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> _unitOfWorkFactory;
        private readonly IWspSyncService _wspSyncService;


        public string Name => "ResyncWspWorkplaces";

        public DateTime CreationDate => new DateTime(2023, 03, 22, 23, 17, 56);


        public ResyncWspWorkplaces(IUnitOfWorkFactory<IEmployeeServiceUnitOfWork> unitOfWorkFactory, IWspSyncService wspSyncService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _wspSyncService = wspSyncService;
        }


        public async Task UpAsync()
        {
            using (var uow = _unitOfWorkFactory.Create())
            {
                var syncLogRepository = uow.SyncLogs;
                var lastDownloadExternalWspDataSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.DownloadExternalWspData);
                if (lastDownloadExternalWspDataSyncLog != null)
                {
                    lastDownloadExternalWspDataSyncLog.IsOutdated = true;
                }

                var lastLinkEmployeeWorkplacesSyncLog = await syncLogRepository.GetLastSuccessfulSyncLogAsync(SyncType.LinkEmployeeWorkplaces);
                if (lastLinkEmployeeWorkplacesSyncLog != null)
                {
                    lastLinkEmployeeWorkplacesSyncLog.IsOutdated = true;
                }

                await uow.SaveChangesAsync();
            }

            var isWspSyncSuccessful = await _wspSyncService.SyncAsync();
            if (!isWspSyncSuccessful)
            {
                throw new InvalidOperationException("Failed to perform wsp sync.");
            }
        }
    }
}
