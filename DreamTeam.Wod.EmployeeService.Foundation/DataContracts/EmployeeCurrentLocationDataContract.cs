using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeCurrentLocationDataContract
    {
        public string EmployeeId { get; set; }

        public string LocationId { get; set; }

        public string LocationName { get; set; }

        public CurrentLocationDataContract Location { get; set; }

        public string ChangedBy { get; set; }

        public DateTime ChangeDate { get; set; }

        public DateOnly SinceDate { get; set; }

        public DateOnly? UntilDate { get; set; }
    }
}