using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.RelocationPlans
{
    public sealed class RelocationPlanGlobalMobilityInfo
    {
        public string GmComment { get; set; }

        public Employee GmManager { get; set; }

        public bool IsInductionPassed { get; set; }
    }
}
