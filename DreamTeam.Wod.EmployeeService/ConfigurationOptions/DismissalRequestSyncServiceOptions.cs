using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions
{
    public sealed class DismissalRequestSyncServiceOptions
    {
        public const string SectionName = "DismissalRequestSyncService";


        public bool Enable { get; set; }

        public TimeSpan SyncInterval { get; set; }

        public string EmployeeIdsToSync { get; set; }
    }
}
