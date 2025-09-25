namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class CompensationInfo
    {
        public int Id { get; set; }

        public int RelocationPlanId { get; set; }

        public RelocationPlan RelocationPlan { get; set; }

        public float Total { get; set; }

        public string Currency { get; set; }

        public CompensationInfoDetails Details { get; set; }

        public PreviousCompensationInfo PreviousCompensation { get; set; }

        public bool PaidInAdvance { get; set; }
    }
}
