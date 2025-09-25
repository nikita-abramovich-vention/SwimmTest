using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Common.Specification;
using DreamTeam.DomainModel;
using DreamTeam.Repositories;
using DreamTeam.Repositories.EntityLoadStrategy;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    [UsedImplicitly]
    public sealed class EmploymentPeriodService : IEmploymentPeriodService
    {
        private readonly IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> _uowProvider;


        public EmploymentPeriodService(IUnitOfWorkProvider<IEmployeeServiceUnitOfWork> uowProvider)
        {
            _uowProvider = uowProvider;
        }


        public async Task<IReadOnlyCollection<EmploymentPeriod>> GetAllReadOnlyAsync()
        {
            var loadStrategy = GetLoadStrategy();

            var uow = _uowProvider.CurrentUow;
            var employmentPeriodRepository = uow.EmploymentPeriods;

            var employmentPeriods = await employmentPeriodRepository.AsReadOnly().GetAllAsync(loadStrategy);

            return employmentPeriods;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriod>> GetByDateAsync(DateOnly date)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentPeriodRepository = uow.EmploymentPeriods;
            var loadStrategy = GetLoadStrategy();
            var specification = EmploymentPeriodSpecification.ByDate(date);

            var employmentPeriods = await employmentPeriodRepository.GetWhereAsync(specification, loadStrategy);

            return employmentPeriods;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriod>> GetByEmployeeIdsAsync(IReadOnlyCollection<string> employeeIds)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentPeriodRepository = uow.EmploymentPeriods;
            var loadStrategy = GetLoadStrategy();
            var specification = EmploymentPeriodSpecification.ByEmployeeIds(employeeIds);

            var employmentPeriods = await employmentPeriodRepository.GetWhereAsync(specification, loadStrategy);

            return employmentPeriods;
        }

        public async Task<IReadOnlyCollection<EmploymentPeriod>> GetForPeriodAsync(DateOnly startDate, DateOnly endDate, [CanBeNull]IReadOnlyCollection<string> employeeIds)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentPeriodRepository = uow.EmploymentPeriods;
            var loadStrategy = GetLoadStrategy();
            var specification = EmploymentPeriodSpecification.ForPeriod(startDate, endDate);
            if (employeeIds != null)
            {
                specification &= EmploymentPeriodSpecification.ByEmployeeIds(employeeIds);
            }

            var employmentPeriods = await employmentPeriodRepository.GetWhereAsync(specification, loadStrategy);

            return employmentPeriods;
        }

        public async Task<PaginatedItems<EmploymentPeriod>> GetByEmployeeIdPaginatedAsync(string employeeId, DateOnly fromDate, DateOnly toDate, PaginationDirection direction)
        {
            var uow = _uowProvider.CurrentUow;
            var employmentPeriodRepository = uow.EmploymentPeriods;
            var loadStrategy = GetLoadStrategy();
            var specification = EmploymentPeriodSpecification.ByEmployeeIds([employeeId]);

            var paginatedEmploymentPeriods = await employmentPeriodRepository.GetPaginatedAsync(
                fromDate,
                toDate,
                direction,
                specification,
                loadStrategy);

            return paginatedEmploymentPeriods;
        }


        private static EntityLoadStrategy<EmploymentPeriod> GetLoadStrategy()
        {
            return new EntityLoadStrategy<EmploymentPeriod>(e => e.Employee);
        }



        private sealed class EmploymentPeriodSpecification : Specification<EmploymentPeriod>
        {
            private EmploymentPeriodSpecification(Expression<Func<EmploymentPeriod, bool>> predicate)
                : base(predicate)
            {

            }


            public static Specification<EmploymentPeriod> ByDate(DateOnly date) =>
                new EmploymentPeriodSpecification(p => p.StartDate <= date && (p.EndDate == null || p.EndDate >= date));

            public static Specification<EmploymentPeriod> ByEmployeeIds(IReadOnlyCollection<string> employeeIds) =>
                new EmploymentPeriodSpecification(p => employeeIds.Contains(p.Employee.ExternalId));

            public static Specification<EmploymentPeriod> ForPeriod(DateOnly startDate, DateOnly endDate) =>
                new EmploymentPeriodSpecification(p => p.StartDate <= endDate && (p.EndDate == null || p.EndDate >= startDate));
        }
    }
}