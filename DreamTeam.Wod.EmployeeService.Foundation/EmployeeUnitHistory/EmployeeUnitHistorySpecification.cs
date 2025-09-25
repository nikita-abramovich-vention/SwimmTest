using DreamTeam.Common.Specification;
using static DreamTeam.Common.Specification.Specification<DreamTeam.Wod.EmployeeService.DomainModel.EmployeeUnitHistory>;

namespace DreamTeam.Wod.EmployeeService.Foundation.EmployeeUnitHistory;

internal sealed class EmployeeUnitHistorySpecification
{
    public static Specification<DomainModel.EmployeeUnitHistory> NotLinked => FromExpression(h => h.ExternalEmployeeUnitHistory == null);

    public static Specification<DomainModel.EmployeeUnitHistory> ByEmployeeId(string employeeId) => FromExpression(h => h.Employee.ExternalId == employeeId);
}