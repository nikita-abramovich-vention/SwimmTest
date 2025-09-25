using DreamTeam.Wod.EmployeeService.DomainModel;

namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationCaseProgressDataContract
    {
        public RelocationCaseVisaProgressDataContract VisaProgress { get; set; }

        public bool IsTransferBooked { get; set; }

        public bool IsAccommodationBooked { get; set; }

        public bool IsVisaGathered { get; set; }

        public RelocationPlanTrpState? TrpState { get; set; }
    }
}
