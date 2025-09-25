using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.EqualityComparison;
using DreamTeam.Common.Extensions;
using DreamTeam.Common.Observable;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.Core.DepartmentService;
using DreamTeam.Core.DepartmentService.DataContracts;
using DreamTeam.Core.DepartmentService.Units;
using DreamTeam.Logging;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Services.DataContracts;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.DataContracts;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationApprovers
{
    [UsedImplicitly]
    public sealed class RelocationApproverService : IRelocationApproverService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;
        private readonly IDepartmentService _departmentService;
        private readonly IUnitProvider _unitProvider;


        public event AsyncObserver<IReadOnlyCollection<RelocationApprover>> PrimaryApproversChanged;

        public event AsyncObserver<IReadOnlyCollection<RelocationApprover>> ApproversRemoved;


        public RelocationApproverService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService,
            IDepartmentService departmentService,
            IUnitProvider unitProvider)
        {
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;
            _departmentService = departmentService;
            _unitProvider = unitProvider;
        }


        public async Task<bool> CheckIsEmployeeRelocationApproverOrHasAssignedRequestsAsync(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();

            var isRelocationApprover = await approverRepository.AnyAsync(a => a.Employee.ExternalId == employeeId);
            if (isRelocationApprover)
            {
                return true;
            }

            var relocationRequestRepository = uow.RelocationPlans;
            var hasAssignedRequests = await relocationRequestRepository.AnyAsync(r => r.State == RelocationPlanState.Active &&
                                                                                      r.Approver.ExternalId == employeeId);

            return hasAssignedRequests;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> GetAllPrimaryApproversAsync()
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.Primary;
            var loadStrategy = GetApproverLoadStrategy();
            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);

            return approvers;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> GetAsync(string countryId = null, IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var approverRepository = currentUow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.Any;
            if (countryId != null)
            {
                specification &= RelocationApproverSpecification.ByCountryId(countryId);
            }
            var loadStrategy = GetApproverLoadStrategy();
            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);

            return approvers;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> GetAsync(IReadOnlyCollection<string> countryIds, IEmployeeServiceUnitOfWork uow = null)
        {
            var currentUow = uow ?? _uowProvider.CurrentUow;
            var approverRepository = currentUow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.ByCountryIds(countryIds);
            var loadStrategy = GetApproverLoadStrategy();

            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);

            return approvers;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> GetByEmployeeIdAsync(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.ByEmployeeExternalId(employeeId);
            var loadStrategy = GetApproverLoadStrategy();
            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);

            return approvers;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> GetPrimaryByCountryIdsAsync(IReadOnlyCollection<string> countryIds)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.Primary & RelocationApproverSpecification.ByCountryIds(countryIds);
            var loadStrategy = GetApproverLoadStrategy();
            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);

            return approvers;
        }

        public async Task<RelocationApprover> GetPrimaryAsync(string countryId)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var specification = RelocationApproverSpecification.Primary & RelocationApproverSpecification.ByCountryId(countryId);
            var loadStrategy = GetApproverLoadStrategy();
            var approver = await approverRepository.GetSingleOrDefaultAsync(specification, loadStrategy);

            return approver;
        }

        public async Task<IReadOnlyCollection<RelocationApprover>> UpdateAsync(
            string countryId,
            IReadOnlyCollection<RelocationApprover> fromApprovers,
            string byPersonId)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var existingApprovers = await GetAsync(countryId);
            var existingApproversMap = existingApprovers.ToDictionary(a => a.EmployeeId);
            var existingPrimaryApprover = existingApprovers.SingleOrDefault(a => a.IsPrimary);

            var approversQueue = existingApprovers
                .Where(a => !a.IsPrimary)
                .OrderBy(a => a.ApproverOrder.Order)
                .ToList();

            approverRepository.DeleteAll(existingApprovers);

            foreach (var approver in fromApprovers)
            {
                if (existingApproversMap.TryGetValue(approver.EmployeeId, out var existingApprover))
                {
                    approver.CreatedBy = existingApprover.CreatedBy;
                    approver.CreationDate = existingApprover.CreationDate;
                    approver.UpdatedBy = byPersonId;
                    approver.UpdateDate = _environmentInfoService.CurrentUtcDateTime;
                    approver.ApproverOrder = existingApprover.ApproverOrder;

                    var queuePosition = approversQueue.FindIndex(a => existingApprover.EmployeeId == a.EmployeeId);
                    if (queuePosition >= 0)
                    {
                        approversQueue.RemoveAt(queuePosition);
                        approversQueue.Insert(queuePosition, approver);
                    }

                    var shouldAddToQueue = existingApprover.IsPrimary && !approver.IsPrimary;
                    if (shouldAddToQueue)
                    {
                        approversQueue = InsertInQueue(approversQueue, approver);
                    }
                }
                else
                {
                    approver.CreatedBy = byPersonId;
                    approver.CreationDate = _environmentInfoService.CurrentUtcDateTime;

                    approversQueue = InsertInQueue(approversQueue, approver);
                }
            }

            var keyComparer = new KeySelectingEqualityComparer<RelocationApprover, int>(a => a.EmployeeId);
            var removedApprovers = existingApprovers.Except(fromApprovers, keyComparer).ToList();
            RecalculateQueue(approversQueue, removedApprovers, uow);

            approverRepository.AddRange(fromApprovers);

            await uow.SaveChangesAsync();

            var changedPrimaryApprover = fromApprovers.SingleOrDefault(a => a.IsPrimary && a.EmployeeId != existingPrimaryApprover?.EmployeeId);
            if (changedPrimaryApprover != null)
            {
                await PrimaryApproversChanged.RaiseAsync(new[] { changedPrimaryApprover });
            }

            if (removedApprovers.Any())
            {
                await ApproversRemoved.RaiseAsync(removedApprovers);
            }

            return fromApprovers;
        }

        public async Task HandleEmployeeChangedAsync(EmployeeDataContract previousEmployee, EmployeeDataContract employee)
        {
            var isUnitChanged = previousEmployee.UnitId != employee.UnitId;
            if (isUnitChanged && employee.IsActive && previousEmployee.IsActive)
            {
                var getUnit = await _departmentService.GetUnitByIdAsync(employee.UnitId);
                if (!getUnit.IsSuccessful)
                {
                    LoggerContext.Current.LogWarning($"Failed to get unit {employee.UnitId} because {getUnit.ErrorCodes.JoinStrings()}");
                    return;
                }

                if (getUnit.Result.IsProduction)
                {
                    return;
                }
            }
            else if (!previousEmployee.IsActive || employee.IsActive)
            {
                return;
            }

            var approverInCountries = await GetByEmployeeIdAsync(employee.Id);
            if (!approverInCountries.Any())
            {
                return;
            }

            await RemoveApproversAsync(approverInCountries);
        }

        public async Task HandleUnitChangedAsync(ServiceEntityChanged<UnitDataContract> unitChanged)
        {
            var unit = unitChanged.NewValue;
            var previousUnit = unitChanged.PreviousValue;
            if (unitChanged.ChangeType != ServiceEntityChangeType.Update || !previousUnit.IsProduction || unit.IsProduction)
            {
                return;
            }

            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();
            var loadStrategy = GetApproverLoadStrategy();
            var specification = RelocationApproverSpecification.ByUnitId(unit.Id);

            var approvers = await approverRepository.GetWhereAsync(specification, loadStrategy);
            if (!approvers.Any())
            {
                return;
            }

            await RemoveApproversAsync(approvers);
        }

        public async Task<RelocationApprover> PickApproverAsync(RelocationPlan relocationPlan, IEmployeeServiceUnitOfWork uow)
        {
            var employeeUnitId = relocationPlan.Employee?.UnitId;
            var countryId = relocationPlan.Location.CountryId;
            if (!String.IsNullOrEmpty(employeeUnitId))
            {
                var getUnit = await _departmentService.GetUnitByIdAsync(employeeUnitId);
                if (!getUnit.IsSuccessful)
                {
                    LoggerContext.Current.LogWarning($"Failed to get unit {employeeUnitId} because {getUnit.ErrorCodes.JoinStrings()}");
                }
                else if (!getUnit.Result.IsProduction)
                {
                    var primaryApprover = await GetPrimaryAsync(countryId);

                    return primaryApprover;
                }
            }

            var approvers = await GetAsync(countryId, uow);

            var approversQueueExceptPrimary = approvers
                .Where(a => !a.IsPrimary)
                .OrderBy(a => a.ApproverOrder.Order)
                .ToList();

            var approver = approversQueueExceptPrimary.FirstOrDefault(a => a.ApproverOrder?.IsNext == true);
            if (approver != null)
            {
                var approverIndex = approversQueueExceptPrimary.IndexOf(approver);
                var nextApprover = approversQueueExceptPrimary[(approverIndex + 1) % approversQueueExceptPrimary.Count];

                approver.ApproverOrder.IsNext = false;
                nextApprover.ApproverOrder.IsNext = true;
            }

            approver ??= approvers.FirstOrDefault(a => a.IsPrimary);

            if (approver != null)
            {
                var assignmentRepository = uow.GetRepository<RelocationApproverAssignment>();
                var newAssignment = new RelocationApproverAssignment
                {
                    Date = _environmentInfoService.CurrentUtcDateTime,
                    RelocationPlan = relocationPlan,
                    Approver = approver.Employee,
                };

                assignmentRepository.Add(newAssignment);
            }

            return approver;
        }

        public RelocationApprover PickUpperApprover(RelocationPlan relocationPlan, IReadOnlyCollection<RelocationApprover> relocationCountryApprovers)
        {
            if (!relocationPlan.ApproverId.HasValue)
            {
                return null;
            }

            var primaryApprover = relocationCountryApprovers.FirstOrDefault(a => a.IsPrimary);

            var relocationEmployeeUnit = _unitProvider.GetUnitById(relocationPlan.Employee.UnitId);
            if (!relocationEmployeeUnit.IsProduction)
            {
                return primaryApprover;
            }

            var approverManagers = _unitProvider.GetEmployeeManagedUnits(relocationPlan.Approver.ExternalId)
                .SelectMany(u => _unitProvider.GetEmployeeParentUnitsManagers(u.Id, relocationPlan.Approver.ExternalId))
                .ToHashSet();

            var approver = relocationCountryApprovers.FirstOrDefault(a => !a.IsPrimary && approverManagers.Contains(a.Employee.ExternalId)) ??
                           primaryApprover;

            return approver;
        }

        public async Task<RelocationApproverAssignmentsProfile> GetApproverAssignmentsProfileAsync(string countryId)
        {
            var uow = _uowProvider.CurrentUow;
            var assignmentsRepository = uow.GetRepository<RelocationApproverAssignment>();
            var loadStrategy = GetAssignmentLoadStrategy();

            var assignments = await assignmentsRepository.GetWhereAsync(a => a.RelocationPlan.Location.CountryId == countryId, loadStrategy);

            var approverRepository = uow.GetRepository<RelocationApprover>();
            var approverLoadStrategy = GetApproverLoadStrategy();
            var approvers = await approverRepository.GetWhereAsync(a => (a.ApproverOrder.IsNext || a.IsPrimary) && a.CountryId == countryId, approverLoadStrategy);
            var nextApprover = approvers.SingleOrDefault(a => a.ApproverOrder?.IsNext == true) ?? approvers.SingleOrDefault();

            var assignmentsProfile = new RelocationApproverAssignmentsProfile
            {
                NextApprover = nextApprover,
                Assignments = assignments,
            };

            return assignmentsProfile;
        }

        private static List<RelocationApprover> InsertInQueue(
            List<RelocationApprover> queue,
            RelocationApprover approver)
        {
            if (approver.IsPrimary)
            {
                return queue;
            }

            var newQueue = queue.ToList();
            var nextOrder = newQueue.FindIndex(a => a.ApproverOrder.IsNext);

            approver.ApproverOrder = new RelocationApproverOrder
            {
                IsNext = nextOrder < 0,
            };

            newQueue.Insert(nextOrder < 0 ? 0 : nextOrder, approver);

            return newQueue;
        }

        private static void RecalculateQueue(
            IReadOnlyCollection<RelocationApprover> previousQueue,
            IReadOnlyCollection<RelocationApprover> removedApprovers,
            IUnitOfWork uow)
        {
            if (previousQueue.Count == 0)
            {
                return;
            }

            var removedFromQueueApproverIds = removedApprovers.Select(a => a.EmployeeId).ToHashSet();
            var ordersToRemove = removedApprovers.Where(a => a.ApproverOrder != null).Select(a => a.ApproverOrder).ToList();

            var primaryApprover = previousQueue.FirstOrDefault(a => a.IsPrimary);
            if (primaryApprover != null)
            {
                if (primaryApprover.ApproverOrder != null)
                {
                    ordersToRemove.Add(primaryApprover.ApproverOrder);
                }

                removedFromQueueApproverIds.Add(primaryApprover.EmployeeId);
            }

            var queue = previousQueue.ToList();
            var nextApprover = queue.Single(a => a.ApproverOrder.IsNext);
            if (removedFromQueueApproverIds.Contains(nextApprover.EmployeeId))
            {
                var nextApproverIndex = queue.IndexOf(nextApprover);
                nextApprover = queue
                    .Skip(nextApproverIndex + 1)
                    .Concat(queue.Take(nextApproverIndex + 1))
                    .FirstOrDefault(a => !removedFromQueueApproverIds.Contains(a.EmployeeId));

                if (nextApprover != null)
                {
                    nextApprover.ApproverOrder.IsNext = true;
                }
            }

            var queueOrderRepository = uow.GetRepository<RelocationApproverOrder>();
            queueOrderRepository.DeleteAll(ordersToRemove);

            queue = queue.ExceptBy(removedFromQueueApproverIds, a => a.EmployeeId).ToList();
            queue.ForEach((approver, index) => approver.ApproverOrder.Order = index);
        }

        private async Task RemoveApproversAsync(IReadOnlyCollection<RelocationApprover> approversToRemove)
        {
            var uow = _uowProvider.CurrentUow;
            var approverRepository = uow.GetRepository<RelocationApprover>();

            approverRepository.DeleteAll(approversToRemove);

            var deletedPrimaryApprovers = approversToRemove.Where(a => a.IsPrimary).ToList();
            var keyComparer = EqualityComparerFactory<RelocationApprover>.FromKey(a => a.EmployeeId);
            var deletedApprovers = approversToRemove.Except(deletedPrimaryApprovers, keyComparer).ToList();
            if (deletedApprovers.Any())
            {
                var countryIds = deletedApprovers.Select(a => a.CountryId).ToList();
                var approvers = await GetAsync(countryIds, uow);
                var approversQueueMap = approvers.Where(a => !a.IsPrimary).ToGroupedDictionary(a => a.CountryId);

                foreach (var approver in deletedApprovers)
                {
                    var approversQueueInCountry = approversQueueMap[approver.CountryId];
                    RecalculateQueue(approversQueueInCountry, new[] { approver }, uow);
                }
            }

            await uow.SaveChangesAsync();

            if (deletedPrimaryApprovers.Any())
            {
                await PrimaryApproversChanged.RaiseAsync(deletedPrimaryApprovers);
            }

            if (deletedApprovers.Any())
            {
                await ApproversRemoved.RaiseAsync(deletedApprovers);
            }
        }



        private static IEntityLoadStrategy<RelocationApprover> GetApproverLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationApprover>(a => a.Employee, a => a.ApproverOrder);
        }

        private static IEntityLoadStrategy<RelocationApproverAssignment> GetAssignmentLoadStrategy()
        {
            return new EntityLoadStrategy<RelocationApproverAssignment>(a => a.RelocationPlan.Employee, a => a.Approver);
        }



        public sealed class RelocationApproverSpecification : Specification<RelocationApprover>
        {
            private RelocationApproverSpecification(Expression<Func<RelocationApprover, bool>> predicate)
                : base(predicate)
            {

            }


            public static Specification<RelocationApprover> Any
                => new RelocationApproverSpecification(a => true);

            public static Specification<RelocationApprover> ByCountryId(string id)
                => new RelocationApproverSpecification(a => a.CountryId == id);

            public static Specification<RelocationApprover> ByCountryIds(IReadOnlyCollection<string> ids)
                => new RelocationApproverSpecification(a => ids.Contains(a.CountryId));

            public static Specification<RelocationApprover> ByEmployeeExternalId(string id)
                => new RelocationApproverSpecification(a => a.Employee.ExternalId == id);

            public static Specification<RelocationApprover> ByUnitId(string unitId)
                => new RelocationApproverSpecification(a => a.Employee.UnitId == unitId);

            public static Specification<RelocationApprover> Primary
                => new RelocationApproverSpecification(a => a.IsPrimary);
        }
    }
}