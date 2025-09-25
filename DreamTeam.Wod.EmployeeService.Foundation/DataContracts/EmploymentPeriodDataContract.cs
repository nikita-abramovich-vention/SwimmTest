using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmploymentPeriodDataContract
    {
        public string EmployeeId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string OrganizationId { get; set; }

        public bool IsInternship { get; set; }
    }
}