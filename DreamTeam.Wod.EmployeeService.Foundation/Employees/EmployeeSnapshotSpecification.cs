using System;
using System.Collections.Generic;
using System.Linq;
using DreamTeam.Common.Specification;
using DreamTeam.Wod.EmployeeService.DomainModel;
using static DreamTeam.Common.Specification.Specification<DreamTeam.Wod.EmployeeService.DomainModel.EmployeeSnapshot>;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public static class EmployeeSnapshotSpecification
{
    public static Specification<EmployeeSnapshot> Active => FromExpression(s => s.IsActive);

    public static Specification<EmployeeSnapshot> ByFromDate(DateOnly fromDate) => FromExpression(s => fromDate <= s.ToDate);

    public static Specification<EmployeeSnapshot> ByToDate(DateOnly toDate) => FromExpression(s => toDate >= s.FromDate);

    public static Specification<EmployeeSnapshot> ByEmployeeId(string employeeId) => FromExpression(s => s.Employee.ExternalId == employeeId);

    public static Specification<EmployeeSnapshot> ByEmployeeIds(IReadOnlyCollection<string> employeeIds) => FromExpression(s => employeeIds.Contains(s.Employee.ExternalId));

    public static Specification<EmployeeSnapshot> ByUnitIds(IReadOnlyCollection<string> unitIds) => FromExpression(s => unitIds.Contains(s.UnitId));
}