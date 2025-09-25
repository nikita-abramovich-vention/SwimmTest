using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    public sealed class RelocationPlanApproverInfo
    {
        public string ApproverComment { get; set; }

        public DateOnly? ApproverDate { get; set; }

        public string Salary { get; set; }

        public string UnitId { get; set; }
    }
}
