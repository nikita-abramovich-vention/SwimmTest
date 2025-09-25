using System;
using DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanCloseInfoDataContract
    {
        public string CloseComment { get; set; }

        public RelocationPlanCloseReason CloseReason { get; set; }

        public DateTime? CloseDate { get; set; }
    }
}
