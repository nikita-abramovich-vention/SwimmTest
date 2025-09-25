using System;
using System.Collections.Generic;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees;

public sealed class EmploymentPeriodEqualityComparer : IEqualityComparer<EmploymentPeriod>
{
    public bool Equals(EmploymentPeriod x, EmploymentPeriod y)
    {
        if (x == null || y == null)
        {
            return x == null && y == null;
        }

        return x.EmployeeId == y.EmployeeId &&
               x.StartDate == y.StartDate &&
               Nullable.Equals(x.EndDate, y.EndDate) &&
               x.OrganizationId == y.OrganizationId &&
               x.IsInternship == y.IsInternship;
    }

    public int GetHashCode(EmploymentPeriod obj)
    {
        return HashCode.Combine(obj.EmployeeId, obj.StartDate, obj.EndDate, obj.OrganizationId, obj.IsInternship);
    }
}
