using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmploymentPeriod
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string OrganizationId { get; set; }

        public bool IsInternship { get; set; }
    }
}