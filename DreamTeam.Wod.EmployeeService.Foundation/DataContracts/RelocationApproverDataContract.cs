namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationApproverDataContract
    {
        public string EmployeeId { get; set; }

        public string CountryId { get; set; }

        public bool IsPrimary { get; set; }
    }
}
