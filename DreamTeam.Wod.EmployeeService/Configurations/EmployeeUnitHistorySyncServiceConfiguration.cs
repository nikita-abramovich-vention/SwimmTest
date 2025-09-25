using System;
using DreamTeam.Common;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation.Configuration;
using Microsoft.Extensions.Options;


namespace DreamTeam.Wod.EmployeeService.Configurations;

[UsedImplicitly]
public sealed class EmployeeUnitHistorySyncServiceConfiguration : IEmployeeUnitHistorySyncServiceConfiguration
{
    private readonly EmployeeUnitHistorySyncServiceOptions _options;


    public bool Enable => _options.Enable;

    public TimeSpan SyncInterval => _options.SyncInterval;


    public EmployeeUnitHistorySyncServiceConfiguration(IOptions<EmployeeUnitHistorySyncServiceOptions> options)
    {
        _options = options.Value;
    }
}