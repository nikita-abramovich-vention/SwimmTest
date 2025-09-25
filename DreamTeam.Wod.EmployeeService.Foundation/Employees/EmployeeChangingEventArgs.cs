using System;
using DreamTeam.Wod.EmployeeService.DomainModel;
using DreamTeam.Wod.EmployeeService.Repositories;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public sealed class EmployeeChangingEventArgs : EventArgs
    {
        public Employee PreviousEmployee { get; }

        public Employee Employee { get; }

        public IEmployeeServiceUnitOfWork UnitOfWork { get; }


        public EmployeeChangingEventArgs(Employee previousEmployee, Employee employee, IEmployeeServiceUnitOfWork uow)
        {
            PreviousEmployee = previousEmployee;
            Employee = employee;
            UnitOfWork = uow;
        }

        public EmployeeChangingEventArgs(Employee employee, IEmployeeServiceUnitOfWork uow)
            : this(null, employee, uow)
        {

        }
    }
}
