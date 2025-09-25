namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public class CompensationDetailsDataContract
    {
        public CompensationDetailsItemDataContract Child { get; set; }

        public CompensationDetailsItemDataContract Spouse { get; set; }

        public CompensationDetailsItemDataContract Employee { get; set; }
    }
}
