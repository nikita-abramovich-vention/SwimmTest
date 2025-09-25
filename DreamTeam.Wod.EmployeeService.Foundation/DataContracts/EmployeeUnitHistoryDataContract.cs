using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts;

public sealed class EmployeeUnitHistoryDataContract
{
    public string Id { get; set; }

    public string EmployeeId { get; set; }

    public string UnitId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}