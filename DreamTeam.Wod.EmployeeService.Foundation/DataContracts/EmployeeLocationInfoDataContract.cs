namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class EmployeeLocationInfoDataContract
    {
        public string EmployeeId { get; set; }

        public EmployeeCurrentLocationDataContract CurrentLocation { get; set; }

        public RelocationPlanDataContract RelocationPlan { get; set; }
    }
}