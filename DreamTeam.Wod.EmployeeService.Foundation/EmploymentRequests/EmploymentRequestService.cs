using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Specification;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmploymentRequests
{
    [UsedImplicitly]
    public sealed class EmploymentRequestService : IEmploymentRequestService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;


        public EmploymentRequestService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider)
        {
            _uowProvider = uowProvider;
        }


        public async Task<IReadOnlyCollection<EmploymentRequest>> GetEmploymentRequestsAsync(bool includeWithEmployees, bool includeInternshipEmploymentRequests)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentRequestRepository = uow.GetRepository<EmploymentRequest>();
            var loadStrategy = GetEmploymentRequestLoadStrategy();
            var specification = new ActiveEmploymentRequestsSpecification() &
                                new EmploymentRequestsWithEmployeesSpecification(includeWithEmployees);
            if (!includeInternshipEmploymentRequests)
            {
                specification &= GetEmploymentRequestOfEmployeeSpecification();
            }

            var employmentRequests = await employmentRequestRepository.GetWhereAsync(specification, loadStrategy);

            return employmentRequests;
        }

        public async Task<IReadOnlyCollection<EmploymentRequest>> GetByIdsAsync(IReadOnlyCollection<string> ids, bool includeInternshipEmploymentRequests)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentRequestRepository = uow.GetRepository<EmploymentRequest>();
            var loadStrategy = GetEmploymentRequestLoadStrategy();
            Specification<EmploymentRequest> specification = new EmploymentRequestsByIdsSpecification(ids);
            if (!includeInternshipEmploymentRequests)
            {
                specification &= GetEmploymentRequestOfEmployeeSpecification();
            }

            var employmentRequests = await employmentRequestRepository.GetWhereAsync(specification, loadStrategy);

            return employmentRequests;
        }

        public async Task<IReadOnlyCollection<EmploymentRequest>> GetByPeriodWithoutInternsAsync(DateOnly fromDate, DateOnly toDate)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentRequestRepository = uow.EmploymentRequests;
            var specification = new ActiveEmploymentRequestsSpecification() &
                                new EmploymentRequestsWithEmployeesSpecification(false) &
                                new EmploymentRequestsByPeriodSpecification(fromDate, toDate) &
                                GetEmploymentRequestOfEmployeeSpecification();

            var employmentRequests = await employmentRequestRepository.GetUniqueAsync(specification);

            return employmentRequests;
        }


        private static IEntityLoadStrategy<EmploymentRequest> GetEmploymentRequestLoadStrategy()
        {
            return new EntityLoadStrategy<EmploymentRequest>(r => r.Employee);
        }

        private static Specification<EmploymentRequest> GetEmploymentRequestOfEmployeeSpecification()
        {
            return Specification<EmploymentRequest>.FromExpression(r => !r.EmployeeId.HasValue || EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(r.Employee));
        }



        private sealed class EmploymentRequestsByIdsSpecification : Specification<EmploymentRequest>
        {
            public EmploymentRequestsByIdsSpecification(IReadOnlyCollection<string> ids)
                : base(r => ids.Contains(r.ExternalId))
            {
            }
        }

        private sealed class EmploymentRequestsByPeriodSpecification : Specification<EmploymentRequest>
        {
            public EmploymentRequestsByPeriodSpecification(DateOnly fromDate, DateOnly toDate)
                : base(r => fromDate <= r.EmploymentDate && r.EmploymentDate <= toDate)
            {
            }
        }

        private sealed class EmploymentRequestsWithEmployeesSpecification : Specification<EmploymentRequest>
        {
            public EmploymentRequestsWithEmployeesSpecification(bool includeWithEmployees)
                : base(r => !r.EmployeeId.HasValue || includeWithEmployees)
            {
            }
        }

        private sealed class ActiveEmploymentRequestsSpecification : Specification<EmploymentRequest>
        {
            public ActiveEmploymentRequestsSpecification()
                : base(r => !r.SourceEmploymentRequest.CloseDate.HasValue)
            {
            }
        }
    }
}
