using System;

namespace DreamTeam.Wod.EmployeeService.ConfigurationOptions
{
    public class WspSyncServiceOptions
    {
        public const string SectionName = "WspSyncService";


        public bool Enable { get; set; }

        public TimeSpan SyncInterval { get; set; }
    }
}