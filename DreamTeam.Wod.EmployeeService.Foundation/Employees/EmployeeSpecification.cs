using System.Collections.Generic;
using System.Linq;
using DreamTeam.Common.Specification;
using DreamTeam.Wod.EmployeeService.DomainModel;
using static DreamTeam.Common.Specification.Specification<DreamTeam.Wod.EmployeeService.DomainModel.Employee>;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public static class EmployeeSpecification
{
    public static Specification<Employee> Active { get; } = FromExpression(e => e.IsActive);

    public static Specification<Employee> EmployeeNotInternship { get; } = FromExpression(e => e.EmploymentType != EmploymentType.Internship);

    public static Specification<Employee> ById(string id) => FromExpression(e => e.ExternalId == id);

    public static Specification<Employee> ByIds(IReadOnlyCollection<string> ids) => FromExpression(e => ids.Contains(e.ExternalId));

    public static Specification<Employee> ByIds(IReadOnlyCollection<int> ids) => FromExpression(e => ids.Contains(e.Id));

    public static Specification<Employee> ByPersonId(string id) => FromExpression(e => e.PersonId == id);

    public static Specification<Employee> ByPersonIds(IReadOnlyCollection<string> personIds) => FromExpression(e => personIds.Contains(e.PersonId));

    public static Specification<Employee> ByUnitId(string unitId) => FromExpression(e => e.UnitId == unitId);

    public static Specification<Employee> ByUnitIds(IReadOnlyCollection<string> unitIds) => FromExpression(e => unitIds.Contains(e.UnitId));

    public static Specification<Employee> ByRoleId(string roleId) => FromExpression(e => e.Roles.Any(r => r.Role.ExternalId == roleId));

    public static Specification<Employee> ByTitleRoleIds(IReadOnlyCollection<string> titleRoleIds) => FromExpression(e => titleRoleIds.Contains(e.TitleRole.ExternalId));

    public static Specification<Employee> ByTitleRoleIds(IReadOnlyCollection<int> titleRoleIds) => FromExpression(e => e.TitleRoleId != null && titleRoleIds.Contains(e.TitleRoleId.Value));
}