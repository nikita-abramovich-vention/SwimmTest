namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationCaseProgress
    {
        public RelocationCaseVisaProgress VisaProgress { get; set; }

        public bool IsTransferBooked { get; set; }

        public bool IsAccommodationBooked { get; set; }

        public bool IsVisaGathered { get; set; }

        public RelocationPlanTrpState? TrpState { get; set; }
    }
}
