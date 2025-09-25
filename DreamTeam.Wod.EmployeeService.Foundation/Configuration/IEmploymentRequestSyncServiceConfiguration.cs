using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration
{
    public interface IEmploymentRequestSyncServiceConfiguration
    {
        bool Enable { get; }

        TimeSpan SyncInterval { get; }
    }
}
