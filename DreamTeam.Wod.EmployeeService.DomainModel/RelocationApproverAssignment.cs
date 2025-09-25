using System;

namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationApproverAssignment
    {
        public int Id { get; set; }

        public int RelocationPlanId { get; set; }

        public RelocationPlan RelocationPlan { get; set; }

        public int ApproverId { get; set; }

        public Employee Approver { get; set; }

        public DateTime Date { get; set; }
    }
}
