using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Specification;
using DreamTeam.Common.SystemUtilities;
using DreamTeam.DomainModel;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests
{
    [UsedImplicitly]
    public sealed class DismissalRequestService : IDismissalRequestService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;
        private readonly IEnvironmentInfoService _environmentInfoService;


        public DismissalRequestService(
            IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider,
            IEnvironmentInfoService environmentInfoService)
        {
            _uowProvider = uowProvider;
            _environmentInfoService = environmentInfoService;
        }


        public async Task<IReadOnlyCollection<DismissalRequest>> GetAllAsync(bool activeOnly = false)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.ByType(DismissalRequestType.Ordinary);
            if (activeOnly)
            {
                specification &= DismissalRequestSpecification.Active;
            }

            var dismissalRequests = await dismissalRequestRepository.GetWhereAsync(specification, loadStrategy);

            return dismissalRequests;
        }

        public Task<DismissalRequest> GetByIdAsync(string id)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.ByType(DismissalRequestType.Ordinary) &
                                Specification<DismissalRequest>.FromExpression(dr => dr.ExternalId == id);

            var dismissalRequest = dismissalRequestRepository.GetSingleOrDefaultAsync(specification, loadStrategy);

            return dismissalRequest;
        }

        public async Task<IReadOnlyCollection<DismissalRequest>> GetByPeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.Active &
                                DismissalRequestSpecification.ByType(DismissalRequestType.Ordinary) &
                                DismissalRequestSpecification.ByPeriod(fromDate, toDate) &
                                DismissalRequestSpecification.EmployeeNotInternship;

            var dismissalRequests = await dismissalRequestRepository.GetUniqueAsync(specification, loadStrategy);

            return dismissalRequests;
        }

        public async Task<IReadOnlyCollection<DismissalRequest>> GetByEmployeeIdAsync(string employeeId)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.Active &
                                DismissalRequestSpecification.ByType(DismissalRequestType.Ordinary) &
                                DismissalRequestSpecification.ByEmployeeId(employeeId);

            var dismissalRequests = await dismissalRequestRepository.GetWhereAsync(specification, loadStrategy);

            return dismissalRequests;
        }

        public async Task<IReadOnlyCollection<DismissalRequest>> GetByPeriodAndEmployeeIdsAsync(DateOnly fromDate, DateOnly toDate, IReadOnlyCollection<string> employeeIds)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestsRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.Active &
                                DismissalRequestSpecification.ByPeriod(fromDate, toDate) &
                                DismissalRequestSpecification.ByEmployeeIds(employeeIds);

            var dismissalRequests = await dismissalRequestsRepository.GetUniqueAsync(specification, loadStrategy);

            return dismissalRequests;
        }

        public async Task<IReadOnlyCollection<DismissalRequest>> GetByEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds, IReadOnlyCollection<DismissalRequestType> types = null)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestsRepository = uow.DismissalRequests;
            var loadStrategy = GetLoadStrategy();
            var specification = DismissalRequestSpecification.Active &
                                DismissalRequestSpecification.ByEmployeeIds(employeeIds);

            if (types is not null)
            {
                specification &= DismissalRequestSpecification.ByTypes(types);
            }

            var dismissalRequests = await dismissalRequestsRepository.GetUniqueAsync(specification, loadStrategy);

            return dismissalRequests;
        }

        public async Task<DismissalRequest> CreateAsync(DismissalRequest dismissalRequest)
        {
            var uow = _uowProvider.CurrentUow;
            var dismissalRequestsRepository = uow.DismissalRequests;

            var newDismissalRequest = new DismissalRequest
            {
                ExternalId = ExternalId.Generate(),
                IsActive = dismissalRequest.IsActive,
                EmployeeId = dismissalRequest.EmployeeId,
                Employee = dismissalRequest.Employee,
                DismissalDate = dismissalRequest.DismissalDate,
                Type = DismissalRequestType.Ordinary,
                CloseDate = dismissalRequest.CloseDate,
                CreationDate = _environmentInfoService.CurrentUtcDateTime,
            };

            dismissalRequestsRepository.Add(newDismissalRequest);
            await uow.SaveChangesAsync();

            return newDismissalRequest;
        }

        public async Task<DismissalRequest> UpdateAsync(DismissalRequest dismissalRequest, DismissalRequest fromDismissalRequest)
        {
            var uow = _uowProvider.CurrentUow;

            dismissalRequest.IsActive = fromDismissalRequest.IsActive;
            dismissalRequest.DismissalDate = fromDismissalRequest.DismissalDate;
            dismissalRequest.CloseDate = fromDismissalRequest.CloseDate;
            dismissalRequest.UpdateDate = _environmentInfoService.CurrentUtcDateTime;

            await uow.SaveChangesAsync();

            return dismissalRequest;
        }


        private static IEntityLoadStrategy<DismissalRequest> GetLoadStrategy()
        {
            return new EntityLoadStrategy<DismissalRequest>(r => r.Employee);
        }
    }
}
