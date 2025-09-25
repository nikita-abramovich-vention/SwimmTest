using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DreamTeam.BackgroundJobs;
using DreamTeam.Common;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Reflection;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers;
using DreamTeam.Wod.EmployeeService.Repositories;
using RelocationPlanChangeType = DreamTeam.Wod.EmployeeService.DomainModel.RelocationPlanChangeType;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    [UsedImplicitly]
    public sealed class RelocationPlanService : IRelocationPlanService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IDepartmentService _departmentService;
        private readonly IRelocationApproverService _relocationApproverService;
        private readonly IJobScheduler _jobScheduler;
        private readonly IEmployeeServiceConfiguration _employeeServiceConfiguration;


        public event AsyncObserver<RelocationPlanChangedEventArgs> RelocationPlanClosed;

        public event AsyncObserver<RelocationPlanChangedEventArgs> RelocationPlanUpdated;


        public RelocationPlanService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService,
            IDepartmentService departmentService,
            IRelocationApproverService relocationApproverService,
            IJobScheduler jobScheduler,
            IEmployeeServiceConfiguration employeeServiceConfiguration)
        {
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;
            _departmentService = departmentService;
            _relocationApproverService = relocationApproverService;
            _jobScheduler = jobScheduler;
            _employeeServiceConfiguration = employeeServiceConfiguration;

            _relocationApproverService.PrimaryApproversChanged += OnPrimaryApproversChanged;
            _relocationApproverService.ApproversRemoved += OnApproversRemoved;
        }


        public void Initialize()
        {
            _jobScheduler.ScheduleRecurringJob<RelocationPlanService>(
                s => s.CloseUnconfirmedRelocationPlansAsync(),
                _employeeServiceConfiguration.CloseUnconfirmedRelocationPlansCron);
        }

        public async Task CloseUnconfirmedRelocationPlansAsync()
        {
            var currentDate = _environmentInfoService.CurrentUtcDateTime;

            LoggerContext.Current.Log("Closing unconfirmed relocation plans...");

            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var specification = RelocationPlanSpecification.Active &
                                RelocationPlanSpecification.ByStatusId(RelocationPlanStatus.BuiltIn.EmployeeConfirmation) &
                                RelocationPlanSpecification.ByLessOrEqualStatusDueDate(currentDate);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var unconfirmedRelocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            foreach (var unconfirmedRelocationPlan in unconfirmedRelocationPlans)
            {
                await CloseAsync(
                    unconfirmedRelocationPlan,
                    null,
                    RelocationPlanCloseReason.Cancelled,
                    "Relocation was canceled due to employee confirmation overdue",
                    currentDate);

                LoggerContext.Current.Log("Unconfirmed relocation plan {relocationPlanId} was closed.", unconfirmedRelocationPlan.ExternalId);
            }

            if (unconfirmedRelocationPlans.Count == 0)
            {
                LoggerContext.Current.Log("No unconfirmed relocation plans were closed.");
            }
            else
            {
                LoggerContext.Current.Log($"Unconfirmed relocation plans were closed. {unconfirmedRelocationPlans.Count} relocation plans were affected.");
            }
        }

        public async Task<IReadOnlyCollection<string>> GetAllExistingRelocationPlanIds(IReadOnlyCollection<string> idsToCheck = null)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var specification = idsToCheck == null
                ? RelocationPlanSpecification.Any
                : RelocationPlanSpecification.ByExternalIds(idsToCheck);
            var relocationPlans = await repository.GetWhereAsync(specification);
            var relocationPlanIds = relocationPlans.Select(p => p.ExternalId).ToList();

            return relocationPlanIds;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetAllAsync(DateTime? changeDate = null, bool activeOnly = true, IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var repository = currentUow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.WithActiveEmployee;
            if (activeOnly)
            {
                specification &= RelocationPlanSpecification.Active;
            }

            if (changeDate.HasValue)
            {
                specification &= RelocationPlanSpecification.ByChangeDate(changeDate.Value);
            }

            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetAllByEmployeeIdAsync(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.ByEmployeeId(employeeId);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<RelocationPlan> GetByExternalIdAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.ByExternalId(id);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlan = await repository.GetSingleOrDefaultAsync(specification, loadStrategy);

            return relocationPlan;
        }

        public async Task<RelocationPlan> GetByEmployeeIdAsync(int employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.ByEmployeeId(employeeId);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlan = await repository.GetSingleOrDefaultAsync(specification, loadStrategy);

            return relocationPlan;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetByEmployeeIdsAsync(IReadOnlyCollection<int> employeeIds)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.ByEmployeeIds(employeeIds);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetConfirmedAsync(DateTime from, DateTime to)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.Confirmed(from, to);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetPendingInductionAsync(DateTime from, DateTime to)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.PendingInduction(from, to);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetPendingConfirmationAsync(DateTime confirmationDueDate)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var specification = RelocationPlanSpecification.Active &
                                RelocationPlanSpecification.ByStatusId(RelocationPlanStatus.BuiltIn.EmployeeConfirmation) &
                                RelocationPlanSpecification.ByEqualStatusDueDate(confirmationDueDate);

            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<RelocationPlan> GetByExternalEmployeeIdAsync(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.ByEmployeeId(employeeId);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlan = await repository.GetSingleOrDefaultAsync(specification, loadStrategy);

            return relocationPlan;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetByExternalEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds, bool includeInactive = false)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var specification = RelocationPlanSpecification.ByEmployeeIds(employeeIds);
            if (!includeInactive)
            {
                specification &= RelocationPlanSpecification.Active;
            }

            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<int> GetPendingApprovalRelocationPlanCountByApproverIdAsync(int approverId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var pendingApprovalCount = await repository.CountAsync(p => p.State == RelocationPlanState.Active &&
                                                                        !p.IsApproved &&
                                                                        p.ApproverId == approverId &&
                                                                        p.Employee.IsActive);

            return pendingApprovalCount;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetByStatusIdAsync(string statusId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var loadStrategy = GetRelocationPlanLoadStrategy();

            var relocationPlans = await repository
                .GetWhereAsync(
                    p => p.State == RelocationPlanState.Active && p.Status.ExternalId == statusId,
                    loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<RelocationPlan>> GetSlimAndApprovedByHrManagerDatePeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();

            var loadStrategy = GetSlimRelocationPlanLoadStrategy();
            var specification = RelocationPlanSpecification.Active &
                                RelocationPlanSpecification.Approved &
                                RelocationPlanSpecification.WithRelocationUnit &
                                RelocationPlanSpecification.ByHrManagerDate(fromDate, toDate) &
                                RelocationPlanSpecification.EmployeeNotInternship;
            var relocationPlans = await repository.GetWhereAsync(specification, loadStrategy);

            return relocationPlans;
        }

        public async Task<IReadOnlyCollection<CurrentLocation>> GetActiveRelocationLocationsAsync()
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.RelocationPlans;
            var locations = await repository.GetActiveRelocationLocationsAsync();

            return locations;
        }

        public async Task<IReadOnlyCollection<RelocationPlanChange>> GetEmployeeRelocationPlanChanges(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlanChange>();

            var loadStrategy = GetRelocationPlanChangeLoadStrategy();
            var relocationPlanChanges = await repository.GetWhereAsync(rc => rc.Employee.ExternalId == employeeId, loadStrategy);

            return relocationPlanChanges;
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> CreateAsync(RelocationPlan relocationPlan, string byPersonId, bool isSync)
        {
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            var uow = _uowProvider.CurrentUow;
            var relocationPlanRepository = uow.RelocationPlans;
            var statusRepository = uow.GetRepository<RelocationPlanStatus>();

            var isRelocationPlanAlreadyExists = await relocationPlanRepository.AnyAsync(p => p.EmployeeId == relocationPlan.EmployeeId && p.State == RelocationPlanState.Active);
            if (isRelocationPlanAlreadyExists)
            {
                return RelocationPlanManagementError.RelocationPlanAlreadyExists;
            }

            var defaultStatus = await statusRepository.GetSingleAsync(s => s.ExternalId == RelocationPlanStatus.BuiltIn.Induction);
            relocationPlan.Status = defaultStatus;
            relocationPlan.StatusId = defaultStatus.Id;

            var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId, uow);
            relocationPlan.InitSteps(countrySteps);
            relocationPlan.SetStep(RelocationStepId.Induction, countrySteps, currentDate);
            relocationPlan.StatusStartDate = currentDate;
            relocationPlan.StatusDueDate = relocationPlan.CurrentStep.ExpectedAt;

            relocationPlan.ExternalId = Guid.NewGuid().ToString("N");

            if (!isSync)
            {
                var sameCountryRelocationPlans = await relocationPlanRepository.GetWhereAsync(p => p.EmployeeId == relocationPlan.EmployeeId && p.Location.CountryId == relocationPlan.Location.CountryId);
                var latestSameCountryRelocationPlan = sameCountryRelocationPlans.MaxBy(p => p.UpdateDate);
                var inductionValidUntilDate = currentDate.Date - _employeeServiceConfiguration.InductionValidityPeriod;
                if (latestSameCountryRelocationPlan != null && latestSameCountryRelocationPlan.InductionStatusChangeDate > inductionValidUntilDate)
                {
                    relocationPlan.IsInductionPassed = latestSameCountryRelocationPlan.IsInductionPassed;
                    relocationPlan.InductionStatusChangeDate = latestSameCountryRelocationPlan.InductionStatusChangeDate;
                    relocationPlan.InductionStatusChangedBy = latestSameCountryRelocationPlan.InductionStatusChangedBy;
                    if (relocationPlan.IsInductionPassed)
                    {
                        relocationPlan.SetStep(RelocationStepId.RelocationConfirmation, countrySteps, currentDate);
                        var inductionStep = relocationPlan.GetStep(RelocationStepId.Induction);
                        if (inductionStep != null)
                        {
                            inductionStep.CompletedAt = relocationPlan.InductionStatusChangeDate ?? inductionStep.CompletedAt;
                        }
                    }

                    relocationPlan.IsConfirmed = latestSameCountryRelocationPlan.IsConfirmed;
                    relocationPlan.ConfirmationDate = latestSameCountryRelocationPlan.ConfirmationDate;
                    if (relocationPlan.IsConfirmed)
                    {
                        relocationPlan.SetStep(RelocationStepId.PendingApproval, countrySteps, currentDate);
                        var relocationConfirmationStep = relocationPlan.GetStep(RelocationStepId.RelocationConfirmation);
                        if (relocationConfirmationStep != null)
                        {
                            relocationConfirmationStep.CompletedAt = relocationPlan.ConfirmationDate ?? relocationConfirmationStep.CompletedAt;
                        }
                    }

                    relocationPlan.StatusStartDate = currentDate;
                    relocationPlan.StatusDueDate = relocationPlan.CurrentStep.ExpectedAt;

                    await UpdateRelocationPlanStatusAsync(relocationPlan, uow);
                }
                relocationPlan.IsApproved = false;
                relocationPlan.ApprovalDate = null;
                relocationPlan.IsEmploymentConfirmedByEmployee = false;

                await PickApproverAsync(relocationPlan, uow);
            }
            else if (relocationPlan.IsApproved)
            {
                var latestSameCountryApprovedRelocationPlan = await relocationPlanRepository.GetLatestSameCountryApprovedRelocationPlanAsync(relocationPlan.EmployeeId, relocationPlan.Location.CountryId);
                relocationPlan.ApprovalDate = latestSameCountryApprovedRelocationPlan != null
                    ? latestSameCountryApprovedRelocationPlan.ApprovalDate
                    : relocationPlan.ApprovalDate;
            }

            relocationPlan.CreationDate = currentDate;
            relocationPlan.CreatedBy = byPersonId;
            if (!String.IsNullOrEmpty(relocationPlan.EmployeeComment))
            {
                relocationPlan.EmployeeCommentChangeDate = currentDate;
            }

            relocationPlanRepository.Add(relocationPlan);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task UpdateAsync(RelocationPlan relocationPlan, RelocationPlan fromRelocationPlan, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            if (relocationPlan.EmployeeComment != fromRelocationPlan.EmployeeComment)
            {
                relocationPlan.EmployeeCommentChangeDate = currentDate;
            }

            relocationPlan.EmployeeDate = fromRelocationPlan.EmployeeDate;
            relocationPlan.EmployeeComment = fromRelocationPlan.EmployeeComment;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> UpdateGlobalMobilityInfoAsync(RelocationPlan relocationPlan, RelocationPlanGlobalMobilityInfo fromGlobalMobilityInfo, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            if (relocationPlan.GmComment != fromGlobalMobilityInfo.GmComment)
            {
                relocationPlan.GmCommentChangeDate = currentDate;
            }

            relocationPlan.GmComment = fromGlobalMobilityInfo.GmComment;
            if (relocationPlan.IsInductionPassed != fromGlobalMobilityInfo.IsInductionPassed)
            {
                if (relocationPlan.IsConfirmed)
                {
                    return RelocationPlanManagementError.CanNotChangeRelocationPlanInductionPassedAfterConfirmation;
                }

                relocationPlan.IsInductionPassed = fromGlobalMobilityInfo.IsInductionPassed;
                relocationPlan.InductionStatusChangeDate = currentDate;
                relocationPlan.InductionStatusChangedBy = byPersonId;

                if (relocationPlan.IsInductionPassed)
                {
                    var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId, uow);
                    relocationPlan.SetStep(RelocationStepId.RelocationConfirmation, countrySteps, currentDate);
                }
            }

            if (fromGlobalMobilityInfo.GmManager != null &&
                fromGlobalMobilityInfo.GmManager.ExternalId != relocationPlan.GmManager?.ExternalId)
            {
                relocationPlan.GmManager = fromGlobalMobilityInfo.GmManager;
            }

            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task SyncRelocationPlanAsync(RelocationPlan relocationPlan, RelocationCaseProgress caseProgress, CompensationInfo compensation, string relocationPlanSourceId, string caseStatusSourceId, bool isStatusHistoryRequired)
        {
            var uow = _uowProvider.CurrentUow;
            var currentDate = _environmentInfoService.CurrentUtcDateTime;

            var caseStatus = await GetOrCreateRelocationPlanStatusAsync(caseStatusSourceId, uow);

            var isUpdated = Reflector.SetProperty(() => relocationPlan.SourceId, relocationPlanSourceId);
            isUpdated |= compensation == null
                ? Reflector.SetProperty(() => relocationPlan.Compensation, null)
                : UpdateFrom(relocationPlan.Compensation ??= new CompensationInfo(), compensation);

            var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId);
            var previousStepId = relocationPlan.CurrentStepId;
            var newStepId = relocationPlan.MatchStep(caseProgress, caseStatus);
            if (previousStepId != newStepId)
            {
                isUpdated = true;
                relocationPlan.SetStep(newStepId, countrySteps, currentDate);
                newStepId = relocationPlan.CurrentStepId;
            }

            var previousStatusId = relocationPlan.StatusId;
            var newStatus = await GetStatusFromStateAsync(relocationPlan.State, caseStatus, newStepId, uow);

            var isStatusUpdated = Reflector.SetProperty(() => relocationPlan.StatusId, newStatus.Status.Id);
            if (isStatusUpdated)
            {
                isUpdated = true;
                relocationPlan.Status = newStatus.Status;

                if (newStatus.FromStepId == null)
                {
                    relocationPlan.StatusStartDate = currentDate;
                    relocationPlan.StatusDueDate = null;
                }
                else
                {
                    var currentStep = relocationPlan.CurrentStep;
                    relocationPlan.StatusStartDate = currentDate;
                    relocationPlan.StatusDueDate = currentStep.ExpectedAt;
                }

                if (isStatusHistoryRequired)
                {
                    var relocationPlanChangeRepository = uow.GetRepository<RelocationPlanChange>();
                    var change = new RelocationPlanChange
                    {
                        Type = RelocationPlanChangeType.Status,
                        RelocationPlanId = relocationPlan.Id,
                        EmployeeId = relocationPlan.EmployeeId,
                        PreviousStatusId = previousStatusId,
                        NewStatusId = newStatus.Status.Id,
                        UpdateDate = currentDate,
                    };

                    relocationPlanChangeRepository.Add(change);
                }
            }

            if (isUpdated)
            {
                relocationPlan.UpdateDate = currentDate;
                relocationPlan.UpdatedBy = null;
                await uow.SaveChangesAsync();
            }
        }

        public async Task UpdateApproverInfoAsync(RelocationPlan relocationPlan, RelocationPlanApproverInfo fromApproverInfo, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            if (relocationPlan.ApproverComment != fromApproverInfo.ApproverComment)
            {
                relocationPlan.ApproverCommentChangeDate = currentDate;
            }

            relocationPlan.ApproverComment = fromApproverInfo.ApproverComment;
            relocationPlan.ApproverDate = fromApproverInfo.ApproverDate;
            relocationPlan.Salary = fromApproverInfo.Salary;
            relocationPlan.RelocationUnitId = fromApproverInfo.UnitId;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> UpdateHrManagerInfoAsync(
            RelocationPlan relocationPlan,
            RelocationPlanHrManagerInfo fromHrManagerInfo,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            if (relocationPlan.HrManagerDate != fromHrManagerInfo.HrManagerDate && relocationPlan.Status.ExternalId != RelocationPlanStatus.BuiltIn.ReadyForEmployment)
            {
                return RelocationPlanManagementError.CanNotChangeEmploymentDateInCurrentState;
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            if (relocationPlan.HrManagerComment != fromHrManagerInfo.HrManagerComment)
            {
                relocationPlan.HrManagerCommentChangeDate = currentDate;
            }

            relocationPlan.HrManagerComment = fromHrManagerInfo.HrManagerComment;
            relocationPlan.HrManagerDate = fromHrManagerInfo.HrManagerDate;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task<OperationResult<IReadOnlyCollection<RelocationPlan>>> UpdateGmStatusesAsync(IReadOnlyCollection<RelocationPlan> relocationPlans, IReadOnlyCollection<RelocationPlanUpdate> fromRelocationPlanUpdates, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            if (relocationPlans.Count != fromRelocationPlanUpdates.Count)
            {
                return OperationResult<IReadOnlyCollection<RelocationPlan>>.CreateUnsuccessful();
            }

            var relocationPlanMap = relocationPlans.ToDictionary(p => p.Employee.ExternalId);
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            var updatedRelocationPlans = new List<RelocationPlan>();

            var countryStepsByCountry = await relocationPlanMap.Values
                .Select(r => r.Location.CountryId)
                .Distinct()
                .SelectAsync(async countryId => (countryId, steps: await GetCountryStepsAsync(countryId, uow)));
            var countryStepsMap = countryStepsByCountry.ToDictionary(x => x.countryId, x => x.steps);

            foreach (var relocationPlanUpdate in fromRelocationPlanUpdates)
            {
                var relocationPlan = relocationPlanMap[relocationPlanUpdate.EmployeeId];
                var inductionPassedChanged = relocationPlan.IsInductionPassed != relocationPlanUpdate.IsInductionPassed;

                if (!inductionPassedChanged)
                {
                    continue;
                }

                if (relocationPlan.IsConfirmed)
                {
                    return OperationResult<IReadOnlyCollection<RelocationPlan>>.CreateUnsuccessful();
                }

                relocationPlan.IsInductionPassed = relocationPlanUpdate.IsInductionPassed;
                relocationPlan.InductionStatusChangedBy = byPersonId;
                relocationPlan.InductionStatusChangeDate = currentDate;
                relocationPlan.UpdatedBy = byPersonId;
                relocationPlan.UpdateDate = currentDate;

                if (relocationPlan.IsInductionPassed)
                {
                    var countrySteps = countryStepsMap[relocationPlan.Location.CountryId];
                    relocationPlan.SetStep(RelocationStepId.RelocationConfirmation, countrySteps, currentDate);
                }

                await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

                updatedRelocationPlans.Add(relocationPlan);
            }

            await uow.SaveChangesAsync();

            return updatedRelocationPlans;
        }

        public async Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsConfirmed)> ConfirmRelocationPlanAsync(RelocationPlan relocationPlan, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            if (!relocationPlan.IsInductionPassed)
            {
                return (RelocationPlanManagementError.CanNotConfirmRelocationPlanInCurrentState, false);
            }

            if (relocationPlan.IsConfirmed)
            {
                return (relocationPlan, false);
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            relocationPlan.IsConfirmed = true;
            relocationPlan.ConfirmationDate = currentDate;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;

            await PickApproverAsync(relocationPlan, uow);

            var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId, uow);
            relocationPlan.SetStep(RelocationStepId.PendingApproval, countrySteps, currentDate);

            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return (relocationPlan, true);
        }

        public async Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsApproved)> ApproveRelocationPlanAsync(RelocationPlan relocationPlan, string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var canBeApproved = CheckIfRelocationPlanCanBeApproved(relocationPlan);
            if (!canBeApproved)
            {
                return (RelocationPlanManagementError.CanNotApproveRelocationPlanInCurrentState, false);
            }

            if (relocationPlan.IsApproved)
            {
                return (relocationPlan, false);
            }

            var now = _environmentInfoService.CurrentUtcDateTime;
            var relocationPlanRepository = uow.RelocationPlans;
            var latestSameCountryApprovedRelocationPlan = await relocationPlanRepository.GetLatestSameCountryApprovedRelocationPlanAsync(relocationPlan.EmployeeId, relocationPlan.Location.CountryId);
            relocationPlan.ApprovalDate = latestSameCountryApprovedRelocationPlan != null
                ? latestSameCountryApprovedRelocationPlan.ApprovalDate
                : now;

            relocationPlan.IsApproved = true;
            relocationPlan.ApprovedBy = byPersonId;
            relocationPlan.UpdateDate = now;
            relocationPlan.UpdatedBy = byPersonId;

            var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId, uow);
            relocationPlan.SetStep(RelocationStepId.ProcessingQueue, countrySteps, now);

            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return (relocationPlan, true);
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetApproverAsync(
            RelocationPlan relocationPlan,
            Employee employee,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            if (relocationPlan.State != RelocationPlanState.Active)
            {
                return RelocationPlanManagementError.CanNotSetApproverInCurrentState;
            }

            if (!employee.IsActive)
            {
                return RelocationPlanManagementError.CanNotSetInactiveApprover;
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            relocationPlan.Approver = employee;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetHrManagerAsync(
            RelocationPlan relocationPlan,
            Employee employee,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            relocationPlan.HrManager = employee;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task<EntityManagementResult<RelocationPlan, RelocationPlanManagementError>> SetGmManagerAsync(
            RelocationPlan relocationPlan,
            Employee employee,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            relocationPlan.GmManager = employee;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return relocationPlan;
        }

        public async Task<(EntityManagementResult<RelocationPlan, RelocationPlanManagementError> Result, bool IsEmploymentConfirmed)> ConfirmRelocationPlanEmploymentByEmployeeAsync(
            RelocationPlan relocationPlan,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;

            if (relocationPlan.Status.ExternalId != RelocationPlanStatus.BuiltIn.EmploymentConfirmationByEmployee)
            {
                return (RelocationPlanManagementError.CanNotConfirmEmploymentByEmployeeInCurrentState, false);
            }

            if (relocationPlan.Employee.CurrentLocationId == null ||
                relocationPlan.Employee.CurrentLocation.Location.CountryId == null ||
                relocationPlan.Employee.CurrentLocation.Location.CountryId != relocationPlan.Location.CountryId)
            {
                return (RelocationPlanManagementError.CurrentCountryDoesNotMatchRelocationCountry, false);
            }

            if (relocationPlan.IsEmploymentConfirmedByEmployee)
            {
                return (relocationPlan, false);
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            relocationPlan.IsEmploymentConfirmedByEmployee = true;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;

            var countrySteps = await GetCountryStepsAsync(relocationPlan.Location.CountryId, uow);
            relocationPlan.SetStep(RelocationStepId.EmploymentInProgress, countrySteps, currentDate);
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            await uow.SaveChangesAsync();

            return (relocationPlan, true);
        }

        public async Task<bool> CloseAsync(
            RelocationPlan relocationPlan,
            string byPersonId,
            RelocationPlanCloseReason reason = RelocationPlanCloseReason.Completed,
            string closeComment = null,
            DateTime? closeDate = null,
            IEmployeeServiceUnitOfWork uow = null)
        {
            if (relocationPlan.State != RelocationPlanState.Active)
            {
                return false;
            }

            var isLocalTransaction = uow == null;
            uow ??= _uowProvider.CurrentUow;

            var previousRelocationPlan = relocationPlan.Clone();
            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            var relocationPlanState = CreateRelocationPlanState(reason);
            if (relocationPlanState != RelocationPlanState.Completed && String.IsNullOrEmpty(closeComment))
            {
                return false;
            }

            relocationPlan.State = relocationPlanState;
            relocationPlan.CloseComment = closeComment;
            relocationPlan.ClosedBy = byPersonId;
            relocationPlan.CloseDate = closeDate ?? currentDate;
            relocationPlan.UpdateDate = currentDate;
            relocationPlan.UpdatedBy = byPersonId;
            await UpdateRelocationPlanStatusAsync(relocationPlan, uow);

            if (isLocalTransaction)
            {
                await uow.SaveChangesAsync();
            }

            await RelocationPlanClosed.RaiseAsync(new RelocationPlanChangedEventArgs(relocationPlan, previousRelocationPlan));

            return true;
        }

        public async Task HandleEmployeeUpdateAsync(Employee previousEmployee, Employee newEmployee, IEmployeeServiceUnitOfWork uow)
        {
            if (previousEmployee.OrganizationId == newEmployee.OrganizationId)
            {
                return;
            }

            var relocationRepository = uow.GetRepository<RelocationPlan>();
            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.ByEmployeeId(newEmployee.Id);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var employeeRelocationPlan = await relocationRepository.GetSingleOrDefaultAsync(specification, loadStrategy);
            if (employeeRelocationPlan == null)
            {
                return;
            }

            var organizationOperationResult = await _departmentService.GetOrganizationAsync(newEmployee.OrganizationId);
            if (!organizationOperationResult.IsSuccessful)
            {
                LoggerContext.Current.LogWarning("Failed to get organization by organization id {organizationId} for employee {employeeId}. Error codes: {errorCodes}.", newEmployee.OrganizationId, newEmployee.Id, organizationOperationResult.ErrorCodes.JoinStrings());
                return;
            }

            var organization = organizationOperationResult.Result;
            if (employeeRelocationPlan.Location.CountryId != organization.CountryId)
            {
                LoggerContext.Current.Log("Organization country {organizationCountryId} of employee with id {employeeId} is different from relocation country {relocationCountryId}. Skipping automatic relocation plan close.", organization.CountryId, newEmployee.Id, employeeRelocationPlan.Location.CountryId);
                return;
            }

            LoggerContext.Current.Log("Organization of employee with id {employeeId} has changed to the organization, requested by relocation. Closing active relocation plan...", newEmployee.Id);
            await CloseAsync(employeeRelocationPlan, newEmployee.UpdatedBy, uow: uow);
        }

        public async Task HandleEmployeeUpdatedAsync(EmployeeDataContract previousEmployee, EmployeeDataContract employee)
        {
            if (!previousEmployee.IsActive || employee.IsActive)
            {
                return;
            }

            var uow = _uowProvider.CurrentUow;
            var relocationRepository = uow.GetRepository<RelocationPlan>();
            var specification = RelocationPlanSpecification.Active & RelocationPlanSpecification.ByApprover(employee.Id);
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await relocationRepository.GetWhereAsync(specification, loadStrategy);

            var countryIds = relocationPlans.Select(r => r.Location.CountryId).ToHashSet();

            var approvers = await _relocationApproverService.GetAsync(countryIds);
            var approversMap = approvers.ToGroupedDictionary(a => a.CountryId);

            foreach (var relocationPlan in relocationPlans)
            {
                if (!approversMap.TryGetValue(relocationPlan.Location.CountryId, out var countryApprovers))
                {
                    LoggerContext.Current.LogWarning("Country {countryId} has no approvers", relocationPlan.Location.CountryId);

                    relocationPlan.ApproverId = null;

                    continue;
                }

                var approver = _relocationApproverService.PickUpperApprover(relocationPlan, countryApprovers);
                relocationPlan.Approver = approver?.Employee;
            }

            await uow.SaveChangesAsync();
        }

        public async Task AddRelocationPlanUpdateHistoryAsync(RelocationPlan previousRelocationPlan, RelocationPlan newRelocationPlan, string byPersonId)
        {
            var changes = new List<RelocationPlanChange>();
            if (previousRelocationPlan.LocationId != newRelocationPlan.LocationId)
            {
                changes.Add(new RelocationPlanChange
                {
                    Type = RelocationPlanChangeType.Destination,
                    PreviousDestinationId = previousRelocationPlan.LocationId,
                    NewDestinationId = newRelocationPlan.LocationId,
                });
            }
            if (previousRelocationPlan.IsInductionPassed != newRelocationPlan.IsInductionPassed)
            {
                changes.Add(new RelocationPlanChange
                {
                    Type = RelocationPlanChangeType.InductionPassed,
                    PreviousIsInductionPassed = previousRelocationPlan.IsInductionPassed,
                    NewIsInductionPassed = newRelocationPlan.IsInductionPassed,
                });
            }
            if (previousRelocationPlan.IsConfirmed != newRelocationPlan.IsConfirmed)
            {
                changes.Add(new RelocationPlanChange
                {
                    Type = RelocationPlanChangeType.Confirmed,
                    PreviousIsConfirmed = previousRelocationPlan.IsConfirmed,
                    NewIsConfirmed = newRelocationPlan.IsConfirmed,
                });
            }
            if (previousRelocationPlan.IsApproved != newRelocationPlan.IsApproved)
            {
                changes.Add(new RelocationPlanChange
                {
                    Type = RelocationPlanChangeType.Approved,
                    PreviousIsApproved = previousRelocationPlan.IsApproved,
                    NewIsApproved = newRelocationPlan.IsApproved,
                });
            }

            if (previousRelocationPlan.IsEmploymentConfirmedByEmployee != newRelocationPlan.IsEmploymentConfirmedByEmployee)
            {
                changes.Add(new RelocationPlanChange
                {
                    Type = RelocationPlanChangeType.EmploymentConfirmedByEmployee,
                    PreviousIsEmploymentConfirmedByEmployee = previousRelocationPlan.IsEmploymentConfirmedByEmployee,
                    NewIsEmploymentConfirmedByEmployee = newRelocationPlan.IsEmploymentConfirmedByEmployee,
                });
            }

            if (!changes.Any())
            {
                return;
            }

            var currentDate = _environmentInfoService.CurrentUtcDateTime;
            foreach (var change in changes)
            {
                change.RelocationPlanId = newRelocationPlan.Id;
                change.EmployeeId = newRelocationPlan.EmployeeId;
                change.UpdatedBy ??= byPersonId;
                change.UpdateDate = currentDate;
            }

            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlanChange>();

            repository.AddRange(changes);

            await uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<CountryRelocationStep>> GetCountryStepsAsync(string countryId, IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var stepsRepository = currentUow.GetRepository<CountryRelocationStep>();

            var steps = await stepsRepository.GetWhereAsync(s => s.CountryId == countryId);
            if (!steps.Any())
            {
                steps = CountryRelocationStep.GetDefaultSteps(countryId);
                stepsRepository.AddRange(steps);
            }
            var orderedSteps = steps.OrderBy(s => s.Order).ToList();

            return orderedSteps;
        }

        public async Task SyncCountryRelocationStepsAsync(IReadOnlyCollection<CountryRelocationStep> newSteps)
        {
            var uow = _uowProvider.CurrentUow;

            await ReconcileCountryStepsAsync(newSteps, uow);
            await UpdateRelocationStepsAsync(newSteps, uow);

            await uow.SaveChangesAsync();
        }


        private static async Task ReconcileCountryStepsAsync(
            IReadOnlyCollection<CountryRelocationStep> newSteps,
            IEmployeeServiceUnitOfWork uow)
        {
            var countryStepsRepository = uow.GetRepository<CountryRelocationStep>();
            var existingSteps = await countryStepsRepository.GetAllAsync();

            var existingStepsMap = existingSteps
                .GroupBy(s => s.CountryId)
                .ToDictionary(
                    g => g.Key,
                    steps => steps.ToDictionary(s => s.StepId));
            var newStepsMap = newSteps
                .GroupBy(s => s.CountryId)
                .ToDictionary(
                    g => g.Key,
                    steps => steps.ToDictionary(s => s.StepId));

            foreach (var (countryId, existingCountryStepsMap) in existingStepsMap)
            {
                if (newStepsMap.TryGetValue(countryId, out var newCountryStepsMap))
                {
                    var stepsToDelete = existingCountryStepsMap.Values
                        .Where(existingStep => !newCountryStepsMap.ContainsKey(existingStep.StepId))
                        .ToList();
                    countryStepsRepository.DeleteAll(stepsToDelete);
                }
            }

            foreach (var (countryId, newCountryStepsMap) in newStepsMap)
            {
                if (existingStepsMap.TryGetValue(countryId, out var existingCountryStepsMap))
                {
                    foreach (var (_, newStep) in newCountryStepsMap)
                    {
                        if (existingCountryStepsMap.TryGetValue(newStep.StepId, out var existingStep))
                        {
                            existingStep.Order = newStep.Order;
                            existingStep.DurationInDays = newStep.DurationInDays;
                        }
                        else
                        {
                            countryStepsRepository.Add(newStep);
                        }
                    }
                }
                else
                {
                    countryStepsRepository.AddRange(newCountryStepsMap.Values);
                }
            }
        }

        private static async Task UpdateRelocationStepsAsync(
            IReadOnlyCollection<CountryRelocationStep> newCountrySteps,
            IEmployeeServiceUnitOfWork uow)
        {
            var repo = uow.GetRepository<RelocationPlan>();
            var loadStrategy = new EntityLoadStrategy<RelocationPlan>(
                r => r.Location,
                r => r.Steps);
            var relocationPlans = await repo.GetAllAsync(loadStrategy);
            var getRelocationStepId = (RelocationPlan plan, RelocationPlanStep step) => new { step.StepId, plan.Location.CountryId };
            var getCountryStepId = (CountryRelocationStep step) => new { step.StepId, step.CountryId };

            var countryStepsMap = newCountrySteps.ToDictionary(getCountryStepId);

            foreach (var relocationPlan in relocationPlans)
            {
                foreach (var step in relocationPlan.Steps)
                {
                    if (countryStepsMap.TryGetValue(getRelocationStepId(relocationPlan, step), out var countryStep))
                    {
                        step.DurationInDays = countryStep.DurationInDays;
                    }
                }
            }
        }

        private async Task UpdateRelocationPlanStatusAsync(RelocationPlan relocationPlan, IEmployeeServiceUnitOfWork uow)
        {
            var status = await GetStatusFromStateAsync(relocationPlan.State, relocationPlan.Status, relocationPlan.CurrentStepId, uow);
            if (status.Status.Id == relocationPlan.StatusId)
            {
                return;
            }

            relocationPlan.Status = status.Status;
            relocationPlan.StatusId = status.Status.Id;

            var now = _environmentInfoService.CurrentUtcDateTime;
            if (status.FromStepId == null)
            {
                relocationPlan.StatusStartDate = now;
                relocationPlan.StatusDueDate = null;
            }
            else
            {
                var currentStep = relocationPlan.CurrentStep;
                relocationPlan.StatusStartDate = now;
                relocationPlan.StatusDueDate = currentStep.ExpectedAt;
            }
        }

        private async Task PickApproverAsync(RelocationPlan relocationPlan, IEmployeeServiceUnitOfWork uow)
        {
            if (!relocationPlan.IsConfirmed || relocationPlan.ApproverId != null)
            {
                return;
            }

            var approver = await _relocationApproverService.PickApproverAsync(relocationPlan, uow);
            if (approver == null)
            {
                return;
            }

            relocationPlan.ApproverId = approver.EmployeeId;
        }

        private async Task OnPrimaryApproversChanged(IReadOnlyCollection<RelocationApprover> primaryApprovers)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var primaryApproversCountries = primaryApprovers.Select(a => a.CountryId).Distinct().ToList();
            var loadStrategy = GetRelocationPlanLoadStrategy();
            var relocationPlans = await repository
                .GetWhereAsync(p => p.ApproverId == null && primaryApproversCountries.Contains(p.Location.CountryId) && p.IsConfirmed, loadStrategy);

            var primaryApproverMapByCountry = primaryApprovers.ToDictionary(a => a.CountryId);
            await UpdateRelocationApproversAsync(relocationPlans, primaryApproverMapByCountry, uow);
        }

        private async Task OnApproversRemoved(IReadOnlyCollection<RelocationApprover> approvers)
        {
            var uow = _uowProvider.CurrentUow;
            var repository = uow.GetRepository<RelocationPlan>();
            var loadStrategy = GetRelocationPlanLoadStrategy();

            var approverIds = approvers.Select(a => a.EmployeeId).Distinct().ToList();
            var relocationPlans = await repository
                .GetWhereAsync(p => p.ApproverId != null && approverIds.Contains(p.ApproverId.Value) && p.IsConfirmed && !p.IsApproved, loadStrategy);

            var approversCountries = relocationPlans.Select(r => r.Location.CountryId).Distinct().ToList();
            var primaryApprovers = await _relocationApproverService.GetPrimaryByCountryIdsAsync(approversCountries);
            var primaryApproverMapByCountry = primaryApprovers.ToDictionary(a => a.CountryId);

            await UpdateRelocationApproversAsync(relocationPlans, primaryApproverMapByCountry, uow);
        }

        private async Task UpdateRelocationApproversAsync(
            IReadOnlyCollection<RelocationPlan> relocationPlans,
            IReadOnlyDictionary<string, RelocationApprover> primaryApproverMapByCountry,
            IEmployeeServiceUnitOfWork uow)
        {
            var previousRelocationPlans = new Dictionary<int, RelocationPlan>();
            foreach (var relocationPlan in relocationPlans)
            {
                var previousRelocationPlan = relocationPlan.Clone();
                previousRelocationPlans.Add(previousRelocationPlan.Id, previousRelocationPlan);

                var approver = primaryApproverMapByCountry.GetValueOrDefault(relocationPlan.Location.CountryId);
                relocationPlan.ApproverId = approver?.EmployeeId;
                await UpdateRelocationPlanStatusAsync(relocationPlan, uow);
            }

            await uow.SaveChangesAsync();

            foreach (var relocationPlan in relocationPlans)
            {
                var previousRelocationPlan = previousRelocationPlans[relocationPlan.Id];
                await RelocationPlanUpdated.RaiseAsync(new RelocationPlanChangedEventArgs(relocationPlan, previousRelocationPlan));
            }
        }

        private static bool CheckIfRelocationPlanCanBeApproved(RelocationPlan relocationPlan)
        {
            return !String.IsNullOrEmpty(relocationPlan.Salary) &&
                   !String.IsNullOrEmpty(relocationPlan.ApproverComment) &&
                   relocationPlan.ApproverDate != null &&
                   relocationPlan.IsConfirmed;
        }

        private static RelocationPlanState CreateRelocationPlanState(RelocationPlanCloseReason reason)
        {
            return reason switch
            {
                RelocationPlanCloseReason.Completed => RelocationPlanState.Completed,
                RelocationPlanCloseReason.Cancelled => RelocationPlanState.Cancelled,
                RelocationPlanCloseReason.Rejected => RelocationPlanState.Rejected,
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unknown reason."),
            };
        }

        private static async Task<(RelocationPlanStatus Status, RelocationStepId? FromStepId)> GetStatusFromStateAsync(RelocationPlanState state, RelocationPlanStatus status, RelocationStepId stepId, IEmployeeServiceUnitOfWork uow)
        {
            var (statusId, fromStepId) = RelocationPlanStatus.From(state, status, stepId);
            var repository = uow.GetRepository<RelocationPlanStatus>();
            var newStatus = await repository.GetSingleAsync(s => s.ExternalId == statusId);

            return (newStatus, fromStepId);
        }

        private async Task<RelocationPlanStatus> GetOrCreateRelocationPlanStatusAsync(string caseStatusSourceId, IEmployeeServiceUnitOfWork uow)
        {
            var relocationPlanStatusRepository = uow.GetRepository<RelocationPlanStatus>();

            var status = await relocationPlanStatusRepository.GetSingleOrDefaultAsync(p => p.CaseStatus.SourceId == caseStatusSourceId);
            if (status != null)
            {
                return status;
            }

            var externalId = caseStatusSourceId.ToLower().Replace(' ', '_');
            var caseStatus = new RelocationCaseStatus
            {
                ExternalId = externalId,
                SourceId = caseStatusSourceId,
                Name = caseStatusSourceId,
                CreationDate = _environmentInfoService.CurrentUtcDateTime,
            };
            status = new RelocationPlanStatus
            {
                ExternalId = externalId,
                Name = caseStatusSourceId,
                CaseStatus = caseStatus,
            };
            relocationPlanStatusRepository.Add(status);

            await uow.SaveChangesAsync();

            return status;
        }

        private static bool UpdateFrom(CompensationInfo compensation, CompensationInfo fromCompensation)
        {
            compensation.PreviousCompensation ??= new PreviousCompensationInfo();
            compensation.Details ??= new CompensationInfoDetails
            {
                Child = new CompensationInfoDetailsItem(),
                Spouse = new CompensationInfoDetailsItem(),
                Employee = new CompensationInfoDetailsItem(),
            };

            var isUpdated = Reflector.SetProperty(() => compensation.Total, fromCompensation.Total) |
                            Reflector.SetProperty(() => compensation.Currency, fromCompensation.Currency) |
                            Reflector.SetProperty(() => compensation.PaidInAdvance, fromCompensation.PaidInAdvance) |
                            Reflector.SetProperty(() => compensation.Details.Child.Amount, fromCompensation.Details.Child.Amount) |
                            Reflector.SetProperty(() => compensation.Details.Child.Enabled, fromCompensation.Details.Child.Enabled) |
                            Reflector.SetProperty(() => compensation.Details.Child.NumberOfPeople, fromCompensation.Details.Child.NumberOfPeople) |
                            Reflector.SetProperty(() => compensation.Details.Spouse.Amount, fromCompensation.Details.Spouse.Amount) |
                            Reflector.SetProperty(() => compensation.Details.Spouse.Enabled, fromCompensation.Details.Spouse.Enabled) |
                            Reflector.SetProperty(() => compensation.Details.Spouse.NumberOfPeople, fromCompensation.Details.Spouse.NumberOfPeople) |
                            Reflector.SetProperty(() => compensation.Details.Employee.Amount, fromCompensation.Details.Employee.Amount) |
                            Reflector.SetProperty(() => compensation.Details.Employee.Enabled, fromCompensation.Details.Employee.Enabled) |
                            Reflector.SetProperty(() => compensation.Details.Employee.NumberOfPeople, fromCompensation.Details.Employee.NumberOfPeople);

            if (fromCompensation.PreviousCompensation != null)
            {
                isUpdated |= Reflector.SetProperty(() => compensation.PreviousCompensation.Currency, fromCompensation.PreviousCompensation.Currency) |
                             Reflector.SetProperty(() => compensation.PreviousCompensation.Amount, fromCompensation.PreviousCompensation.Amount);
            }
            else
            {
                isUpdated |= Reflector.SetProperty(() => compensation.PreviousCompensation.Currency, String.Empty) |
                             Reflector.SetProperty(() => compensation.PreviousCompensation.Amount, 0);
            }

            return isUpdated;
        }

        private static IEntityLoadStrategy<RelocationPlan> GetSlimRelocationPlanLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationPlan>(p => p.Employee);
        }

        private static IEntityLoadStrategy<RelocationPlan> GetRelocationPlanLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationPlan>(
                p => p.Location,
                p => p.Employee.CurrentLocation.Location,
                p => p.Approver,
                p => p.HrManager,
                p => p.Status,
                p => p.Compensation,
                p => p.GmManager,
                p => p.Steps);
        }

        private static IEntityLoadStrategy<RelocationPlanChange> GetRelocationPlanChangeLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationPlanChange>(p => p.Employee, p => p.RelocationPlan, p => p.PreviousDestination, p => p.NewDestination, p => p.PreviousStatus, p => p.NewStatus);
        }


        private sealed class RelocationPlanSpecification : Specification<RelocationPlan>
        {
            private RelocationPlanSpecification(Expression<Func<RelocationPlan, bool>> predicate)
                : base(predicate)
            {
            }


            public static Specification<RelocationPlan> Any
                => new RelocationPlanSpecification(p => true);

            public static Specification<RelocationPlan> Active
                => new RelocationPlanSpecification(p => p.State == RelocationPlanState.Active);

            public static Specification<RelocationPlan> Approved
                => new RelocationPlanSpecification(p => p.IsApproved);

            public static Specification<RelocationPlan> WithRelocationUnit
                => new RelocationPlanSpecification(p => p.RelocationUnitId != null);

            public static Specification<RelocationPlan> ByChangeDate(DateTime changeDate)
                => new RelocationPlanSpecification(p => p.UpdateDate.HasValue && p.UpdateDate > changeDate || p.CreationDate > changeDate);

            public static Specification<RelocationPlan> ByEmployeeId(string id)
                => new RelocationPlanSpecification(p => p.Employee.ExternalId == id);

            public static Specification<RelocationPlan> ByEmployeeId(int id)
                => new RelocationPlanSpecification(p => p.EmployeeId == id);

            public static Specification<RelocationPlan> ByEmployeeIds(IReadOnlyCollection<string> ids)
                => new RelocationPlanSpecification(p => ids.Contains(p.Employee.ExternalId));

            public static Specification<RelocationPlan> ByEmployeeIds(IReadOnlyCollection<int> ids)
                => new RelocationPlanSpecification(p => ids.Contains(p.EmployeeId));

            public static Specification<RelocationPlan> ByExternalId(string id)
                => new RelocationPlanSpecification(p => p.ExternalId == id);

            public static Specification<RelocationPlan> ByExternalIds(IReadOnlyCollection<string> ids)
                => new RelocationPlanSpecification(p => ids.Contains(p.ExternalId));

            public static Specification<RelocationPlan> ByHrManagerDate(DateOnly fromDate, DateOnly toDate)
                => new RelocationPlanSpecification(p => p.HrManagerDate != null && fromDate <= p.HrManagerDate && p.HrManagerDate <= toDate);

            public static Specification<RelocationPlan> ByLessOrEqualStatusDueDate(DateTime statusDueDate)
                => new RelocationPlanSpecification(p => p.StatusDueDate.HasValue && p.StatusDueDate.Value.Date <= statusDueDate.Date);

            public static Specification<RelocationPlan> ByEqualStatusDueDate(DateTime statusDueDate)
                => new RelocationPlanSpecification(p => p.StatusDueDate.HasValue && p.StatusDueDate.Value.Date == statusDueDate.Date);

            public static Specification<RelocationPlan> ByStatusId(string relocationPlanStatusId)
                => new RelocationPlanSpecification(p => p.Status.ExternalId == relocationPlanStatusId);

            public static Specification<RelocationPlan> WithActiveEmployee
                => new RelocationPlanSpecification(p => p.Employee.IsActive);

            public static Specification<RelocationPlan> Confirmed(DateTime from, DateTime to)
                => new RelocationPlanSpecification(p => p.IsConfirmed && p.ConfirmationDate.HasValue && p.ConfirmationDate.Value >= from && p.ConfirmationDate <= to);

            public static Specification<RelocationPlan> PendingInduction(DateTime from, DateTime to)
                => new RelocationPlanSpecification(p => !p.IsInductionPassed && p.CreationDate >= from && p.CreationDate <= to);

            public static Specification<RelocationPlan> ByApprover(string id)
                => new RelocationPlanSpecification(p => p.Approver.ExternalId == id);

            public static Specification<RelocationPlan> EmployeeNotInternship
                => new RelocationPlanSpecification(p => EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(p.Employee));
        }
    }
}
