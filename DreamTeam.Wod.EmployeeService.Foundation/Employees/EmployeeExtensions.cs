using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public static class EmployeeExtensions
    {
        public static string GetTitle(this Employee employee)
        {
            var titleRoleName = employee.TitleRole?.Name;

            if (String.IsNullOrEmpty(titleRoleName))
            {
                return "None";
            }

            return employee.SeniorityId.HasValue && !employee.Seniority.IsHidden && employee.IsSeniorityRequired()
                ? $"{employee.Seniority.Name} {titleRoleName}"
                : titleRoleName;
        }

        public static bool IsSeniorityRequired(this Employee employee)
        {
            return employee.TitleRole?.HasSeniority ?? false;
        }
    }
}