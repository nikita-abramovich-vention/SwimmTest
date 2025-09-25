using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeCurrentLocation
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public int LocationId { get; set; }

        public CurrentLocation Location { get; set; }

        public string ChangedBy { get; set; }

        public DateTime ChangeDate { get; set; }

        public DateOnly SinceDate { get; set; }

        public DateOnly? UntilDate { get; set; }
    }
}