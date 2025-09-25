using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions
{
    public class StudentLabSyncServiceOptions
    {
        public const string SectionName = "StudentLabSyncService";


        public bool Enable { get; set; }

        public TimeSpan SyncInterval { get; set; }

        public Uri StdLabApiUrl { get; set; }

        public string ApiKeyHeaderName { get; set; }

        public string ApiKeyValue { get; set; }
    }
}