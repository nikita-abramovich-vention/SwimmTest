using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel;

public sealed class WageRatePeriod
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public double Rate { get; set; }
}