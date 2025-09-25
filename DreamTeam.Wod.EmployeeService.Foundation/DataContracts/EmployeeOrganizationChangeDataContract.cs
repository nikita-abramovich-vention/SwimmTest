using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeOrganizationChangeDataContract
    {
        public string EmployeeId { get; set; }

        public string PreviousOrganizationId { get; set; }

        public string NewOrganizationId { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
