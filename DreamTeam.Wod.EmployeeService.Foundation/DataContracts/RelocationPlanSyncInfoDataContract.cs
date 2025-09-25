namespace DreamTeam.Wod.EmployeeService.Foundation.DataContracts
{
    public sealed class RelocationPlanSyncInfoDataContract
    {
        public bool IsCaseStatusHistoryRequired { get; set; }

        public string CaseStatusSourceId { get; set; }
    }
}
