using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Observable;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    public interface IRelocationPlanService
    {
        event AsyncObserver<RelocationPlanChangedEventArgs> RelocationPlanClosed;

        event AsyncObserver<RelocationPlanChangedEventArgs> RelocationPlanUpdated;


        void Initialize();

        Task<IReadOnlyCollection<string>> GetAllExistingRelocationPlanIds(IReadOnlyCollection<string> idsToCheck = null);

        Task<IReadOnlyCollection<RelocationPlan>> GetAllAsync(DateTime? changeDate = null, bool activeOnly = true, IEmployeeServiceUnitOfWork uow = null);

        Task<IReadOnlyCollection<RelocationPlan>> GetAllByEmployeeIdAsync(string employeeId);

        Task<RelocationPlan> GetByExternalIdAsync(string id);

        Task<RelocationPlan> GetByEmployeeIdAsync(int employeeId);

        Task<IReadOnlyCollection<RelocationPlan>> GetByEmployeeIdsAsync(IReadOnlyCollection<int> employeeIds);

        Task<IReadOnlyCollection<RelocationPlan>> GetConfirmedAsync(DateTime from, DateTime to);

        Task<IReadOnlyCollection<RelocationPlan>> GetPendingInductionAsync(DateTime from, DateTime to);

        Task<IReadOnlyCollection<RelocationPlan>> GetPendingConfirmationAsync(DateTime confirmationDueDate);

        Task<RelocationPlan> GetByExternalEmployeeIdAsync(string employeeId);

        Task<IReadOnlyCollection<RelocationPlan>> GetByExternalEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds, bool includeInactive = false);

        Task<int> GetPendingApprovalRelocationPlanCountByApproverIdAsync(int approverId);

        Task<IReadOnlyCollection<RelocationPlan>> GetByStatusIdAsync(string statusId);

        Task<IReadOnlyCollection<RelocationPlan>> GetSlimAndApprovedByHrManagerDatePeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate);

        Task<IReadOnlyCollection<CurrentLocation>> GetActiveRelocationLocationsAsync();

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> CreateAsync(RelocationPlan relocationPlan, string byPersonId, bool isSync = false);

        Task UpdateAsync(RelocationPlan relocationPlan, RelocationPlan fromRelocationPlan, string byPersonId);

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> UpdateGlobalMobilityInfoAsync(RelocationPlan relocationPlan, RelocationPlanGlobalMobilityInfo fromGlobalMobilityInfo, string byPersonId);

        Task SyncRelocationPlanAsync(RelocationPlan relocationPlan, RelocationCaseProgress caseProgress, CompensationInfo compensation, string relocationPlanSourceId, string caseStatusSourceId, bool isStatusHistoryRequired);

        Task UpdateApproverInfoAsync(RelocationPlan relocationPlan, RelocationPlanApproverInfo fromApproverInfo, string byPersonId);

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> UpdateHrManagerInfoAsync(RelocationPlan relocationPlan, RelocationPlanHrManagerInfo fromHrManagerInfo, string byPersonId);

        Task<OperationResult<IReadOnlyCollection<RelocationPlan>>> UpdateGmStatusesAsync(IReadOnlyCollection<RelocationPlan> relocationPlans, IReadOnlyCollection<RelocationPlanUpdate> fromRelocationPlanUpdates, string byPersonId);

        Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsConfirmed)> ConfirmRelocationPlanAsync(RelocationPlan relocationPlan, string byPersonId);

        Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsApproved)> ApproveRelocationPlanAsync(RelocationPlan relocationPlan, string byPersonId);

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetApproverAsync(RelocationPlan relocationPlan, Employee employee, string byPersonId);

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetHrManagerAsync(RelocationPlan relocationPlan, Employee employee, string byPersonId);

        Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetGmManagerAsync(RelocationPlan relocationPlan, Employee employee, string byPersonId);

        Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsEmploymentConfirmed)> ConfirmRelocationPlanEmploymentByEmployeeAsync(RelocationPlan relocationPlan, string byPersonId);

        Task<bool> CloseAsync(
            RelocationPlan relocationPlan,
            string byPersonId,
            RelocationPlanCloseReason reason = RelocationPlanCloseReason.Completed,
            string closeComment = null,
            DateTime? closeDate = null,
            IEmployeeServiceUnitOfWork uow = null);

        Task HandleEmployeeUpdateAsync(Employee previousEmployee, Employee newEmployee, IEmployeeServiceUnitOfWork uow);

        Task HandleEmployeeUpdatedAsync(EmployeeDataContract previousEmployee, EmployeeDataContract employee);

        Task AddRelocationPlanUpdateHistoryAsync(RelocationPlan previousRelocationPlan, RelocationPlan newRelocationPlan, string byPersonId);

        Task<IReadOnlyCollection<RelocationPlanChange>> GetEmployeeRelocationPlanChanges(string employeeId);

        Task<IReadOnlyCollection<CountryRelocationStep>> GetCountryStepsAsync(string countryId, IEmployeeServiceUnitOfWork uow = null);

        Task SyncCountryRelocationStepsAsync(IReadOnlyCollection<CountryRelocationStep> newSteps);
    }
}
