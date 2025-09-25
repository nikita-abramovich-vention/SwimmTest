using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions;

public sealed class EmployeeUnitHistorySyncServiceOptions
{
    public const string SectionName = "EmployeeUnitHistorySyncService";


    public bool Enable { get; set; }

    public TimeSpan SyncInterval { get; set; }
}