using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationPlanChange
    {
        public int Id { get; set; }

        public int RelocationPlanId { get; set; }

        public RelocationPlan RelocationPlan { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; }

        public RelocationPlanChangeType Type { get; set; }

        public bool? PreviousIsInductionPassed { get; set; }

        public bool? NewIsInductionPassed { get; set; }

        public bool? PreviousIsConfirmed { get; set; }

        public bool? NewIsConfirmed { get; set; }

        public int? PreviousDestinationId { get; set; }

        public CurrentLocation PreviousDestination { get; set; }

        public int? NewDestinationId { get; set; }

        public CurrentLocation NewDestination { get; set; }

        public bool? PreviousIsApproved { get; set; }

        public bool? NewIsApproved { get; set; }

        public bool? PreviousIsEmploymentConfirmedByEmployee { get; set; }

        public bool? NewIsEmploymentConfirmedByEmployee { get; set; }

        public int? PreviousStatusId { get; set; }

        public RelocationPlanStatus PreviousStatus { get; set; }

        public int? NewStatusId { get; set; }

        public RelocationPlanStatus NewStatus { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
