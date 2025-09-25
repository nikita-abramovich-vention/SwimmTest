using DreamTeam.Repositories;
using DreamTeam.Wod.EmployeeService.Repositories.Repositories;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    public interface IEmployeeServiceUnitOfWork : IUnitOfWork
    {
        IInternshipRepository Internships { get; }

        IStudentLabSyncLogRepository StudentLabSyncLogs { get; }

        ISyncLogRepository SyncLogs { get; }

        IRelocationPlanRepository RelocationPlans { get; }

        IEmployeeSnapshotRepository EmployeeSnapshots { get; }

        IEmployeeSnapshotLogRepository EmployeeSnapshotLogs { get; }

        IEmploymentPeriodRepository EmploymentPeriods { get; }

        IEmploymentRequestRepository EmploymentRequests { get; }

        IDismissalRequestRepository DismissalRequests { get; }

        IExternalEmployeeUnitHistoryRepository ExternalEmployeeUnitHistory { get; }

        IWageRatePeriodRepository WageRatePeriods { get; }
    }
}