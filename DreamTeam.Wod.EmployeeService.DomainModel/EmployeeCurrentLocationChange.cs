using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeCurrentLocationChange
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public int? PreviousLocationId { get; set; }

        public CurrentLocation PreviousLocation { get; set; }

        public int? NewLocationId { get; set; }

        public CurrentLocation NewLocation { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
