using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions
{
    public sealed class EmploymentRequestSyncServiceOptions
    {
        public const string SectionName = "EmploymentRequestSyncService";


        public bool Enable { get; set; }

        public TimeSpan SyncInterval { get; set; }
    }
}
