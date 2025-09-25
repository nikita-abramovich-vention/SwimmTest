using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeSnapshotLog
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public bool IsSuccessful { get; set; }
    }
}