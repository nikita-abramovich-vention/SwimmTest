using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration;

public interface IEmployeeUnitHistorySyncServiceConfiguration
{
    bool Enable { get; }

    TimeSpan SyncInterval { get; }
}