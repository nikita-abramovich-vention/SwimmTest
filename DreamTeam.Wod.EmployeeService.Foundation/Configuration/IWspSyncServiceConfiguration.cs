using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration
{
    public interface IWspSyncServiceConfiguration
    {
        bool Enable { get; }

        TimeSpan SyncInterval { get; }
    }
}