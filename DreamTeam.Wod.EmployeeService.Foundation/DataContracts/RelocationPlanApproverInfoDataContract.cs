using System;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanApproverInfoDataContract
    {
        public string ApproverComment { get; set; }

        public DateOnly? ApproverDate { get; set; }

        public string Salary { get; set; }

        public string UnitId { get; set; }
    }
}
