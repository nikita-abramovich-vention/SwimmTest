namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class CompensationDataContract
    {
        public float Total { get; set; }

        public string Currency { get; set; }

        public CompensationDetailsDataContract Details { get; set; }

        public PreviousCompensationDataContract PreviousCompensation { get; set; }

        public bool PaidInAdvance { get; set; }
    }
}
