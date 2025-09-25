using DreamTeam.Common;
using DreamTeam.Repositories.EntityFramework.Implementations;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories.Repositories;

namespace DreamTeam.Wod.EmployeeService.Repositories
{
    [UsedImplicitly]
    public sealed class EmployeeServiceUnitOfWork : UnitOfWork<EmployeeServiceDbContext>, IEmployeeServiceUnitOfWork
    {
        public IInternshipRepository Internships => (IInternshipRepository)GetRepository<Internship>();

        public IStudentLabSyncLogRepository StudentLabSyncLogs => (IStudentLabSyncLogRepository)GetRepository<StudentLabSyncLog>();

        public ISyncLogRepository SyncLogs => (ISyncLogRepository)GetRepository<SyncLog>();

        public IRelocationPlanRepository RelocationPlans => (IRelocationPlanRepository)GetRepository<RelocationPlan>();

        public IEmployeeSnapshotRepository EmployeeSnapshots => (IEmployeeSnapshotRepository)GetRepository<EmployeeSnapshot>();

        public IEmployeeSnapshotLogRepository EmployeeSnapshotLogs => (IEmployeeSnapshotLogRepository)GetRepository<EmployeeSnapshotLog>();

        public IEmploymentPeriodRepository EmploymentPeriods => (IEmploymentPeriodRepository)GetRepository<EmploymentPeriod>();

        public IEmploymentRequestRepository EmploymentRequests => (IEmploymentRequestRepository)GetRepository<EmploymentRequest>();

        public IDismissalRequestRepository DismissalRequests => (IDismissalRequestRepository)GetRepository<DismissalRequest>();

        public IExternalEmployeeUnitHistoryRepository ExternalEmployeeUnitHistory => (IExternalEmployeeUnitHistoryRepository)GetRepository<ExternalEmployeeUnitHistory>();

        public IWageRatePeriodRepository WageRatePeriods => (IWageRatePeriodRepository)GetRepository<WageRatePeriod>();


        public EmployeeServiceUnitOfWork(EmployeeServiceDbContext dbContext)
            : base(dbContext)
        {
            RegisterRepository<Internship, InternshipRepository>();
            RegisterRepository<StudentLabSyncLog, StudentLabSyncLogRepository>();
            RegisterRepository<SyncLog, SyncLogRepository>();
            RegisterRepository<RelocationPlan, RelocationPlanRepository>();
            RegisterRepository<EmployeeSnapshot, EmployeeSnapshotRepository>();
            RegisterRepository<EmployeeSnapshotLog, EmployeeSnapshotLogRepository>();
            RegisterRepository<EmploymentPeriod, EmploymentPeriodRepository>();
            RegisterRepository<EmploymentRequest, EmploymentRequestRepository>();
            RegisterRepository<DismissalRequest, DismissalRequestRepository>();
            RegisterRepository<ExternalEmployeeUnitHistory, ExternalEmployeeUnitHistoryRepository>();
            RegisterRepository<WageRatePeriod, WageRatePeriodRepository>();
        }
    }
}