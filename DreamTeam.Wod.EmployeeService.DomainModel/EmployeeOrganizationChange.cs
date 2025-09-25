using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class EmployeeOrganizationChange
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public string PreviousOrganizationId { get; set; }

        public string NewOrganizationId { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
