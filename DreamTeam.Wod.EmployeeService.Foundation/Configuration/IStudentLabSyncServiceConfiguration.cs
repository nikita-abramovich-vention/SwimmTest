using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.Configuration
{
    public interface IStudentLabSyncServiceConfiguration
    {
        bool Enable { get; }

        TimeSpan SyncInterval { get; }

        Uri StdLabApiUrl { get; }

        string ApiKeyHeaderName { get; }

        string ApiKeyValue { get; }
    }
}