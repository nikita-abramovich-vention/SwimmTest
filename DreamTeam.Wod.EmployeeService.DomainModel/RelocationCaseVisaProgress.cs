namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationCaseVisaProgress
    {
        public bool IsScheduled { get; set; }

        public bool AreDocsGathered { get; set; }

        public bool AreDocsSentToAgency { get; set; }

        public bool IsAttended { get; set; }

        public bool IsPassportCollected { get; set; }
    }
}
