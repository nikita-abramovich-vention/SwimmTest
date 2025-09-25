using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationPlanStep
    {
        public int RelocationPlanId { get; set; }

        public RelocationPlan RelocationPlan { get; set; }

        public RelocationStepId StepId { get; set; }

        public int Order { get; set; }

        public DateTime? CompletedAt { get; set; }

        public bool IsCompletionDateHidden { get; set; }

        public int? DurationInDays { get; set; }

        public DateTime? ExpectedAt { get; set; }
    }
}