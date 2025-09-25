using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.DomainModel;
using DreamTeam.Microservices.DataContracts;
using DreamTeam.Microservices.Interfaces;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.Roles;

namespace DreamTeam.Wod.EmployeeService.Foundation
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface IEmployeeMicroservice : IMicroservice
    {
        Task<OperationResultDataContract<EmployeeDataContract>> GetEmployeeById(string id);

        Task<OperationResultDataContract<EmployeeDataContract>> GetEmployeeByPersonId(string personId);

        Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeManagers(string id);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesWithRole(string roleId);

        Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeImmediateManagers(string id);

        Task<IReadOnlyDictionary<string, IReadOnlyCollection<EmployeeDataContract>>> GetEmployeeImmediateManagersMap(IReadOnlyCollection<string> ids, bool shouldIncludeInactiveManagers = false);

        Task<OperationResultDataContract<IReadOnlyCollection<EmployeeDataContract>>> GetInternshipImmediateManagers(string id);

        Task<IReadOnlyDictionary<string, IReadOnlyCollection<EmployeeDataContract>>> GetInternshipImmediateManagersMap(IReadOnlyCollection<string> ids, bool shouldIncludeInactiveManagers = false);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployees(bool shouldIncludeInactive);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesWithoutInterns(bool shouldIncludeInactive);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByIds(IReadOnlyCollection<string> ids, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByPeopleIds(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByUnitId(string unitId, bool shouldIncludeSubUnits, bool shouldIncludeInactive);

        Task<IReadOnlyCollection<EmployeeDataContract>> GetEmployeesByUnitIds(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive);

        Task<IReadOnlyCollection<string>> GetAllExistingRelocationPlanIds(IReadOnlyCollection<string> idsToCheck = null);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetAllRelocationPlans(DateTime? changeDate = null);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetActiveRelocationPlans();

        Task<OperationResultDataContract<EmployeeDataContract>> UpdateEmployee(string id, EmployeeDataContract employee);

        Task<OperationResultDataContract<InternshipDataContract>> GetInternshipById(string id);

        Task<InternshipDataContract> GetLastInternshipByDomainName(string domainName);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternships(bool shouldIncludeInactive);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternships(IReadOnlyCollection<string> ids, bool shouldIncludeInactive = false);

        Task<PaginatedItemsDataContract<InternshipDataContract>> GetInternshipsPaginated(
            IReadOnlyCollection<string> ids,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByUnitId(string unitId, bool shouldIncludeSubUnits = false, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByUnitIds(IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByPersonId(string personId, bool shouldIncludeInactive = false);

        Task<PaginatedItemsDataContract<InternshipDataContract>> GetInternshipsByPersonIdPaginated(
            string personId,
            DateOnly fromDate,
            DateOnly toDate,
            PaginationDirection direction,
            bool shouldIncludeInactive = false);

        Task<OperationResultDataContract<InternshipDataContract>> GetLastInternshipByEmployeeId(string employeeId);

        Task<IReadOnlyCollection<InternshipDataContract>> GetInternshipsByPeopleIds(IReadOnlyCollection<string> peopleIds, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<UnitInternshipsCountDataContract>> GetUnitInternshipsCounts();

        Task<bool> CheckIfDomainNameIsTaken(string domainName);

        Task<bool> VerifyDomainName(string domainName);

        Task<OperationResultDataContract<InternshipDataContract>> CreateInternship(InternshipDataContract internship);

        Task<OperationResultDataContract<InternshipDataContract>> UpdateInternship(string id, InternshipDataContract internship);

        Task<OperationResultDataContract<InternshipDataContract>> OpenInternship(string id);

        Task<OperationResultDataContract<InternshipDataContract>> CloseInternship(string id);

        Task DeleteInternship(string id);

        Task<IReadOnlyCollection<SeniorityDataContract>> GetAllSeniority();

        Task<IReadOnlyCollection<RoleDataContract>> GetRoles();

        Task<OperationResultDataContract<RoleWithConfigurationDataContract>> CreateCustomRole(RoleWithConfigurationDataContract customRoleWithConfigurationDataContract);

        Task<OperationResultDataContract> DeleteCustomRole(string id);

        Task<IReadOnlyCollection<CurrentLocationDataContract>> GetCurrentLocations(bool shouldIncludeCustom = false);

        Task<CurrentLocationDataContract> CreateCurrentLocation(string name, string countryId = null);

        Task<OperationResultDataContract<EmployeeLocationInfoDataContract>> GetEmployeeLocationInfo(string employeeId);

        Task<IReadOnlyCollection<EmployeeLocationInfoDataContract>> GetEmployeeLocationInfos(IReadOnlyCollection<string> employeeIds);

        Task<IReadOnlyCollection<RelocationPlanHistoryDataContract>> GetEmployeeRelocationPlanHistory(string employeeId);

        Task<OperationResultDataContract<RelocationPlanDataContract>> GetRelocationPlanByEmployeeId(string employeeId);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetRelocationPlansByEmployeeIds(IReadOnlyCollection<string> employeeIds, bool includeInactive = false);

        Task<IReadOnlyCollection<CurrentLocationDataContract>> GetActiveRelocationLocations();

        Task<OperationResultDataContract> UpdateRelocationPlanPartially(IReadOnlyCollection<RelocationPlanUpdateDataContract> relocationPlanUpdates);

        Task<IReadOnlyCollection<TitleRoleDataContract>> GetTitleRoles();

        Task<IReadOnlyCollection<RoleWithConfigurationDataContract>> GetRolesWithConfiguration(RoleType? type = null);

        Task<OperationResultDataContract<RoleWithConfigurationDataContract>> GetRoleWithConfigurationById(string id);

        Task<OperationResultDataContract<RoleWithConfigurationDataContract>> UpdateRoleWithConfiguration(RoleWithConfigurationDataContract roleWithConfiguration);

        Task<OperationResultDataContract<RelocationPlanDataContract>> GetRelocationPlanById(string id);

        Task<OperationResultDataContract<int>> GetPendingApprovalRelocationPlanCountByEmployeeId(string employeeId);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetRelocationPlansByStatusId(string statusId);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetConfirmedRelocationPlans(DateTime from, DateTime to);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetPendingInductionRelocationPlans(DateTime from, DateTime to);

        Task<IReadOnlyCollection<RelocationPlanDataContract>> GetPendingConfirmationRelocationPlans(DateTime confirmationDueDate);

        Task<OperationResultDataContract<RelocationPlanDataContract>> CreateRelocationPlan(string employeeId, RelocationPlanDataContract relocationPlan, bool isSync = false);

        Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlan(string employeeId, string id, RelocationPlanDataContract relocationPlan);

        Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanGlobalMobilityInfo(string employeeId, string id, RelocationPlanGlobalMobilityInfoDataContract relocationPlanGlobalMobilityInfo);

        Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanGmManager(string id, string employeeId);

        Task<OperationResultDataContract> SyncRelocationPlan(string id, RelocationPlanDataContract relocationPlanDataContract, RelocationPlanSyncInfoDataContract syncInfo);

        Task<OperationResultDataContract<RelocationPlanDataContract>> ConfirmRelocationPlan(string employeeId, string id);

        Task<OperationResultDataContract<RelocationPlanDataContract>> ApproveRelocationPlan(string id);

        Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanApproverInfo(string id, RelocationPlanApproverInfoDataContract relocationPlanApproverInfo);

        Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanApprover(string id, string employeeId);

        Task<OperationResultDataContract<RelocationPlanDataContract>> UpdateRelocationPlanHrManagerInfo(string id, RelocationPlanHrManagerInfoDataContract relocationPlanHrManagerInfo);

        Task<OperationResultDataContract<RelocationPlanDataContract>> SetRelocationPlanHrManager(string id, string employeeId);

        Task<OperationResultDataContract<RelocationPlanDataContract>> ConfirmRelocationPlanEmploymentByEmployee(string employeeId, string id);

        Task<OperationResultDataContract<RelocationPlanDataContract>> CloseRelocationPlan(string employeeId, string id, RelocationPlanCloseInfoDataContract relocationPlanCloseInfo);

        Task<IReadOnlyCollection<RelocationApproverDataContract>> GetRelocationApprovers(string countryId = null);

        Task<IReadOnlyCollection<RelocationApproverDataContract>> GetRelocationApproversByEmployeeId(string employeeId);

        Task<bool> CheckIsEmployeeRelocationApproverOrHasAssignedRequests(string employeeId);

        Task<RelocationApproverDataContract> GetPrimaryRelocationApprover(string countryId);

        Task<IReadOnlyCollection<RelocationApproverDataContract>> GetPrimaryRelocationApproversByCountryIds(IReadOnlyCollection<string> countryIds);

        Task<IReadOnlyCollection<RelocationApproverDataContract>> GetPrimaryRelocationApprovers();

        Task<OperationResultDataContract<IReadOnlyCollection<RelocationApproverDataContract>>> UpdateRelocationApprovers(
            string countryId,
            IReadOnlyCollection<RelocationApproverDataContract> approverDataContracts);

        Task<RelocationApproverAssignmentsProfileDataContract> GetRelocationApproversAssignmentsProfile(string countryId);

        Task<OperationResultDataContract<EmployeeCurrentLocationDataContract>> GetEmployeeCurrentLocation(string employeeId);

        Task<OperationResultDataContract<EmployeeCurrentLocationDataContract>> UpdateEmployeeCurrentLocation(string employeeId, EmployeeCurrentLocationDataContract currentLocation);

        Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetEmployeeSnapshotsPerDay(DateOnly fromDate, DateOnly toDate, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetEmployeeSnapshotsPerDayByUnitIds(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> unitIds, bool shouldIncludeInactive = false);

        Task<IReadOnlyCollection<EmployeeSnapshotDataContract>> GetAllEmployeeSnapshots();

        Task<IReadOnlyCollection<EmploymentRequestDataContract>> GetEmploymentRequests(bool includeWithEmployees = false, bool includeInternshipEmploymentRequests = false);

        Task<IReadOnlyCollection<EmploymentRequestDataContract>> GetEmploymentRequestsByIds(IReadOnlyCollection<string> ids, bool includeInternshipEmploymentRequests = false);

        Task<IReadOnlyCollection<EmployeeUnitHistoryDataContract>> GetAllEmployeeUnitHistory();

        Task<IReadOnlyCollection<EmployeeUnitHistoryDataContract>> GetEmployeeUnitHistoryByEmployeeId(string employeeId);

        Task<OperationResultDataContract<byte[]>> GetWorkplaceSchemeImage(string id);

        Task<EmployeeDynamicRequestsProfileDataContract> GetActiveEmployeeWithoutInternsDynamicRequests(DateOnly fromDate, DateOnly toDate, bool includeRelocations = false);

        Task<IReadOnlyCollection<DismissalRequestDataContract>> GetAllDismissalRequests(bool activeOnly = false);

        Task<IReadOnlyCollection<DismissalRequestDataContract>> GetEmployeeDismissalRequests(string employeeId);

        Task<IReadOnlyCollection<DismissalRequestDataContract>> GetActiveDismissalRequestsByPeriodAndEmployeeIds(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> employeeIds);

        Task<IReadOnlyCollection<DismissalRequestDataContract>> GetActiveDismissalRequestsByEmployeeIds(IReadOnlyCollection<string> employeeIds, IReadOnlyCollection<DismissalRequestType> types = null);

        Task<OperationResultDataContract<DismissalRequestDataContract>> GetDismissalRequestById(string id);

        Task<OperationResultDataContract<DismissalRequestDataContract>> CreateDismissalRequest(DismissalRequestDataContract dismissalRequest);

        Task<OperationResultDataContract<DismissalRequestDataContract>> UpdateDismissalRequest(string id, DismissalRequestDataContract dismissalRequest);

        Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetAllEmploymentPeriods();

        Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsByDate(DateOnly date);

        Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsByEmployeeIds(IReadOnlyCollection<string> employeeIds);

        Task<IReadOnlyCollection<EmploymentPeriodDataContract>> GetEmploymentPeriodsForPeriod(DateOnly startDate, DateOnly endDate, IReadOnlyCollection<string> employeeIds = null);

        Task<PaginatedItemsDataContract<EmploymentPeriodDataContract>> GetEmploymentPeriodsByEmployeeIdPaginated(string employeeId, DateOnly fromDate, DateOnly toDate, PaginationDirection direction);

        Task<OperationResultDataContract> RecalculateEmployeeSnapshots(IReadOnlyCollection<string> employeeIds, DateOnly fromDate);

        Task<OperationResultDataContract> SyncCountryRelocationSteps(IReadOnlyCollection<CountryRelocationStepsDataContract> steps);

        Task<IReadOnlyCollection<double>> GetAllWageRates();
    }
}