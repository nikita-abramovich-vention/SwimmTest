using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class CountryRelocationStepsDataContract
{
    public string CountryId { get; set; }

    public IReadOnlyCollection<CountryRelocationStepDataContract> Steps { get; set; }
}