using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class SyncLog
    {
        public int Id { get; set; }

        public SyncType Type { get; set; }

        public DateTime SyncStartDate { get; set; }

        public DateTime SyncCompletedDate { get; set; }

        public bool IsSuccessful { get; set; }

        public bool IsOutdated { get; set; }

        public int AffectedEntitiesCount { get; set; }
    }
}
