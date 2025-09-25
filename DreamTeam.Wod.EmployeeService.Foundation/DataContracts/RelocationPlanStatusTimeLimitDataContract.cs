namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanStatusTimeLimitDataContract
    {
        public RelocationPlanStatusDataContract RelocationPlanStatus { get; set; }

        public int? TimeLimitDays { get; set; }
    }
}