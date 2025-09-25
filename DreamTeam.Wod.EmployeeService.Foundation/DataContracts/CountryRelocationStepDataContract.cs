using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class CountryRelocationStepDataContract
{
    public RelocationStepId Id { get; set; }

    public int Order { get; set; }

    public int DurationInDays { get; set; }
}