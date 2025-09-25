namespace DreamTeam.Wod.EmployeeService.DomainModel
{
    public sealed class RelocationApproverOrder
    {
        public int Id { get; set; }

        public int Order { get; set; }

        public bool IsNext { get; set; }
    }
}
