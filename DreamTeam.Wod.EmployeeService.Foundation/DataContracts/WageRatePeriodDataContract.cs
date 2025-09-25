using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class WageRatePeriodDataContract
{
    public string EmployeeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public double Rate { get; set; }
}