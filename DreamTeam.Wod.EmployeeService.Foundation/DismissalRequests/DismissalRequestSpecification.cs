using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DreamTeam.Common.Specification;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Foundation.Employees;

namespace DreamTeam.Wod.EmployeeService.Foundation.DismissalRequests
{
    public sealed class DismissalRequestSpecification : Specification<DismissalRequest>
    {
        private DismissalRequestSpecification(Expression<Func<DismissalRequest, bool>> predicate)
            : base(predicate)
        {
        }


        public static Specification<DismissalRequest> EmployeeNotInternship => FromExpression(r => EmployeeSpecification.EmployeeNotInternship.IsSatisfiedBy(r.Employee));

        public static Specification<DismissalRequest> IsLinked => FromExpression(d => d.SourceDismissalRequestId != null);

        public static Specification<DismissalRequest> Active =>
            new DismissalRequestSpecification(r => !r.CloseDate.HasValue && r.IsActive && (r.Employee.IsActive || !r.Employee.DismissalDate.HasValue));

        public static Specification<DismissalRequest> ByPeriod(DateOnly fromDate, DateOnly toDate) =>
            new DismissalRequestSpecification(r => fromDate <= r.DismissalDate && r.DismissalDate <= toDate);

        public static Specification<DismissalRequest> ByType(DismissalRequestType type) =>
            new DismissalRequestSpecification(r => r.Type == type);

        public static Specification<DismissalRequest> ByTypes(IReadOnlyCollection<DismissalRequestType> types) =>
            new DismissalRequestSpecification(r => types.Contains(r.Type));

        public static Specification<DismissalRequest> ByEmployeeId(string employeeId) =>
            new DismissalRequestSpecification(r => r.Employee.ExternalId == employeeId);

        public static Specification<DismissalRequest> ByEmployeeIds(IReadOnlyCollection<string> employeeIds) =>
            new DismissalRequestSpecification(r => employeeIds.Contains(r.Employee.ExternalId));
    }
}
