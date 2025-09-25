using DreamTeam.Common.Observable;

namespace DreamTeam.Wod.EmployeeService.Foundation.Employees
{
    public interface IEmployeeObservable
    {
        event AsyncObserver<EmployeeChangedEventArgs> EmployeeMaternityLeaveStateUpdated;
    }
}
