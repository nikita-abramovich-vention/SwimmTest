using System;
using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration
{
    public interface IDismissalRequestSyncServiceConfiguration
    {
        bool Enable { get; }

        TimeSpan SyncInterval { get; }

        IReadOnlyCollection<string> EmployeeIdsToSync { get; }
    }
}