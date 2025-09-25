using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanChangeDataContract
    {
        public string EmployeeId { get; set; }

        public RelocationPlanChangeType Type { get; set; }

        public bool? PreviousIsInductionPassed { get; set; }

        public bool? IsInductionPassed { get; set; }

        public bool? PreviousIsConfirmed { get; set; }

        public bool? IsConfirmed { get; set; }

        public RelocationPlanStatusDataContract PreviousStatus { get; set; }

        public RelocationPlanStatusDataContract Status { get; set; }

        public CurrentLocationDataContract PreviousDestination { get; set; }

        public CurrentLocationDataContract Destination { get; set; }

        public CurrentLocationDataContract PreviousEmployeeLocation { get; set; }

        public CurrentLocationDataContract EmployeeLocation { get; set; }

        public string PreviousOrganizationId { get; set; }

        public string OrganizationId { get; set; }

        public bool? PreviousIsApproved { get; set; }

        public bool? IsApproved { get; set; }

        public bool? PreviousIsEmploymentConfirmedByEmployee { get; set; }

        public bool? IsEmploymentConfirmedByEmployee { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
