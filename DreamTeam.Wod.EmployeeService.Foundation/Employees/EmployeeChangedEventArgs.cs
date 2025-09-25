using System;
using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public sealed class EmployeeChangedEventArgs : EventArgs
    {
        public Employee PreviousEmployee { get; }

        public Employee Employee { get; }


        public EmployeeChangedEventArgs(Employee previousEmployee, Employee employee)
        {
            PreviousEmployee = previousEmployee;
            Employee = employee;
        }
    }
}
